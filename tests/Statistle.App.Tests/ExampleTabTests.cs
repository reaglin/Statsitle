using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Statistle.App.Learning;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>
/// Learning Core plan 3.22 in Statistics+: an item's examples to try are on the Example tab, each with a figure; with more than one
/// the tab is Examples (n) with Example 1, Example 2 … above them; the Learn page's ▶ Try it button opens one; and an example to
/// try works its analysis out again when a value changes.
/// </summary>
public sealed class ExampleTabTests
{
    private static string Egn3443()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new InvalidOperationException("Statistle.sln not found"), "content", "egn3443");
    }

    private static IEnumerable<T> Controls<T>(FlowDocument document) where T : UIElement =>
        document.Blocks.OfType<BlockUIContainer>().Select(b => b.Child).OfType<T>();

    [Fact]
    public void Central_tendency_has_its_example_to_try_then_its_worked_example()
    {
        var content = StatsContent.Load(Egn3443());
        WpfHost.Run(() =>
        {
            var page = new CoursePage(content);
            var window = new Window { Content = page, Width = 1440, Height = 900, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
            window.Show();
            try
            {
                page.Path.Select(content.Package.FindItem("st.m2.central-tendency")!);
                WpfHost.Pump();
                Assert.Equal(2, page.ExampleTitles.Count);
                Assert.Equal("One bad day: the mean moves, the median does not", page.ExampleTitles[0]);
                Assert.Equal("Examples (2)", ((TabItem)page.ItemTabs.Items[1]).Header);
                Assert.Equal(Visibility.Visible, page.ExampleParts.Visibility);
                // It opens on the worked example, which the Learn page's "The Example" describes.
                Assert.Null(page.ExamplePageShowing);
                Assert.Equal(1, page.ExampleParts.SelectedIndex);

                // The Learn page has no example to try on it, and its ▶ Try it button opens Example 1.
                page.OpenExample(1);
                window.UpdateLayout();
                WpfHost.Pump();
                Assert.Equal(1, page.ItemTabs.SelectedIndex);
                var shown = page.ExamplePageShowing;
                Assert.NotNull(shown);
                var trying = Assert.Single(Controls<TryAnalysis>(shown.Document));

                double Mean() => (double)trying.Current!.Analysis.Properties().Single(p => p.XmlName == "Mean").Read(trying.Current.Analysis)!;
                Assert.Equal(6, Mean(), 9);
                trying.Set("Sample", "4\n7\n5\n29\n5");
                Assert.Equal(10, Mean(), 9);
                Assert.Null(trying.Problem);
                trying.Set("Sample", "");
                Assert.NotNull(trying.Problem); // no values: it says so, and keeps the last figure
                trying.Set("Sample", "4\n7\nabc");
                Assert.Contains("value 3: \"abc\" is not a number", trying.Problem, StringComparison.Ordinal);
                Assert.Contains("last values that could be worked out", trying.Problem, StringComparison.Ordinal);
                Assert.Equal(10, Mean(), 9); // the last values that could be worked out
                trying.Reset();
                Assert.Equal(6, Mean(), 9);
                Assert.Null(trying.Problem);
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void Values_can_be_typed_one_per_line_or_with_commas()
    {
        Assert.Equal("4\n7\n5", TryAnalysis.ValuesText("4, 7,5"));
        Assert.Equal("4\n7\n5", TryAnalysis.ValuesText("4\r\n7\r\n\r\n5\r\n"));
    }
}
