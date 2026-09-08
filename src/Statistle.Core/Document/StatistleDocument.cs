using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Xml.Linq;
using Statistle.Core.Analyses;
using Statistle.Core.Domain;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Core.Document;

/// <summary>A file written by a newer Statistle than this one.</summary>
public sealed class UnsupportedSchemaVersionException(int found, int supported)
    : Exception($"This project was written by a newer version of Statistle (file schema {found}, " +
                $"this build reads up to {supported}). Update Statistle to open it.")
{
    public int FoundVersion { get; } = found;

    public int SupportedVersion { get; } = supported;
}

/// <summary>
/// One step of forward migration. Registering an implementation is all an older file needs;
/// nothing else in the read path changes.
/// </summary>
public interface IDocumentMigration
{
    /// <summary>The schema version this migration upgrades <em>from</em>.</summary>
    int FromVersion { get; }

    void Apply(XElement project, IDictionary<string, XElement> artifacts);
}

/// <summary>
/// An analysis that reads from the project it lives in — a column of one of its data tables.
/// The document sets <see cref="Project"/> when it hands the analysis out; nothing else has to
/// wire it up.
/// </summary>
public interface IUsesProject
{
    StatistleDocument? Project { get; set; }
}

/// <summary>
/// A Statistle project: a zip container holding one readable XML part per artifact.
///
/// <code>
/// myproject.statistle
///  ├─ project.xml             schema version, metadata
///  ├─ tables/1.xml            a data table
///  └─ analyses/DescriptiveStatistics.xml
/// </code>
///
/// Keeping each artifact its own part is what makes <c>Export ▸ Data table</c> a copy-out
/// rather than a second serializer.
/// </summary>
public sealed class StatistleDocument : StatistleObject
{
    public const int CurrentSchemaVersion = 1;

    private const string ProjectPart = "project.xml";
    private const string TableFolder = "tables";
    private const string AnalysisFolder = "analyses";

    private static readonly List<IDocumentMigration> Migrations = [];

    private DateTime _createdUtc = DateTime.UtcNow;
    private DateTime _modifiedUtc = DateTime.UtcNow;

    [StatistleProperty(Label = "Created")]
    public DateTime CreatedUtc
    {
        get => _createdUtc;
        set => SetProperty(ref _createdUtc, value);
    }

    [StatistleProperty(Label = "Modified")]
    public DateTime ModifiedUtc
    {
        get => _modifiedUtc;
        set => SetProperty(ref _modifiedUtc, value);
    }

    /// <summary>
    /// How numbers are shown. Statistics has no unit system to convert between, so this is a
    /// label look-up rather than a choice; it stays so the editor, the report writers and the
    /// figures have one place to ask. See <see cref="UnitSelection"/>.
    /// </summary>
    [StatistleIgnore]
    public UnitSelection Units { get; } = new();

    /// <summary>The project's data tables. Held as separate parts, not inside project.xml.</summary>
    [StatistleIgnore]
    public ObservableCollection<DataTable> Tables { get; } = [];

    /// <summary>
    /// Live analysis state, keyed by analysis type name. An analysis persists its parameters
    /// simply by existing — no registration and no save code of its own.
    /// </summary>
    [StatistleIgnore]
    public Dictionary<string, Analysis> Tools { get; } = [];

    /// <summary>Where this document was last saved or opened from.</summary>
    [StatistleIgnore]
    public string? FilePath { get; private set; }

    /// <summary>
    /// Restores the recorded path after a write that was not a Save As - an autosave or an undo
    /// snapshot. Without this, autosaving would silently repoint the project at its own recovery
    /// copy, and the next Save would write there.
    /// </summary>
    public void SetFilePath(string? path) => FilePath = path;

    /// <summary>Warnings from the most recent open.</summary>
    [StatistleIgnore]
    public LoadDiagnostics Diagnostics { get; private set; } = new();

    public static void RegisterMigration(IDocumentMigration migration)
    {
        ArgumentNullException.ThrowIfNull(migration);
        Migrations.Add(migration);
    }

    /// <summary>Adds an empty data table and returns it.</summary>
    public DataTable AddTable(string? name = null)
    {
        var table = new DataTable
        {
            Id = NextId(Tables.Select(t => t.Id)),
        };
        table.Name = name ?? $"Data table {table.Id}";
        Tables.Add(table);
        return table;
    }

