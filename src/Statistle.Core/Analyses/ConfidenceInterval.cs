using System.ComponentModel;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Descriptive;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum IntervalFor
{
    [Description("A mean, σ known (z)")]
    MeanSigmaKnown = 0,

    [Description("A mean, σ unknown (t)")]
    MeanSigmaUnknown,

    [Description("A proportion (z)")]
    Proportion,

    [Description("A variance and standard deviation (χ²)")]
    Variance,

    [Description("Difference of two means, equal variances (pooled t)")]
    TwoMeansPooled,

    [Description("Difference of two means, unequal variances (Welch t)")]
    TwoMeansWelch,

    [Description("Mean of paired differences (t)")]
    PairedMeans,

    [Description("Difference of two proportions (z)")]
    TwoProportions,
}

/// <summary>Where the numbers come from: typed summary statistics, or columns of a data table.</summary>
public enum DataSource
{
    [Description("Summary statistics entered here")]
    Summary = 0,

    [Description("Columns of a data table")]
    Columns,
}

/// <summary>
/// EGN3443 Module 7 (and the two-sample intervals of Module 10): a confidence interval for a
/// mean, a proportion, a variance, or a difference — from summary statistics or straight from
/// data columns — with every piece the course asks the student to show.
/// </summary>
[StatistleHelp(
    "A confidence interval (Module 7): a range of plausible values for a population parameter, " +
    "estimate ± critical value × standard error. Choose what the interval is for — a mean with " +
    "σ known (z) or unknown (t), a proportion, a variance, or the difference of two means (pooled " +
    "when the variances are alike, Welch when not, paired when each observation in one sample " +
    "matches one in the other) or of two proportions — and give the summary statistics, or " +
    "point at columns of a data table and the summaries are computed for you. Read the " +
    "standard error, the critical value, the margin of error and the two ends. A 95 % interval " +
    "means: of many such intervals from repeated samples, 95 % would contain the true value — " +
    "not that there is a 95 % chance the true value is in this one. The Code tab writes the " +
    "same computation in Python and R and, with data columns, the library call that checks it.")]
public sealed class ConfidenceInterval : Analysis, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, IHasFigures, ICodeSource
{
    private IntervalFor _target = IntervalFor.MeanSigmaUnknown;
    private DataSource _source = DataSource.Summary;
    private double _confidence = 0.95;
    private string _column = string.Empty;
    private string _column2 = string.Empty;
    private double _sampleMean = 13.71;
    private double _sampleSd = 3.55;
    private double _sigma = 1;
    private double _sampleSize = 22;
    private double _successes = 10;
    private double _sample2Mean = 92.733;
    private double _sample2Sd = 2.98;
    private double _sample2Size = 8;
    private double _successes2 = 19;

    private double _estimate = double.NaN;
    private double _standardError = double.NaN;
    private double _critical = double.NaN;
    private double _degreesOfFreedom = double.NaN;
    private double _marginOfError = double.NaN;
    private double _lower = double.NaN;
    private double _upper = double.NaN;
    private double _sdLower = double.NaN;
    private double _sdUpper = double.NaN;
    private double _wilsonLower = double.NaN;
    private double _wilsonUpper = double.NaN;
    private string _statement = string.Empty;

    public ConfidenceInterval()
    {
        Name = "Confidence interval";
        Recalculate();
    }

    public override string Title => "Confidence interval";

    public override string Summary => "An interval for a mean, proportion, variance or difference (Module 7)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 30;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    // ── What and where from ───────────────────────────────────────────────────

    [StatistleProperty(Label = "Interval for", Role = VariableRole.Design,
        Help = "The parameter. Use z for a mean only when σ is genuinely known from long " +
               "experience; with s from the sample use t — the difference matters below n ≈ 30. " +
               "Pooled t assumes the two populations have the same variance (the F-test or a " +
               "ratio of s under 2 supports that); Welch does not. Paired when the two samples " +
               "are measurements on the same items.")]
    public IntervalFor Target
    {
        get => _target;
        set => SetProperty(ref _target, value);
    }

