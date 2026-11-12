using System.ComponentModel;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum SamplingStatistic
{
    [Description("Sample mean x̄")]
    SampleMean = 0,

    [Description("Sample proportion p̂")]
    SampleProportion,
}

/// <summary>What the question says about the population's shape (it decides whether x̄ is normal for a small n).</summary>
public enum PopulationShape
{
    [Description("Not known (rely on n ≥ 30)")]
    NotKnown = 0,

    [Description("Normal")]
    Normal,
}

/// <summary>Which area the figure shades for the value of interest (the probabilities are all calculated either way).</summary>
public enum ShadedTail
{
    [Description("Below the value (≤)")]
    Below = 0,

    [Description("Above the value (≥)")]
    Above,
}

/// <summary>
/// EGN3443 Module 6: the sampling distribution of the sample mean (and the sample proportion)
/// — its centre, its standard error, the probability of a sample statistic by the central limit
/// theorem, and how the spread falls with the sample size.
/// </summary>
[StatistleHelp(
    "The sampling distribution of a statistic (Module 6): what values the sample mean x̄ (or " +
    "the sample proportion p̂) takes from sample to sample. Enter the population's μ and σ (or " +
    "p) and the sample size n. The mean of x̄ is μ and its standard error is σ/√n — the spread " +
    "of the mean, not of the data — and by the central limit theorem x̄ is close to normal " +
    "once n is 30 or so whatever the population's shape (or for any n when the population is " +
    "normal). Enter a value of x̄ of interest and read the probability of a sample mean below, " +
    "above or between values, and the middle 95 % range. The Plots tab draws the population " +
    "and the sampling distribution on one axis so the √n narrowing can be seen. For p̂ the " +
    "standard error is √(p(1 − p)/n), with np and n(1 − p) both at least 10 for the normal " +
    "approximation. The Code tab does the same with scipy.stats.norm and R's pnorm.")]
public sealed class SamplingDistribution : Analysis, IReportsProblem, IHasFigures, ICodeSource
{
    private SamplingStatistic _statistic = SamplingStatistic.SampleMean;
    private double _populationMean = 4200;
    private double _populationSigma = 350;
    private double _populationProportion = 0.15;
    private double _sampleSize = 30;
    private double _value = 4300;
    private double _value2 = double.NaN;
    private PopulationShape _shape = PopulationShape.NotKnown;
    private ShadedTail _shade = ShadedTail.Below;

    private double _centre = double.NaN;
    private double _standardError = double.NaN;
    private double _z = double.NaN;
    private double _atMost = double.NaN;
    private double _atLeast = double.NaN;
    private double _between = double.NaN;
    private double _lower95 = double.NaN;
    private double _upper95 = double.NaN;
    private string _condition = string.Empty;

    public SamplingDistribution()
    {
        Name = "Sampling distribution";
        Recalculate();
    }

    public override string Title => "Sampling distribution";

