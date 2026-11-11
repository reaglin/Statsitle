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

public enum OneSampleTestKind
{
    [Description("Mean, σ known (z-test)")]
    MeanSigmaKnown = 0,

    [Description("Mean, σ unknown (t-test)")]
    MeanSigmaUnknown,

    [Description("Proportion (z-test)")]
    Proportion,

    [Description("Variance (χ²-test)")]
    Variance,
}

/// <summary>
/// EGN3443 Modules 8–9: a hypothesis test on one population — a mean by z or t, a proportion,
/// a variance — with the rejection region, the p-value, the decision, the matching confidence
/// interval, and the power against a stated alternative.
/// </summary>
[StatistleHelp(
    "A one-sample hypothesis test (Modules 8 and 9). State the null value (μ₀, p₀ or σ₀), the " +
    "direction of the alternative and α, give the sample — as summary statistics or a data " +
    "table column — and read the test statistic, its rejection region at α, the p-value and " +
    "the decision, both ways of deciding side by side. A small p-value is strong evidence " +
    "against H₀; p < α rejects it. The two-sided (1 − α) confidence interval is given too: H₀ " +
    "is rejected exactly when the null value lies outside it. For a test on a mean, enter a " +
    "true mean worth detecting and read the power (the chance of rejecting when that is the " +
    "truth) and the sample size that would reach a target power. Use the z-test only when σ is " +
    "known; with s from the sample use t. The Plots tab draws the reference distribution with " +
    "the rejection region shaded and the observed statistic; the Code tab writes the arithmetic " +
    "in Python and R and, with a data column, the library call that checks it.")]
public sealed class OneSampleTest : Analysis, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, IHasFigures, ICodeSource
{
    private OneSampleTestKind _kind = OneSampleTestKind.MeanSigmaKnown;
    private DataSource _source = DataSource.Summary;
    private Alternative _alternative = Alternative.TwoSided;
    private double _alpha = 0.05;
    private double _nullValue = 50;
    private string _column = string.Empty;
    private double _sampleMean = 51.3;
    private double _sampleSd = 2;
    private double _sigma = 2;
    private double _sampleSize = 25;
    private double _successes = 5;
    private double _alternativeValue = 51;
    private double _targetPower = 0.9;

    private string _hypotheses = string.Empty;
    private double _statistic = double.NaN;
    private double _degreesOfFreedom = double.NaN;
    private string _rejectionRegion = string.Empty;
    private double _pValue = double.NaN;
    private string _decision = string.Empty;
    private double _ciLower = double.NaN;
    private double _ciUpper = double.NaN;
    private double _power = double.NaN;
    private double _sampleSizeForPower = double.NaN;

    public OneSampleTest()
    {
        Name = "One-sample test";
        Recalculate();
    }

    public override string Title => "One-sample test";

