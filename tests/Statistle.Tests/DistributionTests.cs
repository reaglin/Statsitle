using Statistle.Core.Analyses;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Reporting;

namespace Statistle.Tests;

/// <summary>Modules 3 to 6: probability rules, discrete and continuous distributions, sampling distributions, against published values.</summary>
public class DistributionTests
{
    // ── Discrete models (Module 4) ────────────────────────────────────────────

    [Fact]
    public void Binomial_matches_the_course_problem_and_its_moments()
    {
        var b = new BinomialModel(20, 0.05);   // EGN3443 Module 4, problem 1

        Assert.Equal(0.1887, b.Pmf(2), 4);
        Assert.Equal(0.9245, b.Cdf(2), 4);
        Assert.Equal(1.0, b.Mean, 9);
        Assert.Equal(0.95, b.Variance, 9);
        Assert.Equal(0, b.Pmf(21), 9);
        Assert.Equal(1, b.Cdf(20), 9);
        Assert.Equal(b.Cdf(4) - b.Cdf(1), b.Between(2, 4), 12);
    }

    [Fact]
    public void Poisson_geometric_negative_binomial_and_hypergeometric_follow_their_formulas()
    {
        Assert.Equal(Math.Exp(-3) * 9 / 2, new PoissonModel(3).Pmf(2), 12);
        Assert.Equal(3, new PoissonModel(3).Variance, 12);

        var g = new GeometricModel(0.2);
        Assert.Equal(0.8 * 0.8 * 0.2, g.Pmf(3), 12);
        Assert.Equal(5, g.Mean, 12);
        Assert.Equal(0, g.Pmf(0), 12);
        Assert.Equal(1 - Math.Pow(0.8, 3), g.Cdf(3), 12);

        var nb = new NegativeBinomialModel(3, 0.5);   // trials to the 3rd success
        Assert.Equal(0.1875, nb.Pmf(5), 12);           // C(4,2)·0.5³·0.5²
        Assert.Equal(0.5, nb.Cdf(5), 12);              // 0.125 + 0.1875 + 0.1875
        Assert.Equal(6, nb.Mean, 12);
        Assert.Equal(3, nb.SupportMin);

        var h = new HypergeometricModel(10, 4, 3);
        Assert.Equal(0.5, h.Pmf(1), 12);               // C(4,1)·C(6,2)/C(10,3) = 60/120
        Assert.Equal(1.2, h.Mean, 12);
        Assert.Equal(3, h.SupportMax);
    }

