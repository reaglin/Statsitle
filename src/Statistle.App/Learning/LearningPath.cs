using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Learning.Core;
using Learning.Core.Content;
using Learning.Core.Progress;
using CoreDashboard = Learning.Core.Progress.Dashboard;

namespace Statistle.App.Learning;

/// <summary>
/// The learning path's tree (ported from PLC+'s ShellViewModel): <see cref="Build"/> makes the rows once from the content
/// package — PlusBook Tools first (My progress, Hand in a report, the Workbench; plan W.2), then each module with its items —
/// and <see cref="Refresh"/> puts every row's mark and words for a learner's progress (call it after sign-in and after anything
/// that changes progress). The book has one part, so the path has no part rows (plan W.1): the modules sit at the top.
/// </summary>
public sealed class LearningPath
{
    private static readonly LearningProgress NoOne = new();
    private readonly List<LearningItem> _order;

    public LearningPath(ContentPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        Package = package;
        _order = [.. package.AllItems];
        // Plan W.2 (Ron, 2026-11-23): the rows that are not the book's materials share one folding row of their own, PlusBook Tools,
        // first in the path, so the book's materials come first when it is folded.
        Common = new PathNode
        {
            IsCommon = true, Weight = FontWeights.Bold, Header = "PlusBook Tools", Mark = "+",
            Detail = "My progress, reports, notes, the glossary and the Workbench",
            SpokenName = "PlusBook Tools: my progress, hand in a report, my notes, the glossary and the Workbench",
            Tip = "My progress, Hand in a report, My notes, the Glossary and the Workbench. The arrow beside it (or the → and ← keys) opens and folds them; choosing its name opens My progress",
        };
        Roots.Add(Common);
        Dashboard = new PathNode { IsDashboard = true, Weight = FontWeights.SemiBold, Header = "My progress", Mark = "▤", Parent = Common };
        Common.Children.Add(Dashboard);
        Reports = new PathNode
        {
            IsReports = true, Weight = FontWeights.SemiBold, Header = "Hand in a report", Mark = "✉", Parent = Common,
            Tip = "Makes a signed progress report (PDF) of a module check, or of everything so far, to hand in to your instructor",
        };
        Common.Children.Add(Reports);
        // My notes and the Glossary (Learning Core plan 3.2, 3.13; Q17, Ron 2026-11-23: "the same as the other PlusBooks").
        Notes = new PathNode
        {
            IsNotes = true, Weight = FontWeights.SemiBold, Header = "My notes", Mark = "✎", Parent = Common,
            Tip = "Every note you have written on the lessons, with a search box. Only you see them",
        };
        Common.Children.Add(Notes);
        Glossary = new PathNode
        {
            IsGlossary = true, Weight = FontWeights.SemiBold, Header = "Glossary", Mark = "Aa", Parent = Common,
            Detail = $"{package.Glossary.Entries.Count} key terms, A to Z",
            SpokenName = $"Glossary: {package.Glossary.Entries.Count} key terms",
            Tip = "The book's key terms and what they mean, with the items that use them",
        };
        Common.Children.Add(Glossary);
        Workbench = new PathNode
        {
            IsTool = true, Weight = FontWeights.SemiBold, Header = "Workbench", Mark = "∑", Parent = Common,
            Detail = "The calculator: your own data and every analysis",
            SpokenName = "Workbench: the calculator, for your own data and every analysis",
            Tip = "Opens the Workbench, the statistics calculator, for your own data and assignments (Ctrl+B). Nothing there counts toward your progress; Course (Ctrl+L) comes back",
        };
        Common.Children.Add(Workbench);
        // Plan W.1 (Ron, 2026-11-23: the book in three parts): each part holds its modules, so a part opens and folds on its own.
        // A module before any part (Start here) stays at the top of the tree.
        PathNode? part = null;
        foreach (var m in package.Modules)
        {
            if (package.StartsPart(m)) Roots.Add(part = PartHeading(package, m));
            else if (package.PartOf(m) is null) part = null;
            var node = new PathNode { Module = m, Parent = part, Weight = FontWeights.SemiBold, Header = $"{m.Number}. {m.Title}" };
            foreach (var i in m.Items) node.Children.Add(new PathNode { Module = m, Item = i, Parent = node, Header = $"{i.Number} {i.Title}" });
            if (part is null) Roots.Add(node); else part.Children.Add(node);
        }
    }

