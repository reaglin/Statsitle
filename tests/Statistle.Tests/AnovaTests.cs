using System.Globalization;
using MathNet.Numerics.Distributions;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>Module 14: the ANOVA engine against the course's alloy problem and a hand-worked two-factor case.</summary>
public class AnovaTests
{
    /// <summary>The course's Module 14 sample problem: tensile strength of three alloys, ten specimens each.</summary>
    internal static StatistleDocument AlloyProject(out DataTable alloys)
    {
        var document = new StatistleDocument();
        alloys = document.AddTable("Tensile strength");
        alloys.Columns.AddColumn("AA2024", [470, 485, 478, 492, 460, 473, 488, 484, 482, 475]);
        alloys.Columns.AddColumn("AA7075", [505, 518, 523, 508, 515, 522, 510, 529, 520, 514]);
        alloys.Columns.AddColumn("AA-X", [495, 508, 512, 492, 516, 505, 498, 510, 504, 489]);
        return document;
    }

    /// <summary>A balanced 3 × 2 design with three replicates: battery life by temperature and material.</summary>
    internal static StatistleDocument BatteryProject(out DataTable battery)
    {
        var document = new StatistleDocument();
        battery = document.AddTable("Battery life");
        battery.Columns.AddColumn("temperature", [1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3]);
        battery.Columns.AddColumn("material", [1, 1, 1, 2, 2, 2, 1, 1, 1, 2, 2, 2, 1, 1, 1, 2, 2, 2]);
        battery.Columns.AddColumn("life", [130, 155, 174, 150, 188, 159, 34, 40, 80, 136, 122, 106, 20, 70, 82, 25, 70, 58]);
        return document;
    }

    [Fact]
    public void The_one_way_engine_reproduces_the_course_alloy_problem()
    {
        AlloyProject(out DataTable alloys);
        OneWayResult r = Anova.OneWay([.. alloys.Columns.Columns.Select(c => c.Name)], [.. alloys.Columns.Columns.Select(c => (IReadOnlyList<double>)c.Values)])!;

        // The course page quotes F ≈ 53.7, p ≈ 5.85e-11, η² ≈ 0.80, but on the data as listed scipy's
        // f_oneway gives F = 48.116, p = 1.255e-9, η² = 0.781 — the engine agrees with scipy; means 478.7, 516.4, 502.9.
        Assert.Equal(30, r.N);
        Assert.Equal(478.7, r.Groups[0].Mean, 9);
        Assert.Equal(516.4, r.Groups[1].Mean, 9);
        Assert.Equal(502.9, r.Groups[2].Mean, 9);
        Assert.Equal(48.1162, r.F, 0.0005);
        Assert.Equal(1.255e-9, r.PValue, 1e-12);
        Assert.Equal(0.7809, r.EtaSquared, 0.0005);
        Assert.Equal(r.SsTotal, r.SsBetween + r.SsWithin, 9);
        Assert.Equal(27, r.DfWithin);
        Assert.Equal(0, r.Residuals.Sum(), 9);

        // Tukey: AA7075 − AA2024 ≈ 37.7, AA-X − AA2024 ≈ 24.2, AA7075 − AA-X ≈ 13.5, all significant.
        (IReadOnlyList<TukeyPair> pairs, double q) = Anova.TukeyHsd(r, 0.05);
        Assert.Equal(3, pairs.Count);
        Assert.Equal(-37.7, pairs.Single(p => p.A == "AA2024" && p.B == "AA7075").Difference, 9);
        Assert.Equal(-24.2, pairs.Single(p => p.A == "AA2024" && p.B == "AA-X").Difference, 9);
        Assert.Equal(13.5, pairs.Single(p => p.A == "AA7075" && p.B == "AA-X").Difference, 9);
        Assert.All(pairs, p => Assert.True(p.Significant));
        Assert.All(pairs, p => Assert.True(p.Lower < p.Difference && p.Difference < p.Upper));
        Assert.Equal(3.506, q, 0.01);   // q_{0.05; 3, 27} from the studentized range table (3.53 at ν = 24, 3.49 at 30)

        // Power for the observed effect: the course reports > 0.99.
        double f = Math.Sqrt(r.EtaSquared / (1 - r.EtaSquared));
        Assert.True(Anova.Power(3, 30, f, 0.05) > 0.99);
    }

