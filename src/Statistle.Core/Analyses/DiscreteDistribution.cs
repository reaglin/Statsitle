using System.ComponentModel;
using System.Globalization;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum DiscreteFamily
{
    [Description("Binomial — successes in n trials")]
    Binomial = 0,

    [Description("Poisson — events at a mean rate")]
    Poisson,

    [Description("Geometric — trials to the first success")]
    Geometric,

    [Description("Negative binomial — trials to the r-th success")]
    NegativeBinomial,

    [Description("Hypergeometric — draws without replacement")]
    Hypergeometric,

    [Description("Typed table of (x, P(x))")]
    Table,
}

/// <summary>
/// EGN3443 Module 4: a discrete random variable — its probability mass function, the
/// probabilities of a value, a range and a tail, its mean and variance — for the binomial,
/// Poisson, geometric, negative binomial and hypergeometric families and a typed table.
/// </summary>
[StatistleHelp(
    "A discrete probability distribution (Module 4). Choose the family, enter its parameters " +
    "and a value of interest k (and an upper value k₂ for a range), and read P(X = k), the " +
    "tails P(X ≤ k), P(X < k), P(X ≥ k), P(X > k), the range P(k ≤ X ≤ k₂), and the mean, " +
    "variance and standard deviation. Binomial: a fixed number of independent trials with the " +
    "same success probability — defective boards in a batch of 20. Poisson: counts of events " +
    "in a fixed interval at a steady mean rate — flaws per metre, calls per hour. Geometric and " +
    "negative binomial: how many trials until the first (r-th) success. Hypergeometric: " +
    "draws without replacement from a finite lot — acceptance sampling. Typed table: any " +
    "distribution written out as (x, P(x)). The Plots tab draws the PMF with the probability " +
    "asked for highlighted, and the CDF. The Code tab does the same with scipy.stats and R.")]
public sealed class DiscreteDistribution : Analysis, IReportsProblem, IHasFigures, ICodeSource
{
    private DiscreteFamily _family = DiscreteFamily.Binomial;
    private double _trials = 20;
    private double _successProbability = 0.05;
    private double _rate = 3;
    private double _successesWanted = 3;
    private double _populationSize = 50;
    private double _populationSuccesses = 5;
    private double _draws = 10;
    private double _k = 2;
    private double _k2 = double.NaN;

    private string _model = string.Empty;
    private double _exactly = double.NaN;
    private double _atMost = double.NaN;
    private double _lessThan = double.NaN;
    private double _atLeast = double.NaN;
    private double _moreThan = double.NaN;
    private double _between = double.NaN;
    private double _mean = double.NaN;
    private double _variance = double.NaN;
    private double _standardDeviation = double.NaN;
    private string _support = string.Empty;

    public DiscreteDistribution()
    {
        Name = "Discrete distribution";

        foreach ((double x, double p) in new[] { (0.0, 0.1), (1, 0.2), (2, 0.4), (3, 0.3) })
        {
            Table.Add(x, p);
        }

        Recalculate();
    }

    public override string Title => "Discrete distribution";

