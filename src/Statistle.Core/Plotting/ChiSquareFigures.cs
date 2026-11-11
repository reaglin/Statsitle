using Statistle.Core.Methods.Distributions;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of a chi-square test: its reference distribution, and the observed counts beside the expected.</summary>
public static class ChiSquareFigures
{
    /// <summary>
    /// The χ²(df) density with the rejection region above the critical value shaded and the
    /// observed χ² as a vertical line. With one or two degrees of freedom the density is
    /// unbounded or steep at zero, so the curve is drawn from just above zero and the axis capped.
    /// </summary>
    public static PlotSpec Reference(string title, double df, double statistic, double critical)
    {
        var model = new ChiSquaredModel(df);
        double high = Math.Max(model.Quantile(0.995), Math.Max(critical, statistic) * 1.08);
        double low = df <= 2 ? high / 2000 : 0;
        double cap = df <= 2 ? 1.0 : double.PositiveInfinity;
        List<double> xs = [], ys = [];

        for (int i = 0; i <= 300; i++)
        {
            double x = low + (high - low) * i / 300;
            xs.Add(x);
            ys.Add(Math.Min(model.Density(x), cap));
        }

        double top = ys.Max() * 1.05;
        List<PlotSeries> series = [];
        List<double> px = [critical], py = [0];

        for (int i = 0; i <= 80; i++)
        {
            double x = critical + (high - critical) * i / 80;
            px.Add(x);
            py.Add(Math.Min(model.Density(x), cap));
        }

        px.Add(high);
        py.Add(0);
        series.Add(new PolygonSeries("Rejection region", AxisSide.Left, px, py) { ColorHint = "#C0392B" });
        series.Add(new LineSeries("Density under H₀", AxisSide.Left, xs, ys) { ColorHint = "#1F2933", Width = 2 });
        series.Add(new LineSeries("Observed χ²", AxisSide.Left, [statistic, statistic], [0, top]) { ColorHint = "#C0392B", Width = 2.5 });

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "χ²", Min: 0, Max: high), new AxisSpec(AxisSide.Left, "density", Min: 0, Max: top)],
            Series = series,
        };
    }

    /// <summary>
    /// The observed and expected count of each category (or cell, row by row) as bars side by
    /// side: category i at x = i, observed just left of it, expected just right.
    /// </summary>
    public static PlotSpec ObservedAndExpected(string title, string axisLabel, IReadOnlyList<double> observed, IReadOnlyList<double> expected,
        IReadOnlyList<string>? names = null)
    {
        ArgumentNullException.ThrowIfNull(observed);
        ArgumentNullException.ThrowIfNull(expected);

        double[] at = [.. Enumerable.Range(1, observed.Count).Select(i => (double)i)];
        double top = Math.Max(observed.Max(), expected.Max()) * 1.1;

        return new PlotSpec
        {
            Title = title,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, axisLabel, Min: 0.4, Max: observed.Count + 0.6,
                    TickLabels: names is { Count: > 0 } && names.Count == observed.Count ? names : null),
                new AxisSpec(AxisSide.Left, "count", Min: 0, Max: top > 0 ? top : 1),
            ],
            Series =
            [
                new BarSeries("Observed", AxisSide.Left, [.. at.Select(x => x - 0.2)], observed, 0.4) { ColorHint = "#3B6EA5" },
                new BarSeries("Expected under H₀", AxisSide.Left, [.. at.Select(x => x + 0.2)], expected, 0.4) { ColorHint = "#B8C4D0" },
            ],
        };
    }
}
