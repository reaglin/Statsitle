using System.ComponentModel;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
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

    [Description("Weibull — life and strength, shape β and scale η")]
    Weibull,
}

/// <summary>Where a continuous distribution's parameters come from: typed, or fitted to a data column.</summary>
public enum ParameterSource
{
    [Description("Typed here")]
    Entered = 0,

    [Description("Fitted to a data column (maximum likelihood)")]
    FittedToData,
}

/// <summary>
/// EGN3443 Module 5: a continuous random variable — density, the probability below, above and
/// between values, the quantile for a probability, z-scores and the empirical rule for the
/// normal — for the uniform, normal, exponential, lognormal and Weibull, and the t, χ² and F
/// distributions the later modules lean on. The parameters can be typed or fitted to a data
/// column by maximum likelihood, with a histogram and a Q–Q plot to judge the fit (plan 10.9).
/// </summary>
[StatistleHelp(
    "A continuous probability distribution (Module 5). Choose the family, enter its parameters, " +
    "a value x (and an upper value x₂ for a range) and a probability q, and read the density " +
    "f(x), P(X ≤ x), P(X > x), P(x ≤ X ≤ x₂), the quantile with P(X ≤ x) = q, and the mean, " +
    "variance, standard deviation and median. A single point has probability zero for a " +
    "continuous variable; only intervals do, as areas under the density. The normal adds the " +
    "z-score and the 68–95–99.7 rule. The uniform is 'any value in the range equally likely', " +
    "the exponential the waiting time between Poisson events (memoryless), the lognormal a " +
    "variable whose logarithm is normal (fatigue life, particle size), the Weibull the life of a " +
    "part or the strength of a material (reliability R(t) = P(X > t), and the hazard rate h(x)). " +
    "Instead of typing the parameters you can fit them to a column of a data table: choose " +
    "'Fitted to a data column', the column, and read the maximum-likelihood estimates, the Q–Q " +
    "correlation and the Kolmogorov–Smirnov distance D; Plots adds the histogram with the fitted " +
    "curve and the Q–Q plot. Student's t, χ² and F " +
    "are the sampling distributions the confidence intervals and tests of Modules 7 to 14 use " +
    "— enter q = 0.975 to read the two-sided 5 % critical value. The Plots tab shades the " +
    "probability asked for under the density; the Code tab does the same in scipy.stats and R.")]
public sealed class ContinuousDistribution : Analysis, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, IHasFigures, ICodeSource
{
    private ContinuousFamily _family = ContinuousFamily.Normal;
    private ParameterSource _source = ParameterSource.Entered;
    private string _column = string.Empty;
    private double _lower = 0;
    private double _upper = 10;
    private double _mu = 50;
    private double _sigma = 5;
    private double _rate = 0.5;
    private double _logMu = 3;
    private double _logSigma = 0.4;
    private double _shape = 2;
    private double _scale = 100;
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
    private double _hazardRate = double.NaN;
    private double _fitCount = double.NaN;
    private double _fitLower = double.NaN;
    private double _fitUpper = double.NaN;
    private double _fitMu = double.NaN;
    private double _fitSigma = double.NaN;
    private double _fitRate = double.NaN;
    private double _fitLogMu = double.NaN;
    private double _fitLogSigma = double.NaN;
    private double _fitShape = double.NaN;
    private double _fitScale = double.NaN;
    private double _qqCorrelation = double.NaN;
    private double _ksStatistic = double.NaN;

    public ContinuousDistribution()
    {
        Name = "Continuous distribution";
        Recalculate();
    }

    public override string Title => "Continuous distribution";

