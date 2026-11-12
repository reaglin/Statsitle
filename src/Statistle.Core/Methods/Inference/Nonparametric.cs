using MathNet.Numerics.Distributions;

namespace Statistle.Core.Methods.Inference;

/// <summary>A rank test's numbers as scipy reports them, so the generated Python prints the same.</summary>
public sealed record RankTest(double Statistic, double Z, double PValue, string Note);

/// <summary>A normality or equal-variance check: the statistic and its p-value.</summary>
public sealed record AssumptionCheck(double Statistic, double PValue);

/// <summary>
/// The non-parametric alternatives and assumption checks of EGN3443 Modules 9–10: Mann–Whitney
/// U and the Wilcoxon signed-rank test (normal approximations with tie corrections, as
/// scipy's <c>method="asymptotic"</c> / <c>"approx"</c>), Shapiro–Wilk (Royston's AS R94, as R
/// and scipy) and Levene's test centered on the median (Brown–Forsythe, scipy's and R's default).
/// </summary>
public static class Nonparametric
{
    /// <summary>Mid-ranks, ties averaged, and the tie term Σ(t³ − t).</summary>
    public static (double[] Ranks, double TieTerm) Rank(IReadOnlyList<double> values)
    {
        int n = values.Count;
        int[] order = [.. Enumerable.Range(0, n).OrderBy(i => values[i])];
        double[] ranks = new double[n];
        double tieTerm = 0;
        int i = 0;

        while (i < n)
        {
            int j = i;

            while (j + 1 < n && values[order[j + 1]] == values[order[i]])
            {
                j++;
            }

            double rank = (i + j) / 2.0 + 1;

            for (int k = i; k <= j; k++)
            {
                ranks[order[k]] = rank;
            }

            int t = j - i + 1;
            tieTerm += (double)t * t * t - t;
            i = j + 1;
        }

        return (ranks, tieTerm);
    }

    /// <summary>
    /// Mann–Whitney U for two independent samples, exactly as scipy's
    /// <c>mannwhitneyu(x, y, alternative=…, method="asymptotic")</c> with the continuity
    /// correction: the statistic reported is U₁ (for x).
    /// </summary>
    public static RankTest MannWhitney(IReadOnlyList<double> x, IReadOnlyList<double> y, Alternative alternative)
    {
        int n1 = x.Count, n2 = y.Count, n = n1 + n2;
        (double[] ranks, double tieTerm) = Rank([.. x, .. y]);
        double r1 = ranks.Take(n1).Sum();
        double u1 = r1 - n1 * (n1 + 1) / 2.0, u2 = (double)n1 * n2 - u1;
        double mu = n1 * n2 / 2.0;
        double sigma = Math.Sqrt(n1 * n2 / 12.0 * ((n + 1) - tieTerm / ((double)n * (n - 1))));
        (double u, int f) = alternative switch
        {
            Alternative.Greater => (u1, 1),
            Alternative.Less => (u2, 1),
            _ => (Math.Max(u1, u2), 2),
        };
        double z = (u - 0.5 - mu) / sigma;
        double p = Math.Min(1, f * (1 - Normal.CDF(0, 1, z)));

        return new RankTest(u1, z, p, $"U₁ = {u1:G6}, U₂ = {u2:G6}; normal approximation with continuity correction" + (tieTerm > 0 ? " and tie correction" : string.Empty));
    }

    /// <summary>
    /// Wilcoxon signed-rank on paired differences, as scipy's
    /// <c>wilcoxon(x, y, zero_method="wilcox", correction=False, method="approx")</c>: zero
    /// differences dropped, the statistic the smaller rank sum for a two-sided test and the
    /// positive rank sum for a one-sided one.
    /// </summary>
    public static RankTest WilcoxonSignedRank(IReadOnlyList<double> differences, Alternative alternative)
    {
        double[] d = [.. differences.Where(v => v != 0)];
        int n = d.Length;

        if (n == 0)
        {
            return new RankTest(double.NaN, double.NaN, double.NaN, "Every difference is zero.");
        }

        (double[] ranks, double tieTerm) = Rank([.. d.Select(Math.Abs)]);
        double rPlus = 0;

        for (int i = 0; i < n; i++)
        {
            if (d[i] > 0)
            {
                rPlus += ranks[i];
            }
        }

        double rMinus = n * (n + 1) / 2.0 - rPlus;
        double mn = n * (n + 1) / 4.0;
        double se = Math.Sqrt(n * (n + 1) * (2.0 * n + 1) / 24 - tieTerm / 48);
        double z = (rPlus - mn) / se;
        double p = alternative switch
        {
            Alternative.Greater => 1 - Normal.CDF(0, 1, z),
            Alternative.Less => Normal.CDF(0, 1, z),
            _ => 2 * (1 - Normal.CDF(0, 1, Math.Abs(z))),
        };
        double statistic = alternative == Alternative.TwoSided ? Math.Min(rPlus, rMinus) : rPlus;

        return new RankTest(statistic, z, Math.Min(1, p), $"W⁺ = {rPlus:G6}, W⁻ = {rMinus:G6} over {n} non-zero difference(s); normal approximation");
    }

