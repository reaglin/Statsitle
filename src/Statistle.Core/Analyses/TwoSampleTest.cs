using System.ComponentModel;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Descriptive;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum TwoSampleTestKind
{
    [Description("Two means, equal variances (pooled t)")]
    PooledT = 0,

    [Description("Two means, unequal variances (Welch t)")]
    WelchT,

    [Description("Paired samples (paired t)")]
    PairedT,

    [Description("Two means, σ₁ and σ₂ known (z)")]
    TwoSampleZ,

    [Description("Two proportions (pooled z)")]
    TwoProportions,

    [Description("Two variances (F)")]
    VarianceRatio,

    [Description("Mann–Whitney U (non-parametric, independent)")]
    MannWhitney,

    [Description("Wilcoxon signed-rank (non-parametric, paired)")]
    WilcoxonSignedRank,
}

/// <summary>
/// EGN3443 Modules 9–10: comparing two populations — means by pooled, Welch, paired or z
/// tests, two proportions, two variances by F, and the rank tests when normality cannot be
/// assumed — with the effect size and the assumption checks the course asks for.
/// </summary>
[StatistleHelp(
    "A two-sample hypothesis test (Modules 9 and 10). Choose the test: pooled t when the two " +
    "populations can be assumed to have the same variance, Welch t when not (the safe default), " +
    "paired t when each observation in one sample is matched to one in the other (before and " +
    "after on the same units), z when both σ are known, the pooled z-test for two proportions, " +
    "the F-test for two variances, or the Mann–Whitney U and Wilcoxon signed-rank tests, which " +
    "rank the data instead of assuming normality. Give summary statistics or two data table " +
    "columns; with columns the parametric tests also report Cohen's d, Shapiro–Wilk normality " +
    "checks of each sample and Levene's test of equal variances, and the rank tests are " +
    "available. Read the statistic, rejection region, p-value, decision and the confidence " +
    "interval for the difference. The Plots tab draws the two samples as box plots and the " +
    "reference distribution with the rejection region; the Code tab writes the arithmetic in " +
    "Python and R with the library calls that check it.")]
public sealed class TwoSampleTest : Analysis, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, IHasFigures, ICodeSource
{
    private TwoSampleTestKind _kind = TwoSampleTestKind.PooledT;
    private DataSource _source = DataSource.Summary;
    private Alternative _alternative = Alternative.TwoSided;
    private double _alpha = 0.05;
    private double _delta0;
    private string _column = string.Empty;
    private string _column2 = string.Empty;
    private double _sampleMean = 92.255;
    private double _sampleSd = 2.39;
    private double _sigma = 2.5;
    private double _sampleSize = 8;
    private double _successes = 27;
    private double _sample2Mean = 92.733;
    private double _sample2Sd = 2.98;
    private double _sigma2 = 2.5;
    private double _sample2Size = 8;
    private double _successes2 = 19;

    private string _hypotheses = string.Empty;
    private double _statistic = double.NaN;
    private double _degreesOfFreedom = double.NaN;
    private string _rejectionRegion = string.Empty;
    private double _pValue = double.NaN;
    private string _decision = string.Empty;
    private double _ciLower = double.NaN;
    private double _ciUpper = double.NaN;
    private double _pooledSd = double.NaN;
    private double _effectSize = double.NaN;
    private double _z = double.NaN;
    private string _note = string.Empty;
    private double _shapiro1 = double.NaN;
    private double _shapiro1P = double.NaN;
    private double _shapiro2 = double.NaN;
    private double _shapiro2P = double.NaN;
    private double _levene = double.NaN;
    private double _leveneP = double.NaN;

    public TwoSampleTest()
    {
        Name = "Two-sample test";
        Recalculate();
    }

    public override string Title => "Two-sample test";

    public override string Summary => "Pooled, Welch, paired, z, two proportions, F, Mann–Whitney, Wilcoxon (Modules 9–10)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 33;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public TestOutcome? Outcome { get; private set; }

    [StatistleIgnore]
    public bool IsPaired => TestKind is TwoSampleTestKind.PairedT or TwoSampleTestKind.WilcoxonSignedRank;

    [StatistleIgnore]
    public bool IsRankTest => TestKind is TwoSampleTestKind.MannWhitney or TwoSampleTestKind.WilcoxonSignedRank;

