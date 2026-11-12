using ScottPlot;
using Statistle.Core.Plotting;
using CoreAxisSide = Statistle.Core.Plotting.AxisSide;
using CoreBarSeries = Statistle.Core.Plotting.BarSeries;

namespace Statistle.App.Plotting;

/// <summary>
/// ScottPlot 5 behind <see cref="IPlotSurface"/>. Every ScottPlot type is confined to this
/// file, so swapping the charting library means writing one more class like it.
/// </summary>
public sealed class ScottPlotSurface : IPlotSurface
{
    private readonly Plot _plot;
    private readonly List<IYAxis> _addedAxes = [];
    private IYAxis? _rightAxis;
    private PlotSpec? _lastSpec;
    private bool _logX;

    public ScottPlotSurface(Plot plot) => _plot = plot;

    public ScottPlotSurface() : this(new Plot()) { }

    /// <summary>The underlying plot, for a host control that owns rendering.</summary>
    public Plot Plot => _plot;

    public void Draw(PlotSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        _plot.Clear();

        // Clear() removes plottables but not axes, so an axis added on a previous draw would
        // survive and the figure would grow a new right-hand scale every time it was redrawn.
        foreach (IYAxis axis in _addedAxes)
        {
            _plot.Axes.Remove(axis);
        }

        _addedAxes.Clear();
        _rightAxis = null;

        // A logarithmic x axis is drawn by plotting log10 of every x and labelling the ticks
        // with the power of ten — the spec keeps its values in real units.
        _logX = spec.Axes.Any(a => a.Side == CoreAxisSide.Bottom && a.Logarithmic);
        _plot.Axes.Bottom.TickGenerator = _logX
            ? new ScottPlot.TickGenerators.NumericAutomatic
            {
                MinorTickGenerator = new ScottPlot.TickGenerators.LogMinorTickGenerator(),
                IntegerTicksOnly = true,
                LabelFormatter = v => Math.Pow(10, v).ToString("G4", System.Globalization.CultureInfo.CurrentCulture),
            }
            : new ScottPlot.TickGenerators.NumericAutomatic();
        _plot.Axes.Bottom.TickLabelStyle.Rotation = 0;
        _plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperCenter;

        if (spec.Axes.FirstOrDefault(a => a.Side == CoreAxisSide.Bottom)?.TickLabels is { Count: > 0 } names)
        {
            // Named categories at x = 1 … n. When they would crowd, each goes on two lines: a cell's row over its
            // column ("Supplier A / Rejected"), or a long name split at the space nearest its middle.
            bool crowded = names.Sum(n => n.Length + 2) > 70;
            var manual = new ScottPlot.TickGenerators.NumericManual();
            for (int i = 0; i < names.Count; i++) manual.AddMajor(i + 1, crowded ? TwoLines(names[i]) : names[i]);
            _plot.Axes.Bottom.TickGenerator = manual;
        }

        _plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();

        if (spec.Axes.FirstOrDefault(a => a.Side == CoreAxisSide.Left)?.TickLabels is { Count: > 0 } rows)
        {
            // Named rows at y = 1 … n (the groups of a box plot laid on their side).
            var manual = new ScottPlot.TickGenerators.NumericManual();
            for (int i = 0; i < rows.Count; i++) manual.AddMajor(i + 1, rows[i]);
            _plot.Axes.Left.TickGenerator = manual;
        }

        if (spec.Axes.Any(a => a.Side == CoreAxisSide.Right))
        {
            _rightAxis = _plot.Axes.AddRightAxis();
            _addedAxes.Add(_rightAxis);
        }

        foreach (PlotSeries series in spec.Series)
        {
            AddSeries(series);
        }

        foreach (AxisSpec axis in spec.Axes)
        {
            ApplyAxis(axis);
        }

        if (!string.IsNullOrEmpty(spec.Title))
        {
            _plot.Title(spec.Title);
        }

        // Square units for a cross-section: zoom the shorter axis out rather than clipping the
        // taller one (SquareUnits() keeps x and shrinks y, which cut the top off a pipe).
        _plot.Axes.Rules.Clear();

        if (spec.SquareAxes)
        {
            _plot.Axes.Rules.Add(new ScottPlot.AxisRules.SquareZoomOut(_plot.Axes.Bottom, _plot.Axes.Left));
        }

        if (spec.ShowLegend)
        {
            _plot.ShowLegend(spec.Legend switch
            {
                LegendCorner.UpperLeft => Alignment.UpperLeft,
                LegendCorner.LowerRight => Alignment.LowerRight,
                _ => Alignment.UpperRight,
            });
        }
        else
        {
            _plot.HideLegend();   // the plot is reused, so a previous spec's legend must be taken down
        }

        _lastSpec = spec;
    }

    public void ExportPng(string path, int width, int height) => _plot.SavePng(path, width, height);

    public (double X, double Y) DataAt(double pixelX, double pixelY)
    {
        Pixel pixel = new((float)pixelX, (float)pixelY);
        Coordinates coordinates = _plot.GetCoordinates(pixel);

        return (_logX ? Math.Pow(10, coordinates.X) : coordinates.X, coordinates.Y);
    }