    public override string Summary => "Uniform, normal, exponential, lognormal, Weibull, t, χ² or F: probabilities, quantiles and a fit to data (Module 5)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 22;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public ContinuousModel? Model { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Distribution", Role = VariableRole.Design,
        Help = "The family. Measurements that pile up around a center with symmetric scatter are " +
               "normal; times between random events are exponential; a quantity that is a " +
               "product of many small factors is lognormal; anything equally likely across a " +
               "range is uniform; the life of a part or the strength of a material is often " +
               "Weibull. t, χ² and F describe statistics computed from samples, not " +
               "measurements. Only the chosen family's parameters are shown.")]
    public ContinuousFamily Family
    {
        get => _family;
        set => SetProperty(ref _family, value);
    }

    [StatistleProperty(Label = "Parameters from", Role = VariableRole.Design,
        Help = "Typed here when the problem states them ('normal with μ = 50 and σ = 5'). Fitted " +
               "to a data column when you have measurements and want the member of the family " +
               "that matches them best: the parameters are estimated by maximum likelihood and " +
               "shown locked, and the Plots tab adds a histogram with the fitted curve and a Q–Q " +
               "plot, so you can see whether the family suits the data. Fitting is offered for " +
               "the uniform, normal, exponential, lognormal and Weibull.")]
    public ParameterSource Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    [StatistleProperty(Label = "Column", Role = VariableRole.Input,
        Help = "The data table column holding the measurements to fit (lifetimes, strengths, " +
               "times between events). The lognormal and Weibull need every value above zero; " +
               "the exponential none below zero.")]
    public string Column
    {
        get => _column;
        set => SetProperty(ref _column, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Values to fit", Role = VariableRole.Input, ValuesOnly = true, Hidden = true,
        Help = "The measurements, copied from the chosen column.")]
    public DataSeries Sample { get; } = new();

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
        Help = "The center of the normal distribution — its mean, median and mode all at once.")]
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

    [StatistleProperty(Label = "Shape (β)", Decimals = 4, Role = VariableRole.Design, Min = 0,
        Help = "The Weibull's shape, β (k in some texts; R's shape, scipy's c). It says how the " +
               "failure rate changes with age: β < 1 falling (early failures, infant mortality), " +
               "β = 1 constant (the exponential), β > 1 rising (wear-out); near β = 3.6 the curve " +
               "looks almost normal. Fatigue and bearing lives are often 1.1 to 3.")]
    public double Shape
    {
        get => _shape;
        set => SetProperty(ref _shape, value);
    }

    [StatistleProperty(Label = "Scale (η)", Decimals = 4, Role = VariableRole.Design, Min = 0,
        Help = "The Weibull's scale, η (λ in some texts; R's and scipy's scale), in the units of " +
               "X: the characteristic life, by which 63.2 % of parts have failed whatever β is, " +
               "since P(X ≤ η) = 1 − e⁻¹.")]
    public double Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
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

    [StatistleProperty(Label = "Density f(x)", Decimals = 6, Role = VariableRole.Output,
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

    [StatistleProperty(Label = "Hazard rate h(x)", Decimals = 6, Role = VariableRole.Output,
        Help = "For a lifetime: the rate of failure at age x among the parts still working, " +
               "h(x) = f(x)/P(X > x). The Weibull's is (β/η)(x/η)^(β−1) — falling for β < 1, " +
               "constant for β = 1, rising for β > 1; the exponential's is λ at every age (it " +
               "does not wear out). P(X > x) above is the reliability R(x), the chance a part " +
               "survives past x. Python X.pdf(x) / X.sf(x); R dweibull(x, shape, scale) / " +
               "(1 − pweibull(x, shape, scale)).")]
    public double HazardRate
    {
        get => _hazardRate;
        set => SetProperty(ref _hazardRate, value);
    }

    [StatistleProperty(Label = "Values fitted (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "How many values of the column the fit used. A fit to fewer than about 20 values " +
               "cannot tell similar families apart; the Q–Q plot says more as n grows.")]
    public double FitCount
    {
        get => _fitCount;
        set => SetProperty(ref _fitCount, value);
    }

    [StatistleProperty(Label = "Fitted a (minimum)", Decimals = 4, Role = VariableRole.Output,
        Help = "The uniform's maximum-likelihood lower limit: the smallest value in the data. " +
               "Python stats.uniform.fit(data) (loc); R min(data).")]
    public double FitLower
    {
        get => _fitLower;
        set => SetProperty(ref _fitLower, value);
    }

    [StatistleProperty(Label = "Fitted b (maximum)", Decimals = 4, Role = VariableRole.Output,
        Help = "The uniform's maximum-likelihood upper limit: the largest value in the data. " +
               "Python loc + scale of stats.uniform.fit(data); R max(data).")]
    public double FitUpper
    {
        get => _fitUpper;
        set => SetProperty(ref _fitUpper, value);
    }

    [StatistleProperty(Label = "Fitted μ", Decimals = 4, Role = VariableRole.Output,
        Help = "The normal's maximum-likelihood mean: the sample mean x̄. Python " +
               "stats.norm.fit(data); R mean(data).")]
    public double FitMu
    {
        get => _fitMu;
        set => SetProperty(ref _fitMu, value);
    }

    [StatistleProperty(Label = "Fitted σ (divides by n)", Decimals = 4, Role = VariableRole.Output,
        Help = "The normal's maximum-likelihood standard deviation, √(Σ(x − x̄)²/n). It divides by " +
               "n, not n − 1, so it is a little smaller than the sample s of Descriptive " +
               "statistics: s = σ̂·√(n/(n − 1)). scipy's stats.norm.fit(data) gives this σ̂; R " +
               "sqrt(mean((data − mean(data))^2)).")]
    public double FitSigma
    {
        get => _fitSigma;
        set => SetProperty(ref _fitSigma, value);
    }

    [StatistleProperty(Label = "Fitted λ = 1/x̄", Decimals = 6, Role = VariableRole.Output,
        Help = "The exponential's maximum-likelihood rate: one over the mean waiting time. " +
               "Python 1 / stats.expon.fit(data, floc=0)[1] (scipy fits the scale x̄); R 1 / mean(data).")]
    public double FitRate
    {
        get => _fitRate;
        set => SetProperty(ref _fitRate, value);
    }

    [StatistleProperty(Label = "Fitted μ of ln X", Decimals = 4, Role = VariableRole.Output,
        Help = "The lognormal's maximum-likelihood meanlog: the mean of ln x. Python " +
               "np.log(scale) from stats.lognorm.fit(data, floc=0); R mean(log(data)).")]
    public double FitLogMu
    {
        get => _fitLogMu;
        set => SetProperty(ref _fitLogMu, value);
    }

    [StatistleProperty(Label = "Fitted σ of ln X", Decimals = 4, Role = VariableRole.Output,
        Help = "The lognormal's maximum-likelihood sdlog: the standard deviation of ln x, " +
               "dividing by n. Python the shape s of stats.lognorm.fit(data, floc=0); R " +
               "sqrt(mean((log(data) − mean(log(data)))^2)).")]
    public double FitLogSigma
    {
        get => _fitLogSigma;
        set => SetProperty(ref _fitLogSigma, value);
    }

    [StatistleProperty(Label = "Fitted shape β", Decimals = 4, Role = VariableRole.Output,
        Help = "The Weibull's maximum-likelihood shape. It has no formula: it is the root of the " +
               "shape equation Σx^β ln x / Σx^β − 1/β − mean(ln x) = 0, found by Newton's method. " +
               "Python brentq, R uniroot on that equation; stats.weibull_min.fit(data, floc=0) agrees to about six figures " +
               "(MASS::fitdistr(data, 'weibull') comes close, to about three figures).")]
    public double FitShape
    {
        get => _fitShape;
        set => SetProperty(ref _fitShape, value);
    }

    [StatistleProperty(Label = "Fitted scale η", Decimals = 4, Role = VariableRole.Output,
        Help = "The Weibull's maximum-likelihood scale, η̂ = (Σx^β̂ / n)^(1/β̂) once β̂ is known. " +
               "The scale of stats.weibull_min.fit(data, floc=0) in Python.")]
    public double FitScale
    {
        get => _fitScale;
        set => SetProperty(ref _fitScale, value);
    }

    [StatistleProperty(Label = "Q–Q correlation (r)", Decimals = 4, Role = VariableRole.Output,
        Help = "How straight the Q–Q plot is: the correlation between the sorted data and the " +
               "fitted model's quantiles at the plotting positions (i − 0.5)/n, as in the course's " +
               "normal probability plot. Near 1 (0.98 or more for 20 to 50 values) the family " +
               "suits the data; a bend at the ends, and a smaller r, says the tails are wrong. " +
               "Compare families on the same column: the larger r fits better.")]
    public double QqCorrelation
    {
        get => _qqCorrelation;
        set => SetProperty(ref _qqCorrelation, value);
    }

    [StatistleProperty(Label = "Kolmogorov–Smirnov D", Decimals = 4, Role = VariableRole.Output,
        Help = "The largest vertical gap between the data's cumulative proportion and the fitted " +
               "P(X ≤ x). Smaller is a closer fit. No p-value is given: the usual one assumes the " +
               "parameters were known in advance, and fitted ones make D look smaller than chance " +
               "would, so that p-value would be too large (Lilliefors' correction deals with this " +
               "for the normal). Python stats.kstest(data, X.cdf).statistic; R " +
               "ks.test(data, function(v) pweibull(v, shape, scale))$statistic.")]
    public double KsStatistic
    {
        get => _ksStatistic;
        set => SetProperty(ref _ksStatistic, value);
    }

    public override void Recalculate()
    {
        Problem = null;
        Model = null;
        ModelDescription = EmpiricalRule = string.Empty;
        Density = AtMost = MoreThan = Between = ZScore = Quantile = Mean = Variance = StandardDeviation = Median = HazardRate = double.NaN;
        FitCount = FitLower = FitUpper = FitMu = FitSigma = FitRate = FitLogMu = FitLogSigma = FitShape = FitScale = double.NaN;
        QqCorrelation = KsStatistic = double.NaN;

        if (Source == ParameterSource.FittedToData && !FitToColumn())
        {
            return;
        }

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

        if (Source == ParameterSource.FittedToData)
        {
            ModelDescription += $", fitted to {Sample.Count} values";
            QqCorrelation = ContinuousFit.QqCorrelation(Sample.YValues, Model);
            KsStatistic = ContinuousFit.KolmogorovSmirnovD(Sample.YValues, Model);
        }

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

        if (Model is WeibullModel weibull)
        {
            HazardRate = weibull.Hazard(X);
        }
        else if (Model is ExponentialModel exponential)
        {
            HazardRate = X < 0 ? 0 : exponential.Rate;
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

    /// <summary>
    /// Copies the column and writes the chosen family's maximum-likelihood parameters into its
    /// entries (locked while fitting) and the fitted results. False, with <see cref="Problem"/>
    /// set, when there is nothing to fit or the family cannot be fitted.
    /// </summary>
    private bool FitToColumn()
    {
        IContinuousFit? fit = MethodScope.ActiveMethods(this).OfType<IContinuousFit>().FirstOrDefault();

        if (fit is null)
        {
            Problem = "Fitting to a data column is offered for the uniform, normal, exponential, lognormal and Weibull. " +
                      "Choose one of them, or set 'Parameters from' back to 'Typed here'.";
            return false;
        }

        string? problem = DataSources.Pull(this, Column, Sample);

        if (problem is not null || Sample.Count == 0)
        {
            Problem = problem ?? DataSources.ChooseOrAdd(this, "Choose the column holding the measurements to fit.");
            return false;
        }

        try
        {
            fit.Fit(this, Sample.YValues);
        }
        catch (ArgumentException ex)
        {
            Problem = ex.Message;
            return false;
        }

        FitCount = Sample.Count;
        return true;
    }

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName == nameof(Column) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Column) or nameof(Sample) ? DataSources.Table(this, Column) : null;

    /// <summary>
    /// Figure 1 the density with the probability shaded, figure 2 the cumulative curve; with
    /// parameters fitted to a column, figure 3 the histogram with the fitted density and figure 4
    /// the Q–Q plot. New figures go after these, so lessons that name a figure by number stand.
    /// </summary>
    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Model is null)
        {
            return [];
        }

        double from = double.IsNaN(X2) ? Model.PlotRange.Low : X;
        double to = double.IsNaN(X2) ? X : X2;

        List<PlotSpec> figures =
        [
            DistributionFigures.Density($"{Name} — {Model.Describe}", Model, from, to),
            DistributionFigures.Cumulative($"{Name} — cumulative", Model, X),
        ];

        if (Source == ParameterSource.FittedToData && Sample.Count > 0)
        {
            figures.Add(DistributionFigures.FittedHistogram($"{Name} — the data and the fitted curve", Sample.YValues, Model));
            figures.Add(DistributionFigures.QuantileQuantile($"{Name} — Q–Q plot (r = {QqCorrelation:F4})", Sample.YValues, Model));
        }

        return figures;
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
        IContinuousFit? fit = Source == ParameterSource.FittedToData
            ? MethodScope.ActiveMethods(this).OfType<IContinuousFit>().FirstOrDefault()
            : null;

        if (fit is not null)
        {
            s.Comment("The parameters fitted to the data by maximum likelihood");
            s.Vector("data", Sample.YValues);
            s.Assign("n", Sample.Count);
            fit.Declare(this, s);
            family.DeclareModel(s);
            s.Line();
            s.PrintRaw("Values fitted (n)", "n", "n");
            fit.PrintFitted(s);
            s.Comment("Q–Q plot: sorted data against the model's quantiles at (i − 0.5)/n");
            s.Either("p = (np.arange(1, n + 1) - 0.5) / n", "p <- (1:n - 0.5) / n");
            s.Print("Q–Q correlation (r)", "np.corrcoef(X.ppf(p), np.sort(data))[0, 1]", $"cor({family.RQuantile("p")}, sort(data))", 4);
            s.Comment("Kolmogorov–Smirnov D only: its p-value assumes parameters not fitted to these data");
            s.Print("Kolmogorov–Smirnov D", "stats.kstest(data, X.cdf).statistic", $"unname(suppressWarnings(ks.test(data, function(v) {family.RCdf("v")}))$statistic)", 4);
        }
        else
        {
            family.Declare(this, s);
        }

        s.Line();
        s.Assign("x", double.IsNaN(X) ? 0 : X);

        if (!double.IsNaN(X2))
        {
            s.Assign("x2", X2);
        }

        s.Assign("q", double.IsNaN(Probability) ? 0.95 : Probability);
        s.Line();
        s.PrintText("Model", ModelDescription);
        s.Print("Density f(x)", "X.pdf(x)", family.RDensity("x"), 6);
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

        if (!double.IsNaN(HazardRate))
        {
            s.Comment("Hazard rate h(x) = f(x) / R(x), where R(x) = P(X > x) is the reliability");
            s.Print("Hazard rate h(x)", "X.pdf(x) / X.sf(x)", $"{family.RDensity("x")} / (1 - {family.RCdf("x")})", 6);
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

    /// <summary>The parameters as numbers, then the model.</summary>
    void Declare(ContinuousDistribution tool, ScriptBuilder script);

    /// <summary>The model alone, over parameter variables already assigned (by a fit).</summary>
    void DeclareModel(ScriptBuilder script);

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

    public void Declare(ContinuousDistribution tool, ScriptBuilder script)
    {
        DeclareParameters(tool, script);
        DeclareModel(script);
    }

    public abstract void DeclareParameters(ContinuousDistribution tool, ScriptBuilder script);

    public abstract void DeclareModel(ScriptBuilder script);

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

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("a", tool.Lower).Assign("b", tool.Upper);

    public override void DeclareModel(ScriptBuilder s) =>
        s.Either("X = stats.uniform(a, b - a)   # scipy takes (start, width)", "# dunif / punif / qunif take (x, a, b)");

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

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("mu", tool.Mu).Assign("sigma", tool.Sigma);

    public override void DeclareModel(ScriptBuilder s) =>
        s.Either("X = stats.norm(mu, sigma)", "# dnorm / pnorm / qnorm take (x, mean, sd)");

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

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(ContinuousDistribution.HazardRate)];

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("lam", tool.Rate);

    public override void DeclareModel(ScriptBuilder s) =>
        s.Either("X = stats.expon(scale=1 / lam)   # scipy takes the scale 1/λ", "# dexp / pexp / qexp take (x, rate)");

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

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("mu", tool.LogMu).Assign("sigma", tool.LogSigma);

    public override void DeclareModel(ScriptBuilder s) =>
        s.Either("X = stats.lognorm(s=sigma, scale=np.exp(mu))   # scipy's shape is σ, scale e^μ", "# dlnorm / plnorm / qlnorm take (x, meanlog, sdlog)");

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

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("df", tool.DegreesOfFreedom);

    public override void DeclareModel(ScriptBuilder s) => s.Either("X = stats.t(df)", "# dt / pt / qt take (x, df)");

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

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("df", tool.DegreesOfFreedom);

    public override void DeclareModel(ScriptBuilder s) => s.Either("X = stats.chi2(df)", "# dchisq / pchisq / qchisq take (x, df)");

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

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("df1", tool.NumeratorDf).Assign("df2", tool.DenominatorDf);

    public override void DeclareModel(ScriptBuilder s) => s.Either("X = stats.f(df1, df2)", "# df / pf / qf take (x, df1, df2)");

    public override string RDensity(string x) => $"df({x}, df1, df2)";

    public override string RCdf(string x) => $"pf({x}, df1, df2)";

    public override string RQuantile(string q) => $"qf({q}, df1, df2)";

    public override string RMean => "if (df2 > 2) df2 / (df2 - 2) else NaN";

    public override string RVariance => "if (df2 > 4) 2 * df2^2 * (df1 + df2 - 2) / (df1 * (df2 - 2)^2 * (df2 - 4)) else NaN";
}

public sealed class WeibullFamily : ContinuousFamilyMethod
{
    public override ContinuousFamily Selects => ContinuousFamily.Weibull;

    public override string Name => "Weibull distribution";

    public override string Description =>
        "P(X ≤ x) = 1 − e^(−(x/η)^β), x ≥ 0; μ = η·Γ(1 + 1/β), σ² = η²[Γ(1 + 2/β) − Γ(1 + 1/β)²]; " +
        "reliability R(x) = e^(−(x/η)^β), hazard h(x) = (β/η)(x/η)^(β−1).";

    public override string Citation =>
        "Montgomery and Runger, 7th ed., §4-10 (Weibull distribution); EGN3443 Module 5 (reliability).";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.Shape), nameof(ContinuousDistribution.Scale), .. Shared];

    public override IReadOnlyList<string> Outputs => [.. base.Outputs, nameof(ContinuousDistribution.HazardRate)];

    public override ContinuousModel Build(ContinuousDistribution tool) =>
        new WeibullModel(Positive(tool.Shape, "the shape β"), Positive(tool.Scale, "the scale η"));

    public override void DeclareParameters(ContinuousDistribution tool, ScriptBuilder s) => s.Assign("shape", tool.Shape).Assign("scale", tool.Scale);

    // R's (shape, scale) are the course's (β, η); scipy's weibull_min calls the shape c.
    public override void DeclareModel(ScriptBuilder s) =>
        s.Either("X = stats.weibull_min(c=shape, scale=scale)   # scipy's c is the shape β, scale is η", "# dweibull / pweibull / qweibull take (x, shape, scale)");

    public override string RDensity(string x) => $"dweibull({x}, shape, scale)";

    public override string RCdf(string x) => $"pweibull({x}, shape, scale)";

    public override string RQuantile(string q) => $"qweibull({q}, shape, scale)";

    public override string RMean => "scale * gamma(1 + 1 / shape)";

    public override string RVariance => "scale^2 * (gamma(1 + 2 / shape) - gamma(1 + 1 / shape)^2)";
}

