using System.Globalization;
using System.Text.Json;
using Learning.Core.Content;

namespace Statistle.Learning;

/// <summary>A data table a task brings with it: named columns of numbers.</summary>
public sealed record TaskTable(string Name, IReadOnlyList<(string Name, double[] Values)> Columns);

/// <summary>One result the learner is asked for: an output property of the analysis, by its property name.</summary>
public sealed record AskedResult(string Result, string? Prompt, double? Tolerance);

/// <summary>
/// An Example or an Exercise (plan 10.1): one analysis, set up from data, with the results an exercise asks for.
/// Written as JSON in a content package's <c>tasks/</c> folder. The answers are never written in the file: the
/// runner calculates them with the analysis (plan D5).
/// </summary>
public sealed class StatsTask
{
    public required string Id { get; init; }
    public required string Title { get; init; }

    /// <summary>The analysis class's name, such as <c>DescriptiveStatistics</c>.</summary>
    public required string Analysis { get; init; }

    /// <summary>Markdown shown above the data: the situation and what to do.</summary>
    public string Intro { get; init; } = "";

    /// <summary>Property name → value as the project file writes it. A number list becomes one value per line;
    /// <c>@Table/Column</c> becomes a reference to a column of one of <see cref="Tables"/>.</summary>
    public IReadOnlyDictionary<string, string> Settings { get; init; } = new Dictionary<string, string>();

    public IReadOnlyList<TaskTable> Tables { get; init; } = [];
    public IReadOnlyList<AskedResult> Ask { get; init; } = [];
    public IReadOnlyList<string> Hints { get; init; } = [];

    /// <summary>The share of the asked results that must be right, 0 to 1 (default: all of them).</summary>
    public double PassThreshold { get; init; } = 1.0;

    public string File { get; init; } = "";
    public string Json { get; init; } = "";

    public bool IsExercise => Ask.Count > 0;

    /// <summary>Reads a task file.</summary>
    /// <exception cref="ContentException">The file is not a task; the message says what to change.</exception>
    public static StatsTask Load(string path, string fileForMessages) =>
        Parse(System.IO.File.ReadAllText(path), fileForMessages);

    /// <exception cref="ContentException">The text is not a task; the message says what to change.</exception>
    public static StatsTask Parse(string json, string fileForMessages = "task")
    {
        try { return ParseJson(json, fileForMessages); }
        catch (JsonException ex) { throw new ContentException($"{fileForMessages} is not a task that can be read: {ex.Message}"); }
    }

    private static StatsTask ParseJson(string json, string file)
    {
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var root = doc.RootElement;
        string? Text(string name) => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
        string Need(string name) => Text(name) is { Length: > 0 } v ? v : throw new ContentException($"{file}: \"{name}\" is missing.");

        var settings = new Dictionary<string, string>(StringComparer.Ordinal);
        if (root.TryGetProperty("settings", out var s) && s.ValueKind == JsonValueKind.Object)
            foreach (var p in s.EnumerateObject())
                settings[p.Name] = SettingText(p.Value, $"{file}: setting \"{p.Name}\"");

        var tables = new List<TaskTable>();
        if (root.TryGetProperty("tables", out var t) && t.ValueKind == JsonValueKind.Array)
            foreach (var table in t.EnumerateArray())
            {
                var name = table.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                if (name.Length == 0) throw new ContentException($"{file}: a table has no \"name\".");
                var columns = new List<(string, double[])>();
                if (table.TryGetProperty("columns", out var cols) && cols.ValueKind == JsonValueKind.Object)
                    foreach (var c in cols.EnumerateObject())
                        columns.Add((c.Name, Numbers(c.Value, $"{file}: table \"{name}\", column \"{c.Name}\"")));
                if (columns.Count == 0) throw new ContentException($"{file}: the table \"{name}\" has no columns.");
                tables.Add(new TaskTable(name, columns));
            }

        var ask = new List<AskedResult>();
        if (root.TryGetProperty("ask", out var a) && a.ValueKind == JsonValueKind.Array)
            foreach (var entry in a.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.String) { ask.Add(new AskedResult(entry.GetString()!, null, null)); continue; }
                var result = entry.TryGetProperty("result", out var r) ? r.GetString() ?? "" : "";
                if (result.Length == 0) throw new ContentException($"{file}: an entry of \"ask\" has no \"result\".");
                ask.Add(new AskedResult(result,
                    entry.TryGetProperty("prompt", out var pr) ? pr.GetString() : null,
                    entry.TryGetProperty("tolerance", out var tol) && tol.ValueKind == JsonValueKind.Number ? tol.GetDouble() : null));
            }

        var hints = new List<string>();
        if (root.TryGetProperty("hints", out var h) && h.ValueKind == JsonValueKind.Array)
            hints.AddRange(h.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0));

        return new StatsTask
        {
            Id = Need("id"),
            Title = Need("title"),
            Analysis = Need("analysis"),
            Intro = Text("intro") ?? "",
            Settings = settings,
            Tables = tables,
            Ask = ask,
            Hints = hints,
            PassThreshold = root.TryGetProperty("passThreshold", out var pt) && pt.ValueKind == JsonValueKind.Number ? pt.GetDouble() : 1.0,
            File = file,
            Json = json,
        };
    }

    private static string SettingText(JsonElement value, string where) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Array => string.Join('\n', Numbers(value, where).Select(v => v.ToString("R", CultureInfo.InvariantCulture))),
        _ => throw new ContentException($"{where} must be text, a number, true/false or a list of numbers."),
    };

    private static double[] Numbers(JsonElement value, string where)
    {
        if (value.ValueKind != JsonValueKind.Array) throw new ContentException($"{where} must be a list of numbers.");
        return [.. value.EnumerateArray().Select(v => v.ValueKind == JsonValueKind.Number
            ? v.GetDouble()
            : throw new ContentException($"{where}: \"{v.GetRawText()}\" is not a number."))];
    }
}
