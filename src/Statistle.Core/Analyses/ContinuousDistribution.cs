using System.ComponentModel;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum ContinuousFamily
{
    [Description("Uniform — equally likely between a and b")]
    Uniform = 0,

    [Description("Normal — the bell curve, μ and σ")]
    Normal,

    [Description("Exponential — waiting time at rate λ")]
    Exponential,

    [Description("Lognormal — ln X is normal")]
    LogNormal,

    [Description("Student's t — ν degrees of freedom")]
    StudentT,

    [Description("Chi-squared — ν degrees of freedom")]
    ChiSquared,

    [Description("F — ν₁ and ν₂ degrees of freedom")]
    F,
}

/// <summary>
/// EGN3443 Module 5: a continuous random variable — density, the probability below, above and
/// between values, the quantile for a probability, z-scores and the empirical rule for the
/// normal — for the uniform, normal, exponential and lognormal, and the t, χ² and F
/// distributions the later modules lean on.
/// </summary>
[StatistleHelp(
    "A continuous probability distribution (Module 5). Choose the family, enter its parameters, " +
    "a value x (and an upper value x₂ for a range) and a probability q, and read the density " +
    "f(x), P(X ≤ x), P(X > x), P(x ≤ X ≤ x₂), the quantile with P(X ≤ x) = q, and the mean, " +
    "variance, standard deviation and median. A single point has probability zero for a " +
    "continuous variable; only intervals do, as areas under the density. The normal adds the " +
    "z-score and the 68–95–99.7 rule. The uniform is 'any value in the range equally likely', " +
    "the exponential the waiting time between Poisson events (memoryless), the lognormal a " +
    "variable whose logarithm is normal (fatigue life, particle size). Student's t, χ² and F " +
    "are the sampling distributions the confidence intervals and tests of Modules 7 to 14 use " +
    "— enter q = 0.975 to read the two-sided 5 % critical value. The Plots tab shades the " +
    "probability asked for under the density; the Code tab does the same in scipy.stats and R.")]
public sealed class ContinuousDistribution : Analysis, IReportsProblem, IHasFigures, ICodeSource
{
    private ContinuousFamily _family = ContinuousFamily.Normal;
    private double _lower = 0;
    private double _upper = 10;
    private double _mu = 50;
    private double _sigma = 5;
    private double _rate = 0.5;
    private double _logMu = 3;
    private double _logSigma = 0.4;
    private double _degreesOfFreedom = 10;
    private double _numeratorDf = 5;
    private double _denominatorDf = 10;
    private double _x = 55;
    private double _x2 = 60;
    private double _probability = 0.95;

    private string _model = string.Empty;
    private double _density = double.NaN;
    private double _atMost = double.NaN;
    private double _moreThan = double.NaN;
    private double _between = double.NaN;
    private double _zScore = double.NaN;
    private double _quantile = double.NaN;
    private double _mean = double.NaN;
    private double _variance = double.NaN;
    private double _standardDeviation = double.NaN;
    private double _median = double.NaN;
    private string _empiricalRule = string.Empty;

    public ContinuousDistribution()
    {
        Name = "Continuous distribution";
        Recalculate();
    }

    public override string Title => "Continuous distribution";