    /// <summary>
    /// This project's instance of an analysis, created on first use and persisted from then on.
    /// </summary>
    public T Tool<T>()
        where T : Analysis, new()
    {
        string key = typeof(T).Name;

        if (Tools.TryGetValue(key, out Analysis? existing) && existing is T typed)
        {
            return typed;
        }

        var created = new T();
        Attach(created);
        Tools[key] = created;
        return created;
    }

    /// <summary>
    /// This project's instance of a discovered analysis. The rail uses this: it has a
    /// descriptor, not a compile-time type, and still needs no per-analysis code.
    /// </summary>
    public Analysis Tool(AnalysisDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (Tools.TryGetValue(descriptor.Key, out Analysis? existing))
        {
            return existing;
        }

        Analysis created = descriptor.Create();
        Attach(created);
        Tools[descriptor.Key] = created;
        return created;
    }

    /// <summary>
    /// The analysis that belongs to <paramref name="target"/>: opened from that object's
    /// Calculate link, it keeps the numbers last used for that object, apart from the rail's
    /// own instance and from every other object's. Created blank on first use
    /// (<see cref="Analysis.StartBlank"/>); the hand-off seeds then fill what the object
    /// already knows. Saved under <c>analyses/{analysis}@{target}.xml</c>.
    /// </summary>
    public Analysis Tool(AnalysisDescriptor descriptor, StatistleObject target)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(target);

        string key = $"{descriptor.Key}@{TargetKey(target)}";

        if (Tools.TryGetValue(key, out Analysis? existing))
        {
            return existing;
        }