    public override string Summary => "z, t, proportion and variance tests on one sample, with power (Modules 8–9)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 32;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public TestOutcome? Outcome { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Test", Role = VariableRole.Design,
        Help = "Which parameter is tested and with what statistic. z for a mean only when σ is " +
               "known; t when it is estimated by s (the usual case). The proportion test uses " +
               "the normal approximation, valid when np₀ and n(1 − p₀) are both at least 10. " +
               "The variance test assumes a normal population and is sensitive to that.")]
    public OneSampleTestKind TestKind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
    }

    [StatistleProperty(Label = "Data from", Role = VariableRole.Design,
        Help = "Summary statistics typed here, or a column of a data table (values, or 1/0 for a " +
               "proportion) from which they are computed and shown locked.")]
    public DataSource Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    [StatistleProperty(Label = "Alternative hypothesis", Role = VariableRole.Design,
        Help = "Two-sided (≠) when any change matters; one-sided (< or >) only when the question " +
               "was directional before the data were seen — 'does the new process reduce " +
               "defects?'. A one-sided test has more power in its direction and none in the other.")]
    public Alternative AlternativeHypothesis
    {
        get => _alternative;
        set => SetProperty(ref _alternative, value);
    }

    [StatistleProperty(Label = "Significance level (α)", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.0001, Max = 0.5,
        Help = "The probability of a Type I error the test is designed to allow — rejecting H₀ " +
               "when it is true. 0.05 is the convention; 0.01 when a false alarm is costly.")]
    public double Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    [StatistleProperty(Label = "Null value (μ₀, p₀ or σ₀)", Decimals = 4, Role = VariableRole.Input,
        Help = "The value H₀ claims: the target mean, the historical defect rate, the " +
               "specification's standard deviation. The test asks whether the sample is " +
               "consistent with it.")]
    public double NullValue
    {
        get => _nullValue;
        set => SetProperty(ref _nullValue, value);
    }

    [StatistleProperty(Label = "Column", Role = VariableRole.Input,
        Help = "The data table column holding the sample (1/0 for a proportion).")]
    public string Column
    {
        get => _column;
        set => SetProperty(ref _column, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Sample values", Role = VariableRole.Input, ValuesOnly = true, Hidden = true,
        Help = "The sample's values, copied from its column.")]
    public DataSeries Sample { get; } = new();

    [StatistleProperty(Label = "Sample mean (x̄)", Decimals = 4, Role = VariableRole.Input,
        Help = "The mean of the sample, x̄ — the estimate the test compares with μ₀.")]
    public double SampleMean
    {
        get => _sampleMean;
        set => SetProperty(ref _sampleMean, value);
    }

    [StatistleProperty(Label = "Sample standard deviation (s)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The sample standard deviation, dividing by n − 1.")]
    public double SampleSd
    {
        get => _sampleSd;
        set => SetProperty(ref _sampleSd, value);
    }

    [StatistleProperty(Label = "Population σ", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The known population standard deviation, for the z-test and for the power calculation.")]
    public double Sigma
    {
        get => _sigma;
        set => SetProperty(ref _sigma, value);
    }

    [StatistleProperty(Label = "Sample size (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 2,
        Help = "How many observations in the sample; the standard error shrinks as 1/√n.")]
    public double SampleSize
    {
        get => _sampleSize;
        set => SetProperty(ref _sampleSize, value);
    }

    [StatistleProperty(Label = "Successes (x)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0,
        Help = "How many of the n items had the attribute; p̂ = x/n.")]
    public double Successes
    {
        get => _successes;
        set => SetProperty(ref _successes, value);
    }

    [StatistleProperty(Label = "True mean worth detecting (μ₁), optional", Decimals = 4, Role = VariableRole.Input,
        Help = "For the power calculation: a value of the mean that would matter in practice. " +
               "Power is the probability the test rejects H₀ when this is the truth; 1 − power " +
               "is β, the Type II error. Leave blank to skip.")]
    public double AlternativeValue
    {
        get => _alternativeValue;
        set => SetProperty(ref _alternativeValue, value);
    }

    [StatistleProperty(Label = "Target power", Decimals = 2, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.5, Max = 0.999,
        Help = "The power wanted at μ₁ — 0.80 or 0.90 are the usual targets — for the sample-size result.")]
    public double TargetPower
    {
        get => _targetPower;
        set => SetProperty(ref _targetPower, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Hypotheses", Role = VariableRole.Output,
        Help = "H₀ and H₁ as they should be written in the report, before any number is computed.")]
    public string Hypotheses
    {
        get => _hypotheses;
        set => SetProperty(ref _hypotheses, value);
    }

    [StatistleProperty(Label = "Test statistic", Decimals = 4, Role = VariableRole.Output,
        Help = "z = (x̄ − μ₀)/(σ/√n), t = (x̄ − μ₀)/(s/√n), z = (p̂ − p₀)/√(p₀(1 − p₀)/n) or " +
               "χ² = (n − 1)s²/σ₀²: how far the sample lies from H₀ in standard-error units.")]
    public double Statistic
    {
        get => _statistic;
        set => SetProperty(ref _statistic, value);
    }

    [StatistleProperty(Label = "Degrees of freedom", Decimals = 0, Role = VariableRole.Calculated,
        Help = "n − 1 for the t and χ² tests; blank for z.")]
    public double DegreesOfFreedom
    {
        get => _degreesOfFreedom;
        set => SetProperty(ref _degreesOfFreedom, value);
    }

    [StatistleProperty(Label = "Rejection region", Role = VariableRole.Output,
        Help = "The values of the statistic that reject H₀ at α — the critical-value way of " +
               "deciding. Python stats.norm.ppf / stats.t.ppf / stats.chi2.ppf; R qnorm / qt / qchisq.")]
    public string RejectionRegion
    {
        get => _rejectionRegion;
        set => SetProperty(ref _rejectionRegion, value);
    }

    [StatistleProperty(Label = "p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability, if H₀ were true, of a statistic at least as extreme as the one " +
               "observed, in the direction(s) of H₁. It is not the probability that H₀ is true. " +
               "Python stats.norm.sf / stats.t.sf; R pnorm / pt with lower.tail = FALSE.")]
    public double PValue
    {
        get => _pValue;
        set => SetProperty(ref _pValue, value);
    }

    [StatistleProperty(Label = "Decision", Role = VariableRole.Output,
        Help = "Reject H₀ when p < α (equivalently, when the statistic falls in the rejection " +
               "region). 'Do not reject' is not 'accept': the data may simply be too few to tell.")]
    public string Decision
    {
        get => _decision;
        set => SetProperty(ref _decision, value);
    }

    [StatistleProperty(Label = "Confidence interval lower", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The two-sided (1 − α) confidence interval for the parameter. H₀ is rejected by a " +
               "two-sided test exactly when the null value is outside it.")]
    public double CiLower
    {
        get => _ciLower;
        set => SetProperty(ref _ciLower, value);
    }

    [StatistleProperty(Label = "Confidence interval upper", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The upper end of the (1 − α) confidence interval.")]
    public double CiUpper
    {
        get => _ciUpper;
        set => SetProperty(ref _ciUpper, value);
    }

    [StatistleProperty(Label = "Power at μ₁", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of rejecting H₀ when the true mean is μ₁, by the normal " +
               "approximation β = Φ(z_{α/2} − δ√n/σ) − Φ(−z_{α/2} − δ√n/σ) (the course's " +
               "method, also for the t-test with s in place of σ). Python statsmodels' " +
               "NormalIndPower; R power.t.test.")]
    public double Power
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    [StatistleProperty(Label = "n for the target power", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "The smallest sample size whose power at μ₁ reaches the target: " +
               "n ≈ [(z_{α/2} + z_β)σ/δ]², then checked.")]
    public double SampleSizeForPower
    {
        get => _sampleSizeForPower;
        set => SetProperty(ref _sampleSizeForPower, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    public override void Recalculate()
    {
        Problem = null;
        Outcome = null;
        Hypotheses = RejectionRegion = Decision = string.Empty;
        Statistic = DegreesOfFreedom = PValue = CiLower = CiUpper = Power = SampleSizeForPower = double.NaN;

        if (Source == DataSource.Columns)
        {
            string? problem = DataSources.Pull(this, Column, Sample);

            if (problem is not null || Sample.Count == 0)
            {
                Problem = problem ?? DataSources.ChooseOrAdd(this, "Choose the column holding the sample.");
                return;
            }

            double[] x = Sample.YValues;
            SampleSize = x.Length;

            if (TestKind == OneSampleTestKind.Proportion)
            {
                Successes = x.Count(v => v != 0);
            }
            else
            {
                SampleMean = SummaryStatistics.Mean(x);
                SampleSd = SummaryStatistics.SampleStandardDeviation(x);
            }
        }

        if (double.IsNaN(Alpha) || Alpha <= 0 || Alpha >= 1 || double.IsNaN(NullValue))
        {
            Problem = "Enter α between 0 and 1 and the null value.";
            return;
        }

        IOneSampleTestMethod method = MethodScope.ActiveMethods(this).OfType<IOneSampleTestMethod>().First();

        try
        {
            (Outcome, Interval ci) = method.Compute(this);
            CiLower = ci.Lower;
            CiUpper = ci.Upper;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Problem = ex.Message;
            return;
        }

        string symbol = method.ParameterSymbol;
        string relation = AlternativeHypothesis switch { Alternative.Less => "<", Alternative.Greater => ">", _ => "≠" };
        Hypotheses = $"H₀: {symbol} = {NullValue:G6} vs H₁: {symbol} {relation} {NullValue:G6}";
        Statistic = Outcome.Statistic;
        DegreesOfFreedom = Outcome.DegreesOfFreedom;
        RejectionRegion = Outcome.RejectionRegion(method.StatisticSymbol);
        PValue = Outcome.PValue;
        Decision = Outcome.Reject
            ? $"Reject H₀ at α = {Alpha:G3}: p = {PValue:F4} < {Alpha:G3}"
            : $"Do not reject H₀ at α = {Alpha:G3}: p = {PValue:F4} ≥ {Alpha:G3}";

        if (TestKind is OneSampleTestKind.MeanSigmaKnown or OneSampleTestKind.MeanSigmaUnknown && !double.IsNaN(AlternativeValue))
        {
            double spread = TestKind == OneSampleTestKind.MeanSigmaKnown ? Sigma : SampleSd;
            Power = HypothesisTests.PowerForMean(NullValue, AlternativeValue, spread, (int)Math.Round(SampleSize), AlternativeHypothesis, Alpha);

            if (!double.IsNaN(TargetPower) && AlternativeValue != NullValue)
            {
                SampleSizeForPower = HypothesisTests.SampleSizeForPower(NullValue, AlternativeValue, spread, AlternativeHypothesis, Alpha, TargetPower);
            }
        }
    }

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName == nameof(Column) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Column) or nameof(Sample) ? DataSources.Table(this, Column) : null;

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Outcome is null)
        {
            return [];
        }

        IOneSampleTestMethod method = MethodScope.ActiveMethods(this).OfType<IOneSampleTestMethod>().First();
        return [InferenceFigures.TestStatistic($"{Name} — {Hypotheses}", method.Reference(this), Outcome, method.StatisticSymbol)];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        IOneSampleTestMethod method = MethodScope.ActiveMethods(this).OfType<IOneSampleTestMethod>().First();
        s.Imports(scipyStats: true);
        s.Comment(Hypotheses);
        s.Assign("alpha", Alpha).Assign("null_value", NullValue);
        s.Assign("alternative", AlternativeHypothesis switch { Alternative.Less => "'less'", Alternative.Greater => "'greater'", _ => "'two-sided'" });

        if (Source == DataSource.Columns)
        {
            s.Vector("x", Sample.YValues);
        }

        method.Declare(this, s);
        s.Line();
        s.PrintText("Hypotheses", Hypotheses);
        s.Print("Test statistic", "stat", "stat", 4);

        if (!double.IsNaN(DegreesOfFreedom))
        {
            s.PrintRaw("Degrees of freedom", "df", "df");
        }

        s.PrintText("Rejection region", RejectionRegion);
        s.Print("p-value", "p", "p", 4);
        s.Either("print(\"Decision: \" + (\"Reject H0\" if p < alpha else \"Do not reject H0\"))", "cat(\"Decision:\", if (p < alpha) \"Reject H0\" else \"Do not reject H0\", \"\\n\")");
        s.Print("Confidence interval lower", "ci_lower", "ci_lower", 4);
        s.Print("Confidence interval upper", "ci_upper", "ci_upper", 4);

        if (!double.IsNaN(Power))
        {
            s.Line();
            s.Comment("Power against μ₁ by the normal approximation (Montgomery & Runger §9-2.2)");
            s.Assign("mu1", AlternativeValue).Assign("spread", TestKind == OneSampleTestKind.MeanSigmaKnown ? Sigma : SampleSd);
            s.Either("shift = (mu1 - null_value) * np.sqrt(n) / spread", "shift <- (mu1 - null_value) * sqrt(n) / spread");

            switch (AlternativeHypothesis)
            {
                case Alternative.Greater:
                    s.Either("power = 1 - stats.norm.cdf(stats.norm.ppf(1 - alpha) - shift)", "power <- 1 - pnorm(qnorm(1 - alpha) - shift)");
                    break;
                case Alternative.Less:
                    s.Either("power = stats.norm.cdf(stats.norm.ppf(alpha) - shift)", "power <- pnorm(qnorm(alpha) - shift)");
                    break;
                default:
                    s.Either("z_half = stats.norm.ppf(1 - alpha / 2)", "z_half <- qnorm(1 - alpha / 2)");
                    s.Either("power = 1 - (stats.norm.cdf(z_half - shift) - stats.norm.cdf(-z_half - shift))", "power <- 1 - (pnorm(z_half - shift) - pnorm(-z_half - shift))");
                    break;
            }

            s.Print("Power at μ₁", "power", "power", 4);

            if (!double.IsNaN(SampleSizeForPower))
            {
                s.Assign("target_power", TargetPower);
                s.Either(
                    AlternativeHypothesis == Alternative.TwoSided ? "z_a = stats.norm.ppf(1 - alpha / 2)" : "z_a = stats.norm.ppf(1 - alpha)",
                    AlternativeHypothesis == Alternative.TwoSided ? "z_a <- qnorm(1 - alpha / 2)" : "z_a <- qnorm(1 - alpha)");
                s.Either("n_power = int(np.ceil(((z_a + stats.norm.ppf(target_power)) * spread / abs(mu1 - null_value)) ** 2))",
                         "n_power <- ceiling(((z_a + qnorm(target_power)) * spread / abs(mu1 - null_value))^2)");
                s.Comment("The formula's n, then raised until the power actually reaches the target (the app does the same)");
                s.PrintRaw("n for the target power", "n_power", "n_power");
            }
        }

        method.LibraryCheck(this, s);
        return s.ToString();
    }
}

/// <summary>A test kind is a method: its inputs, its computation, its reference distribution and its code.</summary>
public interface IOneSampleTestMethod : ICalculationMethod
{
    string ParameterSymbol { get; }

    string StatisticSymbol { get; }

    (TestOutcome Outcome, Interval Interval) Compute(OneSampleTest tool);

    ContinuousModel Reference(OneSampleTest tool);

    void Declare(OneSampleTest tool, ScriptBuilder script);

    void LibraryCheck(OneSampleTest tool, ScriptBuilder script);
}

public abstract class OneSampleTestMethod : CalculationMethod<OneSampleTest>, IOneSampleTestMethod
{
    public abstract OneSampleTestKind Selects { get; }

    public abstract string ParameterSymbol { get; }

    public abstract string StatisticSymbol { get; }

    public override bool IsApplicableTo(OneSampleTest subject) => subject is not null && subject.TestKind == Selects;

    public override string Citation =>
        "EGN3443 Modules 8–9 (Hypothesis Testing); Montgomery, D. C. and Runger, G. C., Applied " +
        "Statistics and Probability for Engineers, 7th ed., 2018, Chapter 9.";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(OneSampleTest.Hypotheses),
        nameof(OneSampleTest.Statistic),
        nameof(OneSampleTest.DegreesOfFreedom),
        nameof(OneSampleTest.RejectionRegion),
        nameof(OneSampleTest.PValue),
        nameof(OneSampleTest.Decision),
        nameof(OneSampleTest.CiLower),
        nameof(OneSampleTest.CiUpper),
    ];

    protected static IReadOnlyList<string> Shared =>
        [nameof(OneSampleTest.AlternativeHypothesis), nameof(OneSampleTest.Alpha), nameof(OneSampleTest.NullValue), nameof(OneSampleTest.SampleSize)];

    public override void Apply(OneSampleTest subject) => subject.Recalculate();

    public abstract (TestOutcome Outcome, Interval Interval) Compute(OneSampleTest tool);

    public abstract ContinuousModel Reference(OneSampleTest tool);

    public abstract void Declare(OneSampleTest tool, ScriptBuilder script);

    public virtual void LibraryCheck(OneSampleTest tool, ScriptBuilder script)
    {
    }

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

    /// <summary>The p-value and two-sided critical values in the script, for a symmetric reference.</summary>
    protected static void SymmetricTail(ScriptBuilder s, string dist, string dfArg)
    {
        // scipy's stats.t / stats.norm are R's pt / pnorm.
        var r = dist[(dist.LastIndexOf('.') + 1)..];
        s.Either(
            $"p = 2 * {dist}.sf(abs(stat){dfArg}) if alternative == 'two-sided' else ({dist}.cdf(stat{dfArg}) if alternative == 'less' else {dist}.sf(stat{dfArg}))",
            $"p <- if (alternative == 'two-sided') 2 * p{r}(abs(stat){dfArg}, lower.tail = FALSE) else if (alternative == 'less') p{r}(stat{dfArg}) else p{r}(stat{dfArg}, lower.tail = FALSE)");
    }
}

public sealed class ZTestMethod : OneSampleTestMethod
{
    public override OneSampleTestKind Selects => OneSampleTestKind.MeanSigmaKnown;

    public override string Name => "z-test on a mean, σ known";

    public override string Description => "z = (x̄ − μ₀)/(σ/√n) against the standard normal; CI x̄ ± z_{α/2}σ/√n; power by the normal approximation.";

    public override string ParameterSymbol => "μ";

    public override string StatisticSymbol => "z";

    public override IReadOnlyList<string> Inputs =>
        [.. Shared, nameof(OneSampleTest.SampleMean), nameof(OneSampleTest.Sigma), nameof(OneSampleTest.AlternativeValue), nameof(OneSampleTest.TargetPower)];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(OneSampleTest.Power), nameof(OneSampleTest.SampleSizeForPower)];

    public override (TestOutcome, Interval) Compute(OneSampleTest t)
    {
        int n = Count(t.SampleSize, "n");
        double sigma = Positive(t.Sigma, "σ"), mean = Given(t.SampleMean, "x̄");
        return (HypothesisTests.ZTestMean(mean, sigma, n, t.NullValue, t.AlternativeHypothesis, t.Alpha), Intervals.MeanSigmaKnown(mean, sigma, n, 1 - t.Alpha));
    }

    public override ContinuousModel Reference(OneSampleTest t) => new NormalModel(0, 1);

    public override void Declare(OneSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("xbar, n = np.mean(x), len(x)", "xbar <- mean(x); n <- length(x)");
        }
        else
        {
            s.Assign("xbar", t.SampleMean).Assign("n", (int)t.SampleSize);
        }

        s.Assign("sigma", t.Sigma);
        s.Either("se = sigma / np.sqrt(n)", "se <- sigma / sqrt(n)");
        s.Either("stat = (xbar - null_value) / se", "stat <- (xbar - null_value) / se");
        SymmetricTail(s, "stats.norm", string.Empty);
        s.Either("ci_lower, ci_upper = xbar - stats.norm.ppf(1 - alpha / 2) * se, xbar + stats.norm.ppf(1 - alpha / 2) * se",
                 "ci_lower <- xbar - qnorm(1 - alpha / 2) * se; ci_upper <- xbar + qnorm(1 - alpha / 2) * se");
    }
}

