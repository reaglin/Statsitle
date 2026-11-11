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