    public override string Summary => "Binomial, Poisson, geometric, negative binomial, hypergeometric or typed (Module 4)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 21;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public DiscreteModel? Model { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Distribution", Role = VariableRole.Design,
        Help = "The family. Ask: is there a fixed number of trials (binomial)? a rate over an " +
               "interval (Poisson)? waiting for a success (geometric, negative binomial)? a " +
               "finite lot sampled without replacement (hypergeometric)? Or is the table simply " +
               "given? Only the chosen family's parameters are shown.")]
    public DiscreteFamily Family
    {
        get => _family;
        set => SetProperty(ref _family, value);
    }

    [StatistleProperty(Label = "Trials (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1, Max = 100000,
        Help = "The fixed number of independent trials — boards inspected, items tested.")]
    public double Trials
    {
        get => _trials;
        set => SetProperty(ref _trials, value);
    }

    [StatistleProperty(Label = "Success probability (p)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Design, Min = 0, Max = 1,
        Help = "The probability of a success on any one trial, the same for every trial and " +
               "unaffected by the others. 'Success' is just the outcome being counted — a " +
               "defect can be a success.")]
    public double SuccessProbability
    {
        get => _successProbability;
        set => SetProperty(ref _successProbability, value);
    }

    [StatistleProperty(Label = "Mean rate (λ)", Decimals = 4, Role = VariableRole.Design, Min = 0,
        Help = "The mean number of events in the interval considered — 3 flaws per 100 m, 2.5 " +
               "arrivals per minute. Scale it to the interval of the question first: 3 per " +
               "100 m is 6 per 200 m. Mean and variance are both λ.")]
    public double Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
    }

    [StatistleProperty(Label = "Successes wanted (r)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1, Max = 1000,
        Help = "For the negative binomial: how many successes are being waited for. X counts the " +
               "trials up to and including the r-th, so X ≥ r.")]
    public double SuccessesWanted
    {
        get => _successesWanted;
        set => SetProperty(ref _successesWanted, value);
    }

    [StatistleProperty(Label = "Population size (N)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1, Max = 1000000,
        Help = "The size of the lot being sampled without replacement.")]
    public double PopulationSize
    {
        get => _populationSize;
        set => SetProperty(ref _populationSize, value);
    }

    [StatistleProperty(Label = "Successes in the population (K)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 0, Max = 1000000,
        Help = "How many of the N items are of the kind being counted — the defectives in the lot.")]
    public double PopulationSuccesses
    {
        get => _populationSuccesses;
        set => SetProperty(ref _populationSuccesses, value);
    }

    [StatistleProperty(Label = "Draws (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 1, Max = 1000000,
        Help = "How many items are drawn from the lot. When n is under a tenth of N the binomial " +
               "with p = K/N is a close approximation.")]
    public double Draws
    {
        get => _draws;
        set => SetProperty(ref _draws, value);
    }

    [StatistleProperty(Label = "Table (x, P(x))", ReportLabel = "Typed distribution (x, P(x))", Role = VariableRole.Design,
        Help = "The distribution written out: one line per outcome, the whole-number value x " +
               "then its probability, separated by a tab, comma or semicolon. The probabilities " +
               "must be non-negative and sum to 1 — the two conditions for a valid PMF.")]
    public DataSeries Table { get; } = new();

    [StatistleProperty(Label = "Value of interest (k)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0,
        Help = "The value the question is about: 'exactly 2 defective', 'at most 2', 'at least 2'. " +
               "Every tail probability below is for this k.")]
    public double K
    {
        get => _k;
        set => SetProperty(ref _k, value);
    }

    [StatistleProperty(Label = "Upper value (k₂), optional", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0,
        Help = "For 'between k and k₂ inclusive': the upper end of the range. Leave blank when the " +
               "question has one value.")]
    public double K2
    {
        get => _k2;
        set => SetProperty(ref _k2, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Model", Role = VariableRole.Calculated,
        Help = "The distribution with the parameters in force.")]
    public string ModelDescription
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }

    [StatistleProperty(Label = "P(X = k)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability mass at k — exactly k successes, events or trials. Python " +
               "X.pmf(k); R dbinom(k, n, p) and its cousins.")]
    public double Exactly
    {
        get => _exactly;
        set => SetProperty(ref _exactly, value);
    }

    [StatistleProperty(Label = "P(X ≤ k)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The cumulative probability, 'at most k' — the sum of the mass from the smallest " +
               "value up to k. Python X.cdf(k); R pbinom(k, n, p).")]
    public double AtMost
    {
        get => _atMost;
        set => SetProperty(ref _atMost, value);
    }

    [StatistleProperty(Label = "P(X < k)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "'Fewer than k': P(X ≤ k − 1). With integers 'less than' and 'at most' differ by " +
               "one whole value, the usual slip in these problems.")]
    public double LessThan
    {
        get => _lessThan;
        set => SetProperty(ref _lessThan, value);
    }

    [StatistleProperty(Label = "P(X ≥ k)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "'At least k': 1 − P(X ≤ k − 1). Python X.sf(k − 1); R 1 − pbinom(k − 1, n, p).")]
    public double AtLeast
    {
        get => _atLeast;
        set => SetProperty(ref _atLeast, value);
    }

    [StatistleProperty(Label = "P(X > k)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "'More than k': 1 − P(X ≤ k). Python X.sf(k); R 1 − pbinom(k, n, p).")]
    public double MoreThan
    {
        get => _moreThan;
        set => SetProperty(ref _moreThan, value);
    }

    [StatistleProperty(Label = "P(k ≤ X ≤ k₂)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of the inclusive range: P(X ≤ k₂) − P(X ≤ k − 1). Blank without a k₂.")]
    public double Between
    {
        get => _between;
        set => SetProperty(ref _between, value);
    }

    [StatistleProperty(Label = "Mean (μ)", Decimals = 4, Role = VariableRole.Output,
        Help = "The expected value E[X] = Σ x·P(x): np for the binomial, λ for the Poisson, 1/p " +
               "for the geometric, r/p for the negative binomial, nK/N for the hypergeometric.")]
    public double Mean
    {
        get => _mean;
        set => SetProperty(ref _mean, value);
    }

    [StatistleProperty(Label = "Variance (σ²)", Decimals = 4, Role = VariableRole.Output,
        Help = "V[X] = Σ (x − μ)²·P(x): np(1 − p) for the binomial, λ for the Poisson.")]
    public double Variance
    {
        get => _variance;
        set => SetProperty(ref _variance, value);
    }

    [StatistleProperty(Label = "Standard deviation (σ)", Decimals = 4, Role = VariableRole.Output,
        Help = "√V[X]: the spread of X in its own units — how far a typical outcome sits from the mean.")]
    public double StandardDeviation
    {
        get => _standardDeviation;
        set => SetProperty(ref _standardDeviation, value);
    }

    [StatistleProperty(Label = "Possible values", Role = VariableRole.Calculated,
        Help = "The range of X with any probability (an unbounded family is cut where 99.95 % of " +
               "the mass has been reached).")]
    public string Support
    {
        get => _support;
        set => SetProperty(ref _support, value);
    }

    public override void Recalculate()
    {
        Problem = null;
        Model = null;
        ModelDescription = Support = string.Empty;
        Exactly = AtMost = LessThan = AtLeast = MoreThan = Between = Mean = Variance = StandardDeviation = double.NaN;

        try
        {
            Model = MethodScope.ActiveMethods(this).OfType<IDiscreteFamily>().FirstOrDefault()?.Build(this);
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
        Support = Model.SupportMax >= 100_000 ? $"{Model.SupportMin} and up" : $"{Model.SupportMin} to {Model.SupportMax}";

        if (double.IsNaN(K))
        {
            Problem = "Enter the value of interest, k.";
            return;
        }

        int k = (int)Math.Round(K);
        Exactly = Model.Pmf(k);
        AtMost = Model.Cdf(k);
        LessThan = Model.Cdf(k - 1);
        AtLeast = 1 - LessThan;
        MoreThan = 1 - AtMost;

        if (!double.IsNaN(K2))
        {
            int k2 = (int)Math.Round(K2);

            if (k2 < k)
            {
                Problem = "The upper value k₂ must not be below k.";
                return;
            }

            Between = Model.Between(k, k2);
        }
    }

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Model is null || double.IsNaN(K))
        {
            return [];
        }

        int k = (int)Math.Round(K);
        int to = double.IsNaN(K2) ? k : (int)Math.Round(K2);

        return
        [
            DistributionFigures.Pmf($"{Name} — {Model.Describe}", Model, k, to),
            DistributionFigures.Cdf($"{Name} — cumulative", Model, k),
        ];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        IDiscreteFamily? family = MethodScope.ActiveMethods(this).OfType<IDiscreteFamily>().FirstOrDefault();

        if (family is null)
        {
            return string.Empty;
        }

        s.Imports(scipyStats: true);
        s.Comment(family.Name);
        family.Declare(this, s);
        s.Line();
        s.Assign("k", (int)Math.Round(double.IsNaN(K) ? 0 : K));

        if (!double.IsNaN(K2))
        {
            s.Assign("k2", (int)Math.Round(K2));
        }

        s.Line();
        s.PrintText("Model", ModelDescription);
        s.Print("P(X = k)", family.PythonPmf("k"), family.RPmf("k"), 4);
        s.Print("P(X ≤ k)", family.PythonCdf("k"), family.RCdf("k"), 4);
        s.Print("P(X < k)", family.PythonCdf("k - 1"), family.RCdf("k - 1"), 4);
        s.Print("P(X ≥ k)", $"1 - {family.PythonCdf("k - 1")}", $"1 - {family.RCdf("k - 1")}", 4);
        s.Print("P(X > k)", $"1 - {family.PythonCdf("k")}", $"1 - {family.RCdf("k")}", 4);

        if (!double.IsNaN(K2))
        {
            s.Print("P(k ≤ X ≤ k₂)", $"{family.PythonCdf("k2")} - {family.PythonCdf("k - 1")}", $"{family.RCdf("k2")} - {family.RCdf("k - 1")}", 4);
        }

        s.Print("Mean (μ)", family.PythonMean, family.RMean, 4);
        s.Print("Variance (σ²)", family.PythonVariance, family.RVariance, 4);
        s.Print("Standard deviation (σ)", $"np.sqrt({family.PythonVariance})", $"sqrt({family.RVariance})", 4);
        s.PrintText("Possible values", Support);

        return s.ToString();
    }
}

/// <summary>A family is a method: it declares its parameters, builds the model, and knows its Python and R.</summary>
public interface IDiscreteFamily : ICalculationMethod
{
    DiscreteModel Build(DiscreteDistribution tool);

    /// <summary>Writes the parameter assignments and, in Python, the frozen distribution <c>X</c>.</summary>
    void Declare(DiscreteDistribution tool, ScriptBuilder script);

    string PythonPmf(string k);

    string PythonCdf(string k);

    string PythonMean { get; }

    string PythonVariance { get; }

    string RPmf(string k);

    string RCdf(string k);

    string RMean { get; }

    string RVariance { get; }
}

public abstract class DiscreteFamilyMethod : CalculationMethod<DiscreteDistribution>, IDiscreteFamily
{
    public abstract DiscreteFamily Selects { get; }

    public override bool IsApplicableTo(DiscreteDistribution subject) => subject is not null && subject.Family == Selects;

    public override string Citation =>
        "EGN3443 Module 4 (Discrete Probability Distributions); Montgomery, D. C. and Runger, G. C., " +
        "Applied Statistics and Probability for Engineers, 7th ed., 2018, Chapter 3.";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(DiscreteDistribution.ModelDescription),
        nameof(DiscreteDistribution.Exactly),
        nameof(DiscreteDistribution.AtMost),
        nameof(DiscreteDistribution.LessThan),
        nameof(DiscreteDistribution.AtLeast),
        nameof(DiscreteDistribution.MoreThan),
        nameof(DiscreteDistribution.Between),
        nameof(DiscreteDistribution.Mean),
        nameof(DiscreteDistribution.Variance),
        nameof(DiscreteDistribution.StandardDeviation),
        nameof(DiscreteDistribution.Support),
    ];

    protected static IReadOnlyList<string> Shared => [nameof(DiscreteDistribution.K), nameof(DiscreteDistribution.K2)];

    public override void Apply(DiscreteDistribution subject) => subject.Recalculate();

    public abstract DiscreteModel Build(DiscreteDistribution tool);

    public abstract void Declare(DiscreteDistribution tool, ScriptBuilder script);

    public virtual string PythonPmf(string k) => $"X.pmf({k})";

    public virtual string PythonCdf(string k) => $"X.cdf({k})";

    public virtual string PythonMean => "X.mean()";

    public virtual string PythonVariance => "X.var()";

    public abstract string RPmf(string k);

    public abstract string RCdf(string k);

    public abstract string RMean { get; }

    public abstract string RVariance { get; }

    protected static int Whole(double value, string what)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentOutOfRangeException(what, $"Enter {what}.");
        }

        return (int)Math.Round(value);
    }

    protected static double Fraction(double value, string what)
    {
        if (double.IsNaN(value) || value < 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(what, $"Enter {what} between 0 and 1.");
        }

        return value;
    }
}