    public override string Summary => "Uniform, normal, exponential, lognormal, t, χ² or F: probabilities and quantiles (Module 5)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 22;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public ContinuousModel? Model { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Distribution", Role = VariableRole.Design,
        Help = "The family. Measurements that pile up around a centre with symmetric scatter are " +
               "normal; times between random events are exponential; a quantity that is a " +
               "product of many small factors is lognormal; anything equally likely across a " +
               "range is uniform. t, χ² and F describe statistics computed from samples, not " +
               "measurements. Only the chosen family's parameters are shown.")]
    public ContinuousFamily Family
    {
        get => _family;
        set => SetProperty(ref _family, value);
    }

    [StatistleProperty(Label = "Lower limit (a)", Decimals = 4, Role = VariableRole.Design,
        Help = "The smallest possible value of a uniform variable.")]
    public double Lower
    {
        get => _lower;
        set => SetProperty(ref _lower, value);
    }

    [StatistleProperty(Label = "Upper limit (b)", Decimals = 4, Role = VariableRole.Design,
        Help = "The largest possible value of a uniform variable; f(x) = 1/(b − a) between them.")]
    public double Upper
    {
        get => _upper;
        set => SetProperty(ref _upper, value);
    }

    [StatistleProperty(Label = "Mean (μ)", Decimals = 4, Role = VariableRole.Design,
        Help = "The centre of the normal distribution — its mean, median and mode all at once.")]
    public double Mu
    {
        get => _mu;
        set => SetProperty(ref _mu, value);
    }

    [StatistleProperty(Label = "Standard deviation (σ)", Decimals = 4, Role = VariableRole.Design, Min = 0,
        Help = "The spread of the normal: 68 % of values lie within one σ of μ, 95 % within two, " +
               "99.7 % within three.")]
    public double Sigma
    {
        get => _sigma;
        set => SetProperty(ref _sigma, value);
    }

    [StatistleProperty(Label = "Rate (λ)", Decimals = 4, Role = VariableRole.Design, Min = 0,
        Help = "Events per unit time (or length) for the exponential; the mean waiting time is " +
               "1/λ. The same λ as the Poisson count of those events. R and this app use the " +
               "rate; scipy uses the scale 1/λ.")]
    public double Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
    }

    [StatistleProperty(Label = "Mean of ln X (μ)", Decimals = 4, Role = VariableRole.Design,
        Help = "The lognormal's first parameter: the mean of the logarithm of X, not of X itself " +
               "(the mean of X is e^(μ + σ²/2)). R's meanlog.")]
    public double LogMu
    {
        get => _logMu;
        set => SetProperty(ref _logMu, value);
    }

    [StatistleProperty(Label = "Standard deviation of ln X (σ)", Decimals = 4, Role = VariableRole.Design, Min = 0,
        Help = "The lognormal's second parameter: the standard deviation of ln X. R's sdlog.")]
    public double LogSigma
    {
        get => _logSigma;
        set => SetProperty(ref _logSigma, value);
    }

    [StatistleProperty(Label = "Degrees of freedom (ν)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1,
        Help = "For t and χ²: usually n − 1 for a sample of n. The t approaches the normal as ν grows " +
               "(by ν = 30 the difference is small).")]
    public double DegreesOfFreedom
    {
        get => _degreesOfFreedom;
        set => SetProperty(ref _degreesOfFreedom, value);
    }

    [StatistleProperty(Label = "Numerator degrees of freedom (ν₁)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1,
        Help = "For F: the degrees of freedom of the variance in the numerator (between groups in ANOVA).")]
    public double NumeratorDf
    {
        get => _numeratorDf;
        set => SetProperty(ref _numeratorDf, value);
    }

    [StatistleProperty(Label = "Denominator degrees of freedom (ν₂)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1,
        Help = "For F: the degrees of freedom of the variance in the denominator (within groups, error).")]
    public double DenominatorDf
    {
        get => _denominatorDf;
        set => SetProperty(ref _denominatorDf, value);
    }

    [StatistleProperty(Label = "Value of interest (x)", Decimals = 4, Role = VariableRole.Input,
        Help = "The value the question is about: 'the probability the part measures under x', " +
               "'the chance the wait exceeds x'.")]
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    [StatistleProperty(Label = "Upper value (x₂), optional", Decimals = 4, Role = VariableRole.Input,
        Help = "For 'between x and x₂': the upper end. Leave blank when the question has one value.")]
    public double X2
    {
        get => _x2;
        set => SetProperty(ref _x2, value);
    }

    [StatistleProperty(Label = "Probability for the quantile (q)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The inverse question: the value x with P(X ≤ x) = q. q = 0.95 gives the 95th " +
               "percentile; for a two-sided 5 % critical value of t or χ² use q = 0.975 (and " +
               "0.025 for the lower one).")]
    public double Probability
    {
        get => _probability;
        set => SetProperty(ref _probability, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Model", Role = VariableRole.Calculated,
        Help = "The distribution with the parameters in force.")]
    public string ModelDescription
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }

    [StatistleProperty(Label = "Density f(x)", Decimals = 4, Role = VariableRole.Output,
        Help = "The height of the density curve at x. Not a probability — P(X = x) is 0 — but " +
               "f(x)·dx is the probability of a small interval at x. Python X.pdf(x); R dnorm(x, μ, σ).")]
    public double Density
    {
        get => _density;
        set => SetProperty(ref _density, value);
    }

    [StatistleProperty(Label = "P(X ≤ x)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The cumulative probability: the area under the density to the left of x. For a " +
               "continuous variable P(X < x) is the same number. Python X.cdf(x); R pnorm(x, μ, σ).")]
    public double AtMost
    {
        get => _atMost;
        set => SetProperty(ref _atMost, value);
    }

    [StatistleProperty(Label = "P(X > x)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The upper tail, 1 − P(X ≤ x) — the area to the right of x. Python X.sf(x); R 1 − pnorm(x, μ, σ) or pnorm(x, μ, σ, lower.tail = FALSE).")]
    public double MoreThan
    {
        get => _moreThan;
        set => SetProperty(ref _moreThan, value);
    }

    [StatistleProperty(Label = "P(x ≤ X ≤ x₂)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The area between the two values: P(X ≤ x₂) − P(X ≤ x). Blank without an x₂.")]
    public double Between
    {
        get => _between;
        set => SetProperty(ref _between, value);
    }

    [StatistleProperty(Label = "z-score of x", Decimals = 4, Role = VariableRole.Output,
        Help = "(x − μ)/σ: how many standard deviations x lies from the mean. It is the value " +
               "looked up in a standard normal table: P(X ≤ x) = Φ(z).")]
    public double ZScore
    {
        get => _zScore;
        set => SetProperty(ref _zScore, value);
    }

    [StatistleProperty(Label = "Quantile x with P(X ≤ x) = q", Decimals = 4, Role = VariableRole.Output,
        Help = "The inverse of the cumulative probability — the percentile, or the critical value " +
               "of a test statistic. Python X.ppf(q); R qnorm(q, μ, σ), qt(q, ν), qchisq(q, ν), qf(q, ν₁, ν₂).")]
    public double Quantile
    {
        get => _quantile;
        set => SetProperty(ref _quantile, value);
    }

    [StatistleProperty(Label = "Mean", Decimals = 4, Role = VariableRole.Output,
        Help = "E[X]: (a + b)/2 uniform, μ normal, 1/λ exponential, e^(μ+σ²/2) lognormal, 0 for t " +
               "(ν > 1), ν for χ², ν₂/(ν₂ − 2) for F (ν₂ > 2).")]
    public double Mean
    {
        get => _mean;
        set => SetProperty(ref _mean, value);
    }

    [StatistleProperty(Label = "Variance", Decimals = 4, Role = VariableRole.Output,
        Help = "V[X]: (b − a)²/12 uniform, σ² normal, 1/λ² exponential, (e^σ² − 1)e^(2μ+σ²) " +
               "lognormal, ν/(ν − 2) for t (ν > 2), 2ν for χ². Blank where the family has none.")]
    public double Variance
    {
        get => _variance;
        set => SetProperty(ref _variance, value);
    }

    [StatistleProperty(Label = "Standard deviation", Decimals = 4, Role = VariableRole.Output,
        Help = "√V[X]: the spread of X in its own units. Blank where the family has no finite variance.")]
    public double StandardDeviation
    {
        get => _standardDeviation;
        set => SetProperty(ref _standardDeviation, value);
    }

    [StatistleProperty(Label = "Median", Decimals = 4, Role = VariableRole.Output,
        Help = "The value with half the probability on each side, the 0.5 quantile. Equal to the " +
               "mean for symmetric families; below it for the right-skewed exponential and lognormal.")]
    public double Median
    {
        get => _median;
        set => SetProperty(ref _median, value);
    }

    [StatistleProperty(Label = "Empirical rule", Role = VariableRole.Calculated,
        Help = "For the normal: the ranges μ ± σ, μ ± 2σ and μ ± 3σ that hold 68.3 %, 95.4 % and " +
               "99.7 % of the values — the quick check every engineer carries in their head.")]
    public string EmpiricalRule
    {
        get => _empiricalRule;
        set => SetProperty(ref _empiricalRule, value);
    }

    public override void Recalculate()
    {
        Problem = null;
        Model = null;
        ModelDescription = EmpiricalRule = string.Empty;
        Density = AtMost = MoreThan = Between = ZScore = Quantile = Mean = Variance = StandardDeviation = Median = double.NaN;

        try
        {
            Model = MethodScope.ActiveMethods(this).OfType<IContinuousFamily>().FirstOrDefault()?.Build(this);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Problem = ex.Message;
            return;
        }

        if (Model is null)
        {
            Problem = "Enter the distribution's parameters.";
            return;
        }

        ModelDescription = Model.Describe;
        Mean = Model.Mean;
        Variance = Model.Variance;
        StandardDeviation = Model.StandardDeviation;
        Median = Model.Median;

        if (Model is NormalModel normal)
        {
            EmpiricalRule = $"68.3 % in {normal.Mu - normal.Sigma:G6} to {normal.Mu + normal.Sigma:G6}; " +
                            $"95.4 % in {normal.Mu - 2 * normal.Sigma:G6} to {normal.Mu + 2 * normal.Sigma:G6}; " +
                            $"99.7 % in {normal.Mu - 3 * normal.Sigma:G6} to {normal.Mu + 3 * normal.Sigma:G6}";
        }

        if (!double.IsNaN(Probability))
        {
            if (Probability <= 0 || Probability >= 1)
            {
                Problem = "The probability for the quantile must be between 0 and 1, exclusive.";
                return;
            }

            Quantile = Model.Quantile(Probability);
        }

        if (double.IsNaN(X))
        {
            Problem = "Enter the value of interest, x.";
            return;
        }

        Density = Model.Density(X);
        AtMost = Model.Cdf(X);
        MoreThan = 1 - AtMost;

        if (Model is NormalModel n2)
        {
            ZScore = n2.Z(X);
        }

        if (!double.IsNaN(X2))
        {
            if (X2 < X)
            {
                Problem = "The upper value x₂ must not be below x.";
                return;
            }

            Between = Model.Between(X, X2);
        }
    }

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Model is null)
        {
            return [];
        }

        double from = double.IsNaN(X2) ? Model.PlotRange.Low : X;
        double to = double.IsNaN(X2) ? X : X2;

        return
        [
            DistributionFigures.Density($"{Name} — {Model.Describe}", Model, from, to),
            DistributionFigures.Cumulative($"{Name} — cumulative", Model, X),
        ];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        IContinuousFamily? family = MethodScope.ActiveMethods(this).OfType<IContinuousFamily>().FirstOrDefault();

        if (family is null)
        {
            return string.Empty;
        }

        s.Imports(scipyStats: true);
        s.Comment(family.Name);
        family.Declare(this, s);
        s.Line();
        s.Assign("x", double.IsNaN(X) ? 0 : X);

        if (!double.IsNaN(X2))
        {
            s.Assign("x2", X2);
        }

        s.Assign("q", double.IsNaN(Probability) ? 0.95 : Probability);
        s.Line();
        s.PrintText("Model", ModelDescription);
        s.Print("Density f(x)", "X.pdf(x)", family.RDensity("x"), 4);
        s.Print("P(X ≤ x)", "X.cdf(x)", family.RCdf("x"), 4);
        s.Print("P(X > x)", "X.sf(x)", $"1 - {family.RCdf("x")}", 4);

        if (!double.IsNaN(X2))
        {
            s.Print("P(x ≤ X ≤ x₂)", "X.cdf(x2) - X.cdf(x)", $"{family.RCdf("x2")} - {family.RCdf("x")}", 4);
        }

        if (Family == ContinuousFamily.Normal)
        {
            s.Print("z-score of x", "(x - mu) / sigma", "(x - mu) / sigma", 4);
        }

        s.Print("Quantile x with P(X ≤ x) = q", "X.ppf(q)", family.RQuantile("q"), 4);
        s.Print("Mean", "X.mean()", family.RMean, 4);
        s.Print("Variance", "X.var()", family.RVariance, 4);
        s.Print("Standard deviation", "X.std()", $"sqrt({family.RVariance})", 4);
        s.Print("Median", "X.median()", family.RQuantile("0.5"), 4);

        if (Family == ContinuousFamily.Normal)
        {
            s.Comment("Empirical rule: 68.3 % within μ ± σ, 95.4 % within μ ± 2σ, 99.7 % within μ ± 3σ");
            s.Either(
                "print(f\"Empirical rule: {mu - sigma:.4g} to {mu + sigma:.4g}; {mu - 2*sigma:.4g} to {mu + 2*sigma:.4g}; {mu - 3*sigma:.4g} to {mu + 3*sigma:.4g}\")",
                "cat(\"Empirical rule:\", mu - sigma, \"to\", mu + sigma, \";\", mu - 2*sigma, \"to\", mu + 2*sigma, \";\", mu - 3*sigma, \"to\", mu + 3*sigma, \"\\n\")");
        }

        return s.ToString();
    }
}

