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
