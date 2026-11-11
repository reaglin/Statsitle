using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace Statistle.Core.Methods.Inference;

/// <summary>A chi-square test's numbers: the counts, the expected counts, χ², df, p and the critical value.</summary>
public sealed record ChiSquareOutcome(
    double Statistic,
    double DegreesOfFreedom,
    double PValue,
    double Critical,
    double Total,
    double SmallestExpected,
    int CellsBelowFive,
    bool YatesApplied);

/// <summary>
/// Pearson's chi-square tests (Montgomery and Runger §9-7 and §9-8): goodness of fit to stated
/// proportions, and independence (or homogeneity) in an r × c contingency table. Without a
/// continuity correction unless Yates is asked for on a 2 × 2 table, as scipy's
/// <c>chi2_contingency(..., correction=False)</c> and R's <c>chisq.test(correct = FALSE)</c>.
/// </summary>
public static class ChiSquare
{
    /// <summary>The upper-tail probability P(χ²(df) ≥ x), accurate for very small p.</summary>
    public static double UpperTail(double df, double x) =>
        x <= 0 ? 1 : SpecialFunctions.GammaUpperRegularized(df / 2, x / 2);

    /// <summary>χ²_{α, df}: the value with α above it.</summary>
    public static double Critical(double df, double alpha) => ChiSquared.InvCDF(df, 1 - alpha);

    /// <summary>
    /// Rescales weights to proportions that add to 1. Null when a weight is not positive or finite.
    /// </summary>
    public static double[]? Proportions(IReadOnlyList<double> weights)
    {
        ArgumentNullException.ThrowIfNull(weights);

        if (weights.Count == 0 || weights.Any(w => !double.IsFinite(w) || w <= 0))
        {
            return null;
        }

        double sum = weights.Sum();
        return [.. weights.Select(w => w / sum)];
    }

    /// <summary>
    /// Goodness of fit: χ² = Σ(Oᵢ − Eᵢ)²/Eᵢ with Eᵢ = n·pᵢ, on k − 1 − m degrees of freedom
    /// (m parameters estimated from the data). The proportions must add to 1.
    /// </summary>
    public static ChiSquareOutcome GoodnessOfFit(IReadOnlyList<double> observed, IReadOnlyList<double> proportions, int estimatedParameters, double alpha, out double[] expected)
    {
        ArgumentNullException.ThrowIfNull(observed);
        ArgumentNullException.ThrowIfNull(proportions);

        double n = observed.Sum();
        expected = [.. proportions.Select(p => n * p)];
        double statistic = 0;

        for (int i = 0; i < observed.Count; i++)
        {
            statistic += (observed[i] - expected[i]) * (observed[i] - expected[i]) / expected[i];
        }

        double df = observed.Count - 1 - estimatedParameters;
        return new ChiSquareOutcome(statistic, df, UpperTail(df, statistic), Critical(df, alpha), n, expected.Min(), expected.Count(e => e < 5), false);
    }

    /// <summary>
    /// Eᵢⱼ = (row i total)(column j total)/n: the counts expected when the row and column
    /// classifications are independent.
    /// </summary>
    public static double[,] Expected(double[,] counts)
    {
        ArgumentNullException.ThrowIfNull(counts);

        int r = counts.GetLength(0), c = counts.GetLength(1);
        double[] rows = new double[r], columns = new double[c];
        double n = 0;

        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                rows[i] += counts[i, j];
                columns[j] += counts[i, j];
                n += counts[i, j];
            }
        }

        double[,] expected = new double[r, c];

        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                expected[i, j] = rows[i] * columns[j] / n;
            }
        }

        return expected;
    }

    /// <summary>
    /// The test of independence: χ² = ΣΣ(Oᵢⱼ − Eᵢⱼ)²/Eᵢⱼ on (r − 1)(c − 1) degrees of freedom.
    /// With <paramref name="yates"/> on a 2 × 2 table each |O − E| is reduced by min(0.5, |O − E|)
    /// first, as scipy (correction=True) and R (correct = TRUE) do; on a larger table it is ignored.
    /// </summary>
    public static ChiSquareOutcome Independence(double[,] counts, bool yates, double alpha, out double[,] expected, out double uncorrected)
    {
        ArgumentNullException.ThrowIfNull(counts);

        int r = counts.GetLength(0), c = counts.GetLength(1);
        expected = Expected(counts);
        bool applyYates = yates && r == 2 && c == 2;
        double statistic = 0, plain = 0, n = 0, smallest = double.PositiveInfinity;
        int belowFive = 0;

        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                double e = expected[i, j], d = Math.Abs(counts[i, j] - e);
                double adjusted = applyYates ? d - Math.Min(0.5, d) : d;
                statistic += adjusted * adjusted / e;
                plain += d * d / e;
                n += counts[i, j];
                smallest = Math.Min(smallest, e);
                belowFive += e < 5 ? 1 : 0;
            }
        }

        uncorrected = plain;
        double df = (r - 1) * (c - 1);
        return new ChiSquareOutcome(statistic, df, UpperTail(df, statistic), Critical(df, alpha), n, smallest, belowFive, applyYates);
    }

    /// <summary>Cramér's V = √(χ²/(n·(min(r, c) − 1))), from the uncorrected χ²: 0 for independence, 1 for complete association.</summary>
    public static double CramersV(double chiSquare, double n, int rows, int columns) =>
        Math.Sqrt(chiSquare / (n * (Math.Min(rows, columns) - 1)));
}