public sealed class BinomialFamily : DiscreteFamilyMethod
{
    public override DiscreteFamily Selects => DiscreteFamily.Binomial;

    public override string Name => "Binomial distribution";

    public override string Description => "P(X = x) = C(n, x)·pˣ·(1 − p)ⁿ⁻ˣ, x = 0 … n; μ = np, σ² = np(1 − p).";

    public override IReadOnlyList<string> Inputs => [nameof(DiscreteDistribution.Trials), nameof(DiscreteDistribution.SuccessProbability), .. Shared];

    public override DiscreteModel Build(DiscreteDistribution tool) =>
        new BinomialModel(Math.Max(1, Whole(tool.Trials, "the number of trials")), Fraction(tool.SuccessProbability, "the success probability"));

    public override void Declare(DiscreteDistribution tool, ScriptBuilder s)
    {
        s.Assign("n", Whole(tool.Trials, "n")).Assign("p", tool.SuccessProbability);
        s.Either("X = stats.binom(n, p)", "# dbinom / pbinom take (k, n, p)");
    }

    public override string RPmf(string k) => $"dbinom({k}, n, p)";

    public override string RCdf(string k) => $"pbinom({k}, n, p)";

    public override string RMean => "n * p";

    public override string RVariance => "n * p * (1 - p)";
}