/// <summary>A family's maximum-likelihood fit to a data column: the estimates, and their Python and R.</summary>
public interface IContinuousFit : ICalculationMethod
{
    /// <summary>Writes the estimates into the family's entries and the fitted results.</summary>
    /// <exception cref="ArgumentException">The values cannot be fitted (too few, out of the family's range).</exception>
    void Fit(ContinuousDistribution tool, IReadOnlyList<double> values);

    /// <summary>Assigns the family's parameter variables from <c>data</c>.</summary>
    void Declare(ContinuousDistribution tool, ScriptBuilder script);

    /// <summary>Prints the fitted results under their labels.</summary>
    void PrintFitted(ScriptBuilder script);
}

/// <summary>
/// Fitting is a method of its own for each family: it applies while 'Parameters from' is a data
/// column and the family is chosen, takes the column as its input, and generates the family's
/// parameters (so the editor locks them) and the fitted results.
/// </summary>
public abstract class ContinuousFitMethod : CalculationMethod<ContinuousDistribution>, IContinuousFit
{
    public abstract ContinuousFamily Fits { get; }

    /// <summary>The family's entries the fit writes.</summary>
    protected abstract IReadOnlyList<string> Parameters { get; }

    /// <summary>The fitted results this family shows.</summary>
    protected abstract IReadOnlyList<string> Estimates { get; }

