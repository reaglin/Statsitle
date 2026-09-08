using System.ComponentModel;
using MathNet.Numerics.Distributions;

namespace Statistle.Core.Methods.Inference;

/// <summary>The alternative hypothesis: which tail, or both.</summary>
public enum Alternative
{
    [Description("Two-sided (≠)")]
    TwoSided = 0,

    [Description("Less than (<)")]
    Less,

    [Description("Greater than (>)")]
    Greater,
}

/// <summary>A confidence interval with the pieces the course asks a student to show.</summary>
public sealed record Interval(
    double Estimate,
    double StandardError,
    double Critical,
    double MarginOfError,
    double Lower,
    double Upper,
    double DegreesOfFreedom = double.NaN)
{
    public double Width => Upper - Lower;
}

/// <summary>A hypothesis test's numbers: the statistic, its reference distribution, the rejection region and the p-value.</summary>
public sealed record TestOutcome(
    double Statistic,
    double DegreesOfFreedom,
    double PValue,
    double CriticalLow,
    double CriticalHigh,
    bool Reject)
{
    /// <summary>The rejection region as the course writes it.</summary>
    public string RejectionRegion(string symbol) =>
        double.IsNaN(CriticalLow) ? $"{symbol} > {CriticalHigh:F4}"
        : double.IsNaN(CriticalHigh) ? $"{symbol} < {CriticalLow:F4}"
        : $"{symbol} < {CriticalLow:F4} or {symbol} > {CriticalHigh:F4}";
}

/// <summary>
/// The confidence intervals of EGN3443 Module 7 (Montgomery &amp; Runger Ch. 8 and 10), each
/// as the course writes it: estimate ± critical × standard error.
/// </summary>
public static class Intervals
{
    public static double ZCritical(double confidence) => Normal.InvCDF(0, 1, 1 - (1 - confidence) / 2);

    public static double TCritical(double confidence, double df) => StudentT.InvCDF(0, 1, df, 1 - (1 - confidence) / 2);

    /// <summary>x̄ ± z_{α/2}·σ/√n — the population σ known.</summary>
    public static Interval MeanSigmaKnown(double mean, double sigma, int n, double confidence)
    {
        double se = sigma / Math.Sqrt(n), z = ZCritical(confidence);
        return new Interval(mean, se, z, z * se, mean - z * se, mean + z * se);
    }

    /// <summary>x̄ ± t_{α/2,n−1}·s/√n — σ estimated by s.</summary>
    public static Interval MeanSigmaUnknown(double mean, double s, int n, double confidence)
    {
        double se = s / Math.Sqrt(n), t = TCritical(confidence, n - 1);
        return new Interval(mean, se, t, t * se, mean - t * se, mean + t * se, n - 1);
    }

    /// <summary>p̂ ± z_{α/2}·√(p̂(1 − p̂)/n) — the Wald (normal approximation) interval the course teaches.</summary>
    public static Interval Proportion(int successes, int n, double confidence)
    {
        double p = (double)successes / n, se = Math.Sqrt(p * (1 - p) / n), z = ZCritical(confidence);
        return new Interval(p, se, z, z * se, Math.Max(0, p - z * se), Math.Min(1, p + z * se));
    }

    /// <summary>Wilson's score interval — what R's prop.test gives; better than Wald for small n or p near 0 or 1.</summary>
    public static Interval ProportionWilson(int successes, int n, double confidence)
    {
        double p = (double)successes / n, z = ZCritical(confidence), z2 = z * z;
        double centre = (p + z2 / (2 * n)) / (1 + z2 / n);
        double half = z * Math.Sqrt(p * (1 - p) / n + z2 / (4.0 * n * n)) / (1 + z2 / n);
        return new Interval(p, Math.Sqrt(p * (1 - p) / n), z, half, centre - half, centre + half);
    }

    /// <summary>(n − 1)s²/χ²_{α/2,n−1} ≤ σ² ≤ (n − 1)s²/χ²_{1−α/2,n−1}.</summary>
    public static Interval Variance(double s2, int n, double confidence)
    {
        double alpha = 1 - confidence, df = n - 1;
        double upperChi = ChiSquared.InvCDF(df, 1 - alpha / 2), lowerChi = ChiSquared.InvCDF(df, alpha / 2);
        double lower = df * s2 / upperChi, upper = df * s2 / lowerChi;
        return new Interval(s2, double.NaN, upperChi, double.NaN, lower, upper, df);
    }

