using System.Globalization;
using System.Xml.Linq;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;

namespace Statistle.Tests;

/// <summary>Plan 10.8: Pearson's chi-square tests of goodness of fit and independence (M&amp;R §9-7, §9-8).</summary>
public class ChiSquareTests
{
    /// <summary>Montgomery and Runger Example 9-14: pension plan preferred (columns) by job type (rows), 500 employees.</summary>
    public static ChiSquareTest Pension()
    {
        var test = new ChiSquareTest { Name = "Pension plans", Test = ChiSquareKind.Independence, RowNames = "Salaried, Hourly" };
        test.Counts.SetColumns([new DataColumn("Plan 1", [160, 40]), new DataColumn("Plan 2", [140, 60]), new DataColumn("Plan 3", [40, 60])]);
        test.Recalculate();
        return test;
    }

    /// <summary>
    /// Montgomery and Runger Example 9-12: defects on 60 printed circuit boards against a Poisson with λ = 0.75
    /// estimated from the data (m = 1), the last two classes combined: O = 32, 15, 13; p = 0.472, 0.354, 0.174.
    /// </summary>
    public static ChiSquareTest CircuitBoards()
    {
        var test = new ChiSquareTest { Name = "Defects per board", Proportions = ExpectedProportions.Entered, EstimatedParameters = 1, CategoryNames = "0, 1, 2 or more" };
        Fill(test.Observed, 32, 15, 13);
        Fill(test.Expected, 0.472, 0.354, 0.174);
        test.Recalculate();
        return test;
    }

    private static void Fill(DataSeries series, params double[] values) => series.SetValues(values, 1);

    [Fact]
    public void Goodness_of_fit_matches_Montgomery_and_Runger_example_9_12()
    {
        ChiSquareTest test = CircuitBoards();

        Assert.Null(test.Problem);
        Assert.Equal(3, test.Categories);
        Assert.Equal(60, test.Total);
        Assert.Equal([28.32, 21.24, 10.44], test.ExpectedCounts.Select(e => Math.Round(e, 2)));
        Assert.Equal(2.94, test.Statistic, 2);                          // the book's χ²₀ = 2.94
        Assert.Equal(1, test.DegreesOfFreedom);                         // k − p − 1 = 3 − 1 − 1
        Assert.Equal(3.8415, test.Critical, 4);                         // χ²_{0.05,1}
        Assert.Equal(0.0865, test.P, 3);                                // 0.05 < p < 0.10, as the book says
        Assert.StartsWith("Do not reject H₀", test.Decision, StringComparison.Ordinal);
        Assert.Equal(10.44, test.SmallestExpected, 4);
        Assert.Equal(0, test.CellsBelowFive);
        Assert.StartsWith("Every expected count is at least 5", test.ExpectedCheck, StringComparison.Ordinal);
        Assert.Contains("2 or more: O = 13, E = 10.4400, (O − E)²/E = 0.6277", test.ExpectedTable, StringComparison.Ordinal);
    }

    [Fact]
    public void Independence_matches_Montgomery_and_Runger_example_9_14()
    {
        ChiSquareTest test = Pension();

        Assert.Null(test.Problem);
        Assert.Equal(2, test.Rows);
        Assert.Equal(3, test.Columns);
        Assert.Equal(500, test.Total);
        Assert.Equal([136, 136, 68, 64, 64, 32], test.ExpectedCounts.Select(e => Math.Round(e, 9)));
        Assert.Equal(49.63, test.Statistic, 2);                         // the book's χ²₀ = 49.63
        Assert.Equal(2, test.DegreesOfFreedom);
        Assert.Equal(5.9915, test.Critical, 4);                         // χ²_{0.05,2}
        Assert.Equal(Math.Exp(-test.Statistic / 2), test.P, 15);        // χ²(2) upper tail is e^(−x/2)
        Assert.True(test.P < 1e-10);
        Assert.StartsWith("Reject H₀", test.Decision, StringComparison.Ordinal);
        Assert.Equal(Math.Sqrt(test.Statistic / 500), test.CramersV, 12);
        Assert.Contains("Salaried: 136.00, 136.00, 68.00", test.ExpectedTable, StringComparison.Ordinal);
        Assert.Contains("Hourly: 64.00, 64.00, 32.00", test.ExpectedTable, StringComparison.Ordinal);
    }

