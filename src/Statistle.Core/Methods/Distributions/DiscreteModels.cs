using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace Statistle.Core.Methods.Distributions;

/// <summary>
/// A discrete probability distribution as the course uses it: a probability mass function on
/// the integers, its cumulative form, mean and variance, and the range worth drawing. The
/// standard ones sit on MathNet.Numerics; the course's own forms (a negative binomial that
/// counts trials, a typed table) are written out.
/// </summary>
public abstract class DiscreteModel
{
    /// <summary>The distribution's name with its parameters, for titles and reports.</summary>
    public abstract string Describe { get; }

    /// <summary>P(X = k).</summary>
    public abstract double Pmf(int k);

    /// <summary>P(X ≤ k).</summary>
    public abstract double Cdf(int k);

    public abstract double Mean { get; }

    public abstract double Variance { get; }

    public double StandardDeviation => Math.Sqrt(Variance);

    /// <summary>The smallest value with any probability.</summary>
    public abstract int SupportMin { get; }

    /// <summary>The largest value with any probability, or the point beyond which less than 0.05 % of the mass lies.</summary>
    public abstract int SupportMax { get; }

    /// <summary>P(a ≤ X ≤ b).</summary>
    public double Between(int a, int b) => a > b ? 0 : Cdf(b) - (a > SupportMin ? Cdf(a - 1) : 0);

    /// <summary>Where an infinite support is cut for drawing: the first k with 99.95 % of the mass below it.</summary>
    protected int TailCut(int start)
    {
        int k = start;

        while (Cdf(k) < 0.9995 && k < start + 100_000)
        {
            k++;
        }

        return k;
    }
}

/// <summary>X = number of successes in n independent trials, each succeeding with probability p.</summary>
public sealed class BinomialModel(int trials, double p) : DiscreteModel
{
    private readonly Binomial _d = new(p, trials);

    public int Trials { get; } = trials;

    public double P { get; } = p;

    public override string Describe => $"Binomial(n = {Trials}, p = {P:G4})";

    public override double Pmf(int k) => k < 0 || k > Trials ? 0 : _d.Probability(k);

    public override double Cdf(int k) => k < 0 ? 0 : k >= Trials ? 1 : _d.CumulativeDistribution(k);

    public override double Mean => Trials * P;

    public override double Variance => Trials * P * (1 - P);

    public override int SupportMin => 0;

    public override int SupportMax => Trials;
}

/// <summary>X = number of events in an interval when they occur at a steady mean rate λ.</summary>
public sealed class PoissonModel(double lambda) : DiscreteModel
{
    private readonly Poisson _d = new(lambda);

    public double Lambda { get; } = lambda;

    public override string Describe => $"Poisson(λ = {Lambda:G4})";

    public override double Pmf(int k) => k < 0 ? 0 : _d.Probability(k);

    public override double Cdf(int k) => k < 0 ? 0 : _d.CumulativeDistribution(k);

    public override double Mean => Lambda;

    public override double Variance => Lambda;

    public override int SupportMin => 0;

    public override int SupportMax => TailCut((int)Math.Ceiling(Lambda));
}

/// <summary>X = number of trials up to and including the first success: P(X = k) = (1 − p)^(k−1)·p, k ≥ 1.</summary>
public sealed class GeometricModel(double p) : DiscreteModel
{
    public double P { get; } = p;

    public override string Describe => $"Geometric(p = {P:G4})";

    public override double Pmf(int k) => k < 1 ? 0 : Math.Pow(1 - P, k - 1) * P;

    public override double Cdf(int k) => k < 1 ? 0 : 1 - Math.Pow(1 - P, k);

    public override double Mean => 1 / P;

    public override double Variance => (1 - P) / (P * P);

    public override int SupportMin => 1;

    public override int SupportMax => TailCut(1);
}

/// <summary>
/// X = number of trials up to and including the r-th success (Montgomery &amp; Runger's form):
/// P(X = x) = C(x − 1, r − 1)·p^r·(1 − p)^(x − r), x ≥ r.
/// </summary>
public sealed class NegativeBinomialModel(int successes, double p) : DiscreteModel
{
    public int Successes { get; } = successes;

    public double P { get; } = p;

    public override string Describe => $"Negative binomial(r = {Successes}, p = {P:G4}), counting trials";

    public override double Pmf(int k) =>
        k < Successes ? 0 : SpecialFunctions.Binomial(k - 1, Successes - 1) * Math.Pow(P, Successes) * Math.Pow(1 - P, k - Successes);

    public override double Cdf(int k)
    {
        double sum = 0;

        for (int x = Successes; x <= k; x++)
        {
            sum += Pmf(x);
        }

        return Math.Min(sum, 1);
    }

    public override double Mean => Successes / P;

    public override double Variance => Successes * (1 - P) / (P * P);

    public override int SupportMin => Successes;

    public override int SupportMax => TailCut(Successes);
}

/// <summary>X = number of successes in n draws without replacement from N items of which K are successes.</summary>
public sealed class HypergeometricModel(int population, int successes, int draws) : DiscreteModel
{
    private readonly Hypergeometric _d = new(population, successes, draws);

    public int Population { get; } = population;

    public int Successes { get; } = successes;

    public int Draws { get; } = draws;

    public override string Describe => $"Hypergeometric(N = {Population}, K = {Successes}, n = {Draws})";

    public override double Pmf(int k) => k < SupportMin || k > SupportMax ? 0 : _d.Probability(k);

    public override double Cdf(int k) => k < SupportMin ? 0 : k >= SupportMax ? 1 : _d.CumulativeDistribution(k);

    public override double Mean => (double)Draws * Successes / Population;

    public override double Variance =>
        (double)Draws * Successes / Population * (1 - (double)Successes / Population) * (Population - Draws) / (Population - 1.0);

    public override int SupportMin => Math.Max(0, Draws - (Population - Successes));

    public override int SupportMax => Math.Min(Draws, Successes);
}

/// <summary>A distribution typed as a table of (x, p) pairs — the course's first examples.</summary>
public sealed class TableModel : DiscreteModel
{
    private readonly SortedDictionary<int, double> _p = [];

    public TableModel(IEnumerable<(double X, double P)> rows)
    {
        foreach ((double x, double p) in rows)
        {
            if (double.IsNaN(x) || double.IsNaN(p))
            {
                continue;
            }

            if (Math.Abs(x - Math.Round(x)) > 1e-9)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), $"The value {x} is not a whole number; a discrete distribution lists integer outcomes.");
            }

            if (p < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), $"P({x}) = {p} is negative.");
            }

            _p[(int)Math.Round(x)] = _p.GetValueOrDefault((int)Math.Round(x)) + p;
        }

        if (_p.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Enter at least one (x, p) row.");
        }

        Total = _p.Values.Sum();

        if (Math.Abs(Total - 1) > 0.005)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), $"The probabilities sum to {Total:G4}, not 1.");
        }
    }

    /// <summary>The sum of the typed probabilities — should be one.</summary>
    public double Total { get; }

    public override string Describe => $"Typed distribution over {_p.Count} value(s)";

    public override double Pmf(int k) => _p.GetValueOrDefault(k);

    public override double Cdf(int k) => _p.Where(e => e.Key <= k).Sum(e => e.Value);

    public override double Mean => _p.Sum(e => e.Key * e.Value);

    public override double Variance
    {
        get
        {
            double mean = Mean;
            return _p.Sum(e => (e.Key - mean) * (e.Key - mean) * e.Value);
        }
    }

    public override int SupportMin => _p.Keys.Min();

    public override int SupportMax => _p.Keys.Max();
}
