using Statistle.Core.Methods.Inference;

namespace Statistle.Tests;

/// <summary>Modules 7–10: intervals, tests, power and the rank tests against Montgomery &amp; Runger's worked examples.</summary>
public class InferenceTests
{
    [Fact]
    public void Confidence_intervals_match_the_textbook_examples()
    {
        // M&R Example 8-1: impact energy, x̄ 64.46 J, σ 1, n 10, 95 %: 63.84 ≤ μ ≤ 65.08.
        Interval ci = Intervals.MeanSigmaKnown(64.46, 1, 10, 0.95);
        Assert.Equal(63.84, ci.Lower, 2);
        Assert.Equal(65.08, ci.Upper, 2);
        Assert.Equal(1.959964, ci.Critical, 5);

        // M&R Example 8-5: alloy load, x̄ 13.71, s 3.55, n 22, 95 %: 12.14 ≤ μ ≤ 15.28 (t = 2.080).
        Interval t = Intervals.MeanSigmaUnknown(13.71, 3.55, 22, 0.95);
        Assert.Equal(2.080, t.Critical, 3);
        Assert.Equal(12.14, t.Lower, 2);
        Assert.Equal(15.28, t.Upper, 2);
        Assert.Equal(21, t.DegreesOfFreedom);

        // M&R Example 8-7: 10 of 85 bearings defective, 95 %. The book rounds p̂ to 0.12 and gets
        // 0.0509 ≤ p ≤ 0.1891; with p̂ = 10/85 exactly the ends are 0.0492 and 0.1861.
        Interval p = Intervals.Proportion(10, 85, 0.95);
        double phat = 10.0 / 85, half = 1.959964 * Math.Sqrt(phat * (1 - phat) / 85);
        Assert.Equal(phat - half, p.Lower, 6);
        Assert.Equal(phat + half, p.Upper, 6);
        Assert.Equal(0.0492, p.Lower, 4);

        // Wilson brackets Wald's centre more tightly for a small proportion and stays inside [0, 1].
        Interval wilson = Intervals.ProportionWilson(1, 20, 0.95);
        Assert.True(wilson.Lower > 0 && wilson.Lower > Intervals.Proportion(1, 20, 0.95).Lower);

        // Variance: (n−1)s²/χ² — s² = 4, n = 20: 19·4/32.852 ≤ σ² ≤ 19·4/8.907.
        Interval v = Intervals.Variance(4, 20, 0.95);
        Assert.Equal(76 / 32.852, v.Lower, 3);
        Assert.Equal(76 / 8.907, v.Upper, 3);

        // Sample sizes, M&R Examples 8-2 and 8-8: σ 1, E 0.5 → n = 16; p 0.12, E 0.05 → n = 163; worst case 385.
        Assert.Equal(16, Intervals.SampleSizeForMean(1, 0.5, 0.95));
        Assert.Equal(163, Intervals.SampleSizeForProportion(0.12, 0.05, 0.95));
        Assert.Equal(385, Intervals.SampleSizeForProportion(0.5, 0.05, 0.95));
    }

