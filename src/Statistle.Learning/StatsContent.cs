using Learning.Core.Content;
using Statistle.Core.Code;
using Statistle.Core.Plotting;

namespace Statistle.Learning;

/// <summary>
/// A Statistics+ content package (plan 10.1): the Learning Core's modules, items, quizzes and reference pages,
/// plus the statistics half — the example and exercise tasks in <c>tasks/*.json</c>.
/// </summary>
public sealed class StatsContent
{
    /// <summary>Widgets a Learn page may use. Each takes a task id; <c>figure</c> also takes the figure's number (1 = first).</summary>
    public static readonly IReadOnlyList<string> WidgetNames = ["figure", "data", "code", "results"];

    public required ContentPackage Package { get; init; }
    public required IReadOnlyDictionary<string, StatsTask> Tasks { get; init; }

    /// <summary>Tasks that could not be read, one sentence each (the validator reports them; the program skips them).</summary>
    public IReadOnlyList<string> TaskProblems { get; init; } = [];

    public StatsTask? Find(string? id) => id is not null && Tasks.TryGetValue(id, out var t) ? t : null;

    /// <exception cref="ContentException">The package itself cannot be read.</exception>
    public static StatsContent Load(string folder)
    {
        var package = ContentPackage.Load(folder);
        var tasks = new Dictionary<string, StatsTask>(StringComparer.OrdinalIgnoreCase);
        var problems = new List<string>();
        var dir = Path.Combine(folder, "tasks");
        if (Directory.Exists(dir))
            foreach (var file in Directory.GetFiles(dir, "*.json").Order(StringComparer.Ordinal))
            {
                var name = "tasks/" + Path.GetFileName(file);
                try
                {
                    var task = StatsTask.Load(file, name);
                    if (!tasks.TryAdd(task.Id, task)) problems.Add($"{name}: the id {task.Id} is used by another task too.");
                }
                catch (ContentException ex) { problems.Add(ex.Message); }
            }
        return new StatsContent { Package = package, Tasks = tasks, TaskProblems = problems };
    }

    /// <summary>
    /// Every problem in the package, one sentence each: the Learning Core's checks plus the statistics ones —
    /// every task builds and calculates, every item's Example is a task and its Exercise asks for results, and
    /// every widget names a task (and a figure it has).
    /// </summary>
    public IReadOnlyList<string> Validate()
    {
        var problems = new List<string>(TaskProblems);
        var validator = new ContentValidator
        {
            ChallengeExists = id => Tasks.ContainsKey(id),
            CheckWidget = CheckWidget,
        };
        problems.AddRange(validator.Validate(Package));

        foreach (var task in Tasks.Values)
        {
            try { problems.AddRange(MistakeProblems(StatsTaskRunner.Build(task))); }
            catch (ContentException ex) { problems.Add(ex.Message); }
        }

        foreach (var item in Package.AllItems)
        {
            if (item.Exercise is { } ex && Find(ex) is { IsExercise: false })
                problems.Add($"{item.File}: the exercise {ex} asks for no results. Give it an \"ask\" list, or use it as the example.");

            // Widgets in questions (plan 10.15): the Learning Core checks them in Learn and Reference pages only.
            if (item.Quiz is { } quiz)
                foreach (var q in quiz.Questions)
                    foreach (var (part, text) in new[] { ("prompt", q.Prompt), ("explanation", q.Explanation ?? "") })
                        foreach (var w in Widgets.In(text))
                            if (CheckWidget(w) is { } problem)
                                problems.Add($"{item.QuizFile ?? item.File}, question {q.Id} ({part}): {problem}");
        }
        return problems;
    }

    /// <summary>
    /// Things an author should look at that do not stop the package (plan 10.14), one sentence each. The Learning Core's
    /// validator has only problems, so these are kept apart: the content test prints them and still passes.
    /// <list type="bullet">
    /// <item>Two results one exercise asks for are within each other's tolerance, so typing one for the other passes.</item>
    /// </list>
    /// </summary>
    public IReadOnlyList<string> Warnings()
    {
        var warnings = new List<string>();
        foreach (var task in Tasks.Values.OrderBy(t => t.File, StringComparer.Ordinal))
        {
            BuiltTask built;
            try { built = StatsTaskRunner.Build(task); }
            catch (ContentException) { continue; }   // a problem, reported by Validate
            warnings.AddRange(OverlapWarnings(built));
        }
        return warnings;
    }

    /// <summary>Two asked results so close that an answer to one is accepted for the other (plan 10.14).</summary>
    public static IEnumerable<string> OverlapWarnings(BuiltTask built)
    {
        var e = built.Expected;
        for (var i = 0; i < e.Count; i++)
            for (var j = i + 1; j < e.Count; j++)
            {
                var gap = Math.Abs(e[i].Value - e[j].Value);
                if (gap <= e[i].Tolerance || gap <= e[j].Tolerance)
                    yield return $"{built.Task.File}: {e[i].Asked.Result} ({e[i].Shown}) and {e[j].Asked.Result} ({e[j].Shown}) are only {gap:G3} apart, " +
                                 $"within the tolerance ({Math.Max(e[i].Tolerance, e[j].Tolerance):G3}), so a learner who types one for the other passes. " +
                                 "Change the data, or give a smaller \"tolerance\".";
            }
    }

    /// <summary>A mistake whose number is accepted as the right answer would never be shown (plan 10.13).</summary>
    public static IEnumerable<string> MistakeProblems(BuiltTask built)
    {
        foreach (var e in built.Expected)
            foreach (var m in e.Mistakes)
                if (Math.Abs(m.Value - e.Value) <= e.Tolerance)
                    yield return $"{built.Task.File}: the mistake for {e.Asked.Result} (\"{Short(m.Says)}\") gives {m.Value:G4}, which is accepted as the right answer ({e.Shown}), " +
                                 "so the learner would never read it. Remove it, or change the data so the two differ.";
    }

    private static string Short(string text) => text.Length <= 40 ? text : text[..37] + "…";

    private string? CheckWidget(WidgetTag tag)
    {
        if (!WidgetNames.Contains(tag.Name, StringComparer.OrdinalIgnoreCase))
            return $"[[{tag.Name}: …]] is not a widget. Use one of: {string.Join(", ", WidgetNames)}.";
        var (id, number) = WidgetArgument(tag.Argument);
        if (Find(id) is not { } task) return $"[[{tag.Name}: {tag.Argument}]] names the task {id}, which is not in tasks/.";
        if (!tag.Name.Equals("figure", StringComparison.OrdinalIgnoreCase)) return null;
        try
        {
            var built = StatsTaskRunner.Build(task);
            var count = built.Analysis is IHasFigures f ? f.Figures(null).Count : 0;
            if (number < 1 || number > count) return $"[[figure: {tag.Argument}]]: {task.Analysis} draws {count} figure(s), so the number must be 1 to {count}.";
        }
        catch (ContentException) { /* reported with the task */ }
        return null;
    }

    /// <summary>Splits a widget's argument into the task id and an optional number (default 1).</summary>
    public static (string Id, int Number) WidgetArgument(string argument)
    {
        var parts = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var id = parts.Length > 0 ? parts[0] : "";
        var number = parts.Length > 1 && int.TryParse(parts[1], out var n) ? n : 1;
        return (id, number);
    }

    /// <summary>The Python or R that reproduces a task's analysis, when it writes code.</summary>
    public static string? Code(BuiltTask built, CodeLanguage language) =>
        built.Analysis is ICodeSource source ? source.Code(language) : null;
}
