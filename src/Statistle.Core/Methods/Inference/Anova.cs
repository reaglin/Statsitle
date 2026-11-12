using MathNet.Numerics.Distributions;
using Statistle.Core.Methods.Descriptive;
using Statistle.Core.Methods.Distributions;

namespace Statistle.Core.Methods.Inference;

/// <summary>One group's summary in an analysis of variance.</summary>
public sealed record GroupSummary(string Name, int N, double Mean, double StandardDeviation);

/// <summary>The one-way ANOVA table and what is read off it.</summary>
public sealed record OneWayResult(
    IReadOnlyList<GroupSummary> Groups,
    int N,
    double GrandMean,
    double SsBetween,
    double SsWithin,
    double SsTotal,
    double DfBetween,
    double DfWithin,
    double MsBetween,
    double MsWithin,
    double F,
    double PValue,
    double EtaSquared,
    double OmegaSquared,
    double[] Fitted,
    double[] Residuals);

/// <summary>Welch's one-way ANOVA: F*, its numerator and (adjusted) denominator df, and the p-value.</summary>
public sealed record WelchResult(double F, double DfNumerator, double DfDenominator, double PValue)
{
    public static WelchResult None { get; } = new(double.NaN, double.NaN, double.NaN, double.NaN);
}

/// <summary>One pairwise comparison of Tukey's honestly significant difference.</summary>
public sealed record TukeyPair(string A, string B, double Difference, double StandardError, double Q, double PValue, double Lower, double Upper, bool Significant);

/// <summary>The two-factor ANOVA table for a balanced design (equal replicates in every cell).</summary>
public sealed record TwoWayResult(
    IReadOnlyList<double> LevelsA,
    IReadOnlyList<double> LevelsB,
    int Replicates,
    int N,
    double GrandMean,
    double[] MeansA,
    double[] MeansB,
    double[,] CellMeans,
    double SsA,
    double SsB,
    double SsAB,
    double SsError,
    double SsTotal,
    double DfA,
    double DfB,
    double DfAB,
    double DfError,
    double FA,
    double FB,
    double FAB,
    double PA,
    double PB,
    double PAB,
    double[] Fitted,
    double[] Residuals)
{
    public double MsA => SsA / DfA;

    public double MsB => SsB / DfB;

    public double MsAB => DfAB > 0 ? SsAB / DfAB : double.NaN;

    public double MsError => SsError / DfError;

    /// <summary>With one observation per cell the interaction cannot be separated from error: the additive model is fitted.</summary>
    public bool Additive => Replicates == 1;
}

/// <summary>Analysis of variance, EGN3443 Module 14 (Montgomery and Runger Chapters 13 and 14).</summary>
public static class Anova
{
    /// <summary>
    /// The single-factor ANOVA: SS_Treatments = Σnᵢ(ȳᵢ − ȳ)², SS_E = ΣΣ(yᵢⱼ − ȳᵢ)², F = MS_Treatments/MS_E
    /// on k − 1 and N − k degrees of freedom. Null when fewer than two groups, a group of fewer
    /// than two values, or no within-group variation.
    /// </summary>
    public static OneWayResult? OneWay(IReadOnlyList<string> names, IReadOnlyList<IReadOnlyList<double>> groups)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(groups);
        int k = groups.Count;

        if (k < 2 || groups.Any(g => g.Count < 2) || groups.Any(g => g.Any(double.IsNaN)))
        {
            return null;
        }

        int n = groups.Sum(g => g.Count);
        double grand = groups.SelectMany(g => g).Sum() / n;
        List<GroupSummary> summaries = [];
        double ssBetween = 0, ssWithin = 0;
        List<double> fitted = [], residuals = [];

        for (int i = 0; i < k; i++)
        {
            IReadOnlyList<double> g = groups[i];
            double mean = g.Sum() / g.Count;
            summaries.Add(new GroupSummary(names[i], g.Count, mean, SummaryStatistics.SampleStandardDeviation(g)));
            ssBetween += g.Count * (mean - grand) * (mean - grand);

            foreach (double v in g)
            {
                ssWithin += (v - mean) * (v - mean);
                fitted.Add(mean);
                residuals.Add(v - mean);
            }
        }

        double dfBetween = k - 1, dfWithin = n - k;

        if (ssWithin <= 0 || dfWithin <= 0)
        {
            return null;
        }