    [Fact]
    public void One_sample_tests_match_the_textbook_examples()
    {
        // M&R Example 9-2: propellant burning rate, μ₀ 50, σ 2, n 25, x̄ 51.3: z = 3.25, p = 0.0012.
        TestOutcome z = HypothesisTests.ZTestMean(51.3, 2, 25, 50, Alternative.TwoSided, 0.05);
        Assert.Equal(3.25, z.Statistic, 9);
        Assert.Equal(0.0012, z.PValue, 4);
        Assert.True(z.Reject);
        Assert.Equal(-1.96, z.CriticalLow, 2);
        Assert.Equal("z < -1.9600 or z > 1.9600", z.RejectionRegion("z"));

        // M&R Example 9-6: golf clubs, n 15, x̄ 0.83725, s 0.02456, H₁: μ > 0.82: t = 2.72, p ≈ 0.008.
        TestOutcome t = HypothesisTests.TTestMean(0.83725, 0.02456, 15, 0.82, Alternative.Greater, 0.05);
        Assert.Equal(2.72, t.Statistic, 2);
        Assert.Equal(14, t.DegreesOfFreedom);
        Assert.InRange(t.PValue, 0.005, 0.010);
        Assert.True(double.IsNaN(t.CriticalLow));
        Assert.Equal(1.761, t.CriticalHigh, 3);

        // The lower-tail p is the complement of the upper-tail p, and the two-sided p is twice the smaller.
        TestOutcome less = HypothesisTests.TTestMean(0.83725, 0.02456, 15, 0.82, Alternative.Less, 0.05);
        Assert.Equal(1 - t.PValue, less.PValue, 12);
        Assert.Equal(2 * t.PValue, HypothesisTests.TTestMean(0.83725, 0.02456, 15, 0.82, Alternative.TwoSided, 0.05).PValue, 12);

        // EGN3443 Module 8 problem 1: 5 defectives in 200, p₀ 0.05, H₁: p < 0.05.
        // z = −1.62, p = 0.052: not quite significant at α = 0.05 — the problem's teaching point.
        TestOutcome prop = HypothesisTests.ZTestProportion(5, 200, 0.05, Alternative.Less, 0.05);
        Assert.Equal((0.025 - 0.05) / Math.Sqrt(0.05 * 0.95 / 200), prop.Statistic, 12);
        Assert.InRange(prop.PValue, 0.050, 0.055);
        Assert.False(prop.Reject);

        // M&R Example 9-8: bottle filling, s² 0.0153, n 20, H₁: σ² > 0.01: χ² = 29.07.
        TestOutcome chi = HypothesisTests.ChiSquareTestVariance(0.0153, 20, 0.01, Alternative.Greater, 0.05);
        Assert.Equal(29.07, chi.Statistic, 2);
        Assert.Equal(30.14, chi.CriticalHigh, 2);
        Assert.False(chi.Reject);
    }

    [Fact]
    public void Two_sample_tests_match_the_textbook_examples()
    {
        // M&R Example 10-5: catalyst yields, n 8 each, x̄ 92.255 / 92.733, s 2.39 / 2.98: s_p = 2.70, t = −0.35.
        TestOutcome pooled = HypothesisTests.PooledT(92.255, 2.39, 8, 92.733, 2.98, 8, 0, Alternative.TwoSided, 0.05);
        Assert.Equal(2.70, Intervals.PooledStandardDeviation(2.39, 8, 2.98, 8), 2);
        Assert.Equal(-0.35, pooled.Statistic, 2);
        Assert.Equal(14, pooled.DegreesOfFreedom);
        Assert.False(pooled.Reject);

        // Welch with equal n and s equals pooled; with unequal s the degrees of freedom fall below n₁ + n₂ − 2.
        Assert.Equal(pooled.Statistic, HypothesisTests.WelchT(92.255, 2.39, 8, 92.733, 2.98, 8, 0, Alternative.TwoSided, 0.05).Statistic, 12);
        Assert.True(Intervals.WelchDegreesOfFreedom(2.39, 8, 2.98, 8) < 14);
        Assert.Equal(14, Intervals.WelchDegreesOfFreedom(2, 8, 2, 8), 9);

        // M&R Example 10-9: shear strength by two methods, d̄ 0.2739, s_d 0.1351, n 9: t = 6.08.
        TestOutcome paired = HypothesisTests.PairedT(0.2739, 0.1351, 9, 0, Alternative.TwoSided, 0.05);
        Assert.Equal(6.08, paired.Statistic, 2);
        Assert.True(paired.PValue < 0.001);

        // Two proportions, pooled: 27/300 vs 19/300 by hand.
        TestOutcome props = HypothesisTests.TwoProportionsZ(27, 300, 19, 300, Alternative.TwoSided, 0.05);
        double pooledP = 46.0 / 600;
        Assert.Equal((0.09 - 19.0 / 300) / Math.Sqrt(pooledP * (1 - pooledP) * (2.0 / 300)), props.Statistic, 12);

        // F for two variances: F = s₁²/s₂² against F(n₁−1, n₂−1); the two-sided p doubles the smaller tail.
        TestOutcome f = HypothesisTests.FTestVariances(3, 16, 2, 16, Alternative.TwoSided, 0.05);
        Assert.Equal(2.25, f.Statistic, 12);
        Assert.Equal(15, f.DegreesOfFreedom);
        Assert.Equal(2.86, f.CriticalHigh, 2);
        Assert.False(f.Reject);

        Assert.Equal((92.255 - 92.733) / 2.70, HypothesisTests.CohensD(92.255, 2.39, 8, 92.733, 2.98, 8), 2);
        Interval ci = Intervals.TwoMeansPooled(92.255, 2.39, 8, 92.733, 2.98, 8, 0.95);
        Assert.True(ci.Lower < 0 && ci.Upper > 0, "no difference is inside the interval, as the test found");
    }

