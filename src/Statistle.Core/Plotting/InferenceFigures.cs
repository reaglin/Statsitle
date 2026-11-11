using Statistle.Core.Methods.Distributions;
using Statistle.Core.Methods.Inference;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of inference: an interval as a bar, a test as its reference distribution with the rejection region and the observed statistic.</summary>
public static class InferenceFigures
{
    /// <summary>A horizontal bar from the lower to the upper limit with the estimate marked.</summary>
    public static PlotSpec IntervalBar(string title, Interval interval)
    {
        double pad = Math.Max((interval.Upper - interval.Lower) * 0.25, 1e-9);

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, "value", Min: interval.Lower - pad, Max: interval.Upper + pad),
                new AxisSpec(AxisSide.Left, string.Empty, Min: 0, Max: 2),
            ],
            Series =
            [
                new LineSeries("Interval", AxisSide.Left, [interval.Lower, interval.Upper], [1, 1]) { ColorHint = "#3B6EA5", Width = 4 },
                new LineSeries("Lower", AxisSide.Left, [interval.Lower, interval.Lower], [0.8, 1.2]) { ColorHint = "#3B6EA5", Width = 2 },
                new LineSeries("Upper", AxisSide.Left, [interval.Upper, interval.Upper], [0.8, 1.2]) { ColorHint = "#3B6EA5", Width = 2 },
                new MarkerSeries("Estimate", AxisSide.Left, [interval.Estimate], [1]) { ColorHint = "#C0392B", Size = 9 },
            ],
        };
    }

    /// <summary>
    /// The reference distribution of a test statistic with the rejection region(s) shaded and
    /// the observed statistic as a vertical line.
    /// </summary>
    public static PlotSpec TestStatistic(string title, ContinuousModel reference, TestOutcome outcome, string symbol)
    {
        (double low, double high) = reference.PlotRange;
        low = Math.Min(low, outcome.Statistic - 0.5);
        high = Math.Max(high, outcome.Statistic + 0.5);
        List<double> xs = [], ys = [];

        for (int i = 0; i <= 300; i++)
        {
            double x = low + (high - low) * i / 300;
            xs.Add(x);
            ys.Add(reference.Density(x));
        }

        List<PlotSeries> series = [];

        if (!double.IsNaN(outcome.CriticalLow))
        {
            series.Add(Shade("Rejection region", reference, low, outcome.CriticalLow));
        }

        if (!double.IsNaN(outcome.CriticalHigh))
        {
            // The second tail of a two-sided test is unnamed, so the legend lists the region once.
            series.Add(Shade(double.IsNaN(outcome.CriticalLow) ? "Rejection region" : string.Empty, reference, outcome.CriticalHigh, high));
        }

        series.Add(new LineSeries("Density under H₀", AxisSide.Left, xs, ys) { ColorHint = "#1F2933", Width = 2 });
        double top = ys.Max() * 1.05;
        series.Add(new LineSeries($"Observed {symbol}", AxisSide.Left, [outcome.Statistic, outcome.Statistic], [0, top]) { ColorHint = "#E08E0B", Width = 3 });

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, symbol), new AxisSpec(AxisSide.Left, "density", Min: 0)],
            Series = series,
        };
    }

    private static PolygonSeries Shade(string name, ContinuousModel model, double from, double to)
    {
        List<double> px = [from], py = [0];

        for (int i = 0; i <= 80; i++)
        {
            double x = from + (to - from) * i / 80;
            px.Add(x);
            py.Add(model.Density(x));
        }

        px.Add(to);
        py.Add(0);
        return new PolygonSeries(name, AxisSide.Left, px, py) { ColorHint = "#C0392B" };
    }

    /// <summary>Two samples as side-by-side box plots on one axis.</summary>
    public static PlotSpec TwoBoxes(string title, string name1, IReadOnlyList<double> x, string name2, IReadOnlyList<double> y)
    {
        List<PlotSeries> series = [];
        double min = Math.Min(x.Min(), y.Min()), max = Math.Max(x.Max(), y.Max()), pad = (max - min) * 0.05;
        AddBox(series, x, 1.5, name1);
        AddBox(series, y, 0.5, name2);

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, "value", Min: min - pad, Max: max + pad),
                new AxisSpec(AxisSide.Left, $"{name2} (below) and {name1} (above)", Min: 0, Max: 2),
            ],
            Series = series,
        };
    }

    internal static void AddBox(List<PlotSeries> series, IReadOnlyList<double> values, double y, string name)
    {
        Methods.Descriptive.Quartiles q = Methods.Descriptive.SummaryStatistics.Type7Quartiles(values);
        double[] inside = [.. values.Where(v => v >= q.LowerFence && v <= q.UpperFence)];
        double lo = inside.Length > 0 ? inside.Min() : q.Q1, hi = inside.Length > 0 ? inside.Max() : q.Q3;
        const double half = 0.25;
        series.Add(new PolygonSeries(name, AxisSide.Left, [q.Q1, q.Q3, q.Q3, q.Q1], [y - half, y - half, y + half, y + half]) { ColorHint = "#3B6EA5" });
        series.Add(new LineSeries(name + " median", AxisSide.Left, [q.Median, q.Median], [y - half, y + half]) { ColorHint = "#1F2933", Width = 2.5 });
        series.Add(new LineSeries(name + " whisker", AxisSide.Left, [lo, q.Q1], [y, y]) { ColorHint = "#1F2933", Width = 1.5 });
        series.Add(new LineSeries(name + " whisker ", AxisSide.Left, [q.Q3, hi], [y, y]) { ColorHint = "#1F2933", Width = 1.5 });
        double[] outliers = [.. values.Where(v => v < q.LowerFence || v > q.UpperFence)];

        if (outliers.Length > 0)
        {
            series.Add(new MarkerSeries(name + " outliers", AxisSide.Left, outliers, [.. outliers.Select(_ => y)]) { ColorHint = "#C0392B" });
        }
    }
}
