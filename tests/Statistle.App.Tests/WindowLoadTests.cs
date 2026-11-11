using System.IO;
using System.Windows.Controls;

namespace Statistle.App.Tests;

/// <summary>
/// The main and Import windows' XAML parses and every StaticResource resolves (a mistake there only shows when the
/// program starts). Builds the windows without showing them. A resource used only inside a row template is not
/// reached here — <see cref="RowDrawingTests"/> draws every row.
/// </summary>
public class WindowLoadTests
{
    [Fact]
    public void The_main_window_and_the_import_window_load()
    {
        WpfHost.Run(() =>
        {
            // Never the parameterless window here: closing it deletes the real recovery copy in
            // %LOCALAPPDATA%\Statistle (a person's unsaved work). Its own folder instead.
            string folder = Path.Combine(Path.GetTempPath(), "statistle-load-" + Guid.NewGuid().ToString("N"));
            var workspace = new ViewModels.WorkspaceViewModel(new NoDialogs(), folder);
            var main = new MainWindow(workspace);
            Assert.Same(workspace, main.DataContext);

            // About is not built here: its picture's pack://application: address resolves against
            // the test host rather than the program.
            var table = new Core.Persistence.ImportedTable("Sheet1", [["x", "name"], ["1", "a"], ["2", "b"]]);
            var import = new Views.ImportDialog([table], Views.ImportDialog.ImportKind.Table, "Data", "file.csv", 2);
            Assert.Equal("Statistics+ — Import data", import.Title);
            Assert.Equal([0], Views.ImportDialog.NumericColumns(table));
            Assert.IsType<DockPanel>(main.Content);
            main.Close();

            // App.OnStartup has run in the host and opened nothing: no window of the program's own is showing.
            WpfHost.Pump();
            Assert.DoesNotContain(System.Windows.Application.Current.Windows.Cast<System.Windows.Window>(), w => w.IsVisible);
        });
    }

    private sealed class NoDialogs : ViewModels.IWorkspaceDialogs
    {
        public ViewModels.UnsavedChoice AskToSave(string fileName, string beforeWhat) => ViewModels.UnsavedChoice.Discard;

        public bool Confirm(string title, string message) => false;

        public void ShowError(string title, string message)
        {
        }

        public string? ChooseProjectToOpen(string initialFolder) => null;

        public string? ChooseProjectSavePath(string suggestedName, string initialFolder) => null;
    }
}
