using System.Windows;
using System.Windows.Controls;
using Statistle.Core.Plotting;

namespace Statistle.App.Plotting;

/// <summary>
/// One figure on the Plots tab: the plot with its own zoom and pan, a reset, a save, and a
/// cursor readout. The tab stacks one of these per figure the selected item declares.
/// </summary>
public partial class FigurePanel : UserControl
{
    private ScottPlotSurface? _surface;
    private string _fileName = "plot.png";

    public FigurePanel()
    {
        InitializeComponent();
    }

    public void Show(PlotSpec figure)
    {
        ArgumentNullException.ThrowIfNull(figure);

        _surface ??= new ScottPlotSurface(PlotView.Plot);
        _surface.Draw(figure);
        PlotView.Refresh();

        string cleaned = string.Concat((figure.Title ?? "plot").Split(System.IO.Path.GetInvalidFileNameChars()));
        _fileName = (cleaned.Length == 0 ? "plot" : cleaned) + ".png";
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _surface?.ResetView();
        PlotView.Refresh();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_surface is null)
        {
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG image (*.png)|*.png",
            DefaultExt = ".png",
            FileName = _fileName,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _surface.ExportPng(dialog.FileName, 1200, 720);
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Could not save the image");
        }
    }

    private void PlotView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_surface is null)
        {
            return;
        }

        Point position = e.GetPosition(PlotView);
        (double x, double y) = _surface.DataAt(position.X, position.Y);

        Readout.Text = $"{x,10:G5}    {y,10:G5}";
    }
}
