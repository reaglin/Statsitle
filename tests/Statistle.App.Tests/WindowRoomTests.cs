using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Learning.Core.Content;
using Learning.Core.Learners;
using Statistle.App.Learning;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>
/// The window's room, from WebDevelopment+ (Learning Core's "The window's room", W.1-W.6, and this plan's W rows): the book has one
/// part, so no part rows; PlusBook Tools first (My progress, Hand in a report, the Workbench); PlusBook Learning's block under the
/// tree folding down; help hiding to the right; the colours; "In development"; Help ▸ Send feedback (F.1). The start screen's
/// sections are checked from <see cref="LearningViewsTests"/>, the one test that may touch <c>LearnerPlaces</c>.
/// </summary>
public sealed class WindowRoomTests
{
    private static string Egn3443()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new InvalidOperationException("Statistle.sln not found"), "content", "egn3443");
    }

    private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

    [Fact]
    public void PlusBook_Tools_come_first_then_Start_here_and_the_three_parts()
    {
        var content = StatsContent.Load(Egn3443());
        WpfHost.Run(() =>
        {
            var path = LearningPath.Build(content.Package);
            // W.1 (Ron, 2026-11-23: the book in three parts): PlusBook Tools, Start here, then Parts I, II and III, each holding its modules.
            Assert.Same(path.Common, path.Roots[0]);
            Assert.Equal(0, path.Roots[1].Module!.Number);
            var parts = path.Parts.ToList();
            Assert.Equal(["Part I · Fundamental Statistics for Engineers", "Part II · Advanced Statistics", "Part III · Additional Applications of Statistics"],
                parts.Select(p => p.Header));
            Assert.Equal(Enumerable.Range(1, 14), parts[0].Children.Select(m => m.Module!.Number));
            Assert.Equal(Enumerable.Range(15, 4), parts[1].Children.Select(m => m.Module!.Number));
            Assert.Equal(Enumerable.Range(19, 5), parts[2].Children.Select(m => m.Module!.Number));
            Assert.Equal("1. Introduction to Statistics for Engineers", parts[0].Children[0].Header);
            Assert.Equal(content.Package.Modules.Select(m => m.Number), path.ModuleNodes.Select(n => n.Module!.Number));
            Assert.All(parts, p => Assert.All(p.Children, m => Assert.Same(p, m.Parent)));
            // W.6: Parts II and III are in development, their modules too, with no items yet; each has a part page.
            Assert.Contains("items complete", parts[0].Detail, StringComparison.Ordinal);
            Assert.All(parts.Skip(1), p => Assert.EndsWith("in development", p.Detail, StringComparison.Ordinal));
            Assert.All(parts.Skip(1).SelectMany(p => p.Children), m => { Assert.Equal("In development", m.Detail); Assert.Empty(m.Module!.Items); });
            Assert.All(parts, p => Assert.True(content.PartPages.ContainsKey(p.Header), p.Header));
            // W.2: My progress, Hand in a report, My notes, the Glossary and the Workbench (the book's tool), in PlusBook Tools.
            Assert.Equal([path.Dashboard, path.Reports, path.Notes, path.Glossary, path.Workbench], path.Common.Children);
            Assert.True(path.Workbench.IsTool);
            Assert.Equal("PlusBook Tools", path.Common.Header);
            Assert.Contains("module checks complete", path.Reports.Detail, StringComparison.Ordinal);
            Assert.Contains("key terms", path.Glossary.Detail, StringComparison.Ordinal);
            // Rows wrap inside the column at every depth.
            Assert.True(path.Dashboard.TextWidth < path.Common.TextWidth);
            Assert.True(parts[0].Children[0].TextWidth < parts[0].TextWidth);

            // Collapse Menu folds everything; one part opens on its own; going to an item opens only its part and module.
            path.Common.IsExpanded = true;
            path.FoldAll();
            Assert.All(path.AllNodes, n => Assert.False(n.IsExpanded));
            parts[2].IsExpanded = true;
            var item = content.Package.Modules.First(m => m.Number == 3).Items[0];
            path.Select(item);
            Assert.True(path.NodeOf(item.Module)!.IsExpanded);
            Assert.Single(path.ModuleNodes, n => n.IsExpanded);
            Assert.True(parts[0].IsExpanded);
            Assert.False(parts[1].IsExpanded);
            Assert.True(parts[2].IsExpanded);
            Assert.False(path.Common.IsExpanded);
        });
    }

    [Fact]
    public void A_module_still_to_come_is_in_development()
    {
        string folder = Path.Combine(Path.GetTempPath(), "statistle-room-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(folder, "modules", "01-later"));
        File.WriteAllText(Path.Combine(folder, "manifest.json"), """{ "id": "room", "version": "0.1.0", "title": "Room", "modules": ["modules/01-later"] }""");
        File.WriteAllText(Path.Combine(folder, "modules", "01-later", "module.md"), "---\nid: room.m1\nnumber: 1\ntitle: Later\nstatus: coming-later\n---\nTo come.\n");
        try
        {
            WpfHost.Run(() =>
            {
                var package = ContentPackage.Load(folder);
                Assert.Equal(ModuleStatus.ComingLater, package.Modules[0].Status);
                var path = LearningPath.Build(package);
                Assert.Equal("In development", path.ModuleNodes.Single().Detail);
                var page = DashboardPage.Markdown(package, new global::Learning.Core.Progress.LearningProgress());
                Assert.Contains("in development", page, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("coming later", page, StringComparison.OrdinalIgnoreCase);
            });
        }
        finally
        {
            try { Directory.Delete(folder, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    public void The_block_folds_down_help_hides_to_the_right_and_each_learner_keeps_the_choice()
    {
        var content = StatsContent.Load(Egn3443());
        string folder = Path.Combine(Path.GetTempPath(), "statistle-room-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            WpfHost.Run(() =>
            {
                var store = new LearnerStore(folder, new LearnerFileOptions("Statistle", ".statprogress", "test", Environment.MachineName));
                var (ada, _) = store.Create("Ada Lovelace", null, "2468", DateTimeOffset.Now);
                var page = new CoursePage(content);
                var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
                window.Show();
                try
                {
                    page.SignIn(ada);
                    WpfHost.Pump();
                    // W.3, W.5: PlusBook Learning's block, green, under the lavender tree, with Collapse Menu.
                    Assert.Equal("PlusBook Learning", page.PathTitle.Text);
                    Assert.Equal("Collapse Menu", page.FoldAllButton.Content);
                    Assert.True(page.PathInfo.IsExpanded);
                    Assert.True(page.PathTree.TranslatePoint(new Point(0, 0), page).Y < page.PathInfo.TranslatePoint(new Point(0, 0), page).Y);
                    var tree = page.PathTree.ActualHeight;
                    page.PathInfo.IsExpanded = false;
                    WpfHost.Pump();
                    Assert.True(page.PathTree.ActualHeight > tree + 30, $"{page.PathTree.ActualHeight} vs {tree}");
                    Assert.Equal("folded", ada.Data.Preferences["pathInfo"]);

                    // W.4: Hide help leaves the light blue strip; the page gets the room; the strip opens it again.
                    Assert.True(page.IsHelpOpen);
                    Assert.Contains("This screen: My progress", page.HelpMarkdownShown, StringComparison.Ordinal);
                    Assert.Contains("PlusBook Tools", page.HelpMarkdownShown, StringComparison.Ordinal);
                    Assert.Equal(Color.FromRgb(0xDC, 0xEB, 0xFA), ((SolidColorBrush)page.HelpStrip.Background).Color);
                    Assert.Equal(Color.FromRgb(0xEE, 0xF5, 0xFD), ((SolidColorBrush)page.HelpPanel.Background).Color);
                    Click(page.HideHelpButton);
                    WpfHost.Pump();
                    Assert.False(page.HelpPanel.IsVisible);
                    Assert.True(page.HelpStrip.IsVisible);
                    Assert.Equal("folded", ada.Data.Preferences["helpPanel"]);
                    page.SignIn(null);           // someone else, or nobody: help and the block open again
                    WpfHost.Pump();
                    Assert.True(page.IsHelpOpen);
                    Assert.True(page.PathInfo.IsExpanded);
                    page.SignIn(ada);            // Ada again: as she left them
                    WpfHost.Pump();
                    Assert.False(page.IsHelpOpen);
                    Assert.False(page.PathInfo.IsExpanded);
                    Click(page.OpenHelpButton);
                    WpfHost.Pump();
                    Assert.True(page.HelpPanel.IsVisible);
                    Assert.Equal("open", ada.Data.Preferences["helpPanel"]);
                    page.ToggleHelp();          // F1 too
                    Assert.False(page.IsHelpOpen);
                    page.ToggleHelp();

                    // W.2: Hand in a report and the Workbench from PlusBook Tools.
                    var workbench = false;
                    page.WorkbenchRequested += () => workbench = true;
                    LearningPath.Choose(page.Path.Reports);
                    WpfHost.Pump();
                    Assert.Contains("This screen: Hand in a report", page.HelpMarkdownShown, StringComparison.Ordinal);
                    LearningPath.Choose(page.Path.Workbench);
                    WpfHost.Pump();
                    Assert.True(workbench);
                    Assert.False(page.Path.Workbench.IsSelected); // so choosing it again works

                    // W.1: Collapse Menu folds everything; the item on screen stays when its module folds.
                    var item = content.Package.Modules.First(m => m.Number == 2).Items[0];
                    page.Path.Select(item);
                    WpfHost.Pump();
                    Assert.Same(item, page.CurrentItem);
                    Assert.Contains("This screen: an item", page.HelpMarkdownShown, StringComparison.Ordinal);
                    page.Path.NodeOf(item.Module)!.IsExpanded = false;
                    WpfHost.Pump();
                    Assert.Same(item, page.CurrentItem);
                    Click(page.FoldAllButton);
                    WpfHost.Pump();
                    Assert.All(page.Path.AllNodes, n => Assert.False(n.IsExpanded));
                    Assert.Same(item, page.CurrentItem);
                }
                finally
                {
                    window.Close();
                    page.SignIn(null);
                    ada.Dispose();
                }
            });
        }
        finally
        {
            try { Directory.Delete(folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    [Fact]
    public void My_notes_the_glossary_course_links_and_the_part_pages_work_as_in_the_other_PlusBooks()
    {
        var content = StatsContent.Load(Egn3443());
        Assert.True(content.Package.Glossary.Entries.Count >= 50, $"{content.Package.Glossary.Entries.Count} terms");
        string folder = Path.Combine(Path.GetTempPath(), "statistle-room-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            WpfHost.Run(() =>
            {
                var store = new LearnerStore(folder, new LearnerFileOptions("Statistle", ".statprogress", "test", Environment.MachineName));
                var (ada, _) = store.Create("Ada Lovelace", null, "2468", DateTimeOffset.Now);
                var page = new CoursePage(content);
                var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
                window.Show();
                try
                {
                    // Not signed in: Course Links waits for a learner; the My notes tab says how to keep notes.
                    Assert.False(page.CourseLinksButton.IsEnabled);
                    var item = content.Package.Modules.First(m => m.Number == 2).Items[0];
                    page.Path.Select(item);
                    WpfHost.Pump();
                    Assert.Equal("My notes", page.NotesTab.Header);
                    Assert.Equal(Visibility.Collapsed, page.ItemNotes.Visibility);

                    page.SignIn(ada);
                    Assert.True(page.CourseLinksButton.IsEnabled);
                    page.Path.Select(item);
                    WpfHost.Pump();
                    // Key terms are marked on the Learn page (a dotted underline, the first use of each).
                    Assert.True(page.LearnView.TermCount > 0);
                    // A note on a paragraph: a ✎ after it, a count on the tab and in the path, and on the My notes page.
                    page.AddNoteOnLearn(1, "Ask about the trimmed mean in class.");
                    WpfHost.Pump();
                    Assert.Equal(1, page.LearnView.NoteMarkCount);
                    Assert.Equal("My notes (1)", page.NotesTab.Header);
                    Assert.Contains("✎ 1 note", page.Path.NodeOf(item)!.Detail, StringComparison.Ordinal);
                    Assert.Contains("1 note on 1 item", page.Path.Notes.Detail, StringComparison.Ordinal);
                    Assert.Single(ada.Data.Notes.Live);
                    LearningPath.Choose(page.Path.Notes);
                    WpfHost.Pump();
                    Assert.Contains("This screen: My notes", page.HelpMarkdownShown, StringComparison.Ordinal);

                    // The Glossary page, at a term, with the way back to the item.
                    page.Path.Select(item);
                    WpfHost.Pump();
                    var mean = content.Package.Glossary.Find("sample mean")!;
                    page.ShowGlossaryPage(mean);
                    WpfHost.Pump();
                    Assert.Same(mean, page.GlossaryTermShown);
                    Assert.Contains("This screen: Glossary", page.HelpMarkdownShown, StringComparison.Ordinal);

                    // A part's page: what it is, its modules; Part II is in development.
                    var part2 = page.Path.Parts.ElementAt(1);
                    LearningPath.Choose(part2);
                    WpfHost.Pump();
                    Assert.Equal("Part II · Advanced Statistics", page.PartShown);
                    Assert.True(part2.IsExpanded);
                    Assert.Contains("This screen: a part of the book", page.HelpMarkdownShown, StringComparison.Ordinal);

                    // Signing out keeps the notes in the file and shows none on screen; the same learner finds them again.
                    page.SignIn(null);
                    WpfHost.Pump();
                    Assert.DoesNotContain("note", page.Path.Notes.Detail.Replace("notes on the lessons", "", StringComparison.Ordinal), StringComparison.Ordinal);
                    page.SignIn(ada);
                    Assert.Contains("1 note", page.Path.Notes.Detail, StringComparison.Ordinal);
                }
                finally
                {
                    window.Close();
                    page.SignIn(null);
                    ada.Dispose();
                }
            });
        }
        finally
        {
            try { Directory.Delete(folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    [Fact]
    public void The_Workbench_help_hides_to_the_right_and_each_learner_keeps_the_choice()
    {
        string folder = Path.Combine(Path.GetTempPath(), "statistle-room-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            WpfHost.Run(() =>
            {
                Environment.SetEnvironmentVariable("STATISTLE_CONTENT", Egn3443());
                var store = new LearnerStore(Path.Combine(folder, "learners"), new LearnerFileOptions("Statistle", ".statprogress", "test", Environment.MachineName));
                var (ada, _) = store.Create("Ada Lovelace", null, "2468", DateTimeOffset.Now);
                var (bob, _) = store.Create("Bob Boole", null, "1357", DateTimeOffset.Now);
                var workspace = new ViewModels.WorkspaceViewModel(new WorkspaceSafetyTests.FakeDialogs(), Path.Combine(folder, "autosave"));
                var window = new MainWindow(workspace) { Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
                window.Show();
                try
                {
                    WpfHost.Pump();
                    Assert.True(window.IsWorkbenchHelpOpen);
                    window.SignIn(ada);
                    window.ShowWorkbench();
                    WpfHost.Pump();
                    Assert.True(window.IsWorkbenchHelpOpen);
                    window.ToggleWorkbenchHelp();
                    Assert.False(window.IsWorkbenchHelpOpen);
                    Assert.Equal("folded", ada.Data.Preferences["workbenchHelp"]);
                    window.SignIn(bob);          // another learner: help open, as he has never hidden it
                    Assert.True(window.IsWorkbenchHelpOpen);
                    window.ToggleWorkbenchHelp();
                    window.ToggleWorkbenchHelp();
                    Assert.True(window.IsWorkbenchHelpOpen);
                    Assert.Equal("open", bob.Data.Preferences["workbenchHelp"]);
                }
                finally
                {
                    window.Close();
                    Environment.SetEnvironmentVariable("STATISTLE_CONTENT", null);
                }
            });
        }
        finally
        {
            try { Directory.Delete(folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    [Fact]
    public void Send_feedback_names_Statistics_plus_and_its_version()
    {
        App.SetUpLearning();
        var url = global::Learning.Wpf.Feedback.Url();
        Assert.StartsWith("https://softwareplus.ai/feedback/?program=statistle&version=", url, StringComparison.Ordinal);
        var v = typeof(App).Assembly.GetName().Version!;
        Assert.EndsWith($"{v.Major}.{v.Minor}.{v.Build}", url, StringComparison.Ordinal);
        Assert.Equal("Statistics+", global::Learning.Core.LearningApp.Name);
    }

    /// <summary>
    /// W.5 and F.1 on the start screen (called from <see cref="LearningViewsTests"/>): four sections, each its own colour; Who is
    /// learning?, Course Tools and Learner files folded, Help and information open with About PlusBooks and Send feedback; a learner
    /// the idle lock signed out comes back to Who is learning? open.
    /// </summary>
    internal static void StartScreenIsInSections(LearnerScreen screen, LearnerRow learner)
    {
        Border Section(string name) => (Border)screen.FindName(name);
        Expander Fold(string name) => (Expander)screen.FindName(name);
        var folds = new[] { Fold("WhoExpander"), Fold("ToolsExpander"), Fold("FilesExpander") };
        Assert.All(folds, x => Assert.False(x.IsExpanded));
        Assert.Equal(Visibility.Visible, ((TextBlock)screen.FindName("SectionsHint")).Visibility);
        var backgrounds = new[] { "WhoSection", "ToolsSection", "FilesSection", "InfoSection" }
            .Select(n => ((SolidColorBrush)Section(n).Background).Color).ToList();
        Assert.Equal([Color.FromRgb(0xEA, 0xF5, 0xEA), Color.FromRgb(0xFF, 0xF5, 0xE0), Color.FromRgb(0xF2, 0xEE, 0xFA), Color.FromRgb(0xDC, 0xEB, 0xFA)], backgrounds);
        Assert.Equal(Visibility.Visible, Section("InfoSection").Visibility);
        Assert.True(((System.Windows.Documents.Hyperlink)screen.FindName("PlusBooksLink")).IsEnabled);
        var feedback = (Button)screen.FindName("FeedbackButton");
        Assert.Equal(Visibility.Visible, feedback.Visibility);
        Assert.Equal("Send _feedback…", feedback.Content);
        Assert.Equal(global::Learning.Wpf.Feedback.ToolTip, feedback.ToolTip);
        // The Workbench and Check reports are Course Tools (no sign-in).
        Assert.Same(Fold("ToolsExpander"), Ancestor<Expander>((Button)screen.FindName("WorkbenchButton")));
        Assert.Same(Fold("ToolsExpander"), Ancestor<Expander>((Button)screen.FindName("CheckReportButton")));
        Assert.Same(Fold("FilesExpander"), Ancestor<Expander>((Button)screen.FindName("RemoveButton")));

        foreach (var x in folds) x.IsExpanded = true;
        WpfHost.Pump();
        screen.Show();                                  // start-up: every section folded
        WpfHost.Pump();
        Assert.All(folds, x => Assert.False(x.IsExpanded));
        screen.Show("Signed out. Your progress is saved."); // Sign out (Switch learner): Who is learning? opens (Ron, plan N.6)
        WpfHost.Pump();
        Assert.True(Fold("WhoExpander").IsExpanded);
        Assert.False(Fold("FilesExpander").IsExpanded);
        screen.Show("Signed out after 15 minutes.", learner.Listing.Header!.ProfileId);
        WpfHost.Pump();
        Assert.True(Fold("WhoExpander").IsExpanded);
        Assert.False(Fold("ToolsExpander").IsExpanded);
    }

    private static T? Ancestor<T>(DependencyObject child) where T : DependencyObject
    {
        for (var p = LogicalTreeHelper.GetParent(child); p is not null; p = LogicalTreeHelper.GetParent(p))
            if (p is T found) return found;
        return null;
    }
}