    [StatistleIgnore]
    public bool IsMeanTest => TestKind is TwoSampleTestKind.PooledT or TwoSampleTestKind.WelchT or TwoSampleTestKind.PairedT or TwoSampleTestKind.TwoSampleZ;

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Test", Role = VariableRole.Design,
        Help = "Which comparison. Independent samples of a measurement: pooled t (equal " +
               "variances — check with the F-test or Levene), Welch t (unequal, or unsure), or " +
               "Mann–Whitney when the data are far from normal. Paired measurements: paired t, " +
               "or Wilcoxon signed-rank. Counts of successes: two proportions. Spreads: F.")]
    public TwoSampleTestKind TestKind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
    }

    [StatistleProperty(Label = "Data from", Role = VariableRole.Design,
        Help = "Summary statistics typed here, or two columns of a data table. The rank tests " +
               "and the assumption checks need the data themselves.")]
    public DataSource Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    [StatistleProperty(Label = "Alternative hypothesis", Role = VariableRole.Design,
        Help = "Two-sided (≠) when any difference matters; 'less' or 'greater' about sample 1 " +
               "relative to sample 2 (μ₁ − μ₂ < Δ₀ or > Δ₀), only when the direction was fixed " +
               "before the data were seen.")]
    public Alternative AlternativeHypothesis
    {
        get => _alternative;
        set => SetProperty(ref _alternative, value);
    }

    [StatistleProperty(Label = "Significance level (α)", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.0001, Max = 0.5,
        Help = "The Type I error rate allowed; 0.05 by convention.")]
    public double Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    [StatistleProperty(Label = "Hypothesised difference (Δ₀)", Decimals = 4, Role = VariableRole.Input,
        Help = "The difference H₀ claims, μ₁ − μ₂ = Δ₀ — almost always 0 (no difference). " +
               "Enter a non-zero value to test for at least a given improvement.")]
    public double Delta0
    {
        get => _delta0;
        set => SetProperty(ref _delta0, value);
    }

    [StatistleProperty(Label = "Column (sample 1)", Role = VariableRole.Input,
        Help = "The data table column holding the first sample (the 'before' of a paired design; 1/0 for a proportion).")]
    public string Column
    {
        get => _column;
        set => SetProperty(ref _column, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Column (sample 2)", Role = VariableRole.Input,
        Help = "The column holding the second sample (the 'after', row by row, of a paired design).")]
    public string Column2
    {
        get => _column2;
        set => SetProperty(ref _column2, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Sample 1 values", Role = VariableRole.Input, ValuesOnly = true, Hidden = true,
        Help = "The first sample's values, copied from its column.")]
    public DataSeries Sample { get; } = new();

    [StatistleProperty(Label = "Sample 2 values", Role = VariableRole.Input, ValuesOnly = true, Hidden = true,
        Help = "The second sample's values, copied from its column.")]
    public DataSeries Sample2 { get; } = new();

    [StatistleProperty(Label = "Sample 1 mean (x̄₁)", Decimals = 4, Role = VariableRole.Input,
        Help = "The first sample's mean (the mean difference d̄, for a paired design).")]
    public double SampleMean
    {
        get => _sampleMean;
        set => SetProperty(ref _sampleMean, value);
    }

    [StatistleProperty(Label = "Sample 1 standard deviation (s₁)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The first sample's standard deviation (s_d of the differences, for a paired design).")]
    public double SampleSd
    {
        get => _sampleSd;
        set => SetProperty(ref _sampleSd, value);
    }

    [StatistleProperty(Label = "Population σ₁", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The known standard deviation of population 1, for the z-test.")]
    public double Sigma
    {
        get => _sigma;
        set => SetProperty(ref _sigma, value);
    }

    [StatistleProperty(Label = "Sample 1 size (n₁)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 2,
        Help = "How many observations in sample 1 (pairs, for a paired design).")]
    public double SampleSize
    {
        get => _sampleSize;
        set => SetProperty(ref _sampleSize, value);
    }

    [StatistleProperty(Label = "Successes in sample 1 (x₁)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0,
        Help = "How many of the n₁ items had the attribute.")]
    public double Successes
    {
        get => _successes;
        set => SetProperty(ref _successes, value);
    }

    [StatistleProperty(Label = "Sample 2 mean (x̄₂)", Decimals = 4, Role = VariableRole.Input,
        Help = "The second sample's mean, x̄₂, the other side of the difference.")]
    public double Sample2Mean
    {
        get => _sample2Mean;
        set => SetProperty(ref _sample2Mean, value);
    }

    [StatistleProperty(Label = "Sample 2 standard deviation (s₂)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The second sample's standard deviation.")]
    public double Sample2Sd
    {
        get => _sample2Sd;
        set => SetProperty(ref _sample2Sd, value);
    }

    [StatistleProperty(Label = "Population σ₂", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The known standard deviation of population 2, for the z-test.")]
    public double Sigma2
    {
        get => _sigma2;
        set => SetProperty(ref _sigma2, value);
    }

    [StatistleProperty(Label = "Sample 2 size (n₂)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 2,
        Help = "How many observations in sample 2; each sample needs at least two.")]
    public double Sample2Size
    {
        get => _sample2Size;
        set => SetProperty(ref _sample2Size, value);
    }

    [StatistleProperty(Label = "Successes in sample 2 (x₂)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0,
        Help = "How many of the n₂ items had the attribute.")]
    public double Successes2
    {
        get => _successes2;
        set => SetProperty(ref _successes2, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Hypotheses", Role = VariableRole.Output,
        Help = "H₀ and H₁ as they should be written in the report.")]
    public string Hypotheses
    {
        get => _hypotheses;
        set => SetProperty(ref _hypotheses, value);
    }

    [StatistleProperty(Label = "Test statistic", Decimals = 4, Role = VariableRole.Output,
        Help = "t, z, F, U₁ (Mann–Whitney: the U for sample 1) or W (Wilcoxon: the smaller " +
               "rank sum for a two-sided test, the positive rank sum otherwise) — the value the " +
               "libraries report.")]
    public double Statistic
    {
        get => _statistic;
        set => SetProperty(ref _statistic, value);
    }

    [StatistleProperty(Label = "Degrees of freedom", Decimals = 2, Role = VariableRole.Calculated,
        Help = "n₁ + n₂ − 2 pooled, Welch–Satterthwaite for Welch, n − 1 paired, n₁ − 1 (numerator) " +
               "for F. Blank for z and the rank tests.")]
    public double DegreesOfFreedom
    {
        get => _degreesOfFreedom;
        set => SetProperty(ref _degreesOfFreedom, value);
    }

    [StatistleProperty(Label = "Rejection region", Role = VariableRole.Output,
        Help = "The values of the statistic that reject H₀ at α. For the rank tests the region is " +
               "on the normal approximation z.")]
    public string RejectionRegion
    {
        get => _rejectionRegion;
        set => SetProperty(ref _rejectionRegion, value);
    }

    [StatistleProperty(Label = "p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability under H₀ of a difference at least as extreme as the one observed, in the direction(s) of H₁.")]
    public double PValue
    {
        get => _pValue;
        set => SetProperty(ref _pValue, value);
    }

    [StatistleProperty(Label = "Decision", Role = VariableRole.Output,
        Help = "Reject H₀ when p < α. Statistical significance is not practical significance: read the effect size too.")]
    public string Decision
    {
        get => _decision;
        set => SetProperty(ref _decision, value);
    }

    [StatistleProperty(Label = "Confidence interval lower", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The two-sided (1 − α) confidence interval for μ₁ − μ₂ (or p₁ − p₂, or σ₁²/σ₂²): " +
               "the range of differences consistent with the data. Blank for the rank tests.")]
    public double CiLower
    {
        get => _ciLower;
        set => SetProperty(ref _ciLower, value);
    }

    [StatistleProperty(Label = "Confidence interval upper", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The upper end of the (1 − α) confidence interval for the difference.")]
    public double CiUpper
    {
        get => _ciUpper;
        set => SetProperty(ref _ciUpper, value);
    }

    [StatistleProperty(Label = "Pooled standard deviation (s_p)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "√[((n₁ − 1)s₁² + (n₂ − 1)s₂²)/(n₁ + n₂ − 2)]: the common σ estimated from both samples.")]
    public double PooledSd
    {
        get => _pooledSd;
        set => SetProperty(ref _pooledSd, value);
    }

    [StatistleProperty(Label = "Effect size (Cohen's d)", Decimals = 4, Role = VariableRole.Output,
        Help = "(x̄₁ − x̄₂)/s_p: the difference in standard-deviation units — the practical " +
               "size of the effect, which the p-value does not tell you. About 0.2 is small, " +
               "0.5 medium, 0.8 large (Cohen, 1988).")]
    public double EffectSize
    {
        get => _effectSize;
        set => SetProperty(ref _effectSize, value);
    }

    [StatistleProperty(Label = "Normal approximation z", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The standardised statistic of the rank test, with continuity and tie corrections as scipy applies them.")]
    public double Z
    {
        get => _z;
        set => SetProperty(ref _z, value);
    }

    [StatistleProperty(Label = "Note", Role = VariableRole.Calculated,
        Help = "The rank sums behind the statistic and the corrections applied.")]
    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk W, sample 1", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The normality check for sample 1 (the differences, for a paired design): W near 1 " +
               "is normal-looking. Python stats.shapiro(x); R shapiro.test(x).")]
    public double Shapiro1
    {
        get => _shapiro1;
        set => SetProperty(ref _shapiro1, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk p, sample 1", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: no evidence against normality, the t-test's assumption holds well " +
               "enough. p < 0.05 with a small sample: consider the rank test.")]
    public double Shapiro1P
    {
        get => _shapiro1P;
        set => SetProperty(ref _shapiro1P, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk W, sample 2", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The normality check for sample 2.")]
    public double Shapiro2
    {
        get => _shapiro2;
        set => SetProperty(ref _shapiro2, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk p, sample 2", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: no evidence against normality in sample 2.")]
    public double Shapiro2P
    {
        get => _shapiro2P;
        set => SetProperty(ref _shapiro2P, value);
    }

    [StatistleProperty(Label = "Levene W", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Levene's test of equal variances, centred on the medians (Brown–Forsythe, the " +
               "default of scipy's levene and R's car::leveneTest).")]
    public double Levene
    {
        get => _levene;
        set => SetProperty(ref _levene, value);
    }

    [StatistleProperty(Label = "Levene p", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: the variances may be taken as equal and the pooled t-test is justified; " +
               "p < 0.05: use Welch.")]
    public double LeveneP
    {
        get => _leveneP;
        set => SetProperty(ref _leveneP, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    public override void Recalculate()
    {
        Problem = null;
        Outcome = null;
        Hypotheses = RejectionRegion = Decision = Note = string.Empty;
        Statistic = DegreesOfFreedom = PValue = CiLower = CiUpper = PooledSd = EffectSize = Z = double.NaN;
        Shapiro1 = Shapiro1P = Shapiro2 = Shapiro2P = Levene = LeveneP = double.NaN;

        if (Source == DataSource.Columns)
        {
            Problem = PullColumns();

            if (Problem is not null)
            {
                return;
            }
        }
        else if (IsRankTest)
        {
            Problem = "The rank tests need the data themselves: set Data from to columns of a data table.";
            return;
        }

        if (double.IsNaN(Alpha) || Alpha <= 0 || Alpha >= 1)
        {
            Problem = "Enter α between 0 and 1.";
            return;
        }

        ITwoSampleTestMethod method = MethodScope.ActiveMethods(this).OfType<ITwoSampleTestMethod>().First();

        try
        {
            method.Compute(this);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Problem = ex.Message;
            return;
        }

        if (Outcome is null)
        {
            return;
        }

        Statistic = Outcome.Statistic;
        DegreesOfFreedom = Outcome.DegreesOfFreedom;
        RejectionRegion = Outcome.RejectionRegion(method.StatisticSymbol);
        PValue = Outcome.PValue;
        Decision = Outcome.Reject
            ? $"Reject H₀ at α = {Alpha:G3}: p = {PValue:F4} < {Alpha:G3}"
            : $"Do not reject H₀ at α = {Alpha:G3}: p = {PValue:F4} ≥ {Alpha:G3}";

        if (Source == DataSource.Columns && IsMeanTest)
        {
            double[] x = Sample.YValues, y = Sample2.YValues;

            if (IsPaired)
            {
                AssumptionCheck d = Nonparametric.ShapiroWilk([.. x.Zip(y, (a, b) => a - b)]);
                Shapiro1 = d.Statistic;
                Shapiro1P = d.PValue;
            }
            else
            {
                AssumptionCheck s1 = Nonparametric.ShapiroWilk(x), s2 = Nonparametric.ShapiroWilk(y), lev = Nonparametric.Levene([x, y]);
                Shapiro1 = s1.Statistic;
                Shapiro1P = s1.PValue;
                Shapiro2 = s2.Statistic;
                Shapiro2P = s2.PValue;
                Levene = lev.Statistic;
                LeveneP = lev.PValue;
            }
        }
    }

    internal void SetOutcome(TestOutcome outcome, Interval? interval, string hypotheses)
    {
        Outcome = outcome;
        Hypotheses = hypotheses;

        if (interval is not null)
        {
            CiLower = interval.Lower;
            CiUpper = interval.Upper;
        }
    }

    private string? PullColumns()
    {
        string? problem = DataSources.Pull(this, Column, Sample);

        if (problem is not null || Sample.Count == 0)
        {
            return problem ?? DataSources.ChooseOrAdd(this, "Choose the column holding sample 1.");
        }

        problem = DataSources.Pull(this, Column2, Sample2);

        if (problem is not null || Sample2.Count == 0)
        {
            return problem ?? DataSources.ChooseOrAdd(this, "Choose the column holding sample 2.");
        }

        double[] x = Sample.YValues, y = Sample2.YValues;

        if (IsPaired)
        {
            if (x.Length != y.Length)
            {
                return $"A paired design needs the same number of values in both columns ({x.Length} and {y.Length}).";
            }

            double[] d = [.. x.Zip(y, (a, b) => a - b)];
            SampleMean = SummaryStatistics.Mean(d);
            SampleSd = SummaryStatistics.SampleStandardDeviation(d);
            SampleSize = d.Length;
            return null;
        }

        if (TestKind == TwoSampleTestKind.TwoProportions)
        {
            Successes = x.Count(v => v != 0);
            SampleSize = x.Length;
            Successes2 = y.Count(v => v != 0);
            Sample2Size = y.Length;
            return null;
        }

        SampleMean = SummaryStatistics.Mean(x);
        SampleSd = SummaryStatistics.SampleStandardDeviation(x);
        SampleSize = x.Length;
        Sample2Mean = SummaryStatistics.Mean(y);
        Sample2Sd = SummaryStatistics.SampleStandardDeviation(y);
        Sample2Size = y.Length;
        return null;
    }

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName is nameof(Column) or nameof(Column2) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) => propertyName switch
    {
        nameof(Column) or nameof(Sample) => DataSources.Table(this, Column),
        nameof(Column2) or nameof(Sample2) => DataSources.Table(this, Column2),
        _ => null,
    };

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        List<PlotSpec> figures = [];

        if (Source == DataSource.Columns && Sample.Count > 0 && Sample2.Count > 0)
        {
            string name1 = DataSources.Split(Column).ColumnName is { Length: > 0 } a ? a : "Sample 1";
            string name2 = DataSources.Split(Column2).ColumnName is { Length: > 0 } b ? b : "Sample 2";
            figures.Add(IsPaired
                ? DescriptiveFigures.BoxPlot($"{Name} — paired differences", [.. Sample.YValues.Zip(Sample2.YValues, (p, q) => p - q)], double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)
                : InferenceFigures.TwoBoxes($"{Name} — the two samples", name1, Sample.YValues, name2, Sample2.YValues));
        }

        if (Outcome is not null && !IsRankTest)
        {
            ITwoSampleTestMethod method = MethodScope.ActiveMethods(this).OfType<ITwoSampleTestMethod>().First();
            figures.Add(InferenceFigures.TestStatistic($"{Name} — {Hypotheses}", method.Reference(this), Outcome, method.StatisticSymbol));
        }

        return figures;
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        ITwoSampleTestMethod method = MethodScope.ActiveMethods(this).OfType<ITwoSampleTestMethod>().First();
        s.Imports(scipyStats: true);
        s.Comment(Hypotheses);
        s.Assign("alpha", Alpha).Assign("delta0", Delta0);
        // scipy says 'two-sided'; R's t.test, var.test and wilcox.test want "two.sided".
        var tail = AlternativeHypothesis switch { Alternative.Less => "less", Alternative.Greater => "greater", _ => "" };
        s.Either($"alternative = '{(tail.Length > 0 ? tail : "two-sided")}'", $"alternative <- '{(tail.Length > 0 ? tail : "two.sided")}'");

        if (Source == DataSource.Columns)
        {
            s.Vector("x", Sample.YValues).Vector("y", Sample2.YValues);
        }

        method.Declare(this, s);
        s.Line();
        s.PrintText("Hypotheses", Hypotheses);
        s.Print("Test statistic", "stat", "stat", 4);

        if (!double.IsNaN(DegreesOfFreedom))
        {
            s.Print("Degrees of freedom", "df", "df", 2);
        }

        s.PrintText("Rejection region", RejectionRegion);
        s.Print("p-value", "p", "p", 4);
        s.Either("print(\"Decision: \" + (\"Reject H0\" if p < alpha else \"Do not reject H0\"))", "cat(\"Decision:\", if (p < alpha) \"Reject H0\" else \"Do not reject H0\", \"\\n\")");

        if (!double.IsNaN(CiLower))
        {
            s.Print("Confidence interval lower", "ci_lower", "ci_lower", 4);
            s.Print("Confidence interval upper", "ci_upper", "ci_upper", 4);
        }

        if (!double.IsNaN(PooledSd))
        {
            s.Print("Pooled standard deviation (s_p)", "sp", "sp", 4);
        }

        if (!double.IsNaN(EffectSize))
        {
            s.Print("Effect size (Cohen's d)", "cohen_d", "cohen_d", 4);
        }

        if (!double.IsNaN(Z))
        {
            s.Print("Normal approximation z", "z", "z", 4);
            s.PrintText("Note", Note);
        }

        if (!double.IsNaN(Shapiro1))
        {
            s.Line();
            s.Comment("Assumption checks: Shapiro–Wilk normality, Levene (median-centred) equal variances");

            if (IsPaired)
            {
                s.Either("w1, p_w1 = stats.shapiro(x - y)", "sw <- shapiro.test(x - y); w1 <- sw$statistic; p_w1 <- sw$p.value");
                s.Print("Shapiro–Wilk W, sample 1", "w1", "w1", 4);
                s.Print("Shapiro–Wilk p, sample 1", "p_w1", "p_w1", 4);
            }
            else
            {
                s.Either("w1, p_w1 = stats.shapiro(x)", "sw1 <- shapiro.test(x); w1 <- sw1$statistic; p_w1 <- sw1$p.value");
                s.Either("w2, p_w2 = stats.shapiro(y)", "sw2 <- shapiro.test(y); w2 <- sw2$statistic; p_w2 <- sw2$p.value");
                s.Either("lev, p_lev = stats.levene(x, y)   # center='median' is the default", "# Levene, median-centred, by hand (car::leveneTest(y ~ g) gives the same)");
                s.Either(string.Empty, "zx <- abs(x - median(x)); zy <- abs(y - median(y)); zall <- c(zx, zy)");
                s.Either(string.Empty, "between <- length(zx) * (mean(zx) - mean(zall))^2 + length(zy) * (mean(zy) - mean(zall))^2");
                s.Either(string.Empty, "within <- sum((zx - mean(zx))^2) + sum((zy - mean(zy))^2)");
                s.Either(string.Empty, "lev <- (length(zall) - 2) / 1 * between / within; p_lev <- 1 - pf(lev, 1, length(zall) - 2)");
                s.Print("Shapiro–Wilk W, sample 1", "w1", "w1", 4);
                s.Print("Shapiro–Wilk p, sample 1", "p_w1", "p_w1", 4);
                s.Print("Shapiro–Wilk W, sample 2", "w2", "w2", 4);
                s.Print("Shapiro–Wilk p, sample 2", "p_w2", "p_w2", 4);
                s.Print("Levene W", "lev", "lev", 4);
                s.Print("Levene p", "p_lev", "p_lev", 4);
            }
        }

        method.LibraryCheck(this, s);
        return s.ToString();
    }
}

/// <summary>A test kind is a method: its inputs, its computation (writing the outcome into the analysis), its reference distribution and its code.</summary>
public interface ITwoSampleTestMethod : ICalculationMethod
{
    string StatisticSymbol { get; }

    void Compute(TwoSampleTest tool);

    ContinuousModel Reference(TwoSampleTest tool);

    void Declare(TwoSampleTest tool, ScriptBuilder script);

    void LibraryCheck(TwoSampleTest tool, ScriptBuilder script);
}

public abstract class TwoSampleTestMethod : CalculationMethod<TwoSampleTest>, ITwoSampleTestMethod
{
    public abstract TwoSampleTestKind Selects { get; }

    public abstract string StatisticSymbol { get; }

    public override bool IsApplicableTo(TwoSampleTest subject) => subject is not null && subject.TestKind == Selects;

    public override string Citation =>
        "EGN3443 Modules 9–10 (Two-Sample Hypothesis Testing); Montgomery, D. C. and Runger, G. C., " +
        "Applied Statistics and Probability for Engineers, 7th ed., 2018, Chapter 10.";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(TwoSampleTest.Hypotheses),
        nameof(TwoSampleTest.Statistic),
        nameof(TwoSampleTest.DegreesOfFreedom),
        nameof(TwoSampleTest.RejectionRegion),
        nameof(TwoSampleTest.PValue),
        nameof(TwoSampleTest.Decision),
        nameof(TwoSampleTest.CiLower),
        nameof(TwoSampleTest.CiUpper),
    ];

    protected static IReadOnlyList<string> Shared => [nameof(TwoSampleTest.AlternativeHypothesis), nameof(TwoSampleTest.Alpha)];

    protected static IReadOnlyList<string> Means =>
        [nameof(TwoSampleTest.Delta0), nameof(TwoSampleTest.SampleMean), nameof(TwoSampleTest.SampleSd), nameof(TwoSampleTest.SampleSize),
         nameof(TwoSampleTest.Sample2Mean), nameof(TwoSampleTest.Sample2Sd), nameof(TwoSampleTest.Sample2Size)];

    public override void Apply(TwoSampleTest subject) => subject.Recalculate();

    public abstract void Compute(TwoSampleTest tool);

    public abstract ContinuousModel Reference(TwoSampleTest tool);

    public abstract void Declare(TwoSampleTest tool, ScriptBuilder script);

    public virtual void LibraryCheck(TwoSampleTest tool, ScriptBuilder script)
    {
    }

    protected static string Relation(Alternative a) => a switch { Alternative.Less => "<", Alternative.Greater => ">", _ => "≠" };

    protected static int Count(double value, string what, int least = 2)
    {
        if (double.IsNaN(value) || value < least)
        {
            throw new ArgumentOutOfRangeException(what, $"Enter {what} of at least {least}.");
        }

        return (int)Math.Round(value);
    }

    protected static double Positive(double value, string what)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(what, $"Enter {what} greater than zero.");
        }

        return value;
    }

    protected static double Given(double value, string what)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentOutOfRangeException(what, $"Enter {what}.");
        }

        return value;
    }

    protected static void SymmetricTail(ScriptBuilder s, string dist, string dfArg)
    {
        // scipy's stats.t / stats.norm are R's pt / pnorm.
        var r = dist[(dist.LastIndexOf('.') + 1)..];
        s.Either(
            $"p = 2 * {dist}.sf(abs(stat){dfArg}) if alternative == 'two-sided' else ({dist}.cdf(stat{dfArg}) if alternative == 'less' else {dist}.sf(stat{dfArg}))",
            $"p <- if (alternative == 'two.sided') 2 * p{r}(abs(stat){dfArg}, lower.tail = FALSE) else if (alternative == 'less') p{r}(stat{dfArg}) else p{r}(stat{dfArg}, lower.tail = FALSE)");
    }

    /// <summary>Assigns x̄₁, s₁, n₁, x̄₂, s₂, n₂ from the data or the entries.</summary>
    protected static void DeclareTwoSummaries(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("xbar1, s1, n1 = np.mean(x), np.std(x, ddof=1), len(x)", "xbar1 <- mean(x); s1 <- sd(x); n1 <- length(x)");
            s.Either("xbar2, s2, n2 = np.mean(y), np.std(y, ddof=1), len(y)", "xbar2 <- mean(y); s2 <- sd(y); n2 <- length(y)");
        }
        else
        {
            s.Assign("xbar1", t.SampleMean).Assign("s1", t.SampleSd).Assign("n1", (int)t.SampleSize);
            s.Assign("xbar2", t.Sample2Mean).Assign("s2", t.Sample2Sd).Assign("n2", (int)t.Sample2Size);
        }
    }

    protected static void DeclareCohen(ScriptBuilder s)
    {
        s.Either("sp = np.sqrt(((n1 - 1) * s1**2 + (n2 - 1) * s2**2) / (n1 + n2 - 2))", "sp <- sqrt(((n1 - 1) * s1^2 + (n2 - 1) * s2^2) / (n1 + n2 - 2))");
        s.Either("cohen_d = (xbar1 - xbar2) / sp", "cohen_d <- (xbar1 - xbar2) / sp");
    }
}

public sealed class PooledTMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.PooledT;

    public override string Name => "Pooled t-test";

    public override string Description => "t = (x̄₁ − x̄₂ − Δ₀)/(s_p√(1/n₁ + 1/n₂)), ν = n₁ + n₂ − 2, s_p² the weighted average of s₁² and s₂².";

    public override string StatisticSymbol => "t";

    public override IReadOnlyList<string> Inputs => [.. Shared, .. Means];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(TwoSampleTest.PooledSd), nameof(TwoSampleTest.EffectSize)];

    public override void Compute(TwoSampleTest t)
    {
        int n1 = Count(t.SampleSize, "n₁"), n2 = Count(t.Sample2Size, "n₂");
        double m1 = Given(t.SampleMean, "x̄₁"), s1 = Positive(t.SampleSd, "s₁"), m2 = Given(t.Sample2Mean, "x̄₂"), s2 = Positive(t.Sample2Sd, "s₂");
        t.SetOutcome(HypothesisTests.PooledT(m1, s1, n1, m2, s2, n2, t.Delta0, t.AlternativeHypothesis, t.Alpha), Intervals.TwoMeansPooled(m1, s1, n1, m2, s2, n2, 1 - t.Alpha),
            $"H₀: μ₁ − μ₂ = {t.Delta0:G6} vs H₁: μ₁ − μ₂ {Relation(t.AlternativeHypothesis)} {t.Delta0:G6}");
        t.PooledSd = Intervals.PooledStandardDeviation(s1, n1, s2, n2);
        t.EffectSize = HypothesisTests.CohensD(m1, s1, n1, m2, s2, n2);
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new StudentTModel(Math.Max(1, t.SampleSize + t.Sample2Size - 2));

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        DeclareTwoSummaries(t, s);
        s.Assign("df", "n1 + n2 - 2");
        DeclareCohen(s);
        s.Either("se = sp * np.sqrt(1 / n1 + 1 / n2)", "se <- sp * sqrt(1 / n1 + 1 / n2)");
        s.Either("stat = (xbar1 - xbar2 - delta0) / se", "stat <- (xbar1 - xbar2 - delta0) / se");
        SymmetricTail(s, "stats.t", ", df");
        s.Either("ci_lower, ci_upper = (xbar1 - xbar2) - stats.t.ppf(1 - alpha / 2, df) * se, (xbar1 - xbar2) + stats.t.ppf(1 - alpha / 2, df) * se",
                 "ci_lower <- (xbar1 - xbar2) - qt(1 - alpha / 2, df) * se; ci_upper <- (xbar1 - xbar2) + qt(1 - alpha / 2, df) * se");
    }

    public override void LibraryCheck(TwoSampleTest t, ScriptBuilder s)
    {
        s.Comment("Library check (equal_var=True / var.equal = TRUE is the pooled test)");
        s.Either(t.Source == DataSource.Columns ? "print(\"Library check:\", stats.ttest_ind(x, y, equal_var=True, alternative=alternative))"
                                                 : "print(\"Library check:\", stats.ttest_ind_from_stats(xbar1, s1, n1, xbar2, s2, n2, equal_var=True, alternative=alternative))",
                 t.Source == DataSource.Columns ? "print(t.test(x, y, var.equal = TRUE, alternative = alternative, mu = delta0, conf.level = 1 - alpha))"
                                                 : "# R's t.test needs the data; the arithmetic above is the test");
    }
}

