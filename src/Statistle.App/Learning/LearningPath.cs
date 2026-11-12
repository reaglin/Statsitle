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
/// package — My progress at the top, then each module with its items — and <see cref="Refresh"/> puts every row's mark and
/// words for a learner's progress (call it after sign-in and after anything that changes progress). PLC+'s "Hand in a report"
/// row is not here: reports come in a later phase. The main window adds its Workbench rows after these.
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
        Dashboard = new PathNode { IsDashboard = true, Weight = FontWeights.SemiBold, Header = "My progress", Mark = "▤" };
        Roots.Add(Dashboard);
        foreach (var m in package.Modules)
        {
            var node = new PathNode { Module = m, Weight = FontWeights.SemiBold, Header = $"{m.Number}. {m.Title}" };
            foreach (var i in m.Items) node.Children.Add(new PathNode { Module = m, Item = i, Parent = node, Header = $"{i.Number} {i.Title}" });
            Roots.Add(node);
        }
    }

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

    /// <summary>The rows at the top of the tree: My progress, then the modules (each with its items as children).</summary>
    public ObservableCollection<PathNode> Roots { get; } = [];

    /// <summary>The My progress row.</summary>
    public PathNode Dashboard { get; }

    /// <summary>"12 of 98 items · 2 of 15 modules complete", for the status bar.</summary>
    public string TotalsText { get; private set; } = "";

    /// <summary>"Time in Statistics+: 3 h 10 min (today 25 min)", for the status bar.</summary>
    public string TimeText { get; private set; } = "";

    /// <summary>Every row's mark and words, the totals and the time, for <paramref name="progress"/> (null: nobody's).</summary>
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

        foreach (var node in Roots.Where(r => r.Module is not null && r.Item is null))
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
                PathState.ComingLater => "Coming later",
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
                child.Detail = ItemDetail(item, state);
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
    public PathNode? NodeOf(LearningItem item) => Roots.SelectMany(r => r.Children).FirstOrDefault(n => n.Item == item);

    /// <summary>The row of a module, or null.</summary>
    public PathNode? NodeOf(LearningModule module) => Roots.FirstOrDefault(r => r.Module == module && r.Item is null);

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
        if (node.Parent is { } parent) parent.IsExpanded = true;
        node.IsSelected = true;
    }
}
