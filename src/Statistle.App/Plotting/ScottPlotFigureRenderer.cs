using ScottPlot;
using Statistle.Core.Plotting;

namespace Statistle.App.Plotting;

/// <summary>
/// Draws a figure off-screen and hands back the PNG — what lets a report carry its plots.
/// The only place besides <see cref="ScottPlotSurface"/> that names the charting library.
/// </summary>
public sealed class ScottPlotFigureRenderer : IFigureRenderer
{
    public byte[] RenderPng(PlotSpec figure, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(figure);

        var surface = new ScottPlotSurface();
        surface.Draw(figure);

        return surface.Plot.GetImageBytes(width, height, ImageFormat.Png);
    }
}