    public override bool IsApplicableTo(ContinuousDistribution subject) =>
        subject is not null && subject.Source == ParameterSource.FittedToData && subject.Family == Fits;

    public override string Citation =>
        "Maximum likelihood: Montgomery and Runger, 7th ed., §7-4.2; probability plots §6-6; " +
        "the estimates match scipy.stats (fit, with loc fixed at 0 where the family starts at 0).";

    public override IReadOnlyList<string> Inputs => [nameof(ContinuousDistribution.Column)];

    public override IReadOnlyList<string> Outputs =>
    [
        .. Parameters,
        nameof(ContinuousDistribution.FitCount),
        .. Estimates,
        nameof(ContinuousDistribution.QqCorrelation),
        nameof(ContinuousDistribution.KsStatistic),
    ];

    public override IReadOnlyList<string> Generates => Parameters;

    public override void Apply(ContinuousDistribution subject) => subject.Recalculate();

    public abstract void Fit(ContinuousDistribution tool, IReadOnlyList<double> values);

    public abstract void Declare(ContinuousDistribution tool, ScriptBuilder script);

    public abstract void PrintFitted(ScriptBuilder script);
}

public sealed class UniformFit : ContinuousFitMethod
{
    public override ContinuousFamily Fits => ContinuousFamily.Uniform;