    [StatistleProperty(Label = "Data from", Role = VariableRole.Design,
        Help = "Enter the summary statistics (x̄, s, n; successes and n) yourself, or choose " +
               "columns of a data table and they are computed for you and shown, locked. For a " +
               "proportion from a column, code the observations 1 (success) and 0.")]
    public DataSource Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    [StatistleProperty(Label = "Confidence level", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.5, Max = 0.9999,
        Help = "0.90, 0.95 (the usual) or 0.99. The higher the confidence, the wider the interval: " +
               "certainty is bought with precision.")]
    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    [StatistleProperty(Label = "Column (sample 1)", Role = VariableRole.Input,
        Help = "The data table column holding the sample (or the first sample, or the 'before' " +
               "measurements of a paired design).")]
    public string Column
    {
        get => _column;
        set => SetProperty(ref _column, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Column (sample 2)", Role = VariableRole.Input,
        Help = "The column holding the second sample, or the 'after' measurements paired row by row with the first.")]
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

    // ── Summary statistics ────────────────────────────────────────────────────

    [StatistleProperty(Label = "Sample mean (x̄)", Decimals = 4, Role = VariableRole.Input,
        Help = "The mean of the sample (of the differences, for a paired design).")]
    public double SampleMean
    {
        get => _sampleMean;
        set => SetProperty(ref _sampleMean, value);
    }

    [StatistleProperty(Label = "Sample standard deviation (s)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The sample standard deviation, dividing by n − 1 (of the differences, for a paired design).")]
    public double SampleSd
    {
        get => _sampleSd;
        set => SetProperty(ref _sampleSd, value);
    }

    [StatistleProperty(Label = "Population σ", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The known population standard deviation — from long production history, a " +
               "calibrated instrument, or the problem statement.")]
    public double Sigma
    {
        get => _sigma;
        set => SetProperty(ref _sigma, value);
    }

    [StatistleProperty(Label = "Sample size (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 2,
        Help = "How many observations (pairs, for a paired design). The standard error shrinks as 1/√n.")]
    public double SampleSize
    {
        get => _sampleSize;
        set => SetProperty(ref _sampleSize, value);
    }

    [StatistleProperty(Label = "Successes (x)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0,
        Help = "How many of the n items had the attribute — the defectives, the failures, the yeses. p̂ = x/n.")]
    public double Successes
    {
        get => _successes;
        set => SetProperty(ref _successes, value);
    }

    [StatistleProperty(Label = "Sample 2 mean (x̄₂)", Decimals = 4, Role = VariableRole.Input,
        Help = "The mean of the second sample.")]
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

    [StatistleProperty(Label = "Sample 2 size (n₂)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 2,
        Help = "How many observations in the second sample.")]
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

    [StatistleProperty(Label = "Point estimate", Decimals = 4, Role = VariableRole.Output,
        Help = "The sample statistic the interval is built around: x̄, p̂, s², x̄₁ − x̄₂, d̄ or p̂₁ − p̂₂.")]
    public double Estimate
    {
        get => _estimate;
        set => SetProperty(ref _estimate, value);
    }

    [StatistleProperty(Label = "Standard error", Decimals = 4, Role = VariableRole.Output,
        Help = "The standard deviation of the estimate: σ/√n or s/√n for a mean, √(p̂(1 − p̂)/n) " +
               "for a proportion, √(s₁²/n₁ + s₂²/n₂) or s_p√(1/n₁ + 1/n₂) for a difference. " +
               "Blank for a variance, whose interval is not symmetric.")]
    public double StandardError
    {
        get => _standardError;
        set => SetProperty(ref _standardError, value);
    }

    [StatistleProperty(Label = "Critical value", Decimals = 4, Role = VariableRole.Output,
        Help = "z_{α/2} or t_{α/2,ν} — the multiplier of the standard error (for a variance, " +
               "χ²_{α/2,ν}, the divisor for the lower end). Python stats.norm.ppf / stats.t.ppf; R qnorm / qt.")]
    public double Critical
    {
        get => _critical;
        set => SetProperty(ref _critical, value);
    }

    [StatistleProperty(Label = "Degrees of freedom", Decimals = 2, Role = VariableRole.Calculated,
        Help = "n − 1 for one sample, n₁ + n₂ − 2 pooled, the Welch–Satterthwaite value (not " +
               "usually a whole number) for Welch. Blank for a z interval.")]
    public double DegreesOfFreedom
    {
        get => _degreesOfFreedom;
        set => SetProperty(ref _degreesOfFreedom, value);
    }

    [StatistleProperty(Label = "Margin of error", Decimals = 4, Role = VariableRole.Output,
        Help = "Critical value × standard error: the half-width of the interval, the ± in " +
               "'x̄ ± E'. Blank for a variance.")]
    public double MarginOfError
    {
        get => _marginOfError;
        set => SetProperty(ref _marginOfError, value);
    }