    /// <summary>
    /// Shapiro–Wilk W and its p-value by Royston's algorithm AS R94 (1995), the one in R's
    /// <c>shapiro.test</c> and scipy's <c>shapiro</c>. Valid for 3 ≤ n ≤ 5000.
    /// </summary>
    public static AssumptionCheck ShapiroWilk(IReadOnlyList<double> sample)
    {
        int n = sample.Count;

        if (n < 3)
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        double[] x = [.. sample.Order()];
        double mean = x.Average();
        double ssq = x.Sum(v => (v - mean) * (v - mean));

        if (ssq <= 0)
        {
            return new AssumptionCheck(1, 1);
        }

        int half = n / 2;
        double[] m = new double[half];
        double summ2 = 0;

        for (int i = 1; i <= half; i++)
        {
            m[i - 1] = Normal.InvCDF(0, 1, (i - 0.375) / (n + 0.25));
            summ2 += m[i - 1] * m[i - 1];
        }

        summ2 *= 2;
        double ssumm2 = Math.Sqrt(summ2), rsn = 1 / Math.Sqrt(n);
        double[] a = new double[half];
        double a1 = Poly([0, 0.221157, -0.147981, -2.07119, 4.434685, -2.706056], rsn) - m[0] / ssumm2;

        if (n > 5)
        {
            double a2 = Poly([0, 0.042981, -0.293762, -1.752461, 5.682633, -3.582633], rsn) - m[1] / ssumm2;
            double fac = Math.Sqrt((summ2 - 2 * m[0] * m[0] - 2 * m[1] * m[1]) / (1 - 2 * a1 * a1 - 2 * a2 * a2));
            a[0] = a1;
            a[1] = a2;

            for (int i = 2; i < half; i++)
            {
                a[i] = -m[i] / fac;
            }
        }
        else
        {
            double fac = Math.Sqrt((summ2 - 2 * m[0] * m[0]) / (1 - 2 * a1 * a1));
            a[0] = a1;

            for (int i = 1; i < half; i++)
            {
                a[i] = -m[i] / fac;
            }
        }

        // W = (Σ aᵢ x₍ᵢ₎)² / Σ(x − x̄)², the coefficients antisymmetric about the middle.
        double sa = 0;

        for (int i = 0; i < half; i++)
        {
            sa += a[i] * (x[i] - x[n - 1 - i]);
        }

        double w = sa * sa / ssq;
        w = Math.Min(w, 1);
        double p;

        if (n == 3)
        {
            p = Math.Clamp(6 / Math.PI * (Math.Asin(Math.Sqrt(w)) - Math.PI / 3), 0, 1);
        }
        else if (n <= 11)
        {
            double gamma = Poly([-2.273, 0.459], n);
            double mu = Poly([0.544, -0.39978, 0.025054, -6.714e-4], n);
            double sigma = Math.Exp(Poly([1.3822, -0.77857, 0.062767, -0.0020322], n));
            double arg = gamma - Math.Log(1 - w);
            p = arg <= 0 ? 1e-99 : 1 - Normal.CDF(mu, sigma, -Math.Log(arg));
        }
        else
        {
            double ln = Math.Log(n);
            double mu = Poly([-1.5861, -0.31082, -0.083751, 0.0038915], ln);
            double sigma = Math.Exp(Poly([-0.4803, -0.082676, 0.0030302], ln));
            p = 1 - Normal.CDF(mu, sigma, Math.Log(1 - w));
        }

        return new AssumptionCheck(w, Math.Clamp(p, 0, 1));
    }

    /// <summary>
    /// Levene's test for equal variances, centered on each group's median (Brown–Forsythe) — the
    /// default of scipy's <c>levene</c> and R's <c>car::leveneTest</c>. W ~ F(k − 1, N − k).
    /// </summary>
    public static AssumptionCheck Levene(IReadOnlyList<IReadOnlyList<double>> groups)
    {
        int k = groups.Count, total = groups.Sum(g => g.Count);

        if (k < 2 || groups.Any(g => g.Count < 2))
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        List<double[]> z = [];

        foreach (IReadOnlyList<double> g in groups)
        {
            double median = Descriptive.SummaryStatistics.Median(g);
            z.Add([.. g.Select(v => Math.Abs(v - median))]);
        }

        double grand = z.SelectMany(v => v).Average();
        double between = z.Sum(v => v.Length * Math.Pow(v.Average() - grand, 2));
        double within = z.Sum(v => v.Sum(u => Math.Pow(u - v.Average(), 2)));

        if (within <= 0)
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        double w = (total - k) / (k - 1.0) * between / within;
        return new AssumptionCheck(w, 1 - FisherSnedecor.CDF(k - 1, total - k, w));
    }

    private static double Poly(double[] c, double x)
    {
        double result = c[^1];

        for (int i = c.Length - 2; i >= 0; i--)
        {
            result = result * x + c[i];
        }

        return result;
    }
}
