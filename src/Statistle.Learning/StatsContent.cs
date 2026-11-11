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
            try { StatsTaskRunner.Build(task); }
            catch (ContentException ex) { problems.Add(ex.Message); }
        }

        foreach (var item in Package.AllItems)
        {
            if (item.Exercise is { } ex && Find(ex) is { IsExercise: false })
                problems.Add($"{item.File}: the exercise {ex} asks for no results. Give it an \"ask\" list, or use it as the example.");
        }
        return problems;
    }

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