    [StatistleProperty(Label = "Lower limit", Decimals = 4, Role = VariableRole.Output,
        Help = "The lower end of the interval.")]
    public double Lower
    {
        get => _lower;
        set => SetProperty(ref _lower, value);
    }

    [StatistleProperty(Label = "Upper limit", Decimals = 4, Role = VariableRole.Output,
        Help = "The upper end of the interval.")]
    public double Upper
    {
        get => _upper;
        set => SetProperty(ref _upper, value);
    }

    [StatistleProperty(Label = "Lower limit for σ", Decimals = 4, Role = VariableRole.Output,
        Help = "The square root of the variance interval's lower end: the interval for the standard deviation.")]
    public double SdLower
    {
        get => _sdLower;
        set => SetProperty(ref _sdLower, value);
    }

    [StatistleProperty(Label = "Upper limit for σ", Decimals = 4, Role = VariableRole.Output,
        Help = "The square root of the variance interval's upper end.")]
    public double SdUpper
    {
        get => _sdUpper;
        set => SetProperty(ref _sdUpper, value);
    }

    [StatistleProperty(Label = "Wilson score lower", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The lower end of Wilson's score interval — what R's prop.test reports. Better " +
               "than the Wald interval above when n is small or p̂ is near 0 or 1; the course's " +
               "hand formula is Wald.")]
    public double WilsonLower
    {
        get => _wilsonLower;
        set => SetProperty(ref _wilsonLower, value);
    }

