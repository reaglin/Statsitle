using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Learning.Core;
using Learning.Core.Content;
using Statistle.App.Plotting;
using Statistle.Core.Code;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;
using Statistle.Learning;

namespace Statistle.App.Learning;

/// <summary>
/// The Example and Exercise tabs of an item (plan 10.3, 10.4). An Example shows a task's analysis worked: the
/// data, the results, the figures and the Python and R. An Exercise shows the data and a box for each result asked;
/// <b>Check my answers</b> grades them with <see cref="StatsTaskRunner"/>, and once they pass the worked solution
/// opens below. Either can be copied to the Workbench to change its inputs.
/// </summary>
public sealed class TaskWorkspace : ContentControl
{
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(0x5B, 0x66, 0x70));
    private static readonly Brush Good = new SolidColorBrush(Color.FromRgb(0x1E, 0x7B, 0x34));
    private static readonly Brush Bad = new SolidColorBrush(Color.FromRgb(0xB0, 0x24, 0x18));
    private static readonly Brush Panel = new SolidColorBrush(Color.FromRgb(0xF4, 0xF6, 0xF8));
    private static readonly Brush Line = new SolidColorBrush(Color.FromRgb(0xD5, 0xDB, 0xE1));

    private readonly Func<string, FrameworkElement> _markdown;
    private readonly Dictionary<string, TextBox> _boxes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TextBlock> _marks = new(StringComparer.Ordinal);
    private BuiltTask? _built;
    private StackPanel? _solution;
    private TextBlock? _summary;
    private StackPanel? _hintPanel;
    private int _hintsShown;

    // What was typed in each exercise this session, so moving to another item and back keeps it (as the quizzes do).
    private static readonly Dictionary<string, string> Typed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Forgets every exercise's typed answers (when another learner signs in).</summary>
    public static void ForgetAll() => Typed.Clear();

    /// <param name="markdown">Draws a piece of Markdown (the task's introduction); the main window passes its Markdown view.</param>
    public TaskWorkspace(Func<string, FrameworkElement>? markdown = null)
    {
        _markdown = markdown ?? (text => new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap });
        Focusable = false;
    }

    /// <summary>Raised when an exercise has been checked: the outcome, how many hints were shown, and the work (JSON).</summary>
    public event Action<ChallengeOutcome, int, string>? Checked;

    /// <summary>Raised by <b>Open in the Workbench</b>, with the task set up as shown.</summary>
    public event Action<BuiltTask>? OpenInWorkbench;

    public BuiltTask? Built => _built;

    /// <summary>Shows a task as an Example: everything worked out.</summary>
    public void ShowExample(StatsTask task) => Show(task, exercise: false, savedWork: null, passed: false);

    /// <summary>Shows a task as an Exercise, with the learner's last answers and whether it has passed.</summary>
    public void ShowExercise(StatsTask task, string? savedWork, bool passed) => Show(task, exercise: true, savedWork, passed);

    /// <summary>Shows a sentence in place of a task (no example, a task that cannot be built).</summary>
    public void ShowMessage(string text)
    {
        _built = null;
        Content = new ScrollViewer
        {
            Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(16), Foreground = Muted },
        };
    }

    private void Show(StatsTask task, bool exercise, string? savedWork, bool passed)
    {
        _boxes.Clear();
        _marks.Clear();
        _hintsShown = 0;
        try { _built = StatsTaskRunner.Build(task); }
        catch (ContentException ex)
        {
            ShowMessage("This task could not be set up, so it cannot be shown. Please tell the author: " + ex.Message);
            return;
        }

        var page = new StackPanel { Margin = new Thickness(16, 12, 16, 24), MaxWidth = 980, HorizontalAlignment = HorizontalAlignment.Left };
        page.Children.Add(new TextBlock
        {
            Text = (exercise ? "Exercise: " : "Example: ") + task.Title,
            FontSize = 18, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4), TextWrapping = TextWrapping.Wrap,
        });
        page.Children.Add(new TextBlock
        {
            Text = exercise
                ? $"Work out the results asked for below (by hand, in Python or R, or in the Workbench, as the instructions say), type them in, and press Check my answers. The Workbench analysis that does this: {_built.Analysis.Title}."
                : $"A worked example with {_built.Analysis.Title}: the data, every result, the figures, and the Python and R that give the same numbers. Open it in the Workbench to change the inputs and watch the results move.",
            Foreground = Muted, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10),
        });
        if (task.Intro.Length > 0) page.Children.Add(_markdown(task.Intro));

        page.Children.Add(Heading("The data"));
        page.Children.Add(DataView(_built));

        if (exercise)
        {
            page.Children.Add(Heading("Your answers"));
            page.Children.Add(AnswerForm(_built, Typed.TryGetValue(task.Id, out var typed) ? typed : savedWork));
            _solution = new StackPanel { Visibility = passed ? Visibility.Visible : Visibility.Collapsed };
            _solution.Children.Add(Heading("The worked solution"));
            AddWorked(_solution, _built);
            page.Children.Add(_solution);
            if (passed) Summary("You have passed this exercise. The worked solution is below.", true);
        }
        else
        {
            page.Children.Add(Heading("The results"));
            AddWorked(page, _built);
        }

        Content = new ScrollViewer { Content = page, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
    }

    // ── The data ─────────────────────────────────────────────────────────────

    /// <summary>The task's data as columns: its own number-list settings and the tables it brings.</summary>
    public static IReadOnlyList<(string Name, double[] Values)> DataColumns(BuiltTask built)
    {
        var columns = new List<(string, double[])>();
        foreach (var property in built.Analysis.Properties())
        {
            if (property.IsResult || !built.Task.Settings.ContainsKey(property.XmlName)) continue;
            if (property.Read(built.Analysis) is not DataSeries { Count: > 0 } series) continue;
            if (property.IsSingleColumnSeries)
            {
                columns.Add((property.Label, series.YValues));
                continue;
            }

            // A series of pairs, such as a typed PMF "Table (x, P(x))": both halves, named from the label.
            var names = System.Text.RegularExpressions.Regex.Match(property.Label, @"\(([^,]+), (.+)\)\s*$");
            columns.Add((names.Success ? names.Groups[1].Value : "x", series.XValues));
            columns.Add((names.Success ? names.Groups[2].Value : "y", series.YValues));
        }
        foreach (var table in built.Task.Tables)
            foreach (var (name, values) in table.Columns)
                columns.Add((built.Task.Tables.Count > 1 ? $"{table.Name}: {name}" : name, values));
        return columns;
    }

    /// <summary>
    /// The settings a task gives, by the label the Workbench shows, for a task with no data column (a binomial's n and
    /// p, a simulation's experiment). Blank numbers and the analysis's name are left out.
    /// </summary>
    public static IReadOnlyList<(string Label, string Value)> GivenValues(BuiltTask built)
    {
        var properties = built.Analysis.Properties().ToDictionary(p => p.XmlName, StringComparer.Ordinal);
        var given = new List<(string, string)>();
        foreach (var name in built.Task.Settings.Keys)
        {
            if (name == "Name" || !properties.TryGetValue(name, out var property) || property.IsResult) continue;
            var shown = built.Analysis.Display(property);
            if (shown.Length == 0 || shown == "—") continue;
            given.Add((property.Label, shown));
        }
        return given;
    }

    /// <summary>The answer box's tooltip: how precise the answer must be, from the result and its tolerance.</summary>
    public static string AnswerTip(ExpectedResult expected)
    {
        var precision = expected.Property.Decimals <= 0
            ? "It is a count: type the whole number."
            : expected.Tolerance < 0.005
                ? "Give at least three decimals, such as 0.137."
                : "Two decimals are usually enough, such as 4.87.";
        return $"Type your result as a number. {precision} Enter checks your answers.";
    }

    private FrameworkElement DataView(BuiltTask built)
    {
        var columns = DataColumns(built);
        var panel = new StackPanel();
        if (columns.Count == 0)
        {
            var given = GivenValues(built);
            panel.Children.Add(new TextBlock
            {
                Text = given.Count == 0
                    ? "This task has no data table: the numbers it starts from are in the text above."
                    : "This task has no data table. It starts from these values (also in the text above):",
                Foreground = Muted, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 4),
            });
            if (given.Count > 0)
            {
                var table = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
                table.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                table.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                for (int i = 0; i < given.Count; i++)
                {
                    table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    var label = new TextBlock { Text = given[i].Label, Margin = new Thickness(0, 1, 16, 1), TextWrapping = TextWrapping.Wrap, MaxWidth = 420 };
                    var value = new TextBlock { Text = given[i].Value, Margin = new Thickness(0, 1, 0, 1), FontFamily = new FontFamily("Consolas") };
                    Grid.SetRow(label, i); Grid.SetRow(value, i); Grid.SetColumn(value, 1);
                    table.Children.Add(label); table.Children.Add(value);
                }
                AutomationProperties.SetName(table, "Given values");
                panel.Children.Add(table);
            }
            return panel;
        }

        panel.Children.Add(DataTable(columns, RowLabels(built)));

        var buttons = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
        buttons.Children.Add(SmallButton("Copy the data", "Copies the data with a header row, separated by tabs: paste it into a spreadsheet, or into the Workbench (+ Data table, then Ctrl+V)",
            () => Clipboard.SetText(DataText(columns))));
        buttons.Children.Add(SmallButton("Copy as Python", "Copies the data as numpy arrays, to paste into Python or a Colab notebook",
            () => Clipboard.SetText(DataCode(columns, CodeLanguage.Python))));
        buttons.Children.Add(SmallButton("Copy as R", "Copies the data as R vectors, to paste into R or RStudio",
            () => Clipboard.SetText(DataCode(columns, CodeLanguage.R))));
        panel.Children.Add(buttons);
        return panel;
    }

    /// <summary>
    /// The names of the data's rows, when the task gives them: a contingency table's <c>RowNames</c> or a goodness of
    /// fit's <c>CategoryNames</c> (comma-separated). Null means the rows are just numbered.
    /// </summary>
    public static IReadOnlyList<string>? RowLabels(BuiltTask built)
    {
        foreach (var key in new[] { "RowNames", "CategoryNames" })
            if (built.Task.Settings.TryGetValue(key, out var text) && text.Trim().Length > 0)
                return [.. text.Split(',').Select(s => s.Trim())];
        return null;
    }

    /// <summary>The data as an aligned table, a column per variable, with numbered or named rows.</summary>
    public static FrameworkElement DataTable(IReadOnlyList<(string Name, double[] Values)> columns, IReadOnlyList<string>? rowLabels = null)
    {
        var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        foreach (var _ in columns) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var rows = columns.Max(c => c.Values.Length);
        for (var r = 0; r <= rows; r++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        grid.Children.Add(Cell(rowLabels is null ? "#" : "", 0, 0, header: true));
        for (var c = 0; c < columns.Count; c++)
        {
            grid.Children.Add(Cell(columns[c].Name, 0, c + 1, header: true));
            var decimals = columns[c].Values.Select(Decimals).DefaultIfEmpty(0).Max();
            for (var r = 0; r < columns[c].Values.Length; r++)
                grid.Children.Add(Cell(columns[c].Values[r].ToString("F" + decimals, CultureInfo.CurrentCulture), r + 1, c + 1, header: false));
        }
        for (var r = 1; r <= rows; r++)
            grid.Children.Add(rowLabels is not null && r <= rowLabels.Count
                ? Cell(rowLabels[r - 1], r, 0, header: true)
                : Cell(r.ToString(CultureInfo.CurrentCulture), r, 0, header: false, muted: true));

        var scroller = new ScrollViewer
        {
            Content = grid, MaxHeight = 260, HorizontalAlignment = HorizontalAlignment.Left,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            BorderBrush = Line, BorderThickness = new Thickness(1),
        };
        AutomationProperties.SetName(scroller, "The data");
        return new Border { Child = scroller, BorderBrush = Line, BorderThickness = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Left };
    }

    /// <summary>How many decimals a value was written with (up to 6), so a column shows 25.0 beside 25.1, not 25.</summary>
    private static int Decimals(double value)
    {
        for (var d = 0; d < 6; d++)
            if (Math.Abs(value - Math.Round(value, d)) < 1e-9) return d;
        return 6;
    }

    public static string DataText(IReadOnlyList<(string Name, double[] Values)> columns)
    {
        var rows = columns.Count == 0 ? 0 : columns.Max(c => c.Values.Length);
        var lines = new List<string> { string.Join('\t', columns.Select(c => c.Name)) };
        for (var r = 0; r < rows; r++)
            lines.Add(string.Join('\t', columns.Select(c => r < c.Values.Length ? c.Values[r].ToString("R", CultureInfo.InvariantCulture) : "")));
        return string.Join(Environment.NewLine, lines);
    }

    public static string DataCode(IReadOnlyList<(string Name, double[] Values)> columns, CodeLanguage language)
    {
        var lines = new List<string>();
        if (language == CodeLanguage.Python) lines.Add("import numpy as np");
        lines.AddRange(columns.Select(c => CodeText.Vector(CodeText.Identifier(c.Name), c.Values, language)));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    // ── The answers ──────────────────────────────────────────────────────────

    private FrameworkElement AnswerForm(BuiltTask built, string? savedWork)
    {
        var saved = SavedAnswers(savedWork);
        var form = new Grid { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8) };
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(440) });
        for (var i = 0; i < built.Expected.Count; i++)
        {
            var expected = built.Expected[i];
            form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var unit = UnitSystem.BaseUnit(expected.Property.Unit).Label;
            var label = new TextBlock
            {
                Text = expected.Label + (unit.Length > 0 ? $" ({unit})" : ""), TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 10, 4),
                ToolTip = expected.Property.Help.Length > 0 ? expected.Property.Help : null,
            };
            var box = new TextBox
            {
                Text = saved.TryGetValue(expected.Asked.Result, out var s) ? s : "",
                Margin = new Thickness(0, 3, 10, 3), Padding = new Thickness(4, 2, 4, 2), VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = AnswerTip(expected),
            };
            AutomationProperties.SetName(box, expected.Label);
            box.KeyDown += (_, e) => { if (e.Key == Key.Enter) { CheckAnswers(); e.Handled = true; } };
            box.TextChanged += (_, _) => RememberTyped();
            var mark = new TextBlock { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(label, i); Grid.SetRow(box, i); Grid.SetRow(mark, i);
            Grid.SetColumn(box, 1); Grid.SetColumn(mark, 2);
            form.Children.Add(label); form.Children.Add(box); form.Children.Add(mark);
            _boxes[expected.Asked.Result] = box;
            _marks[expected.Asked.Result] = mark;
        }

        var panel = new StackPanel();
        panel.Children.Add(form);
        var buttons = new WrapPanel { Margin = new Thickness(0, 4, 0, 6) };
        var check = new Button
        {
            Content = "_Check my answers", Padding = new Thickness(14, 5, 14, 5), Margin = new Thickness(0, 0, 8, 0), IsDefault = false,
            ToolTip = "Compares your results with the ones the analysis calculates. Wrong ones say whether they are too high or too low, or name a common mistake, never the answer. Try as often as you like.",
        };
        check.Click += (_, _) => CheckAnswers();
        buttons.Children.Add(check);
        if (built.Task.Hints.Count > 0)
        {
            var hint = SmallButton("Show a hint", $"Shows the next hint ({built.Task.Hints.Count} in all). Hints are noted in your progress.", ShowHint);
            buttons.Children.Add(hint);
        }
        panel.Children.Add(buttons);
        _hintPanel = new StackPanel();
        panel.Children.Add(_hintPanel);
        _summary = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4), FontWeight = FontWeights.SemiBold };
        panel.Children.Add(_summary);
        return panel;
    }

    private void RememberTyped()
    {
        if (_built is null) return;
        Typed[_built.Task.Id] = StatsTaskRunner.Work(_boxes.ToDictionary(b => b.Key, b => b.Value.Text, StringComparer.Ordinal));
    }

    private void ShowHint()
    {
        if (_built is null || _hintPanel is null || _hintsShown >= _built.Task.Hints.Count) return;
        _hintPanel.Children.Add(new Border
        {
            Background = Panel, BorderBrush = Line, BorderThickness = new Thickness(1), Padding = new Thickness(8, 5, 8, 5), Margin = new Thickness(0, 0, 0, 4),
            Child = new TextBlock { Text = $"Hint {_hintsShown + 1}: {_built.Task.Hints[_hintsShown]}", TextWrapping = TextWrapping.Wrap },
        });
        _hintsShown++;
    }

    /// <summary>Types an answer into a result's box (the tests, and a future "fill from my Workbench" button).</summary>
    public void Type(string result, string text)
    {
        if (_boxes.TryGetValue(result, out var box)) box.Text = text;
    }

    /// <summary>Grades what is in the boxes and raises <see cref="Checked"/>.</summary>
    public void CheckAnswers()
    {
        if (_built is null) return;
        var answers = _boxes.ToDictionary(b => b.Key, b => b.Value.Text, StringComparer.Ordinal);
        if (answers.Values.All(string.IsNullOrWhiteSpace))
        {
            Summary("Type a result in each box, then press Check my answers. Nothing has been counted yet.", false);
            return;
        }
        var work = StatsTaskRunner.Work(answers);
        var outcome = new StatsTaskRunner().Run(_built.Task.Json, work);
        foreach (var expected in _built.Expected)
        {
            var failure = StatsTaskRunner.Check(expected, answers.GetValueOrDefault(expected.Asked.Result));
            var mark = _marks[expected.Asked.Result];
            mark.Text = failure is null ? "✓ Right" : "✗ " + failure[(failure.IndexOf(':', StringComparison.Ordinal) + 1)..].Trim();
            mark.Foreground = failure is null ? Good : Bad;
        }
        var right = (int)outcome.Score;
        if (outcome.Passed)
        {
            Summary($"Passed: {right} of {outcome.MaxScore:0} right. The worked solution is below; compare it with yours.", true);
            if (_solution is not null) _solution.Visibility = Visibility.Visible;
        }
        else
        {
            Summary($"{right} of {outcome.MaxScore:0} right. Correct the ones marked ✗ and check again.", false);
        }
        Checked?.Invoke(outcome, _hintsShown, work);
    }

    private void Summary(string text, bool good)
    {
        if (_summary is null) return;
        _summary.Text = text;
        _summary.Foreground = good ? Good : Bad;
    }

    private static Dictionary<string, string> SavedAnswers(string? work)
    {
        var answers = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(work)) return answers;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(work);
            if (doc.RootElement.TryGetProperty("answers", out var a))
                foreach (var p in a.EnumerateObject()) answers[p.Name] = p.Value.GetString() ?? "";
        }
        catch (System.Text.Json.JsonException) { }
        return answers;
    }

    // ── The worked analysis ──────────────────────────────────────────────────

    private void AddWorked(Panel page, BuiltTask built)
    {
        page.Children.Add(ResultsGrid(built));

        if (built.Analysis is IHasFigures drawable)
        {
            var figures = drawable.Figures(null);
            if (figures.Count > 0) page.Children.Add(Heading("Figures"));
            foreach (var figure in figures)
            {
                var panel = new FigurePanel { Width = 760, HorizontalAlignment = HorizontalAlignment.Left };
                panel.Show(figure);
                page.Children.Add(panel);
            }
        }

        if (built.Analysis is ICodeSource)
        {
            page.Children.Add(Heading("The same in Python and R"));
            page.Children.Add(CodeTabs(built));
        }

        var open = new Button
        {
            Content = "Open in the Workbench", Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Left,
            ToolTip = "Copies this analysis and its data into the Workbench, where you can change the inputs and watch every result, figure and line of code change",
        };
        open.Click += (_, _) => OpenInWorkbench?.Invoke(built);
        page.Children.Add(open);
    }

    /// <summary>The results as label, value, unit, with each result's help as its tooltip.</summary>
    public static FrameworkElement ResultsGrid(BuiltTask built)
    {
        var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 120 });
        var row = 0;
        // Only the results the analysis's settings use (as the Workbench shows them), and none that are blank: a Learn
        // page shows what the case gives, not a row of "—" for what it does not ask.
        foreach (var property in Statistle.Core.Methods.MethodScope.VisibleProperties(built.Analysis).Where(p => p.IsResult && !p.Hidden))
        {
            var shown = built.Analysis.Display(property);
            if (shown.Length == 0 || shown == "—") continue;
            var unit = UnitSystem.BaseUnit(property.Unit).Label;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock { Text = property.Label, Margin = new Thickness(0, 2, 12, 2), TextWrapping = TextWrapping.Wrap, ToolTip = property.Help.Length > 0 ? property.Help : null };
            var value = new TextBlock { Text = shown + (unit.Length > 0 && shown != "—" ? " " + unit : ""), Margin = new Thickness(0, 2, 0, 2), FontFamily = new FontFamily("Consolas"), TextWrapping = TextWrapping.Wrap, MaxWidth = 560 };
            Grid.SetRow(label, row); Grid.SetRow(value, row); Grid.SetColumn(value, 1);
            grid.Children.Add(label); grid.Children.Add(value);
            row++;
        }
        AutomationProperties.SetName(grid, "Results");
        return grid;
    }

    /// <summary>The analysis's Python and R in two tabs, each with Copy.</summary>
    public static FrameworkElement CodeTabs(BuiltTask built)
    {
        var tabs = new TabControl { MaxWidth = 900, HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 600 };
        foreach (var (language, name) in new[] { (CodeLanguage.Python, "Python"), (CodeLanguage.R, "R") })
        {
            var code = StatsContent.Code(built, language) ?? "";
            var text = new TextBox
            {
                Text = code, IsReadOnly = true, FontFamily = new FontFamily("Consolas"), FontSize = 12,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 360, BorderThickness = new Thickness(0), Background = Panel, Padding = new Thickness(6),
            };
            AutomationProperties.SetName(text, $"{name} code");
            var copy = SmallButton("Copy " + name, $"Copies the {name} to paste into {(language == CodeLanguage.Python ? "Python or a Colab notebook" : "R or RStudio")}", () => Clipboard.SetText(code));
            copy.HorizontalAlignment = HorizontalAlignment.Left;
            var panel = new DockPanel();
            DockPanel.SetDock(copy, Dock.Bottom);
            panel.Children.Add(copy);
            panel.Children.Add(text);
            tabs.Items.Add(new TabItem { Header = name, Content = panel, ToolTip = $"The {name} that reproduces these numbers" });
        }
        return tabs;
    }

    // ── Small pieces ─────────────────────────────────────────────────────────

    private static TextBlock Heading(string text) =>
        new() { Text = text, FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 6) };

    private static TextBlock Cell(string text, int row, int column, bool header, bool muted = false)
    {
        var cell = new TextBlock
        {
            Text = text, Padding = new Thickness(8, 2, 8, 2), FontWeight = header ? FontWeights.SemiBold : FontWeights.Normal,
            Foreground = muted ? Muted : Brushes.Black, Background = header ? Panel : Brushes.Transparent,
            TextAlignment = header ? TextAlignment.Left : TextAlignment.Right,
        };
        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, column);
        return cell;
    }

    private static Button SmallButton(string text, string tip, Action click)
    {
        var button = new Button { Content = text, Padding = new Thickness(8, 3, 8, 3), Margin = new Thickness(0, 4, 8, 0), ToolTip = tip };
        button.Click += (_, _) =>
        {
            try { click(); }
            catch (System.Runtime.InteropServices.ExternalException) { /* the clipboard was busy; the person can press again */ }
        };
        return button;
    }
}
