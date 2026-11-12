using Statistle.Core.Methods.Regression;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of a regression: the data with the fit, and the diagnostic plots of Module 13.</summary>
public static class RegressionFigures
{
    /// <summary>Observations as markers with the fitted curve through them (in the original units).</summary>
    public static PlotSpec Scatter(string title, string xLabel, string yLabel, IReadOnlyList<double> x, IReadOnlyList<double> y, Func<double, double>? fitted)
    {
        List<PlotSeries> series = [new MarkerSeries("Observations", AxisSide.Left, x, y) { ColorHint = "#3B6EA5" }];

        if (fitted is not null && x.Count > 1)
        {
            double low = x.Min(), high = x.Max(), pad = (high - low) * 0.03;

            // The curve runs a little past the data, but never across x = 0 from data on one side of it: a fit in
            // ln x or 1/x shoots to infinity there and squashes the data flat (Module 13, cost against 1/x).
            double from = low > 0 && low - pad <= 0 ? low : low - pad, to = high < 0 && high + pad >= 0 ? high : high + pad;
            List<double> cx = [], cy = [];

            for (int i = 0; i <= 120; i++)
            {
                double v = from + (to - from) * i / 120;
                double f = fitted(v);

                if (!double.IsNaN(f) && !double.IsInfinity(f))
                {
                    cx.Add(v);
                    cy.Add(f);
                }
            }

            series.Add(new LineSeries("Fitted", AxisSide.Left, cx, cy) { ColorHint = "#C0392B", Width = 2 });
        }

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, xLabel), new AxisSpec(AxisSide.Left, yLabel)],
            Series = series,
            Legend = Rises(x, y, fitted) ? LegendCorner.LowerRight : LegendCorner.UpperRight,
        };
    }

    /// <summary>
    /// Whether the data rise from left to right (by the fitted curve's ends, or else the sign of the covariance):
    /// then the upper right holds the last observations and the legend goes to the empty lower right
    /// (Module 13: an influential point at the largest x was hidden under it).
    /// </summary>
    public static bool Rises(IReadOnlyList<double> x, IReadOnlyList<double> y, Func<double, double>? fitted)
    {
        if (x.Count < 2)
        {
            return false;
        }

        if (fitted is not null)
        {
            double atLow = fitted(x.Min()), atHigh = fitted(x.Max());

            if (!double.IsNaN(atLow) && !double.IsNaN(atHigh))
            {
                return atHigh > atLow;
            }
        }

        double mx = x.Average(), my = y.Average();
        return x.Zip(y).Sum(p => (p.First - mx) * (p.Second - my)) > 0;
    }

    /// <summary>Residuals against fitted values with a zero line: a pattern means the model is missing something.</summary>
    public static PlotSpec ResidualsVersusFitted(string title, LinearModel model)
    {
        double low = model.Fitted.Min(), high = model.Fitted.Max();

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes = [new AxisSpec(AxisSide.Bottom, "fitted value"), new AxisSpec(AxisSide.Left, "residual")],
            Series =
            [
                new LineSeries("Zero", AxisSide.Left, [low, high], [0, 0]) { ColorHint = "#B8C4D0", Width = 1.5 },
                new MarkerSeries("Residuals", AxisSide.Left, model.Fitted, model.Residuals) { ColorHint = "#3B6EA5" },
            ],
        };
    }

    /// <summary>Normal Q–Q plot of the standardized residuals with the reference line.</summary>
    public static PlotSpec NormalQq(string title, LinearModel model)
    {
        (double[] theoretical, double[] ordered) = model.NormalQuantiles();
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

    /// <summary>Observed against predicted: a good model hugs the 45° line.</summary>
    public static PlotSpec ObservedVersusPredicted(string title, LinearModel model, IReadOnlyList<double> y)
    {
        double low = Math.Min(y.Min(), model.Fitted.Min()), high = Math.Max(y.Max(), model.Fitted.Max());

        return new PlotSpec
        {
            Title = title,
            ShowLegend = false,
            Axes = [new AxisSpec(AxisSide.Bottom, "predicted"), new AxisSpec(AxisSide.Left, "observed")],
            Series =
            [
                new LineSeries("Equality", AxisSide.Left, [low, high], [low, high]) { ColorHint = "#B8C4D0", Width = 1.5 },
                new MarkerSeries("Observations", AxisSide.Left, model.Fitted, y) { ColorHint = "#3B6EA5" },
            ],
        };
    }
}
