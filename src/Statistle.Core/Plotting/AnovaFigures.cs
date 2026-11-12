using MathNet.Numerics.Distributions;
using Statistle.Core.Methods.Inference;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of an analysis of variance: the groups side by side, their means with intervals, the interaction plot, and the residual Q–Q.</summary>
public static class AnovaFigures
{
    /// <summary>Every group as a box plot on one value axis, the first group at the top.</summary>
    public static PlotSpec Boxes(string title, string valueLabel, IReadOnlyList<string> names, IReadOnlyList<IReadOnlyList<double>> groups)
    {
        List<PlotSeries> series = [];
        double min = groups.Min(g => g.Min()), max = groups.Max(g => g.Max()), pad = Math.Max((max - min) * 0.05, 1e-9);
        int k = groups.Count;

        for (int i = 0; i < k; i++)
        {
            InferenceFigures.AddBox(series, groups[i], k - i, names[i]);
        }

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, valueLabel, Min: min - pad, Max: max + pad),
                // Each box named at its own height (the first group at the top), not by numbered ticks.
                new AxisSpec(AxisSide.Left, "group", Min: 0.5, Max: k + 0.5, TickLabels: [.. names.Reverse()]),
            ],
            Series = series,
        };
    }

    /// <summary>Group means with their intervals, in group order along the bottom axis.</summary>
    public static PlotSpec Means(string title, string valueLabel, IReadOnlyList<GroupSummary> groups, IReadOnlyList<double> lower, IReadOnlyList<double> upper)
    {
        List<PlotSeries> series = [];
        double[] x = [.. Enumerable.Range(1, groups.Count).Select(i => (double)i)];

        for (int i = 0; i < groups.Count; i++)
        {
            series.Add(new LineSeries(groups[i].Name + " interval", AxisSide.Left, [x[i], x[i]], [lower[i], upper[i]]) { ColorHint = "#3B6EA5", Width = 2 });
            series.Add(new LineSeries(groups[i].Name + " cap", AxisSide.Left, [x[i] - 0.1, x[i] + 0.1], [lower[i], lower[i]]) { ColorHint = "#3B6EA5", Width = 1.5 });
            series.Add(new LineSeries(groups[i].Name + " cap ", AxisSide.Left, [x[i] - 0.1, x[i] + 0.1], [upper[i], upper[i]]) { ColorHint = "#3B6EA5", Width = 1.5 });
        }

        series.Add(new MarkerSeries("Means", AxisSide.Left, x, [.. groups.Select(g => g.Mean)]) { ColorHint = "#C0392B", Size = 9 });
        double lo = lower.Min(), hi = upper.Max(), pad = Math.Max((hi - lo) * 0.1, 1e-9);

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, "group", Min: 0.5, Max: groups.Count + 0.5, TickLabels: [.. groups.Select(g => g.Name)]),
                new AxisSpec(AxisSide.Left, valueLabel, Min: lo - pad, Max: hi + pad),
            ],
            Series = series,
        };
    }

    /// <summary>Cell means against the levels of A, one line per level of B: parallel lines mean no interaction.</summary>
    public static PlotSpec Interaction(string title, string aName, string bName, string valueLabel, TwoWayResult result)
    {
        List<PlotSeries> series = [];
        string[] palette = ["#3B6EA5", "#C0392B", "#2E8B57", "#8E44AD", "#D68910", "#1F2933"];
        // The levels of A as categories at 1 … a, each named by its level: coded levels 1, 2, 3 no longer get ticks 1.1, 1.2 …
        double[] x = [.. Enumerable.Range(1, result.LevelsA.Count).Select(i => (double)i)];

        for (int j = 0; j < result.LevelsB.Count; j++)
        {
            double[] y = [.. Enumerable.Range(0, result.LevelsA.Count).Select(i => result.CellMeans[i, j])];
            series.Add(new LineSeries($"{bName} = {result.LevelsB[j]:G6}", AxisSide.Left, x, y) { ColorHint = palette[j % palette.Length], Width = 2 });
            // No legend entry of its own: the line's entry names the level, and a second entry per level made the
            // legend twice as tall, so it hid cell means in the upper right corner.
            series.Add(new MarkerSeries(string.Empty, AxisSide.Left, x, y) { ColorHint = palette[j % palette.Length] });
        }

        return new PlotSpec
        {
            Title = title,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, $"level of {aName}", Min: 0.7, Max: result.LevelsA.Count + 0.3,
                    TickLabels: [.. result.LevelsA.Select(v => v.ToString("G6", System.Globalization.CultureInfo.CurrentCulture))]),
                new AxisSpec(AxisSide.Left, $"mean {valueLabel}"),
            ],
            Series = series,
        };
    }

    /// <summary>Normal Q–Q plot of the residuals with the reference line.</summary>
    public static PlotSpec ResidualQq(string title, IReadOnlyList<double> residuals)
    {
        int n = residuals.Count;
        double sd = Math.Sqrt(residuals.Sum(r => r * r) / Math.Max(1, n - 1));
        double[] ordered = [.. residuals.Select(r => sd > 0 ? r / sd : r).Order()];
        double[] theoretical = [.. Enumerable.Range(1, n).Select(i => Normal.InvCDF(0, 1, (i - 0.375) / (n + 0.25)))];
        double low = theoretical.Min(), high = theoretical.Max();

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes = [new AxisSpec(AxisSide.Bottom, "theoretical normal quantile"), new AxisSpec(AxisSide.Left, "standardized residual")],
            Legend = LegendCorner.UpperLeft,
            Series =
            [
                new LineSeries("Normal", AxisSide.Left, [low, high], [low, high]) { ColorHint = "#B8C4D0", Width = 1.5 },
                new MarkerSeries("Residuals", AxisSide.Left, theoretical, ordered) { ColorHint = "#3B6EA5" },
            ],
        };
    }
}
