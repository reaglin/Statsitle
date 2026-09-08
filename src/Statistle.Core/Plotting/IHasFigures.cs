using Statistle.Core.Units;

namespace Statistle.Core.Plotting;

/// <summary>
/// An object that can say how it should be drawn — as many figures as it has. The Plots tab
/// shows them all; the report carries them beside the values. A regression gives its points
/// with the fitted curve through them. The workspace asks; it names no type.
/// </summary>
public interface IHasFigures
{
    /// <summary>The figures to show, in display units; empty when there is nothing to draw yet.</summary>
    IReadOnlyList<PlotSpec> Figures(UnitSelection? units);
}

/// <summary>
/// Turns a figure into a PNG. Implemented in the UI assembly, where the charting library lives;
/// Core passes it through to the report writers so a report can carry its plots without Core
/// referencing a renderer.
/// </summary>
public interface IFigureRenderer
{
    byte[] RenderPng(PlotSpec figure, int width, int height);
}
