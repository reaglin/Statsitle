using Statistle.Learning;
using Xunit.Abstractions;

namespace Statistle.Learning.Tests;

/// <summary>Plan 10.6: the shipped package passes every check, the Learning Core's and the statistics ones.</summary>
public class ContentTests(ITestOutputHelper output)
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

        // Plan 10.14: warnings are for the author to look at; they are printed and do not fail the package.
        var warnings = content.Warnings();
        output.WriteLine(warnings.Count == 0 ? "No warnings." : "Warnings:\n" + string.Join("\n", warnings));
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

    [Fact]
    public void Content_every_mistake_of_the_pilot_is_recognized()
    {
        // Plan 10.13: typing the number a mistake gives shows its sentence, for every mistake in the package.
        var content = StatsContent.Load(Egn3443);
        var count = 0;
        foreach (var task in content.Tasks.Values.Where(t => t.IsExercise))
            foreach (var e in StatsTaskRunner.Build(task).Expected)
                foreach (var m in e.Mistakes)
                {
                    var typed = m.Value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
                    Assert.Equal($"{e.Label}: {typed} is not right. {m.Says}", StatsTaskRunner.Check(e, typed));
                    count++;
                }
        Assert.True(count >= 4, "The pilot's exercises should recognize the common mistakes (sample vs population variance, mean vs median).");
    }

    [Fact]
    public void Content_a_widget_in_a_question_is_checked_like_one_in_a_Learn_page()
    {
        // Plan 10.15: the pilot's skew question shows a histogram; a widget naming a missing task is a problem.
        var shipped = StatsContent.Load(Egn3443);
        Assert.Contains(shipped.Package.AllItems.Where(i => i.Quiz is not null).SelectMany(i => i.Quiz!.Questions),
            q => q.Prompt.Contains("[[figure:", StringComparison.Ordinal));

        var copy = Path.Combine(Path.GetTempPath(), "statistle-content-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopyFolder(Egn3443, copy);
            var quiz = Path.Combine(copy, "quizzes", "m2-distribution.json");
            var text = File.ReadAllText(quiz)
                .Replace("[[figure: st.m2.selecting.eg 1]]", "[[figure: st.m2.no-such-task 1]]", StringComparison.Ordinal)
                .Replace("so it is positively skewed.\"", "so it is positively skewed.\\n[[figure: st.m2.selecting.eg 9]]\"", StringComparison.Ordinal);
            File.WriteAllText(quiz, text);
            var problems = StatsContent.Load(copy).Validate();
            Assert.Contains(problems, p => p.Contains("m2-distribution.json, question q9 (prompt)", StringComparison.Ordinal) && p.Contains("st.m2.no-such-task", StringComparison.Ordinal));
            Assert.Contains(problems, p => p.Contains("question q9 (explanation)", StringComparison.Ordinal) && p.Contains("must be 1 to", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(copy)) Directory.Delete(copy, recursive: true);
        }
    }

    private static void CopyFolder(string from, string to)
    {
        foreach (var dir in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(from, to, StringComparison.Ordinal));
        Directory.CreateDirectory(to);
        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(from, to, StringComparison.Ordinal));
    }
}