public sealed class PoissonFamily : DiscreteFamilyMethod
{
    public override DiscreteFamily Selects => DiscreteFamily.Poisson;

    public override string Name => "Poisson distribution";

    public override string Description => "P(X = x) = e^(−λ)·λˣ / x!, x = 0, 1, 2, …; μ = σ² = λ.";

    public override IReadOnlyList<string> Inputs => [nameof(DiscreteDistribution.Rate), .. Shared];

    public override DiscreteModel Build(DiscreteDistribution tool)
    {
        if (double.IsNaN(tool.Rate) || tool.Rate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tool.Rate), "Enter a mean rate λ greater than zero.");
        }

        return new PoissonModel(tool.Rate);
    }

    public override void Declare(DiscreteDistribution tool, ScriptBuilder s)
    {
        s.Assign("lam", tool.Rate);
        s.Either("X = stats.poisson(lam)", "# dpois / ppois take (k, lambda)");
    }

    public override string RPmf(string k) => $"dpois({k}, lam)";

    public override string RCdf(string k) => $"ppois({k}, lam)";

    public override string RMean => "lam";

    public override string RVariance => "lam";
}

public sealed class GeometricFamily : DiscreteFamilyMethod
{
    public override DiscreteFamily Selects => DiscreteFamily.Geometric;