    public override string Name => "Uniform fitted to a data column";

    public override string Description => "â = the smallest value, b̂ = the largest (maximum likelihood).";

    protected override IReadOnlyList<string> Parameters => [nameof(ContinuousDistribution.Lower), nameof(ContinuousDistribution.Upper)];

    protected override IReadOnlyList<string> Estimates => [nameof(ContinuousDistribution.FitLower), nameof(ContinuousDistribution.FitUpper)];

    public override void Fit(ContinuousDistribution tool, IReadOnlyList<double> values)
    {
        (double a, double b) = ContinuousFit.Uniform(values);
        tool.Lower = tool.FitLower = a;
        tool.Upper = tool.FitUpper = b;
    }

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s) =>
        s.Either("a, b = np.min(data), np.max(data)   # the same as loc, loc + scale of stats.uniform.fit(data)", "a <- min(data); b <- max(data)");

    public override void PrintFitted(ScriptBuilder s)
    {
        s.Print("Fitted a (minimum)", "a", "a", 4);
        s.Print("Fitted b (maximum)", "b", "b", 4);
    }
}

public sealed class NormalFit : ContinuousFitMethod
{
    public override ContinuousFamily Fits => ContinuousFamily.Normal;

    public override string Name => "Normal fitted to a data column";

    public override string Description => "μ̂ = x̄, σ̂ = √(Σ(x − x̄)²/n) (maximum likelihood; divides by n, not n − 1).";