public sealed class TTestMethod : OneSampleTestMethod
{
    public override OneSampleTestKind Selects => OneSampleTestKind.MeanSigmaUnknown;

    public override string Name => "t-test on a mean";

    public override string Description => "t = (x̄ − μ₀)/(s/√n) against t with n − 1 degrees of freedom; CI x̄ ± t_{α/2,n−1}s/√n.";

    public override string ParameterSymbol => "μ";

    public override string StatisticSymbol => "t";

    public override IReadOnlyList<string> Inputs =>
        [.. Shared, nameof(OneSampleTest.SampleMean), nameof(OneSampleTest.SampleSd), nameof(OneSampleTest.AlternativeValue), nameof(OneSampleTest.TargetPower)];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(OneSampleTest.Power), nameof(OneSampleTest.SampleSizeForPower)];

    public override (TestOutcome, Interval) Compute(OneSampleTest t)
    {
        int n = Count(t.SampleSize, "n");
        double s = Positive(t.SampleSd, "s"), mean = Given(t.SampleMean, "x̄");
        return (HypothesisTests.TTestMean(mean, s, n, t.NullValue, t.AlternativeHypothesis, t.Alpha), Intervals.MeanSigmaUnknown(mean, s, n, 1 - t.Alpha));
    }

    public override ContinuousModel Reference(OneSampleTest t) => new StudentTModel(Math.Max(1, t.SampleSize - 1));

    public override void Declare(OneSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("xbar, s, n = np.mean(x), np.std(x, ddof=1), len(x)", "xbar <- mean(x); s <- sd(x); n <- length(x)");
        }
        else
        {
            s.Assign("xbar", t.SampleMean).Assign("s", t.SampleSd).Assign("n", (int)t.SampleSize);
        }

        s.Assign("df", "n - 1");
        s.Either("se = s / np.sqrt(n)", "se <- s / sqrt(n)");
        s.Either("stat = (xbar - null_value) / se", "stat <- (xbar - null_value) / se");
        SymmetricTail(s, "stats.t", ", df");
        s.Either("ci_lower, ci_upper = xbar - stats.t.ppf(1 - alpha / 2, df) * se, xbar + stats.t.ppf(1 - alpha / 2, df) * se",
                 "ci_lower <- xbar - qt(1 - alpha / 2, df) * se; ci_upper <- xbar + qt(1 - alpha / 2, df) * se");
    }

    public override void LibraryCheck(OneSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Comment("Library check: the same t and p straight from the data");
            s.Either("print(\"Library check:\", stats.ttest_1samp(x, null_value, alternative=alternative))",
                     "print(t.test(x, mu = null_value, alternative = alternative, conf.level = 1 - alpha))");
        }
    }
}