    [StatistleProperty(Label = "Wilson score upper", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The upper end of Wilson's score interval.")]
    public double WilsonUpper
    {
        get => _wilsonUpper;
        set => SetProperty(ref _wilsonUpper, value);
    }

    [StatistleProperty(Label = "Interval", Role = VariableRole.Output,
        Help = "The interval as it is written in a report.")]
    public string Statement
    {
        get => _statement;
        set => SetProperty(ref _statement, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    [StatistleIgnore]
    public bool TwoSample => Target is IntervalFor.TwoMeansPooled or IntervalFor.TwoMeansWelch or IntervalFor.PairedMeans or IntervalFor.TwoProportions;

    [StatistleIgnore]
    public Interval? Result { get; private set; }

    public override void Recalculate()
    {
        Problem = null;
        Result = null;
        Estimate = StandardError = Critical = DegreesOfFreedom = MarginOfError = Lower = Upper = double.NaN;
        SdLower = SdUpper = WilsonLower = WilsonUpper = double.NaN;
        Statement = string.Empty;

        if (Source == DataSource.Columns)
        {
            Problem = PullColumns();

            if (Problem is not null)
            {
                return;
            }
        }

        if (double.IsNaN(Confidence) || Confidence <= 0 || Confidence >= 1)
        {
            Problem = "Enter a confidence level between 0 and 1.";
            return;
        }

        try
        {
            Result = MethodScope.ActiveMethods(this).OfType<IIntervalMethod>().First().Compute(this);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Problem = ex.Message;
            return;
        }

        Interval r = Result;
        Estimate = r.Estimate;
        StandardError = r.StandardError;
        Critical = r.Critical;
        DegreesOfFreedom = r.DegreesOfFreedom;
        MarginOfError = r.MarginOfError;
        Lower = r.Lower;
        Upper = r.Upper;

        if (Target == IntervalFor.Variance)
        {
            SdLower = Math.Sqrt(r.Lower);
            SdUpper = Math.Sqrt(r.Upper);
        }

        if (Target == IntervalFor.Proportion)
        {
            Interval wilson = Intervals.ProportionWilson((int)Math.Round(Successes), (int)Math.Round(SampleSize), Confidence);
            WilsonLower = wilson.Lower;
            WilsonUpper = wilson.Upper;
        }

        string parameter = Target switch
        {
            IntervalFor.MeanSigmaKnown or IntervalFor.MeanSigmaUnknown => "μ",
            IntervalFor.Proportion => "p",
            IntervalFor.Variance => "σ²",
            IntervalFor.PairedMeans => "μ_d",
            IntervalFor.TwoProportions => "p₁ − p₂",
            _ => "μ₁ − μ₂",
        };
        Statement = $"{Confidence * 100:G4} % CI for {parameter}: {r.Lower:G6} to {r.Upper:G6}";
    }

    /// <summary>Fills the summary statistics from the chosen columns; locked in the editor while a column is chosen.</summary>
    private string? PullColumns()
    {
        string? problem = DataSources.Pull(this, Column, Sample);

        if (problem is not null || Sample.Count == 0)
        {
            return problem ?? DataSources.ChooseOrAdd(this, "Choose the column holding the sample.");
        }

        double[] x = Sample.YValues;

        if (TwoSample)
        {
            problem = DataSources.Pull(this, Column2, Sample2);

            if (problem is not null || Sample2.Count == 0)
            {
                return problem ?? DataSources.ChooseOrAdd(this, "Choose the column holding the second sample.");
            }
        }

        double[] y = Sample2.YValues;

        switch (Target)
        {
            case IntervalFor.PairedMeans:
                if (x.Length != y.Length)
                {
                    return $"A paired design needs the same number of values in both columns ({x.Length} and {y.Length}).";
                }

                double[] d = [.. x.Zip(y, (a, b) => a - b)];
                SampleMean = SummaryStatistics.Mean(d);
                SampleSd = SummaryStatistics.SampleStandardDeviation(d);
                SampleSize = d.Length;
                break;

            case IntervalFor.Proportion:
                Successes = x.Count(v => v != 0);
                SampleSize = x.Length;
                break;

            case IntervalFor.TwoProportions:
                Successes = x.Count(v => v != 0);
                SampleSize = x.Length;
                Successes2 = y.Count(v => v != 0);
                Sample2Size = y.Length;
                break;

            default:
                SampleMean = SummaryStatistics.Mean(x);
                SampleSd = SummaryStatistics.SampleStandardDeviation(x);
                SampleSize = x.Length;

                if (TwoSample)
                {
                    Sample2Mean = SummaryStatistics.Mean(y);
                    Sample2Sd = SummaryStatistics.SampleStandardDeviation(y);
                    Sample2Size = y.Length;
                }

                break;
        }

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

    /// <summary>The interval drawn as a bar with the estimate on it.</summary>
    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units) =>
        Result is null ? [] : [InferenceFigures.IntervalBar($"{Name} — {Statement}", Result)];

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        IIntervalMethod method = MethodScope.ActiveMethods(this).OfType<IIntervalMethod>().First();
        s.Imports(scipyStats: true);
        s.Assign("confidence", Confidence);
        s.Either("alpha = 1 - confidence", "alpha <- 1 - confidence");

        if (Source == DataSource.Columns)
        {
            s.Comment("The data, then the summaries the interval is built from");
            s.Vector("x", Sample.YValues);

            if (TwoSample)
            {
                s.Vector("y", Sample2.YValues);
            }
        }

        method.Declare(this, s);
        s.Line();
        s.Print("Point estimate", "estimate", "estimate", 4);

        if (Target != IntervalFor.Variance)
        {
            s.Print("Standard error", "se", "se", 4);
        }

        s.Print("Critical value", "crit", "crit", 4);

        if (!double.IsNaN(DegreesOfFreedom))
        {
            s.Print("Degrees of freedom", "df", "df", 2);
        }

        if (Target != IntervalFor.Variance)
        {
            s.Print("Margin of error", "crit * se", "crit * se", 4);
        }

        s.Print("Lower limit", "lower", "lower", 4);
        s.Print("Upper limit", "upper", "upper", 4);

        if (Target == IntervalFor.Variance)
        {
            s.Print("Lower limit for σ", "np.sqrt(lower)", "sqrt(lower)", 4);
            s.Print("Upper limit for σ", "np.sqrt(upper)", "sqrt(upper)", 4);
        }

        if (Target == IntervalFor.Proportion)
        {
            s.Comment("Wilson's score interval — what R's prop.test and statsmodels' proportion_confint(method='wilson') give");
            s.Either("centre = (p_hat + crit**2 / (2 * n)) / (1 + crit**2 / n)", "centre <- (p_hat + crit^2 / (2 * n)) / (1 + crit^2 / n)");
            s.Either("half = crit * np.sqrt(p_hat * (1 - p_hat) / n + crit**2 / (4 * n**2)) / (1 + crit**2 / n)", "half <- crit * sqrt(p_hat * (1 - p_hat) / n + crit^2 / (4 * n^2)) / (1 + crit^2 / n)");
            s.Print("Wilson score lower", "centre - half", "centre - half", 4);
            s.Print("Wilson score upper", "centre + half", "centre + half", 4);
        }

        s.PrintText("Interval", Statement);
        method.LibraryCheck(this, s);

        return s.ToString();
    }
}

/// <summary>An interval kind is a method: it names its inputs, computes the interval, and writes its code.</summary>
public interface IIntervalMethod : ICalculationMethod
{
    Interval Compute(ConfidenceInterval tool);

    /// <summary>Assigns the summaries and computes estimate, se, crit, (df,) lower, upper in the script.</summary>
    void Declare(ConfidenceInterval tool, ScriptBuilder script);

    /// <summary>With data columns, the library call that gives the same interval.</summary>
    void LibraryCheck(ConfidenceInterval tool, ScriptBuilder script);
}

public abstract class IntervalMethod : CalculationMethod<ConfidenceInterval>, IIntervalMethod
{
    public abstract IntervalFor Selects { get; }

    public override bool IsApplicableTo(ConfidenceInterval subject) => subject is not null && subject.Target == Selects;

    public override string Citation =>
        "EGN3443 Module 7 (Point Estimation and Confidence Intervals); Montgomery, D. C. and Runger, G. C., " +
        "Applied Statistics and Probability for Engineers, 7th ed., 2018, Chapter 8.";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(ConfidenceInterval.Estimate),
        nameof(ConfidenceInterval.StandardError),
        nameof(ConfidenceInterval.Critical),
        nameof(ConfidenceInterval.DegreesOfFreedom),
        nameof(ConfidenceInterval.MarginOfError),
        nameof(ConfidenceInterval.Lower),
        nameof(ConfidenceInterval.Upper),
        nameof(ConfidenceInterval.Statement),
    ];

    public override void Apply(ConfidenceInterval subject) => subject.Recalculate();

    public abstract Interval Compute(ConfidenceInterval tool);

    public abstract void Declare(ConfidenceInterval tool, ScriptBuilder script);

    public virtual void LibraryCheck(ConfidenceInterval tool, ScriptBuilder script)
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

    protected static void DeclareZ(ScriptBuilder s) =>
        s.Either("crit = stats.norm.ppf(1 - alpha / 2)", "crit <- qnorm(1 - alpha / 2)");

    protected static void DeclareT(ScriptBuilder s) =>
        s.Either("crit = stats.t.ppf(1 - alpha / 2, df)", "crit <- qt(1 - alpha / 2, df)");

    protected static void Ends(ScriptBuilder s) =>
        s.Either("lower, upper = estimate - crit * se, estimate + crit * se", "lower <- estimate - crit * se; upper <- estimate + crit * se");
}

public sealed class MeanSigmaKnownInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.MeanSigmaKnown;

    public override string Name => "Mean, σ known";

    public override string Description => "x̄ ± z_{α/2}·σ/√n.";

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.Sigma), nameof(ConfidenceInterval.SampleSize)];

    public override Interval Compute(ConfidenceInterval t) =>
        Intervals.MeanSigmaKnown(Given(t.SampleMean, "the sample mean"), Positive(t.Sigma, "σ"), Count(t.SampleSize, "n"), t.Confidence);

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("xbar", t.SampleMean).Assign("sigma", t.Sigma).Assign("n", (int)t.SampleSize);
        s.Assign("estimate", "xbar");
        s.Either("se = sigma / np.sqrt(n)", "se <- sigma / sqrt(n)");
        DeclareZ(s);
        Ends(s);
    }
}