    [Fact]
    public void A_typed_table_gives_its_expectation_and_variance_and_must_sum_to_one()
    {
        var t = new TableModel([(0, 0.1), (1, 0.2), (2, 0.4), (3, 0.3)]);

        Assert.Equal(1.9, t.Mean, 12);
        Assert.Equal(0.89, t.Variance, 12);            // Σx²p − μ² = 4.5 − 3.61
        Assert.Equal(0.3, t.Cdf(1), 12);
        Assert.Equal(0.4, t.Pmf(2), 12);
        Assert.Equal(0, t.Pmf(7), 12);

        Assert.Throws<ArgumentOutOfRangeException>(() => new TableModel([(0, 0.5), (1, 0.4)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TableModel([(0.5, 1.0)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TableModel([]));
    }

    // ── Continuous models (Module 5) ──────────────────────────────────────────

    [Fact]
    public void Normal_t_chi_squared_and_F_reproduce_the_table_values()
    {
        var z = new NormalModel(0, 1);
        Assert.Equal(0.9750, z.Cdf(1.96), 4);
        Assert.Equal(1.959964, z.Quantile(0.975), 6);
        Assert.Equal(1.0, new NormalModel(50, 5).Z(55), 12);
        Assert.Equal(0.6827, z.Between(-1, 1), 4);

        Assert.Equal(2.228, new StudentTModel(10).Quantile(0.975), 3);
        Assert.Equal(18.307, new ChiSquaredModel(10).Quantile(0.95), 3);
        Assert.Equal(3.326, new FModel(5, 10).Quantile(0.95), 3);
        Assert.True(double.IsNaN(new StudentTModel(1).Mean));
        Assert.Equal(10.0 / 8, new StudentTModel(10).Variance, 9);
    }

    [Fact]
    public void Uniform_exponential_and_lognormal_follow_their_closed_forms()
    {
        var u = new UniformModel(2, 5);
        Assert.Equal(1.0 / 3, u.Cdf(3), 12);
        Assert.Equal(3.5, u.Mean, 12);
        Assert.Equal(0.75, u.Variance, 12);
        Assert.Equal(1.0 / 3, u.Density(4), 12);

        var e = new ExponentialModel(2);
        Assert.Equal(1 - Math.Exp(-2), e.Cdf(1), 12);
        Assert.Equal(0.5, e.Mean, 12);
        Assert.Equal(0.25, e.Variance, 12);
        Assert.Equal(Math.Log(2) / 2, e.Median, 9);

        var ln = new LogNormalModel(3, 0.4);
        Assert.Equal(Math.Exp(3 + 0.08), ln.Mean, 9);
        Assert.Equal(Math.Exp(3), ln.Median, 6);
        Assert.Equal(0.5, ln.Cdf(Math.Exp(3)), 9);
    }

    // ── The analyses ──────────────────────────────────────────────────────────

    [Fact]
    public void Probability_rules_follow_the_relationship_chosen()
    {
        var rules = new ProbabilityRules();   // independent, 0.3 and 0.5, 10 choose 3
        Assert.Null(rules.Problem);
        Assert.Equal(0.15, rules.IntersectionResult, 12);
        Assert.Equal(0.65, rules.Union, 12);
        Assert.Equal(0.3, rules.AGivenB, 12);
        Assert.Equal(0.5, rules.BGivenA, 12);
        Assert.StartsWith("Independent", rules.Independence, StringComparison.Ordinal);
        Assert.Equal(720, rules.Permutations, 9);
        Assert.Equal(120, rules.Combinations, 9);

        rules.Relationship = EventRelationship.MutuallyExclusive;
        rules.Recalculate();
        Assert.Equal(0, rules.IntersectionResult, 12);
        Assert.Equal(0.8, rules.Union, 12);
        Assert.StartsWith("Not independent", rules.Independence, StringComparison.Ordinal);

        rules.ProbabilityB = 0.75;
        rules.Recalculate();
        Assert.Contains("> 1", rules.Problem, StringComparison.Ordinal);

        // Bayes: a 1 % fault, a test with 95 % sensitivity and a 5 % false-positive rate.
        var bayes = new ProbabilityRules { Relationship = EventRelationship.KnownLikelihoods, ProbabilityA = 0.01, LikelihoodGivenA = 0.95, LikelihoodGivenNotA = 0.05 };
        bayes.Recalculate();
        Assert.Null(bayes.Problem);
        Assert.Equal(0.059, bayes.ProbabilityBResult, 12);
        Assert.Equal(0.0095 / 0.059, bayes.AGivenB, 12);

        var counting = new ProbabilityRules { Items = 3, Chosen = 5 };
        counting.Recalculate();
        Assert.Contains("r > n", counting.Problem, StringComparison.Ordinal);

        Assert.Equal(6, MethodCatalog.For(typeof(ProbabilityRules)).Count);
        Assert.All(MethodCatalog.For(typeof(ProbabilityRules)), m => Assert.Empty(MethodContract.Verify(m)));
        string[] visible = [.. MethodScope.VisibleProperties(bayes).Select(p => p.XmlName)];
        Assert.Contains(nameof(ProbabilityRules.LikelihoodGivenA), visible);
        Assert.DoesNotContain(nameof(ProbabilityRules.ProbabilityB), visible);
    }

    [Fact]
    public void The_discrete_analysis_reports_every_tail_and_shows_only_its_familys_parameters()
    {
        var d = new DiscreteDistribution();   // binomial 20, 0.05, k = 2
        Assert.Null(d.Problem);
        Assert.Equal(0.1887, d.Exactly, 4);
        Assert.Equal(0.9245, d.AtMost, 4);
        Assert.Equal(new BinomialModel(20, 0.05).Cdf(1), d.LessThan, 12);
        Assert.Equal(1 - d.LessThan, d.AtLeast, 12);
        Assert.Equal(1 - d.AtMost, d.MoreThan, 12);
        Assert.True(double.IsNaN(d.Between));
        Assert.Equal("0 to 20", d.Support);
        Assert.Equal(2, d.Figures(null).Count);

        d.K2 = 4;
        d.Recalculate();
        Assert.Equal(new BinomialModel(20, 0.05).Between(2, 4), d.Between, 12);

        d.K2 = 1;
        d.Recalculate();
        Assert.Contains("k₂", d.Problem, StringComparison.Ordinal);

        string[] Visible() => [.. MethodScope.VisibleProperties(d).Select(p => p.XmlName)];
        Assert.Contains(nameof(DiscreteDistribution.Trials), Visible());
        Assert.DoesNotContain(nameof(DiscreteDistribution.Rate), Visible());
        d.Family = DiscreteFamily.Poisson;
        d.K2 = double.NaN;
        d.Recalculate();
        Assert.Contains(nameof(DiscreteDistribution.Rate), Visible());
        Assert.DoesNotContain(nameof(DiscreteDistribution.Trials), Visible());
        Assert.Equal(3, d.Mean, 12);

        d.Family = DiscreteFamily.Table;
        d.Recalculate();
        Assert.Equal(1.9, d.Mean, 12);

        d.StartBlank();
        Assert.NotNull(d.Problem);
        Assert.Empty(d.Figures(null));

        Assert.Equal(6, MethodCatalog.For(typeof(DiscreteDistribution)).Count);
        Assert.All(MethodCatalog.For(typeof(DiscreteDistribution)), m => Assert.Empty(MethodContract.Verify(m)));
    }

    [Fact]
    public void The_continuous_analysis_gives_tails_quantiles_and_the_normal_extras()
    {
        var c = new ContinuousDistribution();   // normal 50, 5; x 55, x2 60, q 0.95
        Assert.Null(c.Problem);
        Assert.Equal(0.8413, c.AtMost, 4);
        Assert.Equal(1.0, c.ZScore, 12);
        Assert.Equal(new NormalModel(50, 5).Between(55, 60), c.Between, 12);
        Assert.Equal(50 + 1.644854 * 5, c.Quantile, 4);
        Assert.Contains("95.4 % in 40 to 60", c.EmpiricalRule, StringComparison.Ordinal);
        Assert.Equal(50, c.Median, 9);
        Assert.Equal(2, c.Figures(null).Count);

        string[] Visible() => [.. MethodScope.VisibleProperties(c).Select(p => p.XmlName)];
        Assert.Contains(nameof(ContinuousDistribution.ZScore), Visible());
        Assert.Contains(nameof(ContinuousDistribution.EmpiricalRule), Visible());

        c.Family = ContinuousFamily.StudentT;
        c.DegreesOfFreedom = 10;
        c.Probability = 0.975;
        c.Recalculate();
        Assert.Equal(2.228, c.Quantile, 3);
        Assert.DoesNotContain(nameof(ContinuousDistribution.ZScore), Visible());
        Assert.DoesNotContain(nameof(ContinuousDistribution.Mu), Visible());
        Assert.Contains(nameof(ContinuousDistribution.DegreesOfFreedom), Visible());

        c.Family = ContinuousFamily.Exponential;
        c.Rate = 2;
        c.X = 1;
        c.X2 = double.NaN;
        c.Recalculate();
        Assert.Equal(1 - Math.Exp(-2), c.AtMost, 12);
        Assert.Equal(0.5, c.Mean, 12);

        c.Rate = -1;
        c.Recalculate();
        Assert.Contains("greater than zero", c.Problem, StringComparison.Ordinal);

        Assert.Equal(7, MethodCatalog.For(typeof(ContinuousDistribution)).Count);
        Assert.All(MethodCatalog.For(typeof(ContinuousDistribution)), m => Assert.Empty(MethodContract.Verify(m)));
        Assert.Contains(ReportBuilder.For(new ContinuousDistribution()).Citations, ci => ci.Method.Contains("Normal", StringComparison.Ordinal));
    }

    [Fact]
    public void The_sampling_figure_shades_the_side_asked_and_a_normal_population_needs_no_large_n()
    {
        // Module 6 content: "P(x̄ ≥ a)" was drawn as the area below a; a stated normal population was warned about at n < 30.
        var s = new SamplingDistribution { SampleSize = 5 };
        s.Recalculate();
        Assert.Contains("only if", s.Condition, StringComparison.Ordinal);
        var below = s.Figures(null)[0].Series.OfType<Statistle.Core.Plotting.PolygonSeries>().Single();
        Assert.True(below.X.Max() <= s.Value + 1e-9);

        s.Shape = PopulationShape.Normal;
        s.Shade = ShadedTail.Above;
        s.Recalculate();
        Assert.Contains("exactly normal", s.Condition, StringComparison.Ordinal);
        var above = s.Figures(null)[0].Series.OfType<Statistle.Core.Plotting.PolygonSeries>().Single();
        Assert.True(above.X.Min() >= s.Value - 1e-9);
        Assert.True(above.X.Max() > s.Value);
    }

    [Fact]
    public void The_sampling_distribution_narrows_by_root_n_and_states_the_CLT_condition()
    {
        var s = new SamplingDistribution();   // EGN3443 Module 6 problem 1: μ 4200, σ 350, n 30, x̄ 4300
        Assert.Null(s.Problem);
        Assert.Equal(4200, s.Centre, 9);
        Assert.Equal(350 / Math.Sqrt(30), s.StandardError, 9);
        Assert.Equal(100 / (350 / Math.Sqrt(30)), s.Z, 9);
        Assert.Equal(new NormalModel(0, 1).Cdf(s.Z), s.AtMost, 9);
        Assert.Equal(0.9412, s.AtMost, 3);
        Assert.Contains("≥ 30", s.Condition, StringComparison.Ordinal);
        Assert.Equal(4200 - 1.959964 * s.StandardError, s.Lower95, 9);
        Assert.Single(s.Figures(null));

        s.SampleSize = 5;
        s.Recalculate();
        Assert.Equal(350 / Math.Sqrt(5), s.StandardError, 9);
        Assert.Contains("< 30", s.Condition, StringComparison.Ordinal);

        s.Statistic = SamplingStatistic.SampleProportion;
        s.PopulationProportion = 0.15;
        s.SampleSize = 100;
        s.Value = 0.2;
        s.Recalculate();
        Assert.Equal(Math.Sqrt(0.15 * 0.85 / 100), s.StandardError, 12);
        Assert.Contains("both ≥ 10", s.Condition, StringComparison.Ordinal);

        s.SampleSize = 20;
        s.Recalculate();
        Assert.Contains("below 10", s.Condition, StringComparison.Ordinal);

        Assert.Equal(2, MethodCatalog.For(typeof(SamplingDistribution)).Count);
        Assert.All(MethodCatalog.For(typeof(SamplingDistribution)), m => Assert.Empty(MethodContract.Verify(m)));
    }
}