    public override string Summary => "The mean and standard error of x̄ or p̂, and CLT probabilities (Module 6)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 23;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public string? Problem { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Statistic", Role = VariableRole.Design,
        Help = "The sample mean of a measurement, or the sample proportion of items with some " +
               "attribute (defective, over the limit).")]
    public SamplingStatistic Statistic
    {
        get => _statistic;
        set => SetProperty(ref _statistic, value);
    }

    [StatistleProperty(Label = "Population mean (μ)", Decimals = 4, Role = VariableRole.Input,
        Help = "The mean of the population the samples come from — known or assumed for the question.")]
    public double PopulationMean
    {
        get => _populationMean;
        set => SetProperty(ref _populationMean, value);
    }

    [StatistleProperty(Label = "Population standard deviation (σ)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The spread of individual values in the population. The sample mean's spread is " +
               "this divided by √n.")]
    public double PopulationSigma
    {
        get => _populationSigma;
        set => SetProperty(ref _populationSigma, value);
    }

    [StatistleProperty(Label = "Population proportion (p)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The fraction of the population with the attribute — the true defect rate.")]
    public double PopulationProportion
    {
        get => _populationProportion;
        set => SetProperty(ref _populationProportion, value);
    }

    [StatistleProperty(Label = "Sample size (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1,
        Help = "How many observations each sample has. Quadrupling n halves the standard error.")]
    public double SampleSize
    {
        get => _sampleSize;
        set => SetProperty(ref _sampleSize, value);
    }

    [StatistleProperty(Label = "Value of interest (x̄ or p̂)", Decimals = 4, Role = VariableRole.Input,
        Help = "A sample statistic the question is about: 'the probability a sample of 30 " +
               "averages more than 4300 psi'. Leave blank for the distribution alone.")]
    public double Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    [StatistleProperty(Label = "Upper value, optional", Decimals = 4, Role = VariableRole.Input,
        Help = "For 'between': the upper end of the range of the statistic.")]
    public double Value2
    {
        get => _value2;
        set => SetProperty(ref _value2, value);
    }

    [StatistleProperty(Label = "Population shape", Role = VariableRole.Design,
        Help = "Normal, when the question says the measurement is normally distributed: then x̄ is exactly normal " +
               "for any n. Not known, otherwise: the central limit theorem makes x̄ close to normal once n is about 30.")]
    public PopulationShape Shape
    {
        get => _shape;
        set => SetProperty(ref _shape, value);
    }

    [StatistleProperty(Label = "Shade on the figure", Role = VariableRole.Design,
        Help = "Which side of the value of interest the Plots tab shades: below for 'less than', above for " +
               "'more than'. With an upper value, the figure shades between the two. Every probability is " +
               "calculated either way.")]
    public ShadedTail Shade
    {
        get => _shade;
        set => SetProperty(ref _shade, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Mean of the sampling distribution", Decimals = 4, Role = VariableRole.Output,
        Help = "E[x̄] = μ, or E[p̂] = p: the statistic is unbiased — on average it hits the parameter.")]
    public double Centre
    {
        get => _centre;
        set => SetProperty(ref _centre, value);
    }

    [StatistleProperty(Label = "Standard error", Decimals = 4, Role = VariableRole.Output,
        Help = "σ/√n for the mean, √(p(1 − p)/n) for the proportion: the standard deviation of " +
               "the statistic from sample to sample. Python sigma / np.sqrt(n); R sigma / sqrt(n).")]
    public double StandardError
    {
        get => _standardError;
        set => SetProperty(ref _standardError, value);
    }

    [StatistleProperty(Label = "z for the value", Decimals = 4, Role = VariableRole.Output,
        Help = "(value − center) / standard error: how many standard errors the sample statistic " +
               "lies from its expectation. Compare with the z-score of a single observation, " +
               "which divides by σ instead.")]
    public double Z
    {
        get => _z;
        set => SetProperty(ref _z, value);
    }

    [StatistleProperty(Label = "P(statistic ≤ value)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of a sample statistic at or below the value, Φ(z), by the " +
               "central limit theorem. Python stats.norm(mu, se).cdf(value); R pnorm(value, mu, se).")]
    public double AtMost
    {
        get => _atMost;
        set => SetProperty(ref _atMost, value);
    }

    [StatistleProperty(Label = "P(statistic ≥ value)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of a sample statistic at or above the value, 1 − Φ(z).")]
    public double AtLeast
    {
        get => _atLeast;
        set => SetProperty(ref _atLeast, value);
    }

    [StatistleProperty(Label = "P(value ≤ statistic ≤ upper)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of a sample statistic between the two values. Blank without an upper value.")]
    public double Between
    {
        get => _between;
        set => SetProperty(ref _between, value);
    }

    [StatistleProperty(Label = "Middle 95 % from", Decimals = 4, Role = VariableRole.Calculated,
        Help = "center − 1.96 × standard error: 95 % of sample statistics fall between this and the value below.")]
    public double Lower95
    {
        get => _lower95;
        set => SetProperty(ref _lower95, value);
    }

    [StatistleProperty(Label = "Middle 95 % to", Decimals = 4, Role = VariableRole.Calculated,
        Help = "center + 1.96 × standard error.")]
    public double Upper95
    {
        get => _upper95;
        set => SetProperty(ref _upper95, value);
    }

    [StatistleProperty(Label = "Normal approximation", Role = VariableRole.Output,
        Help = "Whether the central limit theorem's conditions are met: n ≥ 30 for a mean (or a " +
               "normal population at any n), np ≥ 10 and n(1 − p) ≥ 10 for a proportion. When " +
               "they are not, the probabilities above are rough.")]
    public string Condition
    {
        get => _condition;
        set => SetProperty(ref _condition, value);
    }

    public override void Recalculate()
    {
        Problem = null;
        Centre = StandardError = Z = AtMost = AtLeast = Between = Lower95 = Upper95 = double.NaN;
        Condition = string.Empty;

        if (double.IsNaN(SampleSize) || SampleSize < 1)
        {
            Problem = "Enter the sample size, n ≥ 1.";
            return;
        }

        int n = (int)Math.Round(SampleSize);
        double centre, se;

        if (Statistic == SamplingStatistic.SampleMean)
        {
            if (double.IsNaN(PopulationMean) || double.IsNaN(PopulationSigma))
            {
                Problem = "Enter the population mean and standard deviation.";
                return;
            }

            if (PopulationSigma <= 0)
            {
                Problem = "The population standard deviation must be greater than zero.";
                return;
            }

            centre = PopulationMean;
            se = PopulationSigma / Math.Sqrt(n);
            Condition = Shape == PopulationShape.Normal
                ? $"The population is normal, so x̄ is exactly normal for any n (here n = {n})."
                : n >= 30
                    ? $"n = {n} ≥ 30: the central limit theorem makes x̄ close to normal whatever the population's shape."
                    : $"n = {n} < 30: x̄ is normal only if the population is (approximately) normal; otherwise treat these probabilities as rough.";
        }
        else
        {
            if (double.IsNaN(PopulationProportion) || PopulationProportion <= 0 || PopulationProportion >= 1)
            {
                Problem = "Enter a population proportion between 0 and 1, exclusive.";
                return;
            }

            centre = PopulationProportion;
            se = Math.Sqrt(PopulationProportion * (1 - PopulationProportion) / n);
            double np = n * PopulationProportion, nq = n * (1 - PopulationProportion);
            Condition = np >= 10 && nq >= 10
                ? $"np = {np:G4} and n(1 − p) = {nq:G4} are both ≥ 10: the normal approximation to p̂ is adequate."
                : $"np = {np:G4}, n(1 − p) = {nq:G4}: at least one is below 10, so the normal approximation to p̂ is poor — use the binomial.";
        }

        Centre = centre;
        StandardError = se;
        Lower95 = centre - 1.959964 * se;
        Upper95 = centre + 1.959964 * se;

        if (double.IsNaN(Value))
        {
            return;
        }

        var sampling = new NormalModel(centre, se);
        Z = (Value - centre) / se;
        AtMost = sampling.Cdf(Value);
        AtLeast = 1 - AtMost;

        if (!double.IsNaN(Value2))
        {
            if (Value2 < Value)
            {
                Problem = "The upper value must not be below the value of interest.";
                return;
            }

            Between = sampling.Between(Value, Value2);
        }
    }

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (double.IsNaN(StandardError))
        {
            return [];
        }

        int n = (int)Math.Round(SampleSize);
        bool above = double.IsNaN(Value2) && Shade == ShadedTail.Above;
        double from = double.IsNaN(Value2) ? (above ? Value : double.NegativeInfinity) : Value;
        double to = double.IsNaN(Value2) ? (above ? double.PositiveInfinity : Value) : Value2;

        return Statistic == SamplingStatistic.SampleMean
            ? [DistributionFigures.SamplingOfTheMean($"{Name} — x̄ for n = {n}", PopulationMean, PopulationSigma, n, from, to)]
            : [DistributionFigures.Density($"{Name} — p̂ for n = {n}", new NormalModel(Centre, StandardError),
                double.IsNegativeInfinity(from) ? Centre - 4 * StandardError : from,
                double.IsPositiveInfinity(to) ? Centre + 4 * StandardError : to)];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        s.Imports(scipyStats: true);
        bool mean = Statistic == SamplingStatistic.SampleMean;

        if (mean)
        {
            s.Comment("Sampling distribution of the sample mean: center μ, standard error σ/√n");
            s.Assign("mu", PopulationMean).Assign("sigma", PopulationSigma).Assign("n", (int)Math.Round(double.IsNaN(SampleSize) ? 1 : SampleSize));
            s.Assign("center", "mu");
            s.Either("se = sigma / np.sqrt(n)", "se <- sigma / sqrt(n)");
        }
        else
        {
            s.Comment("Sampling distribution of the sample proportion: center p, standard error √(p(1 − p)/n)");
            s.Assign("p", PopulationProportion).Assign("n", (int)Math.Round(double.IsNaN(SampleSize) ? 1 : SampleSize));
            s.Assign("center", "p");
            s.Either("se = np.sqrt(p * (1 - p) / n)", "se <- sqrt(p * (1 - p) / n)");
        }

        s.Either("S = stats.norm(center, se)   # the sampling distribution, by the central limit theorem", "# pnorm(value, center, se) is the sampling distribution, by the central limit theorem");
        s.Line();
        s.Print("Mean of the sampling distribution", "center", "center", 4);
        s.Print("Standard error", "se", "se", 4);

        if (!double.IsNaN(Value))
        {
            s.Assign("value", Value);

            if (!double.IsNaN(Value2))
            {
                s.Assign("upper", Value2);
            }

            s.Print("z for the value", "(value - center) / se", "(value - center) / se", 4);
            s.Print("P(statistic ≤ value)", "S.cdf(value)", "pnorm(value, center, se)", 4);
            s.Print("P(statistic ≥ value)", "S.sf(value)", "1 - pnorm(value, center, se)", 4);

            if (!double.IsNaN(Value2))
            {
                s.Print("P(value ≤ statistic ≤ upper)", "S.cdf(upper) - S.cdf(value)", "pnorm(upper, center, se) - pnorm(value, center, se)", 4);
            }
        }

        s.Print("Middle 95 % from", "center - 1.959964 * se", "center - qnorm(0.975) * se", 4);
        s.Print("Middle 95 % to", "center + 1.959964 * se", "center + qnorm(0.975) * se", 4);
        s.PrintText("Normal approximation", Condition);

        return s.ToString();
    }
}

