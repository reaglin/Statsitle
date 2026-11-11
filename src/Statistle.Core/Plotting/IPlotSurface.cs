namespace Statistle.Core.Plotting;

/// <summary>Which edge of the figure an axis lives on.</summary>
public enum AxisSide
{
    Left,
    Right,
    Bottom,
    Top,
}

/// <summary>
/// One axis of a figure. <paramref name="Inverted"/> makes values increase downward — the
/// secondary axis of a two-scale figure can be drawn that way so its series hangs
/// from the top of the plot. <paramref name="Logarithmic"/> spaces the axis by log10 — a
/// frequency curve's return-period axis. Series values stay in real units; the renderer
/// transforms.
/// </summary>
public sealed record AxisSpec(
    AxisSide Side,
    string Label,
    bool Inverted = false,
    double? Min = null,
    double? Max = null,
    bool Logarithmic = false,
    IReadOnlyList<string>? TickLabels = null);   // names for x = 1, 2, …, n (categories), in place of numbers

/// <summary>A drawable series. The value axis names which <see cref="AxisSpec"/> it is scaled against.</summary>
public abstract record PlotSeries(string Name, AxisSide ValueAxis)
{
    public string? ColorHint { get; init; }
}

/// <summary>A continuous curve — a fitted regression, a cumulative distribution.</summary>
public sealed record LineSeries(
    string Name,
    AxisSide ValueAxis,
    IReadOnlyList<double> X,
    IReadOnlyList<double> Y) : PlotSeries(Name, ValueAxis)
{
    public double Width { get; init; } = 2.0;
    public bool FillUnder { get; init; }
}

/// <summary>Discrete bars at a fixed interval — a hyetograph.</summary>
/// <summary>Points without a connecting line — observations on a regression plot.</summary>
public sealed record MarkerSeries(
    string Name,
    AxisSide ValueAxis,
    IReadOnlyList<double> X,
    IReadOnlyList<double> Y) : PlotSeries(Name, ValueAxis)
{
    public double Size { get; init; } = 7.0;
}

public sealed record BarSeries(
    string Name,
    AxisSide ValueAxis,
    IReadOnlyList<double> X,
    IReadOnlyList<double> Y,
    double BarWidth) : PlotSeries(Name, ValueAxis);

/// <summary>A closed, filled polygon — the water in a pipe section.</summary>
public sealed record PolygonSeries(
    string Name,
    AxisSide ValueAxis,
    IReadOnlyList<double> X,
    IReadOnlyList<double> Y) : PlotSeries(Name, ValueAxis);

/// <summary>
/// A complete figure, described with no reference to any charting library. Anything that can
/// render this can back <see cref="IPlotSurface"/>, which is what keeps the library swappable.
/// </summary>
public sealed class PlotSpec
{
    public string Title { get; init; } = string.Empty;

    public IReadOnlyList<AxisSpec> Axes { get; init; } = [];

    public IReadOnlyList<PlotSeries> Series { get; init; } = [];

    public bool ShowLegend { get; init; } = true;

    /// <summary>True to draw x and y at the same scale — a cross-section must not be stretched.</summary>
    public bool SquareAxes { get; init; }
}

/// <summary>
/// The one seam between the domain and whatever draws it. Implementations live in the UI
/// assembly; <c>Statistle.Core</c> only ever builds a <see cref="PlotSpec"/>.
/// </summary>
public interface IPlotSurface
{
    /// <summary>Render the figure, replacing anything previously drawn.</summary>
    void Draw(PlotSpec spec);

    /// <summary>Write the current figure to a PNG file.</summary>
    void ExportPng(string path, int width, int height);

    /// <summary>
    /// The data coordinates under a point on the rendered figure, for a cursor readout. Pixels in,
    /// engineering units out — no charting type crosses the seam.
    /// </summary>
    (double X, double Y) DataAt(double pixelX, double pixelY);

    /// <summary>Restores the figure to the view it was drawn with.</summary>
    void ResetView();
}