/// <summary>A family is a method: parameters, the model, and its Python and R.</summary>
public interface IContinuousFamily : ICalculationMethod
{
    ContinuousModel Build(ContinuousDistribution tool);

    void Declare(ContinuousDistribution tool, ScriptBuilder script);

    string RDensity(string x);

    string RCdf(string x);

    string RQuantile(string q);

    string RMean { get; }

    string RVariance { get; }
}

public abstract class ContinuousFamilyMethod : CalculationMethod<ContinuousDistribution>, IContinuousFamily
{
    public abstract ContinuousFamily Selects { get; }

    public override bool IsApplicableTo(ContinuousDistribution subject) => subject is not null && subject.Family == Selects;

    public override string Citation =>
        "EGN3443 Module 5 (Continuous Probability Distributions); Montgomery, D. C. and Runger, G. C., " +
        "Applied Statistics and Probability for Engineers, 7th ed., 2018, Chapter 4.";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(ContinuousDistribution.ModelDescription),
        nameof(ContinuousDistribution.Density),
        nameof(ContinuousDistribution.AtMost),
        nameof(ContinuousDistribution.MoreThan),
        nameof(ContinuousDistribution.Between),
        nameof(ContinuousDistribution.Quantile),
        nameof(ContinuousDistribution.Mean),
        nameof(ContinuousDistribution.Variance),
        nameof(ContinuousDistribution.StandardDeviation),
        nameof(ContinuousDistribution.Median),
    ];

    protected static IReadOnlyList<string> Shared =>
        [nameof(ContinuousDistribution.X), nameof(ContinuousDistribution.X2), nameof(ContinuousDistribution.Probability)];

    public override void Apply(ContinuousDistribution subject) => subject.Recalculate();

    public abstract ContinuousModel Build(ContinuousDistribution tool);

    public abstract void Declare(ContinuousDistribution tool, ScriptBuilder script);

    public abstract string RDensity(string x);

    public abstract string RCdf(string x);

    public abstract string RQuantile(string q);

    public abstract string RMean { get; }

    public abstract string RVariance { get; }

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
}