    /// <summary>(x̄₁ − x̄₂) ± z·√(σ₁²/n₁ + σ₂²/n₂).</summary>
    public static Interval TwoMeansSigmaKnown(double mean1, double sigma1, int n1, double mean2, double sigma2, int n2, double confidence)
    {
        double se = Math.Sqrt(sigma1 * sigma1 / n1 + sigma2 * sigma2 / n2), z = ZCritical(confidence), d = mean1 - mean2;
        return new Interval(d, se, z, z * se, d - z * se, d + z * se);
    }

    /// <summary>Pooled: (x̄₁ − x̄₂) ± t_{α/2,n₁+n₂−2}·s_p√(1/n₁ + 1/n₂), s_p² = [(n₁−1)s₁² + (n₂−1)s₂²]/(n₁+n₂−2).</summary>
    public static Interval TwoMeansPooled(double mean1, double s1, int n1, double mean2, double s2, int n2, double confidence)
    {
        double sp = PooledStandardDeviation(s1, n1, s2, n2);
        double se = sp * Math.Sqrt(1.0 / n1 + 1.0 / n2), df = n1 + n2 - 2, t = TCritical(confidence, df), d = mean1 - mean2;
        return new Interval(d, se, t, t * se, d - t * se, d + t * se, df);
    }

    /// <summary>Welch: (x̄₁ − x̄₂) ± t_{α/2,ν}·√(s₁²/n₁ + s₂²/n₂) with the Welch–Satterthwaite ν.</summary>
    public static Interval TwoMeansWelch(double mean1, double s1, int n1, double mean2, double s2, int n2, double confidence)
    {
        double se = Math.Sqrt(s1 * s1 / n1 + s2 * s2 / n2), df = WelchDegreesOfFreedom(s1, n1, s2, n2), t = TCritical(confidence, df), d = mean1 - mean2;
        return new Interval(d, se, t, t * se, d - t * se, d + t * se, df);
    }

    /// <summary>(p̂₁ − p̂₂) ± z·√(p̂₁(1−p̂₁)/n₁ + p̂₂(1−p̂₂)/n₂).</summary>
    public static Interval TwoProportions(int k1, int n1, int k2, int n2, double confidence)
    {
        double p1 = (double)k1 / n1, p2 = (double)k2 / n2, d = p1 - p2;
        double se = Math.Sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2), z = ZCritical(confidence);
        return new Interval(d, se, z, z * se, d - z * se, d + z * se);
    }

    public static double PooledStandardDeviation(double s1, int n1, double s2, int n2) =>
        Math.Sqrt(((n1 - 1) * s1 * s1 + (n2 - 1) * s2 * s2) / (n1 + n2 - 2));

    public static double WelchDegreesOfFreedom(double s1, int n1, double s2, int n2)
    {
        double a = s1 * s1 / n1, b = s2 * s2 / n2;
        return (a + b) * (a + b) / (a * a / (n1 - 1) + b * b / (n2 - 1));
    }

    /// <summary>n = (z_{α/2}·σ/E)², rounded up.</summary>
    public static int SampleSizeForMean(double sigma, double margin, double confidence) =>
        (int)Math.Ceiling(Math.Pow(ZCritical(confidence) * sigma / margin, 2));

    /// <summary>n = (z_{α/2}/E)²·p(1 − p), rounded up; p = 0.5 is the conservative worst case.</summary>
    public static int SampleSizeForProportion(double p, double margin, double confidence) =>
        (int)Math.Ceiling(Math.Pow(ZCritical(confidence) / margin, 2) * p * (1 - p));
}

/// <summary>
/// The hypothesis tests of EGN3443 Modules 8–10 (Montgomery &amp; Runger Ch. 9 and 10): the
/// statistic, the rejection region at α, and the p-value for the alternative chosen.
/// </summary>
public static class HypothesisTests
{
    /// <summary>The p-value and critical values for a symmetric reference distribution (z or t).</summary>
    private static TestOutcome Symmetric(double statistic, double df, Func<double, double> cdf, Func<double, double> quantile, Alternative alternative, double alpha)
    {
        double lowerTail = cdf(statistic);
        double p = alternative switch
        {
            Alternative.Less => lowerTail,
            Alternative.Greater => 1 - lowerTail,
            _ => 2 * Math.Min(lowerTail, 1 - lowerTail),
        };
        (double low, double high) = alternative switch
        {
            Alternative.Less => (quantile(alpha), double.NaN),
            Alternative.Greater => (double.NaN, quantile(1 - alpha)),
            _ => (quantile(alpha / 2), quantile(1 - alpha / 2)),
        };

        return new TestOutcome(statistic, df, p, low, high, p < alpha);
    }

