using Statistle.Core.Methods.Descriptive;

namespace Statistle.Core.Plotting;

/// <summary>The two pictures of one sample: its histogram and its box plot.</summary>
public static class DescriptiveFigures
{
    /// <summary>
    /// Bars of the frequency table, with the normal curve of the same mean and standard
    /// deviation scaled to the same area (n · class width · density), so the eye can judge
    /// how far the sample is from normal.
    /// </summary>
    public static PlotSpec Histogram(string title, IReadOnlyList<FrequencyClass> table, double mean, double standardDeviation)
    {
        List<PlotSeries> series = [];

        if (table.Count > 0)
        {
            double width = table[0].Upper - table[0].Lower;
            int n = table[^1].CumulativeFrequency;

            series.Add(new BarSeries("Frequency", AxisSide.Left, [.. table.Select(c => c.Mark)], [.. table.Select(c => (double)c.Frequency)], width)
            {
                ColorHint = "#3B6EA5",
            });

            if (standardDeviation > 0 && !double.IsNaN(mean))
            {
                double from = table[0].Lower - width, to = table[^1].Upper + width;
                List<double> xs = [], ys = [];

                for (int i = 0; i <= 120; i++)
                {
                    double x = from + (to - from) * i / 120;
                    double z = (x - mean) / standardDeviation;
                    xs.Add(x);
                    ys.Add(n * width * Math.Exp(-0.5 * z * z) / (standardDeviation * Math.Sqrt(2 * Math.PI)));
                }

                series.Add(new LineSeries("Normal curve, same mean and s", AxisSide.Left, xs, ys)
                {
                    ColorHint = "#C0392B",
                    Width = 2,
                });
            }
        }

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "value"), new AxisSpec(AxisSide.Left, "frequency")],
            Series = series,
        };
    }

    /// <summary>
    /// An individuals control chart: the values in the order they were taken, the centre line at
    /// the mean, the control limits at x̄ ∓ 3s, and any point beyond a limit marked in red.
    /// </summary>
    public static PlotSpec ControlChart(string title, IReadOnlyList<double> values, double mean, double lcl, double ucl)
    {
        double[] order = [.. Enumerable.Range(1, values.Count).Select(i => (double)i)];
        double first = 0.5, last = values.Count + 0.5;
        int[] outside = [.. Enumerable.Range(0, values.Count).Where(i => values[i] < lcl || values[i] > ucl)];

        List<PlotSeries> series =
        [
            new LineSeries($"UCL = {Show(ucl)}", AxisSide.Left, [first, last], [ucl, ucl]) { ColorHint = "#C0392B", Width = 1.5 },
            new LineSeries($"Centre line x̄ = {Show(mean)}", AxisSide.Left, [first, last], [mean, mean]) { ColorHint = "#1F2933", Width = 1.5 },
            new LineSeries($"LCL = {Show(lcl)}", AxisSide.Left, [first, last], [lcl, lcl]) { ColorHint = "#C0392B", Width = 1.5 },
            new LineSeries("Values in order", AxisSide.Left, order, values) { ColorHint = "#3B6EA5", Width = 1.5 },
            new MarkerSeries("Value", AxisSide.Left, order, values) { ColorHint = "#3B6EA5", Size = 6 },
        ];

        if (outside.Length > 0)
        {
            series.Add(new MarkerSeries("Outside the limits", AxisSide.Left, [.. outside.Select(i => order[i])], [.. outside.Select(i => values[i])])
            {
                ColorHint = "#C0392B",
                Size = 9,
            });
        }

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "observation number", Min: first, Max: last), new AxisSpec(AxisSide.Left, "value")],
            Series = series,
        };
    }

    private static string Show(double value) => value.ToString("G5", System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>
    /// Tukey's box plot, horizontal: the box from Q1 to Q3 with the median across it, whiskers
    /// to the last values inside the fences, and the outliers beyond as points.
    /// </summary>
    public static PlotSpec BoxPlot(string title, IReadOnlyList<double> values, double q1, double median, double q3, double lowerFence, double upperFence)
    {
        double[] inside = [.. values.Where(v => v >= lowerFence && v <= upperFence)];
        double[] outliers = [.. values.Where(v => v < lowerFence || v > upperFence)];
        double whiskerLow = inside.Length > 0 ? inside.Min() : q1;
        double whiskerHigh = inside.Length > 0 ? inside.Max() : q3;
        const double y = 1, half = 0.3;

        List<PlotSeries> series =
        [
            new PolygonSeries("Middle half (Q1 to Q3)", AxisSide.Left, [q1, q3, q3, q1], [y - half, y - half, y + half, y + half]) { ColorHint = "#3B6EA5" },
            new LineSeries("Median", AxisSide.Left, [median, median], [y - half, y + half]) { ColorHint = "#1F2933", Width = 2.5 },
            new LineSeries("Whisker", AxisSide.Left, [whiskerLow, q1], [y, y]) { ColorHint = "#1F2933", Width = 1.5 },
            new LineSeries("Whisker ", AxisSide.Left, [q3, whiskerHigh], [y, y]) { ColorHint = "#1F2933", Width = 1.5 },
            new LineSeries("Whisker end", AxisSide.Left, [whiskerLow, whiskerLow], [y - half / 2, y + half / 2]) { ColorHint = "#1F2933", Width = 1.5 },
            new LineSeries("Whisker end ", AxisSide.Left, [whiskerHigh, whiskerHigh], [y - half / 2, y + half / 2]) { ColorHint = "#1F2933", Width = 1.5 },
        ];

        if (outliers.Length > 0)
        {
            series.Add(new MarkerSeries("Outliers", AxisSide.Left, outliers, [.. outliers.Select(_ => y)]) { ColorHint = "#C0392B" });
        }

        double min = Math.Min(values.Min(), lowerFence), max = Math.Max(values.Max(), upperFence);
        double pad = (max - min) * 0.05;

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, "value", Min: values.Min() - pad, Max: values.Max() + pad),
                new AxisSpec(AxisSide.Left, string.Empty, Min: 0, Max: 2),
            ],
            Series = series,
        };
    }
}