    /// <summary>The heading row of the part that begins with <paramref name="first"/>: its name and its modules' numbers.</summary>
    private static PathNode PartHeading(ContentPackage package, LearningModule first)
    {
        var name = first.Part!;
        var range = Range(package.Modules.Where(m => package.PartOf(m) == name).Select(m => m.Number).ToList());
        return new PathNode
        {
            IsPart = true, PartFirst = first, Header = name, Weight = FontWeights.Bold, Detail = range, Mark = "📖",
            SpokenName = $"{name}: {range}", Tip = PartTip(name, range),
        };
    }

    private static string Range(IReadOnlyList<int> numbers) => numbers.Count == 1 ? $"Module {numbers[0]}" : $"Modules {numbers[0]} to {numbers[^1]}";

    private static string PartTip(string part, string detail) =>
        $"{part}: {detail}. Choose it to read what this part is. The arrow beside it (or the → and ← keys) opens and folds its modules.";

    /// <summary>The part rows (Part I, II, III), in order.</summary>
    public IEnumerable<PathNode> Parts => Roots.Where(r => r.IsPart);

    /// <summary>Builds the tree for a package, with every mark for <paramref name="progress"/> (null: nobody's yet).</summary>
    public static LearningPath Build(ContentPackage package, LearningProgress? progress = null)
    {
        var path = new LearningPath(package);
        path.Refresh(progress);
        return path;
    }

    public ContentPackage Package { get; }

    /// <summary>The learner's progress the marks were last drawn for.</summary>
    public LearningProgress Progress { get; private set; } = NoOne;

    /// <summary>The rows at the top of the tree: PlusBook Tools, then the modules (each with its items as children).</summary>
    public ObservableCollection<PathNode> Roots { get; } = [];

    /// <summary>PlusBook Tools (plan W.2): the folding row that holds My progress, Hand in a report and the Workbench.</summary>
    public PathNode Common { get; }

    /// <summary>The My progress row.</summary>
    public PathNode Dashboard { get; }

    /// <summary>The Hand in a report row.</summary>
    public PathNode Reports { get; }

    /// <summary>The Workbench row (the book's tool).</summary>
    public PathNode Workbench { get; }

    /// <summary>The My notes row.</summary>
    public PathNode Notes { get; }

    /// <summary>The Glossary row.</summary>
    public PathNode Glossary { get; }

    /// <summary>Every module's row, inside its part or at the top of the tree, in the book's order.</summary>
    public IEnumerable<PathNode> ModuleNodes =>
        Roots.SelectMany(r => r.IsPart ? r.Children : [r]).Where(n => n.Module is not null && n.Item is null);

    /// <summary>Every row in the tree, at any depth.</summary>
    public IEnumerable<PathNode> AllNodes => Roots.SelectMany(Self);

    private static IEnumerable<PathNode> Self(PathNode n) => n.Children.SelectMany(Self).Prepend(n);

    /// <summary>Collapse Menu (plan W.1, W.5): PlusBook Tools and every module fold, so the path shows the modules only.</summary>
    public void FoldAll()
    {
        foreach (var n in AllNodes) n.IsExpanded = false;
    }

    /// <summary>The rows that hold this one open, so it can be seen (the others stay as they are).</summary>
    public static void OpenTo(PathNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        for (var p = node.Parent; p is not null; p = p.Parent) p.IsExpanded = true;
    }

    /// <summary>Chooses a row (again, if it is already chosen, so its page shows again), opening the rows that hold it.</summary>
    public static void Choose(PathNode node)
    {
        OpenTo(node);
        node.IsSelected = false;
        node.IsSelected = true;
    }

    /// <summary>After every <see cref="Refresh"/>: the totals and the time have changed (the block under the tree shows them).</summary>
    public event Action? Refreshed;

    /// <summary>"12 of 98 items · 2 of 15 modules complete", for the status bar.</summary>
    public string TotalsText { get; private set; } = "";

    /// <summary>"Time in Statistics+: 3 h 10 min (today 25 min)", for the status bar.</summary>
    public string TimeText { get; private set; } = "";

    /// <summary>Every row's mark and words, the totals and the time, for <paramref name="progress"/> (null: nobody's).</summary>
    /// <summary>The signed-in learner's notes per item (item id → count), for "✎ n notes" in the path; null: none.</summary>
    public Func<IReadOnlyDictionary<string, int>>? NoteCounts { get; set; }