    public override string Name => "Geometric distribution";

    public override string Description => "P(X = x) = (1 − p)^(x−1)·p, x = 1, 2, …; μ = 1/p, σ² = (1 − p)/p². Memoryless.";

    public override IReadOnlyList<string> Inputs => [nameof(DiscreteDistribution.SuccessProbability), .. Shared];

    public override DiscreteModel Build(DiscreteDistribution tool)
    {
        double p = Fraction(tool.SuccessProbability, "the success probability");

        if (p <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tool.SuccessProbability), "The success probability must be greater than zero, or the first success never comes.");
        }

        return new GeometricModel(p);
    }

    public override void Declare(DiscreteDistribution tool, ScriptBuilder s)
    {
        s.Assign("p", tool.SuccessProbability);
        s.Either("X = stats.geom(p)   # counts trials to the first success, as the course does", "# R's dgeom counts failures before the first success: use k - 1");
    }

    public override string RPmf(string k) => $"dgeom({k} - 1, p)";

    public override string RCdf(string k) => $"pgeom({k} - 1, p)";

    public override string RMean => "1 / p";

    public override string RVariance => "(1 - p) / p^2";
}

public sealed class NegativeBinomialFamily : DiscreteFamilyMethod
{
    public override DiscreteFamily Selects => DiscreteFamily.NegativeBinomial;

    public override string Name => "Negative binomial distribution";

    public override string Description => "P(X = x) = C(x − 1, r − 1)·p^r·(1 − p)^(x−r), x = r, r + 1, …; μ = r/p, σ² = r(1 − p)/p².";

    public override IReadOnlyList<string> Inputs => [nameof(DiscreteDistribution.SuccessesWanted), nameof(DiscreteDistribution.SuccessProbability), .. Shared];

    public override DiscreteModel Build(DiscreteDistribution tool)
    {
        double p = Fraction(tool.SuccessProbability, "the success probability");
        int r = Whole(tool.SuccessesWanted, "the successes wanted");

        if (p <= 0 || r < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(tool.SuccessesWanted), "Enter r ≥ 1 and a success probability greater than zero.");
        }

