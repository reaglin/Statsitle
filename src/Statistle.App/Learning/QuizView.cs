using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Learning.Core;
using Learning.Core.Content;

namespace Statistle.App.Learning;

/// <summary>
/// An item's questions on its Exercise tab, or a module check (ported from PLC+): one choice, several choices, a number,
/// a typed answer, and pairs to match. "Check my answers" marks each question right or not; the answers stay, for the whole
/// session, so a learner can go and read again, come back, change the wrong ones and check again. The Learning Core grades
/// them (<see cref="QuizGrader"/>), and <see cref="Graded"/> hands the result to the window, which records it
/// (<c>progress.Record(item, result.ToOutcome(), …)</c>, as PLC+ does).
/// <para>
/// PLC+'s own kinds (a truth table to fill in, a machine with a fault to find) are in the core's list too; here they show
/// "This question needs PLC+" and count as unanswered.
/// </para>
/// </summary>
public sealed class QuizView : UserControl
{
    /// <summary>Each quiz's answers and last result in this session, so leaving the tab or the item loses nothing.</summary>
    private static readonly Dictionary<string, (Dictionary<string, QuestionAnswer> Answers, QuizResult? Result)> Session = [];

    /// <summary>Forgets every answer (Switch learner): the next learner starts with blank questions.</summary>
    public static void ForgetAll() => Session.Clear();

    private readonly StackPanel _panel = new() { Margin = new Thickness(4, 8, 12, 16) };
    private readonly List<Entry> _questions = [];
    private Quiz? _quiz;
    private TextBlock _score = new();

    private sealed record Entry(Question Question, int Number, Func<QuestionAnswer> Read, Action<QuestionAnswer> Write, TextBlock Feedback, StackPanel FeedbackWidgets, Border Card);

    /// <summary>
    /// Draws a widget line of a question's prompt or explanation (<c>[[figure: task 1]]</c>, plan 10.15), as the Learn
    /// page does; null when it cannot be drawn. Set it before <see cref="Load"/>. Without it, widgets show as a note.
    /// </summary>
    public Func<WidgetTag, UIElement?>? WidgetFactory { get; set; }

    /// <summary>The learner checked their answers.</summary>
    public event Action<QuizResult>? Graded;

    /// <summary>What to read again when an answer to a check is wrong (the item that teaches the question's outcome).</summary>
    public Func<Question, string?>? WhereToLook { get; set; }

    /// <summary>Who is answering (the learner's ID): each learner sees the options in an order of their own (<see cref="OptionOrder"/>).</summary>
    public string ShuffleSeed { get; set; } = "";

    public QuizView()
    {
        LearningTheme.Apply(this);
        Content = _panel;
    }

    private static Brush Res(string key) => LearningTheme.Brush(key);