    /// <summary>
    /// Puts the axes back where Draw left them. Zoom and pan are the host control's; this is how
    /// a reader gets out of wherever they have zoomed to.
    /// </summary>
    public void ResetView()
    {
        if (_lastSpec is not null)
        {
            Draw(_lastSpec);
        }
    }

    private void AddSeries(PlotSeries series)
    {
        switch (series)
        {
            case LineSeries line:
                {
                    double[] xs = Xs(line.X);
                    double[] ys = [.. line.Y];
                    var scatter = _plot.Add.ScatterLine(xs, ys);
                    scatter.LegendText = line.Name;
                    scatter.LineWidth = (float)line.Width;

                    if (line.ColorHint is not null)
                    {
                        scatter.LineColor = Color.FromHex(line.ColorHint);
                    }

                    if (line.FillUnder)
                    {
                        scatter.FillY = true;
                        scatter.FillYValue = 0;
                        scatter.FillYColor = scatter.LineColor.WithAlpha(.25);
                    }

                    BindToAxis(scatter, line.ValueAxis);
                    break;
                }

            case MarkerSeries markers:
                {
                    double[] xs = Xs(markers.X);
                    double[] ys = [.. markers.Y];
                    var points = _plot.Add.ScatterPoints(xs, ys);
                    points.LegendText = markers.Name;
                    points.MarkerSize = (float)markers.Size;

                    if (markers.ColorHint is not null)
                    {
                        points.Color = Color.FromHex(markers.ColorHint);
                    }

                    BindToAxis(points, markers.ValueAxis);
                    break;
                }

            case PolygonSeries polygon:
                {
                    Coordinates[] points = [.. polygon.X.Select((x, i) => new Coordinates(_logX ? Math.Log10(Math.Max(x, 1e-12)) : x, polygon.Y[i]))];
                    var filled = _plot.Add.Polygon(points);
                    filled.LegendText = polygon.Name;
                    Color fill = polygon.ColorHint is null ? Colors.SteelBlue : Color.FromHex(polygon.ColorHint);
                    filled.FillStyle.Color = fill.WithAlpha(.45);
                    filled.LineStyle.Color = fill;
                    filled.LineStyle.Width = 1.5f;

                    BindToAxis(filled, polygon.ValueAxis);
                    break;
                }

            case CoreBarSeries bars:
                {
                    Bar[] items =
                    [
                        .. Xs(bars.X).Select((x, i) => new Bar
                        {
                            Position = x,
                            Value = bars.Y[i],
                            Size = bars.BarWidth,
                            FillColor = bars.ColorHint is null
                                ? Colors.SteelBlue
                                : Color.FromHex(bars.ColorHint),
                        }),
                    ];

                    var plotted = _plot.Add.Bars(items);
                    plotted.LegendText = bars.Name;

                    BindToAxis(plotted, bars.ValueAxis);
                    break;
                }

            default:
                throw new NotSupportedException($"Unhandled series type {series.GetType().Name}.");
        }
    }

    private double[] Xs(IReadOnlyList<double> x) =>
        _logX ? [.. x.Select(v => Math.Log10(Math.Max(v, 1e-12)))] : [.. x];

    private void BindToAxis(IPlottable plottable, CoreAxisSide side)
    {
        if (side == CoreAxisSide.Right && _rightAxis is not null)
        {
            plottable.Axes.YAxis = _rightAxis;
        }
    }

    private static string TwoLines(string name)
    {
        if (name.Contains(" / ", StringComparison.Ordinal)) return name.Replace(" / ", "\n", StringComparison.Ordinal);
        int middle = name.Length / 2, best = -1;
        for (int i = 0; i < name.Length; i++)
            if (name[i] == ' ' && (best < 0 || Math.Abs(i - middle) < Math.Abs(best - middle))) best = i;
        return best < 0 ? name : name[..best] + "\n" + name[(best + 1)..];
    }

    private void ApplyAxis(AxisSpec axis)
    {
        switch (axis.Side)
        {
            case CoreAxisSide.Bottom:
                _plot.Axes.Bottom.Label.Text = axis.Label;
                if (axis.Min is double xMin && axis.Max is double xMax)
                {
                    if (_logX)
                    {
                        _plot.Axes.Bottom.Range.Set(Math.Log10(Math.Max(xMin, 1e-12)), Math.Log10(Math.Max(xMax, 1e-12)));
                    }
                    else
                    {
                        _plot.Axes.Bottom.Range.Set(xMin, xMax);
                    }
                }

                break;

            case CoreAxisSide.Left:
                _plot.Axes.Left.Label.Text = axis.Label;
                SetRange(_plot.Axes.Left, axis);
                break;

            case CoreAxisSide.Right when _rightAxis is not null:
                _rightAxis.Label.Text = axis.Label;
                SetRange(_rightAxis, axis);
                break;

            case CoreAxisSide.Top:
                _plot.Axes.Top.Label.Text = axis.Label;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// An inverted axis is expressed to ScottPlot by handing it a range whose bottom value is
    /// the larger one — that is how a secondary series ends up hanging from the top.
    /// </summary>
    private static void SetRange(IAxis target, AxisSpec axis)
    {
        if (axis.Min is not double min || axis.Max is not double max)
        {
            return;
        }

        if (axis.Inverted)
        {
            target.Range.Set(max, min);
        }
        else
        {
            target.Range.Set(min, max);
        }
    }
}