    [Fact]
    public void The_studentized_range_and_noncentral_F_agree_with_known_values()
    {
        // With two groups the range is √2 |z|, so q_{0.95; 2, ∞} = √2 · z_{0.975} = 2.772.
        Assert.Equal(Math.Sqrt(2) * Normal.InvCDF(0, 1, 0.975), StudentizedRange.Quantile(0.95, 2, 1e6), 0.002);
        Assert.Equal(0.95, StudentizedRange.Cdf(StudentizedRange.Quantile(0.95, 4, 20), 4, 20), 1e-6);
        Assert.Equal(3.958, StudentizedRange.Quantile(0.95, 4, 20), 0.01);   // table value
        Assert.True(StudentizedRange.Cdf(1, 3, 10) < StudentizedRange.Cdf(2, 3, 10));

        // λ = 0 is the central F.
        Assert.Equal(FisherSnedecor.CDF(3, 20, 2.5), NoncentralF.Cdf(2.5, 3, 20, 0), 1e-9);
        Assert.True(NoncentralF.Cdf(2.5, 3, 20, 10) < FisherSnedecor.CDF(3, 20, 2.5));
        Assert.Equal(0.05, Anova.Power(3, 30, 0, 0.05), 1e-6);   // no effect: power = α
    }

    [Fact]
    public void Bartlett_and_Kruskal_Wallis_behave()
    {
        double[] a = [1, 2, 3, 4, 5], b = [2, 3, 4, 5, 6], c = [10, 20, 30, 40, 50];
        AssumptionCheck sameSpread = Anova.Bartlett([a, b]);
        Assert.Equal(0, sameSpread.Statistic, 9);
        Assert.Equal(1, sameSpread.PValue, 9);
        Assert.True(Anova.Bartlett([a, c]).PValue < 0.01);

        AssumptionCheck kw = Anova.KruskalWallis([a, b, c]);
        Assert.True(kw.Statistic > 0 && kw.PValue < 0.05);
        Assert.Equal(1, Anova.KruskalWallis([a, a]).PValue, 3);   // identical groups: H = 0
    }

    [Fact]
    public void The_two_way_engine_matches_a_hand_worked_balanced_case()
    {
        // Cells (A, B): (1,1) 10, 12; (1,2) 14, 16; (2,1) 20, 22; (2,2) 30, 32.
        double[] a = [1, 1, 1, 1, 2, 2, 2, 2], b = [1, 1, 2, 2, 1, 1, 2, 2], y = [10, 12, 14, 16, 20, 22, 30, 32];
        (TwoWayResult? r, string? problem) = Anova.TwoWay(a, b, y);
        Assert.Null(problem);
        Assert.NotNull(r);
        Assert.Equal(19.5, r.GrandMean, 9);
        Assert.Equal(338, r.SsA, 9);
        Assert.Equal(98, r.SsB, 9);
        Assert.Equal(18, r.SsAB, 9);
        Assert.Equal(8, r.SsError, 9);
        Assert.Equal(462, r.SsTotal, 9);
        Assert.Equal(169, r.FA, 9);
        Assert.Equal(49, r.FB, 9);
        Assert.Equal(9, r.FAB, 9);
        Assert.Equal(4, r.DfError);
        Assert.Equal(11, r.CellMeans[0, 0], 9);
        Assert.Equal(31, r.CellMeans[1, 1], 9);
        Assert.Equal(0, r.Residuals.Sum(), 9);

        // One observation per cell: additive model, error = interaction.
        (TwoWayResult? additive, _) = Anova.TwoWay([1, 1, 2, 2, 3, 3], [1, 2, 1, 2, 1, 2], [10, 14, 20, 30, 25, 33]);
        Assert.NotNull(additive);
        Assert.True(additive.Additive);
        Assert.Equal(2, additive.DfError);
        Assert.True(double.IsNaN(additive.FAB));

        // Unbalanced designs are refused with the cell named.
        (TwoWayResult? none, string? why) = Anova.TwoWay([1, 1, 1, 2, 2], [1, 1, 2, 1, 2], [1, 2, 3, 4, 5]);
        Assert.Null(none);
        Assert.Contains("not balanced", why, StringComparison.Ordinal);
    }