    protected override IReadOnlyList<string> Parameters => [nameof(ContinuousDistribution.Mu), nameof(ContinuousDistribution.Sigma)];

    protected override IReadOnlyList<string> Estimates => [nameof(ContinuousDistribution.FitMu), nameof(ContinuousDistribution.FitSigma)];

    public override void Fit(ContinuousDistribution tool, IReadOnlyList<double> values)
    {
        (double mu, double sigma) = ContinuousFit.Normal(values);
        tool.Mu = tool.FitMu = mu;
        tool.Sigma = tool.FitSigma = sigma;
    }

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s) =>
        s.Either("mu, sigma = stats.norm.fit(data)   # maximum likelihood: σ̂ divides by n", "mu <- mean(data); sigma <- sqrt(mean((data - mu)^2))   # maximum likelihood: divides by n");

    public override void PrintFitted(ScriptBuilder s)
    {
        s.Print("Fitted μ", "mu", "mu", 4);
        s.Print("Fitted σ (divides by n)", "sigma", "sigma", 4);
    }
}

public sealed class ExponentialFit : ContinuousFitMethod
{
    public override ContinuousFamily Fits => ContinuousFamily.Exponential;

    public override string Name => "Exponential fitted to a data column";

    public override string Description => "λ̂ = 1/x̄ (maximum likelihood).";

