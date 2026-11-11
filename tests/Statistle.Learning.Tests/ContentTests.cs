using Statistle.Learning;

namespace Statistle.Learning.Tests;

/// <summary>Plan 10.6: the shipped package passes every check, the Learning Core's and the statistics ones.</summary>
public class ContentTests
{
    public static string Root()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return dir ?? throw new InvalidOperationException("Statistle.sln not found above the test folder.");
    }

    public static string Egn3443 => Path.Combine(Root(), "content", "egn3443");

    [Fact]
    public void Content_package_egn3443_has_no_problems()
    {
        var content = StatsContent.Load(Egn3443);
        var problems = content.Validate();
        Assert.True(problems.Count == 0, "The content has problems:\n" + string.Join("\n", problems));
    }

    [Fact]
    public void Content_every_exercise_passes_with_its_own_answers_and_fails_with_none()
    {
        var content = StatsContent.Load(Egn3443);
        var runner = new StatsTaskRunner();
        foreach (var task in content.Tasks.Values.Where(t => t.IsExercise))
        {
            var built = StatsTaskRunner.Build(task);
            var right = built.Expected.ToDictionary(e => e.Asked.Result, e => e.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            Assert.True(runner.Run(task.Json, StatsTaskRunner.Work(right)).Passed, $"{task.Id} fails with its own answers.");
            Assert.False(runner.Run(task.Json, "{}").Passed, $"{task.Id} passes with no answers.");
        }
    }
}
