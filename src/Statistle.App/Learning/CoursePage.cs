using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Learning.Core;
using Learning.Core.Content;
using Learning.Core.Learners;
using Learning.Core.Progress;
using Statistle.App.Plotting;
using Statistle.Core.Plotting;
using Statistle.Learning;

namespace Statistle.App.Learning;

/// <summary>
/// The Course (plan 9.2-9.4): the learning path on the left, and on the right My progress, a module's first page,
/// or an item with its Learn, Example, Exercise and Reference tabs. Progress is recorded in the signed-in learner's
/// file and saved at once; without a learner the Course can be read, but nothing is kept.
/// </summary>
public sealed class CoursePage : UserControl
{
    private readonly StatsContent _content;
    private readonly LearningPath _path;
    private readonly Dictionary<string, BuiltTask?> _built = new(StringComparer.OrdinalIgnoreCase);
    private readonly TreeView _tree = new();
    private readonly ContentControl _right = new() { Focusable = false };
    private readonly TextBlock _notSignedIn;

    // The item page, built once and refilled.
    private readonly TextBlock _itemModule = new() { Foreground = LearningTheme.Brush("MutedTextBrush") };
    private readonly TextBlock _itemTitle = new() { FontSize = 20, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _itemStatus = new() { Foreground = LearningTheme.Brush("MutedTextBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
    private readonly Button _previous = new() { Content = "◂ Previous", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 6, 0) };
    private readonly Button _next = new() { Content = "Next ▸", Padding = new Thickness(10, 3, 10, 3) };
    private readonly TabControl _tabs = new() { Margin = new Thickness(0, 8, 0, 0) };
    private readonly TabItem _learnTab, _exampleTab, _exerciseTab, _referenceTab;
    private readonly MarkdownView _learn = new();
    private readonly Button _read = new() { Content = "I have read this", Padding = new Thickness(12, 4, 12, 4), HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBlock _learnFooter = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
    private readonly TaskWorkspace _example;
    private readonly TaskWorkspace _exercise;
    private readonly QuizView _quiz = new();
    private readonly MarkdownView _exerciseNone = new();
    private readonly ContentControl _exerciseHost = new() { Focusable = false };
    private readonly MarkdownView _reference = new();
    private readonly FrameworkElement _itemPage;

    private LearnerFile? _session;
    private LearningItem? _item;

    /// <summary>A sentence for the status line.</summary>
    public event Action<string>? Status;

    /// <summary>Open in the Workbench, from an Example or a solved Exercise.</summary>
    public event Action<BuiltTask>? OpenInWorkbench;

    /// <summary>The "Sign in" link on the not-signed-in banner.</summary>
    public event Action? SignInRequested;

    public CoursePage(StatsContent content)
    {
        _content = content;
        _path = LearningPath.Build(content.Package);
        LearningTheme.Apply(this);
        Focusable = false;

        _example = new TaskWorkspace(RenderMarkdown);
        _exercise = new TaskWorkspace(RenderMarkdown);
        _example.OpenInWorkbench += b => OpenInWorkbench?.Invoke(b);
        _exercise.OpenInWorkbench += b => OpenInWorkbench?.Invoke(b);
        _exercise.Checked += Exercise_Checked;
        _quiz.Graded += Quiz_Graded;
        _learn.WidgetFactory = _reference.WidgetFactory = _exerciseNone.WidgetFactory = Widget;

        // ── The tree ──
        _tree.ItemTemplate = TreeTemplate();
        _tree.ItemsSource = _path.Roots;
        _tree.BorderThickness = new Thickness(0);
        _tree.Background = Brushes.Transparent;
        ScrollViewer.SetHorizontalScrollBarVisibility(_tree, ScrollBarVisibility.Disabled);
        _tree.SelectedItemChanged += (_, e) => { if (e.NewValue is PathNode node) Open(node); };
        AutomationProperties.SetName(_tree, "The course: My progress, then each module and its items");
        var treeStyle = new Style(typeof(TreeViewItem));
        treeStyle.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty, new Binding(nameof(PathNode.IsExpanded)) { Mode = BindingMode.TwoWay }));
        treeStyle.Setters.Add(new Setter(TreeViewItem.IsSelectedProperty, new Binding(nameof(PathNode.IsSelected)) { Mode = BindingMode.TwoWay }));
        treeStyle.Setters.Add(new Setter(AutomationProperties.NameProperty, new Binding(nameof(PathNode.SpokenName))));
        treeStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(nameof(PathNode.Tip))));
        _tree.ItemContainerStyle = treeStyle;

