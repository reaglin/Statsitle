using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Reporting;

namespace Statistle.Tests;

/// <summary>Modules 7–10 as the student meets them: the four analyses, from summaries and from data columns.</summary>
public class Phase3AnalysisTests
{
    private static readonly double[] CatalystA = [91.50, 94.18, 92.18, 95.39, 91.79, 89.07, 94.72, 89.21];
    private static readonly double[] CatalystB = [89.19, 90.95, 90.46, 93.21, 97.19, 97.04, 91.07, 92.75];

    /// <summary>A project with Montgomery &amp; Runger's catalyst yields (Example 10-5) as two columns, plus a 1/0 column.</summary>
    internal static StatistleDocument CatalystProject(out DataTable table)
    {
        var document = new StatistleDocument();
        table = document.AddTable();
        table.Name = "Catalyst";
        table.Columns.AddColumn("A", CatalystA);
        table.Columns.AddColumn("B", CatalystB);
        table.Columns.AddColumn("Pass", [1, 1, 0, 1, 1, 1, 0, 1, 1, 1]);
        return document;
    }

    [Fact]
    public void The_interval_analysis_reproduces_the_textbook_and_reads_columns()
    {
        var ci = new ConfidenceInterval();   // t, 13.71, 3.55, 22 — M&R Example 8-5
        Assert.Null(ci.Problem);
        Assert.Equal(12.14, ci.Lower, 2);
        Assert.Equal(15.28, ci.Upper, 2);
        Assert.Equal(21, ci.DegreesOfFreedom);
        Assert.Contains("95 % CI for μ", ci.Statement, StringComparison.Ordinal);
        Assert.Single(ci.Figures(null));

        ci.Target = IntervalFor.Proportion;
        ci.Recalculate();
        Assert.Equal(10.0 / 22, ci.Estimate, 12);
        Assert.False(double.IsNaN(ci.WilsonLower));
        Assert.True(double.IsNaN(ci.SdLower));

        ci.Target = IntervalFor.Variance;
        ci.Recalculate();
        Assert.Equal(Math.Sqrt(ci.Lower), ci.SdLower, 12);
        Assert.True(double.IsNaN(ci.MarginOfError));

        // From columns: the summaries are computed and locked, and the pooled interval matches the engine.
        StatistleDocument project = CatalystProject(out DataTable table);
        var pooled = new ConfidenceInterval { Project = project, Target = IntervalFor.TwoMeansPooled, Source = DataSource.Columns,
            Column = DataSources.Reference(table.Id, "A"), Column2 = DataSources.Reference(table.Id, "B") };
        pooled.Recalculate();
        Assert.Null(pooled.Problem);
        Assert.Equal(8, pooled.SampleSize);
        Assert.Equal(CatalystA.Average(), pooled.SampleMean, 9);
        Assert.True(MethodScope.IsLocked(pooled, nameof(ConfidenceInterval.SampleMean)));
        Assert.Equal(14, pooled.DegreesOfFreedom);
        Interval expected = Intervals.TwoMeansPooled(pooled.SampleMean, pooled.SampleSd, 8, pooled.Sample2Mean, pooled.Sample2Sd, 8, 0.95);
        Assert.Equal(expected.Lower, pooled.Lower, 9);

        pooled.Target = IntervalFor.Proportion;
        pooled.Column = DataSources.Reference(table.Id, "Pass");
        pooled.Recalculate();
        Assert.Null(pooled.Problem);
        Assert.Equal(8, pooled.Successes);
        Assert.Equal(10, pooled.SampleSize);

        pooled.Target = IntervalFor.PairedMeans;
        pooled.Recalculate();
        Assert.Contains("same number", pooled.Problem, StringComparison.Ordinal);   // 10 vs 8 values

        Assert.Equal(11, MethodCatalog.For(typeof(ConfidenceInterval)).Count);
        Assert.All(MethodCatalog.For(typeof(ConfidenceInterval)), m => Assert.Empty(MethodContract.Verify(m)));

        var blank = new ConfidenceInterval();
        blank.StartBlank();
        Assert.NotNull(blank.Problem);
    }

