using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Tests;

/// <summary>
/// Save/reload round-trips every property, and a property added after a file was written loads
/// at its default.
/// </summary>
public class PersistenceTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "statistle-tests", Guid.NewGuid().ToString("N"));

    public PersistenceTests() => Directory.CreateDirectory(_folder);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // A locked temp file must not fail the run.
        }

        GC.SuppressFinalize(this);
    }

    private string Path_(string name) => Path.Combine(_folder, name);

    /// <summary>
    /// The guarantee that makes the reflection-driven design safe: nobody has to remember to
    /// add a new property to a serializer, so this test asserts it for whatever exists today.
    /// Every persisted property of every domain type is given a distinctive value, saved and
    /// read back. A property that cannot round-trip fails here the day it is added.
    /// </summary>
    [Theory]
    [InlineData(typeof(DataTable))]
    [InlineData(typeof(DescriptiveStatistics))]
    [InlineData(typeof(SimpleRegression))]
    public void Every_property_of_every_domain_type_round_trips(Type type)
    {
        var original = (StatistleObject)Activator.CreateInstance(type)!;
        Populate(original);

        XElement written = StatistleXml.Write(original, "artifact");

        var reloaded = (StatistleObject)Activator.CreateInstance(type)!;
        var diagnostics = new LoadDiagnostics();
        StatistleXml.Read(reloaded, written, diagnostics);

        Assert.False(diagnostics.HasWarnings, diagnostics.ToString());

        foreach (StatistleProperty property in original.Properties())
        {
            object? before = property.Read(original);
            object? after = property.Read(reloaded);

            if (before is DataSeries series)
            {
                Assert.Equal(series.Points, ((DataSeries)after!).Points);
                continue;
            }

            if (before is ColumnTable table)
            {
                Assert.Equal(table.Save(), ((ColumnTable)after!).Save());
                continue;
            }

            Assert.Equal(before, after);
        }
    }

    [Fact]
    public void Project_round_trips_through_a_statistle_file()
    {
        var document = new StatistleDocument { Name = "Tensile study" };

        DataTable strength = document.AddTable("Tensile strength");
        strength.Columns.AddColumn("AA2024", [470, 485, 478]);
        strength.Columns.AddColumn("AA7075", [505, 518, 523, 508]);

        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();
        analysis.Column = DataSources.Reference(strength.Id, "AA7075");
        analysis.Recalculate();

        string file = Path_("project.statistle");
        document.Save(file);

        StatistleDocument reopened = StatistleDocument.Open(file);

        Assert.Equal("Tensile study", reopened.Name);

        DataTable reloaded = Assert.Single(reopened.Tables);
        Assert.Equal("Tensile strength", reloaded.Name);
        Assert.Equal(2, reloaded.Columns.ColumnCount);
        Assert.Equal([470, 485, 478], reloaded.Columns.Column("AA2024")!.ToArray());
        Assert.Equal(4, reloaded.Columns.Column("AA7075")!.Count);

        // The analysis is restored and recomputed against the reloaded table.
        DescriptiveStatistics restored = reopened.Tool<DescriptiveStatistics>();
        Assert.Equal(4, restored.Count);
        Assert.Equal(analysis.Mean, restored.Mean, 10);
        Assert.False(reopened.Diagnostics.HasWarnings, reopened.Diagnostics.ToString());
    }

    [Fact]
    public void Document_layout_is_one_readable_part_per_artifact()
    {
        var document = new StatistleDocument();
        document.AddTable();
        document.AddTable();
        document.Tool<DescriptiveStatistics>();

        string file = Path_("layout.statistle");
        document.Save(file);

        using var archive = ZipFile.OpenRead(file);
        string[] parts = [.. archive.Entries.Select(e => e.FullName).Order(StringComparer.Ordinal)];

        Assert.Equal(
            ["analyses/DescriptiveStatistics.xml", "project.xml", "tables/1.xml", "tables/2.xml"],
            parts);
    }

    [Fact]
    public void A_property_added_after_the_file_was_written_loads_at_its_default()
    {
        // Simulates an older file: everything but the name is absent.
        var older = new XElement("analysis", new XElement("Name", "Legacy"));

        var analysis = new DescriptiveStatistics();
        var diagnostics = new LoadDiagnostics();

        StatistleXml.Read(analysis, older, diagnostics);

        Assert.Equal("Legacy", analysis.Name);
        Assert.Equal(string.Empty, analysis.Column);
        Assert.False(diagnostics.HasWarnings);
    }

    [Fact]
    public void An_element_this_build_does_not_know_is_ignored()
    {
        var fromTheFuture = new XElement(
            "table",
            new XElement("Name", "Strengths"),
            new XElement("SomethingAddedLater", "42"));

        var table = new DataTable();
        var diagnostics = new LoadDiagnostics();

        StatistleXml.Read(table, fromTheFuture, diagnostics);

        Assert.Equal("Strengths", table.Name);
        Assert.False(diagnostics.HasWarnings);
    }

    [Fact]
    public void Unparseable_data_keeps_the_default_and_is_reported()
    {
        // A corrupt file must not load as quietly wrong numbers.
        var corrupt = new XElement("analysis", new XElement("ReturnCount", "not a number"));

        var probe = new PopulationProbe();
        var diagnostics = new LoadDiagnostics();

        StatistleXml.Read(probe, corrupt, diagnostics);

        Assert.Equal(0, probe.ReturnCount);
        Assert.True(diagnostics.HasWarnings);
        Assert.Contains("ReturnCount", diagnostics.Warnings[0].Location, StringComparison.Ordinal);
    }

    [Fact]
    public void Full_precision_survives_a_save()
    {
        // Decimals are a display concern; storage is always full precision.
        var probe = new PopulationProbe { Measured = 41.123456789012345 };

        var reloaded = new PopulationProbe();
        StatistleXml.Read(reloaded, StatistleXml.Write(probe, "analysis"), null);

        Assert.Equal(41.123456789012345, reloaded.Measured);
        Assert.Equal("41.12", reloaded.Display(
            reloaded.Properties().Single(p => p.XmlName == nameof(PopulationProbe.Measured))));
    }

    [Fact]
    public void A_file_from_a_newer_schema_fails_with_a_clear_message()
    {
        string file = Path_("future.statistle");
        new StatistleDocument().Save(file);

        // Re-stamp project.xml with a schema version this build cannot know.
        using (var archive = ZipFile.Open(file, ZipArchiveMode.Update))
        {
            ZipArchiveEntry entry = archive.GetEntry("project.xml")!;
            XElement project;
            using (var reader = new StreamReader(entry.Open()))
            {
                project = XDocument.Load(reader).Root!;
            }

            project.SetAttributeValue("schemaVersion", StatistleDocument.CurrentSchemaVersion + 5);
            entry.Delete();

            ZipArchiveEntry replacement = archive.CreateEntry("project.xml");
            using var writer = new StreamWriter(replacement.Open());
            new XDocument(project).Save(writer);
        }

        var thrown = Assert.Throws<UnsupportedSchemaVersionException>(() => StatistleDocument.Open(file));

        Assert.Equal(StatistleDocument.CurrentSchemaVersion + 5, thrown.FoundVersion);
        Assert.Contains("newer version of Statistle", thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Opening_something_that_is_not_a_project_says_so()
    {
        string file = Path_("notaproject.statistle");
        using (var stream = new FileStream(file, FileMode.Create))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            archive.CreateEntry("readme.txt");
        }

        var thrown = Assert.Throws<InvalidDataException>(() => StatistleDocument.Open(file));
        Assert.Contains("not a Statistle project", thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_single_artifact_exports_and_imports()
    {
        var table = new DataTable { Name = "Exported" };
        table.Columns.AddColumn("x", [1, 2, 3]);
        table.Columns.AddColumn("y", [4.5, 5.5]);

        string file = Path_("table.xml");
        StatistleDocument.ExportArtifact(table, file);

        DataTable imported = StatistleDocument.ImportArtifact<DataTable>(file);

        Assert.Equal("Exported", imported.Name);
        Assert.Equal(["x", "y"], imported.Columns.Names);
        Assert.Equal([4.5, 5.5], imported.Columns.Column("y")!.ToArray());
    }

    [Fact]
    public void A_failed_save_does_not_destroy_the_existing_project()
    {
        string file = Path_("keepme.statistle");
        var document = new StatistleDocument { Name = "Original" };
        document.AddTable("Keep me");
        document.Save(file);

        long sizeBefore = new FileInfo(file).Length;

        // Hold the target open for writing; the save writes a temp file and only then moves.
        using (var _ = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var second = new StatistleDocument { Name = "Replacement" };

            // Windows reports the blocked move as UnauthorizedAccessException, not IOException.
            Assert.ThrowsAny<SystemException>(() => second.Save(file));
        }

        Assert.Equal(sizeBefore, new FileInfo(file).Length);
        Assert.Equal("Original", StatistleDocument.Open(file).Name);
    }

    [Fact]
    public void Changing_a_property_raises_change_notification()
    {
        // Autosave, undo and the live recalculation all fall out of this.
        var probe = new PopulationProbe();
        List<string?> changed = [];
        probe.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        probe.Measured = 10;
        probe.Measured = 10; // no change, no event

        Assert.Equal([nameof(PopulationProbe.Measured)], changed);
    }

    /// <summary>
    /// A minimal analysis whose properties are only what these tests need to populate.
    /// </summary>
    public sealed class PopulationProbe : Analysis
    {
        private double _measured;
        private int _returnCount;

        public override string Title => "Population probe";

        public override string Summary => "Exists only for the persistence tests";

        public override ToolKind Kind => ToolKind.FreeStanding;

        [StatistleProperty(Label = "Measured", Decimals = 2)]
        public double Measured
        {
            get => _measured;
            set => SetProperty(ref _measured, value);
        }

        [StatistleProperty(Label = "Return count", Decimals = 0)]
        public int ReturnCount
        {
            get => _returnCount;
            set => SetProperty(ref _returnCount, value);
        }
    }

    /// <summary>Gives every writable property a value distinct from its default.</summary>
    private static void Populate(StatistleObject target)
    {
        int seed = 1;

        foreach (StatistleProperty property in target.Properties())
        {
            PropertyInfo info = property.Property;
            Type type = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;

            if (type == typeof(DataSeries))
            {
                var series = (DataSeries)property.Read(target)!;
                series.Add(0.25, 1.5);
                series.Add(0.50, 2.25);
                continue;
            }

            if (property.Read(target) is ColumnTable table)
            {
                table.Clear();
                table.AddColumn("First", [1.5, 2.5, 3.5]);
                table.AddColumn("Second", [10.25, 20.5]);
                continue;
            }

            if (!info.CanWrite)
            {
                continue;
            }

            object value = type switch
            {
                _ when type == typeof(string) => $"value {seed}",
                _ when type == typeof(double) => seed + 0.125,
                _ when type == typeof(float) => seed + 0.5f,
                _ when type == typeof(int) => seed,
                _ when type == typeof(long) => (long)seed,
                _ when type == typeof(bool) => true,
                _ when type == typeof(DateTime) => new DateTime(2026, 9, 2, 10, 30, 0, DateTimeKind.Utc),
                _ when type.IsEnum => Enum.GetValues(type).GetValue(0)!,
                _ => throw new NotSupportedException(
                    $"{target.GetType().Name}.{info.Name} is a {type.Name}, which the round-trip " +
                    "test does not know how to populate. Teach it, or the property is untested."),
            };

            info.SetValue(target, value);
            seed++;
        }
    }
}
