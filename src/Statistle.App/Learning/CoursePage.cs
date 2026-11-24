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
using Learning.Core.Notes;
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
    private readonly Button _handIn = new() { Content = "Hand in a progress rep_ort…", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(6, 0, 0, 0) };
    private readonly ReportsView _reports = new();
    private readonly TabControl _tabs = new() { Margin = new Thickness(0, 8, 0, 0) };
    private readonly TabItem _learnTab, _exampleTab, _exerciseTab, _referenceTab;
    private readonly MarkdownView _learn = new();
    private readonly Button _read = new() { Content = "I have read this", Padding = new Thickness(12, 4, 12, 4), HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBlock _learnFooter = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
    private readonly TaskWorkspace _example;
    // The Example tab (Learning Core plan 3.22): the item's written examples to try, each a page with a figure, then its own worked
    // example (the task); with more than one, the tabs Example 1, Example 2 … above them.
    private readonly TabControl _exampleParts = new() { Margin = new Thickness(0, 6, 0, 4), Padding = new Thickness(0), BorderThickness = new Thickness(0, 1, 0, 0), Visibility = Visibility.Collapsed };
    private readonly Grid _exampleHost = new();
    private readonly List<MarkdownView> _examplePages = [];
    private List<string> _exampleTitles = [];
    private bool _exampleHasTask, _showingExamples;
    private readonly TaskWorkspace _exercise;
    private readonly QuizView _quiz = new();
    private readonly MarkdownView _exerciseNone = new();
    private readonly ContentControl _exerciseHost = new() { Focusable = false };
    private readonly MarkdownView _reference = new();
    private readonly FrameworkElement _itemPage;

    // The window's room (plan W.3, W.4): PlusBook Learning's block under the tree, folding down; help on the right, hiding to the right.
    private readonly Expander _pathInfo = new() { ExpandDirection = ExpandDirection.Down, IsExpanded = true, Margin = new Thickness(8, 4, 12, 6) };
    private readonly TextBlock _pathTitle = new() { Text = "PlusBook Learning", FontSize = 15, FontWeight = FontWeights.SemiBold };
    private readonly TextBlock _totals = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _time = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
    private readonly Button _foldAll = new() { Content = "Collapse Menu", Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Border _helpPanel = new() { Width = 320 };
    private readonly Border _helpStrip = new() { Visibility = Visibility.Collapsed };
    private readonly Button _hideHelp = new() { Content = "Hide help ▸" };
    private readonly Button _openHelp = new();
    private readonly MarkdownView _help = new(); // a tab stop, so the keyboard can reach and scroll help (UX review 2026-11-23)
    private bool _showingPreferences;

    // My notes, the Glossary and Course Links (Q17, Ron 2026-11-23: "Bring the Statistics up to the standard of the other PlusBooks").
    private readonly NotesPanel _itemNotes = new();
    private readonly NotesPanel _allNotes = new() { AllNotes = true };
    private readonly GlossaryPanel _glossary = new();
    private readonly TabItem _notesTab;
    private readonly TextBlock _notesSignIn;
    private Border _notesPage = new(), _glossaryPage = new();
    private readonly Button _courseText = new() { Content = "📘 Course website", Margin = new Thickness(0, 0, 6, 4), Visibility = Visibility.Collapsed };
    private readonly Button _courseLms = new() { Content = "🎓 Course in the LMS", Margin = new Thickness(0, 0, 6, 4), Visibility = Visibility.Collapsed };
    private readonly Button _courseLinks = new() { Content = "Course Links…", Margin = new Thickness(0, 0, 0, 4) };

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
        _reference.WidgetFactory = _exerciseNone.WidgetFactory = Widget;
        // The Learn page's [[see-example: N]] is a button to Example N (Learning Core plan 3.22: the Try it goes on the Example tab).
        _learn.WidgetFactory = w => w.Name.Equals("see-example", StringComparison.OrdinalIgnoreCase) ? SeeExampleButton(w.Argument) : Widget(w);
        _quiz.WidgetFactory = WidgetElement;

        // ── The tree ──
        _tree.ItemTemplate = TreeTemplate();
        _tree.ItemsSource = _path.Roots;
        _tree.BorderThickness = new Thickness(0);
        _tree.Background = Brushes.Transparent;
        ScrollViewer.SetHorizontalScrollBarVisibility(_tree, ScrollBarVisibility.Disabled);
        _tree.SelectedItemChanged += (_, e) => { if (e.NewValue is PathNode node) Chosen(node, e.OldValue as PathNode); };
        AutomationProperties.SetName(_tree, "PlusBook Learning: PlusBook Tools, then each module and its items");
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

        // PlusBook Learning (plan W.3, W.5; Ron, 2026-11-23): the tree first, in lavender; the title, totals, time, key and buttons
        // under it in a light green block that folds down to its title, so the tree has the most room.
        var left = new Grid { Background = LearningTheme.Brush("FilesBrush") };
        left.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        left.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _notSignedIn.Margin = new Thickness(10, 10, 10, 4);
        _tree.Padding = new Thickness(6, 8, 4, 8);
        Grid.SetRow(_tree, 1);
        left.Children.Add(_notSignedIn);
        left.Children.Add(_tree);
        var blockBack = new Border { Background = LearningTheme.Brush("LearningBrush"), BorderBrush = LearningTheme.Brush("LearningLineBrush"), BorderThickness = new Thickness(0, 1, 0, 0) };
        Grid.SetRow(blockBack, 2);
        Grid.SetRow(_pathInfo, 2);
        left.Children.Add(blockBack);
        left.Children.Add(_pathInfo);
        _pathTitle.ToolTip = "Opens or folds the totals, your time, the key and the button under the path; folded, the path above has more room";
        _pathInfo.Header = _pathTitle;
        AutomationProperties.SetName(_pathInfo, "PlusBook Learning: totals, key and buttons");
        AutomationProperties.SetHelpText(_pathInfo, "Your totals, your time, the key to the marks and Collapse Menu. Space opens or folds them");
        var sectionButton = new Style(typeof(Button), (Style)Application.Current.FindResource("SectionButton"));
        sectionButton.Setters.Add(new Setter(BorderBrushProperty, LearningTheme.Brush("LearningLineBrush")));
        sectionButton.Setters.Add(new Setter(PaddingProperty, new Thickness(10, 3, 10, 3)));
        _foldAll.Style = sectionButton;
        _foldAll.ToolTip = "Folds PlusBook Tools and every module, so the path shows only their names";
        AutomationProperties.SetHelpText(_foldAll, "Open a module again with the arrow beside it, or the → key; each opens and folds on its own");
        _foldAll.Click += (_, _) => FoldAll();
        _totals.ToolTip = "Items and modules complete in the whole course. My progress (in PlusBook Tools) has the detail";
        _time.ToolTip = "Time counts while you are using Statistics+ (a key or the mouse in the last two minutes), not while it sits idle";
        var block = new StackPanel { Margin = new Thickness(4, 4, 0, 2) };
        block.Children.Add(_totals);
        block.Children.Add(_time);
        block.Children.Add(new TextBlock
        {
            Text = "+ PlusBook Tools · ▤ my progress · ✉ hand in a report · ✎ my notes · Aa glossary · ∑ Workbench · 📖 a part of the book · ★ mastered · ✓ complete · ↺ review · ◐ started · ○ not started · – in development",
            TextWrapping = TextWrapping.Wrap, FontSize = 12, Foreground = LearningTheme.Brush("MutedTextBrush"), Margin = new Thickness(0, 2, 0, 0),
        });
        block.Children.Add(_foldAll);
        // Course Links (Learning Core plan 3.10; Q17): the course website and the course in the LMS, or a way to add them.
        var links = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
        foreach (var b in new[] { _courseText, _courseLms, _courseLinks }) { b.Style = sectionButton; ToolTipService.SetShowOnDisabled(b, true); links.Children.Add(b); }
        _courseText.Click += (_, _) => OpenLink(_session?.Data.Course?.TextUrl, "Course website");
        _courseLms.Click += (_, _) => OpenLink(_session?.Data.Course?.LmsUrl, "Course in the LMS");
        _courseLinks.Click += (_, _) => EditCourseLinks();
        block.Children.Add(links);
        _pathInfo.Content = block;
        _pathInfo.Expanded += PathInfo_Toggled;
        _pathInfo.Collapsed += PathInfo_Toggled;
        _path.Refreshed += () =>
        {
            _totals.Text = _path.TotalsText;
            _time.Text = _path.TimeText;
        };

        // ── The item page ──
        foreach (var b in new[] { _previous, _next, _read, _handIn }) ToolTipService.SetShowOnDisabled(b, true);
        _previous.Click += (_, _) => { if (_item is not null && _path.PreviousInOrder(_item) is { } p) _path.Select(p); };
        _next.Click += (_, _) => { if (_item is not null && _path.NextInOrder(_item) is { } n) _path.Select(n); };
        _read.Click += (_, _) => MarkRead();
        _handIn.Click += (_, _) => ShowReports(_item?.Id);

        // Hand in a report (plan 14.1): the page shows each exercise that passed with its results and figure.
        _reports.Package = content.Package;
        _reports.Session = () => _session;
        _reports.Save = () => { Save(); _path.Refresh(Progress); };
        _reports.Status = text => Status?.Invoke(text);
        _reports.Picture = evidence =>
        {
            try { return ExerciseEvidence.Png(_content, evidence); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or System.IO.IOException or NotSupportedException) { return null; }
        };

        var learnPanel = new DockPanel();
        var footer = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
        DockPanel.SetDock(_read, Dock.Left);
        footer.Children.Add(_read);
        footer.Children.Add(_learnFooter);
        DockPanel.SetDock(footer, Dock.Bottom);
        learnPanel.Children.Add(footer);
        learnPanel.Children.Add(_learn);

        // No access keys on the tabs: Ctrl+1 to Ctrl+4 choose them, and Alt+L must open the Learner menu (UX sweep, plan 15.1).
        _learnTab = Tab("Learn", learnPanel, "The lesson: the idea, the formulas, a worked case, and the Python and R (Ctrl+1)");
        AutomationProperties.SetName(_exampleParts, "This item's examples: choose one");
        _exampleParts.SelectionChanged += ExampleParts_SelectionChanged;
        _exampleHost.Children.Add(_example);
        var examplePanel = new DockPanel();
        DockPanel.SetDock(_exampleParts, Dock.Top);
        examplePanel.Children.Add(_exampleParts);
        examplePanel.Children.Add(_exampleHost);
        _exampleTab = Tab("Example", examplePanel, "Examples to try, each with a figure, and a worked example: the data, every result, the figures and the code (Ctrl+2)");
        _exerciseTab = Tab("Exercise", _exerciseHost, "Your turn: work out the results and check them, or answer the questions (Ctrl+3)");
        _referenceTab = Tab("Reference", _reference, "The formulas, the Python and R functions, and the sources, to look back at (Ctrl+4)");
        _notesSignIn = NotSignedInNotes();
        var notesHost = new Grid();
        notesHost.Children.Add(_itemNotes);
        notesHost.Children.Add(_notesSignIn);
        _notesTab = Tab("My notes", notesHost, "Your own notes on this item: write one, read them, change or delete them. Only you see them (Ctrl+5)");
        _tabs.Items.Add(_learnTab);
        _tabs.Items.Add(_exampleTab);
        _tabs.Items.Add(_exerciseTab);
        _tabs.Items.Add(_referenceTab);
        _tabs.Items.Add(_notesTab);
        SetUpNotesAndGlossary();

        var header = new DockPanel();
        var nav = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
        nav.Children.Add(_previous);
        nav.Children.Add(_next);
        nav.Children.Add(_handIn);
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
        var splitter = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = LearningTheme.Brush("LineBrush"), ToolTip = "Drag to make PlusBook Learning wider or narrower" };
        Grid.SetColumn(splitter, 1);
        var rightSide = new DockPanel();
        Grid.SetColumn(rightSide, 2);
        BuildHelp(rightSide);
        rightSide.Children.Add(_right);
        grid.Children.Add(left);
        grid.Children.Add(splitter);
        grid.Children.Add(rightSide);
        Content = grid;

        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _learnTab), Key.D1, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _exampleTab), Key.D2, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _exerciseTab), Key.D3, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _referenceTab), Key.D4, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayKey(() => _tabs.SelectedItem = _notesTab), Key.D5, ModifierKeys.Control));

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

    /// <summary>The questions on the Exercise tab (for tests).</summary>
    public QuizView QuizView => _quiz;

    /// <summary>The item on the right, if an item is shown.</summary>
    public LearningItem? CurrentItem => _item;

    /// <summary>The learner whose progress is shown and kept (null: nobody signed in).</summary>
    public LearnerFile? Session => _session;

    /// <summary>
    /// Shows the Course for a learner (or for nobody), on My progress. The answers typed but not yet checked are kept in
    /// memory while Statistics+ is open: signing out (or the idle lock) keeps them, so the same learner finds them again;
    /// they are forgotten when a different learner signs in (UX sweep, plan 15.1).
    /// </summary>
    public void SignIn(LearnerFile? session)
    {
        _quiz.Remember();
        KeepNoteDraft();           // the learner leaving: a note half written is kept in their file, not lost
        _itemNotes.Forget();       // nothing of their notes stays on screen
        _allNotes.Forget();
        if (session is not null)
        {
            var who = session.Header.ProfileId;
            if (!string.Equals(who, _lastLearner, StringComparison.Ordinal)) ForgetAnswers();
            _lastLearner = who;
        }
        _session = session;
        ShowCourseLinks();
        _notSignedIn.Visibility = session is null ? Visibility.Visible : Visibility.Collapsed;
        ShowPreferences();
        _path.Refresh(Progress);
        LearningPath.Choose(_path.Dashboard);
        ShowDashboard();
    }

    // ── The window's room (plan W.1-W.4) ────────────────────────────────────

    private const string PathInfoPreference = "pathInfo";
    private const string HelpPreference = "helpPanel";

    /// <summary>The block under the tree and help, as this learner left them (both open the first time, and for nobody).</summary>
    private void ShowPreferences()
    {
        _showingPreferences = true;
        _pathInfo.IsExpanded = _session?.Data.Preferences.GetValueOrDefault(PathInfoPreference) != "folded";
        SetHelp(_session?.Data.Preferences.GetValueOrDefault(HelpPreference) != "folded");
        _showingPreferences = false;
    }

    /// <summary>The learner folded or opened PlusBook Learning's block: kept that way for them (saved with their progress).</summary>
    private void PathInfo_Toggled(object sender, RoutedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, _pathInfo) || _showingPreferences) return;
        if (_session is not null)
        {
            _session.Data.Preferences[PathInfoPreference] = _pathInfo.IsExpanded ? "open" : "folded";
            Save();
        }
        Status?.Invoke(_pathInfo.IsExpanded
            ? "PlusBook Learning's totals, time, key and Collapse Menu are open under the path."
            : "PlusBook Learning's block is folded to its title, so the path has more room. Choose PlusBook Learning to open it again.");
    }

    /// <summary>PlusBook Learning's block under the tree (for the tests).</summary>
    public Expander PathInfo => _pathInfo;

    /// <summary>The block's title (for the tests).</summary>
    public TextBlock PathTitle => _pathTitle;

    /// <summary>Collapse Menu (for the tests).</summary>
    public Button FoldAllButton => _foldAll;

    /// <summary>The tree of the path (for the tests).</summary>
    public TreeView PathTree => _tree;

    /// <summary>Help on the right, its Hide help bar, and the strip that opens it again (for the tests).</summary>
    public Border HelpPanel => _helpPanel;

    public Button HideHelpButton => _hideHelp;

    public Border HelpStrip => _helpStrip;

    public Button OpenHelpButton => _openHelp;

    /// <summary>What help says now (for the tests).</summary>
    public string HelpMarkdownShown => _help.Markdown;

    /// <summary>Whether help shows on the right.</summary>
    public bool IsHelpOpen => _helpPanel.Visibility == Visibility.Visible;

    /// <summary>Collapse Menu (plan W.1): PlusBook Tools and every module fold; the page on screen stays.</summary>
    public void FoldAll()
    {
        _path.FoldAll();
        Status?.Invoke("The path shows PlusBook Tools and the modules only. Open one with the arrow beside it (or the → key); each opens and folds on its own.");
    }

    private void SetHelp(bool open)
    {
        _helpPanel.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
        _helpStrip.Visibility = open ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// F1 in the Course, Hide help ▸ and the Help strip (plan W.4): help hides to the right and opens again; the learner's choice is
    /// kept for them. From a button the focus follows to the one that now shows; F1 leaves it where it was.
    /// </summary>
    public void ToggleHelp(bool fromButton = false)
    {
        var open = !IsHelpOpen;
        var focusInHelp = _helpPanel.IsKeyboardFocusWithin;
        SetHelp(open);
        if (_session is not null)
        {
            _session.Data.Preferences[HelpPreference] = open ? "open" : "folded";
            Save();
        }
        if (fromButton) { if (open) _hideHelp.Focus(); else _openHelp.Focus(); }
        else if (!open && focusInHelp) _openHelp.Focus();
        Status?.Invoke((open ? "Help is open on the right. Hide help (the light blue bar at its top, or F1) hides it."
                             : "Help is hidden to the right, so the page has more room. The light blue Help strip at the right edge (or F1) opens it again.")
                       + (_session is null ? " (Not signed in: this choice is not kept.)" : ""));
    }

    /// <summary>Help on the right: the light blue Hide help bar across its top, and the strip left at the right edge while it is hidden.</summary>
    private void BuildHelp(DockPanel host)
    {
        var barStyle = (Style)Application.Current.FindResource("HelpBarButton");
        _hideHelp.Style = barStyle;
        _hideHelp.HorizontalAlignment = HorizontalAlignment.Stretch;
        _hideHelp.HorizontalContentAlignment = HorizontalAlignment.Center;
        _hideHelp.Padding = new Thickness(8, 7, 8, 7);
        _hideHelp.BorderThickness = new Thickness(0, 0, 0, 1);
        _hideHelp.ToolTip = "Hides help to the right, so the page has more room; the light blue Help strip at the right edge (or F1) opens it again";
        AutomationProperties.SetName(_hideHelp, "Hide help");
        _hideHelp.Click += (_, _) => ToggleHelp(fromButton: true);
        _help.Background = Brushes.Transparent;
        AutomationProperties.SetName(_help, "Help for this screen");
        var panel = new DockPanel();
        DockPanel.SetDock(_hideHelp, Dock.Top);
        panel.Children.Add(_hideHelp);
        panel.Children.Add(_help);
        _helpPanel.Child = panel;
        _helpPanel.Background = LearningTheme.Brush("HelpPanelBrush");
        _helpPanel.BorderBrush = LearningTheme.Brush("HelpBarLineBrush");
        _helpPanel.BorderThickness = new Thickness(1, 0, 0, 0);

        _openHelp.Style = barStyle;
        _openHelp.VerticalAlignment = VerticalAlignment.Stretch;
        _openHelp.VerticalContentAlignment = VerticalAlignment.Top;
        _openHelp.Padding = new Thickness(6, 12, 6, 12);
        _openHelp.BorderThickness = new Thickness(0);
        _openHelp.ToolTip = "Opens help for this screen again (F1)";
        AutomationProperties.SetName(_openHelp, "Help (F1): open help"); // the words on it first, for voice control (UX review 2026-11-23)
        var words = new StackPanel();
        words.Children.Add(new TextBlock { Text = "◂", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 6) });
        words.Children.Add(new TextBlock { Text = "Help (F1)", LayoutTransform = new RotateTransform(90) });
        _openHelp.Content = words;
        _openHelp.Click += (_, _) => ToggleHelp(fromButton: true);
        _helpStrip.Child = _openHelp;
        _helpStrip.Background = LearningTheme.Brush("HelpBarBrush");
        _helpStrip.BorderBrush = LearningTheme.Brush("HelpBarLineBrush");
        _helpStrip.BorderThickness = new Thickness(1, 0, 0, 0);

        DockPanel.SetDock(_helpStrip, Dock.Right);
        DockPanel.SetDock(_helpPanel, Dock.Right);
        host.Children.Add(_helpStrip);
        host.Children.Add(_helpPanel);
    }

    /// <summary>Help for the screen on the right: what it is for, then how to use the book.</summary>
    private void ScreenHelp(string title, string text) =>
        _help.Markdown = $"## This screen: {title}\n\n{text}\n\n{HelpMarkdown}";

    // ── My notes, the Glossary and Course Links (Q17; from PythonProgramming+) ──

    private void SetUpNotesAndGlossary()
    {
        _path.NoteCounts = () => _session?.Data.Notes.CountByItem() ?? new Dictionary<string, int>();
        // The theme on the panels themselves, so they draw even while their page is not on screen.
        foreach (var panel in new FrameworkElement[] { _itemNotes, _allNotes, _glossary }) LearningTheme.Apply(panel);
        _notesPage = new Border { Child = _allNotes, Padding = new Thickness(16, 12, 16, 8), Tag = "notes" };
        _glossaryPage = new Border { Child = _glossary, Padding = new Thickness(16, 12, 16, 8), Tag = "glossary" };
        foreach (var panel in new[] { _itemNotes, _allNotes })
        {
            panel.Session = () => _session;
            panel.FindItem = _content.Package.FindItem;
            panel.Changed = NotesChanged;
            panel.Status = text => Status?.Invoke(text);
            panel.GoTo = GoToNote;
            panel.PageText = (item, tab) => tab == NoteTabs.Reference ? ReferenceMarkdown(item) : global::Learning.Core.Content.ExamplePages.Split(item.Learn).Learn;
        }
        foreach (var (view, tab) in NotedPages)
        {
            view.EnableNotes();
            view.CanNote = () => _session is not null;
            view.NoteRequested += i => RequestNote(view, tab, i);
            view.NoteMarkClicked += i => ShowNotesOn(view, tab, i);
            view.Glossary = _content.Package.Glossary;
            view.TermClicked += entry => ShowGlossaryPage(entry);
        }
        _glossary.Load(_content.Package);
        _glossary.Back = BackFromGlossary;
        _glossary.Open = (item, _) => { _glossaryFrom = null; _path.Select(item); };
    }

    /// <summary>The pages a learner can note, with the tab each is on.</summary>
    private IEnumerable<(MarkdownView View, string Tab)> NotedPages => [(_learn, NoteTabs.Learn), (_reference, NoteTabs.Reference)];

    /// <summary>The item's My notes tab and the My notes page (for the tests).</summary>
    public NotesPanel ItemNotes => _itemNotes;

    public NotesPanel AllNotesPage => _allNotes;

    /// <summary>The Glossary page (for the tests).</summary>
    public GlossaryPanel GlossaryPage => _glossary;

    /// <summary>The Learn tab's page (for the tests).</summary>
    public MarkdownView LearnView => _learn;

    /// <summary>The My notes tab (for the tests).</summary>
    public TabItem NotesTab => _notesTab;

    /// <summary>The Course Links button (for the tests).</summary>
    public Button CourseLinksButton => _courseLinks;

    /// <summary>A note was added, changed or deleted: saved in the learner's file, and the path, marks and lists drawn again.</summary>
    private void NotesChanged()
    {
        if (_session is null) return;
        _session.Data.NotesChanged(DateTimeOffset.Now);
        Save();
        _path.Refresh(Progress);
        RefreshNotes();
        if (_right.Content is FrameworkElement { Tag: "notes" }) _allNotes.ShowAll();
    }

    /// <summary>The item's notes: a ✎ after each paragraph that has one, the list on the My notes tab, and the count on the tab.</summary>
    private void RefreshNotes()
    {
        if (_item is not { } item) return;
        // Both stay in the tab (the panel keeps the theme's resources); only one shows.
        _itemNotes.Visibility = _session is null ? Visibility.Collapsed : Visibility.Visible;
        _notesSignIn.Visibility = _session is null ? Visibility.Visible : Visibility.Collapsed;
        if (_session is null)
        {
            foreach (var (view, _) in NotedPages) view.MarkNotes([]);
            _notesTab.Header = "My notes";
            return;
        }
        var book = _session.Data.Notes;
        var placed = new List<NoteLocation>();
        foreach (var (view, tab) in NotedPages)
        {
            var here = book.Place(item.Id, tab, view.BlockSources).Where(p => p.Note.Anchor.Block is not null).ToList();
            view.MarkNotes(here.Where(p => p.BlockIndex is not null).Select(p => (p.BlockIndex!.Value, NotesPanel.Short(p.Note.Text, 200))));
            placed.AddRange(here);
        }
        _itemNotes.ShowItem(item, placed.GroupBy(p => p.Note.Id).ToDictionary(g => g.Key, g => g.First()));
        var count = book.For(item.Id).Count;
        _notesTab.Header = count == 0 ? "My notes" : $"My notes ({count})";
        AutomationProperties.SetName(_notesTab, count == 0 ? "My notes" : $"My notes, {count} on this item");
    }

    /// <summary>Notes belong to a learner: without one, the tab says how to keep them.</summary>
    private TextBlock NotSignedInNotes()
    {
        var text = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(14, 12, 14, 8), MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        var signIn = new Hyperlink(new Run("Sign in")) { ToolTip = "Opens the Who is learning? screen" };
        signIn.Click += (_, _) => SignInRequested?.Invoke();
        text.Inlines.Add(new Run("Your notes are kept in your own progress file, so sign in first: "));
        text.Inlines.Add(signIn);
        text.Inlines.Add(new Run(". Then write a note here, or right-click a paragraph of the lesson and choose Add a note to this paragraph."));
        return text;
    }

    /// <summary>Add a note to this paragraph (or page), from the lesson's right-click menu: a small window over the lesson.</summary>
    private void RequestNote(MarkdownView view, string tab, int? block)
    {
        if (_item is not { } item) return;
        if (_session is null)
        {
            Status?.Invoke("Sign in first (Learner ▸ Sign in): your notes are kept in your own progress file.");
            return;
        }
        var sources = view.BlockSources;
        var anchor = block is int i && i < sources.Count ? NoteAnchor.OnBlock(item.Id, tab, sources[i]) : NoteAnchor.OnItem(item.Id, tab);
        var page = $"{item.Number}'s {(tab == NoteTabs.Reference ? "Reference" : "Learn")} page";
        if (Window.GetWindow(this) is not { } owner || NoteWindow.Ask(owner, anchor, page) is not { } text) { Status?.Invoke("No note was added."); return; }
        if (_session.Data.Notes.Add(anchor, text, DateTimeOffset.Now) is null) return;
        NotesChanged();
        Status?.Invoke(anchor.Block is null
            ? $"Note added to {page}. It is on the My notes tab with your other notes on {item.Number}."
            : "Note added: a ✎ after the paragraph shows it (point at it to read it, click it to open My notes).");
    }

    /// <summary>Adds a note on a paragraph of the Learn page, as the right-click menu does (for the tests).</summary>
    public void AddNoteOnLearn(int block, string text)
    {
        if (_item is not { } item || _session is null) return;
        _session.Data.Notes.Add(NoteAnchor.OnBlock(item.Id, NoteTabs.Learn, _learn.BlockSources[block]), text, DateTimeOffset.Now);
        NotesChanged();
    }

    /// <summary>The ✎ after a paragraph was clicked: the My notes tab, with that paragraph's newest note marked.</summary>
    private void ShowNotesOn(MarkdownView view, string tab, int block)
    {
        if (_item is not { } item || _session is null) return;
        var note = _session.Data.Notes.Place(item.Id, tab, view.BlockSources).Where(p => p.BlockIndex == block).Select(p => p.Note)
            .OrderByDescending(n => n.Created).FirstOrDefault();
        _tabs.SelectedItem = _notesTab;
        if (note is not null) Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () => _itemNotes.Highlight(note.Id));
        Status?.Invoke($"Your notes on {item.Number}: the one on that paragraph is marked ▶. Its Show button takes you back to the paragraph.");
    }

    /// <summary>Open a note where it is: its item, its tab, and the paragraph it is on.</summary>
    private void GoToNote(LearnerNote note)
    {
        if (_content.Package.FindItem(note.Anchor.ItemId) is not { } item) return;
        if (_item != item) _path.Select(item);
        if (_item != item) return;
        var (tab, view) = note.Anchor.Tab switch
        {
            NoteTabs.Learn => (_learnTab, _learn),
            NoteTabs.Reference => (_referenceTab, _reference),
            NoteTabs.Example => (_exampleTab, (MarkdownView?)null),
            NoteTabs.Exercise => (_exerciseTab, null),
            _ => ((TabItem?)null, null),
        };
        var at = view is null || _session is null || note.Anchor.Block is null ? null
            : _session.Data.Notes.Place(item.Id, note.Anchor.Tab!, view.BlockSources).FirstOrDefault(p => p.Note.Id == note.Id);
        _tabs.SelectedItem = at?.BlockIndex is not null ? tab : note.Anchor.Block is null && tab is not null ? tab : _notesTab;
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
        {
            _itemNotes.Highlight(note.Id);
            if (view is not null && at?.BlockIndex is int i && _tabs.SelectedItem == tab) view.ShowBlock(i);
            Status?.Invoke(at?.Place switch
            {
                NotePlace.Found => "The paragraph your note is on is selected, with a ✎ after it. The note is on the My notes tab.",
                NotePlace.Changed => "The lesson has been updated since you wrote this note: the selected paragraph is the one most like the one it was on.",
                NotePlace.Gone => $"The paragraph this note was on is no longer in the lesson; the note stays on {item.Number}'s My notes tab, with the words it was on.",
                _ => _tabs.SelectedItem == _notesTab ? $"Your note is marked ▶ on {item.Number}'s My notes tab." : $"Your note is on {item.Number}; the My notes tab lists it.",
            });
        });
    }

    /// <summary>Leaving an item (or signing out): a note half written, or an edit not saved, is kept rather than lost.</summary>
    public string? KeepNoteDraft()
    {
        if (_session is null) return null;
        var item = _itemNotes.KeepDraft();
        var page = _allNotes.KeepDraft();
        return item is null ? page : page is null ? item : item + " " + page;
    }

    /// <summary>The My notes page in PlusBook Tools: every note, with search, each opening its item.</summary>
    public void ShowNotesPage()
    {
        var kept = KeepNoteDraft();
        _item = null;
        if (_session is null)
        {
            var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8), Tag = "notes" };
            var title = new TextBlock { Text = "My notes", FontSize = 20, FontWeight = FontWeights.SemiBold };
            DockPanel.SetDock(title, Dock.Top);
            page.Children.Add(title);
            page.Children.Add(NotSignedInNotes());
            _right.Content = page;
        }
        else
        {
            _right.Content = _notesPage;
            _allNotes.ShowAll();
        }
        ScreenHelp("My notes",
            "Every note you have written, newest first, with the item each is on. Type in **Find in my notes** to search; **Open** goes to the note in its lesson. " +
            "Write notes on an item's **My notes** tab (Ctrl+5), or right-click a paragraph of a lesson and choose **Add a note to this paragraph**. Only you see them; they are kept in your progress file and never put in a report.");
        Status?.Invoke((kept is null ? "" : kept + " ") + (_session is null ? "My notes: sign in first (Learner ▸ Sign in); your notes are kept in your own progress file." : "My notes: every note you have written, newest first. Type in Find in my notes to search; Open goes to a note in its lesson."));
    }

    private GlossaryEntry? _glossaryTerm;
    /// <summary>The item and tab the learner left for the Glossary, to go back to.</summary>
    private (LearningItem Item, TabItem? Tab)? _glossaryFrom;

    /// <summary>The Glossary page, at one term (from a key term in a lesson) or at the top.</summary>
    public void ShowGlossaryPage(GlossaryEntry? entry)
    {
        if (entry is not null && _item is { } from) _glossaryFrom = (from, _tabs.SelectedItem as TabItem);
        else if (entry is null) _glossaryFrom = null;
        var keptNote = KeepNoteDraft();
        _item = null;
        _right.Content = _glossaryPage;
        _glossary.SetBack(_glossaryFrom is var (fromItem, fromTab) ? $"◂ Back to {fromItem.Number} ({fromTab?.Header as string ?? "Learn"})" : null);
        if (entry is not null) _glossary.ShowTerm(entry);
        _glossaryTerm = entry;
        ScreenHelp("Glossary",
            "The book's key terms, A to Z: what each means, its other names, and the items that use it. Type in **Find a term** to look one up; press an item number under **Used in** to open that item. " +
            "In a lesson, a key term has a dotted underline the first time a page uses it: point at it to read its meaning, click it to come here.");
        Status?.Invoke((keptNote is null ? "" : keptNote + " ") + (entry is not null
            ? $"Glossary: “{entry.Term}” is marked. Press an item number under Used in to open that item" + (_glossaryFrom is null ? "." : ", or ◂ Back to go back where you were.")
            : "Glossary: every key term, A to Z. Type in Find a term to look one up."));
    }

    /// <summary>The term the Glossary page showed last (for the tests).</summary>
    public GlossaryEntry? GlossaryTermShown => _glossaryTerm;

    private void BackFromGlossary()
    {
        if (_glossaryFrom is not var (item, tab)) return;
        _glossaryFrom = null;
        _path.Select(item);
        if (tab is not null && _item == item) _tabs.SelectedItem = tab;
        Status?.Invoke($"Back on {item}.");
    }

    /// <summary>The buttons under PlusBook Learning: the course website and the LMS when set, and a way to set them.</summary>
    private void ShowCourseLinks()
    {
        var links = _session?.Data.Course;
        _courseText.Visibility = links?.TextUrl is null ? Visibility.Collapsed : Visibility.Visible;
        _courseLms.Visibility = links?.LmsUrl is null ? Visibility.Collapsed : Visibility.Visible;
        var named = links?.Name is { Length: > 0 } n ? n : "your course";
        _courseText.ToolTip = $"Opens the website of {named} in your web browser: {links?.TextUrl}";
        _courseLms.ToolTip = $"Opens {named} in the learning management system, in your web browser: {links?.LmsUrl}";
        var any = links is not null && (links.TextUrl is not null || links.LmsUrl is not null);
        _courseLinks.IsEnabled = _session is not null;
        AutomationProperties.SetName(_courseLinks, any ? "Change the course links" : "Add the course links");
        _courseLinks.ToolTip = _session is null ? "Sign in first: the links are kept with your progress"
            : any ? "Change or clear the links to your course in the LMS and to the course website"
            : "Add links to your course in the LMS (Canvas, Brightspace …) and to the course website, from your instructor (optional)";
    }

    /// <summary>Course Links… (also Learner ▸ Course links…).</summary>
    public void EditCourseLinks()
    {
        if (_session is null || Window.GetWindow(this) is not { } owner) return;
        if (CourseLinksWindow.Edit(owner, _session.Data.Course) is not { } links) { Status?.Invoke("Your course links are as they were."); return; }
        _session.Data.Course = links.IsEmpty ? null : links;
        _session.Data.NotesChanged(DateTimeOffset.Now);
        Save();
        ShowCourseLinks();
        Status?.Invoke(links.IsEmpty ? "Your course links are cleared." : "Your course links are kept: the buttons under PlusBook Learning open them.");
    }

    private void OpenLink(string? url, string what)
    {
        if (url is null) return;
        global::Learning.Wpf.WebPages.Open(url, Window.GetWindow(this), "Statistics+ — " + what);
    }

    /// <summary>A part's own page (plan W.1, P.1): what the part is, its modules, and a button to its first module.</summary>
    private void ShowPart(PathNode node)
    {
        var keptNote = KeepNoteDraft();
        _item = null;
        node.IsExpanded = true; // its page lists its modules: they show on the left too (the arrow folds it again)
        var name = node.Header;
        var modules = _content.Package.Modules.Where(m => _content.Package.PartOf(m) == name).ToList();
        var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        var head = new StackPanel();
        head.Children.Add(new TextBlock { Text = name, FontSize = 20, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        head.Children.Add(new TextBlock { Text = node.Detail, Foreground = LearningTheme.Brush("MutedTextBrush"), Margin = new Thickness(0, 2, 0, 6), TextWrapping = TextWrapping.Wrap });
        var buttons = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        var inDevelopment = modules.All(m => m.Status == ModuleStatus.ComingLater);
        foreach (var m in inDevelopment || modules.Count <= 1 ? modules : modules.Take(1))
        {
            var b = new Button
            {
                Content = inDevelopment ? $"Module {m.Number} · {m.Title}" : $"Open Module {m.Number} · {m.Title}", Margin = new Thickness(0, 0, 8, 6),
                Padding = new Thickness(12, 4, 12, 4),
                ToolTip = inDevelopment ? $"Opens Module {m.Number}'s page: what it will teach. It is in development." : "Opens the first module of this part; or choose any module on the left",
            };
            b.Click += (_, _) => { if (_path.NodeOf(m) is { } row) LearningPath.Choose(row); };
            buttons.Children.Add(b);
        }
        head.Children.Add(buttons);
        DockPanel.SetDock(head, Dock.Top);
        page.Children.Add(head);
        var md = new StringBuilder(_content.PartPages.GetValueOrDefault(name) ?? "").AppendLine().AppendLine();
        md.AppendLine("## The modules in this part").AppendLine();
        foreach (var m in modules)
        {
            var p = Progress.Of(m);
            md.AppendLine($"- **Module {m.Number} · {m.Title}** — " + (m.Status == ModuleStatus.ComingLater ? "in development" : $"{p.Complete} of {p.Total} items complete"));
        }
        page.Children.Add(new MarkdownView { Markdown = md.ToString(), Tag = "part" });
        _right.Content = page;
        PartShown = name;
        ScreenHelp("a part of the book",
            "What this part of the book is for, and the modules in it. Press a button under the title to open a module, or choose any module on the left. " +
            "The part opens in the path when its page shows; the arrow beside a part folds or opens it, each part on its own, and **Collapse Menu** folds them all.");
        Status?.Invoke((keptNote is null ? "" : keptNote + " ") + $"{name}: what it is and its modules." + (inDevelopment ? " This part is in development." : ""));
    }

    /// <summary>The part whose page was shown last (for the tests).</summary>
    public string? PartShown { get; private set; }

    // ── Choosing a row in the path ──────────────────────────────────────────

    private bool _restoring;

    private void Chosen(PathNode node, PathNode? old)
    {
        if (_restoring) return;
        // A fold, not a choice: folding a module (or PlusBook Tools) that holds the chosen row makes the tree choose the folded row.
        // The page on screen stays. So is ← from one of PlusBook Tools' rows onto PlusBook Tools (a second ← folds it).
        if ((!node.IsExpanded || node.IsCommon) && old is not null && IsInside(old, node)) return;
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () => RowOf(node)?.BringIntoView());
        if (node.IsCommon)
        {
            // PlusBook Tools has no page of its own: it opens, and My progress (its first row) shows. After the click, so the two
            // rows do not select each other from inside this handler.
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, () =>
            {
                node.IsSelected = false;
                LearningPath.Choose(_path.Dashboard);
                RowOf(_path.Dashboard)?.Focus();
            });
        }
        else if (node.IsTool)
        {
            // The Workbench opens in its own part of the window; the row on screen before stays chosen, so the Course is as it was.
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, () =>
            {
                _restoring = true;
                node.IsSelected = false;
                if (old is not null && !old.IsTool && !old.IsCommon) old.IsSelected = true;
                _restoring = false;
                WorkbenchRequested?.Invoke();
            });
        }
        else Open(node);
    }

    private static bool IsInside(PathNode row, PathNode under)
    {
        for (var p = row.Parent; p is not null; p = p.Parent) if (p == under) return true;
        return false;
    }

    /// <summary>The tree's row for a node at any depth (null while the row that holds it is folded and the row not made yet).</summary>
    public TreeViewItem? RowOf(PathNode? node)
    {
        if (node is null) return null;
        if (node.Parent is null) return _tree.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;
        return RowOf(node.Parent)?.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;
    }

    /// <summary>The Workbench row in PlusBook Tools was chosen: the window shows the Workbench.</summary>
    public event Action? WorkbenchRequested;

    private string? _lastLearner;

    /// <summary>
    /// Forgets every answer typed in this session (another learner signs in, or someone uses the Course without signing in
    /// after a learner), so nobody sees another learner's answers.
    /// </summary>
    public void ForgetAnswers()
    {
        QuizView.ForgetAll();
        TaskWorkspace.ForgetAll();
        _quiz.Reset();
        _lastLearner = null;
        if (_item is not null) { _item = null; LearningPath.Choose(_path.Dashboard); ShowDashboard(); }
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

    /// <summary>
    /// Hand in a report (plan 14.1, 14.2): what a report covers, what it will say, saving the signed PDF, and the result line.
    /// <paramref name="pointId"/> is a module check's ID to start on (null: the first one ready).
    /// </summary>
    public void ShowReports(string? pointId = null)
    {
        _item = null;
        var page = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
        var back = new Button { Content = "◂ Back to My progress", Padding = new Thickness(10, 3, 10, 3), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8), ToolTip = "Shows My progress" };
        back.Click += (_, _) => { LearningPath.Choose(_path.Dashboard); ShowDashboard(); };
        ScreenHelp("Hand in a report",
            "1. Choose a module check, or everything so far. 2. Check what the report will say; **My details for reports** adds your student ID, course and class code to the cover. " +
            "3. Save the PDF report, and hand in that file as it is, the way your instructor asks; do not save it again in another program. " +
            "The **result line** is a one-line summary your instructor can check: copy it if they ask for it. A report made before the check is complete is marked Incomplete.");
        DockPanel.SetDock(back, Dock.Top);
        page.Children.Add(back);
        var title = new TextBlock { Text = "Hand in a progress report", FontSize = 20, FontWeight = FontWeights.SemiBold };
        AutomationProperties.SetHeadingLevel(title, AutomationHeadingLevel.Level1);
        DockPanel.SetDock(title, Dock.Top);
        page.Children.Add(title);
        if (_session is null)
        {
            var text = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
            var signIn = new Hyperlink(new Run("Sign in")) { ToolTip = "Opens the Who is learning? screen" };
            signIn.Click += (_, _) => SignInRequested?.Invoke();
            text.Inlines.Add(new Run("A report shows your own progress, so sign in first: "));
            text.Inlines.Add(signIn);
            text.Inlines.Add(new Run(". Nobody is signed in, so there is no progress to report."));
            page.Children.Add(text);
            _right.Content = page;
            Status?.Invoke("Sign in to hand in a progress report.");
            return;
        }
        var lead = new TextBlock
        {
            Text = "Your instructor may ask for a progress report when you finish a module check. Statistics+ makes it as a signed PDF that shows your scores, " +
                   "your outcomes and the results of each exercise you passed. You hand in the file; your instructor can check it was not changed. " +
                   "(It is not the Workbench's analysis report, which File ▸ Save report as PDF makes.)",
            TextWrapping = TextWrapping.Wrap, Foreground = LearningTheme.Brush("MutedTextBrush"), Margin = new Thickness(0, 2, 0, 4), MaxWidth = 900, HorizontalAlignment = HorizontalAlignment.Left,
        };
        DockPanel.SetDock(lead, Dock.Top);
        page.Children.Add(lead);
        page.Children.Add(_reports);
        _right.Content = page;
        _reports.Show(pointId);
        Status?.Invoke("Hand in a progress report: 1. choose what it covers, 2. save the PDF, 3. hand it in as your instructor asks.");
    }

    /// <summary>The Hand in a report page (for the tests).</summary>
    public ReportsView ReportsPage => _reports;

    /// <summary>How to use the book, under the screen's own help in the help panel (USABILITY-CHECKLIST 1.2; plan W.4).</summary>
    private const string HelpMarkdown = """
        ## How to use this PlusBook

        Statistics+ is a book in three parts: **Part I · Fundamental Statistics for Engineers** follows EGN3443, Probability and Statistics for Engineers, topic by topic; **Part III · Additional Applications of Statistics** shows how statistics is used in six fields; **Part II · Advanced Statistics** is in development.

        ### PlusBook Learning (left)

        - **PlusBook Tools** (+), at the top: **My progress** (▤: every module, the module checks, the outcomes to review, your time), **Hand in a report** (✉), **My notes** (✎), the **Glossary** (Aa) and the **Workbench** (∑), the calculator.
        - Then **Start here**, and the book's **parts** (📖), each with its modules. Choose a part to read what it is. A module opens on a page with what it teaches (its outcomes) and its items. Under it are its **items**, one topic each.
        - The arrow beside PlusBook Tools, a part or a module opens or folds it, each on its own. **Collapse Menu** folds them all.
        - Under the path, in green, are your totals, your time, the key to the marks, Collapse Menu and **Course Links**. Choose **PlusBook Learning** to fold them down and give the path more room.
        - A module's mark: **★** mastered, **✓** complete, **↺** review suggested, **◐** in progress, **○** not started, **–** in development. An item's: **✓** complete, **◐** started, **○** not started.

        ### The four tabs of an item

        - **Learn** (Ctrl+1): the lesson: the idea, the formulas, a worked case, the Python and R. **▶ Try it on the Example tab** opens the example that goes with what you have just read.
        - **Example** (Ctrl+2): examples to try, each with a figure: change the values and watch the results and the figure change. Then the worked example: the data, every result, the figures and the code (**Open in the Workbench** copies it to the calculator). With more than one, choose **Example 1**, **Example 2** … at the top. Nothing here counts toward your progress.
        - **Exercise** (Ctrl+3): your turn: work out the results (by hand, or in Python or R) and press **Check my answers**, or answer the questions.
        - **Reference** (Ctrl+4): the formulas, the Python and R functions, and the sources.
        - **My notes** (Ctrl+5): your own notes on the item.

        ### My notes, key terms and course links

        Write your own notes as you learn: on an item's **My notes** tab, or right-click a paragraph of a lesson and choose **Add a note to this paragraph** (a ✎ after it shows the note). Only you see them; they are kept in your progress file and never put in a report. **My notes** in PlusBook Tools has them all, with a search box.

        A key term has a dotted underline the first time a page uses it: point at it to read its meaning, click it to open the **Glossary**.

        If your class gives you its course website and LMS address, **Course Links…** (under PlusBook Learning, or Learner ▸ Course links…) keeps them, with buttons that open them.

        An item is complete when its exercise passes, when you pass its questions, or (for a reading) when you press **I have read this**. **Previous** and **Next** (top right) go through the items in order.

        A wrong answer says whether it is too high or too low, names a common mistake when it sees one, or says where to look again; never the answer itself. Try as often as you like. **Show a hint** gives one hint at a time.

        ### Your progress

        Sign in on the **Who is learning?** screen (Learner ▸ Sign in). Your progress is kept in a file of your own, locked with your PIN, and saved after every step. **Learner ▸ Back up my progress** saves a copy to a USB drive or a cloud folder. **Learner ▸ Sign out** saves and locks it; so does closing Statistics+, and so does leaving the computer unused for a while.

        ### The Workbench

        The **Workbench** (in PlusBook Tools, the Workbench button at the top, or Ctrl+B) is the calculator, for your own data and assignments: data tables, every analysis, the report, the plots and the Python and R. **Course** (Ctrl+L) comes back here.

        ### Handing in your work

        When you finish a module check, your instructor may ask for a **progress report**. **Hand in a report** (in PlusBook Tools, at the top of the path; also Learner ▸ Hand in a progress report, and the button on My progress and on each module check) makes a signed PDF: your scores, your outcomes, and the results of each exercise you passed with its figure.

        - The first time, **My details for reports** asks for what goes on the cover. Only your name is needed; type the **class code** if your instructor gave you one.
        - A report made before the check is complete says **Incomplete** and lists what is missing.
        - Upload the PDF **exactly as saved**. Renaming it is fine; opening and saving it again, or printing it to PDF, breaks its seal, and your instructor's check says it was changed.
        - The **result line** is a one-line summary to paste where your instructor asks.

        It is not the Workbench's analysis report (File ▸ Save report as PDF), which is not signed.

        **Instructors:** File ▸ Check reports and class codes (for instructors) checks a report or a result line, and makes a class code. Help on the web has a page for instructors.

        ### Room on the screen

        **Hide help ▸**, the light blue bar at the top of this panel, hides help to the right; the light blue **Help (F1)** strip at the right edge, or F1, opens it again. While you are signed in, Statistics+ remembers your choice, and whether PlusBook Learning's block is folded.

        More help: Help ▸ Help on the web, or Help ▸ Send feedback to tell us what is missing.
        """;

    // ── What the right side shows ─────────────────────────────────────────────

    private void Open(PathNode node)
    {
        if (node.IsDashboard) ShowDashboard();
        else if (node.IsReports) ShowReports();
        else if (node.IsNotes) ShowNotesPage();
        else if (node.IsGlossary) ShowGlossaryPage(null);
        else if (node.IsPart) ShowPart(node);
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
        var report = new Button
        {
            Content = "Hand in a progress rep_ort…", Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(8, 8, 0, 8), IsEnabled = _session is not null,
            ToolTip = _session is null ? "Sign in first: a report shows your own progress" : "Makes a signed progress report (PDF) of a module check, or of everything so far, to hand in",
        };
        ToolTipService.SetShowOnDisabled(report, true);
        report.Click += (_, _) => ShowReports();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(go);
        buttons.Children.Add(report);
        DockPanel.SetDock(buttons, Dock.Top);
        page.Children.Add(buttons);
        page.Children.Add(new MarkdownView { Markdown = DashboardPage.Markdown(_content.Package, Progress) });
        _right.Content = page;
        ScreenHelp("My progress",
            "Your progress through the whole course: each module, the module checks, the outcomes to review, and your time. " +
            "**Continue with** opens the next item that is not complete. At a module check, make a report to hand in: **Hand in a report**, the next row in PlusBook Tools. " +
            "Choose a module or an item on the left to work on it." + (_session is null ? " You are not signed in, so nothing here is kept: Learner ▸ Sign in." : ""));
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
            // Plan W.6 (Ron, 2026-11-23): a module still to come says "In development", not "Coming later".
            Text = m.Status == ModuleStatus.ComingLater
                ? "In development: it comes in a later version of Statistics+, and is in the path so you can see where it fits."
                : $"{p.Complete} of {p.Total} items complete" + (m.Minutes > 0 ? $" · about {Hours(m.Minutes)}" : ""),
            TextWrapping = TextWrapping.Wrap,
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
        ScreenHelp("a module",
            "A module's first page: what you will be able to do when you finish it (its outcomes, each with its mark), its items and where you are with each, and what the module is about. " +
            "Press the button under the title to start (or go on), or choose an item on the left. The arrow beside a module folds or opens its items.");
        Status?.Invoke($"Module {m.Number}: what it teaches and its items. Choose an item on the left, or press {(first is null ? "an item" : "the button")} to begin.");
    }

    private string ModuleMarkdown(LearningModule m)
    {
        var md = new StringBuilder();
        if (m.Status == ModuleStatus.ComingLater)
        {
            // A module still to come: what it will teach, plainly, with no marks nobody can earn yet (UX review 2026-11-23).
            md.AppendLine("## What you will be able to do").AppendLine();
            foreach (var o in m.Outcomes) md.AppendLine("- " + o.Text);
            md.AppendLine();
            if (m.Topics.Count > 0)
            {
                md.AppendLine("## What it will cover").AppendLine();
                foreach (var t in m.Topics) md.AppendLine("- " + t);
                md.AppendLine();
            }
            md.AppendLine("To ask for a topic, use Help ▸ Send feedback.");
            return md.ToString();
        }
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
                    : i.Exercise is not null ? (ExampleCount(i) > 1 ? "Lesson, examples and exercise" : "Lesson, example and exercise")
                    : i.Quiz is not null ? (ExampleCount(i) == 0 ? "Lesson and questions" : ExampleCount(i) > 1 ? "Lesson, examples and questions" : "Lesson, example and questions")
                    : ExampleCount(i) == 0 ? "Reading" : "Reading, with examples to try";
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
        var keptNote = _item != item ? KeepNoteDraft() : null;
        // The item's own key terms are marked on its pages; an item that lists none marks every term in the glossary.
        var ownTerms = item.Terms.Select(_content.Package.Glossary.Find).OfType<GlossaryEntry>().ToHashSet();
        foreach (var (view, _) in NotedPages) view.MarkOnly = ownTerms.Count == 0 ? null : ownTerms.Contains;
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

        // The examples first: the Learn page's see-example buttons name them.
        var (learn, written) = ExamplePages.Split(item.Learn);
        var example = _content.Find(item.Example);
        _exampleTitles = [.. written.Select(p => p.Title), .. example is null ? Array.Empty<string>() : [example.Title]];
        _learn.Markdown = learn.Trim().Length > 0 ? learn : $"# {item.Title}\n\n*This page is still to be written.*";

        // Example: the written examples (each a page with a figure to try), then the item's worked example.
        foreach (var old in _examplePages) _exampleHost.Children.Remove(old);
        _examplePages.Clear();
        string Heading(int k) => (_exampleTitles.Count > 1 ? $"Example {k + 1}: " : "Example: ") + _exampleTitles[k];
        for (var k = 0; k < written.Count; k++)
        {
            var page = written[k];
            // The way back to the lesson on the page itself, and the progress note where it is seen (UX review 2026-11-17).
            var view = new MarkdownView
            {
                Visibility = Visibility.Collapsed,
                WidgetFactory = w => w.Name == "back-to-lesson" ? BackToLessonButton() : Widget(w),
            };
            AutomationProperties.SetName(view, Heading(k));
            view.Markdown = $"[[back-to-lesson: ]]\n\n# {Heading(k)}\n\n*An example to try: change the values and watch. Nothing here counts toward your progress.*\n\n{page.Markdown}";
            _exampleHost.Children.Add(view);
            _examplePages.Add(view);
        }
        _exampleHasTask = example is not null;
        if (example is not null)
            _example.ShowExample(example, Heading(_exampleTitles.Count - 1), written.Count == 0 ? null
                : "Also on this tab, to try for yourself: " + string.Join("; ", Enumerable.Range(0, written.Count).Select(k => $"Example {k + 1}, {_exampleTitles[k]}")) + " (choose it above).");
        else _example.ShowMessage(item.Kind == ItemKind.Assess
            ? "A module check has no example of its own: each item it covers has its own."
            : "This item has no example to try. Its Learn tab has the figures and tables that go with it.");
        _showingExamples = true;
        _exampleParts.Items.Clear();
        for (var k = 0; k < _exampleTitles.Count; k++)
            _exampleParts.Items.Add(new TabItem
            {
                Header = new TextBlock { Text = $"Example {k + 1}: " + _exampleTitles[k], MaxWidth = 420, TextTrimming = TextTrimming.CharacterEllipsis },
                ToolTip = k < written.Count ? $"Example {k + 1}: {_exampleTitles[k]}, to try here" : $"Example {k + 1}: {_exampleTitles[k]}, worked: the data, every result, the figures and the Python and R",
            });
        _exampleParts.Visibility = _exampleTitles.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        _showingExamples = false;
        // The worked example first when there is one, as the Learn pages' "The Example" sections describe it (PLC+ opens on its machine).
        ShowExamplePart(_exampleHasTask ? written.Count : 0);
        _exampleTab.Header = _exampleTitles.Count switch { 0 => "Example (none)", 1 => "Example", var n => $"Examples ({n})" };
        _exampleTab.ToolTip = _exampleTitles.Count switch
        {
            0 => "This item has no example (Ctrl+2)",
            1 => example is null ? $"An example to try: {_exampleTitles[0]} (Ctrl+2)" : "A worked example: the data, every result, the figures and the code (Ctrl+2)",
            var n => $"{n} examples: {string.Join("; ", _exampleTitles)} (Ctrl+2)",
        };

        // Exercise: a task, the questions, or what the check covers.
        if (_content.Find(item.Exercise) is { } exercise)
        {
            var saved = _session?.Data.Work.TryGetValue(WorkKey(exercise.Id), out var w) == true ? w.Content : null;
            _exercise.ShowExercise(exercise, saved, Progress.StateOf(item) == ItemState.Complete);
            _exerciseHost.Content = _exercise;
            _exerciseTab.Header = "Exercise";
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
            _quiz.WhereToLook = q => _content.LookAgainAt(item, q) is { } named
                ? $"Look again at {named}."
                : q.Objective is { Length: > 0 } objective
                && item.Module.Items.FirstOrDefault(i => i.Kind != ItemKind.Assess && i.Objectives.Contains(objective, StringComparer.OrdinalIgnoreCase)) is { } teaches
                    ? $"Look again at {teaches}."
                    : null;
            _quiz.ShuffleSeed = _session?.Data.Profile.ProfileId ?? "";
            _quiz.Load(quiz, item.Kind == ItemKind.Assess ? $"Module {item.Module.Number} check: the questions" : "Questions", intro);
            _exerciseHost.Content = new ScrollViewer { Content = _quiz, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            _exerciseTab.Header = "Exercise (questions)";
        }
        else
        {
            _exerciseNone.Markdown = item.Kind == ItemKind.Assess ? CoversMarkdown(item)
                : "## No exercise for this item\n\nThis item is complete when you have read it: press **I have read this** on the Learn tab.";
            _exerciseHost.Content = _exerciseNone;
            _exerciseTab.Header = item.Kind == ItemKind.Assess ? "Exercise" : "Exercise (none)";
        }

        _reference.Markdown = ReferenceMarkdown(item);
        ShowItemStatus();
        RefreshNotes();
        _tabs.SelectedItem = _learnTab;
        _right.Content = _itemPage;
        ScreenHelp(item.Kind == ItemKind.Assess ? "a module check" : "an item",
            item.Kind == ItemKind.Assess
                ? "The module check: questions on the whole module, on the Exercise tab (Ctrl+3). It is complete when the items it covers are complete and you pass its questions (80 %). " +
                  "Then make a report to hand in: **Hand in a progress report** at the top right, or **Hand in a report** in PlusBook Tools."
                : "One topic of the course: **Learn** (Ctrl+1), **Example** (Ctrl+2), **Exercise** (Ctrl+3) and **Reference** (Ctrl+4), and **My notes** (Ctrl+5) for your own notes. Read, try the example, then do the exercise; right-click a paragraph to put a note on it. " +
                  "The line under the title says where you are with it. **Previous** and **Next** (top right) go through the items in order.");
        Status?.Invoke((keptNote is null ? "" : keptNote + " ") + $"{item}: {_learnFooter.Text}");
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
        _handIn.Visibility = item.Kind == ItemKind.Assess ? Visibility.Visible : Visibility.Collapsed;
        _handIn.IsEnabled = _session is not null;
        _handIn.ToolTip = _session is null ? "Sign in first: a report shows your own progress"
            : state == ItemState.Complete ? "Makes the signed progress report (PDF) of this module check, to hand in"
            : "Makes the progress report of this module check. It is not complete yet, so the report says Incomplete and lists what is missing";
        _read.IsEnabled = state != ItemState.Complete;
        _read.ToolTip = state == ItemState.Complete ? "This item is already complete" : "Marks this item complete. An item without an exercise is complete when you have read it.";
        _learnFooter.Text = item.Kind == ItemKind.Assess ? "The questions are on the Exercise tab (Ctrl+3)."
            : item.Exercise is not null ? (ExampleCount(item) > 1 ? "Next: the Examples (Ctrl+2): the ones to try, then the worked one. Then your own Exercise (Ctrl+3)." : "Next: the worked Example (Ctrl+2), then your own Exercise (Ctrl+3).")
            : item.Quiz is not null ? (state == ItemState.Complete ? "Complete. Next ▸ goes on." : "When you have read the page, answer the questions on the Exercise tab (Ctrl+3).")
            : state == ItemState.Complete ? "Complete. Next ▸ goes on."
            : ExampleCount(item) > 0 ? "Try the Example tab (Ctrl+2) too. When you have read the page, press I have read this."
            : "When you have read the page, press I have read this.";
    }

    // ── The Example tab's parts ──────────────────────────────────────────────

    /// <summary>How many examples an item's Example tab has: its written ones and its own worked example.</summary>
    private int ExampleCount(LearningItem item) =>
        ExamplePages.Split(item.Learn).Examples.Count + (_content.Find(item.Example) is null ? 0 : 1);

    /// <summary>The Example tab's examples, by title, in order (for the tests).</summary>
    public IReadOnlyList<string> ExampleTitles => _exampleTitles;

    /// <summary>The Example tab's Example 1, Example 2 … strip (for the tests).</summary>
    public TabControl ExampleParts => _exampleParts;

    /// <summary>The written example showing on the Example tab, if one is (for the tests).</summary>
    public MarkdownView? ExamplePageShowing => _examplePages.FirstOrDefault(v => v.Visibility == Visibility.Visible);

    /// <summary>Shows Example k (from 0) of the item: one of its written pages, or the worked example after them.</summary>
    public void ShowExamplePart(int k)
    {
        for (var i = 0; i < _examplePages.Count; i++) _examplePages[i].Visibility = i == k ? Visibility.Visible : Visibility.Collapsed;
        _example.Visibility = k >= _examplePages.Count ? Visibility.Visible : Visibility.Collapsed;
        if (_exampleParts.SelectedIndex != k && k < _exampleParts.Items.Count)
        {
            _showingExamples = true;
            _exampleParts.SelectedIndex = k;
            _showingExamples = false;
        }
    }

    private void ExampleParts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, _exampleParts)) return;
        e.Handled = true; // not the item's own tabs
        if (_showingExamples || _exampleParts.SelectedIndex < 0) return;
        var k = _exampleParts.SelectedIndex;
        ShowExamplePart(k);
        Status?.Invoke(k < _examplePages.Count
            ? $"Example {k + 1}: {_exampleTitles[k]}. Try it: change the values and watch the figure and the results. Nothing here counts toward your progress."
            : $"Example {k + 1}: {_exampleTitles[k]}, worked: the data, every result, the figures and the Python and R. Open in the Workbench to change it.");
    }

    /// <summary>"◂ Back to the lesson" at the top of a written example: the Learn tab, as the learner left it.</summary>
    private Block BackToLessonButton()
    {
        var back = new Button
        {
            Content = "◂ Back to the lesson", Padding = new Thickness(10, 3, 10, 3), HorizontalAlignment = HorizontalAlignment.Left,
            ToolTip = "Goes back to the Learn tab, where you were reading (Ctrl+1)",
        };
        back.Click += (_, _) =>
        {
            _tabs.SelectedItem = _learnTab;
            Status?.Invoke("Back to the lesson. The examples stay on the Example tab (Ctrl+2).");
        };
        return MarkdownView.Wrap(back);
    }

    /// <summary>The Learn page's <c>[[see-example: N]]</c>: a button that opens Example N on the Example tab.</summary>
    private Block? SeeExampleButton(string argument)
    {
        if (!int.TryParse(argument.Trim(), out var n) || n < 1 || n > _exampleTitles.Count) return null;
        var title = _exampleTitles[n - 1];
        var words = _exampleTitles.Count > 1 ? $"Example {n}: {title}" : title;
        var text = new TextBlock { TextWrapping = TextWrapping.Wrap };
        text.Inlines.Add(new Run("▶ Try it on the Example tab: ") { Foreground = new SolidColorBrush(Color.FromRgb(0x1F, 0x6F, 0x3A)), FontWeight = FontWeights.SemiBold });
        text.Inlines.Add(new Run(words) { FontWeight = FontWeights.SemiBold });
        var button = new Button
        {
            Content = text, HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(12, 6, 12, 6),
            ToolTip = $"Opens {words} on the Example tab (Ctrl+2), with a figure to try. Come back with the Learn tab (Ctrl+1).",
        };
        AutomationProperties.SetName(button, $"Try it on the Example tab: {words}");
        button.Click += (_, _) => OpenExample(n);
        return MarkdownView.Wrap(button);
    }

    /// <summary>What a see-example button does: the Example tab, on Example n (from 1).</summary>
    public void OpenExample(int n)
    {
        if (n < 1 || n > _exampleTitles.Count) return;
        _tabs.SelectedItem = _exampleTab;
        ShowExamplePart(n - 1);
        var words = _exampleTitles.Count > 1 ? $"Example {n}: {_exampleTitles[n - 1]}" : _exampleTitles[n - 1];
        Status?.Invoke($"{words}. Come back to the lesson with the Learn tab (Ctrl+1).");
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

    /// <summary>A widget as a block of a Markdown page (Learn, Reference, an exercise's text).</summary>
    private Block? Widget(WidgetTag tag) => WidgetElement(tag) is { } element ? MarkdownView.Wrap(element) : null;

    /// <summary>
    /// Draws [[figure: task n]], [[data: task]], [[results: task]] and [[code: task]] (docs/CONTENT-FORMAT.md), for a
    /// Markdown page or a quiz question (plan 10.15). Null when it cannot be drawn.
    /// </summary>
    public FrameworkElement? WidgetElement(WidgetTag tag)
    {
        if (tag.Name.Equals("try", StringComparison.OrdinalIgnoreCase))
        {
            // An example to try is built afresh each time, so it starts at the task's values.
            if (TrySpec.Parse(tag.Argument, out _) is not { } spec || _content.Find(spec.TaskId) is not { } task) return null;
            try { return new TryAnalysis(task, spec); }
            catch (ContentException) { return null; }
        }
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
                return panel;
            case "data":
                var columns = TaskWorkspace.DataColumns(built);
                if (columns.Count == 0) return null;
                var table = TaskWorkspace.DataTable(columns, TaskWorkspace.RowLabels(built));
                table.ToolTip = "The data, one row per observation. Open the item's Example or Exercise tab to copy it.";
                AutomationProperties.SetName(table, "Data: " + built.Task.Title);
                return table;
            case "results":
                return TaskWorkspace.ResultsGrid(built);
            case "code":
                return TaskWorkspace.CodeTabs(built);
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
        header.SetBinding(TextBlock.MaxWidthProperty, new Binding(nameof(PathNode.TextWidth)));
        top.AppendChild(mark);
        top.AppendChild(header);
        var detail = new FrameworkElementFactory(typeof(TextBlock));
        detail.SetBinding(TextBlock.TextProperty, new Binding(nameof(PathNode.Detail)));
        detail.SetValue(TextBlock.FontSizeProperty, 11.0);
        detail.SetValue(TextBlock.ForegroundProperty, LearningTheme.Brush("MutedTextBrush"));
        detail.SetValue(TextBlock.MarginProperty, new Thickness(18, 0, 0, 0));
        detail.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        detail.SetBinding(TextBlock.MaxWidthProperty, new Binding(nameof(PathNode.TextWidth)));
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