    [Fact]
    public void The_sample_size_analysis_gives_the_textbook_answers()
    {
        var n = new SampleSize();   // mean, σ 1, E 0.5, 95 % — M&R Example 8-2
        Assert.Equal(16, n.SampleSizeNeeded);
        Assert.Contains("rounded up", n.Note, StringComparison.Ordinal);

        n.Target = SizeFor.Proportion;
        n.Margin = 0.05;
        n.Recalculate();
        Assert.Equal(385, n.SampleSizeNeeded);

        n.ProportionGuess = 0.12;
        n.Recalculate();
        Assert.Equal(163, n.SampleSizeNeeded);

        n.Margin = 0;
        n.Recalculate();
        Assert.NotNull(n.Problem);
        Assert.All(MethodCatalog.For(typeof(SampleSize)), m => Assert.Empty(MethodContract.Verify(m)));
    }

    [Fact]
    public void The_one_sample_analysis_tests_decides_and_gives_power()
    {
        var z = new OneSampleTest();   // z, μ₀ 50, σ 2, n 25, x̄ 51.3, μ₁ 51 — M&R Examples 9-2 and 9-3
        Assert.Null(z.Problem);
        Assert.Equal(3.25, z.Statistic, 9);
        Assert.Equal(0.0012, z.PValue, 4);
        Assert.StartsWith("Reject H₀", z.Decision, StringComparison.Ordinal);
        Assert.Equal("z < -1.9600 or z > 1.9600", z.RejectionRegion);
        Assert.Equal("H₀: μ = 50 vs H₁: μ ≠ 50", z.Hypotheses);
        Assert.Equal(0.7054, z.Power, 3);
        Assert.Equal(43, z.SampleSizeForPower);
        Assert.True(z.CiLower < 51.3 && z.CiUpper > 51.3);
        Assert.Single(z.Figures(null));

        z.AlternativeHypothesis = Alternative.Greater;
        z.Recalculate();
        Assert.Equal("z > 1.6449", z.RejectionRegion);
        Assert.Equal(0.0006, z.PValue, 4);

        // t from a column: the summary is generated and locked, the library check is written.
        StatistleDocument project = CatalystProject(out DataTable table);
        var t = new OneSampleTest { Project = project, TestKind = OneSampleTestKind.MeanSigmaUnknown, Source = DataSource.Columns, Column = DataSources.Reference(table.Id, "A"), NullValue = 90, AlternativeValue = double.NaN };
        t.Recalculate();
        Assert.Null(t.Problem);
        Assert.True(MethodScope.IsLocked(t, nameof(OneSampleTest.SampleMean)));
        Assert.Equal(7, t.DegreesOfFreedom);
        Assert.Equal(HypothesisTests.TTestMean(CatalystA.Average(), t.SampleSd, 8, 90, Alternative.TwoSided, 0.05).PValue, t.PValue, 12);
        Assert.Contains("ttest_1samp", t.Code(Core.Code.CodeLanguage.Python), StringComparison.Ordinal);
        Assert.Contains("t.test(x", t.Code(Core.Code.CodeLanguage.R), StringComparison.Ordinal);
        Assert.True(double.IsNaN(t.Power));

        var v = new OneSampleTest { TestKind = OneSampleTestKind.Variance, SampleSd = Math.Sqrt(0.0153), SampleSize = 20, NullValue = 0.1, AlternativeHypothesis = Alternative.Greater };
        v.Recalculate();
        Assert.Equal(29.07, v.Statistic, 2);   // M&R Example 9-8
        Assert.StartsWith("Do not reject", v.Decision, StringComparison.Ordinal);

        Assert.Equal(6, MethodCatalog.For(typeof(OneSampleTest)).Count);
        Assert.All(MethodCatalog.For(typeof(OneSampleTest)), m => Assert.Empty(MethodContract.Verify(m)));
        string[] visible = [.. MethodScope.VisibleProperties(v).Select(p => p.XmlName)];
        Assert.DoesNotContain(nameof(OneSampleTest.Power), visible);
        Assert.DoesNotContain(nameof(OneSampleTest.Sigma), visible);
    }