    [Fact]
    public void Power_and_the_sample_size_for_it_match_the_textbook_example()
    {
        // M&R Example 9-3: propellant, μ₀ 50, μ₁ 51, σ 2, n 25, two-sided α 0.05: β = 0.2946 → power 0.705.
        double power = HypothesisTests.PowerForMean(50, 51, 2, 25, Alternative.TwoSided, 0.05);
        Assert.Equal(1 - 0.2946, power, 3);

        // For β = 0.10 the formula gives n = [(1.96 + 1.2816)·2/1]² = 42.03, so 43; the book's OC chart reads "about 42".
        Assert.Equal(43, HypothesisTests.SampleSizeForPower(50, 51, 2, Alternative.TwoSided, 0.05, 0.90));
        Assert.True(HypothesisTests.PowerForMean(50, 51, 2, 43, Alternative.TwoSided, 0.05) >= 0.90);
        Assert.True(HypothesisTests.PowerForMean(50, 51, 2, 42, Alternative.TwoSided, 0.05) < 0.90);

        // One-tailed power for a shift in the direction of the alternative beats two-tailed.
        Assert.True(HypothesisTests.PowerForMean(50, 51, 2, 25, Alternative.Greater, 0.05) > power);
        Assert.True(HypothesisTests.PowerForMean(50, 51, 2, 25, Alternative.Less, 0.05) < 0.05);
    }

    [Fact]
    public void Rank_tests_and_assumption_checks_behave()
    {
        (double[] ranks, double ties) = Nonparametric.Rank([10, 20, 20, 30]);
        Assert.Equal([1, 2.5, 2.5, 4], ranks);
        Assert.Equal(6, ties, 9);   // one tie of 2: 2³ − 2

        double[] a = [1.1, 2.3, 2.9, 3.4, 4.8, 5.2, 6.0, 6.5, 7.1, 8.3];
        double[] b = [4.5, 5.5, 6.1, 7.2, 7.9, 8.8, 9.4, 9.9, 10.5, 11.2];
        RankTest mw = Nonparametric.MannWhitney(a, b, Alternative.TwoSided);
        Assert.True(mw.Statistic < 50, "the first sample ranks lower: U₁ below n₁n₂/2");
        Assert.True(mw.PValue < 0.05);
        Assert.Equal(mw.PValue / 2, Nonparametric.MannWhitney(a, b, Alternative.Less).PValue, 9);

        double[] d = [0.5, 1.2, -0.3, 0.8, 1.5, 0.9, -0.1, 1.1, 0.7, 0.4];
        RankTest w = Nonparametric.WilcoxonSignedRank(d, Alternative.TwoSided);
        Assert.True(w.PValue < 0.05);
        Assert.Equal(Nonparametric.WilcoxonSignedRank(d, Alternative.Greater).PValue, w.PValue / 2, 9);
        Assert.True(double.IsNaN(Nonparametric.WilcoxonSignedRank([0, 0], Alternative.TwoSided).Statistic));

        // Shapiro–Wilk: a near-normal sample passes, a heavily skewed one does not; W ≤ 1.
        AssumptionCheck normalish = Nonparametric.ShapiroWilk([470, 485, 478, 492, 460, 473, 488, 484, 482, 475]);
        Assert.InRange(normalish.Statistic, 0.9, 1.0);
        Assert.True(normalish.PValue > 0.05);
        AssumptionCheck skewed = Nonparametric.ShapiroWilk([1, 1, 1, 1, 2, 2, 3, 5, 9, 20, 45, 90]);
        Assert.True(skewed.PValue < 0.01);
        Assert.True(double.IsNaN(Nonparametric.ShapiroWilk([1, 2]).Statistic));

        // Levene: equal spreads pass, a fourfold spread fails.
        Assert.True(Nonparametric.Levene([a, [.. a.Select(v => v + 3)]]).PValue > 0.5);
        Assert.True(Nonparametric.Levene([a, [.. a.Select(v => v * 4)]]).PValue < 0.05);
    }
}
