using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Learning.Core;
using Learning.Core.Content;
using Learning.Core.Learners;
using Learning.Core.Progress;
using Statistle.App.Learning;

namespace Statistle.App.Tests;

/// <summary>
/// Statistics+'s learning pieces (ported from PLC+, plan 9.1–9.4) build and draw off screen: the start screen, the Markdown
/// view with a widget drawn through its hook, the path's tree and the My progress page, and a quiz that is checked.
/// </summary>
public class LearningViewsTests
{
    private const string Sample = """
        # Measures of central tendency

        The **mean** is the sum divided by *n*; see https://reaglin.github.io/EGN3443/ for the course.

        - The mean uses every value.
        - The median is the middle one.

        1. Sort the values.
        2. Take the middle one.

        | Statistic | Python | R |
        |---|---|---|
        | Mean | `np.mean(x)` | `mean(x)` |
        | Median | `np.median(x)` | `median(x)` |

        ```python
        import numpy as np
        x = np.array([50.2, 49.8])   # [[data: not a widget inside code]]
        print(np.mean(x))
        ```

        [[data: st.m2.central-tendency.eg]]

        [[figure: st.m2.central-tendency.eg 1]]
        """;

    [Fact]
    public void A_bar_written_with_a_backslash_or_in_code_stays_in_its_table_cell()
    {
        Assert.Equal(["Conditional", "P(A | B) = P(A ∩ B) / P(B)", "`x | y`"], MarkdownView.SplitRow(@"Conditional | P(A \| B) = P(A ∩ B) / P(B) | `x | y`"));
    }

    [Fact]
    public void Markdown_view_draws_headings_lists_tables_code_and_widgets_through_its_hook()
    {
        WpfHost.Run(() =>
        {
            var asked = new List<WidgetTag>();
            var view = new MarkdownView
            {
                WidgetFactory = w =>
                {
                    asked.Add(w);
                    return w.Name == "data" ? MarkdownView.Wrap(new TextBlock { Text = "the data table", Tag = "widget" }) : null;
                },
                Markdown = Sample,
            };
            var window = new Window { Content = view, Width = 800, Height = 600 };
            view.Measure(new Size(800, 600));
            view.Arrange(new Rect(0, 0, 800, 600));
            WpfHost.Pump();

            var blocks = view.Document.Blocks.ToList();
            Assert.Equal(2, asked.Count); // the widget line inside the code block is code, not a widget
            Assert.Contains(blocks, b => b is BlockUIContainer { Child: TextBlock { Tag: "widget" } });
            Assert.Contains(blocks, b => b is Paragraph p && new TextRange(p.ContentStart, p.ContentEnd).Text.Contains("cannot draw", StringComparison.Ordinal));
            Assert.Contains(blocks, b => b is Table);
            Assert.Equal(2, blocks.OfType<List>().Count());
            var code = Assert.Single(blocks.OfType<Paragraph>(), p => p.FontFamily.Source == "Consolas");
            Assert.Contains("print(np.mean(x))", new TextRange(code.ContentStart, code.ContentEnd).Text, StringComparison.Ordinal);
            Assert.Contains(blocks.OfType<Paragraph>(), p => p.FontSize > 20); // the # heading
            window.Close();
        });
    }

