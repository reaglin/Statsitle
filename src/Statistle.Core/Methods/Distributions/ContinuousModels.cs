using MathNet.Numerics.Distributions;

namespace Statistle.Core.Methods.Distributions;

/// <summary>
/// A continuous probability distribution: density, cumulative probability, quantile, moments,
/// and the range worth drawing. All of them sit on MathNet.Numerics; the parameterisations are
/// the course's (Montgomery &amp; Runger Ch. 4), which are also R's.
/// </summary>
public abstract class ContinuousModel
{
    public abstract string Describe { get; }

    /// <summary>The probability density f(x).</summary>
    public abstract double Density(double x);

    /// <summary>P(X ≤ x).</summary>
    public abstract double Cdf(double x);

    /// <summary>The x with P(X ≤ x) = p.</summary>
    public abstract double Quantile(double p);

    public abstract double Mean { get; }

    public abstract double Variance { get; }

    public double StandardDeviation => Math.Sqrt(Variance);

    public double Median => Quantile(0.5);

    /// <summary>P(a ≤ X ≤ b).</summary>
    public double Between(double a, double b) => a > b ? 0 : Cdf(b) - Cdf(a);

    /// <summary>The x range to draw: where nearly all the probability is.</summary>
    public abstract (double Low, double High) PlotRange { get; }
}

/// <summary>Every value between a and b equally likely: f(x) = 1/(b − a).</summary>
public sealed class UniformModel(double a, double b) : ContinuousModel
{
    private readonly ContinuousUniform _d = new(a, b);

    public double A { get; } = a;

    public double B { get; } = b;

    public override string Describe => $"Uniform(a = {A:G4}, b = {B:G4})";

    public override double Density(double x) => _d.Density(x);

    public override double Cdf(double x) => _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => (A + B) / 2;

    public override double Variance => (B - A) * (B - A) / 12;

    public override (double Low, double High) PlotRange => (A - 0.15 * (B - A), B + 0.15 * (B - A));
}

/// <summary>The normal (Gaussian) distribution with mean μ and standard deviation σ.</summary>
public sealed class NormalModel(double mu, double sigma) : ContinuousModel
{
    private readonly Normal _d = new(mu, sigma);

    public double Mu { get; } = mu;

    public double Sigma { get; } = sigma;

    public override string Describe => $"Normal(μ = {Mu:G4}, σ = {Sigma:G4})";

    public override double Density(double x) => _d.Density(x);

    public override double Cdf(double x) => _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => Mu;

    public override double Variance => Sigma * Sigma;

    /// <summary>z = (x − μ)/σ: how many standard deviations x lies from the mean.</summary>
    public double Z(double x) => (x - Mu) / Sigma;

    public override (double Low, double High) PlotRange => (Mu - 4 * Sigma, Mu + 4 * Sigma);
}

/// <summary>Waiting time between events at rate λ: f(x) = λe^(−λx), x ≥ 0. Memoryless.</summary>
public sealed class ExponentialModel(double rate) : ContinuousModel
{
    private readonly Exponential _d = new(rate);

    public double Rate { get; } = rate;

    public override string Describe => $"Exponential(λ = {Rate:G4})";

    public override double Density(double x) => x < 0 ? 0 : _d.Density(x);

    public override double Cdf(double x) => x < 0 ? 0 : _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => 1 / Rate;

    public override double Variance => 1 / (Rate * Rate);

    public override (double Low, double High) PlotRange => (0, Quantile(0.995));
}

/// <summary>ln X is normal with mean μ and standard deviation σ (R's meanlog, sdlog).</summary>
public sealed class LogNormalModel(double mu, double sigma) : ContinuousModel
{
    private readonly LogNormal _d = new(mu, sigma);

    public double Mu { get; } = mu;

    public double Sigma { get; } = sigma;

    public override string Describe => $"Lognormal(μ = {Mu:G4}, σ = {Sigma:G4} of ln X)";

    public override double Density(double x) => x <= 0 ? 0 : _d.Density(x);

    public override double Cdf(double x) => x <= 0 ? 0 : _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => Math.Exp(Mu + Sigma * Sigma / 2);

    public override double Variance => (Math.Exp(Sigma * Sigma) - 1) * Math.Exp(2 * Mu + Sigma * Sigma);

    public override (double Low, double High) PlotRange => (0, Quantile(0.99));
}

/// <summary>Student's t with ν degrees of freedom — the sampling distribution behind every t-test.</summary>
public sealed class StudentTModel(double degreesOfFreedom) : ContinuousModel
{
    private readonly StudentT _d = new(0, 1, degreesOfFreedom);