public sealed class MeanSigmaUnknownInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.MeanSigmaUnknown;

    public override string Name => "Mean, σ unknown";

    public override string Description => "x̄ ± t_{α/2,n−1}·s/√n.";

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize)];

    public override Interval Compute(ConfidenceInterval t) =>
        Intervals.MeanSigmaUnknown(Given(t.SampleMean, "the sample mean"), Positive(t.SampleSd, "s"), Count(t.SampleSize, "n"), t.Confidence);

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("xbar", t.SampleMean).Assign("s", t.SampleSd).Assign("n", (int)t.SampleSize);
        s.Assign("estimate", "xbar").Assign("df", "n - 1");
        s.Either("se = s / np.sqrt(n)", "se <- s / sqrt(n)");
        DeclareT(s);
        Ends(s);
    }

    public override void LibraryCheck(ConfidenceInterval t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Comment("The same interval from the library, straight from the data");
            s.Either("print(\"Library check:\", stats.t.interval(confidence, len(x) - 1, loc=np.mean(x), scale=stats.sem(x)))",
                     "cat(\"Library check:\", t.test(x, conf.level = confidence)$conf.int, \"\\n\")");
        }
    }
}

public sealed class ProportionInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.Proportion;

    public override string Name => "Proportion";

    public override string Description => "p̂ ± z_{α/2}·√(p̂(1 − p̂)/n) (Wald), with Wilson's score interval beside it.";

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.Successes), nameof(ConfidenceInterval.SampleSize)];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(ConfidenceInterval.WilsonLower), nameof(ConfidenceInterval.WilsonUpper)];

    public override Interval Compute(ConfidenceInterval t)
    {
        int n = Count(t.SampleSize, "n", 1), k = Count(t.Successes, "the successes", 0);

        if (k > n)
        {
            throw new ArgumentOutOfRangeException(nameof(t.Successes), "The successes cannot exceed the sample size.");
        }

        return Intervals.Proportion(k, n, t.Confidence);
    }

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("k", (int)t.Successes).Assign("n", (int)t.SampleSize);
        s.Assign("p_hat", "k / n").Assign("estimate", "p_hat");
        s.Either("se = np.sqrt(p_hat * (1 - p_hat) / n)", "se <- sqrt(p_hat * (1 - p_hat) / n)");
        DeclareZ(s);
        s.Comment("The Wald interval, held inside [0, 1]");
        s.Either("lower, upper = max(0, estimate - crit * se), min(1, estimate + crit * se)", "lower <- max(0, estimate - crit * se); upper <- min(1, estimate + crit * se)");
    }
}

