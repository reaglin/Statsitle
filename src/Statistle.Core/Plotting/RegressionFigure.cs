using Statistle.Core.Methods.Statistics;
using Statistle.Core.Persistence;

namespace Statistle.Core.Plotting;

/// <summary>Observations as points, with a fitted form drawn through their x range.</summary>
public static class RegressionFigure
{
    private const int CurveSamples = 120;

    public static PlotSpec Build(DataSeries data, RegressionResult? fit, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        double[] xs = data.XValues;
        double[] ys = data.YValues;

        double xMin = xs.Length > 0 ? xs.Min() : 0;
        double xMax = xs.Length > 0 ? xs.Max() : 1;
        double yMin = ys.Length > 0 ? Math.Min(ys.Min(), 0) : 0;
        double yMax = ys.Length > 0 ? ys.Max() : 1;

        List<PlotSeries> series =
        [
            new MarkerSeries("Observations", AxisSide.Left, xs, ys) { ColorHint = "#1F2933" },
        ];

        string heading = string.IsNullOrWhiteSpace(title) ? "Regression" : title;

        if (fit is not null && xMax > xMin)
        {
            List<double> curveX = [];
            List<double> curveY = [];

            for (int i = 0; i <= CurveSamples; i++)
            {
                double x = xMin + ((xMax - xMin) * i / CurveSamples);
                double y = fit.Predict(x);

                if (double.IsFinite(y))
                {
                    curveX.Add(x);
                    curveY.Add(y);
                }
            }

            if (curveX.Count > 1)
            {
                series.Add(new LineSeries(fit.Equation, AxisSide.Left, curveX, curveY) { ColorHint = "#C0392B" });

                // Let the fitted curve set the vertical range too, but not run away with it.
                double curveMax = curveY.Max();
                double curveMin = curveY.Min();
                yMax = Math.Max(yMax, Math.Min(curveMax, yMax * 1.5));
                yMin = Math.Min(yMin, Math.Max(curveMin, yMin - (yMax - yMin) * 0.5));
            }

            heading = $"{heading} — {fit.Equation}   r² = {fit.RSquared:F4}";
        }

        double xPad = (xMax - xMin) * 0.05;
        double yPad = (yMax - yMin) * 0.08;

        return new PlotSpec
        {
            Title = heading,
            ShowLegend = true,
            Axes =
            [
                new AxisSpec(AxisSide.Bottom, "x", Min: xMin - xPad, Max: xMax + xPad),
                new AxisSpec(AxisSide.Left, "y", Min: yMin - yPad, Max: yMax + yPad),
            ],
            Series = series,
        };
    }
}