public sealed class ProportionTestMethod : OneSampleTestMethod
{
    public override OneSampleTestKind Selects => OneSampleTestKind.Proportion;

    public override string Name => "z-test on a proportion";

    public override string Description => "z = (p̂ − p₀)/√(p₀(1 − p₀)/n), no continuity correction; Wald CI p̂ ± z_{α/2}√(p̂(1 − p̂)/n).";

    public override string ParameterSymbol => "p";

    public override string StatisticSymbol => "z";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(OneSampleTest.Successes)];

    public override (TestOutcome, Interval) Compute(OneSampleTest t)
    {
        int n = Count(t.SampleSize, "n", 1), k = Count(t.Successes, "the successes", 0);

        if (k > n)
        {
            throw new ArgumentOutOfRangeException(nameof(t.Successes), "The successes cannot exceed the sample size.");
        }

        if (t.NullValue <= 0 || t.NullValue >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(t.NullValue), "The null proportion p₀ must be between 0 and 1.");
        }

        return (HypothesisTests.ZTestProportion(k, n, t.NullValue, t.AlternativeHypothesis, t.Alpha), Intervals.Proportion(k, n, 1 - t.Alpha));
    }

    public override ContinuousModel Reference(OneSampleTest t) => new NormalModel(0, 1);

    public override void Declare(OneSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("k, n = int(np.sum(x != 0)), len(x)", "k <- sum(x != 0); n <- length(x)");
        }
        else
        {
            s.Assign("k", (int)t.Successes).Assign("n", (int)t.SampleSize);
        }

        s.Assign("p_hat", "k / n");
        s.Either("stat = (p_hat - null_value) / np.sqrt(null_value * (1 - null_value) / n)", "stat <- (p_hat - null_value) / sqrt(null_value * (1 - null_value) / n)");
        SymmetricTail(s, "stats.norm", string.Empty);
        s.Either("se_hat = np.sqrt(p_hat * (1 - p_hat) / n)", "se_hat <- sqrt(p_hat * (1 - p_hat) / n)");
        s.Either("ci_lower, ci_upper = max(0, p_hat - stats.norm.ppf(1 - alpha / 2) * se_hat), min(1, p_hat + stats.norm.ppf(1 - alpha / 2) * se_hat)",
                 "ci_lower <- max(0, p_hat - qnorm(1 - alpha / 2) * se_hat); ci_upper <- min(1, p_hat + qnorm(1 - alpha / 2) * se_hat)");
    }

    public override void LibraryCheck(OneSampleTest t, ScriptBuilder s)
    {
        s.Comment("Library check: statsmodels' proportions_ztest / R's prop.test (correct = FALSE) give the same z and p");
        s.Either("# from statsmodels.stats.proportion import proportions_ztest; proportions_ztest(k, n, null_value, alternative='larger' if alternative == 'greater' else ('smaller' if alternative == 'less' else 'two-sided'))",
                 "print(prop.test(k, n, p = null_value, alternative = alternative, correct = FALSE))");
    }
}