public sealed class UniformFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.Uniform;

    public override string Name => "Uniform distribution";

    public override string Description => "f(x) = 1/(b − a) on [a, b]; μ = (a + b)/2, σ² = (b − a)²/12.";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.Lower), nameof(ContinuousDistribution.Upper), .. Shared];

    public override ContinuousModel Build(ContinuousDistribution tool)
    {
        double a = Given(tool.Lower, "the lower limit"), b = Given(tool.Upper, "the upper limit");

        if (b <= a)
        {
            throw new ArgumentOutOfRangeException(nameof(tool.Upper), "The upper limit must be above the lower limit.");
        }

        return new UniformModel(a, b);
    }

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("a", tool.Lower).Assign("b", tool.Upper);
        s.Either("X = stats.uniform(a, b - a)   # scipy takes (start, width)", "# dunif / punif / qunif take (x, a, b)");
    }

    public override string RDensity(string x) => $"dunif({x}, a, b)";

    public override string RCdf(string x) => $"punif({x}, a, b)";

    public override string RQuantile(string q) => $"qunif({q}, a, b)";

    public override string RMean => "(a + b) / 2";

    public override string RVariance => "(b - a)^2 / 12";
}

public sealed class NormalFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.Normal;

    public override string Name => "Normal distribution";

    public override string Description => "f(x) = e^(−(x−μ)²/2σ²) / (σ√2π); z = (x − μ)/σ; P(X ≤ x) = Φ(z).";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.Mu), nameof(ContinuousDistribution.Sigma), .. Shared];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(ContinuousDistribution.ZScore), nameof(ContinuousDistribution.EmpiricalRule)];

    public override ContinuousModel Build(ContinuousDistribution tool) =>
        new NormalModel(Given(tool.Mu, "the mean"), Positive(tool.Sigma, "the standard deviation"));

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("mu", tool.Mu).Assign("sigma", tool.Sigma);
        s.Either("X = stats.norm(mu, sigma)", "# dnorm / pnorm / qnorm take (x, mean, sd)");
    }

    public override string RDensity(string x) => $"dnorm({x}, mu, sigma)";

    public override string RCdf(string x) => $"pnorm({x}, mu, sigma)";

    public override string RQuantile(string q) => $"qnorm({q}, mu, sigma)";

    public override string RMean => "mu";

    public override string RVariance => "sigma^2";
}