        double msBetween = ssBetween / dfBetween, msWithin = ssWithin / dfWithin;
        double f = msBetween / msWithin;
        double p = 1 - FisherSnedecor.CDF(dfBetween, dfWithin, f);
        double ssTotal = ssBetween + ssWithin;
        double eta = ssTotal > 0 ? ssBetween / ssTotal : double.NaN;
        double omega = ssTotal > 0 ? Math.Max(0, (ssBetween - dfBetween * msWithin) / (ssTotal + msWithin)) : double.NaN;

        return new OneWayResult(summaries, n, grand, ssBetween, ssWithin, ssTotal, dfBetween, dfWithin, msBetween, msWithin, f, p, eta, omega, [.. fitted], [.. residuals]);
    }

    /// <summary>
    /// Tukey's HSD for every pair of groups (Tukey–Kramer for unequal sizes): the difference of
    /// means, its standard error √(MS_E/2 · (1/nᵢ + 1/nⱼ)), q = |diff|/se against the studentized
    /// range with k groups and N − k degrees of freedom, and the simultaneous confidence interval
    /// diff ± q_{α;k,ν}·se. Matches scipy's <c>tukey_hsd</c> and R's <c>TukeyHSD</c>.
    /// </summary>
    public static (IReadOnlyList<TukeyPair> Pairs, double QCritical) TukeyHsd(OneWayResult result, double alpha)
    {
        ArgumentNullException.ThrowIfNull(result);
        int k = result.Groups.Count;
        double qCritical = StudentizedRange.Quantile(1 - alpha, k, result.DfWithin);
        List<TukeyPair> pairs = [];

        for (int i = 0; i < k; i++)
        {
            for (int j = i + 1; j < k; j++)
            {
                GroupSummary a = result.Groups[i], b = result.Groups[j];
                double diff = a.Mean - b.Mean;
                double se = Math.Sqrt(result.MsWithin / 2 * (1.0 / a.N + 1.0 / b.N));
                double q = Math.Abs(diff) / se;
                double p = 1 - StudentizedRange.Cdf(q, k, result.DfWithin);
                double half = qCritical * se;
                pairs.Add(new TukeyPair(a.Name, b.Name, diff, se, q, p, diff - half, diff + half, p < alpha));
            }
        }

        return (pairs, qCritical);
    }

    /// <summary>
    /// Welch's ANOVA (Welch 1951), the one-way test of equal means that does not assume equal
    /// variances: wᵢ = nᵢ/sᵢ², W = Σwᵢ, x̄_w = Σwᵢx̄ᵢ/W, A = Σwᵢ(x̄ᵢ − x̄_w)²/(k − 1),
    /// Λ = Σ(1 − wᵢ/W)²/(nᵢ − 1), F* = A / (1 + 2(k − 2)Λ/(k² − 1)) against F(k − 1, (k² − 1)/(3Λ)).
    /// As R's <c>oneway.test(y ~ g, var.equal = FALSE)</c>. NaN when a group has fewer than two
    /// values or no spread.
    /// </summary>
    public static WelchResult Welch(IReadOnlyList<IReadOnlyList<double>> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        int k = groups.Count;

        if (k < 2 || groups.Any(g => g.Count < 2))
        {
            return WelchResult.None;
        }

        double[] n = [.. groups.Select(g => (double)g.Count)];
        double[] means = [.. groups.Select(g => g.Average())];
        double[] variances = [.. groups.Select(g => SummaryStatistics.SampleVariance(g))];

        if (variances.Any(v => !(v > 0)))
        {
            return WelchResult.None;
        }

        double[] w = [.. n.Select((ni, i) => ni / variances[i])];
        double sumW = w.Sum();
        double weightedMean = w.Select((wi, i) => wi * means[i]).Sum() / sumW;
        double a = w.Select((wi, i) => wi * (means[i] - weightedMean) * (means[i] - weightedMean)).Sum() / (k - 1);
        double lambda = w.Select((wi, i) => (1 - wi / sumW) * (1 - wi / sumW) / (n[i] - 1)).Sum();
        double f = a / (1 + 2.0 * (k - 2) * lambda / (k * k - 1.0));
        double df1 = k - 1, df2 = (k * k - 1.0) / (3 * lambda);
        return new WelchResult(f, df1, df2, 1 - FisherSnedecor.CDF(df1, df2, f));
    }

    /// <summary>
    /// Bartlett's test of equal variances: T = [(N − k) ln s_p² − Σ(nᵢ − 1) ln sᵢ²] / c, with
    /// c = 1 + [Σ1/(nᵢ − 1) − 1/(N − k)] / (3(k − 1)), against χ²(k − 1). Sensitive to
    /// non-normality, which is why Levene is reported beside it.
    /// </summary>
    public static AssumptionCheck Bartlett(IReadOnlyList<IReadOnlyList<double>> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        int k = groups.Count, n = groups.Sum(g => g.Count);

        if (k < 2 || groups.Any(g => g.Count < 2))
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        double[] variances = [.. groups.Select(g => SummaryStatistics.SampleVariance(g))];

        if (variances.Any(v => v <= 0))
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        double pooled = groups.Select((g, i) => (g.Count - 1) * variances[i]).Sum() / (n - k);
        double numerator = (n - k) * Math.Log(pooled) - groups.Select((g, i) => (g.Count - 1) * Math.Log(variances[i])).Sum();
        double c = 1 + (groups.Sum(g => 1.0 / (g.Count - 1)) - 1.0 / (n - k)) / (3.0 * (k - 1));
        double t = numerator / c;
        return new AssumptionCheck(t, 1 - ChiSquared.CDF(k - 1, t));
    }

    /// <summary>
    /// Kruskal–Wallis H on the pooled mid-ranks, corrected for ties, against χ²(k − 1): the
    /// rank-based alternative to the one-way F when normality is doubtful. As scipy's
    /// <c>kruskal</c> and R's <c>kruskal.test</c>.
    /// </summary>
    public static AssumptionCheck KruskalWallis(IReadOnlyList<IReadOnlyList<double>> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        int k = groups.Count, n = groups.Sum(g => g.Count);

        if (k < 2 || n < 3 || groups.Any(g => g.Count == 0))
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        (double[] ranks, double tieTerm) = Nonparametric.Rank([.. groups.SelectMany(g => g)]);
        double h = 0;
        int offset = 0;

        foreach (IReadOnlyList<double> g in groups)
        {
            double sum = 0;

            for (int i = 0; i < g.Count; i++)
            {
                sum += ranks[offset + i];
            }

            h += sum * sum / g.Count;
            offset += g.Count;
        }

        h = 12.0 / (n * (n + 1.0)) * h - 3 * (n + 1.0);
        double correction = 1 - tieTerm / ((double)n * n * n - n);

        if (correction <= 0)
        {
            return new AssumptionCheck(double.NaN, double.NaN);
        }

        h /= correction;
        return new AssumptionCheck(h, 1 - ChiSquared.CDF(k - 1, h));
    }

    /// <summary>
    /// The power of the one-way F test for Cohen's f = √(η²/(1 − η²)) with N observations in k
    /// groups at level α: 1 − F_nc(F_crit; k − 1, N − k, λ = f²N). As statsmodels'
    /// <c>FTestAnovaPower</c> and R's <c>pwr.anova.test</c> (which takes λ = f²·k·n).
    /// </summary>
    public static double Power(int k, int n, double cohensF, double alpha)
    {
        if (k < 2 || n <= k || double.IsNaN(cohensF) || cohensF < 0 || alpha <= 0 || alpha >= 1)
        {
            return double.NaN;
        }

        double critical = FisherSnedecor.InvCDF(k - 1, n - k, 1 - alpha);
        return 1 - NoncentralF.Cdf(critical, k - 1, n - k, cohensF * cohensF * n);
    }

    /// <summary>
    /// The two-factor ANOVA for a balanced design given as one row per observation: a level of A,
    /// a level of B, and the response. With n replicates per cell SS_A = bnΣ(ȳᵢ.. − ȳ)²,
    /// SS_B = anΣ(ȳ.ⱼ. − ȳ)², SS_AB = nΣΣ(ȳᵢⱼ. − ȳᵢ.. − ȳ.ⱼ. + ȳ)², SS_E = ΣΣΣ(y − ȳᵢⱼ.)²; each
    /// mean square is tested against MS_E. With n = 1 the interaction is not estimable and the
    /// additive model is fitted (SS_E is then what the interaction would have been). Returns the
    /// reason instead of a table when the design is not balanced.
    /// </summary>
    public static (TwoWayResult? Result, string? Problem) TwoWay(IReadOnlyList<double> factorA, IReadOnlyList<double> factorB, IReadOnlyList<double> response)
    {
        ArgumentNullException.ThrowIfNull(factorA);
        ArgumentNullException.ThrowIfNull(factorB);
        ArgumentNullException.ThrowIfNull(response);
        int n = response.Count;

        if (factorA.Count != n || factorB.Count != n)
        {
            return (null, "The two factor columns and the response must have the same number of rows.");
        }

        if (factorA.Any(double.IsNaN) || factorB.Any(double.IsNaN) || response.Any(double.IsNaN))
        {
            return (null, "Every row needs a level of A, a level of B and a response.");
        }

        double[] levelsA = [.. factorA.Distinct().Order()], levelsB = [.. factorB.Distinct().Order()];
        int a = levelsA.Length, b = levelsB.Length;

        if (a < 2 || b < 2)
        {
            return (null, "Each factor needs at least two levels.");
        }

        var cells = new List<double>[a, b];

        for (int i = 0; i < n; i++)
        {
            int ia = Array.IndexOf(levelsA, factorA[i]), ib = Array.IndexOf(levelsB, factorB[i]);
            (cells[ia, ib] ??= []).Add(response[i]);
        }

        int replicates = cells[0, 0]?.Count ?? 0;

        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                if ((cells[i, j]?.Count ?? 0) != replicates)
                {
                    return (null, $"The design is not balanced: cell A = {levelsA[i]:G6}, B = {levelsB[j]:G6} has {cells[i, j]?.Count ?? 0} observation(s) where the first cell has {replicates}. Equal replicates in every cell are needed.");
                }
            }
        }

        if (replicates < 1)
        {
            return (null, "No observations.");
        }

        double grand = response.Sum() / n;
        var cellMeans = new double[a, b];
        double[] meansA = new double[a], meansB = new double[b];

        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                cellMeans[i, j] = cells[i, j].Average();
                meansA[i] += cellMeans[i, j] / b;
                meansB[j] += cellMeans[i, j] / a;
            }
        }

        double ssA = b * replicates * meansA.Sum(m => (m - grand) * (m - grand));
        double ssB = a * replicates * meansB.Sum(m => (m - grand) * (m - grand));
        double ssAB = 0, ssE = 0, ssT = 0;
        double[] fitted = new double[n], residuals = new double[n];

        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                double interaction = cellMeans[i, j] - meansA[i] - meansB[j] + grand;
                ssAB += replicates * interaction * interaction;

                foreach (double v in cells[i, j])
                {
                    ssE += (v - cellMeans[i, j]) * (v - cellMeans[i, j]);
                }
            }
        }

        for (int r = 0; r < n; r++)
        {
            int ia = Array.IndexOf(levelsA, factorA[r]), ib = Array.IndexOf(levelsB, factorB[r]);
            fitted[r] = replicates == 1 ? meansA[ia] + meansB[ib] - grand : cellMeans[ia, ib];
            residuals[r] = response[r] - fitted[r];
            ssT += (response[r] - grand) * (response[r] - grand);
        }

        double dfA = a - 1, dfB = b - 1, dfAB = (a - 1.0) * (b - 1), dfE = (double)a * b * (replicates - 1);

        if (replicates == 1)
        {
            // Additive model: the interaction sum of squares is the error.
            ssE = ssAB;
            dfE = dfAB;
            ssAB = double.NaN;
            dfAB = 0;
        }

        if (ssE <= 0 || dfE <= 0)
        {
            return (null, "There is no residual variation to test against (every cell's values are identical).");
        }

        double msE = ssE / dfE;
        double fA = ssA / dfA / msE, fB = ssB / dfB / msE;
        double fAB = replicates == 1 ? double.NaN : ssAB / dfAB / msE;
        double pA = 1 - FisherSnedecor.CDF(dfA, dfE, fA), pB = 1 - FisherSnedecor.CDF(dfB, dfE, fB);
        double pAB = replicates == 1 ? double.NaN : 1 - FisherSnedecor.CDF(dfAB, dfE, fAB);

        return (new TwoWayResult(levelsA, levelsB, replicates, n, grand, meansA, meansB, cellMeans,
            ssA, ssB, ssAB, ssE, ssT, dfA, dfB, dfAB, dfE, fA, fB, fAB, pA, pB, pAB, fitted, residuals), null);
    }
}