    /// <summary>A one-tailed-shaped reference distribution (χ², F): the two-sided p doubles the smaller tail.</summary>
    private static TestOutcome Skewed(double statistic, double df, Func<double, double> cdf, Func<double, double> quantile, Alternative alternative, double alpha)
    {
        double lowerTail = cdf(statistic);
        double p = alternative switch
        {
            Alternative.Less => lowerTail,
            Alternative.Greater => 1 - lowerTail,
            _ => Math.Min(1, 2 * Math.Min(lowerTail, 1 - lowerTail)),
        };
        (double low, double high) = alternative switch
        {
            Alternative.Less => (quantile(alpha), double.NaN),
            Alternative.Greater => (double.NaN, quantile(1 - alpha)),
            _ => (quantile(alpha / 2), quantile(1 - alpha / 2)),
        };

        return new TestOutcome(statistic, df, p, low, high, p < alpha);
    }

    private static TestOutcome Z(double z, Alternative alternative, double alpha) =>
        Symmetric(z, double.NaN, v => Normal.CDF(0, 1, v), q => Normal.InvCDF(0, 1, q), alternative, alpha);

    private static TestOutcome T(double t, double df, Alternative alternative, double alpha) =>
        Symmetric(t, df, v => StudentT.CDF(0, 1, df, v), q => StudentT.InvCDF(0, 1, df, q), alternative, alpha);

    /// <summary>z = (x̄ − μ₀)/(σ/√n).</summary>
    public static TestOutcome ZTestMean(double mean, double sigma, int n, double mu0, Alternative alternative, double alpha) =>
        Z((mean - mu0) / (sigma / Math.Sqrt(n)), alternative, alpha);

    /// <summary>t = (x̄ − μ₀)/(s/√n), ν = n − 1.</summary>
    public static TestOutcome TTestMean(double mean, double s, int n, double mu0, Alternative alternative, double alpha) =>
        T((mean - mu0) / (s / Math.Sqrt(n)), n - 1, alternative, alpha);

    /// <summary>z = (p̂ − p₀)/√(p₀(1 − p₀)/n), no continuity correction — the course's hand formula.</summary>
    public static TestOutcome ZTestProportion(int successes, int n, double p0, Alternative alternative, double alpha) =>
        Z(((double)successes / n - p0) / Math.Sqrt(p0 * (1 - p0) / n), alternative, alpha);

    /// <summary>χ² = (n − 1)s²/σ₀², ν = n − 1.</summary>
    public static TestOutcome ChiSquareTestVariance(double s2, int n, double sigma0Squared, Alternative alternative, double alpha)
    {
        double df = n - 1;
        return Skewed(df * s2 / sigma0Squared, df, v => ChiSquared.CDF(df, v), q => ChiSquared.InvCDF(df, q), alternative, alpha);
    }

    /// <summary>z = (x̄₁ − x̄₂ − Δ₀)/√(σ₁²/n₁ + σ₂²/n₂).</summary>
    public static TestOutcome TwoSampleZ(double mean1, double sigma1, int n1, double mean2, double sigma2, int n2, double delta0, Alternative alternative, double alpha) =>
        Z((mean1 - mean2 - delta0) / Math.Sqrt(sigma1 * sigma1 / n1 + sigma2 * sigma2 / n2), alternative, alpha);

    /// <summary>Pooled t: t = (x̄₁ − x̄₂ − Δ₀)/(s_p√(1/n₁ + 1/n₂)), ν = n₁ + n₂ − 2.</summary>
    public static TestOutcome PooledT(double mean1, double s1, int n1, double mean2, double s2, int n2, double delta0, Alternative alternative, double alpha)
    {
        double sp = Intervals.PooledStandardDeviation(s1, n1, s2, n2);
        return T((mean1 - mean2 - delta0) / (sp * Math.Sqrt(1.0 / n1 + 1.0 / n2)), n1 + n2 - 2, alternative, alpha);
    }