    public void Refresh(LearningProgress? progress, DateTimeOffset? at = null)
    {
        Progress = progress ?? NoOne;
        var on = LearningTheme.Brush("PowerOnBrush");
        var started = LearningTheme.Brush("AccentBrush");
        var muted = LearningTheme.Brush("MutedTextBrush");
        var now = at ?? DateTimeOffset.Now;
        var summary = CoreDashboard.Build(Package, Progress, now);
        var review = summary.ReviewSuggested.Count;
        Dashboard.Header = "My progress";
        Dashboard.Mark = "▤";
        Dashboard.MarkBrush = started;
        Dashboard.Detail = $"{summary.Totals.ItemsComplete} of {summary.Totals.Items} items · {CoreDashboard.Duration(summary.Totals.Time)}" +
                           (review > 0 ? $" · {review} to review" : "");
        Dashboard.SpokenName = "My progress: " + Dashboard.Detail;
        Dashboard.Tip = $"Your progress through the whole path: each module, the module checks, outcomes to review, and your time in {LearningApp.Name}";
        var ready = summary.Submissions.Count(x => x.State != SubmissionState.NotReached);
        Reports.Detail = $"A signed PDF for your instructor · {ready} of {summary.Submissions.Count} module checks complete";
        Reports.SpokenName = "Hand in a report: " + Reports.Detail;
        var noteCounts = NoteCounts?.Invoke() ?? new Dictionary<string, int>();
        var notes = noteCounts.Values.Sum();
        Notes.MarkBrush = notes > 0 ? started : muted;
        Notes.Detail = notes > 0 ? $"{notes} note{(notes == 1 ? "" : "s")} on {noteCounts.Count} item{(noteCounts.Count == 1 ? "" : "s")}" : "Your own notes on the lessons";
        Notes.SpokenName = "My notes: " + Notes.Detail;
        Glossary.MarkBrush = started;

        foreach (var part in Parts)
        {
            var entries = summary.Modules.Where(e => part.Children.Any(c => c.Module == e.Module)).ToList();
            var range = Range([.. entries.Select(e => e.Module.Number)]);
            // A part whose modules are all still to come says so (plan W.6), not "0 of 0 items".
            part.Detail = entries.Count > 0 && entries.All(e => e.State == PathState.ComingLater)
                ? $"{range} · in development"
                : $"{range} · {entries.Sum(e => e.Complete)} of {entries.Sum(e => e.Total)} items complete";
            var titles = string.Join(", ", entries.Select(e => e.Module.Title));
            part.SpokenName = $"{part.Header}: {part.Detail}. Its modules: {titles}";
            part.Tip = PartTip(part.Header, part.Detail) + $" Its modules: {titles}.";
        }

        foreach (var node in ModuleNodes)
        {
            var entry = summary.Modules.FirstOrDefault(e => e.Module == node.Module);
            if (entry is null) continue;
            var m = node.Module!;
            node.Header = $"{m.Number}. {m.Title}";
            node.Weight = FontWeights.SemiBold;
            node.Mark = CoreDashboard.Mark(entry.State);
            node.MarkBrush = entry.State switch
            {
                PathState.Complete or PathState.Mastered => on,
                PathState.InProgress or PathState.NeedsReview => started,
                _ => muted,
            };
            node.Detail = entry.State switch
            {
                PathState.ComingLater => "In development", // Ron, 2026-11-23 (plan W.6): not "Coming later"
                PathState.Outline => $"{entry.Complete} of {entry.Total} complete · outline",
                PathState.Mastered => $"{entry.Complete} of {entry.Total} complete · mastered{StatusWords(m)}",
                PathState.NeedsReview => $"{entry.Complete} of {entry.Total} complete · review suggested{StatusWords(m)}",
                PathState.StartAfter => $"{entry.Complete} of {entry.Total} complete · after module {string.Join(" and ", entry.RecommendedFirst.Select(x => x.Number))}{StatusWords(m)}",
                _ => $"{entry.Complete} of {entry.Total} complete{StatusWords(m)}",
            };
            node.SpokenName = $"Module {m.Number}, {m.Title}: {CoreDashboard.Words(entry)}. {node.Detail}";
            node.Tip = $"Module {m.Number}: {m.Title}. {CoreDashboard.Words(entry)}. Choose it to see what it teaches, what to finish first, and its items.";

            foreach (var child in node.Children.Where(c => c.Item is not null))
            {
                var item = child.Item!;
                var state = Progress.StateOf(item);
                child.Header = $"{item.Number} {item.Title}";
                (child.Mark, child.MarkBrush) = state switch
                {
                    ItemState.Complete => ("✓", on),
                    ItemState.InProgress => ("◐", started),
                    _ => ("○", muted),
                };
                child.Detail = ItemDetail(item, state) + (noteCounts.TryGetValue(item.Id, out var n) ? $" · ✎ {n} note{(n == 1 ? "" : "s")}" : "");
                child.SpokenName = $"{item.Number} {item.Title}: {child.Detail}";
                child.Tip = item.Kind == ItemKind.Assess
                    ? item.Quiz is null ? "The module check: complete when every item it covers is" : "The module check: complete when every item it covers is complete and you pass its questions"
                    : item.Exercise is not null ? "An exercise: complete when Check my answers finds your results right"
                    : item.Quiz is not null ? "Questions: complete when you pass the practice questions on the Exercise tab"
                    : "A reading: complete when you press I have read this";
            }
        }
        TotalsText = $"{summary.Totals.ItemsComplete} of {summary.Totals.Items} items · {summary.Totals.ModulesComplete} of {summary.Totals.Modules} modules complete";
        TimeText = TimeWords(Progress, now);
        Refreshed?.Invoke();
    }