public sealed class ExponentialFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.Exponential;

    public override string Name => "Exponential distribution";

    public override string Description => "f(x) = λe^(−λx), x ≥ 0; P(X ≤ x) = 1 − e^(−λx); μ = 1/λ, σ² = 1/λ². Memoryless.";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.Rate), .. Shared];

    public override ContinuousModel Build(ContinuousDistribution tool) => new ExponentialModel(Positive(tool.Rate, "the rate λ"));

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("lam", tool.Rate);
        s.Either("X = stats.expon(scale=1 / lam)   # scipy takes the scale 1/λ", "# dexp / pexp / qexp take (x, rate)");
    }

    public override string RDensity(string x) => $"dexp({x}, lam)";

    public override string RCdf(string x) => $"pexp({x}, lam)";

    public override string RQuantile(string q) => $"qexp({q}, lam)";

    public override string RMean => "1 / lam";

    public override string RVariance => "1 / lam^2";
}

public sealed class LogNormalFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.LogNormal;

    public override string Name => "Lognormal distribution";

    public override string Description => "ln X ~ N(μ, σ²); E[X] = e^(μ+σ²/2), V[X] = (e^σ² − 1)·e^(2μ+σ²).";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.LogMu), nameof(ContinuousDistribution.LogSigma), .. Shared];

    public override ContinuousModel Build(ContinuousDistribution tool) =>
        new LogNormalModel(Given(tool.LogMu, "the mean of ln X"), Positive(tool.LogSigma, "the standard deviation of ln X"));

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("mu", tool.LogMu).Assign("sigma", tool.LogSigma);
        s.Either("X = stats.lognorm(s=sigma, scale=np.exp(mu))   # scipy's shape is σ, scale e^μ", "# dlnorm / plnorm / qlnorm take (x, meanlog, sdlog)");
    }

    public override string RDensity(string x) => $"dlnorm({x}, mu, sigma)";

    public override string RCdf(string x) => $"plnorm({x}, mu, sigma)";

    public override string RQuantile(string q) => $"qlnorm({q}, mu, sigma)";

    public override string RMean => "exp(mu + sigma^2 / 2)";

    public override string RVariance => "(exp(sigma^2) - 1) * exp(2 * mu + sigma^2)";
}