public sealed class VarianceInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.Variance;

    public override string Name => "Variance and standard deviation";

    public override string Description => "(n − 1)s²/χ²_{α/2,n−1} ≤ σ² ≤ (n − 1)s²/χ²_{1−α/2,n−1}; square roots for σ.";

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize)];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(ConfidenceInterval.SdLower), nameof(ConfidenceInterval.SdUpper)];

    public override Interval Compute(ConfidenceInterval t)
    {
        double s = Positive(t.SampleSd, "s");
        return Intervals.Variance(s * s, Count(t.SampleSize, "n"), t.Confidence);
    }

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("s", t.SampleSd).Assign("n", (int)t.SampleSize).Assign("df", "n - 1");
        s.Either("estimate = s**2", "estimate <- s^2");
        s.Either("crit = stats.chi2.ppf(1 - alpha / 2, df)", "crit <- qchisq(1 - alpha / 2, df)");
        s.Either("lower = df * s**2 / crit", "lower <- df * s^2 / crit");
        s.Either("upper = df * s**2 / stats.chi2.ppf(alpha / 2, df)", "upper <- df * s^2 / qchisq(alpha / 2, df)");
    }
}

public sealed class TwoMeansPooledInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.TwoMeansPooled;

    public override string Name => "Difference of two means, pooled";

    public override string Description => "(x̄₁ − x̄₂) ± t_{α/2,n₁+n₂−2}·s_p√(1/n₁ + 1/n₂).";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-2.3 (confidence interval on the difference in means, variances unknown and equal).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize),
         nameof(ConfidenceInterval.Sample2Mean), nameof(ConfidenceInterval.Sample2Sd), nameof(ConfidenceInterval.Sample2Size)];

    public override Interval Compute(ConfidenceInterval t) =>
        Intervals.TwoMeansPooled(Given(t.SampleMean, "x̄₁"), Positive(t.SampleSd, "s₁"), Count(t.SampleSize, "n₁"), Given(t.Sample2Mean, "x̄₂"), Positive(t.Sample2Sd, "s₂"), Count(t.Sample2Size, "n₂"), t.Confidence);

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("xbar1", t.SampleMean).Assign("s1", t.SampleSd).Assign("n1", (int)t.SampleSize);
        s.Assign("xbar2", t.Sample2Mean).Assign("s2", t.Sample2Sd).Assign("n2", (int)t.Sample2Size);
        s.Assign("estimate", "xbar1 - xbar2").Assign("df", "n1 + n2 - 2");
        s.Either("sp = np.sqrt(((n1 - 1) * s1**2 + (n2 - 1) * s2**2) / df)", "sp <- sqrt(((n1 - 1) * s1^2 + (n2 - 1) * s2^2) / df)");
        s.Either("se = sp * np.sqrt(1 / n1 + 1 / n2)", "se <- sp * sqrt(1 / n1 + 1 / n2)");
        DeclareT(s);
        Ends(s);
    }

    public override void LibraryCheck(ConfidenceInterval t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Comment("Library check: the pooled t-test's confidence interval");
            s.Either("print(\"Library check:\", stats.ttest_ind(x, y, equal_var=True).confidence_interval(confidence))",
                     "cat(\"Library check:\", t.test(x, y, var.equal = TRUE, conf.level = confidence)$conf.int, \"\\n\")");
        }
    }
}