public sealed class WelchTMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.WelchT;

    public override string Name => "Welch t-test";

    public override string Description => "t = (x̄₁ − x̄₂ − Δ₀)/√(s₁²/n₁ + s₂²/n₂) with the Welch–Satterthwaite degrees of freedom; no equal-variance assumption.";

    public override string StatisticSymbol => "t";

    public override IReadOnlyList<string> Inputs => [.. Shared, .. Means];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(TwoSampleTest.EffectSize)];

    public override void Compute(TwoSampleTest t)
    {
        int n1 = Count(t.SampleSize, "n₁"), n2 = Count(t.Sample2Size, "n₂");
        double m1 = Given(t.SampleMean, "x̄₁"), s1 = Positive(t.SampleSd, "s₁"), m2 = Given(t.Sample2Mean, "x̄₂"), s2 = Positive(t.Sample2Sd, "s₂");
        t.SetOutcome(HypothesisTests.WelchT(m1, s1, n1, m2, s2, n2, t.Delta0, t.AlternativeHypothesis, t.Alpha), Intervals.TwoMeansWelch(m1, s1, n1, m2, s2, n2, 1 - t.Alpha),
            $"H₀: μ₁ − μ₂ = {t.Delta0:G6} vs H₁: μ₁ − μ₂ {Relation(t.AlternativeHypothesis)} {t.Delta0:G6}");
        t.EffectSize = HypothesisTests.CohensD(m1, s1, n1, m2, s2, n2);
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new StudentTModel(Math.Max(1, t.Outcome?.DegreesOfFreedom ?? 1));

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        DeclareTwoSummaries(t, s);
        s.Either("a, b = s1**2 / n1, s2**2 / n2", "a <- s1^2 / n1; b <- s2^2 / n2");
        s.Either("se = np.sqrt(a + b)", "se <- sqrt(a + b)");
        s.Either("df = (a + b)**2 / (a**2 / (n1 - 1) + b**2 / (n2 - 1))", "df <- (a + b)^2 / (a^2 / (n1 - 1) + b^2 / (n2 - 1))");
        DeclareCohen(s);
        s.Either("stat = (xbar1 - xbar2 - delta0) / se", "stat <- (xbar1 - xbar2 - delta0) / se");
        SymmetricTail(s, "stats.t", ", df");
        s.Either("ci_lower, ci_upper = (xbar1 - xbar2) - stats.t.ppf(1 - alpha / 2, df) * se, (xbar1 - xbar2) + stats.t.ppf(1 - alpha / 2, df) * se",
                 "ci_lower <- (xbar1 - xbar2) - qt(1 - alpha / 2, df) * se; ci_upper <- (xbar1 - xbar2) + qt(1 - alpha / 2, df) * se");
    }

    public override void LibraryCheck(TwoSampleTest t, ScriptBuilder s)
    {
        s.Comment("Library check (Welch is the default of both libraries)");
        s.Either(t.Source == DataSource.Columns ? "print(\"Library check:\", stats.ttest_ind(x, y, equal_var=False, alternative=alternative))"
                                                 : "print(\"Library check:\", stats.ttest_ind_from_stats(xbar1, s1, n1, xbar2, s2, n2, equal_var=False, alternative=alternative))",
                 t.Source == DataSource.Columns ? "print(t.test(x, y, alternative = alternative, mu = delta0, conf.level = 1 - alpha))" : "# R's t.test needs the data; the arithmetic above is the test");
    }
}

