using Statistle.Core.Methods.Distributions;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of a distribution: the mass or density with the probability asked for shaded, and the cumulative curve.</summary>
public static class DistributionFigures
{
    /// <summary>The PMF as bars, with the values from <paramref name="from"/> to <paramref name="to"/> highlighted.</summary>
    public static PlotSpec Pmf(string title, DiscreteModel model, int from, int to)
    {
        List<double> xs = [], ys = [], hx = [], hy = [];

        for (int k = model.SupportMin; k <= model.SupportMax; k++)
        {
            double p = model.Pmf(k);
            xs.Add(k);
            ys.Add(p);

            if (k >= from && k <= to)
            {
                hx.Add(k);
                hy.Add(p);
            }
        }

        List<PlotSeries> series = [new BarSeries("P(X = k)", AxisSide.Left, xs, ys, 0.8) { ColorHint = "#B8C4D0" }];

        if (hx.Count > 0)
        {
            series.Add(new BarSeries("The probability asked for", AxisSide.Left, hx, hy, 0.8) { ColorHint = "#3B6EA5" });
        }

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "k"), new AxisSpec(AxisSide.Left, "probability", Min: 0)],
            Series = series,
        };
    }

    /// <summary>The CDF as a step line, with the value k marked.</summary>
    public static PlotSpec Cdf(string title, DiscreteModel model, int k)
    {
        List<double> xs = [], ys = [];

        for (int i = model.SupportMin; i <= model.SupportMax; i++)
        {
            double p = model.Cdf(i);
            xs.Add(i);
            ys.Add(i == model.SupportMin ? 0 : model.Cdf(i - 1));
            xs.Add(i);
            ys.Add(p);
            xs.Add(i + 1);
            ys.Add(p);
        }

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes = [new AxisSpec(AxisSide.Bottom, "k"), new AxisSpec(AxisSide.Left, "P(X ≤ k)", Min: 0, Max: 1.05)],
            Series =
            [
                new LineSeries("P(X ≤ k)", AxisSide.Left, xs, ys) { ColorHint = "#1F2933", Width = 2 },
                new LineSeries("k", AxisSide.Left, [k, k], [0, 1]) { ColorHint = "#C0392B", Width = 1.5 },
            ],
        };
    }

    /// <summary>The density curve with the area from <paramref name="from"/> to <paramref name="to"/> shaded.</summary>
    public static PlotSpec Density(string title, ContinuousModel model, double from, double to)
    {
        (double low, double high) = model.PlotRange;
        List<double> xs = [], ys = [];

        for (int i = 0; i <= 300; i++)
        {
            double x = low + (high - low) * i / 300;
            xs.Add(x);
            ys.Add(model.Density(x));
        }

        List<PlotSeries> series = [];

        if (!double.IsNaN(from) && !double.IsNaN(to) && to > from)
        {
            double a = Math.Max(from, low), b = Math.Min(to, high);

            if (b > a)
            {
                List<double> px = [a], py = [0];

                for (int i = 0; i <= 120; i++)
                {
                    double x = a + (b - a) * i / 120;
                    px.Add(x);
                    py.Add(model.Density(x));
                }

                px.Add(b);
                py.Add(0);
                series.Add(new PolygonSeries("The probability asked for", AxisSide.Left, px, py) { ColorHint = "#3B6EA5" });
            }
        }

        series.Add(new LineSeries("f(x)", AxisSide.Left, xs, ys) { ColorHint = "#1F2933", Width = 2 });

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "x"), new AxisSpec(AxisSide.Left, "density", Min: 0)],
            Series = series,
        };
    }

    /// <summary>The cumulative curve with x marked.</summary>
    public static PlotSpec Cumulative(string title, ContinuousModel model, double x)
    {
        (double low, double high) = model.PlotRange;
        List<double> xs = [], ys = [];

        for (int i = 0; i <= 300; i++)
        {
            double v = low + (high - low) * i / 300;
            xs.Add(v);
            ys.Add(model.Cdf(v));
        }

        List<PlotSeries> series = [new LineSeries("P(X ≤ x)", AxisSide.Left, xs, ys) { ColorHint = "#1F2933", Width = 2 }];

        if (!double.IsNaN(x))
        {
            series.Add(new LineSeries("x", AxisSide.Left, [x, x], [0, 1]) { ColorHint = "#C0392B", Width = 1.5 });
        }

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes = [new AxisSpec(AxisSide.Bottom, "x"), new AxisSpec(AxisSide.Left, "P(X ≤ x)", Min: 0, Max: 1.05)],
            Series = series,
        };
    }

    /// <summary>
    /// The population and the sampling distribution of the mean on one axis: the same centre,
    /// the sampling distribution narrower by √n, with the probability asked for shaded.
    /// </summary>
    public static PlotSpec SamplingOfTheMean(string title, double mu, double sigma, int n, double from, double to)
    {
        var population = new NormalModel(mu, sigma);
        var sampling = new NormalModel(mu, sigma / Math.Sqrt(n));
        double low = mu - 4 * sigma, high = mu + 4 * sigma;
        List<double> xs = [], pop = [], samp = [];

        for (int i = 0; i <= 300; i++)
        {
            double x = low + (high - low) * i / 300;
            xs.Add(x);
            pop.Add(population.Density(x));
            samp.Add(sampling.Density(x));
        }

        List<PlotSeries> series = [];

        if (!double.IsNaN(from) && !double.IsNaN(to) && to > from)
        {
            double a = Math.Max(from, low), b = Math.Min(to, high);
            List<double> px = [a], py = [0];

            for (int i = 0; i <= 120; i++)
            {
                double x = a + (b - a) * i / 120;
                px.Add(x);
                py.Add(sampling.Density(x));
            }

            px.Add(b);
            py.Add(0);
            series.Add(new PolygonSeries("The probability asked for", AxisSide.Left, px, py) { ColorHint = "#3B6EA5" });
        }

        series.Add(new LineSeries("Population, drawn as normal (σ)", AxisSide.Left, xs, pop) { ColorHint = "#B8C4D0", Width = 2 });
        series.Add(new LineSeries($"Sample mean, n = {n} (σ/√n)", AxisSide.Left, xs, samp) { ColorHint = "#1F2933", Width = 2 });

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, "value"), new AxisSpec(AxisSide.Left, "density", Min: 0)],
            Series = series,
        };
    }
}