public sealed class TwoMeansWelchInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.TwoMeansWelch;

    public override string Name => "Difference of two means, Welch";

    public override string Description => "(x̄₁ − x̄₂) ± t_{α/2,ν}·√(s₁²/n₁ + s₂²/n₂), ν by Welch–Satterthwaite.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-2.2 (variances unknown and unequal).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize),
         nameof(ConfidenceInterval.Sample2Mean), nameof(ConfidenceInterval.Sample2Sd), nameof(ConfidenceInterval.Sample2Size)];

    public override Interval Compute(ConfidenceInterval t) =>
        Intervals.TwoMeansWelch(Given(t.SampleMean, "x̄₁"), Positive(t.SampleSd, "s₁"), Count(t.SampleSize, "n₁"), Given(t.Sample2Mean, "x̄₂"), Positive(t.Sample2Sd, "s₂"), Count(t.Sample2Size, "n₂"), t.Confidence);

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("xbar1", t.SampleMean).Assign("s1", t.SampleSd).Assign("n1", (int)t.SampleSize);
        s.Assign("xbar2", t.Sample2Mean).Assign("s2", t.Sample2Sd).Assign("n2", (int)t.Sample2Size);
        s.Assign("estimate", "xbar1 - xbar2");
        s.Either("a, b = s1**2 / n1, s2**2 / n2", "a <- s1^2 / n1; b <- s2^2 / n2");
        s.Either("se = np.sqrt(a + b)", "se <- sqrt(a + b)");
        s.Comment("Welch–Satterthwaite degrees of freedom");
        s.Either("df = (a + b)**2 / (a**2 / (n1 - 1) + b**2 / (n2 - 1))", "df <- (a + b)^2 / (a^2 / (n1 - 1) + b^2 / (n2 - 1))");
        DeclareT(s);
        Ends(s);
    }

    public override void LibraryCheck(ConfidenceInterval t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Comment("Library check: Welch's t-test confidence interval (the default in both libraries)");
            s.Either("print(\"Library check:\", stats.ttest_ind(x, y, equal_var=False).confidence_interval(confidence))",
                     "cat(\"Library check:\", t.test(x, y, conf.level = confidence)$conf.int, \"\\n\")");
        }
    }
}

public sealed class PairedMeansInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.PairedMeans;

    public override string Name => "Mean of paired differences";

    public override string Description => "d̄ ± t_{α/2,n−1}·s_d/√n on the differences d = x − y.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-4 (paired t-test).";

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize)];

    public override Interval Compute(ConfidenceInterval t) =>
        Intervals.MeanSigmaUnknown(Given(t.SampleMean, "the mean difference d̄"), Positive(t.SampleSd, "s_d"), Count(t.SampleSize, "the number of pairs"), t.Confidence);

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Comment("The paired differences");
            s.Either("d = x - y", "d <- x - y");
            s.Either("dbar, s_d, n = np.mean(d), np.std(d, ddof=1), len(d)", "dbar <- mean(d); s_d <- sd(d); n <- length(d)");
        }
        else
        {
            s.Assign("dbar", t.SampleMean).Assign("s_d", t.SampleSd).Assign("n", (int)t.SampleSize);
        }

        s.Assign("estimate", "dbar").Assign("df", "n - 1");
        s.Either("se = s_d / np.sqrt(n)", "se <- s_d / sqrt(n)");
        DeclareT(s);
        Ends(s);
    }

    public override void LibraryCheck(ConfidenceInterval t, ScriptBuilder s)
    {
        if (t.Source == DataSource.Columns)
        {
            s.Either("print(\"Library check:\", stats.ttest_rel(x, y).confidence_interval(confidence))",
                     "cat(\"Library check:\", t.test(x, y, paired = TRUE, conf.level = confidence)$conf.int, \"\\n\")");
        }
    }
}

