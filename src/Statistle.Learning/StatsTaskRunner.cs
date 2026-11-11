using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Learning.Core;
using Learning.Core.Content;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Persistence;

namespace Statistle.Learning;

/// <summary>What a task asks, with the value the analysis calculated and how close an answer must be.</summary>
public sealed record ExpectedResult(AskedResult Asked, StatistleProperty Property, double Value, string Shown, double Tolerance)
{
    /// <summary>What the learner reads beside the answer box: the prompt, or the result's label and unit.</summary>
    public string Label => Asked.Prompt ?? Property.Label;
}

/// <summary>A task set up and calculated: the analysis (for the Example and the worked solution) and the answers.</summary>
public sealed record BuiltTask(StatsTask Task, Analysis Analysis, StatistleDocument? Project, IReadOnlyList<ExpectedResult> Expected);

/// <summary>
/// Grades Statistics+ exercises for the Learning Core (plan 10.2). The task names an analysis and its data; the
/// runner builds that analysis, runs it, and compares the learner's typed results with what it calculated.
/// The learner's work is JSON: <c>{"answers": {"Mean": "4.87", "Median": "4.6"}}</c>.
/// </summary>
public sealed class StatsTaskRunner : IChallengeRunner
{
    public string Domain => "stats";

    /// <summary>Every analysis class in Statistle.Core, by class name.</summary>
    public static IReadOnlyDictionary<string, Type> Analyses { get; } = typeof(Analysis).Assembly.GetTypes()
        .Where(t => t is { IsAbstract: false, IsClass: true } && typeof(Analysis).IsAssignableFrom(t)
                    && t.GetConstructor(Type.EmptyTypes) is not null)
        .ToDictionary(t => t.Name, StringComparer.Ordinal);

    public ChallengeOutcome Run(string challengeJson, string workJson)
    {
        var hash = WorkHash.Of(workJson);
        StatsTask task;
        BuiltTask built;
        try
        {
            task = StatsTask.Parse(challengeJson);
            built = Build(task);
        }
        catch (ContentException ex)
        {
            return new ChallengeOutcome("?", false, [ex.Message], [], 0, 0, false, hash);
        }

        var answers = ReadAnswers(workJson);
        var scenarios = new List<ScenarioOutcome>();
        foreach (var expected in built.Expected)
        {
            answers.TryGetValue(expected.Asked.Result, out var typed);
            var failure = Check(expected, typed);
            scenarios.Add(new ScenarioOutcome(expected.Label, false, failure is null, failure is null ? 1 : 0, 1, failure, null));
        }

        double score = scenarios.Sum(s => s.Points), max = scenarios.Count;
        var passed = max > 0 && score / max >= task.PassThreshold - 1e-9;
        return new ChallengeOutcome(task.Id, true, [], scenarios, score, max, passed, hash);
    }

    /// <summary>The work JSON for a set of typed answers, by result name.</summary>
    public static string Work(IReadOnlyDictionary<string, string> answers) =>
        JsonSerializer.Serialize(new Dictionary<string, object> { ["answers"] = answers });