public sealed class PairedTMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.PairedT;

    public override string Name => "Paired t-test";

    public override string Description => "On the differences d = x − y: t = (d̄ − Δ₀)/(s_d/√n), ν = n − 1. Pairing removes the between-unit variation.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-4 (paired t-test).";

    public override string StatisticSymbol => "t";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(TwoSampleTest.Delta0), nameof(TwoSampleTest.SampleMean), nameof(TwoSampleTest.SampleSd), nameof(TwoSampleTest.SampleSize)];

    public override void Compute(TwoSampleTest t)
    {
        int n = Count(t.SampleSize, "the number of pairs");
        double d = Given(t.SampleMean, "the mean difference d̄"), sd = Positive(t.SampleSd, "s_d");
        t.SetOutcome(HypothesisTests.PairedT(d, sd, n, t.Delta0, t.AlternativeHypothesis, t.Alpha), Intervals.MeanSigmaUnknown(d, sd, n, 1 - t.Alpha),
            $"H₀: μ_d = {t.Delta0:G6} vs H₁: μ_d {Relation(t.AlternativeHypothesis)} {t.Delta0:G6}");
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new StudentTModel(Math.Max(1, t.SampleSize - 1));

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("d = x - y", "d <- x - y");
            s.Either("dbar, s_d, n = np.mean(d), np.std(d, ddof=1), len(d)", "dbar <- mean(d); s_d <- sd(d); n <- length(d)");
        }
        else
        {
            s.Assign("dbar", t.SampleMean).Assign("s_d", t.SampleSd).Assign("n", (int)t.SampleSize);
        }

        s.Assign("df", "n - 1");
        s.Either("se = s_d / np.sqrt(n)", "se <- s_d / sqrt(n)");
        s.Either("stat = (dbar - delta0) / se", "stat <- (dbar - delta0) / se");
        SymmetricTail(s, "stats.t", ", df");
        s.Either("ci_lower, ci_upper = dbar - stats.t.ppf(1 - alpha / 2, df) * se, dbar + stats.t.ppf(1 - alpha / 2, df) * se",
                 "ci_lower <- dbar - qt(1 - alpha / 2, df) * se; ci_upper <- dbar + qt(1 - alpha / 2, df) * se");
    }

    public override void LibraryCheck(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("print(\"Library check:\", stats.ttest_rel(x, y, alternative=alternative))", "print(t.test(x, y, paired = TRUE, alternative = alternative, mu = delta0))");
        }
    }
}