public sealed class StudentTFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.StudentT;

    public override string Name => "Student's t distribution";

    public override string Description => "The distribution of (x̄ − μ)/(s/√n) for a normal sample of n = ν + 1; heavier tails than the normal, converging to it.";

    public override string Citation => "Montgomery and Runger, 7th ed., §8-2.1 (t distribution); Student (W. S. Gosset), Biometrika 6, 1908.";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.DegreesOfFreedom), .. Shared];

    public override ContinuousModel Build(ContinuousDistribution tool) => new StudentTModel(Positive(tool.DegreesOfFreedom, "the degrees of freedom"));

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("df", tool.DegreesOfFreedom);
        s.Either("X = stats.t(df)", "# dt / pt / qt take (x, df)");
    }

    public override string RDensity(string x) => $"dt({x}, df)";

    public override string RCdf(string x) => $"pt({x}, df)";

    public override string RQuantile(string q) => $"qt({q}, df)";

    public override string RMean => "if (df > 1) 0 else NaN";

    public override string RVariance => "if (df > 2) df / (df - 2) else NaN";
}

public sealed class ChiSquaredFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.ChiSquared;

    public override string Name => "Chi-squared distribution";

    public override string Description => "The distribution of (n − 1)s²/σ² for a normal sample; μ = ν, σ² = 2ν; right-skewed.";

    public override string Citation => "Montgomery and Runger, 7th ed., §8-3 (χ² distribution).";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.DegreesOfFreedom), .. Shared];

    public override ContinuousModel Build(ContinuousDistribution tool) => new ChiSquaredModel(Positive(tool.DegreesOfFreedom, "the degrees of freedom"));

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("df", tool.DegreesOfFreedom);
        s.Either("X = stats.chi2(df)", "# dchisq / pchisq / qchisq take (x, df)");
    }

    public override string RDensity(string x) => $"dchisq({x}, df)";

    public override string RCdf(string x) => $"pchisq({x}, df)";

    public override string RQuantile(string q) => $"qchisq({q}, df)";

    public override string RMean => "df";

    public override string RVariance => "2 * df";
}