    /// <summary>
    /// Sets the task's analysis up and calculates it: its tables become a project, its settings are read as a
    /// project file reads them, and every asked result is looked up.
    /// </summary>
    /// <exception cref="ContentException">The task names something the analysis does not have.</exception>
    public static BuiltTask Build(StatsTask task)
    {
        if (!Analyses.TryGetValue(task.Analysis, out var type))
            throw new ContentException($"{task.File}: there is no analysis called \"{task.Analysis}\". Use one of: {string.Join(", ", Analyses.Keys.Order(StringComparer.Ordinal))}.");
        var analysis = (Analysis)Activator.CreateInstance(type)!;

        StatistleDocument? project = null;
        var tableIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (task.Tables.Count > 0)
        {
            project = new StatistleDocument();
            foreach (var table in task.Tables)
            {
                var added = project.AddTable(table.Name);
                foreach (var (name, values) in table.Columns) added.Columns.AddColumn(name, values);
                tableIds[table.Name] = added.Id;
            }
        }
        if (analysis is IUsesProject user) user.Project = project;

        var properties = analysis.Properties().ToDictionary(p => p.XmlName, StringComparer.Ordinal);
        var element = new XElement("analysis");
        foreach (var (name, raw) in task.Settings)
        {
            if (!properties.TryGetValue(name, out var property))
                throw new ContentException($"{task.File}: the {analysis.Title} analysis has no setting \"{name}\". Its settings are: {string.Join(", ", analysis.Properties().Where(p => !p.IsResult && !p.Hidden).Select(p => p.XmlName))}.");
            if (property.IsResult)
                throw new ContentException($"{task.File}: \"{name}\" is a result of {analysis.Title}, so a task cannot set it.");
            element.Add(new XElement(name, Resolve(raw, tableIds, task.File)));
        }
        var diagnostics = new LoadDiagnostics();
        StatistleXml.Read(analysis, element, diagnostics);
        if (diagnostics.Warnings.Count > 0)
            throw new ContentException($"{task.File}: {string.Join(" ", diagnostics.Warnings)}");
        analysis.Recalculate();

        var expected = new List<ExpectedResult>();
        foreach (var asked in task.Ask)
        {
            if (!properties.TryGetValue(asked.Result, out var property) || !property.IsResult)
                throw new ContentException($"{task.File}: \"{asked.Result}\" is not a result of {analysis.Title}. Its results are: {string.Join(", ", analysis.Properties().Where(p => p.IsResult && IsNumber(p)).Select(p => p.XmlName))}.");
            if (!IsNumber(property))
                throw new ContentException($"{task.File}: \"{asked.Result}\" is not a number, so it cannot be typed and checked. Ask it as a quiz question instead.");
            var value = Convert.ToDouble(property.Read(analysis), CultureInfo.InvariantCulture);
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ContentException($"{task.File}: {analysis.Title} could not calculate \"{asked.Result}\" from this data{(analysis is IReportsProblem { Problem: { } p } ? $" ({p})" : "")}.");
            expected.Add(new ExpectedResult(asked, property, value, analysis.Display(property), asked.Tolerance ?? DefaultTolerance(property, value)));
        }

        return new BuiltTask(task, analysis, project, expected);
    }

    /// <summary>
    /// How close a typed answer must be when the task does not say: half a unit in the second decimal or 0.5 % of
    /// the value, whichever is larger; whole numbers (counts) must be exact. A student who rounds sensibly passes.
    /// </summary>
    public static double DefaultTolerance(StatistleProperty property, double value) =>
        property.Decimals <= 0 ? 0.5 : Math.Max(0.005 * Math.Abs(value), 0.5 * Math.Pow(10, -Math.Min(property.Decimals, 2)));

    /// <summary>Null when the typed answer is right; otherwise one sentence saying what is wrong (never the answer).</summary>
    public static string? Check(ExpectedResult expected, string? typed)
    {
        if (string.IsNullOrWhiteSpace(typed)) return $"{expected.Label}: no answer yet.";
        if (!TryNumber(typed, out var number))
            return $"{expected.Label}: \"{typed.Trim()}\" is not a number. Type digits, with a point for decimals, such as 4.87.";
        if (Math.Abs(number - expected.Value) <= expected.Tolerance) return null;
        var way = number > expected.Value ? "too high" : "too low";
        return $"{expected.Label}: {typed.Trim()} is {way}. Check the formula in the Learn tab and your arithmetic.";
    }

    public static bool TryNumber(string text, out double number)
    {
        var t = text.Trim().TrimEnd('%').Trim();
        return double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out number)
               || double.TryParse(t, NumberStyles.Float, CultureInfo.CurrentCulture, out number);
    }

    private static bool IsNumber(StatistleProperty p) =>
        (Nullable.GetUnderlyingType(p.Property.PropertyType) ?? p.Property.PropertyType) is var t
        && (t == typeof(double) || t == typeof(int) || t == typeof(float) || t == typeof(long));

    private static string Resolve(string raw, Dictionary<string, int> tableIds, string file)
    {
        if (!raw.StartsWith('@')) return raw;
        var slash = raw.IndexOf('/', StringComparison.Ordinal);
        if (slash < 2) throw new ContentException($"{file}: \"{raw}\" should be written @Table/Column.");
        var table = raw[1..slash];
        if (!tableIds.TryGetValue(table, out var id)) throw new ContentException($"{file}: \"{raw}\" names the table \"{table}\", which the task does not have.");
        return Statistle.Core.Domain.DataSources.Reference(id, raw[(slash + 1)..]);
    }

    private static Dictionary<string, string> ReadAnswers(string workJson)
    {
        var answers = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(workJson)) return answers;
        try
        {
            using var doc = JsonDocument.Parse(workJson);
            if (doc.RootElement.TryGetProperty("answers", out var a) && a.ValueKind == JsonValueKind.Object)
                foreach (var p in a.EnumerateObject())
                    answers[p.Name] = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? "" : p.Value.GetRawText();
        }
        catch (JsonException) { }
        return answers;
    }
}