        return new NegativeBinomialModel(r, p);
    }

    public override void Declare(DiscreteDistribution tool, ScriptBuilder s)
    {
        s.Assign("r", Whole(tool.SuccessesWanted, "r")).Assign("p", tool.SuccessProbability);
        s.Either("X = stats.nbinom(r, p)   # scipy counts failures; k trials is k - r failures", "# R's dnbinom counts failures before the r-th success: use k - r");
    }

    public override string PythonPmf(string k) => $"X.pmf({k} - r)";

    public override string PythonCdf(string k) => $"X.cdf({k} - r)";

    public override string PythonMean => "X.mean() + r";

    public override string RPmf(string k) => $"dnbinom({k} - r, r, p)";

    public override string RCdf(string k) => $"pnbinom({k} - r, r, p)";

    public override string RMean => "r / p";

    public override string RVariance => "r * (1 - p) / p^2";
}

public sealed class HypergeometricFamily : DiscreteFamilyMethod
{
    public override DiscreteFamily Selects => DiscreteFamily.Hypergeometric;

    public override string Name => "Hypergeometric distribution";

    public override string Description => "P(X = x) = C(K, x)·C(N − K, n − x) / C(N, n); μ = nK/N, σ² = nK/N·(1 − K/N)·(N − n)/(N − 1).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(DiscreteDistribution.PopulationSize), nameof(DiscreteDistribution.PopulationSuccesses), nameof(DiscreteDistribution.Draws), .. Shared];

    public override DiscreteModel Build(DiscreteDistribution tool)
    {
        int population = Whole(tool.PopulationSize, "the population size");
        int successes = Whole(tool.PopulationSuccesses, "the successes in the population");
        int draws = Whole(tool.Draws, "the draws");

        if (population < 1 || successes < 0 || successes > population || draws < 1 || draws > population)
        {
            throw new ArgumentOutOfRangeException(nameof(tool.PopulationSize), "Need 0 ≤ K ≤ N and 1 ≤ n ≤ N.");
        }

        return new HypergeometricModel(population, successes, draws);
    }

    public override void Declare(DiscreteDistribution tool, ScriptBuilder s)
    {
        s.Assign("N", Whole(tool.PopulationSize, "N")).Assign("K", Whole(tool.PopulationSuccesses, "K")).Assign("n", Whole(tool.Draws, "n"));
        s.Either("X = stats.hypergeom(N, K, n)   # (population, successes, draws)", "# dhyper(k, m = K, n = N - K, k = n draws)");
    }

    public override string RPmf(string k) => $"dhyper({k}, K, N - K, n)";

    public override string RCdf(string k) => $"phyper({k}, K, N - K, n)";

    public override string RMean => "n * K / N";

    public override string RVariance => "n * K / N * (1 - K / N) * (N - n) / (N - 1)";
}

public sealed class TableFamily : DiscreteFamilyMethod
{
    public override DiscreteFamily Selects => DiscreteFamily.Table;

    public override string Name => "Typed probability distribution";

    public override string Description => "E[X] = Σ x·P(x), V[X] = Σ (x − μ)²·P(x) over the rows entered; P(X ≤ k) sums the rows up to k.";

    public override string Citation => "Montgomery and Runger, 7th ed., §3-2 to 3-4 (probability mass functions, mean and variance).";

    public override IReadOnlyList<string> Inputs => [nameof(DiscreteDistribution.Table), .. Shared];

    public override DiscreteModel Build(DiscreteDistribution tool) => new TableModel(tool.Table.Points.Select(p => (p.X, p.Y)));

    public override void Declare(DiscreteDistribution tool, ScriptBuilder s)
    {
        s.Vector("x", tool.Table.XValues).Vector("p", tool.Table.YValues);
        s.Either("X = stats.rv_discrete(values=(x, p))", "# Sums over the rows of the table");
    }

    public override string RPmf(string k) => $"sum(p[x == {k}])";

    public override string RCdf(string k) => $"sum(p[x <= {k}])";

    public override string RMean => "sum(x * p)";

    public override string RVariance => "sum((x - sum(x * p))^2 * p)";
}