public sealed class TwoSampleZMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.TwoSampleZ;

    public override string Name => "Two-sample z-test";

    public override string Description => "z = (x̄₁ − x̄₂ − Δ₀)/√(σ₁²/n₁ + σ₂²/n₂), both population variances known.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-1 (inference on the difference in means, variances known).";

    public override string StatisticSymbol => "z";

    public override IReadOnlyList<string> Inputs =>
        [.. Shared, nameof(TwoSampleTest.Delta0), nameof(TwoSampleTest.SampleMean), nameof(TwoSampleTest.Sigma), nameof(TwoSampleTest.SampleSize),
         nameof(TwoSampleTest.Sample2Mean), nameof(TwoSampleTest.Sigma2), nameof(TwoSampleTest.Sample2Size)];

    public override void Compute(TwoSampleTest t)
    {
        int n1 = Count(t.SampleSize, "n₁"), n2 = Count(t.Sample2Size, "n₂");
        double m1 = Given(t.SampleMean, "x̄₁"), s1 = Positive(t.Sigma, "σ₁"), m2 = Given(t.Sample2Mean, "x̄₂"), s2 = Positive(t.Sigma2, "σ₂");
        t.SetOutcome(HypothesisTests.TwoSampleZ(m1, s1, n1, m2, s2, n2, t.Delta0, t.AlternativeHypothesis, t.Alpha), Intervals.TwoMeansSigmaKnown(m1, s1, n1, m2, s2, n2, 1 - t.Alpha),
            $"H₀: μ₁ − μ₂ = {t.Delta0:G6} vs H₁: μ₁ − μ₂ {Relation(t.AlternativeHypothesis)} {t.Delta0:G6}");
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new NormalModel(0, 1);

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("xbar1, n1, xbar2, n2 = np.mean(x), len(x), np.mean(y), len(y)", "xbar1 <- mean(x); n1 <- length(x); xbar2 <- mean(y); n2 <- length(y)");
        }
        else
        {
            s.Assign("xbar1", t.SampleMean).Assign("n1", (int)t.SampleSize).Assign("xbar2", t.Sample2Mean).Assign("n2", (int)t.Sample2Size);
        }

        s.Assign("sigma1", t.Sigma).Assign("sigma2", t.Sigma2);
        s.Either("se = np.sqrt(sigma1**2 / n1 + sigma2**2 / n2)", "se <- sqrt(sigma1^2 / n1 + sigma2^2 / n2)");
        s.Either("stat = (xbar1 - xbar2 - delta0) / se", "stat <- (xbar1 - xbar2 - delta0) / se");
        SymmetricTail(s, "stats.norm", string.Empty);
        s.Either("ci_lower, ci_upper = (xbar1 - xbar2) - stats.norm.ppf(1 - alpha / 2) * se, (xbar1 - xbar2) + stats.norm.ppf(1 - alpha / 2) * se",
                 "ci_lower <- (xbar1 - xbar2) - qnorm(1 - alpha / 2) * se; ci_upper <- (xbar1 - xbar2) + qnorm(1 - alpha / 2) * se");
    }
}

