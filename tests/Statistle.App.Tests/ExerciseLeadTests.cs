using System.IO;
using Statistle.App.Learning;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>The exercise's lead says how to reach the Workbench, unless the task already says so (UX sweep, plan 15.1).</summary>
public sealed class ExerciseLeadTests
{
    private static readonly StatsContent Content = StatsContent.Load(Egn3443());

    private static string Egn3443()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new InvalidOperationException("Statistle.sln not found"), "content", "egn3443");
    }

    [Fact]
    public void A_task_that_gives_its_own_Workbench_steps_is_not_given_them_twice()
    {
        var own = Content.Tasks.Values.First(t => t.Intro.Contains("To use the Workbench", StringComparison.OrdinalIgnoreCase));
        var lead = TaskWorkspace.ExerciseLead(StatsTaskRunner.Build(own));
        Assert.Contains("The task says how", lead, StringComparison.Ordinal);
        Assert.DoesNotContain("To use it:", lead, StringComparison.Ordinal);

        var other = Content.Tasks.Values.First(t => !t.Intro.Contains("To use the Workbench", StringComparison.OrdinalIgnoreCase)
                                                    && StatsTaskRunner.Build(t).Expected.Count > 0);
        Assert.Contains("To use it:", TaskWorkspace.ExerciseLead(StatsTaskRunner.Build(other)), StringComparison.Ordinal);
    }
}