    protected override IReadOnlyList<string> Parameters => [nameof(ContinuousDistribution.Rate)];

    protected override IReadOnlyList<string> Estimates => [nameof(ContinuousDistribution.FitRate)];

    public override void Fit(ContinuousDistribution tool, IReadOnlyList<double> values) =>
        tool.Rate = tool.FitRate = ContinuousFit.Exponential(values);

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s) =>
        s.Either("loc, mean_wait = stats.expon.fit(data, floc=0); lam = 1 / mean_wait   # scipy fits the scale x̄", "lam <- 1 / mean(data)");

    public override void PrintFitted(ScriptBuilder s) => s.Print("Fitted λ = 1/x̄", "lam", "lam", 6);
}

public sealed class LogNormalFit : ContinuousFitMethod
{
    public override ContinuousFamily Fits => ContinuousFamily.LogNormal;

    public override string Name => "Lognormal fitted to a data column";

    public override string Description => "μ̂ = mean of ln x, σ̂ = √(Σ(ln x − μ̂)²/n) (maximum likelihood).";

    protected override IReadOnlyList<string> Parameters => [nameof(ContinuousDistribution.LogMu), nameof(ContinuousDistribution.LogSigma)];

    protected override IReadOnlyList<string> Estimates => [nameof(ContinuousDistribution.FitLogMu), nameof(ContinuousDistribution.FitLogSigma)];