public sealed class TwoProportionsMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.TwoProportions;

    public override string Name => "Two-proportion z-test";

    public override string Description => "z = (p̂₁ − p̂₂)/√(p̂(1 − p̂)(1/n₁ + 1/n₂)) with the pooled p̂ = (x₁ + x₂)/(n₁ + n₂); CI with the unpooled standard error.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-6 (inference on two population proportions).";

    public override string StatisticSymbol => "z";

    public override IReadOnlyList<string> Inputs =>
        [.. Shared, nameof(TwoSampleTest.Successes), nameof(TwoSampleTest.SampleSize), nameof(TwoSampleTest.Successes2), nameof(TwoSampleTest.Sample2Size)];

    public override void Compute(TwoSampleTest t)
    {
        int n1 = Count(t.SampleSize, "n₁", 1), k1 = Count(t.Successes, "x₁", 0), n2 = Count(t.Sample2Size, "n₂", 1), k2 = Count(t.Successes2, "x₂", 0);

        if (k1 > n1 || k2 > n2)
        {
            throw new ArgumentOutOfRangeException(nameof(t.Successes), "The successes cannot exceed the sample size.");
        }

        t.SetOutcome(HypothesisTests.TwoProportionsZ(k1, n1, k2, n2, t.AlternativeHypothesis, t.Alpha), Intervals.TwoProportions(k1, n1, k2, n2, 1 - t.Alpha),
            $"H₀: p₁ = p₂ vs H₁: p₁ {Relation(t.AlternativeHypothesis)} p₂");
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new NormalModel(0, 1);

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("k1, n1, k2, n2 = int(np.sum(x != 0)), len(x), int(np.sum(y != 0)), len(y)", "k1 <- sum(x != 0); n1 <- length(x); k2 <- sum(y != 0); n2 <- length(y)");
        }
        else
        {
            s.Assign("k1", (int)t.Successes).Assign("n1", (int)t.SampleSize).Assign("k2", (int)t.Successes2).Assign("n2", (int)t.Sample2Size);
        }

        s.Either("p1, p2, pooled = k1 / n1, k2 / n2, (k1 + k2) / (n1 + n2)", "p1 <- k1 / n1; p2 <- k2 / n2; pooled <- (k1 + k2) / (n1 + n2)");
        s.Either("stat = (p1 - p2) / np.sqrt(pooled * (1 - pooled) * (1 / n1 + 1 / n2))", "stat <- (p1 - p2) / sqrt(pooled * (1 - pooled) * (1 / n1 + 1 / n2))");
        SymmetricTail(s, "stats.norm", string.Empty);
        s.Either("se = np.sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2)", "se <- sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2)");
        s.Either("ci_lower, ci_upper = (p1 - p2) - stats.norm.ppf(1 - alpha / 2) * se, (p1 - p2) + stats.norm.ppf(1 - alpha / 2) * se",
                 "ci_lower <- (p1 - p2) - qnorm(1 - alpha / 2) * se; ci_upper <- (p1 - p2) + qnorm(1 - alpha / 2) * se");
    }

    public override void LibraryCheck(TwoSampleTest t, ScriptBuilder s)
    {
        s.Comment("Library check: R prop.test(c(k1, k2), c(n1, n2), correct = FALSE); Python statsmodels proportions_ztest([k1, k2], [n1, n2])");
        s.Either("# from statsmodels.stats.proportion import proportions_ztest", "print(prop.test(c(k1, k2), c(n1, n2), alternative = alternative, correct = FALSE))");
    }
}