    /// <summary>"Time in Statistics+: 3 h 10 min (today 25 min)", or "none yet".</summary>
    public static string TimeWords(LearningProgress progress, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(progress);
        var all = progress.TimeOnTask();
        var today = progress.TimeOnTask(s => s.Start.ToLocalTime().Date == now.ToLocalTime().Date);
        return all.TotalMinutes < 1 ? $"Time in {LearningApp.Name}: none yet"
            : $"Time in {LearningApp.Name}: {CoreDashboard.Duration(all)}" + (today.TotalMinutes >= 1 ? $" (today {CoreDashboard.Duration(today)})" : "");
    }

    /// <summary>" · first draft" or " · outline" after a module's words, or nothing for a finished module.</summary>
    public static string StatusWords(LearningModule m)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m.Status switch
        {
            ModuleStatus.Draft => "",
            ModuleStatus.Outline => " · outline",
            _ => "",
        };
    }

    /// <summary>An item row's detail: "Exercise · best 2 of 3", "Reading · complete", "Module check · 3 of 5 items done · …".</summary>
    public string ItemDetail(LearningItem item, ItemState state)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Kind == ItemKind.Assess)
        {
            var done = item.Covers.Count(c => Package.FindItem(c) is { } i && Progress.StateOf(i) == ItemState.Complete);
            var questions = item.Quiz is null ? ""
                : Progress.Items.TryGetValue(item.Id, out var own) && own.Completed is not null ? " · questions passed"
                : Progress.Best(item) is { } quizBest ? $" · questions: best {quizBest.Score:0} of {quizBest.MaxScore:0}"
                : " · questions not passed yet";
            return $"Module check · {done} of {item.Covers.Count} items done{questions}";
        }
        var kind = item.Exercise is not null ? "Exercise" : item.Quiz is not null ? "Questions" : "Reading";
        if (state == ItemState.Complete) return $"{kind} · complete";
        if (!item.CompletesByReading && Progress.Best(item) is { } best) return $"{kind} · best {best.Score:0} of {best.MaxScore:0}";
        return state == ItemState.InProgress ? $"{kind} · started" : kind;
    }

    /// <summary>Where the learner is with an item, for its page: "Complete ✓", "Started: best 2 of 3", "Not started".</summary>
    public string WhereYouAre(LearningItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var state = Progress.StateOf(item);
        if (item.Kind == ItemKind.Assess) return ItemDetail(item, state).Replace("Module check · ", "", StringComparison.Ordinal);
        if (state == ItemState.Complete) return "Complete ✓";
        if (!item.CompletesByReading && Progress.Best(item) is { } best) return $"Started: best {best.Score:0} of {best.MaxScore:0}";
        return state == ItemState.InProgress ? "Started" : "Not started";
    }

    /// <summary>The row of an item, or null.</summary>
    public PathNode? NodeOf(LearningItem item) => ModuleNodes.SelectMany(r => r.Children).FirstOrDefault(n => n.Item == item);

    /// <summary>The row of a module, or null.</summary>
    public PathNode? NodeOf(LearningModule module) => ModuleNodes.FirstOrDefault(r => r.Module == module);

    /// <summary>The item after this one in the path (Next), or null at the end.</summary>
    public LearningItem? NextInOrder(LearningItem item)
    {
        var index = _order.IndexOf(item);
        return index >= 0 && index + 1 < _order.Count ? _order[index + 1] : null;
    }

    /// <summary>The item before this one in the path (Previous), or null at the start.</summary>
    public LearningItem? PreviousInOrder(LearningItem item)
    {
        var index = _order.IndexOf(item);
        return index > 0 ? _order[index - 1] : null;
    }

    /// <summary>Chooses an item's row (its module opened), so the tree shows it.</summary>
    public void Select(LearningItem item)
    {
        if (NodeOf(item) is not { } node) return;
        OpenTo(node); // only its module opens; the others stay as they are (plan W.1)
        node.IsSelected = true;
    }
}