    public double DegreesOfFreedom { get; } = degreesOfFreedom;

    public override string Describe => $"t(ν = {DegreesOfFreedom:G4})";

    public override double Density(double x) => _d.Density(x);

    public override double Cdf(double x) => _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => DegreesOfFreedom > 1 ? 0 : double.NaN;

    public override double Variance => DegreesOfFreedom > 2 ? DegreesOfFreedom / (DegreesOfFreedom - 2) : double.NaN;

    public override (double Low, double High) PlotRange => (-5, 5);
}

/// <summary>Chi-squared with ν degrees of freedom — the distribution of a sample variance ratio.</summary>
public sealed class ChiSquaredModel(double degreesOfFreedom) : ContinuousModel
{
    private readonly ChiSquared _d = new(degreesOfFreedom);

    public double DegreesOfFreedom { get; } = degreesOfFreedom;

    public override string Describe => $"χ²(ν = {DegreesOfFreedom:G4})";

    public override double Density(double x) => x < 0 ? 0 : _d.Density(x);

    public override double Cdf(double x) => x < 0 ? 0 : _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => DegreesOfFreedom;

    public override double Variance => 2 * DegreesOfFreedom;

    public override (double Low, double High) PlotRange => (0, Quantile(0.995));
}

/// <summary>The F distribution with ν₁ and ν₂ degrees of freedom — the ratio of two variances, ANOVA's statistic.</summary>
public sealed class FModel(double numeratorDf, double denominatorDf) : ContinuousModel
{
    private readonly FisherSnedecor _d = new(numeratorDf, denominatorDf);

    public double NumeratorDf { get; } = numeratorDf;

    public double DenominatorDf { get; } = denominatorDf;

    public override string Describe => $"F(ν₁ = {NumeratorDf:G4}, ν₂ = {DenominatorDf:G4})";

    public override double Density(double x) => x < 0 ? 0 : _d.Density(x);

    public override double Cdf(double x) => x < 0 ? 0 : _d.CumulativeDistribution(x);

    public override double Quantile(double p) => _d.InverseCumulativeDistribution(p);

    public override double Mean => DenominatorDf > 2 ? DenominatorDf / (DenominatorDf - 2) : double.NaN;

    public override double Variance =>
        DenominatorDf > 4
            ? 2 * DenominatorDf * DenominatorDf * (NumeratorDf + DenominatorDf - 2) / (NumeratorDf * (DenominatorDf - 2) * (DenominatorDf - 2) * (DenominatorDf - 4))
            : double.NaN;

    public override (double Low, double High) PlotRange => (0, Quantile(0.99));
}

/// <summary>
/// The Weibull with shape β and scale η (R's shape and scale; scipy's weibull_min c = β,
/// scale = η): P(X ≤ x) = 1 − e^(−(x/η)^β), x ≥ 0. β &lt; 1 failures that thin out (infant
/// mortality), β = 1 the exponential with λ = 1/η, β &gt; 1 wear-out.
/// </summary>
public sealed class WeibullModel(double shape, double scale) : ContinuousModel
{
    private readonly Weibull _d = new(shape, scale);

    public double Shape { get; } = shape;

    public double Scale { get; } = scale;

    public override string Describe => $"Weibull(β = {Shape:G4}, η = {Scale:G4})";

    public override double Density(double x) => x < 0 ? 0 : _d.Density(x);

    public override double Cdf(double x) => x <= 0 ? 0 : 1 - Math.Exp(-Math.Pow(x / Scale, Shape));

    public override double Quantile(double p) => Scale * Math.Pow(-Math.Log(1 - p), 1 / Shape);

    public override double Mean => Scale * MathNet.Numerics.SpecialFunctions.Gamma(1 + 1 / Shape);

    public override double Variance
    {
        get
        {
            double g1 = MathNet.Numerics.SpecialFunctions.Gamma(1 + 1 / Shape);
            return Scale * Scale * (MathNet.Numerics.SpecialFunctions.Gamma(1 + 2 / Shape) - g1 * g1);
        }
    }

    /// <summary>The hazard (instantaneous failure) rate h(x) = f(x)/R(x) = (β/η)(x/η)^(β−1).</summary>
    public double Hazard(double x) => x < 0 ? 0 : Shape / Scale * Math.Pow(x / Scale, Shape - 1);

    public override (double Low, double High) PlotRange => (0, Quantile(0.995));
}
