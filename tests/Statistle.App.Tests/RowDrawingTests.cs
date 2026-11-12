using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Statistle.App.ViewModels;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;

namespace Statistle.App.Tests;

/// <summary>
/// Shows the main window (off screen), opens the sample project and selects every data table and every
/// analysis, drawing each one's rows — value, choice, list, series and table — and every tab. 1.1.0's
/// hand-test build crashed on the first table it drew ("Cannot find resource named 'RejectionText'":
/// a style used by the row templates was defined after them). <see cref="WindowLoadTests"/> could not
/// see it because it draws no row. Fails on any exception, at once or from a deferred callback.
/// </summary>
public sealed class RowDrawingTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "statistle-rows-" + Guid.NewGuid().ToString("N"));

    public RowDrawingTests() => Directory.CreateDirectory(_folder);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void Every_table_and_analysis_of_the_sample_project_draws_its_rows()
    {
        WpfHost.Run(() =>
        {
            // Its own autosave folder and dialogs: the real recovery copy in %LOCALAPPDATA% is never touched.
            var dialogs = new SilentDialogs();
            var workspace = new WorkspaceViewModel(dialogs, Path.Combine(_folder, "autosave"));
            var window = new MainWindow(workspace)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -32000,
                Top = -32000,
                ShowInTaskbar = false,
                ShowActivated = false,
            };

            window.Show();
            window.ShowWorkbench();
            try
            {
                SampleProjects.Install(_folder);
                Assert.True(workspace.OpenPath(Path.Combine(_folder, SampleProjects.DemonstrationFileName)));
                Assert.NotEmpty(workspace.Tables);

                var kinds = new HashSet<string>();
                var tabs = (TabControl)window.FindName("ContentTabs");

                foreach (DataTable table in workspace.Tables.ToList())
                {
                    workspace.Selected = table;
                    Draw(window, workspace, kinds, $"data table {table.Name}");
                }

                foreach (AnalysisDescriptor descriptor in workspace.ProjectTools.Concat(workspace.Analyses))
                {
                    workspace.SelectTool(descriptor);
                    Draw(window, workspace, kinds, descriptor.Title);
                }

                Assert.Equal(new[] { "choice", "list", "series", "table", "value" }, kinds.Order().ToArray());

                // Every tab, for a data table and for an analysis.
                foreach (Action select in new Action[]
                         {
                             () => workspace.Selected = workspace.Tables[0],
                             () => workspace.SelectTool(workspace.Analyses[0]),
                         })
                {
                    select();
                    for (int tab = 0; tab < tabs.Items.Count; tab++)
                    {
                        tabs.SelectedIndex = tab;
                        window.UpdateLayout();
                        WpfHost.Pump();
                    }

                    tabs.SelectedIndex = 0;
                }

                // A screen reader hears each data table's name, not "Statistle.Core.Domain.DataTable".
                var tree = (TreeView)window.FindName("ProjectTree");
                var tablesNode = (TreeViewItem)tree.Items[0];
                foreach (DataTable table in workspace.Tables)
                {
                    var node = (TreeViewItem)tablesNode.ItemContainerGenerator.ContainerFromItem(table);
                    Assert.NotNull(node);
                    Assert.Equal(table.Name, AutomationProperties.GetName(node));
                }

                Assert.Empty(dialogs.Errors);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void Draw(MainWindow window, WorkspaceViewModel workspace, HashSet<string> kinds, string what)
    {
        window.UpdateLayout();
        WpfHost.Pump();

        string listName = workspace.ResultsBeside ? "EntryRowsControl" : "PropertyRows";
        var rows = (ItemsControl)window.FindName(listName);
        Assert.True(workspace.Rows.Count > 0, $"{what} has no rows");

        // The rows were really drawn: the first row's template is applied and holds an editor.
        var first = (FrameworkElement?)rows.ItemContainerGenerator.ContainerFromIndex(0);
        Assert.True(first is not null, $"{what}: no row was drawn");
        Assert.True(Descendants(first).Any(e => e is TextBox or ComboBox), $"{what}: the first row has no editor");

        foreach (PropertyRow row in workspace.Rows)
        {
            // A field with no unit has no empty brackets in the help panel's title (UX review 2026-10-31).
            Assert.DoesNotContain("()", row.HelpTitle);

            kinds.Add(row switch
            {
                { IsTable: true } => "table",
                { IsChoice: true } => "choice",
                { IsEnum: true } => "list",
                { IsSeries: true } => "series",
                _ => "value",
            });
        }

        // At the default size a two-column form fits: no sideways scrolling to read a result, and no box
        // cut off at the edge (UX review 2026-10-31).
        if (workspace.ResultsBeside && window.ActualWidth >= window.Width - 1 && window.Width >= 1440)
        {
            var split = (ScrollViewer)window.FindName("SplitScroll");
            Assert.True(split.ExtentWidth <= split.ViewportWidth + 1,
                $"{what}: the two-column form is {split.ExtentWidth:F0} px wide in a {split.ViewportWidth:F0} px window");

            foreach (Control box in Descendants(split).OfType<Control>().Where(c => c is TextBox or ComboBox && c.IsVisible))
            {
                double right = box.TranslatePoint(new Point(box.ActualWidth, 0), split).X;
                Assert.True(right <= split.ViewportWidth + 1,
                    $"{what}: {AutomationProperties.GetName(box)} ends at {right:F0} px, past the {split.ViewportWidth:F0} px window");
            }
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (DependencyObject below in Descendants(child))
            {
                yield return below;
            }
        }
    }
}

/// <summary>Answers nothing and shows nothing; records any error the workspace would have shown.</summary>
internal sealed class SilentDialogs : IWorkspaceDialogs
{
    public List<string> Errors { get; } = [];

    public UnsavedChoice AskToSave(string fileName, string beforeWhat) => UnsavedChoice.Discard;

    public bool Confirm(string title, string message) => false;

    public void ShowError(string title, string message) => Errors.Add($"{title}: {message}");

    public string? ChooseProjectToOpen(string initialFolder) => null;

    public string? ChooseProjectSavePath(string suggestedName, string initialFolder) => null;
}
