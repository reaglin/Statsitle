namespace Statistle.Core.Methods.Distributions;

/// <summary>
/// Maximum-likelihood fits of the continuous families to a sample, and two measures of how well
/// a fitted model matches the sample: the correlation of its Q–Q plot and the Kolmogorov–Smirnov
/// distance D. Each estimate matches scipy.stats' <c>fit</c> (with the location fixed at 0 where
/// the family starts at 0); see METHODS-AND-SOURCES.md.
/// </summary>
public static class ContinuousFit
{
    /// <summary>μ̂ = x̄ and σ̂ = √(Σ(x − x̄)²/n), the maximum-likelihood σ (scipy <c>norm.fit</c>), not s.</summary>
    public static (double Mu, double Sigma) Normal(IReadOnlyList<double> x)
    {
        Require(x, 2);
        double mean = x.Average();
        double sigma = Math.Sqrt(x.Sum(v => (v - mean) * (v - mean)) / x.Count);

        if (sigma <= 0)
        {
            throw new ArgumentException("All the values are the same, so there is no spread to fit a normal to.");
        }

        return (mean, sigma);
    }

    /// <summary>λ̂ = 1/x̄ (scipy <c>expon.fit(x, floc=0)</c> gives the scale x̄).</summary>
    public static double Exponential(IReadOnlyList<double> x)
    {
        Require(x, 1);

        if (x.Any(v => v < 0))
        {
            throw new ArgumentException("An exponential variable cannot be negative, and the column has a negative value.");
        }

        double mean = x.Average();

        if (mean <= 0)
        {
            throw new ArgumentException("The values are all zero, so no rate can be fitted.");
        }

        return 1 / mean;
    }

    /// <summary>â = min, b̂ = max (scipy <c>uniform.fit</c>: loc = min, scale = max − min).</summary>
    public static (double A, double B) Uniform(IReadOnlyList<double> x)
    {
        Require(x, 2);
        double a = x.Min(), b = x.Max();

        if (b <= a)
        {
            throw new ArgumentException("All the values are the same, so there is no range to fit a uniform to.");
        }

        return (a, b);
    }

    /// <summary>The mean and maximum-likelihood standard deviation (divide by n) of ln x (scipy <c>lognorm.fit(x, floc=0)</c>).</summary>
    public static (double Mu, double Sigma) LogNormal(IReadOnlyList<double> x)
    {
        Positive(x, "lognormal");
        double[] logs = [.. x.Select(v => Math.Log(v))];
        double mean = logs.Average();
        double sigma = Math.Sqrt(logs.Sum(v => (v - mean) * (v - mean)) / logs.Length);

        if (sigma <= 0)
        {
            throw new ArgumentException("All the values are the same, so there is no spread to fit a lognormal to.");
        }

        return (mean, sigma);
    }

    /// <summary>
    /// The Weibull's maximum-likelihood β̂ and η̂: β̂ solves the shape equation
    /// Σxᵢ^β ln xᵢ / Σxᵢ^β − 1/β − (1/n)Σ ln xᵢ = 0 (increasing in β, so Newton's steps kept
    /// inside a bracket always converge), then η̂ = ((1/n)Σxᵢ^β̂)^(1/β̂). The values are divided by
    /// their mean first, which leaves β̂ unchanged and keeps xᵢ^β from overflowing. scipy
    /// <c>weibull_min.fit(x, floc=0)</c> (a general optimizer) agrees to about six figures.
    /// </summary>
    public static (double Shape, double Scale) Weibull(IReadOnlyList<double> x)
    {
        Positive(x, "Weibull");
        double centre = x.Average();
        double[] u = [.. x.Select(v => v / centre)];
        double[] logs = [.. u.Select(v => Math.Log(v))];
        double meanLog = logs.Average();

        if (logs.All(l => Math.Abs(l - logs[0]) < 1e-12))
        {
            throw new ArgumentException("All the values are the same, so there is no spread to fit a Weibull to.");
        }

        (double g, double slope) Equation(double beta)
        {
            double b = 0, a = 0, c = 0;

            for (int i = 0; i < u.Length; i++)
            {
                double p = Math.Pow(u[i], beta);
                b += p;
                a += p * logs[i];
                c += p * logs[i] * logs[i];
            }

            return (a / b - 1 / beta - meanLog, (c * b - a * a) / (b * b) + 1 / (beta * beta));
        }

        double lo = 1e-3, hi = 1.0;

        while (Equation(hi).g < 0 && hi < 1e4)
        {
            lo = hi;
            hi *= 2;
        }

        // Menon's start: β ≈ π / (√6 · sd of ln x), kept inside the bracket.
        double sdLog = Math.Sqrt(logs.Sum(l => (l - meanLog) * (l - meanLog)) / Math.Max(1, logs.Length - 1));
        double beta = Math.Clamp(Math.PI / (Math.Sqrt(6) * sdLog), lo, hi);

        for (int iteration = 0; iteration < 200; iteration++)
        {
            (double g, double slope) = Equation(beta);

            if (g < 0)
            {
                lo = beta;
            }
            else
            {
                hi = beta;
            }

            double next = beta - g / slope;

            if (double.IsNaN(next) || next <= lo || next >= hi)
            {
                next = (lo + hi) / 2;   // Newton left the bracket: bisect instead
            }

            if (Math.Abs(next - beta) <= 1e-14 * beta)
            {
                beta = next;
                break;
            }

            beta = next;
        }

        double scale = centre * Math.Pow(u.Average(v => Math.Pow(v, beta)), 1 / beta);
        return (beta, scale);
    }

    /// <summary>The plotting positions (i − 0.5)/n of the course's probability plot (M&amp;R §6-6).</summary>
    public static double[] PlottingPositions(int n) => [.. Enumerable.Range(1, n).Select(i => (i - 0.5) / n)];

    /// <summary>
    /// The correlation between the sorted sample and the model's quantiles at (i − 0.5)/n: the
    /// straightness of the Q–Q plot. Near 1 when the model fits.
    /// </summary>
    public static double QqCorrelation(IReadOnlyList<double> x, ContinuousModel model)
    {
        double[] sorted = [.. x.Order()];
        double[] theory = [.. PlottingPositions(sorted.Length).Select(model.Quantile)];
        return MathNet.Numerics.Statistics.Correlation.Pearson(theory, sorted);
    }

    /// <summary>
    /// D = the largest distance between the sample's empirical CDF and the model's CDF
    /// (scipy <c>kstest(x, cdf).statistic</c>, R <c>ks.test(x, cdf)$statistic</c>).
    /// </summary>
    public static double KolmogorovSmirnovD(IReadOnlyList<double> x, ContinuousModel model)
    {
        double[] sorted = [.. x.Order()];
        int n = sorted.Length;
        double d = 0;

        for (int i = 0; i < n; i++)
        {
            double f = model.Cdf(sorted[i]);
            d = Math.Max(d, Math.Max((i + 1.0) / n - f, f - (double)i / n));
        }

        return d;
    }

    private static void Require(IReadOnlyList<double> x, int least)
    {
        ArgumentNullException.ThrowIfNull(x);

        if (x.Count < least)
        {
            throw new ArgumentException($"A fit needs at least {least} values in the column.");
        }
    }

    private static void Positive(IReadOnlyList<double> x, string family)
    {
        Require(x, 2);

        if (x.Any(v => v <= 0))
        {
            throw new ArgumentException($"A {family} variable is always above zero, and the column has a value of zero or below.");
        }
    }
}