    public override void Fit(ContinuousDistribution tool, IReadOnlyList<double> values)
    {
        (double mu, double sigma) = ContinuousFit.LogNormal(values);
        tool.LogMu = tool.FitLogMu = mu;
        tool.LogSigma = tool.FitLogSigma = sigma;
    }

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s) =>
        s.Either("sigma, loc, scale_ = stats.lognorm.fit(data, floc=0); mu = np.log(scale_)   # scipy's shape is σ, scale e^μ",
                 "mu <- mean(log(data)); sigma <- sqrt(mean((log(data) - mu)^2))   # maximum likelihood: divides by n");

    public override void PrintFitted(ScriptBuilder s)
    {
        s.Print("Fitted μ of ln X", "mu", "mu", 4);
        s.Print("Fitted σ of ln X", "sigma", "sigma", 4);
    }
}

public sealed class WeibullFit : ContinuousFitMethod
{
    public override ContinuousFamily Fits => ContinuousFamily.Weibull;

    public override string Name => "Weibull fitted to a data column";

    public override string Description =>
        "β̂ solves Σx^β ln x / Σx^β − 1/β − mean(ln x) = 0 (Newton's method), η̂ = (Σx^β̂/n)^(1/β̂) (maximum likelihood).";

    public override string Citation =>
        "Maximum likelihood for the Weibull: Lawless, J. F., Statistical Models and Methods for Lifetime Data, 2nd ed., " +
        "2003, §5.2; Montgomery and Runger, 7th ed., §4-10 and §7-4.2; scipy.stats.weibull_min.fit(x, floc=0) agrees to about six figures.";

    protected override IReadOnlyList<string> Parameters => [nameof(ContinuousDistribution.Shape), nameof(ContinuousDistribution.Scale)];

    protected override IReadOnlyList<string> Estimates => [nameof(ContinuousDistribution.FitShape), nameof(ContinuousDistribution.FitScale)];

    public override void Fit(ContinuousDistribution tool, IReadOnlyList<double> values)
    {
        (double shape, double scale) = ContinuousFit.Weibull(values);
        tool.Shape = tool.FitShape = shape;
        tool.Scale = tool.FitScale = scale;
    }

    public override void Declare(ContinuousDistribution tool, ScriptBuilder s)
    {
        if (s.IsPython)
        {
            s.Line("# The maximum-likelihood shape solves the shape equation exactly; stats.weibull_min.fit(data, floc=0)");
            s.Line("# gives the same β and η to about six figures (it uses a general-purpose optimizer)");
            s.Line("from scipy.optimize import brentq");
            s.Line("u = data / np.mean(data)   # dividing by the mean leaves the shape unchanged and avoids overflow");
            s.Line("shape = brentq(lambda k: np.sum(u**k * np.log(u)) / np.sum(u**k) - 1 / k - np.mean(np.log(u)), 0.01, 100, xtol=1e-12)");
            s.Line("scale = np.mean(data) * np.mean(u**shape)**(1 / shape)");
            return;
        }

        s.Line("# Base R has no Weibull fit: solve the maximum-likelihood shape equation (MASS::fitdistr(data, 'weibull') gives nearly the same: its optimizer stops sooner)");
        s.Line("u <- data / mean(data)   # dividing by the mean leaves the shape unchanged and avoids overflow");
        s.Line("shape_equation <- function(k) sum(u^k * log(u)) / sum(u^k) - 1 / k - mean(log(u))");
        s.Line("shape <- uniroot(shape_equation, c(0.01, 100), tol = 1e-12)$root");
        s.Line("scale <- mean(data) * mean(u^shape)^(1 / shape)");
    }

    public override void PrintFitted(ScriptBuilder s)
    {
        s.Print("Fitted shape β", "shape", "shape", 4);
        s.Print("Fitted scale η", "scale", "scale", 4);
    }
}
