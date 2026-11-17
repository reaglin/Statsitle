using System.Globalization;
using System.Text;
using Learning.Core.Content;
using Statistle.Core.Analyses;
using Statistle.Learning;

namespace Statistle.Learning.Tests;

/// <summary>
/// Learning Core plan 3.22, the pattern for every PlusBook: the examples to try go on the Example tab, each with a figure
/// (<see cref="ExamplePages"/>), and Statistics+'s own example to try, <c>[[try: …]]</c> (<see cref="TrySpec"/>).
/// </summary>
public class ExampleTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void Split_keeps_the_learn_page_and_cuts_the_examples_at_each_title()
    {
        var (learn, pages) = ExamplePages.Split("# Lesson\n\nText.\n\n[[see-example: 1]]\n\n[[example: First one]]\nTry this.\n[[try: a.b Sample]]\n\n[[example:Second]]\nMore.\n[[figure: a.b 1]]\n");
        Assert.Equal("# Lesson\n\nText.\n\n[[see-example: 1]]\n", learn);
        Assert.Equal(2, pages.Count);
        Assert.Equal("First one", pages[0].Title);
        Assert.Contains("[[try: a.b Sample]]", pages[0].Markdown, StringComparison.Ordinal);
        Assert.Equal("Second", pages[1].Title);
        Assert.DoesNotContain("Try this", pages[1].Markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Split_of_a_page_without_examples_is_the_page()
    {
        var (learn, pages) = ExamplePages.Split("# Lesson\r\n\r\nText.");
        Assert.Equal("# Lesson\n\nText.\n", learn);
        Assert.Empty(pages);
    }

    [Fact]
    public void Try_spec_reads_settings_ranges_results_and_figure()
    {
        var spec = TrySpec.Parse(" st.x.try  Mean=40..60 Sample  | ProbabilityBetween Mean figure=2 ", out var problem);
        Assert.Null(problem);
        Assert.NotNull(spec);
        Assert.Equal("st.x.try", spec.TaskId);
        Assert.Equal(2, spec.Inputs.Count);
        Assert.Equal(new TryInput("Mean", 40, 60), spec.Inputs[0]);
        Assert.False(spec.Inputs[1].HasRange);
        Assert.Equal(["ProbabilityBetween", "Mean"], spec.Results);
        Assert.Equal(2, spec.Figure);
        Assert.Equal(0, TrySpec.Parse("t A | figure=none", out _)!.Figure);
        Assert.Equal(TrySpec.AllFigures, TrySpec.Parse("t A | figure=all", out _)!.Figure);
        Assert.Equal(1, TrySpec.Parse("t A", out _)!.Figure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("st.x.try")]
    [InlineData("st.x.try Mean=60..40")]
    [InlineData("st.x.try Mean=a..b")]
    [InlineData("st.x.try Mean Mean")]
    [InlineData("st.x.try Mean | figure=two")]
    public void Try_spec_says_what_is_wrong(string argument)
    {
        Assert.Null(TrySpec.Parse(argument, out var problem));
        Assert.False(string.IsNullOrWhiteSpace(problem));
    }

    private static StatsTask Rejects() => StatsTask.Parse("""
        { "id": "t.rejects", "title": "Rejects", "analysis": "DescriptiveStatistics", "settings": { "Sample": [4, 7, 5, 9, 5] } }
        """, "tasks/t.json");

    [Fact]
    public void Try_build_changes_only_the_named_setting_and_works_it_out_again()
    {
        var task = Rejects();
        var before = StatsTaskRunner.Build(task);
        var after = TrySpec.Build(task, new Dictionary<string, string> { ["Sample"] = "4\n7\n5\n29\n5" });
        double Read(BuiltTask b, string name) => Convert.ToDouble(b.Analysis.Properties().Single(p => p.XmlName == name).Read(b.Analysis), CultureInfo.InvariantCulture);
        Assert.Equal(6, Read(before, "Mean"), 9);
        Assert.Equal(10, Read(after, "Mean"), 9);
        Assert.Equal(5, Read(after, "Median"), 9);
        Assert.Equal("4\n7\n5\n9\n5", TrySpec.TextOf(before.Analysis, before.Analysis.Properties().Single(p => p.XmlName == "Sample")));
    }

    [Fact]
    public void Try_kinds_cover_numbers_choices_switches_and_values()
    {
        var properties = new DescriptiveStatistics().Properties().ToDictionary(p => p.XmlName);
        Assert.Equal(TryInputKind.Values, TrySpec.KindOf(properties["Sample"]));
        Assert.Equal(TryInputKind.Unsupported, TrySpec.KindOf(properties["Name"]));
        Assert.Equal(TryInputKind.Unsupported, TrySpec.KindOf(properties["Id"]));
    }

    [Fact]
    public void Try_changes_a_column_of_the_tasks_table_named_in_quotes()
    {
        var task = StatsTask.Parse("""
            { "id": "t.line", "title": "Line", "analysis": "SimpleRegression",
              "tables": [ { "name": "Turning", "columns": { "Feed (mm/rev)": [1, 2, 3, 4], "Ra": [2, 4, 6, 8] } } ],
              "settings": { "XColumn": "@Turning/Feed (mm/rev)", "YColumn": "@Turning/Ra" } }
            """, "tasks/t.json");
        var spec = TrySpec.Parse("t.line \"@Turning/Feed (mm/rev)\" @Turning/Ra | Slope", out var problem);
        Assert.Null(problem);
        Assert.Equal(("Turning", "Feed (mm/rev)"), spec!.Inputs[0].TableColumn);
        Assert.True(spec.Inputs[1].IsColumn);
        Assert.Equal("2\n4\n6\n8", TrySpec.ColumnText(task, spec.Inputs[1]));
        double Slope(BuiltTask b) => Convert.ToDouble(b.Analysis.Properties().Single(p => p.XmlName == "Slope").Read(b.Analysis), CultureInfo.InvariantCulture);
        Assert.Equal(2, Slope(StatsTaskRunner.Build(task)), 9);
        Assert.Equal(4, Slope(TrySpec.Build(task, new Dictionary<string, string> { ["@Turning/Ra"] = "4\n8\n12\n16" })), 9);
    }

    // ── The validator ────────────────────────────────────────────────────────

    private static string CopyOfPackage()
    {
        var copy = Path.Combine(Path.GetTempPath(), "statistle-examples-" + Guid.NewGuid().ToString("N"));
        foreach (var file in Directory.GetFiles(ContentTests.Egn3443, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(copy, Path.GetRelativePath(ContentTests.Egn3443, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
        return copy;
    }

    private static IReadOnlyList<string> ProblemsWith(string itemRelative, Func<string, string> change, params (string File, string Json)[] tasks)
    {
        var copy = CopyOfPackage();
        try
        {
            var path = Path.Combine(copy, itemRelative);
            File.WriteAllText(path, change(File.ReadAllText(path)));
            foreach (var (file, json) in tasks) File.WriteAllText(Path.Combine(copy, "tasks", file), json);
            return StatsContent.Load(copy).Validate();
        }
        finally { Directory.Delete(copy, recursive: true); }
    }

    private const string Item = "modules/02-descriptive/02-central-tendency.md";
    private const string RejectsTask = """{ "id": "st.test.rejects", "title": "Rejects", "analysis": "DescriptiveStatistics", "settings": { "Sample": [4, 7, 5, 9, 5] } }""";

    [Fact]
    public void Validator_wants_a_figure_in_every_example()
    {
        var problems = ProblemsWith(Item, md => md + "\n\n[[example: Words only]]\n\nNothing to see here.\n");
        Assert.Contains(problems, p => p.Contains("Words only", StringComparison.Ordinal) && p.Contains("no figure", StringComparison.Ordinal));
    }

    [Fact]
    public void Validator_checks_every_see_example_button()
    {
        var problems = ProblemsWith(Item, md => md.Replace("## Common mistakes", "[[see-example: 9]]\n\n## Common mistakes", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Contains("[[see-example: 9]] names no example", StringComparison.Ordinal));
    }

    [Fact]
    public void Validator_keeps_try_off_the_learn_page()
    {
        var problems = ProblemsWith(Item, md => md.Replace("## Common mistakes", "[[try: st.test.rejects Sample]]\n\n## Common mistakes", StringComparison.Ordinal),
            ("test-rejects.json", RejectsTask));
        Assert.Contains(problems, p => p.Contains("is on the Learn page", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("[[try: st.test.rejects Nonsense]]", "has no setting \"Nonsense\"")]
    [InlineData("[[try: st.test.rejects Sample | NotAResult]]", "is not a result")]
    [InlineData("[[try: st.test.rejects Mean]]", "is a result")]
    [InlineData("[[try: st.test.rejects Sample | figure=9]]", "figure=9")]
    [InlineData("[[try: st.m2.central-tendency.ex Sample]]", "an exercise")]
    [InlineData("[[try: st.test.nowhere Sample]]", "not in tasks/")]
    public void Validator_checks_each_try(string widget, string expected)
    {
        var problems = ProblemsWith(Item, md => md + $"\n\n[[example: A test]]\n\n{widget}\n", ("test-rejects.json", RejectsTask));
        Assert.Contains(problems, p => p.Contains(expected, StringComparison.Ordinal));
    }

    [Fact]
    public void Validator_accepts_a_good_example()
    {
        var problems = ProblemsWith(Item, md => md.Replace("## Common mistakes", "[[see-example: 1]]\n\n## Common mistakes", StringComparison.Ordinal)
                                                + "\n\n[[example: A test]]\n\nChange one value.\n\n[[try: st.test.rejects Sample | Mean Median]]\n",
            ("test-rejects.json", RejectsTask));
        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    /// <summary>
    /// For authors writing an example's words: a task's results as the example starts, and after changes, so each number the words
    /// promise ("the mean jumps to 10") can be checked against the engine. STATISTLE_PRINT_TASK names the task;
    /// STATISTLE_PRINT_CHANGES is changes separated by ";", each Setting=value with values in a list separated by commas
    /// (<c>Sample=4,7,5,29,5; Classes=3</c>), and "|" between steps to print each step (<c>Mean=50 | Mean=55</c>); a column of a task table is <c>@Table/Column=1,2,3</c>. Prints to the test output.
    /// </summary>
    [Fact]
    public void Print_a_task_when_asked()
    {
        if (Environment.GetEnvironmentVariable("STATISTLE_PRINT_TASK") is not { Length: > 0 } id) return;
        var content = StatsContent.Load(ContentTests.Egn3443);
        var task = content.Find(id) ?? throw new InvalidOperationException($"No task {id}.");
        var steps = new List<Dictionary<string, string>> { new() };
        var changed = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var step in (Environment.GetEnvironmentVariable("STATISTLE_PRINT_CHANGES") ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var change in step.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var eq = change.IndexOf('=', StringComparison.Ordinal);
                changed[change[..eq].Trim()] = string.Join('\n', change[(eq + 1)..].Split(',', StringSplitOptions.TrimEntries));
            }
            steps.Add(new Dictionary<string, string>(changed));
        }
        var text = new StringBuilder();
        foreach (var step in steps)
        {
            text.AppendLine(step.Count == 0 ? "== As it starts" : "== With " + string.Join("; ", step.Select(kv => $"{kv.Key}={kv.Value.Replace('\n', ',')}")));
            try
            {
                var built = TrySpec.Build(task, step);
                if (built.Analysis is Statistle.Core.Persistence.IReportsProblem { Problem: { Length: > 0 } problem }) text.AppendLine("Problem: " + problem);
                foreach (var p in Statistle.Core.Methods.MethodScope.VisibleProperties(built.Analysis).Where(p => p.IsResult && !p.Hidden))
                    text.AppendLine($"{p.XmlName} ({p.Label}): {built.Analysis.Display(p)}");
            }
            catch (ContentException ex) { text.AppendLine("Cannot be worked out: " + ex.Message); }
        }
        output.WriteLine(text.ToString());
        if (Environment.GetEnvironmentVariable("STATISTLE_PRINT_FILE") is { Length: > 0 } file) File.WriteAllText(file, text.ToString());
    }

    /// <summary>
    /// For authors: every analysis's settings an example can change, and its results. Writes the list when
    /// STATISTLE_TRY_SETTINGS names a file (docs/TRY-SETTINGS.md is made this way).
    /// </summary>
    [Fact]
    public void Write_the_settings_each_analysis_can_change()
    {
        var md = new StringBuilder("# What an example to try can change\n\nMade by `ExampleTests.Write_the_settings_each_analysis_can_change` (set STATISTLE_TRY_SETTINGS to a file). ");
        md.AppendLine("For each analysis: the settings a `[[try: …]]` can name (with the kind of control), and the results it can show after the `|`. A column of the task's own table is named `@Table/Column` (in double quotes when it has a space), whatever the analysis.").AppendLine();
        foreach (var (name, type) in StatsTaskRunner.Analyses.OrderBy(a => a.Key, StringComparer.Ordinal))
        {
            var analysis = (Analysis)Activator.CreateInstance(type)!;
            md.AppendLine($"## {name}").AppendLine();
            var settings = analysis.Properties().Where(p => !p.IsResult && !p.Hidden && TrySpec.KindOf(p) != TryInputKind.Unsupported)
                .Select(p => TrySpec.KindOf(p) switch
                {
                    TryInputKind.Choice => $"`{p.XmlName}` (choice: {string.Join(", ", Enum.GetNames(Nullable.GetUnderlyingType(p.Property.PropertyType) ?? p.Property.PropertyType))})",
                    var k => $"`{p.XmlName}` ({k.ToString().ToLowerInvariant()})",
                });
            md.AppendLine("- Settings: " + string.Join(", ", settings));
            md.AppendLine("- Results: " + string.Join(", ", analysis.Properties().Where(p => p.IsResult && !p.Hidden).Select(p => $"`{p.XmlName}`")));
            md.AppendLine();
        }
        Assert.Contains("DescriptiveStatistics", md.ToString(), StringComparison.Ordinal);
        if (Environment.GetEnvironmentVariable("STATISTLE_TRY_SETTINGS") is { Length: > 0 } file) File.WriteAllText(file, md.ToString());
    }
}
