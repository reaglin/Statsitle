using System.IO;
using System.Windows;
using System.Windows.Controls;
using Learning.Core.Content;
using Statistle.App.Learning;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>
/// Plan 9.3: the Course opens My progress, every module and every item of the shipped content, and every tab of each,
/// without an exception, with nobody signed in (so nothing is written).
/// </summary>
public sealed class CourseDrawingTests
{
    private static string Egn3443()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new InvalidOperationException("Statistle.sln not found"), "content", "egn3443");
    }

    [Fact]
    public void Every_module_item_and_tab_of_the_course_draws()
    {
        var content = StatsContent.Load(Egn3443());
        WpfHost.Run(() =>
        {
            var page = new CoursePage(content);
            var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
            window.Show();
            try
            {
                var opened = 0;
                foreach (PathNode root in page.Path.Roots)
                {
                    root.IsSelected = true;
                    WpfHost.Pump();
                    foreach (var child in root.Children)
                    {
                        page.Path.Select(child.Item!);
                        WpfHost.Pump();
                        Assert.Equal(child.Item, page.CurrentItem);
                        var tabs = page.ItemTabs;
                        foreach (TabItem tab in tabs.Items)
                        {
                            tabs.SelectedItem = tab;
                            window.UpdateLayout();
                            WpfHost.Pump();
                        }
                        opened++;
                    }
                }
                Assert.Equal(content.Package.AllItems.Count(), opened);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void A_question_with_a_figure_draws_the_figure_in_the_quiz()
    {
        // Plan 10.15: [[figure: task n]] in a question's prompt is drawn, as in a Learn page; the tag is not shown as text.
        var content = StatsContent.Load(Egn3443());
        var item = content.Package.AllItems.First(i => i.Quiz?.Questions.Any(q => q.Prompt.Contains("[[figure:", StringComparison.Ordinal)) == true);
        WpfHost.Run(() =>
        {
            var page = new CoursePage(content);
            var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
            window.Show();
            try
            {
                page.Path.Select(item);
                WpfHost.Pump();
                page.ItemTabs.SelectedIndex = 2;
                window.UpdateLayout();
                WpfHost.Pump();
                var all = Descendants(page.QuizView).ToList();
                Assert.Contains(all, d => d is Statistle.App.Plotting.FigurePanel);
                Assert.DoesNotContain(all.OfType<TextBlock>(), t => t.Text.Contains("[[", StringComparison.Ordinal));
                Assert.DoesNotContain(all.OfType<TextBlock>(), t => t.Text.Contains("cannot draw", StringComparison.Ordinal));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            yield return child;
            foreach (var d in Descendants(child)) yield return d;
        }
    }

    [Fact]
    public void An_exercise_checked_without_a_learner_is_graded_but_not_kept()
    {
        var content = StatsContent.Load(Egn3443());
        var item = content.Package.AllItems.First(i => i.Exercise is not null);
        WpfHost.Run(() =>
        {
            var page = new CoursePage(content);
            var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
            window.Show();
            try
            {
                var statuses = new List<string>();
                page.Status += statuses.Add;
                page.Path.Select(item);
                WpfHost.Pump();
                Assert.Equal(item, page.CurrentItem);
                Assert.Equal(item.Exercise, page.ExerciseView.Built?.Task.Id);
                page.ExerciseView.CheckAnswers();
                Assert.DoesNotContain(statuses, t => t.Contains("right", StringComparison.Ordinal));   // an empty check is not counted
                page.ExerciseView.Type(page.ExerciseView.Built!.Expected[0].Asked.Result, "-1");
                page.ExerciseView.CheckAnswers();
                Assert.Contains(statuses, s => s.Contains("not kept", StringComparison.Ordinal));
                Assert.Null(page.Session);
            }
            finally
            {
                window.Close();
            }
        });
    }
}

/// <summary>Pictures of the Course for checking a build by eye, when STATISTLE_COURSE_SHOTS names a folder (not part of the normal run).</summary>
public sealed class CourseShots
{
    [Fact]
    public void Write_course_pictures_when_asked()
    {
        var folder = Environment.GetEnvironmentVariable("STATISTLE_COURSE_SHOTS");
        if (string.IsNullOrEmpty(folder)) return;
        Directory.CreateDirectory(folder);
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        var content = StatsContent.Load(Path.Combine(dir!, "content", "egn3443"));
        WpfHost.Run(() =>
        {
            var page = new CoursePage(content);
            var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
            window.Show();
            void Shot(string name)
            {
                window.UpdateLayout();
                WpfHost.Pump();
                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)page.ActualWidth, (int)page.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                bitmap.Render(page);
                var png = new System.Windows.Media.Imaging.PngBitmapEncoder();
                png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                using var file = File.Create(Path.Combine(folder, name + ".png"));
                png.Save(file);
            }
            try
            {
                Shot("01-my-progress");
                page.Path.Roots.First(r => r.Module is not null).IsSelected = true;
                Shot("02-module");
                foreach (var item in content.Package.AllItems)
                {
                    page.Path.Select(item);
                    var slug = item.Id.Split('.').Last();
                    for (var t = 0; t < page.ItemTabs.Items.Count; t++)
                    {
                        page.ItemTabs.SelectedIndex = t;
                        if (t == 2 && item.Exercise is not null)
                        {
                            page.ExerciseView.Type(page.ExerciseView.Built!.Expected[0].Asked.Result, "1");
                            page.ExerciseView.CheckAnswers();
                        }
                        Shot($"item-{item.Number}-{slug}-{t + 1}");
                    }
                }
            }
            finally { window.Close(); }
        });
    }
}
