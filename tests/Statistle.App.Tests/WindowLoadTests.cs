using System.Threading;
using System.Windows.Controls;

namespace Statistle.App.Tests;

/// <summary>
/// The main and Import windows' XAML parses and every StaticResource resolves (a mistake there only shows when the
/// program starts). Builds the windows without showing them.
/// </summary>
public class WindowLoadTests
{
    [Fact]
    public void The_main_window_and_the_import_window_load()
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                var app = new App();
                app.InitializeComponent();

                var main = new MainWindow();
                Assert.IsType<ViewModels.WorkspaceViewModel>(main.DataContext);

                // About is not built here: its picture's pack://application: address resolves against
                // the test host rather than the program.
                var table = new Core.Persistence.ImportedTable("Sheet1", [["x", "name"], ["1", "a"], ["2", "b"]]);
                var import = new Views.ImportDialog([table], Views.ImportDialog.ImportKind.Table, "Data", "file.csv", 2);
                Assert.Equal("Statistle — Import data", import.Title);
                Assert.Equal([0], Views.ImportDialog.NumericColumns(table));
                Assert.IsType<DockPanel>(main.Content);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
    }
}