public sealed class VarianceTestMethod : OneSampleTestMethod
{
    public override OneSampleTestKind Selects => OneSampleTestKind.Variance;

    public override string Name => "χ²-test on a variance";

    public override string Description => "χ² = (n − 1)s²/σ₀² against χ² with n − 1 degrees of freedom; the null value is σ₀ (the app squares it).";

    public override string ParameterSymbol => "σ";

    public override string StatisticSymbol => "χ²";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(OneSampleTest.SampleSd)];

    public override (TestOutcome, Interval) Compute(OneSampleTest t)
    {
        int n = Count(t.SampleSize, "n");
        double s = Positive(t.SampleSd, "s"), sigma0 = Positive(t.NullValue, "σ₀");
        Interval v = Intervals.Variance(s * s, n, 1 - t.Alpha);
        return (HypothesisTests.ChiSquareTestVariance(s * s, n, sigma0 * sigma0, t.AlternativeHypothesis, t.Alpha), new Interval(s, double.NaN, v.Critical, double.NaN, Math.Sqrt(v.Lower), Math.Sqrt(v.Upper), n - 1));
    }

    public override ContinuousModel Reference(OneSampleTest t) => new ChiSquaredModel(Math.Max(1, t.SampleSize - 1));

    public override void Declare(OneSampleTest t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("s, n = np.std(x, ddof=1), len(x)", "s <- sd(x); n <- length(x)");
        }
        else
        {
            s.Assign("s", t.SampleSd).Assign("n", (int)t.SampleSize);
        }

        s.Assign("df", "n - 1");
        s.Either("stat = df * s**2 / null_value**2", "stat <- df * s^2 / null_value^2");
        s.Either("lower_tail = stats.chi2.cdf(stat, df)", "lower_tail <- pchisq(stat, df)");
        s.Either("p = min(1, 2 * min(lower_tail, 1 - lower_tail)) if alternative == 'two-sided' else (lower_tail if alternative == 'less' else 1 - lower_tail)",
                 "p <- if (alternative == 'two-sided') min(1, 2 * min(lower_tail, 1 - lower_tail)) else if (alternative == 'less') lower_tail else 1 - lower_tail");
        s.Comment("Confidence interval for σ: square roots of the χ² interval for σ²");
        s.Either("ci_lower, ci_upper = np.sqrt(df * s**2 / stats.chi2.ppf(1 - alpha / 2, df)), np.sqrt(df * s**2 / stats.chi2.ppf(alpha / 2, df))",
                 "ci_lower <- sqrt(df * s^2 / qchisq(1 - alpha / 2, df)); ci_upper <- sqrt(df * s^2 / qchisq(alpha / 2, df))");
    }
}