        Analysis created = descriptor.Create();
        Attach(created);
        created.StartBlank();
        Tools[key] = created;
        return created;
    }

    /// <summary>How an object is named in a per-object key: its kind and id, or an analysis's own key.</summary>
    public string TargetKey(StatistleObject target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target is Analysis analysis)
        {
            string? stored = Tools.FirstOrDefault(t => ReferenceEquals(t.Value, analysis)).Key;
            return (stored ?? analysis.GetType().Name).Replace('@', '_');
        }

        return $"{target.GetType().Name}-{target.Id}";
    }

    /// <summary>True when the object a per-object key names is still in the project.</summary>
    private bool TargetExists(string targetKey)
    {
        if (Tools.ContainsKey(targetKey))
        {
            return true;
        }

        int dash = targetKey.LastIndexOf('-');
        if (dash <= 0 || !int.TryParse(targetKey[(dash + 1)..], out int id))
        {
            return false;
        }

        return targetKey[..dash] switch
        {
            nameof(DataTable) => Tables.Any(t => t.Id == id),
            _ => false,
        };
    }

    /// <summary>Gives a project-bound analysis its project. Nothing else has to wire it up.</summary>
    private void Attach(Analysis analysis)
    {
        if (analysis is IUsesProject usesProject)
        {
            usesProject.Project = this;
        }
    }

    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ModifiedUtc = DateTime.UtcNow;

        string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Write to a temporary file first, so a failure part-way through does not destroy the
        // project that was already there.
        string temporary = path + ".tmp";

        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            XElement project = StatistleXml.Write(this, "project");
            project.SetAttributeValue("schemaVersion", CurrentSchemaVersion);
            WriteEntry(archive, ProjectPart, project);

            foreach (DataTable table in Tables)
            {
                WriteEntry(archive, $"{TableFolder}/{table.Id}.xml", StatistleXml.Write(table, "table"));
            }

            foreach ((string key, Analysis analysis) in Tools)
            {
                WriteEntry(archive, $"{AnalysisFolder}/{key}.xml", StatistleXml.Write(analysis, "analysis"));
            }
        }

        File.Move(temporary, path, overwrite: true);
        FilePath = path;
    }

    public static StatistleDocument Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var document = new StatistleDocument();
        var diagnostics = new LoadDiagnostics();

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry projectEntry = archive.GetEntry(ProjectPart)
            ?? throw new InvalidDataException(
                $"'{Path.GetFileName(path)}' is not a Statistle project: {ProjectPart} is missing.");

        XElement project = ReadEntry(projectEntry);

        int version = (int?)project.Attribute("schemaVersion") ?? 1;
        if (version > CurrentSchemaVersion)
        {
            throw new UnsupportedSchemaVersionException(version, CurrentSchemaVersion);
        }

        Dictionary<string, XElement> artifacts = [];
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName != ProjectPart && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                artifacts[entry.FullName] = ReadEntry(entry);
            }
        }

        // Bring an older file forward before anything reads it.
        foreach (IDocumentMigration migration in Migrations
                     .Where(m => m.FromVersion >= version && m.FromVersion < CurrentSchemaVersion)
                     .OrderBy(m => m.FromVersion))
        {
            migration.Apply(project, artifacts);
        }

        StatistleXml.Read(document, project, diagnostics);

        foreach ((string name, XElement element) in artifacts.OrderBy(a => a.Key, StringComparer.Ordinal))
        {
            if (name.StartsWith(TableFolder + "/", StringComparison.OrdinalIgnoreCase))
            {
                var table = new DataTable();
                StatistleXml.Read(table, element, diagnostics);
                document.Tables.Add(table);
            }
            else if (name.StartsWith(AnalysisFolder + "/", StringComparison.OrdinalIgnoreCase))
            {
                document.ReadTool(name, element, diagnostics);
            }
            else
            {
                diagnostics.Add(name, string.Empty, "unrecognised part; ignored");
            }
        }

        // An analysis kept for an object that is no longer in the project is dropped.
        foreach (string key in document.Tools.Keys.Where(k => k.Contains('@')).ToList())
        {
            if (!document.TargetExists(key[(key.IndexOf('@') + 1)..]))
            {
                document.Tools.Remove(key);
                diagnostics.Add(
                    $"{AnalysisFolder}/{key}.xml", string.Empty,
                    "kept for an object no longer in the project; dropped");
            }
        }

        // Everything is in place now, so project-bound analyses can compute.
        foreach (Analysis analysis in document.Tools.Values)
        {
            analysis.Recalculate();
        }

        document.FilePath = path;
        document.Diagnostics = diagnostics;
        return document;
    }

    /// <summary>
    /// Writes one artifact to a standalone XML file — the same bytes the document holds, so
    /// export needs no serializer of its own.
    /// </summary>
    public static void ExportArtifact(StatistleObject artifact, string path)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string elementName = artifact.GetType().Name.ToLowerInvariant();
        new XDocument(StatistleXml.Write(artifact, elementName)).Save(path);
    }

    /// <summary>Reads an artifact exported by <see cref="ExportArtifact"/> back in.</summary>
    public static T ImportArtifact<T>(string path, LoadDiagnostics? diagnostics = null)
        where T : StatistleObject, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        XDocument loaded = XDocument.Load(path);
        var artifact = new T();
        StatistleXml.Read(artifact, loaded.Root!, diagnostics);
        return artifact;
    }

    /// <summary>
    /// Restores one analysis's saved parameters. A part whose analysis no longer exists in this
    /// build is reported and skipped rather than failing the open — a project must survive an
    /// analysis being renamed or withdrawn.
    /// </summary>
    private void ReadTool(string partName, XElement element, LoadDiagnostics diagnostics)
    {
        string key = Path.GetFileNameWithoutExtension(partName);
        string analysisKey = key.Contains('@') ? key[..key.IndexOf('@')] : key;

        AnalysisDescriptor? descriptor = AnalysisCatalog
            .Discover(typeof(Analysis).Assembly)
            .FirstOrDefault(d => d.Key == analysisKey);

        if (descriptor is null)
        {
            diagnostics.Add(partName, key, "no analysis of this name in this build; skipped");
            return;
        }

        Analysis analysis = descriptor.Create();
        Attach(analysis);
        StatistleXml.Read(analysis, element, diagnostics);

        // Not recalculated here: parts load in name order, so analyses/ is read before
        // tables/, and a project-bound analysis would compute against an empty project.
        Tools[key] = analysis;
    }

    private static int NextId(IEnumerable<int> existing)
    {
        int[] used = [.. existing];
        return used.Length == 0 ? 1 : used.Max() + 1;
    }

    private static void WriteEntry(ZipArchive archive, string name, XElement content)
    {
        ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open());
        new XDocument(content).Save(writer);
    }

    private static XElement ReadEntry(ZipArchiveEntry entry)
    {
        using var reader = new StreamReader(entry.Open());
        return XDocument.Load(reader).Root!;
    }
}