    /// <summary>Welch t: t = (x̄₁ − x̄₂ − Δ₀)/√(s₁²/n₁ + s₂²/n₂), ν by Welch–Satterthwaite.</summary>
    public static TestOutcome WelchT(double mean1, double s1, int n1, double mean2, double s2, int n2, double delta0, Alternative alternative, double alpha) =>
        T((mean1 - mean2 - delta0) / Math.Sqrt(s1 * s1 / n1 + s2 * s2 / n2), Intervals.WelchDegreesOfFreedom(s1, n1, s2, n2), alternative, alpha);

    /// <summary>Paired t on the differences: t = (d̄ − Δ₀)/(s_d/√n), ν = n − 1.</summary>
    public static TestOutcome PairedT(double meanDifference, double sdDifference, int n, double delta0, Alternative alternative, double alpha) =>
        T((meanDifference - delta0) / (sdDifference / Math.Sqrt(n)), n - 1, alternative, alpha);

    /// <summary>Two proportions with the pooled p̂ = (k₁ + k₂)/(n₁ + n₂): z = (p̂₁ − p̂₂)/√(p̂(1 − p̂)(1/n₁ + 1/n₂)).</summary>
    public static TestOutcome TwoProportionsZ(int k1, int n1, int k2, int n2, Alternative alternative, double alpha)
    {
        double p1 = (double)k1 / n1, p2 = (double)k2 / n2, pooled = (double)(k1 + k2) / (n1 + n2);
        return Z((p1 - p2) / Math.Sqrt(pooled * (1 - pooled) * (1.0 / n1 + 1.0 / n2)), alternative, alpha);
    }

    /// <summary>F = s₁²/s₂² with ν₁ = n₁ − 1, ν₂ = n₂ − 1.</summary>
    public static TestOutcome FTestVariances(double s1, int n1, double s2, int n2, Alternative alternative, double alpha)
    {
        double d1 = n1 - 1, d2 = n2 - 1;
        return Skewed(s1 * s1 / (s2 * s2), d1, v => FisherSnedecor.CDF(d1, d2, v), q => FisherSnedecor.InvCDF(d1, d2, q), alternative, alpha);
    }

    /// <summary>(x̄₁ − x̄₂)/s_p — Cohen's d, the difference in pooled standard deviations: 0.2 small, 0.5 medium, 0.8 large.</summary>
    public static double CohensD(double mean1, double s1, int n1, double mean2, double s2, int n2) =>
        (mean1 - mean2) / Intervals.PooledStandardDeviation(s1, n1, s2, n2);

    /// <summary>
    /// The power of a z-test on a mean (also the course's approximation for the t-test) against
    /// a true mean μ₁: 1 − β, with β = Φ(z_{α/2} − δ√n/σ) − Φ(−z_{α/2} − δ√n/σ) for the two-sided
    /// test and the matching one-tailed forms (Montgomery &amp; Runger §9-2.2).
    /// </summary>
    public static double PowerForMean(double mu0, double mu1, double sigma, int n, Alternative alternative, double alpha)
    {
        double shift = (mu1 - mu0) * Math.Sqrt(n) / sigma;

        return alternative switch
        {
            Alternative.Greater => 1 - Normal.CDF(0, 1, Normal.InvCDF(0, 1, 1 - alpha) - shift),
            Alternative.Less => Normal.CDF(0, 1, Normal.InvCDF(0, 1, alpha) - shift),
            _ => 1 - (Normal.CDF(0, 1, Normal.InvCDF(0, 1, 1 - alpha / 2) - shift) - Normal.CDF(0, 1, -Normal.InvCDF(0, 1, 1 - alpha / 2) - shift)),
        };
    }

    /// <summary>The smallest n whose power reaches the target — n = [(z_α + z_β)σ/δ]² for one tail, z_{α/2} for two, then checked.</summary>
    public static int SampleSizeForPower(double mu0, double mu1, double sigma, Alternative alternative, double alpha, double targetPower)
    {
        double delta = Math.Abs(mu1 - mu0);
        double za = Normal.InvCDF(0, 1, 1 - (alternative == Alternative.TwoSided ? alpha / 2 : alpha));
        double zb = Normal.InvCDF(0, 1, targetPower);
        int n = Math.Max(1, (int)Math.Ceiling(Math.Pow((za + zb) * sigma / delta, 2)));

        while (n < 1_000_000 && PowerForMean(mu0, mu1, sigma, n, alternative, alpha) < targetPower)
        {
            n++;
        }

        return n;
    }
}