public sealed class VarianceRatioMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.VarianceRatio;

    public override string Name => "F-test on two variances";

    public override string Description => "F = s₁²/s₂² against F(n₁ − 1, n₂ − 1); CI for σ₁²/σ₂² from the F quantiles. Assumes normal populations.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-5 (inference on two population variances).";

    public override string StatisticSymbol => "F";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(TwoSampleTest.SampleSd), nameof(TwoSampleTest.SampleSize), nameof(TwoSampleTest.Sample2Sd), nameof(TwoSampleTest.Sample2Size)];

    public override void Compute(TwoSampleTest t)
    {
        int n1 = Count(t.SampleSize, "n₁"), n2 = Count(t.Sample2Size, "n₂");
        double s1 = Positive(t.SampleSd, "s₁"), s2 = Positive(t.Sample2Sd, "s₂");
        TestOutcome outcome = HypothesisTests.FTestVariances(s1, n1, s2, n2, t.AlternativeHypothesis, t.Alpha);
        double ratio = s1 * s1 / (s2 * s2), alpha = t.Alpha;
        double lower = ratio / MathNet.Numerics.Distributions.FisherSnedecor.InvCDF(n1 - 1, n2 - 1, 1 - alpha / 2);
        double upper = ratio / MathNet.Numerics.Distributions.FisherSnedecor.InvCDF(n1 - 1, n2 - 1, alpha / 2);
        t.SetOutcome(outcome, new Interval(ratio, double.NaN, outcome.CriticalHigh, double.NaN, lower, upper, n1 - 1),
            $"H₀: σ₁² = σ₂² vs H₁: σ₁² {Relation(t.AlternativeHypothesis)} σ₂²");
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new FModel(Math.Max(1, t.SampleSize - 1), Math.Max(1, t.Sample2Size - 1));

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("s1, n1, s2, n2 = np.std(x, ddof=1), len(x), np.std(y, ddof=1), len(y)", "s1 <- sd(x); n1 <- length(x); s2 <- sd(y); n2 <- length(y)");
        }
        else
        {
            s.Assign("s1", t.SampleSd).Assign("n1", (int)t.SampleSize).Assign("s2", t.Sample2Sd).Assign("n2", (int)t.Sample2Size);
        }

        s.Either("df, df2 = n1 - 1, n2 - 1", "df <- n1 - 1; df2 <- n2 - 1");
        s.Either("stat = s1**2 / s2**2", "stat <- s1^2 / s2^2");
        s.Either("lower_tail = stats.f.cdf(stat, df, df2)", "lower_tail <- pf(stat, df, df2)");
        s.Either("p = min(1, 2 * min(lower_tail, 1 - lower_tail)) if alternative == 'two-sided' else (lower_tail if alternative == 'less' else 1 - lower_tail)",
                 "p <- if (alternative == 'two.sided') min(1, 2 * min(lower_tail, 1 - lower_tail)) else if (alternative == 'less') lower_tail else 1 - lower_tail");
        s.Either("ci_lower, ci_upper = stat / stats.f.ppf(1 - alpha / 2, df, df2), stat / stats.f.ppf(alpha / 2, df, df2)",
                 "ci_lower <- stat / qf(1 - alpha / 2, df, df2); ci_upper <- stat / qf(alpha / 2, df, df2)");
    }

    public override void LibraryCheck(TwoSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("# scipy has no F-test for two variances; the arithmetic above is the test", "print(var.test(x, y, alternative = alternative, conf.level = 1 - alpha))");
        }
    }
}

public sealed class MannWhitneyMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.MannWhitney;

    public override string Name => "Mann–Whitney U test";

    public override string Description => "Ranks both samples together; U₁ = R₁ − n₁(n₁ + 1)/2; normal approximation with continuity and tie corrections (scipy's method='asymptotic').";

    public override string Citation => "Mann, H. B. and Whitney, D. R., Annals of Mathematical Statistics 18, 1947; Montgomery and Runger, 7th ed., §10-6 (nonparametric procedures).";

    public override string StatisticSymbol => "z";

    public override IReadOnlyList<string> Inputs => [.. Shared];

    public override IReadOnlyList<string> Outputs => [nameof(TwoSampleTest.Hypotheses), nameof(TwoSampleTest.Statistic), nameof(TwoSampleTest.RejectionRegion), nameof(TwoSampleTest.PValue), nameof(TwoSampleTest.Decision), nameof(TwoSampleTest.Z), nameof(TwoSampleTest.Note)];

    public override void Compute(TwoSampleTest t)
    {
        RankTest r = Nonparametric.MannWhitney(t.Sample.YValues, t.Sample2.YValues, t.AlternativeHypothesis);
        TestOutcome z = ZOutcome(r, t);
        t.SetOutcome(z with { Statistic = r.Statistic, DegreesOfFreedom = double.NaN }, null,
            $"H₀: the two distributions are the same vs H₁: sample 1 tends to be {(t.AlternativeHypothesis == Alternative.Less ? "smaller" : t.AlternativeHypothesis == Alternative.Greater ? "larger" : "different")}");
        t.Z = r.Z;
        t.Note = r.Note;
    }

    internal static TestOutcome ZOutcome(RankTest r, TwoSampleTest t)
    {
        double q = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, 1 - t.Alpha / 2);
        (double low, double high) = t.AlternativeHypothesis switch
        {
            Alternative.Less => (-MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, 1 - t.Alpha), double.NaN),
            Alternative.Greater => (double.NaN, MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, 1 - t.Alpha)),
            _ => (-q, q),
        };
        return new TestOutcome(r.Z, double.NaN, r.PValue, low, high, r.PValue < t.Alpha);
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new NormalModel(0, 1);

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        s.Comment("Mann–Whitney U: rank everything together, U₁ = R₁ − n₁(n₁+1)/2");
        s.Either("res = stats.mannwhitneyu(x, y, alternative=alternative, method='asymptotic')", "res <- wilcox.test(x, y, alternative = alternative, exact = FALSE, correct = TRUE)");
        s.Either("stat, p = res.statistic, res.pvalue", "stat <- res$statistic; p <- res$p.value");
        s.Comment("The normal approximation z the p-value comes from, with continuity and tie corrections");
        s.Either("n1, n2 = len(x), len(y)", "n1 <- length(x); n2 <- length(y)");
        s.Either("ranks = stats.rankdata(np.concatenate([x, y]))", "ranks <- rank(c(x, y))");
        s.Either("u1 = ranks[:n1].sum() - n1 * (n1 + 1) / 2", "u1 <- sum(ranks[1:n1]) - n1 * (n1 + 1) / 2");
        s.Either("u2 = n1 * n2 - u1", "u2 <- n1 * n2 - u1");
        s.Either("_, counts = np.unique(np.concatenate([x, y]), return_counts=True)", "counts <- as.numeric(table(c(x, y)))");
        s.Either("sigma = np.sqrt(n1 * n2 / 12 * ((n1 + n2 + 1) - np.sum(counts**3 - counts) / ((n1 + n2) * (n1 + n2 - 1))))",
                 "sigma <- sqrt(n1 * n2 / 12 * ((n1 + n2 + 1) - sum(counts^3 - counts) / ((n1 + n2) * (n1 + n2 - 1))))");
        s.Either("u = u1 if alternative == 'greater' else (u2 if alternative == 'less' else max(u1, u2))", "u <- if (alternative == 'greater') u1 else if (alternative == 'less') u2 else max(u1, u2)");
        s.Either("z = (u - 0.5 - n1 * n2 / 2) / sigma", "z <- (u - 0.5 - n1 * n2 / 2) / sigma");
    }
}