    [Fact]
    public void Equal_proportions_give_the_hand_calculation_for_a_die()
    {
        var test = new ChiSquareTest();   // opens with 60 rolls: 8, 12, 9, 11, 6, 14

        Assert.Null(test.Problem);
        Assert.Equal(6, test.Categories);
        Assert.All(test.ExpectedCounts, e => Assert.Equal(10, e, 12));
        Assert.Equal(4.2, test.Statistic, 12);                          // (4 + 4 + 1 + 1 + 16 + 16)/10
        Assert.Equal(5, test.DegreesOfFreedom);
        Assert.StartsWith("H₀: all 6 categories are equally likely", test.Hypotheses, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Invariants_hold(bool independence)
    {
        ChiSquareTest test = independence ? Pension() : CircuitBoards();

        Assert.Equal(test.ObservedCounts.Sum(), test.ExpectedCounts.Sum(), 9);   // Σ E = Σ O = n
        Assert.Equal(test.Total, test.ObservedCounts.Sum(), 12);
        Assert.Equal(test.DegreesOfFreedom, independence ? (test.Rows - 1) * (test.Columns - 1) : test.Categories - 1 - test.EstimatedParameters);
        double byHand = test.ObservedCounts.Select((o, i) => (o - test.ExpectedCounts[i]) * (o - test.ExpectedCounts[i]) / test.ExpectedCounts[i]).Sum();
        Assert.Equal(byHand, test.Statistic, 9);
        Assert.Equal(test.P < test.Alpha, test.Statistic > test.Critical);
        Assert.Equal(test.ExpectedCounts.Min(), test.SmallestExpected, 12);
    }

    [Fact]
    public void Counts_exactly_as_expected_give_zero_and_p_one()
    {
        var test = new ChiSquareTest { Test = ChiSquareKind.Independence };
        test.Counts.SetColumns([new DataColumn("A", [10, 20]), new DataColumn("B", [30, 60])]);   // rows in the same 1 : 3 ratio
        test.Recalculate();

        Assert.Equal(0, test.Statistic, 12);
        Assert.Equal(1, test.P, 12);
        Assert.Equal(0, test.CramersV, 12);
    }

    [Fact]
    public void Yates_correction_on_a_2_by_2_table_matches_the_shortcut_formula()
    {
        var test = new ChiSquareTest { Test = ChiSquareKind.Independence };
        test.Counts.SetColumns([new DataColumn("Pass", [12, 3]), new DataColumn("Fail", [5, 10])]);
        test.Recalculate();
        double n = 30, cross = 12 * 10 - 5 * 3, margins = 17.0 * 13 * 15 * 15;

        Assert.Equal(n * cross * cross / margins, test.Statistic, 10);               // no correction by default
        Assert.Contains("No continuity correction", test.Notes, StringComparison.Ordinal);

        test.Correction = ContinuityCorrection.Yates;
        test.Recalculate();
        Assert.Equal(n * Math.Pow(Math.Abs(cross) - n / 2, 2) / margins, test.Statistic, 10);
        Assert.Contains("Yates' continuity correction was applied", test.Notes, StringComparison.Ordinal);
        Assert.Equal(Math.Sqrt(n * cross * cross / margins / n), test.CramersV, 10);  // V from the uncorrected χ²

        ChiSquareTest larger = Pension();
        double plain = larger.Statistic;
        larger.Correction = ContinuityCorrection.Yates;
        larger.Recalculate();
        Assert.Equal(plain, larger.Statistic, 12);
        Assert.Contains("only to a 2 × 2 table", larger.Notes, StringComparison.Ordinal);
    }

    [Fact]
    public void Small_expected_counts_are_flagged_not_refused()
    {
        var test = new ChiSquareTest();
        Fill(test.Observed, 2, 3, 1);
        test.Recalculate();

        Assert.Null(test.Problem);
        Assert.Equal(3, test.CellsBelowFive);
        Assert.Equal(2, test.SmallestExpected, 12);
        Assert.StartsWith("Warning — 3 of 3 expected counts are below 5", test.ExpectedCheck, StringComparison.Ordinal);
    }

    [Fact]
    public void Weights_that_do_not_add_to_one_are_rescaled_and_said_so()
    {
        var test = new ChiSquareTest { Proportions = ExpectedProportions.Entered };
        Fill(test.Observed, 30, 50, 20);
        Fill(test.Expected, 1, 2, 1);
        test.Recalculate();

        Assert.Equal([0.25, 0.5, 0.25], test.ProportionsUsed);
        Assert.Equal([25.0, 50, 25], test.ExpectedCounts);
        Assert.Contains("read as weights", test.Notes, StringComparison.Ordinal);
    }

    [Fact]
    public void Bad_entries_explain_themselves_and_leave_the_results_blank()
    {
        var mismatch = new ChiSquareTest { Proportions = ExpectedProportions.Entered };
        Fill(mismatch.Observed, 10, 20, 30);
        Fill(mismatch.Expected, 0.5, 0.5);
        mismatch.Recalculate();
        Assert.Contains("3 observed counts and 2 expected proportions", mismatch.Problem, StringComparison.Ordinal);
        Assert.True(double.IsNaN(mismatch.Statistic));
        Assert.True(double.IsNaN(mismatch.P));

        var negative = new ChiSquareTest();
        Fill(negative.Observed, 10, -2);
        negative.Recalculate();
        Assert.Contains("cannot be negative", negative.Problem, StringComparison.Ordinal);

        var noDf = new ChiSquareTest { EstimatedParameters = 1 };
        Fill(noDf.Observed, 10, 12);
        noDf.Recalculate();
        Assert.Contains("degrees of freedom", noDf.Problem, StringComparison.Ordinal);

        var ragged = new ChiSquareTest { Test = ChiSquareKind.Independence };
        ragged.Counts.SetColumns([new DataColumn("A", [1, 2, 3]), new DataColumn("B", [4, 5])]);
        ragged.Recalculate();
        Assert.Contains("Every cell needs a count", ragged.Problem, StringComparison.Ordinal);

        var emptyRow = new ChiSquareTest { Test = ChiSquareKind.Independence };
        emptyRow.Counts.SetColumns([new DataColumn("A", [1, 0]), new DataColumn("B", [4, 0])]);
        emptyRow.Recalculate();
        Assert.Contains("Row 'Row 2' has no counts", emptyRow.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Counts_come_from_project_tables_and_are_locked()
    {
        var project = new StatistleDocument();
        DataTable pension = project.AddTable("Pension");
        pension.Columns.AddColumn("Plan 1", [160, 40]);
        pension.Columns.AddColumn("Plan 2", [140, 60]);
        pension.Columns.AddColumn("Plan 3", [40, 60]);
        pension.Columns.AddColumn("Notes", [1, 2]);

        // A task writes "@Pension/" — a column reference with the table's id — and the analysis keeps the table.
        var test = new ChiSquareTest { Project = project, Test = ChiSquareKind.Independence, CountsTable = DataSources.Reference(pension.Id, string.Empty), CountsColumns = "Plan 1, Plan 2, Plan 3" };
        test.Recalculate();
        Assert.Equal(pension.Id.ToString(CultureInfo.InvariantCulture), test.CountsTable);
        Assert.Null(test.Problem);
        Assert.Equal(Pension().Statistic, test.Statistic, 12);
        Assert.Equal(["Plan 1", "Plan 2", "Plan 3"], test.Counts.Names);
        Assert.True(MethodScope.IsLocked(test, nameof(ChiSquareTest.Counts)));
        Assert.Contains(test.ChoicesFor(nameof(ChiSquareTest.CountsTable)), c => c.Label == "Pension");

        test.CountsColumns = "Plan 9";
        test.Recalculate();
        Assert.Contains("no column named 'Plan 9'", test.Problem, StringComparison.Ordinal);

        DataTable boards = project.AddTable("Boards");
        boards.Columns.AddColumn("defects", [32, 15, 13]);
        boards.Columns.AddColumn("p", [0.472, 0.354, 0.174]);
        var fit = new ChiSquareTest
        {
            Project = project,
            ObservedColumn = DataSources.Reference(boards.Id, "defects"),
            Proportions = ExpectedProportions.Entered,
            ExpectedColumn = DataSources.Reference(boards.Id, "p"),
            EstimatedParameters = 1,
        };
        fit.Recalculate();
        Assert.Null(fit.Problem);
        Assert.Equal(CircuitBoards().Statistic, fit.Statistic, 12);
        Assert.True(MethodScope.IsLocked(fit, nameof(ChiSquareTest.Observed)));
        Assert.True(MethodScope.IsLocked(fit, nameof(ChiSquareTest.Expected)));
    }

    [Fact]
    public void The_form_shows_only_the_chosen_tests_entries_and_the_methods_are_sound()
    {
        ChiSquareTest fit = CircuitBoards();
        ChiSquareTest table = Pension();
        IReadOnlyList<string> Hidden(ChiSquareTest t) => [.. MethodScope.HiddenProperties(t).Select(p => p.XmlName)];

        Assert.Contains(nameof(ChiSquareTest.Counts), Hidden(fit));
        Assert.Contains(nameof(ChiSquareTest.CramersV), Hidden(fit));
        Assert.Contains(nameof(ChiSquareTest.Correction), Hidden(fit));
        Assert.DoesNotContain(nameof(ChiSquareTest.Expected), Hidden(fit));
        Assert.Contains(nameof(ChiSquareTest.Observed), Hidden(table));
        Assert.Contains(nameof(ChiSquareTest.Categories), Hidden(table));
        Assert.Contains(nameof(ChiSquareTest.CountsColumns), Hidden(table));   // only with a data table chosen
        Assert.Contains(nameof(ChiSquareTest.Expected), Hidden(new ChiSquareTest()));   // equal proportions
        Assert.All(MethodCatalog.For(typeof(ChiSquareTest)), m => Assert.Empty(MethodContract.Verify(m)));
        Assert.Contains(AnalysisCatalog.Discover(typeof(Analysis).Assembly), d => d.ToolType == typeof(ChiSquareTest));
    }

    [Fact]
    public void The_figures_are_the_reference_distribution_then_observed_against_expected()
    {
        foreach (ChiSquareTest test in new[] { CircuitBoards(), Pension() })
        {
            IReadOnlyList<PlotSpec> figures = test.Figures(null);
            Assert.Equal(2, figures.Count);
            Assert.Contains(figures[0].Series, s => s.Name == "Rejection region");
            Assert.Contains(figures[0].Series, s => s is LineSeries { Name: "Observed χ²" } line && line.X[0] == test.Statistic);
            Assert.All(((LineSeries)figures[0].Series.First(s => s.Name == "Density under H₀")).Y, y => Assert.True(double.IsFinite(y)));
            var bars = figures[1].Series.OfType<BarSeries>().ToList();
            Assert.Equal(2, bars.Count);
            Assert.Equal(test.ObservedCounts, bars[0].Y);
            Assert.Equal(test.ExpectedCounts, bars[1].Y);
        }

        var blank = new ChiSquareTest { Alpha = 2 };
        blank.Recalculate();
        Assert.NotNull(blank.Problem);
        Assert.Empty(blank.Figures(null));
    }

    [Fact]
    public void Every_setting_and_the_contingency_table_round_trip()
    {
        ChiSquareTest original = Pension();
        original.Correction = ContinuityCorrection.Yates;
        original.Alpha = 0.01;
        Fill(original.Observed, 4, 5, 6);
        XElement written = StatistleXml.Write(original, "artifact");

        var reloaded = new ChiSquareTest();
        var diagnostics = new LoadDiagnostics();
        StatistleXml.Read(reloaded, written, diagnostics);
        reloaded.Recalculate();

        Assert.False(diagnostics.HasWarnings, diagnostics.ToString());
        Assert.Equal(original.Counts.Save(), reloaded.Counts.Save());
        Assert.Equal([4.0, 5, 6], reloaded.Observed.YValues);
        Assert.Equal(ChiSquareKind.Independence, reloaded.Test);
        Assert.Equal(ContinuityCorrection.Yates, reloaded.Correction);
        Assert.Equal("Salaried, Hourly", reloaded.RowNames);
        Assert.Equal(original.Statistic, reloaded.Statistic, 12);
    }

    [Fact]
    public void The_upper_tail_is_accurate_far_out()
    {
        Assert.Equal(Math.Exp(-50), ChiSquare.UpperTail(2, 100), 1e-30);
        Assert.Equal(0.05, ChiSquare.UpperTail(1, 3.841458820694124), 12);
        Assert.Equal(1, ChiSquare.UpperTail(3, 0));
    }
}