/// <summary>Each statistic is a method naming its parameters; both cite the central limit theorem.</summary>
public abstract class SamplingStatisticMethod : CalculationMethod<SamplingDistribution>
{
    public abstract SamplingStatistic Selects { get; }

    public override bool IsApplicableTo(SamplingDistribution subject) => subject is not null && subject.Statistic == Selects;

    public override string Citation =>
        "EGN3443 Module 6 (Sampling Distributions); Montgomery, D. C. and Runger, G. C., Applied " +
        "Statistics and Probability for Engineers, 7th ed., 2018, §7-2 (sampling distributions and " +
        "the central limit theorem).";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SamplingDistribution.Centre),
        nameof(SamplingDistribution.StandardError),
        nameof(SamplingDistribution.Z),
        nameof(SamplingDistribution.AtMost),
        nameof(SamplingDistribution.AtLeast),
        nameof(SamplingDistribution.Between),
        nameof(SamplingDistribution.Lower95),
        nameof(SamplingDistribution.Upper95),
        nameof(SamplingDistribution.Condition),
    ];

    protected static IReadOnlyList<string> Shared =>
        [nameof(SamplingDistribution.SampleSize), nameof(SamplingDistribution.Value), nameof(SamplingDistribution.Value2)];

    public override void Apply(SamplingDistribution subject) => subject.Recalculate();
}

public sealed class SampleMeanStatistic : SamplingStatisticMethod
{
    public override SamplingStatistic Selects => SamplingStatistic.SampleMean;

    public override string Name => "Sampling distribution of the mean";

    public override string Description => "x̄ ~ N(μ, σ²/n) approximately, for n ≥ 30 or a normal population; z = (x̄ − μ)/(σ/√n).";

    public override IReadOnlyList<string> Inputs => [nameof(SamplingDistribution.PopulationMean), nameof(SamplingDistribution.PopulationSigma), .. Shared];
}

public sealed class SampleProportionStatistic : SamplingStatisticMethod
{
    public override SamplingStatistic Selects => SamplingStatistic.SampleProportion;

    public override string Name => "Sampling distribution of a proportion";

    public override string Description => "p̂ ~ N(p, p(1 − p)/n) approximately when np ≥ 10 and n(1 − p) ≥ 10.";

    public override IReadOnlyList<string> Inputs => [nameof(SamplingDistribution.PopulationProportion), .. Shared];
}