    [Fact]
    public void The_two_sample_analysis_covers_every_test_and_checks_assumptions_from_columns()
    {
        var pooled = new TwoSampleTest();   // M&R Example 10-5 summaries
        Assert.Null(pooled.Problem);
        Assert.Equal(-0.35, pooled.Statistic, 2);
        Assert.Equal(2.70, pooled.PooledSd, 2);
        Assert.StartsWith("Do not reject", pooled.Decision, StringComparison.Ordinal);
        Assert.True(pooled.CiLower < 0 && pooled.CiUpper > 0);
        Assert.True(double.IsNaN(pooled.Shapiro1), "no data, no assumption checks");
        Assert.Single(pooled.Figures(null));

        pooled.TestKind = TwoSampleTestKind.MannWhitney;
        pooled.Recalculate();
        Assert.Contains("need the data", pooled.Problem, StringComparison.Ordinal);

        StatistleDocument project = CatalystProject(out DataTable table);
        var welch = new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.WelchT, Source = DataSource.Columns, Column = DataSources.Reference(table.Id, "A"), Column2 = DataSources.Reference(table.Id, "B") };
        welch.Recalculate();
        Assert.Null(welch.Problem);
        Assert.Equal(HypothesisTests.WelchT(CatalystA.Average(), welch.SampleSd, 8, CatalystB.Average(), welch.Sample2Sd, 8, 0, Alternative.TwoSided, 0.05).PValue, welch.PValue, 12);
        Assert.InRange(welch.Shapiro1, 0.8, 1.0);
        Assert.InRange(welch.Shapiro1P, 0, 1);
        Assert.InRange(welch.Shapiro2P, 0, 1);
        Assert.InRange(welch.LeveneP, 0, 1);
        Assert.False(double.IsNaN(welch.EffectSize));
        Assert.Equal(2, welch.Figures(null).Count);

        welch.TestKind = TwoSampleTestKind.MannWhitney;
        welch.Recalculate();
        Assert.Null(welch.Problem);
        Assert.Equal(Nonparametric.MannWhitney(CatalystA, CatalystB, Alternative.TwoSided).PValue, welch.PValue, 12);
        Assert.True(double.IsNaN(welch.DegreesOfFreedom));
        Assert.Contains("U₁", welch.Note, StringComparison.Ordinal);
        Assert.True(double.IsNaN(welch.Shapiro1), "the rank test does not report the parametric checks");

        welch.TestKind = TwoSampleTestKind.WilcoxonSignedRank;
        welch.Recalculate();
        Assert.Null(welch.Problem);
        Assert.Equal(Nonparametric.WilcoxonSignedRank([.. CatalystA.Zip(CatalystB, (a, b) => a - b)], Alternative.TwoSided).PValue, welch.PValue, 12);

        welch.TestKind = TwoSampleTestKind.PairedT;
        welch.Recalculate();
        Assert.Null(welch.Problem);
        Assert.Equal(7, welch.DegreesOfFreedom);
        Assert.False(double.IsNaN(welch.Shapiro1));
        Assert.True(double.IsNaN(welch.Levene), "one sample of differences: no variance comparison");

        welch.TestKind = TwoSampleTestKind.VarianceRatio;
        welch.Recalculate();
        Assert.Equal(welch.SampleSd * welch.SampleSd / (welch.Sample2Sd * welch.Sample2Sd), welch.Statistic, 12);
        Assert.True(welch.CiLower < welch.Statistic && welch.CiUpper > welch.Statistic);

        var props = new TwoSampleTest { TestKind = TwoSampleTestKind.TwoProportions, Successes = 27, SampleSize = 300, Successes2 = 19, Sample2Size = 300 };
        props.Recalculate();
        Assert.Equal(HypothesisTests.TwoProportionsZ(27, 300, 19, 300, Alternative.TwoSided, 0.05).Statistic, props.Statistic, 12);
        Assert.Equal("H₀: p₁ = p₂ vs H₁: p₁ ≠ p₂", props.Hypotheses);

        Assert.Equal(10, MethodCatalog.For(typeof(TwoSampleTest)).Count);
        Assert.All(MethodCatalog.For(typeof(TwoSampleTest)), m => Assert.Empty(MethodContract.Verify(m)));
        Assert.Contains(ReportBuilder.For(welch).Citations, c => c.Method.Contains("F-test", StringComparison.Ordinal));
    }
}