    [Fact]
    public void The_one_way_analysis_reads_columns_or_a_factor_and_writes_its_tables()
    {
        StatistleDocument project = AlloyProject(out DataTable alloys);
        var analysis = new OneWayAnova { Project = project, Table = alloys.Id.ToString(CultureInfo.InvariantCulture) };
        analysis.Recalculate();

        Assert.Null(analysis.Problem);
        Assert.Equal(3, analysis.K);
        Assert.Equal(30, analysis.N);
        Assert.Equal(48.1162, analysis.F, 0.0005);
        Assert.StartsWith("Reject H₀", analysis.Decision, StringComparison.Ordinal);
        Assert.Equal(3, analysis.TukeySignificant);
        Assert.Contains("AA2024: n = 10", analysis.GroupTable, StringComparison.Ordinal);
        Assert.Contains("AA2024 − AA7075", analysis.TukeyTable, StringComparison.Ordinal);
        Assert.True(analysis.LeveneP > 0.05 && analysis.BartlettP > 0.05);
        Assert.True(analysis.KruskalP < 0.001);
        Assert.Equal(3, analysis.Figures(null).Count);
        Assert.True(analysis.Power > 0.99);
        double ssBetween = analysis.SsBetween;

        analysis.Groups = "AA2024, AA7075";
        analysis.Recalculate();
        Assert.Equal(2, analysis.K);
        Assert.Single(analysis.Pairs);

        analysis.Groups = "AA2024, nothing";
        analysis.Recalculate();
        Assert.Contains("nothing", analysis.Problem, StringComparison.Ordinal);

        // The same data stacked as a factor column and a response column gives the same F.
        DataTable stacked = project.AddTable("Stacked");
        stacked.Columns.AddColumn("alloy", [.. Enumerable.Range(0, 30).Select(i => (double)(i / 10 + 1))]);
        stacked.Columns.AddColumn("strength", [.. alloys.Columns.Columns.SelectMany(c => c.Values)]);
        var byFactor = new OneWayAnova
        {
            Project = project, Table = stacked.Id.ToString(CultureInfo.InvariantCulture), Layout = GroupLayout.FactorAndResponse,
            FactorColumn = "alloy", ResponseColumn = "strength",
        };
        byFactor.Recalculate();
        Assert.Null(byFactor.Problem);
        Assert.Equal(ssBetween, byFactor.SsBetween, 9);
        Assert.Contains("alloy = 1", byFactor.GroupTable, StringComparison.Ordinal);
        Assert.Contains(MethodScope.HiddenProperties(byFactor), p => p.XmlName == nameof(OneWayAnova.Groups));
        Assert.Contains(MethodScope.HiddenProperties(analysis), p => p.XmlName == nameof(OneWayAnova.FactorColumn));

        Assert.Equal(6, MethodCatalog.For(typeof(OneWayAnova)).Count);
        Assert.All(MethodCatalog.For(typeof(OneWayAnova)), m => Assert.Empty(MethodContract.Verify(m)));
        Assert.NotNull(new OneWayAnova().Problem);
    }

    [Fact]
    public void The_two_way_analysis_reports_the_table_and_the_decisions()
    {
        StatistleDocument project = BatteryProject(out DataTable battery);
        var analysis = new TwoWayAnova
        {
            Project = project, Table = battery.Id.ToString(CultureInfo.InvariantCulture),
            FactorA = "temperature", FactorB = "material", Response = "life",
        };
        analysis.Recalculate();

        Assert.Null(analysis.Problem);
        Assert.Equal(3, analysis.LevelsA);
        Assert.Equal(2, analysis.LevelsB);
        Assert.Equal(3, analysis.Replicates);
        Assert.Equal(18, analysis.N);
        Assert.Equal(12, analysis.DfError);
        Assert.Equal(analysis.SsTotal, analysis.SsA + analysis.SsB + analysis.SsAB + analysis.SsError, 6);
        Assert.True(analysis.PA < 0.05);
        Assert.StartsWith("Reject H₀", analysis.DecisionA, StringComparison.Ordinal);
        Assert.Contains("temperature = 1", analysis.CellTable, StringComparison.Ordinal);
        Assert.Equal(2, analysis.Figures(null).Count);

        analysis.Response = "temperature";
        analysis.Recalculate();
        Assert.Contains("three different columns", analysis.Problem, StringComparison.Ordinal);

        Assert.Equal(2, MethodCatalog.For(typeof(TwoWayAnova)).Count);
        Assert.All(MethodCatalog.For(typeof(TwoWayAnova)), m => Assert.Empty(MethodContract.Verify(m)));
        Assert.NotNull(new TwoWayAnova().Problem);
    }
}