public sealed class TwoProportionsInterval : IntervalMethod
{
    public override IntervalFor Selects => IntervalFor.TwoProportions;

    public override string Name => "Difference of two proportions";

    public override string Description => "(p̂₁ − p̂₂) ± z_{α/2}·√(p̂₁(1−p̂₁)/n₁ + p̂₂(1−p̂₂)/n₂).";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-6.2 (confidence interval on the difference in proportions).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(ConfidenceInterval.Confidence), nameof(ConfidenceInterval.Successes), nameof(ConfidenceInterval.SampleSize), nameof(ConfidenceInterval.Successes2), nameof(ConfidenceInterval.Sample2Size)];

    public override Interval Compute(ConfidenceInterval t)
    {
        int n1 = Count(t.SampleSize, "n₁", 1), k1 = Count(t.Successes, "x₁", 0), n2 = Count(t.Sample2Size, "n₂", 1), k2 = Count(t.Successes2, "x₂", 0);

        if (k1 > n1 || k2 > n2)
        {
            throw new ArgumentOutOfRangeException(nameof(t.Successes), "The successes cannot exceed the sample size.");
        }

        return Intervals.TwoProportions(k1, n1, k2, n2, t.Confidence);
    }

    public override void Declare(ConfidenceInterval t, ScriptBuilder s)
    {
        s.Assign("k1", (int)t.Successes).Assign("n1", (int)t.SampleSize).Assign("k2", (int)t.Successes2).Assign("n2", (int)t.Sample2Size);
        s.Either("p1, p2 = k1 / n1, k2 / n2", "p1 <- k1 / n1; p2 <- k2 / n2");
        s.Assign("estimate", "p1 - p2");
        s.Either("se = np.sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2)", "se <- sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2)");
        DeclareZ(s);
        Ends(s);
    }
}

/// <summary>Summaries typed by hand: nothing generated, nothing locked.</summary>
public sealed class SummaryEntered : CalculationMethod<ConfidenceInterval>
{
    public override string Name => "Summary statistics entered";

    public override string Description => "The sample statistics are typed into the analysis.";

    public override string Citation => "Statistle entry.";

    public override bool IsApplicableTo(ConfidenceInterval subject) => subject is not null && subject.Source == DataSource.Summary;

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [];

    public override void Apply(ConfidenceInterval subject)
    {
    }
}

/// <summary>Summaries computed from one data column; the column's values and the summaries are locked.</summary>
public sealed class OneColumnSource : CalculationMethod<ConfidenceInterval>
{
    public override string Name => "From a data table column";

    public override string Description => "x̄, s and n (or the successes and n) are computed from the chosen column.";

    public override string Citation => "Statistle project data.";

    public override bool IsApplicableTo(ConfidenceInterval subject) => subject is not null && subject.Source == DataSource.Columns && !subject.TwoSample;

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Column)];

    public override IReadOnlyList<string> Outputs => [nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize), nameof(ConfidenceInterval.Successes)];

    public override IReadOnlyList<string> Generates => Outputs;

    public override void Apply(ConfidenceInterval subject) => subject.Recalculate();
}

/// <summary>Summaries computed from two data columns.</summary>
public sealed class TwoColumnSource : CalculationMethod<ConfidenceInterval>
{
    public override string Name => "From two data table columns";

    public override string Description => "Both samples' summaries (or the paired differences') are computed from the chosen columns.";

    public override string Citation => "Statistle project data.";

    public override bool IsApplicableTo(ConfidenceInterval subject) => subject is not null && subject.Source == DataSource.Columns && subject.TwoSample;

    public override IReadOnlyList<string> Inputs => [nameof(ConfidenceInterval.Column), nameof(ConfidenceInterval.Column2)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(ConfidenceInterval.SampleMean), nameof(ConfidenceInterval.SampleSd), nameof(ConfidenceInterval.SampleSize), nameof(ConfidenceInterval.Successes),
        nameof(ConfidenceInterval.Sample2Mean), nameof(ConfidenceInterval.Sample2Sd), nameof(ConfidenceInterval.Sample2Size), nameof(ConfidenceInterval.Successes2),
    ];

    public override IReadOnlyList<string> Generates => Outputs;

    public override void Apply(ConfidenceInterval subject) => subject.Recalculate();
}