    [Fact]
    public void Start_screen_lists_a_learner_and_offers_the_workbench()
    {
        string data = Path.Combine(Path.GetTempPath(), "statistle-learners-" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable(LearnerPlaces.DataVariable, data);
        try
        {
            WpfHost.Run(() =>
            {
                LearningApp.Name = "Statistics+";
                Assert.Equal(data, LearnerPlaces.DataFolder);
                LearnerPlaces.EnsureShared(LearnerPlaces.DefaultLearnersFolder);
                var store = new LearnerStore(LearnerPlaces.DefaultLearnersFolder, LearnerPlaces.Options);
                store.Create("Ada Lovelace", null, "2468", DateTimeOffset.Now).File.Dispose();

                var screen = new LearnerScreen();
                var window = new Window { Content = screen, Width = 1100, Height = 760 };
                var workbench = false;
                screen.WorkbenchOnly += () => workbench = true;
                screen.Show();
                screen.Measure(new Size(1100, 760));
                screen.Arrange(new Rect(0, 0, 1100, 760));
                WpfHost.Pump();

                var list = (ListBox)screen.FindName("LearnersList");
                var row = Assert.IsType<LearnerRow>(Assert.Single(list.Items.Cast<object>()));
                Assert.Equal("Ada Lovelace", row.Name);
                Assert.Same(row, list.SelectedItem); // the only learner is chosen, ready for the PIN

                var button = (Button)screen.FindName("WorkbenchButton");
                button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.True(workbench);
                window.Close();
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable(LearnerPlaces.DataVariable, null);
            try { Directory.Delete(data, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    [Fact]
    public void Path_dashboard_and_quiz_work_on_a_small_package()
    {
        string folder = WritePackage();
        try
        {
            WpfHost.Run(() =>
            {
                LearningApp.Name = "Statistics+";
                var package = ContentPackage.Load(folder);
                var progress = new LearningProgress();
                var path = LearningPath.Build(package, progress);

                Assert.Equal(2, path.Roots.Count); // My progress, then the module
                Assert.True(path.Roots[0].IsDashboard);
                var module = path.Roots[1];
                Assert.Equal(3, module.Children.Count);
                Assert.All(module.Children, c => Assert.Equal("○", c.Mark));
                Assert.Contains("Time in Statistics+", path.TimeText, StringComparison.Ordinal);

                var page = DashboardPage.Markdown(package, progress);
                Assert.Contains("## So far", page, StringComparison.Ordinal);
                Assert.Contains("## The path", page, StringComparison.Ordinal);
                Assert.DoesNotContain("PLC+", page, StringComparison.Ordinal);
                var view = new MarkdownView { Markdown = page };
                Assert.Contains(view.Document.Blocks, b => b is Table);

                var item = package.FindItem("st.m2.quiz")!;
                var quiz = new QuizView { ShuffleSeed = "ada" };
                QuizResult? graded = null;
                quiz.Graded += r => graded = r;
                quiz.Load(item.Quiz!, "Questions", []);
                var window = new Window { Content = new ScrollViewer { Content = quiz }, Width = 900, Height = 700 };
                quiz.Measure(new Size(900, 2000));
                quiz.Arrange(new Rect(0, 0, 900, 2000));
                WpfHost.Pump();

                var check = ((StackPanel)quiz.Content).Children.OfType<Button>().Single();
                check.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.NotNull(graded);
                Assert.False(graded!.Passed); // nothing answered
                Assert.Equal(4, graded.Questions.Count);

                progress.Record(item, graded.ToOutcome(), DateTimeOffset.Now, machine: "test");
                path.Refresh(progress);
                Assert.Equal("◐", path.NodeOf(item)!.Mark);
                window.Close();
                QuizView.ForgetAll();
            });
        }
        finally
        {
            try { Directory.Delete(folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>One module: a reading, a quiz of four kinds, and its check.</summary>
    private static string WritePackage()
    {
        string root = Path.Combine(Path.GetTempPath(), "statistle-package-" + Guid.NewGuid().ToString("N"));
        string module = Path.Combine(root, "modules", "02-descriptive");
        Directory.CreateDirectory(module);
        Directory.CreateDirectory(Path.Combine(root, "quizzes"));
        File.WriteAllText(Path.Combine(root, "manifest.json"),
            """{ "id": "test", "version": "0.1.0", "title": "Test", "modules": ["modules/02-descriptive"] }""");
        File.WriteAllText(Path.Combine(module, "module.md"), """
            ---
            id: st.m2
            number: 2
            title: Descriptive Statistics
            status: draft
            minutes: 60
            prerequisites: []
            outcomes:
              - st.m2.o1: Calculate the mean and the median of a data set.
            topics:
              - Central tendency
            ---

            The introduction.
            """);
        File.WriteAllText(Path.Combine(module, "01-reading.md"), """
            ---
            id: st.m2.reading
            title: Central tendency
            minutes: 20
            objectives: [st.m2.o1]
            ---

            # Central tendency

            The mean and the median.
            """);
        File.WriteAllText(Path.Combine(module, "02-quiz.md"), """
            ---
            id: st.m2.quiz
            title: Practice
            minutes: 10
            objectives: [st.m2.o1]
            quiz: quizzes/m2-practice.json
            ---

            # Practice
            """);
        File.WriteAllText(Path.Combine(module, "03-check.md"), """
            ---
            id: st.m2.check
            title: Module check
            kind: assess
            covers: [st.m2.reading, st.m2.quiz]
            minutes: 10
            ---

            # Module check
            """);
        File.WriteAllText(Path.Combine(root, "quizzes", "m2-practice.json"), """
            {
              "id": "st.m2.practice", "title": "Practice", "passThreshold": 1.0, "practice": true,
              "questions": [
                { "id": "q1", "kind": "choice", "objective": "st.m2.o1", "prompt": "Which uses **every** value?",
                  "options": ["The mean", "The median"], "correct": [0], "explanation": "The mean adds them all." },
                { "id": "q2", "kind": "numeric", "objective": "st.m2.o1", "prompt": "The mean of 2, 4 and 6?",
                  "answer": 4, "tolerance": 0, "explanation": "12 / 3 = 4." },
                { "id": "q3", "kind": "match", "objective": "st.m2.o1", "prompt": "Match each to its R function.",
                  "pairs": [ { "left": "mean", "right": "mean(x)" }, { "left": "median", "right": "median(x)" } ],
                  "explanation": "R names them as they are." },
                { "id": "q4", "kind": "text", "objective": "st.m2.o1", "prompt": "The middle value is the …?",
                  "answers": ["median"], "explanation": "The median." }
              ]
            }
            """);
        return root;
    }
}

/// <summary>UX review 2026-11-11 (modules 0–4): a task with no data column lists what it starts from; the answer box says how precise to be.</summary>
public sealed class ExerciseHelpTests
{
    private static Statistle.Learning.BuiltTask Binomial()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        var task = Statistle.Learning.StatsTask.Load(Path.Combine(dir!, "content", "egn3443", "tasks", "m4-binomial-ex.json"), "m4-binomial-ex.json");
        return Statistle.Learning.StatsTaskRunner.Build(task);
    }

    [Fact]
    public void A_task_without_data_lists_its_given_values_by_their_Workbench_labels()
    {
        var given = TaskWorkspace.GivenValues(Binomial());
        Assert.DoesNotContain(given, g => g.Label == "Name");
        Assert.Contains(given, g => g.Value == "15");
        Assert.Contains(given, g => g.Value.StartsWith("0.08", StringComparison.Ordinal));
        Assert.True(given.Count >= 3);
    }

    [Fact]
    public void The_answer_box_says_three_decimals_when_the_tolerance_needs_them()
    {
        var built = Binomial();
        Assert.Contains("three decimals", TaskWorkspace.AnswerTip(built.Expected[0]), StringComparison.Ordinal);   // P(X = 2), tolerance 0.001
        Assert.Contains("Two decimals", TaskWorkspace.AnswerTip(built.Expected[2]), StringComparison.Ordinal);      // the mean
    }
}
