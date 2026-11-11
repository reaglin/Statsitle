using Learning.Core.Content;
using Statistle.Core.Analyses;
using Statistle.Learning;

namespace Statistle.Learning.Tests;

/// <summary>Plan 10.2: an exercise is graded against what the analysis itself calculates.</summary>
public class TaskRunnerTests
{
    // Mean 5.0, median 4.5, sample SD 2.0 (values chosen so the arithmetic is checkable by hand).
    private const string Bolts = """
        {
          "id": "test.bolts",
          "title": "Bolt lengths",
          "analysis": "DescriptiveStatistics",
          "settings": { "Sample": [3, 4, 4, 5, 9] },
          "ask": [ "Mean", { "result": "Median", "prompt": "The median length (mm)" }, "Count" ]
        }
        """;

    private static readonly StatsTaskRunner Runner = new();

    private static string Answers(string mean, string median, string count) =>
        StatsTaskRunner.Work(new Dictionary<string, string> { ["Mean"] = mean, ["Median"] = median, ["Count"] = count });

    [Fact]
    public void The_answers_come_from_the_analysis()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse(Bolts));
        Assert.IsType<DescriptiveStatistics>(built.Analysis);
        Assert.Equal(5.0, built.Expected[0].Value, 12);
        Assert.Equal(4.0, built.Expected[1].Value, 12);
        Assert.Equal(5, built.Expected[2].Value);
        Assert.Equal("The median length (mm)", built.Expected[1].Label);
        Assert.Equal("Mean", built.Expected[0].Label);
    }

    [Fact]
    public void A_null_setting_leaves_a_number_blank()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse("""
            { "id": "test.null", "title": "Blank x2", "analysis": "ContinuousDistribution", "settings": { "X2": null } }
            """));
        Assert.True(double.IsNaN(((ContinuousDistribution)built.Analysis).X2));
    }

    [Fact]
    public void Right_answers_pass()
    {
        var outcome = Runner.Run(Bolts, Answers("5", "4", "5"));
        Assert.True(outcome.Ran);
        Assert.True(outcome.Passed);
        Assert.Equal(3, outcome.Score);
        Assert.Equal("test.bolts", outcome.ChallengeId);
    }

    [Fact]
    public void A_sensibly_rounded_answer_passes_and_a_count_must_be_exact()
    {
        Assert.True(Runner.Run(Bolts, Answers("5.004", "4.0", "5")).Passed);
        var outcome = Runner.Run(Bolts, Answers("5", "4", "6"));
        Assert.False(outcome.Passed);
        Assert.Equal(2, outcome.Score);
    }

    [Fact]
    public void A_wrong_answer_says_which_result_and_which_way_but_not_the_answer()
    {
        var outcome = Runner.Run(Bolts, Answers("5.6", "4", "5"));
        var failed = Assert.Single(outcome.Scenarios, s => !s.Passed);
        Assert.StartsWith("Mean: 5.6 is too high.", failed.Failure);
        Assert.DoesNotContain("5.0", failed.Failure);
    }

    [Fact]
    public void Missing_and_unreadable_answers_are_explained()
    {
        var outcome = Runner.Run(Bolts, StatsTaskRunner.Work(new Dictionary<string, string> { ["Mean"] = "five" }));
        Assert.Contains(outcome.Scenarios, s => s.Failure == "Mean: \"five\" is not a number. Type digits, with a point for decimals, such as 4.87.");
        Assert.Contains(outcome.Scenarios, s => s.Failure == "The median length (mm): no answer yet.");
    }

    [Fact]
    public void A_pass_threshold_below_one_allows_a_mistake()
    {
        var lenient = Bolts.Replace("\"title\": \"Bolt lengths\",", "\"title\": \"Bolt lengths\", \"passThreshold\": 0.6,", StringComparison.Ordinal);
        Assert.True(Runner.Run(lenient, Answers("5", "4", "7")).Passed);
    }

    [Fact]
    public void A_task_that_names_what_the_analysis_does_not_have_is_refused_with_a_sentence()
    {
        var noAnalysis = Assert.Throws<ContentException>(() => StatsTaskRunner.Build(StatsTask.Parse(Bolts.Replace("DescriptiveStatistics", "Descriptive", StringComparison.Ordinal))));
        Assert.Contains("there is no analysis called \"Descriptive\"", noAnalysis.Message);

        var noSetting = Assert.Throws<ContentException>(() => StatsTaskRunner.Build(StatsTask.Parse(Bolts.Replace("\"Sample\"", "\"Samples\"", StringComparison.Ordinal))));
        Assert.Contains("has no setting \"Samples\"", noSetting.Message);

        var notResult = Assert.Throws<ContentException>(() => StatsTaskRunner.Build(StatsTask.Parse(Bolts.Replace("\"Mean\"", "\"Average\"", StringComparison.Ordinal))));
        Assert.Contains("\"Average\" is not a result of Descriptive statistics", notResult.Message);

        var words = Assert.Throws<ContentException>(() => StatsTaskRunner.Build(StatsTask.Parse(Bolts.Replace("\"Mean\"", "\"Mode\"", StringComparison.Ordinal))));
        Assert.Contains("is not a number", words.Message);
    }

    [Fact]
    public void A_broken_task_does_not_run_and_says_why()
    {
        var outcome = Runner.Run("{ \"id\": \"x\" }", "{}");
        Assert.False(outcome.Ran);
        Assert.Contains("\"title\" is missing", Assert.Single(outcome.Problems));
    }

    [Fact]
    public void A_task_can_bring_a_data_table_and_point_an_analysis_at_its_column()
    {
        const string json = """
            {
              "id": "test.table",
              "title": "From a table",
              "analysis": "DescriptiveStatistics",
              "tables": [ { "name": "Bolts", "columns": { "Length": [3, 4, 4, 5, 9], "Other": [1, 2] } } ],
              "settings": { "Column": "@Bolts/Length" },
              "ask": [ "Mean" ]
            }
            """;
        var built = StatsTaskRunner.Build(StatsTask.Parse(json));
        Assert.NotNull(built.Project);
        Assert.Equal(5.0, Assert.Single(built.Expected).Value, 12);
    }

    // Plan 10.13. Sample 3, 4, 4, 5, 9: sample variance 5.5 (÷ 4), population variance 4.4 (÷ 5), sample SD 2.345.
    private const string Spread = """
        {
          "id": "test.spread",
          "title": "Spread",
          "analysis": "DescriptiveStatistics",
          "settings": { "Sample": [3, 4, 4, 5, 9] },
          "ask": [ "Variance", "CoefficientOfVariation" ],
          "mistakes": {
            "Variance": [
              { "result": "PopulationVariance", "says": "You divided by n; the sample variance divides by n − 1." },
              { "result": "StandardDeviation", "says": "That is the standard deviation. The variance is its square." },
              { "value": 22, "says": "That is the sum of the squares. Divide it by n − 1." }
            ],
            "CoefficientOfVariation": { "scale": 0.01, "says": "Give it as a percentage: multiply by 100." }
          }
        }
        """;

    private static string? Failure(string task, string result, string typed)
    {
        var outcome = Runner.Run(task, StatsTaskRunner.Work(new Dictionary<string, string> { [result] = typed }));
        var built = StatsTaskRunner.Build(StatsTask.Parse(task));
        var label = built.Expected.Single(e => e.Asked.Result == result).Label;
        return outcome.Scenarios.Single(s => s.Name == label).Failure;
    }

    [Fact]
    public void A_mistake_the_task_names_is_said_in_place_of_too_high_or_too_low()
    {
        Assert.Equal("Variance (s²): 4.4 is not right. You divided by n; the sample variance divides by n − 1.", Failure(Spread, "Variance", "4.4"));
        Assert.Equal("Variance (s²): 4.41 is not right. You divided by n; the sample variance divides by n − 1.", Failure(Spread, "Variance", "4.41"));
        Assert.Contains("That is the standard deviation", Failure(Spread, "Variance", "2.345"));
        Assert.Contains("sum of the squares", Failure(Spread, "Variance", "22"));
        Assert.Contains("multiply by 100", Failure(Spread, "CoefficientOfVariation", "0.469"));
        Assert.Contains("too low", Failure(Spread, "Variance", "3.9"));   // not near any mistake
        Assert.Null(Failure(Spread, "Variance", "5.5"));
    }

    [Fact]
    public void The_mistake_values_come_from_the_analysis()
    {
        var built = StatsTaskRunner.Build(StatsTask.Parse(Spread));
        var variance = built.Expected[0];
        Assert.Equal(4.4, variance.Mistakes[0].Value, 12);
        Assert.Equal(Math.Sqrt(5.5), variance.Mistakes[1].Value, 12);
        Assert.Equal(22, variance.Mistakes[2].Value);
        Assert.Equal(built.Expected[1].Value * 0.01, Assert.Single(built.Expected[1].Mistakes).Value, 12);
        Assert.Empty(StatsContent.MistakeProblems(built));
    }

    [Fact]
    public void A_mistake_that_names_what_the_analysis_or_the_task_does_not_have_is_refused()
    {
        var noResult = Assert.Throws<ContentException>(() => StatsTaskRunner.Build(StatsTask.Parse(Spread.Replace("\"PopulationVariance\"", "\"PopVariance\"", StringComparison.Ordinal))));
        Assert.Contains("names \"PopVariance\", which is not a number result of Descriptive statistics", noResult.Message);

        var notAsked = Assert.Throws<ContentException>(() => StatsTaskRunner.Build(StatsTask.Parse(Spread.Replace("\"CoefficientOfVariation\": {", "\"Mean\": {", StringComparison.Ordinal))));
        Assert.Contains("\"mistakes\" lists \"Mean\", which the task does not ask", notAsked.Message);

        var noSays = Assert.Throws<ContentException>(() => StatsTask.Parse(Spread.Replace("\"says\": \"Give it as a percentage: multiply by 100.\"", "\"x\": 1", StringComparison.Ordinal)));
        Assert.Contains("has no \"says\"", noSays.Message);

        var both = Assert.Throws<ContentException>(() => StatsTask.Parse(Spread.Replace("{ \"value\": 22,", "{ \"value\": 22, \"result\": \"Mean\",", StringComparison.Ordinal)));
        Assert.Contains("not both", both.Message);
    }

    [Fact]
    public void A_mistake_accepted_as_the_right_answer_is_a_problem()
    {
        // Population and sample variance of a large sample are within the 0.5 % tolerance of each other.
        var many = string.Join(", ", Enumerable.Range(0, 400).Select(i => (i % 7).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var task = Spread.Replace("[3, 4, 4, 5, 9]", $"[{many}]", StringComparison.Ordinal);
        var problem = Assert.Single(StatsContent.MistakeProblems(StatsTaskRunner.Build(StatsTask.Parse(task))));
        Assert.Contains("the learner would never read it", problem);
    }

    [Fact]
    public void Two_asked_results_within_each_others_tolerance_give_a_warning()
    {
        // Plan 10.14. Mean 3.002 and median 3: 0.002 apart, inside the default tolerance (0.015).
        const string close = """
            { "id": "test.close", "title": "Close", "analysis": "DescriptiveStatistics",
              "settings": { "Sample": [1, 2, 3, 4, 5.01] }, "ask": [ "Mean", "Median", "Range" ] }
            """;
        var warning = Assert.Single(StatsContent.OverlapWarnings(StatsTaskRunner.Build(StatsTask.Parse(close))));
        Assert.Contains("Mean", warning);
        Assert.Contains("Median", warning);
        Assert.Contains("types one for the other passes", warning);

        // A tighter tolerance clears it.
        var tight = close.Replace("\"Mean\", \"Median\"", "{ \"result\": \"Mean\", \"tolerance\": 0.001 }, { \"result\": \"Median\", \"tolerance\": 0.001 }", StringComparison.Ordinal);
        Assert.Empty(StatsContent.OverlapWarnings(StatsTaskRunner.Build(StatsTask.Parse(tight))));
    }

    [Fact]
    public void Every_analysis_can_be_built_from_a_task_with_its_defaults()
    {
        // Each analysis opens on a worked example, so a task with no settings must build and calculate.
        foreach (var name in StatsTaskRunner.Analyses.Keys)
        {
            var built = StatsTaskRunner.Build(StatsTask.Parse($$"""{ "id": "t.{{name}}", "title": "{{name}}", "analysis": "{{name}}" }"""));
            Assert.Empty(built.Expected);
        }
    }
}
