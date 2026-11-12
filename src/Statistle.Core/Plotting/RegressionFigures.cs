using Statistle.Core.Methods.Regression;

namespace Statistle.Core.Plotting;

/// <summary>The pictures of a regression: the data with the fit, and the diagnostic plots of Module 13.</summary>
public static class RegressionFigures
{
    /// <summary>Observations as markers with the fitted curve through them (in the original units).</summary>
    public static PlotSpec Scatter(string title, string xLabel, string yLabel, IReadOnlyList<double> x, IReadOnlyList<double> y, Func<double, double>? fitted)
    {
        List<PlotSeries> series = [new MarkerSeries("Observations", AxisSide.Left, x, y) { ColorHint = "#3B6EA5" }];
        LegendCorner legend = LegendCorner.UpperRight;

        if (fitted is not null && x.Count > 1)
        {
            double low = x.Min(), high = x.Max(), pad = (high - low) * 0.03;
            List<double> cx = [], cy = [];

            for (int i = 0; i <= 120; i++)
            {
                double v = low - pad + (high - low + 2 * pad) * i / 120;
                double f = fitted(v);

                if (!double.IsNaN(f) && !double.IsInfinity(f))
                {
                    cx.Add(v);
                    cy.Add(f);
                }
            }

            series.Add(new LineSeries("Fitted", AxisSide.Left, cx, cy) { ColorHint = "#C0392B", Width = 2 });

            // A rising fit puts the highest points at the upper right, where the legend would hide them.
            if (cy.Count > 1 && cy[^1] > cy[0])
            {
                legend = LegendCorner.UpperLeft;
            }
        }

        return new PlotSpec
        {
            Title = title,
            Axes = [new AxisSpec(AxisSide.Bottom, xLabel), new AxisSpec(AxisSide.Left, yLabel)],
            Series = series,
            Legend = legend,
        };
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
