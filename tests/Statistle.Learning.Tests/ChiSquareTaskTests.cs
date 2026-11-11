using Statistle.Core.Analyses;
using Statistle.Learning;

namespace Statistle.Learning.Tests;

/// <summary>Plan 10.8: the ways a task can set up the Chi-square tests analysis (docs/CONTENT-FORMAT.md).</summary>
public class ChiSquareTaskTests
{
    [Fact]
    public void A_goodness_of_fit_task_types_the_counts_and_proportions()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse("""
            {
              "id": "test.chi.fit", "title": "Defects per board", "analysis": "ChiSquareTest",
              "settings": { "Test": "GoodnessOfFit", "Observed": [32, 15, 13], "Proportions": "Entered",
                            "Expected": [0.472, 0.354, 0.174], "EstimatedParameters": 1, "CategoryNames": "0, 1, 2 or more" },
              "ask": [ "Statistic", "DegreesOfFreedom", "Critical", "P", "SmallestExpected", "Total", "Categories", "CellsBelowFive" ]
            }
            """));
        Assert.Equal(2.9392, built.Expected[0].Value, 4);
        Assert.Equal(1, built.Expected[1].Value);
        Assert.Equal(0.0865, built.Expected[3].Value, 4);
        Assert.Null(StatsTaskRunner.Check(built.Expected[0], "2.94"));
    }

    [Fact]
    public void A_contingency_task_types_the_table()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse("""
            {
              "id": "test.chi.typed", "title": "Pension plans", "analysis": "ChiSquareTest",
              "settings": { "Test": "Independence", "Counts": "Plan 1\tPlan 2\tPlan 3\n160\t140\t40\n40\t60\t60", "RowNames": "Salaried, Hourly" },
              "ask": [ "Statistic", "DegreesOfFreedom", "CramersV", "Rows", "Columns" ]
            }
            """));
        Assert.Equal(49.6324, built.Expected[0].Value, 4);
        Assert.Equal(2, built.Expected[1].Value);
        Assert.Equal(3, built.Expected[4].Value);
    }

    [Fact]
    public void A_contingency_task_brings_a_data_table_and_points_at_it()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse("""
            {
              "id": "test.chi.table", "title": "Pension plans", "analysis": "ChiSquareTest",
              "tables": [ { "name": "Pension", "columns": { "Plan 1": [160, 40], "Plan 2": [140, 60], "Plan 3": [40, 60] } } ],
              "settings": { "Test": "Independence", "CountsTable": "@Pension/", "Correction": "None", "Alpha": 0.01 },
              "ask": [ "Statistic", "Critical" ]
            }
            """));
        Assert.Equal(49.6324, built.Expected[0].Value, 4);
        Assert.Equal(9.2103, built.Expected[1].Value, 4);
        Assert.Null(((ChiSquareTest)built.Analysis).Problem);
    }

    [Fact]
    public void A_goodness_of_fit_task_reads_its_counts_from_a_column()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse("""
            {
              "id": "test.chi.column", "title": "Die", "analysis": "ChiSquareTest",
              "tables": [ { "name": "Rolls", "columns": { "Face": [1, 2, 3, 4, 5, 6], "Count": [8, 12, 9, 11, 6, 14] } } ],
              "settings": { "ObservedColumn": "@Rolls/Count", "Proportions": "Equal" },
              "ask": [ "Statistic" ]
            }
            """));
        Assert.Equal(4.2, Assert.Single(built.Expected).Value, 12);
    }
}