public sealed class WilcoxonMethod : TwoSampleTestMethod
{
    public override TwoSampleTestKind Selects => TwoSampleTestKind.WilcoxonSignedRank;

    public override string Name => "Wilcoxon signed-rank test";

    public override string Description => "Ranks the absolute paired differences (zeros dropped); normal approximation without continuity correction (scipy's method='approx', correction=False).";

    public override string Citation => "Wilcoxon, F., Biometrics Bulletin 1, 1945; Montgomery and Runger, 7th ed., §9-9 (nonparametric procedures).";

    public override string StatisticSymbol => "z";

    public override IReadOnlyList<string> Inputs => [.. Shared];

    public override IReadOnlyList<string> Outputs => [nameof(TwoSampleTest.Hypotheses), nameof(TwoSampleTest.Statistic), nameof(TwoSampleTest.RejectionRegion), nameof(TwoSampleTest.PValue), nameof(TwoSampleTest.Decision), nameof(TwoSampleTest.Z), nameof(TwoSampleTest.Note)];

    public override void Compute(TwoSampleTest t)
    {
        double[] d = [.. t.Sample.YValues.Zip(t.Sample2.YValues, (a, b) => a - b)];
        RankTest r = Nonparametric.WilcoxonSignedRank(d, t.AlternativeHypothesis);

        if (double.IsNaN(r.Statistic))
        {
            throw new ArgumentOutOfRangeException(nameof(t.Sample), r.Note);
        }

        TestOutcome z = MannWhitneyMethod.ZOutcome(r, t);
        t.SetOutcome(z with { Statistic = r.Statistic, DegreesOfFreedom = double.NaN }, null,
            $"H₀: the differences are symmetric about 0 vs H₁: they tend to be {(t.AlternativeHypothesis == Alternative.Less ? "negative" : t.AlternativeHypothesis == Alternative.Greater ? "positive" : "non-zero")}");
        t.Z = r.Z;
        t.Note = r.Note;
    }

    public override ContinuousModel Reference(TwoSampleTest t) => new NormalModel(0, 1);

    public override void Declare(TwoSampleTest t, ScriptBuilder s)
    {
        s.Comment("Wilcoxon signed-rank on the paired differences, zeros dropped, normal approximation");
        s.Either("res = stats.wilcoxon(x, y, alternative=alternative, zero_method='wilcox', correction=False, method='approx')",
                 "res <- wilcox.test(x, y, paired = TRUE, alternative = alternative, exact = FALSE, correct = FALSE)");
        s.Either("stat, p = res.statistic, res.pvalue", "stat <- res$statistic; p <- res$p.value");
        s.Comment("The z behind the p-value");
        s.Either("d = (x - y)[(x - y) != 0]", "d <- (x - y)[(x - y) != 0]");
        s.Either("n = len(d)", "n <- length(d)");
        s.Either("r = stats.rankdata(np.abs(d))", "r <- rank(abs(d))");
        s.Either("r_plus = r[d > 0].sum()", "r_plus <- sum(r[d > 0])");
        s.Either("_, counts = np.unique(np.abs(d), return_counts=True)", "counts <- as.numeric(table(abs(d)))");
        s.Either("se = np.sqrt(n * (n + 1) * (2 * n + 1) / 24 - np.sum(counts**3 - counts) / 48)", "se <- sqrt(n * (n + 1) * (2 * n + 1) / 24 - sum(counts^3 - counts) / 48)");
        s.Either("z = (r_plus - n * (n + 1) / 4) / se", "z <- (r_plus - n * (n + 1) / 4) / se");
    }
}

/// <summary>Summaries typed by hand.</summary>
public sealed class TwoSampleSummaryEntered : CalculationMethod<TwoSampleTest>
{
    public override string Name => "Summary statistics entered";

    public override string Description => "The sample statistics are typed into the analysis.";

    public override string Citation => "Statistics+ entry.";

    public override bool IsApplicableTo(TwoSampleTest subject) => subject is not null && subject.Source == DataSource.Summary;

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [];

    public override void Apply(TwoSampleTest subject)
    {
    }
}

/// <summary>Summaries from two data columns, locked while the columns are chosen; the assumption checks come with them.</summary>
public sealed class TwoSampleColumnSource : CalculationMethod<TwoSampleTest>
{
    public override string Name => "From two data table columns";

    public override string Description => "Both samples' summaries (or the paired differences') are computed from the chosen columns; Shapiro–Wilk and Levene checks are reported for the mean tests.";

    public override string Citation =>
        "Shapiro, S. S. and Wilk, M. B., Biometrika 52, 1965, by Royston's algorithm AS R94 (1995); Levene, H., 1960, median-centred (Brown and Forsythe, 1974).";

    public override bool IsApplicableTo(TwoSampleTest subject) => subject is not null && subject.Source == DataSource.Columns;

    public override IReadOnlyList<string> Inputs => [nameof(TwoSampleTest.Column), nameof(TwoSampleTest.Column2)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(TwoSampleTest.SampleMean), nameof(TwoSampleTest.SampleSd), nameof(TwoSampleTest.SampleSize), nameof(TwoSampleTest.Successes),
        nameof(TwoSampleTest.Sample2Mean), nameof(TwoSampleTest.Sample2Sd), nameof(TwoSampleTest.Sample2Size), nameof(TwoSampleTest.Successes2),
        nameof(TwoSampleTest.Shapiro1), nameof(TwoSampleTest.Shapiro1P), nameof(TwoSampleTest.Shapiro2), nameof(TwoSampleTest.Shapiro2P),
        nameof(TwoSampleTest.Levene), nameof(TwoSampleTest.LeveneP),
    ];

    public override IReadOnlyList<string> Generates =>
    [
        nameof(TwoSampleTest.SampleMean), nameof(TwoSampleTest.SampleSd), nameof(TwoSampleTest.SampleSize), nameof(TwoSampleTest.Successes),
        nameof(TwoSampleTest.Sample2Mean), nameof(TwoSampleTest.Sample2Sd), nameof(TwoSampleTest.Sample2Size), nameof(TwoSampleTest.Successes2),
    ];

    public override void Apply(TwoSampleTest subject) => subject.Recalculate();
}
