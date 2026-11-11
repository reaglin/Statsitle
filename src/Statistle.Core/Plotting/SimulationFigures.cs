using Statistle.Core.Methods.Descriptive;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of a simulation: the law of large numbers, outcomes against theory, the darts, the intervals and the sample means.</summary>
public static class SimulationFigures
{
    /// <summary>
    /// The proportion of trials so far in which the event happened, against the trial number (on
    /// a log scale, so the wild start and the settled end both show), with the theoretical
    /// probability as a horizontal line: the law of large numbers.
    /// </summary>
    public static PlotSpec RunningProportion(string title, IReadOnlyList<(double Trial, double Proportion)> points, double theory)
    {
        List<PlotSeries> series = [];

        if (points.Count > 0 && !double.IsNaN(theory))
        {
            series.Add(new LineSeries("Theoretical probability", AxisSide.Left, [1.0, points[^1].Trial], [theory, theory])
            {
                ColorHint = "#C0392B",
                Width = 2,
            });
        }

        series.Add(new LineSeries("Proportion so far", AxisSide.Left, [.. points.Select(p => p.Trial)], [.. points.Select(p => p.Proportion)])
        {
            ColorHint = "#3B6EA5",
            Width = 1.5,
        });

        return new PlotSpec
        {
            Title = title,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, "trial (log scale)", Min: 1, Logarithmic: true),
                new AxisSpec(AxisSide.Left, "proportion of trials with the event", Min: 0, Max: 1),
            ],
            Series = series,
        };
    }

    /// <summary>
    /// The share of trials giving each outcome (bars) beside the theoretical probability of that
    /// outcome (narrower bars), with the outcomes that count as the event in a darker colour.
    /// </summary>
    public static PlotSpec Outcomes(string title, string outcomeName, IReadOnlyList<(int Outcome, double Share, double Theory, bool IsEvent)> rows)
    {
        List<PlotSeries> series =
        [
            new BarSeries("Simulated share", AxisSide.Left, [.. rows.Where(r => !r.IsEvent).Select(r => r.Outcome - 0.2)], [.. rows.Where(r => !r.IsEvent).Select(r => r.Share)], 0.4) { ColorHint = "#B8C4D0" },
            new BarSeries("Simulated share, the event", AxisSide.Left, [.. rows.Where(r => r.IsEvent).Select(r => r.Outcome - 0.2)], [.. rows.Where(r => r.IsEvent).Select(r => r.Share)], 0.4) { ColorHint = "#3B6EA5" },
            new BarSeries("Theoretical probability", AxisSide.Left, [.. rows.Select(r => r.Outcome + 0.2)], [.. rows.Select(r => r.Theory)], 0.4) { ColorHint = "#C0392B" },
        ];

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, outcomeName), new AxisSpec(AxisSide.Left, "probability", Min: 0)],
            Series = WithoutEmpty(series),
        };
    }

    /// <summary>The first darts on the square, inside the circle in blue and outside in red, with the circle drawn.</summary>
    public static PlotSpec Darts(string title, IReadOnlyList<(double X, double Y, bool Inside)> darts)
    {
        List<double> cx = [], cy = [];

        for (int i = 0; i <= 200; i++)
        {
            double a = 2 * Math.PI * i / 200;
            cx.Add(Math.Cos(a));
            cy.Add(Math.Sin(a));
        }

        return new PlotSpec
        {
            Title = title,
            SquareAxes = true,
            Axes = [new AxisSpec(AxisSide.Bottom, "x", Min: -1.05, Max: 1.05), new AxisSpec(AxisSide.Left, "y", Min: -1.05, Max: 1.05)],
            Series = WithoutEmpty(
            [
                new LineSeries("Square", AxisSide.Left, [-1.0, 1, 1, -1, -1], [-1.0, -1, 1, 1, -1]) { ColorHint = "#1F2933", Width = 1.5 },
                new LineSeries("Circle", AxisSide.Left, cx, cy) { ColorHint = "#1F2933", Width = 1.5 },
                new MarkerSeries("Inside", AxisSide.Left, [.. darts.Where(d => d.Inside).Select(d => d.X)], [.. darts.Where(d => d.Inside).Select(d => d.Y)]) { ColorHint = "#3B6EA5", Size = 3 },
                new MarkerSeries("Outside", AxisSide.Left, [.. darts.Where(d => !d.Inside).Select(d => d.X)], [.. darts.Where(d => !d.Inside).Select(d => d.Y)]) { ColorHint = "#C0392B", Size = 3 },
            ]),
        };
    }

    private static IReadOnlyList<PlotSeries> WithoutEmpty(IEnumerable<PlotSeries> series) =>
        [.. series.Where(s => s is not BarSeries { X.Count: 0 } and not MarkerSeries { X.Count: 0 })];

    /// <summary>
    /// The first intervals, one per row, those containing μ in blue and those missing it in red,
    /// with μ as a vertical line: about one in twenty misses at 95 %.
    /// </summary>
    public static PlotSpec Intervals(string title, IReadOnlyList<(double Lower, double Upper, bool Covers)> intervals, double mu)
    {
        List<PlotSeries> series = [];

        for (int i = 0; i < intervals.Count; i++)
        {
            (double lower, double upper, bool covers) = intervals[i];
            series.Add(new LineSeries(covers ? "Contains μ" : "Misses μ", AxisSide.Left, [lower, upper], [i + 1.0, i + 1.0])
            {
                ColorHint = covers ? "#3B6EA5" : "#C0392B",
                Width = covers ? 1.5 : 2.5,
            });
        }

        series.Add(new LineSeries("μ", AxisSide.Left, [mu, mu], [0.0, intervals.Count + 1.0]) { ColorHint = "#1F2933", Width = 2 });

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes = [new AxisSpec(AxisSide.Bottom, "value"), new AxisSpec(AxisSide.Left, "sample", Min: 0, Max: intervals.Count + 1)],
            Series = series,
        };
    }

    /// <summary>
    /// The histogram of the sample means with the normal curve the central limit theorem
    /// predicts — mean μ, standard deviation σ/√n — scaled to the same area.
    /// </summary>
    public static PlotSpec SampleMeans(string title, IReadOnlyList<double> means, int classes, double mu, double standardError)
    {
        IReadOnlyList<FrequencyClass> table = SummaryStatistics.FrequencyTable(means, classes);
        List<PlotSeries> series = [];

        if (table.Count > 0)
        {
            double width = table[0].Upper - table[0].Lower;
            series.Add(new BarSeries("Sample means", AxisSide.Left, [.. table.Select(c => c.Mark)], [.. table.Select(c => (double)c.Frequency)], width)
            {
                ColorHint = "#3B6EA5",
            });

            if (standardError > 0 && width > 0)
            {
                double from = Math.Min(table[0].Lower, mu - 4 * standardError), to = Math.Max(table[^1].Upper, mu + 4 * standardError);
                List<double> xs = [], ys = [];

                for (int i = 0; i <= 160; i++)
                {
                    double x = from + (to - from) * i / 160;
                    double z = (x - mu) / standardError;
                    xs.Add(x);
                    ys.Add(means.Count * width * Math.Exp(-0.5 * z * z) / (standardError * Math.Sqrt(2 * Math.PI)));
                }

                series.Add(new LineSeries("Central limit theorem: normal, mean μ, sd σ/√n", AxisSide.Left, xs, ys) { ColorHint = "#C0392B", Width = 2 });
            }
        }

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "sample mean x̄"), new AxisSpec(AxisSide.Left, "number of samples")],
            Series = series,
        };
    }
}
