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
    /// <summary>
    /// Widgets a page may use. Each takes a task id; <c>figure</c> also takes the figure's number (1 = first); <c>try</c> is an
    /// example to change (<see cref="TrySpec"/>). <c>example</c> starts an example on the Example tab and <c>see-example</c> is the
    /// Learn page's button to one (<see cref="ExamplePages"/>; Learning Core plan 3.22).
    /// </summary>
    public static readonly IReadOnlyList<string> WidgetNames = ["figure", "data", "code", "results", "try", "example", "see-example"];

    /// <summary>The widgets that count as an example's figure: something to look at or to move (not the code).</summary>
    public static readonly IReadOnlyList<string> FigureWidgets = ["try", "figure", "data", "results"];

    public required ContentPackage Package { get; init; }
    public required IReadOnlyDictionary<string, StatsTask> Tasks { get; init; }

    /// <summary>Tasks that could not be read, one sentence each (the validator reports them; the program skips them).</summary>
    public IReadOnlyList<string> TaskProblems { get; init; } = [];

    /// <summary>
    /// Each part's own page (plan W.1, P.1): <c>parts/*.md</c>, front matter <c>part:</c> naming the part as its modules do, then
    /// the Markdown shown when the part's row is chosen in the path.
    /// </summary>
    public IReadOnlyDictionary<string, string> PartPages { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public StatsTask? Find(string? id) => id is not null && Tasks.TryGetValue(id, out var t) ? t : null;

    /// <summary>
    /// A question's "lookAgain" item (quiz file, question id → item id): the item a learner who got it wrong should go
    /// back to. The Learning Core ignores the field; without it the Course points to the first item teaching the outcome.
    /// </summary>
    public IReadOnlyDictionary<(string QuizFile, string QuestionId), string> LookAgain { get; init; } =
        new Dictionary<(string, string), string>();

    /// <summary>The item to look at again after a wrong answer to this question, when its quiz names one.</summary>
    public LearningItem? LookAgainAt(LearningItem quizItem, Question question) =>
        quizItem.QuizFile is { } file && LookAgain.TryGetValue((file, question.Id), out var id) ? Package.FindItem(id) : null;

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
        var lookAgain = new Dictionary<(string, string), string>();
        foreach (var file in package.AllItems.Select(i => i.QuizFile).OfType<string>().Distinct(StringComparer.Ordinal))
        {
            try
            {
                using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, file)));
                if (json.RootElement.TryGetProperty("questions", out var questions) && questions.ValueKind == System.Text.Json.JsonValueKind.Array)
                    foreach (var q in questions.EnumerateArray())
                        if (q.TryGetProperty("id", out var qid) && q.TryGetProperty("lookAgain", out var look) && look.ValueKind == System.Text.Json.JsonValueKind.String)
                            lookAgain[(file, qid.GetString() ?? "")] = look.GetString() ?? "";
            }
            catch (System.Text.Json.JsonException ex) { problems.Add($"{file}: {ex.Message}"); }
        }
        var partPages = new Dictionary<string, string>(StringComparer.Ordinal);
        var parts = Path.Combine(folder, "parts");
        if (Directory.Exists(parts))
            foreach (var file in Directory.GetFiles(parts, "*.md").Order(StringComparer.OrdinalIgnoreCase))
            {
                var text = File.ReadAllText(file).Replace("\r\n", "\n", StringComparison.Ordinal);
                var end = text.StartsWith("---\n", StringComparison.Ordinal) ? text.IndexOf("\n---\n", 4, StringComparison.Ordinal) : -1;
                var name = end < 0 ? null : text[4..end].Split('\n').Select(l => l.Trim())
                    .FirstOrDefault(l => l.StartsWith("part:", StringComparison.Ordinal))?[5..].Trim();
                if (string.IsNullOrEmpty(name)) { problems.Add($"parts/{Path.GetFileName(file)}: no front matter with a part: line."); continue; }
                partPages[name] = text[(end + 5)..].Trim();
            }
        return new StatsContent { Package = package, Tasks = tasks, TaskProblems = problems, LookAgain = lookAgain, PartPages = partPages };
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

        problems.AddRange(CheckExamples());

        foreach (var item in Package.AllItems)
        {
            if (item.Exercise is { } ex && Find(ex) is { IsExercise: false })
                problems.Add($"{item.File}: the exercise {ex} asks for no results. Give it an \"ask\" list, or use it as the example.");

            // Widgets in questions (plan 10.15): the Learning Core checks them in Learn and Reference pages only.
            if (item.Quiz is { } quiz)
                foreach (var q in quiz.Questions)
                    if (item.QuizFile is { } qf && LookAgain.TryGetValue((qf, q.Id), out var look)
                        && (Package.FindItem(look) is not { } target || target.Kind == ItemKind.Assess))
                        problems.Add($"{qf}, question {q.Id}: \"lookAgain\" names {look}, which is not an item that teaches (a lesson of the package).");

            if (item.Quiz is { } quiz2)
                foreach (var q in quiz2.Questions)
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

    /// <summary>
    /// The Example tab's written examples (Learning Core plan 3.22): each has a title and a figure (an example to try, a figure, the
    /// data or the results: Ron, 2026-11-17, "always contains some type of figure"); no see-example button inside one; and every
    /// <c>[[see-example: N]]</c> on a Learn page names one of the item's examples (the written ones first, then the item's own
    /// <c>example:</c> task).
    /// </summary>
    public IEnumerable<string> CheckExamples()
    {
        foreach (var item in Package.AllItems)
        {
            var (learn, pages) = ExamplePages.Split(item.Learn);
            for (var i = 0; i < pages.Count; i++)
            {
                var widgets = Widgets.In(pages[i].Markdown).ToList();
                if (pages[i].Title.Length == 0) yield return $"{item.File}: example {i + 1} has no title. Write it as [[example: A title]].";
                if (!widgets.Any(w => FigureWidgets.Contains(w.Name, StringComparer.OrdinalIgnoreCase)))
                    yield return $"{item.File}: example {i + 1} ({pages[i].Title}) has no figure: give it an example to try ([[try: …]]), a [[figure: …]], the [[data: …]] or the [[results: …]].";
                if (widgets.Any(w => w.Name.Equals("see-example", StringComparison.OrdinalIgnoreCase)))
                    yield return $"{item.File}: example {i + 1} ({pages[i].Title}) has a see-example button; those go on the Learn page.";
            }
            if (Widgets.In(learn).Any(w => w.Name.Equals("try", StringComparison.OrdinalIgnoreCase)))
                yield return $"{item.File}: a [[try: …]] is on the Learn page. Examples to try go on the Example tab: put it in an [[example: Title]] section at the end of the file, and a [[see-example: N]] button where it belongs in the lesson.";
            var count = pages.Count + (Find(item.Example) is null ? 0 : 1);
            foreach (var w in Widgets.In(learn).Where(w => w.Name.Equals("see-example", StringComparison.OrdinalIgnoreCase)))
                if (!int.TryParse(w.Argument.Trim(), out var n) || n < 1 || n > count)
                    yield return $"{item.File}: [[see-example: {w.Argument.Trim()}]] names no example: the item has {count} (number them from 1).";
        }
    }

    private string? CheckWidget(WidgetTag tag)
    {
        if (!WidgetNames.Contains(tag.Name, StringComparer.OrdinalIgnoreCase))
            return $"[[{tag.Name}: …]] is not a widget. Use one of: {string.Join(", ", WidgetNames)}.";
        if (tag.Name.Equals("example", StringComparison.OrdinalIgnoreCase) || tag.Name.Equals("see-example", StringComparison.OrdinalIgnoreCase))
            return null; // checked with their item (CheckExamples)
        if (tag.Name.Equals("try", StringComparison.OrdinalIgnoreCase))
            return TrySpec.Parse(tag.Argument, out var parseProblem) is not { } spec ? parseProblem
                : spec.Problems(this).FirstOrDefault() is { } problem ? problem : null;
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