    /// <summary>Shows a quiz, with the answers given to it earlier in this session. <paramref name="intro"/> lines come first.</summary>
    public void Load(Quiz quiz, string heading, IReadOnlyList<string> intro)
    {
        Remember();
        _quiz = quiz;
        _questions.Clear();
        _panel.Children.Clear();
        _panel.Children.Add(new TextBlock { Text = heading, FontSize = 18, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap });
        foreach (var line in intro) _panel.Children.Add(Para(line, 0, 2));
        _panel.Children.Add(Para(
            $"{quiz.Questions.Count} question{(quiz.Questions.Count == 1 ? "" : "s")}. Answer them all, then press Check my answers at the bottom. " +
            $"You need {quiz.PassThreshold:P0} to pass, and you can check as often as you like. " +
            (quiz.Practice ? "Each answer says why, and shows the right one when yours is wrong." : "A right answer says why; a wrong one says where to look again, not what the answer is.")
            + $" Your answers stay while {LearningApp.Name} is open, so you can go and read again.", 6, 10, muted: true));

        var n = 0;
        foreach (var q in quiz.Questions)
        {
            n++;
            var card = new StackPanel();
            var border = new Border
            {
                Child = card, BorderBrush = Res("LineBrush"), BorderThickness = new Thickness(1), Background = Res("PanelBrush"),
                Padding = new Thickness(14, 10, 14, 12), Margin = new Thickness(0, 0, 0, 12),
            };
            var spoken = WithoutWidgets(q.Prompt, out _).Replace("**", "", StringComparison.Ordinal).Replace("`", "", StringComparison.Ordinal);
            AutomationProperties.SetName(border, $"Question {n}: {spoken}");
            card.Children.Add(new TextBlock { Text = $"Question {n} of {quiz.Questions.Count}", FontWeight = FontWeights.SemiBold, Foreground = Res("MutedTextBrush") });
            AddPrompt(card, q.Prompt);
            var (read, write) = Inputs(q, card, n);
            var feedback = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0), Visibility = Visibility.Collapsed };
            AutomationProperties.SetLiveSetting(feedback, AutomationLiveSetting.Polite);
            card.Children.Add(feedback);
            var feedbackWidgets = new StackPanel { Visibility = Visibility.Collapsed };
            card.Children.Add(feedbackWidgets);
            _panel.Children.Add(border);
            _questions.Add(new Entry(q, n, read, write, feedback, feedbackWidgets, border));
        }

        var check = new Button
        {
            Content = "_Check my answers", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 4, 0, 0),
            ToolTip = "Marks every answer ✓ or ✗. Your answers stay, so you can change the ✗ ones and check again.",
        };
        check.Click += (_, _) => Check();
        _panel.Children.Add(check);
        _score = new TextBlock { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0), TextWrapping = TextWrapping.Wrap };
        AutomationProperties.SetLiveSetting(_score, AutomationLiveSetting.Polite);
        _panel.Children.Add(_score);

        // What the learner had answered, and how it was marked, the last time they were here.
        if (Session.TryGetValue(quiz.Id, out var saved))
        {
            foreach (var e in _questions)
                if (saved.Answers.TryGetValue(e.Question.Id, out var a)) e.Write(a);
            if (saved.Result is { } result) Show(result, scroll: false);
        }
    }

    /// <summary>Keeps the answers on screen now, for when the learner comes back.</summary>
    public void Remember()
    {
        if (_quiz is null) return;
        var answers = _questions.ToDictionary(x => x.Question.Id, x => x.Read());
        Session[_quiz.Id] = (answers, Session.TryGetValue(_quiz.Id, out var old) ? old.Result : null);
    }

    /// <summary>Clears the questions on screen without keeping them (another learner signs in).</summary>
    public void Reset()
    {
        _quiz = null;
        _questions.Clear();
        _panel.Children.Clear();
    }

    /// <summary>Checks the answers on screen, as the Check my answers button does (also for the tests).</summary>
    public void Check()
    {
        if (_quiz is null) return;
        var answers = _questions.ToDictionary(x => x.Question.Id, x => x.Read());
        // A question left blank is not marked wrong: nothing is counted until every question has an answer (UX sweep, plan 15.1).
        var blank = _questions.Where(e => IsBlank(e.Question, answers[e.Question.Id])).Select(e => e.Number).ToList();
        if (blank.Count > 0)
        {
            Session[_quiz.Id] = (answers, Session.TryGetValue(_quiz.Id, out var old) ? old.Result : null);
            _score.Text = BlankMessage(blank);
            _score.Foreground = Res("WrongBrush");
            _questions.First(e => e.Number == blank[0]).Card.BringIntoView();
            return;
        }
        _score.ClearValue(TextBlock.ForegroundProperty);
        var result = QuizGrader.Grade(_quiz, answers, WhereToLook);
        Session[_quiz.Id] = (answers, result);
        Show(result, scroll: true);
        Graded?.Invoke(result);
    }

    /// <summary>"Questions 4 and 6 have no answer yet. Answer them, then press Check my answers. Nothing has been counted."</summary>
    public static string BlankMessage(IReadOnlyList<int> numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var list = numbers.Count == 1
            ? numbers[0].ToString(culture)
            : string.Join(", ", numbers.Take(numbers.Count - 1).Select(n => n.ToString(culture))) + " and " + numbers[numbers.Count - 1].ToString(culture);
        return (numbers.Count == 1 ? $"Question {list} has" : $"Questions {list} have") +
               " no answer yet. Answer " + (numbers.Count == 1 ? "it" : "them") + ", then press Check my answers. Nothing has been counted.";
    }

    /// <summary>
    /// Whether a question has no answer (pairs to match: one still on Choose…). PLC+'s own kinds cannot be answered here,
    /// so they never hold a check up.
    /// </summary>
    private static bool IsBlank(Question q, QuestionAnswer a) => q.Kind switch
    {
        QuestionKind.Choice or QuestionKind.Multiple => a.Chosen.Count == 0,
        QuestionKind.Numeric or QuestionKind.Text => string.IsNullOrWhiteSpace(a.Text),
        QuestionKind.Match => a.Matches.Count == 0 || a.Matches.Any(m => m < 0),
        _ => false,
    };

    private void Show(QuizResult result, bool scroll)
    {
        Entry? firstWrong = null;
        foreach (var e in _questions)
        {
            var r = result.Questions.FirstOrDefault(x => x.Id == e.Question.Id);
            if (r is null) continue;
            e.Feedback.Visibility = Visibility.Visible;
            e.Feedback.Text = (r.Correct ? "✓ " : "✗ ") + WithoutWidgets(r.Feedback, out var widgets);
            // A picture in the explanation is drawn under the feedback when the feedback shows the explanation.
            e.FeedbackWidgets.Children.Clear();
            foreach (var w in widgets) e.FeedbackWidgets.Children.Add(DrawWidget(w));
            e.FeedbackWidgets.Visibility = widgets.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            e.Feedback.Foreground = r.Correct ? Res("PowerOnBrush") : Res("WrongBrush");
            e.Card.BorderBrush = r.Correct ? Res("PowerOnBrush") : Res("WrongBrush");
            e.Card.BorderThickness = new Thickness(r.Correct ? 1 : 2);
            if (!r.Correct) firstWrong ??= e;
        }
        var wrong = _questions.Where(e => result.Questions.Any(r => r.Id == e.Question.Id && !r.Correct)).Select(e => e.Number).ToList();
        var right = result.Questions.Count(q => q.Correct);
        _score.Text = $"{(result.Passed ? "Passed" : "Not passed yet")}: {right} of {result.Questions.Count} right ({result.Score:0} of {result.MaxScore:0} points)."
                      + (wrong.Count == 0 ? "" : $" Marked ✗: question{(wrong.Count == 1 ? "" : "s")} {string.Join(", ", wrong)}.")
                      + (result.Passed || _quiz is null ? "" : $" You need {_quiz.PassThreshold:P0}: change the answers marked ✗ and check again.");
        if (scroll) (firstWrong?.Card ?? (FrameworkElement)_score).BringIntoView();
    }

    // ---- The inputs for each kind of question: how to read the learner's answer, and how to put it back ----

    private (Func<QuestionAnswer>, Action<QuestionAnswer>) Inputs(Question q, StackPanel card, int n)
    {
        switch (q.Kind)
        {
            case QuestionKind.Fault:
            case QuestionKind.TruthTable:
                card.Children.Add(Para("This question needs PLC+: it is about a machine or a truth table, which Statistics+ does not draw. " +
                                       "Tell your instructor; it counts as not answered.", 0, 0, muted: true));
                return (() => new QuestionAnswer(), _ => { });
            case QuestionKind.Choice:
            {
                // buttons[k] shows the option at place order[k] in the file; answers are kept by the file's places.
                var order = OptionOrder.For(ShuffleSeed, _quiz?.Id ?? "", q);
                var buttons = order.Select(i => new RadioButton { Content = Wrapped(q.Options[i]), GroupName = $"q{n}-{q.Id}", Margin = new Thickness(0, 2, 0, 2) }).ToList();
                foreach (var b in buttons) card.Children.Add(b);
                return (() => new QuestionAnswer { Chosen = [.. buttons.Select((b, k) => (b, k)).Where(x => x.b.IsChecked == true).Select(x => order[x.k])] },
                        a => { for (var k = 0; k < buttons.Count; k++) buttons[k].IsChecked = a.Chosen.Contains(order[k]); });
            }
            case QuestionKind.Multiple:
            {
                if (!q.Prompt.Contains("every one", StringComparison.OrdinalIgnoreCase))
                    card.Children.Add(Para("Choose every one that is right.", 0, 4, muted: true));
                var order = OptionOrder.For(ShuffleSeed, _quiz?.Id ?? "", q);
                var boxes = order.Select(i => new CheckBox { Content = Wrapped(q.Options[i]), Margin = new Thickness(0, 2, 0, 2) }).ToList();
                foreach (var b in boxes) card.Children.Add(b);
                return (() => new QuestionAnswer { Chosen = [.. boxes.Select((b, k) => (b, k)).Where(x => x.b.IsChecked == true).Select(x => order[x.k])] },
                        a => { for (var k = 0; k < boxes.Count; k++) boxes[k].IsChecked = a.Chosen.Contains(order[k]); });
            }
            case QuestionKind.Numeric:
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                var box = new TextBox { Width = 120, ToolTip = "Type a number, such as 100 or 0.5" + (q.Unit is null ? "" : $", in {q.Unit}") };
                AutomationProperties.SetName(box, $"Answer to question {n}" + (q.Unit is null ? "" : $", in {q.Unit}"));
                row.Children.Add(box);
                if (q.Unit is not null) row.Children.Add(new TextBlock { Text = q.Unit, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) });
                card.Children.Add(row);
                return (() => new QuestionAnswer { Text = box.Text }, a => box.Text = a.Text ?? "");
            }
            case QuestionKind.Text:
            {
                var tip = q.Base switch
                {
                    2 => "Type only 0s and 1s, such as 0010 1101 (spaces, 2# or 0b are fine)",
                    16 => "Type 0-9 and A-F, such as 1F (16#, 0x or a trailing h are fine)",
                    8 => "Type only the digits 0-7, such as 17 (8# is fine)",
                    _ => "Type your answer (capitals and spaces do not matter)",
                };
                var box = new TextBox { Width = 260, FontFamily = new FontFamily("Consolas"), FontSize = 16, ToolTip = tip, HorizontalAlignment = HorizontalAlignment.Left };
                AutomationProperties.SetName(box, $"Answer to question {n}");
                AutomationProperties.SetHelpText(box, tip);
                card.Children.Add(box);
                card.Children.Add(Para(tip + ".", 4, 0, muted: true));
                return (() => new QuestionAnswer { Text = box.Text }, a => box.Text = a.Text ?? "");
            }
            case QuestionKind.Match:
            {
                var choices = q.MatchChoices;
                var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var combos = new List<ComboBox>();
                for (var i = 0; i < q.Pairs.Count; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    var shortCode = q.Pairs[i].Left.Length <= 16 && !q.Pairs[i].Left.Contains(' ');
                    var left = new TextBlock
                    {
                        Text = q.Pairs[i].Left, TextWrapping = TextWrapping.Wrap, MaxWidth = 420,
                        VerticalAlignment = shortCode ? VerticalAlignment.Center : VerticalAlignment.Top, Margin = new Thickness(0, 5, 14, 3),
                    };
                    if (shortCode) { left.FontFamily = new FontFamily("Consolas"); left.FontSize = 16; }
                    var combo = new ComboBox { MinWidth = 300, MaxWidth = 460, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 3, 0, 3), ToolTip = $"Choose what {q.Pairs[i].Left} goes with" };
                    combo.Items.Add("Choose…");
                    foreach (var c in choices) combo.Items.Add(c);
                    combo.SelectedIndex = 0;
                    AutomationProperties.SetName(combo, $"Question {n}: what {q.Pairs[i].Left} is");
                    Grid.SetRow(left, i);
                    Grid.SetRow(combo, i);
                    Grid.SetColumn(combo, 1);
                    grid.Children.Add(left);
                    grid.Children.Add(combo);
                    combos.Add(combo);
                }
                card.Children.Add(grid);
                return (() => new QuestionAnswer { Matches = [.. combos.Select(c => c.SelectedIndex - 1)] },
                        a => { for (var i = 0; i < combos.Count && i < a.Matches.Count; i++) combos[i].SelectedIndex = a.Matches[i] + 1; });
            }
            default:
                return (() => new QuestionAnswer(), _ => { });
        }
    }

    // ---- Widgets in a question (plan 10.15) -----------------------------------------------------------

    /// <summary>The prompt: its text in paragraphs, and each widget line drawn where it is written.</summary>
    private void AddPrompt(StackPanel card, string prompt)
    {
        var text = new List<string>();
        void Flush()
        {
            var joined = string.Join("\n", text).Trim();
            if (joined.Length > 0) card.Children.Add(Para(joined, 2, 8));
            text.Clear();
        }
        foreach (var line in prompt.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (Widgets.On(line) is { } widget)
            {
                Flush();
                card.Children.Add(DrawWidget(widget));
            }
            else text.Add(line);
        }
        Flush();
    }

    /// <summary>The text without its widget lines, and the widgets taken out, in order.</summary>
    internal static string WithoutWidgets(string text, out List<WidgetTag> widgets)
    {
        widgets = [];
        var kept = new List<string>();
        foreach (var line in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (Widgets.On(line) is { } widget) widgets.Add(widget);
            else kept.Add(line);
        }
        return string.Join("\n", kept).Trim();
    }

    private UIElement DrawWidget(WidgetTag widget)
    {
        UIElement? drawn;
        try { drawn = WidgetFactory?.Invoke(widget); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException or ContentException)
        {
            drawn = null; // a widget that fails is a note in the question, never a crash of the page
        }
        if (drawn is FrameworkElement fe) fe.Margin = new Thickness(0, 4, 0, 10);
        if (drawn is not null) return drawn;
        var note = Para("(The picture for this question could not be drawn. Answer from the text, and please tell your instructor.)", 2, 8, muted: true);
        note.ToolTip = $"{widget.Name}: {widget.Argument}";
        return note;
    }

    // ---- Text -----------------------------------------------------------------------------------------

    private static TextBlock Wrapped(string text)
    {
        var block = new TextBlock { TextWrapping = TextWrapping.Wrap, MaxWidth = 760 };
        AddInlines(block, text);
        return block;
    }

    /// <summary>A paragraph with **bold** shown bold and `code` in a fixed-width font (the only Markdown a question needs).</summary>
    private static TextBlock Para(string markdown, double above, double below, bool muted = false)
    {
        var block = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, above, 0, below), MaxWidth = 900, HorizontalAlignment = HorizontalAlignment.Left };
        if (muted) block.Foreground = Res("MutedTextBrush");
        AddInlines(block, markdown);
        return block;
    }

    private static void AddInlines(TextBlock block, string markdown)
    {
        var parts = markdown.Split("**");
        for (var i = 0; i < parts.Length; i++)
        {
            var code = parts[i].Split('`');
            for (var j = 0; j < code.Length; j++)
            {
                if (code[j].Length == 0) continue;
                var run = new Run(code[j]) { FontWeight = i % 2 == 1 ? FontWeights.SemiBold : FontWeights.Normal };
                if (j % 2 == 1) run.FontFamily = new FontFamily("Consolas");
                block.Inlines.Add(run);
            }
        }
    }
}