public sealed class FFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.F;

    public override string Name => "F distribution";

    public override string Description => "The ratio of two independent sample variances (each over its σ²); the statistic of ANOVA and the two-variance test.";

    public override string Citation => "Montgomery and Runger, 7th ed., §10-5.1 (F distribution).";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.NumeratorDf), nameof(ContinuousDistribution.DenominatorDf), .. Shared];

    public override ContinuousModel Build(ContinuousDistribution tool) =>
        new FModel(Positive(tool.NumeratorDf, "the numerator degrees of freedom"), Positive(tool.DenominatorDf, "the denominator degrees of freedom"));

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        s.Assign("df1", tool.NumeratorDf).Assign("df2", tool.DenominatorDf);
        s.Either("X = stats.f(df1, df2)", "# df / pf / qf take (x, df1, df2)");
    }

    public override string RDensity(string x) => $"df({x}, df1, df2)";

    public override string RCdf(string x) => $"pf({x}, df1, df2)";

    public override string RQuantile(string q) => $"qf({q}, df1, df2)";

    public override string RMean => "if (df2 > 2) df2 / (df2 - 2) else NaN";

    public override string RVariance => "if (df2 > 4) 2 * df2^2 * (df1 + df2 - 2) / (df1 * (df2 - 2)^2 * (df2 - 4)) else NaN";
}