        _notSignedIn = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8), Padding = new Thickness(8, 5, 8, 5), Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF4, 0xD6)) };
        var signIn = new Hyperlink(new Run("Sign in")) { ToolTip = "Opens the Who is learning? screen, so your progress is kept" };
        signIn.Click += (_, _) => SignInRequested?.Invoke();
        _notSignedIn.Inlines.Add(new Run("Not signed in: you can read and try everything, but your progress is not kept. "));
        _notSignedIn.Inlines.Add(signIn);

        var left = new DockPanel { Margin = new Thickness(10, 10, 6, 10) };
        var leftTitle = new TextBlock { Text = "The course", FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(4, 0, 0, 2) };
        var leftLead = new TextBlock
        {
            Text = "Work through the modules in order. Each item has a lesson, a worked example, an exercise and a reference page. ✓ complete · ◐ started · ○ not started.",
            TextWrapping = TextWrapping.Wrap, Foreground = LearningTheme.Brush("MutedTextBrush"), Margin = new Thickness(4, 0, 0, 8), FontSize = 11.5,
        };
        DockPanel.SetDock(leftTitle, Dock.Top);
        DockPanel.SetDock(leftLead, Dock.Top);
        DockPanel.SetDock(_notSignedIn, Dock.Top);
        left.Children.Add(leftTitle);
        left.Children.Add(leftLead);
        left.Children.Add(_notSignedIn);
        left.Children.Add(_tree);

        // ── The item page ──
        foreach (var b in new[] { _previous, _next, _read }) ToolTipService.SetShowOnDisabled(b, true);
        _previous.Click += (_, _) => { if (_item is not null && _path.PreviousInOrder(_item) is { } p) _path.Select(p); };
        _next.Click += (_, _) => { if (_item is not null && _path.NextInOrder(_item) is { } n) _path.Select(n); };
        _read.Click += (_, _) => MarkRead();

        var learnPanel = new DockPanel();
        var footer = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
        DockPanel.SetDock(_read, Dock.Left);
        footer.Children.Add(_read);
        footer.Children.Add(_learnFooter);
        DockPanel.SetDock(footer, Dock.Bottom);
        learnPanel.Children.Add(footer);
        learnPanel.Children.Add(_learn);

        _learnTab = Tab("_Learn", learnPanel, "The lesson: the idea, the formulas, a worked case, and the Python and R");
        _exampleTab = Tab("Exa_mple", _example, "A worked example: the data, every result, the figures and the code");
        _exerciseTab = Tab("Exerc_ise", _exerciseHost, "Your turn: work out the results and check them, or answer the questions");
        _referenceTab = Tab("_Reference", _reference, "The formulas, the Python and R functions, and the sources, to look back at");
        _tabs.Items.Add(_learnTab);
        _tabs.Items.Add(_exampleTab);
        _tabs.Items.Add(_exerciseTab);
        _tabs.Items.Add(_referenceTab);

        var header = new DockPanel();
        var nav = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
        nav.Children.Add(_previous);
        nav.Children.Add(_next);
        DockPanel.SetDock(nav, Dock.Right);
        header.Children.Add(nav);
        var titles = new StackPanel();
        titles.Children.Add(_itemModule);
        titles.Children.Add(_itemTitle);
        titles.Children.Add(_itemStatus);
        header.Children.Add(titles);
        var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        DockPanel.SetDock(header, Dock.Top);
        page.Children.Add(header);
        page.Children.Add(_tabs);
        _itemPage = page;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320), MinWidth = 220 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var leftBorder = new Border { Child = left, Background = LearningTheme.Brush("PanelBrush") };
        var splitter = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = LearningTheme.Brush("LineBrush"), ToolTip = "Drag to make the course list wider or narrower" };
        Grid.SetColumn(splitter, 1);
        Grid.SetColumn(_right, 2);
        grid.Children.Add(leftBorder);
        grid.Children.Add(splitter);
        grid.Children.Add(_right);
        Content = grid;

        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _learnTab), Key.D1, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _exampleTab), Key.D2, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _exerciseTab), Key.D3, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _referenceTab), Key.D4, ModifierKeys.Control));

        SignIn(null);
    }

    private LearningProgress Progress => _session?.Data.Progress ?? _anonymous;
    private readonly LearningProgress _anonymous = new();

    /// <summary>The path shown in the tree (for the window's menus and the tests).</summary>
    public LearningPath Path => _path;

    /// <summary>The item page's four tabs (for the tests).</summary>
    public TabControl ItemTabs => _tabs;

    /// <summary>The Exercise tab's task view (for the tests).</summary>
    public TaskWorkspace ExerciseView => _exercise;

    /// <summary>The item on the right, if an item is shown.</summary>
    public LearningItem? CurrentItem => _item;

    /// <summary>The learner whose progress is shown and kept (null: nobody signed in).</summary>
    public LearnerFile? Session => _session;

    /// <summary>Shows the Course for a learner (or for nobody), on My progress.</summary>
    public void SignIn(LearnerFile? session)
    {
        _session = session;
        QuizView.ForgetAll();
        TaskWorkspace.ForgetAll();
        _notSignedIn.Visibility = session is null ? Visibility.Visible : Visibility.Collapsed;
        _path.Refresh(Progress);
        _path.Dashboard.IsSelected = true;
        ShowDashboard();
    }

    /// <summary>Saves the learner's progress now (after every change, and before signing out or closing).</summary>
    public void Save()
    {
        if (_session is null) return;
        try { _session.Save(DateTimeOffset.Now); }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            Status?.Invoke($"Your progress could not be saved just now ({ex.Message}). Statistics+ tries again with your next step.");
        }
    }

    /// <summary>F1 in the Course: how the Course works (USABILITY-CHECKLIST 1.2).</summary>
    public void ShowHelp()
    {
        _item = null;
        var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        var back = new Button { Content = "◂ Back to My progress", Padding = new Thickness(10, 3, 10, 3), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8), ToolTip = "Closes this help and shows My progress" };
        back.Click += (_, _) => { _path.Dashboard.IsSelected = true; ShowDashboard(); };
        DockPanel.SetDock(back, Dock.Top);
        page.Children.Add(back);
        page.Children.Add(new MarkdownView { Markdown = HelpMarkdown });
        _right.Content = page;
        Status?.Invoke("How the Course works. Choose an item on the left to go back to it.");
    }

    private const string HelpMarkdown = """
        # How the Course works

        The Course follows EGN3443, Probability and Statistics for Engineers, topic by topic.

        ## The list on the left

        - **My progress**: every module, the module checks, the outcomes to review, and your time.
        - Each **module** opens on a page with what it teaches (its outcomes) and its items.
        - Each **item** is one topic. Its mark: **✓** complete, **◐** started, **○** not started.

        ## The four tabs of an item

        | Tab | Keys | What it is for |
        |---|---|---|
        | **Learn** | Ctrl+1 | The lesson: the idea, the formulas, a worked case, the Python and R |
        | **Example** | Ctrl+2 | A worked example: the data, every result, the figures and the code. **Open in the Workbench** copies it to the calculator, to change the inputs |
        | **Exercise** | Ctrl+3 | Your turn: work out the results (by hand, or in Python or R) and press **Check my answers**, or answer the questions |
        | **Reference** | Ctrl+4 | The formulas, the Python and R functions, and the sources |

        An item is complete when its exercise passes, when you pass its questions, or (for a reading) when you press
        **I have read this**. **Previous** and **Next** (top right) go through the items in order.

        A wrong answer says whether it is too high or too low, or where to look again; never the answer itself. Try as often
        as you like. **Show a hint** gives one hint at a time.

        ## Your progress

        Sign in on the **Who is learning?** screen (Learner ▸ Sign in). Your progress is kept in a file of your own, locked
        with your PIN, and saved after every step. **Learner ▸ Back up my progress** saves a copy to a USB drive or a cloud
        folder. **Learner ▸ Sign out** saves and locks it; so does closing Statistics+, and so does leaving the computer
        unused for a while.

        ## The Workbench

        The **Workbench** (Ctrl+B) is the calculator, for your own data and assignments: data tables, every analysis, the
        report, the plots and the Python and R. **Course** (Ctrl+L) comes back here.

        More help: Help ▸ Help on the web.
        """;

    // ── What the right side shows ─────────────────────────────────────────────

    private void Open(PathNode node)
    {
        if (node.IsDashboard) ShowDashboard();
        else if (node.Item is { } item) ShowItem(item);
        else if (node.Module is { } module) ShowModule(module);
    }

    private void ShowDashboard()
    {
        _item = null;
        var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        var title = new TextBlock { Text = "My progress" + (_session is null ? "" : " · " + _session.Data.Profile.DisplayName), FontSize = 20, FontWeight = FontWeights.SemiBold };
        DockPanel.SetDock(title, Dock.Top);
        page.Children.Add(title);
        var next = Progress.Continue(_content.Package);
        var go = new Button
        {
            Content = next is null ? "Every item is complete" : $"Continue with {next}", IsEnabled = next is not null,
            Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(0, 8, 0, 8), HorizontalAlignment = HorizontalAlignment.Left,
            ToolTip = "Opens the next item that is not complete yet",
        };
        ToolTipService.SetShowOnDisabled(go, true);
        if (next is null) go.ToolTip = "Every item in the course is complete";
        go.Click += (_, _) => { if (next is not null) _path.Select(next); };
        DockPanel.SetDock(go, Dock.Top);
        page.Children.Add(go);
        page.Children.Add(new MarkdownView { Markdown = DashboardPage.Markdown(_content.Package, Progress) });
        _right.Content = page;
        Status?.Invoke("My progress: each module, the module checks, outcomes to review, and your time. Choose a module or an item on the left to work on it.");
    }

    private void ShowModule(LearningModule m)
    {
        _item = null;
        var p = Progress.Of(m);
        var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        var head = new StackPanel();
        head.Children.Add(new TextBlock { Text = $"Module {m.Number} · {m.Title}", FontSize = 20, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        head.Children.Add(new TextBlock
        {
            Text = $"{p.Complete} of {p.Total} items complete" + (m.Minutes > 0 ? $" · about {Hours(m.Minutes)}" : ""),
            Foreground = LearningTheme.Brush("MutedTextBrush"), Margin = new Thickness(0, 2, 0, 6),
        });
        var first = m.Items.FirstOrDefault(i => Progress.StateOf(i) != ItemState.Complete) ?? m.Items.FirstOrDefault();
        if (first is not null)
        {
            var start = new Button
            {
                Content = p.State == ModuleState.NotStarted ? $"Start with {first}" : p.State == ModuleState.Complete ? $"Open {first}" : $"Continue with {first}",
                Padding = new Thickness(12, 4, 12, 4), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8), ToolTip = "Opens the item, on its Learn tab",
            };
            start.Click += (_, _) => _path.Select(first);
            head.Children.Add(start);
        }
        DockPanel.SetDock(head, Dock.Top);
        page.Children.Add(head);
        page.Children.Add(new MarkdownView { Markdown = ModuleMarkdown(m) });
        _right.Content = page;
        Status?.Invoke($"Module {m.Number}: what it teaches and its items. Choose an item on the left, or press {(first is null ? "an item" : "the button")} to begin.");
    }

    private string ModuleMarkdown(LearningModule m)
    {
        var md = new StringBuilder();
        md.AppendLine("## What you will be able to do").AppendLine();
        foreach (var op in Mastery.Of(_content.Package, Progress, m))
            md.AppendLine(op.State switch
            {
                MasteryState.Mastered => $"- ★ {op.Outcome.Text} **(mastered)**",
                MasteryState.NeedsReview => $"- ↺ {op.Outcome.Text} **(review suggested)**",
                MasteryState.InProgress => $"- ◐ {op.Outcome.Text} (in progress)",
                _ => $"- ○ {op.Outcome.Text}",
            });
        md.AppendLine().AppendLine("*★ mastered · ↺ review suggested · ◐ in progress · ○ not started.*").AppendLine();
        if (m.Items.Count > 0)
        {
            md.AppendLine("## The items in this module").AppendLine();
            md.AppendLine("| Item | What it is | Where you are |").AppendLine("|---|---|---|");
            foreach (var i in m.Items)
            {
                var kind = i.Kind == ItemKind.Assess ? "Module check"
                    : i.Exercise is not null ? "Lesson, example and exercise"
                    : i.Quiz is not null ? (i.Example is null ? "Lesson and questions" : "Lesson, example and questions")
                    : "Reading";
                md.AppendLine($"| {i.Number} {i.Title} | {kind} | {_path.WhereYouAre(i)} |");
            }
            md.AppendLine();
        }
        if (m.Introduction.Trim().Length > 0) md.AppendLine("## About this module").AppendLine().AppendLine(m.Introduction);
        return md.ToString();
    }

    private void ShowItem(LearningItem item)
    {
        _quiz.Remember();
        _item = item;
        Progress.Opened(_content.Package, item, DateTimeOffset.Now);
        Save();
        _path.Refresh(Progress);

        _itemModule.Text = $"Module {item.Module.Number} · {item.Module.Title}";
        _itemTitle.Text = $"{item.Number} {item.Title}";
        _previous.IsEnabled = _path.PreviousInOrder(item) is not null;
        _next.IsEnabled = _path.NextInOrder(item) is not null;
        _previous.ToolTip = _path.PreviousInOrder(item) is { } before ? $"Back to {before}" : "This is the first item";
        _next.ToolTip = _path.NextInOrder(item) is { } after ? $"On to {after}" : "This is the last item";

        _learn.Markdown = item.Learn.Trim().Length > 0 ? item.Learn : $"# {item.Title}\n\n*This page is still to be written.*";

        // Example
        if (_content.Find(item.Example) is { } example)
        {
            _example.ShowExample(example);
            _exampleTab.Header = "Exa_mple";
        }
        else
        {
            _example.ShowMessage(item.Kind == ItemKind.Assess
                ? "A module check has no example of its own: each item it covers has its own."
                : "This item has no worked example. Its Learn tab has the figures and tables that go with it.");
            _exampleTab.Header = "Exa_mple (none)";
        }

        // Exercise: a task, the questions, or what the check covers.
        if (_content.Find(item.Exercise) is { } exercise)
        {
            var saved = _session?.Data.Work.TryGetValue(WorkKey(exercise.Id), out var w) == true ? w.Content : null;
            _exercise.ShowExercise(exercise, saved, Progress.StateOf(item) == ItemState.Complete);
            _exerciseHost.Content = _exercise;
            _exerciseTab.Header = "Exerc_ise";
        }
        else if (item.Quiz is { } quiz)
        {
            var intro = new List<string>();
            if (item.Kind == ItemKind.Assess)
            {
                intro.Add("This check covers:");
                intro.AddRange(item.Covers.Select(_content.Package.FindItem).OfType<LearningItem>()
                    .Select(c => $"   •  {c}: {_path.WhereYouAre(c).ToLowerInvariant()}"));
                intro.Add("The module is complete when those items are complete and you have passed these questions.");
            }
            _quiz.WhereToLook = q => q.Objective is { Length: > 0 } objective
                && item.Module.Items.FirstOrDefault(i => i.Kind != ItemKind.Assess && i.Objectives.Contains(objective, StringComparer.OrdinalIgnoreCase)) is { } teaches
                    ? $"Look again at {teaches}."
                    : null;
            _quiz.ShuffleSeed = _session?.Data.Profile.ProfileId ?? "";
            _quiz.Load(quiz, item.Kind == ItemKind.Assess ? $"Module {item.Module.Number} check: the questions" : "Questions", intro);
            _exerciseHost.Content = new ScrollViewer { Content = _quiz, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            _exerciseTab.Header = "Exerc_ise (questions)";
        }
        else
        {
            _exerciseNone.Markdown = item.Kind == ItemKind.Assess ? CoversMarkdown(item)
                : "## No exercise for this item\n\nThis item is complete when you have read it: press **I have read this** on the Learn tab.";
            _exerciseHost.Content = _exerciseNone;
            _exerciseTab.Header = item.Kind == ItemKind.Assess ? "Exerc_ise" : "Exerc_ise (none)";
        }

        _reference.Markdown = ReferenceMarkdown(item);
        ShowItemStatus();
        _tabs.SelectedItem = _learnTab;
        _right.Content = _itemPage;
        Status?.Invoke($"{item}: {_learnFooter.Text}");
    }

    private void ShowItemStatus()
    {
        if (_item is not { } item) return;
        var state = Progress.StateOf(item);
        var parts = new List<string>();
        if (item.Minutes > 0) parts.Add($"About {item.Minutes} minutes");
        parts.Add(state == ItemState.Complete ? "Complete ✓" : _path.ItemDetail(item, state));
        _itemStatus.Text = string.Join(" · ", parts);
        _read.Visibility = item.CompletesByReading ? Visibility.Visible : Visibility.Collapsed;
        _read.IsEnabled = state != ItemState.Complete;
        _read.ToolTip = state == ItemState.Complete ? "This item is already complete" : "Marks this item complete. An item without an exercise is complete when you have read it.";
        _learnFooter.Text = item.Kind == ItemKind.Assess ? "The questions are on the Exercise tab (Ctrl+3)."
            : item.Exercise is not null ? "Next: the worked Example (Ctrl+2), then your own Exercise (Ctrl+3)."
            : item.Quiz is not null ? (state == ItemState.Complete ? "Complete. Next ▸ goes on." : "When you have read the page, answer the questions on the Exercise tab (Ctrl+3).")
            : state == ItemState.Complete ? "Complete. Next ▸ goes on." : "When you have read the page, press I have read this.";
    }

    private string CoversMarkdown(LearningItem item)
    {
        var md = new StringBuilder("## What this module check covers\n\n| Item | Where you are |\n|---|---|\n");
        foreach (var c in item.Covers)
            if (_content.Package.FindItem(c) is { } covered) md.AppendLine($"| {covered} | {_path.WhereYouAre(covered)} |");
        md.AppendLine().AppendLine(Progress.StateOf(item) == ItemState.Complete ? "Every item is complete: so is this module check. ✓" : "When every item above is complete, so is this module check.");
        return md.ToString();
    }

    private string ReferenceMarkdown(LearningItem item)
    {
        if (item.Kind == ItemKind.Assess)
        {
            var md = new StringBuilder($"# Module {item.Module.Number}: what you should be able to do\n\n");
            foreach (var o in item.Module.Outcomes) md.AppendLine("- " + o.Text);
            return md.ToString();
        }
        var pages = item.References.Select(r => _content.Package.References.TryGetValue(r, out var p) ? p : null).OfType<ReferencePage>().ToList();
        return pages.Count == 0 ? "# Reference\n\n*This item has no reference pages yet.*" : string.Join("\n\n", pages.Select(p => p.Body));
    }

    // ── Progress ─────────────────────────────────────────────────────────────

    private static string WorkKey(string taskId) => "answers:" + taskId;

    private void Exercise_Checked(ChallengeOutcome outcome, int hints, string work)
    {
        if (_item is null || !string.Equals(_item.Exercise, outcome.ChallengeId, StringComparison.OrdinalIgnoreCase)) return;
        var wasComplete = Progress.StateOf(_item) == ItemState.Complete;
        Progress.Record(_item, outcome, DateTimeOffset.Now, hints, Environment.MachineName);
        if (_session is not null)
        {
            _session.Data.Work[WorkKey(outcome.ChallengeId)] = new SavedWork { Content = work, Saved = DateTimeOffset.Now };
            if (outcome.Passed) _session.Data.Work[global::Learning.Core.Reports.ReportBuilder.PassedWorkKey(outcome.ChallengeId)] = new SavedWork { Content = work, Saved = DateTimeOffset.Now };
        }
        if (outcome.Passed && !wasComplete) Completed($"Passed: {_item.Number} is complete.");
        else Changed(outcome.Passed ? "Passed again." : $"{outcome.Score:0} of {outcome.MaxScore:0} right. Correct the ones marked ✗ and check again.");
    }

    private void Quiz_Graded(QuizResult result)
    {
        if (_item is null) return;
        var wasComplete = Progress.StateOf(_item) == ItemState.Complete;
        Progress.Record(_item, result.ToOutcome(), DateTimeOffset.Now, machine: Environment.MachineName);
        var right = result.Questions.Count(q => q.Correct);
        if (Progress.StateOf(_item) == ItemState.Complete && !wasComplete) Completed($"Passed: {right} of {result.Questions.Count} right. {_item.Number} is complete.");
        else Changed(result.Passed
            ? _item.Kind == ItemKind.Assess && Progress.StateOf(_item) != ItemState.Complete
                ? $"Passed: {right} of {result.Questions.Count} right. The check completes when these are complete too: " +
                  string.Join(", ", _item.Covers.Select(_content.Package.FindItem).OfType<LearningItem>().Where(c => Progress.StateOf(c) != ItemState.Complete).Select(c => c.Number)) + "."
                : $"Passed: {right} of {result.Questions.Count} right."
            : $"Not passed yet: {right} of {result.Questions.Count} right. Change the answers marked ✗ and press Check my answers again.");
    }

    private void MarkRead()
    {
        if (_item is null || !Progress.MarkRead(_item, DateTimeOffset.Now)) return;
        Completed($"{_item.Number} is complete.");
    }

    private void Changed(string what)
    {
        Save();
        _path.Refresh(Progress);
        ShowItemStatus();
        Status?.Invoke(what + (_session is null ? " (Not signed in: this is not kept.)" : ""));
    }

    private void Completed(string what)
    {
        var module = _item!.Module;
        var p = Progress.Of(module);
        var next = Progress.NextAfter(_content.Package, _item);
        Changed(what + (p.State == ModuleState.Complete ? $" That completes module {module.Number}." : $" Module {module.Number}: {p.Complete} of {p.Total} complete.")
                     + (next is null ? " Every item in the path is complete." : $" Next: {next}."));
    }

    // ── Widgets in a Learn page ──────────────────────────────────────────────

    private FrameworkElement RenderMarkdown(string markdown) =>
        new MarkdownView { Markdown = markdown, WidgetFactory = Widget, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, MaxHeight = 600, Margin = new Thickness(-18, -12, 0, -24) };

    private BuiltTask? Built(string id)
    {
        if (_built.TryGetValue(id, out var cached)) return cached;
        BuiltTask? built = null;
        if (_content.Find(id) is { } task)
        {
            try { built = StatsTaskRunner.Build(task); }
            catch (ContentException) { }
        }
        _built[id] = built;
        return built;
    }

    /// <summary>Draws [[figure: task n]], [[data: task]], [[results: task]] and [[code: task]] (docs/CONTENT-FORMAT.md).</summary>
    private Block? Widget(WidgetTag tag)
    {
        var (id, number) = StatsContent.WidgetArgument(tag.Argument);
        if (Built(id) is not { } built) return null;
        switch (tag.Name.ToLowerInvariant())
        {
            case "figure":
                if (built.Analysis is not IHasFigures drawable) return null;
                var figures = drawable.Figures(null);
                if (number < 1 || number > figures.Count) return null;
                var panel = new FigurePanel { Width = 760, HorizontalAlignment = HorizontalAlignment.Left };
                panel.Show(figures[number - 1]);
                return MarkdownView.Wrap(panel);
            case "data":
                var columns = TaskWorkspace.DataColumns(built);
                if (columns.Count == 0) return null;
                var text = new TextBox
                {
                    Text = TaskWorkspace.DataText(columns), IsReadOnly = true, FontFamily = new FontFamily("Consolas"), MaxHeight = 220,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 240,
                    ToolTip = "The data, one row per observation. Select and copy it to use it elsewhere.",
                };
                AutomationProperties.SetName(text, "Data: " + built.Task.Title);
                return MarkdownView.Wrap(text);
            case "results":
                return MarkdownView.Wrap(TaskWorkspace.ResultsGrid(built));
            case "code":
                return MarkdownView.Wrap(TaskWorkspace.CodeTabs(built));
            default:
                return null;
        }
    }

    // ── Small pieces ─────────────────────────────────────────────────────────

    private static string Hours(int minutes) => minutes < 90 ? $"{minutes} minutes" : $"{minutes / 60.0:0.#} hours";

    private static TabItem Tab(string header, object content, string tip) => new() { Header = header, Content = content, ToolTip = tip };

    private static DataTemplate TreeTemplate()
    {
        var template = new HierarchicalDataTemplate(typeof(PathNode)) { ItemsSource = new Binding(nameof(PathNode.Children)) };
        var panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.MarginProperty, new Thickness(0, 2, 0, 2));
        var top = new FrameworkElementFactory(typeof(StackPanel));
        top.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        var mark = new FrameworkElementFactory(typeof(TextBlock));
        mark.SetBinding(TextBlock.TextProperty, new Binding(nameof(PathNode.Mark)));
        mark.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(PathNode.MarkBrush)));
        mark.SetValue(TextBlock.WidthProperty, 18.0);
        var header = new FrameworkElementFactory(typeof(TextBlock));
        header.SetBinding(TextBlock.TextProperty, new Binding(nameof(PathNode.Header)));
        header.SetBinding(TextBlock.FontWeightProperty, new Binding(nameof(PathNode.Weight)));
        header.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        header.SetValue(TextBlock.MaxWidthProperty, 250.0);
        top.AppendChild(mark);
        top.AppendChild(header);
        var detail = new FrameworkElementFactory(typeof(TextBlock));
        detail.SetBinding(TextBlock.TextProperty, new Binding(nameof(PathNode.Detail)));
        detail.SetValue(TextBlock.FontSizeProperty, 11.0);
        detail.SetValue(TextBlock.ForegroundProperty, LearningTheme.Brush("MutedTextBrush"));
        detail.SetValue(TextBlock.MarginProperty, new Thickness(18, 0, 0, 0));
        detail.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        detail.SetValue(TextBlock.MaxWidthProperty, 250.0);
        panel.AppendChild(top);
        panel.AppendChild(detail);
        template.VisualTree = panel;
        return template;
    }

    private sealed class RelayKey(Action run) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => run();
    }
}