/// <summary>Summaries typed by hand.</summary>
public sealed class OneSampleSummaryEntered : CalculationMethod<OneSampleTest>
{
    public override string Name => "Summary statistics entered";

    public override string Description => "The sample statistics are typed into the analysis.";

    public override string Citation => "Statistics+ entry.";

    public override bool IsApplicableTo(OneSampleTest subject) => subject is not null && subject.Source == DataSource.Summary;

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [];

    public override void Apply(OneSampleTest subject)
    {
    }
}

/// <summary>Summaries from a data column, locked while the column is chosen.</summary>
public sealed class OneSampleColumnSource : CalculationMethod<OneSampleTest>
{
    public override string Name => "From a data table column";

    public override string Description => "x̄, s and n (or the successes and n) are computed from the chosen column.";

    public override string Citation => "Statistics+ project data.";

    public override bool IsApplicableTo(OneSampleTest subject) => subject is not null && subject.Source == DataSource.Columns;

    public override IReadOnlyList<string> Inputs => [nameof(OneSampleTest.Column)];

    public override IReadOnlyList<string> Outputs => [nameof(OneSampleTest.SampleMean), nameof(OneSampleTest.SampleSd), nameof(OneSampleTest.SampleSize), nameof(OneSampleTest.Successes)];

    public override IReadOnlyList<string> Generates => Outputs;

    public override void Apply(OneSampleTest subject) => subject.Recalculate();
}
