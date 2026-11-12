using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Learning.Core;
using Learning.Core.Content;
using Learning.Core.Learners;
using Learning.Core.Reports;
using Statistle.App.Learning;
using Statistle.App.ViewModels;
using Statistle.Core.Document;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>
/// The Microsoft Store screenshots and the softwareplus.ai pictures (plan 15.3, 15.4): the whole window at 1600 × 1000 with a
/// sample learner part-way through the course. Runs only when STATISTLE_STORE_SHOTS names a folder, and on its own
/// (<c>--filter StoreShots</c>), because it points the learners' folder at a folder of its own before anything reads it.
/// </summary>
public sealed class StoreShots
{
    [Fact]
    public void Write_the_store_screenshots_when_asked()
    {
        var folder = Environment.GetEnvironmentVariable("STATISTLE_STORE_SHOTS");
        if (string.IsNullOrEmpty(folder)) return;
        Directory.CreateDirectory(folder);
        // A short folder for the start screen to show (it prints where learners' files are), made and removed here.
        var data = Path.Combine(Path.GetPathRoot(Path.GetTempPath())!, "Lab", "Statistics+");
        if (Directory.Exists(data)) throw new InvalidOperationException($"{data} already exists: remove it, or it would be deleted.");
        Environment.SetEnvironmentVariable(LearnerPlaces.DataVariable, data);
        try
        {
            WpfHost.Run(() => Shoot(folder, data));
        }
        finally
        {
            try
            {
                Directory.Delete(data, recursive: true);
                var lab = Path.GetDirectoryName(data)!;
                if (!Directory.EnumerateFileSystemEntries(lab).Any()) Directory.Delete(lab);
            }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    private static void Shoot(string folder, string data)
    {
        LearningApp.Name = "Statistics+";
        var workspace = new WorkspaceViewModel(new SilentDialogs(), Path.Combine(data, "autosave"));
        var window = new MainWindow(workspace)
        {
            Width = 1600, Height = 1000, WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -32000, Top = -32000, ShowInTaskbar = false, ShowActivated = false,
        };
        window.Show();
        var content = window.Course ?? throw new InvalidOperationException(window.CourseProblem);
        var page = window.CoursePage!;

        void Shot(string name)
        {
            window.UpdateLayout();
            WpfHost.Pump();
            var root = (FrameworkElement)window.Content;
            var bitmap = new RenderTargetBitmap((int)root.ActualWidth, (int)root.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            var white = new DrawingVisual();
            using (var dc = white.RenderOpen()) dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, root.ActualWidth, root.ActualHeight));
            bitmap.Render(white);
            bitmap.Render(root);
            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(folder, name + ".png"));
            png.Save(file);
        }

        try
        {
            // Two learners on the computer, one of them part-way through the course.
            var learners = Path.Combine(data, "Learners");
            Directory.CreateDirectory(learners);
            var store = new LearnerStore(learners, LearnerPlaces.Options);
            store.Create("Jordan Lee", null, "1357", DateTimeOffset.Now.AddDays(-20)).File.Dispose();
            var (file, _) = store.Create("Maria Gomez", "U1234567", "2468", DateTimeOffset.Now.AddDays(-30), SampleProgress(content));

            window.ShowLearnerScreen();
            Shot("08-who-is-learning");

            window.SignIn(file);
            page.Path.Dashboard.IsSelected = true;
            Shot("03-my-progress");

            // A Learn tab with a figure, and an exercise with one answer wrong.
            var welch = content.Package.Modules.Single(m => m.Number == 9).Items.First(i => i.Exercise is not null);
            page.Path.Select(welch);
            page.ItemTabs.SelectedIndex = 0;
            Shot("02-learn");
            page.ItemTabs.SelectedIndex = 2;
            var built = page.ExerciseView.Built!;
            for (var i = 0; i < built.Expected.Count; i++)
            {
                var e = built.Expected[i];
                var value = i == built.Expected.Count - 1 ? e.Value * 2 : e.Value;   // the last one wrong, to show the mistake message
                page.ExerciseView.Type(e.Asked.Result, value.ToString("G6", CultureInfo.InvariantCulture));
            }
            page.ExerciseView.CheckAnswers();
            Shot("01-exercise");

            // A module check.
            var check = content.Package.Modules.Single(m => m.Number == 8).Items.Single(i => i.Kind == ItemKind.Assess);
            page.Path.Select(check);
            WpfHost.Pump();
            page.ItemTabs.SelectedIndex = 2;
            Shot("04-module-check");

            page.ShowReports();
            Shot("07-progress-report");

            // The Workbench: a two-sample test from the sample project, and its Python.
            SampleProjects.Install(data);
            workspace.OpenPath(Path.Combine(data, SampleProjects.DemonstrationFileName));
            window.ShowWorkbench();
            workspace.SelectTool(workspace.Analyses.First(a => a.Title.Contains("Two-sample", StringComparison.OrdinalIgnoreCase)));
            var tabs = (TabControl)window.FindName("ContentTabs");
            Select(tabs, "Properties");
            Shot("05-workbench");
            Select(tabs, "Code");
            Shot("06-code");
            page.SignIn(null);
            file.Dispose();
        }
        finally
        {
            window.Close();
        }
    }

    private static void Select(TabControl tabs, string header) =>
        tabs.SelectedItem = tabs.Items.OfType<TabItem>().First(t => (t.Header?.ToString() ?? "").Contains(header, StringComparison.OrdinalIgnoreCase));

    /// <summary>Modules 0–7 done (every item read, passed or answered), Module 8 started: a learner a few weeks in.</summary>
    private static LearnerData SampleProgress(StatsContent content)
    {
        var data = new LearnerData { Profile = new LearnerProfile("maria", "Maria Gomez", "U1234567", DateTimeOffset.Now.AddDays(-30)) };
        var at = DateTimeOffset.Now.AddDays(-28);
        foreach (var module in content.Package.Modules.Where(m => m.Number <= 8))
        {
            foreach (var item in module.Items)
            {
                if (module.Number == 8 && item.Number.EndsWith('3')) break;   // part-way through Module 8
                at = at.AddHours(5);
                if (item.Exercise is { } id && content.Find(id) is { } task)
                {
                    var b = StatsTaskRunner.Build(task);
                    var work = StatsTaskRunner.Work(b.Expected.ToDictionary(e => e.Asked.Result, e => e.Value.ToString("R", CultureInfo.InvariantCulture), StringComparer.Ordinal));
                    var outcome = new StatsTaskRunner().Run(task.Json, work);
                    data.Progress.Record(item, outcome, at);
                    data.Work[ReportBuilder.PassedWorkKey(outcome.ChallengeId)] = new SavedWork { Content = work, Saved = at };
                }
                else if (item.QuizFile is not null || item.Kind == ItemKind.Assess)
                {
                    data.Progress.Record(item, new ChallengeOutcome(item.QuizFile ?? item.Id, true, [],
                        item.Quiz?.Questions.Select(q => new ScenarioOutcome(q.Id, false, true, 1, 1, null, null)).ToList() ?? [], 10, 10, true, "sample"), at);
                }
                else
                {
                    data.Progress.MarkRead(item, at);
                }
                var session = data.Progress.StartSession(at, "LAB-PC");
                global::Learning.Core.Progress.LearningProgress.AddActiveTime(session, item.Id, 1500, at.AddMinutes(25));
            }
        }
        return data;
    }
}
