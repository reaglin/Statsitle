using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Learning.Core.Content;
using Statistle.App.Plotting;
using Statistle.Core.Analyses;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;
using Statistle.Learning;

namespace Statistle.App.Learning;

/// <summary>
/// An example to try (<c>[[try: …]]</c>, Learning Core plan 3.21-3.22): the settings it names as controls (a slider for a number with
/// a range, a box for a number, a list for a choice, a tick box, or the values one per line), and the analysis's figure and results
/// worked out again each time one changes, so the learner sees what that one thing does. Nothing here counts toward progress.
/// </summary>
public sealed class TryAnalysis : Border
{
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(0x5B, 0x66, 0x70));
    private static readonly Brush Warn = new SolidColorBrush(Color.FromRgb(0xB0, 0x24, 0x18));
    private static readonly Brush Accent = new SolidColorBrush(Color.FromRgb(0x1F, 0x6F, 0x3A));

    private readonly StatsTask _task;
    private readonly TrySpec _spec;
    private readonly Dictionary<string, string> _start = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _now = new(StringComparer.Ordinal);
    private readonly List<Action> _resets = [];
    private readonly List<FigurePanel> _figures = [];
    private readonly ContentControl _results = new() { Focusable = false };
    private readonly TextBlock _problem = new() { TextWrapping = TextWrapping.Wrap, Foreground = Warn, Margin = new Thickness(0, 6, 0, 0), Visibility = Visibility.Collapsed };
    private readonly DispatcherTimer _later = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private bool _resetting;

    /// <summary>The analysis as last worked out (for the tests).</summary>
    public BuiltTask? Current { get; private set; }

    /// <summary>The words under the controls when the last change could not be worked out (for the tests).</summary>
    public string? Problem => _problem.Visibility == Visibility.Visible ? _problem.Text : null;

    /// <param name="task">The task the example starts from.</param>
    /// <param name="spec">What the learner can change and what to show.</param>
    /// <exception cref="ContentException">The task cannot be set up.</exception>
    public TryAnalysis(StatsTask task, TrySpec spec)
    {
        _task = task;
        _spec = spec;
        var built = StatsTaskRunner.Build(task);
        Current = built;
        _later.Tick += (_, _) => { _later.Stop(); Recalculate(); };
        // One results host and the problem line are live regions: a screen reader hears each change (UX review 2026-11-17).
        AutomationProperties.SetName(_results, "The results");
        AutomationProperties.SetLiveSetting(_results, AutomationLiveSetting.Polite);
        AutomationProperties.SetLiveSetting(_problem, AutomationLiveSetting.Assertive);

        BorderBrush = Accent;
        BorderThickness = new Thickness(3, 0, 0, 0);
        Padding = new Thickness(12, 6, 8, 8);
        Margin = new Thickness(0, 4, 0, 4);
        HorizontalAlignment = HorizontalAlignment.Left;
        MaxWidth = 900;
        AutomationProperties.SetName(this, "Try it: " + task.Title);

        var page = new StackPanel();
        var head = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
        head.Inlines.Add(new System.Windows.Documents.Run("▶ Try it: ") { Foreground = Accent, FontWeight = FontWeights.SemiBold });
        head.Inlines.Add(new System.Windows.Documents.Run("change " + Join(spec.Resolve(built.Analysis).Select(r => Lead(r.Input, r.Property))) +
                                                          " and watch the " + (spec.Figure == 0 ? "results" : "figure and the results") + " change with it."));
        page.Children.Add(head);

        var controls = new Grid { Margin = new Thickness(0, 0, 0, 4), HorizontalAlignment = HorizontalAlignment.Left };
        controls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        controls.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var row = 0;
        foreach (var (input, property) in spec.Resolve(built.Analysis))
        {
            if (input.IsColumn)
            {
                // A column of the task's own data (a regression's x or y, an ANOVA's group).
                if (TrySpec.ColumnText(task, input) is not { } columnText) continue;
                _start[input.Name] = _now[input.Name] = columnText;
                AddRow(controls, ref row, input.TableColumn.Column, null, ValuesBox(input.Name, columnText, $"The {input.TableColumn.Column} column of the data."));
                continue;
            }
            if (property is null) continue;
            var text = TrySpec.TextOf(built.Analysis, property);
            _start[property.XmlName] = _now[property.XmlName] = text;
            var control = Control(input, property, text);
            if (control is null) continue;
            AddRow(controls, ref row, property.Label, property.Help.Length > 0 ? property.Help : null, control);
        }
        page.Children.Add(controls);

        var reset = new Button
        {
            Content = "Back to the start", Padding = new Thickness(10, 2, 10, 2), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 2, 0, 0),
            ToolTip = "Puts every value above back as the example started, so you can try another change",
        };
        reset.Click += (_, _) => Reset();
        page.Children.Add(reset);
        page.Children.Add(_problem);
        // The results first, beside what is being changed, so they are in sight while typing (UX review 2026-11-17, M1).
        page.Children.Add(new TextBlock { Text = "The results", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 2) });
        page.Children.Add(_results);

        if (built.Analysis is IHasFigures drawable && spec.Figure != 0)
        {
            var all = drawable.Figures(null);
            var shown = spec.Figure == TrySpec.AllFigures ? Enumerable.Range(0, all.Count) : spec.Figure <= all.Count ? [spec.Figure - 1] : [];
            foreach (var k in shown)
            {
                var panel = new FigurePanel { Width = 760, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 8, 0, 0) };
                panel.Tag = k;
                panel.Show(all[k]);
                _figures.Add(panel);
                page.Children.Add(panel);
            }
        }
        ShowResults(built);
        Child = page;
    }

    private static void AddRow(Grid controls, ref int row, string label, string? help, FrameworkElement control)
    {
        controls.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var text = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 12, 4), VerticalAlignment = VerticalAlignment.Top, ToolTip = help };
        Grid.SetRow(text, row);
        Grid.SetRow(control, row);
        Grid.SetColumn(control, 1);
        controls.Children.Add(text);
        controls.Children.Add(control);
        AutomationProperties.SetName(control, label);
        row++;
    }

    private static string Join(IEnumerable<string> words)
    {
        var list = words.ToList();
        return list.Count switch { 0 => "the values", 1 => list[0], _ => string.Join(", ", list.Take(list.Count - 1)) + " or " + list[^1] };
    }

    /// <summary>How the lead names a control: "the values in Sample" for a list, "the Mean" for one setting (UX review 2026-11-17).</summary>
    private static string Lead(TryInput input, StatistleProperty? property) =>
        input.IsColumn ? $"the values in {input.TableColumn.Column}"
        : property is not null && TrySpec.KindOf(property) == TryInputKind.Values ? $"the values in {property.Label}"
        : $"the {(property?.Label ?? input.Name).Replace(", optional", "", StringComparison.Ordinal)}";

    // ── The controls ─────────────────────────────────────────────────────────

    private FrameworkElement? Control(TryInput input, StatistleProperty property, string text)
    {
        var name = property.XmlName;
        var tip = property.Help.Length > 0 ? property.Help : property.Label;
        switch (TrySpec.KindOf(property))
        {
            case TryInputKind.Number or TryInputKind.Whole when input.HasRange:
            {
                var whole = TrySpec.KindOf(property) == TryInputKind.Whole || property.Decimals <= 0; // a count kept as a double (Tests, Folds) steps by 1 too
                var start = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                var step = whole ? 1 : NiceStep((input.High!.Value - input.Low!.Value) / 100);
                var slider = new Slider
                {
                    Minimum = input.Low!.Value, Maximum = input.High!.Value, Value = start, Width = 300, SmallChange = step, LargeChange = step * 10,
                    IsSnapToTickEnabled = true, TickFrequency = step, VerticalAlignment = VerticalAlignment.Center, ToolTip = tip,
                };
                var shown = new TextBlock { Width = 90, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, FontFamily = new FontFamily("Consolas") };
                void Show(double v) => shown.Text = Format(v, step);
                Show(start);
                slider.ValueChanged += (_, e) =>
                {
                    Show(e.NewValue);
                    if (_resetting) return;
                    _now[name] = e.NewValue.ToString("R", CultureInfo.InvariantCulture);
                    Changed(soon: true);
                };
                _resets.Add(() => { slider.Value = start; Show(start); });
                var lowHigh = new TextBlock
                {
                    Text = $"{Format(input.Low.Value, step)} to {Format(input.High.Value, step)}", Foreground = Muted, FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0),
                };
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                panel.Children.Add(slider);
                panel.Children.Add(shown);
                panel.Children.Add(lowHigh);
                return panel;
            }
            case TryInputKind.Number or TryInputKind.Whole:
            {
                var box = new TextBox { Text = text == "NaN" ? "" : text, Width = 120, Margin = new Thickness(0, 2, 0, 2), ToolTip = tip + " Type a number; the results change as you type." };
                box.TextChanged += (_, _) =>
                {
                    if (_resetting) return;
                    _now[name] = box.Text.Trim();
                    Changed(soon: true);
                };
                _resets.Add(() => box.Text = text == "NaN" ? "" : text);
                return box;
            }
            case TryInputKind.Choice:
            {
                var type = Nullable.GetUnderlyingType(property.Property.PropertyType) ?? property.Property.PropertyType;
                var values = Enum.GetValues(type).Cast<Enum>().ToList();
                var combo = new ComboBox { Width = 300, Margin = new Thickness(0, 2, 0, 2), ToolTip = tip, ItemsSource = values.Select(EnumLabel.Of).ToList() };
                var startIndex = values.FindIndex(v => v.ToString() == text);
                combo.SelectedIndex = startIndex;
                combo.SelectionChanged += (_, _) =>
                {
                    if (_resetting || combo.SelectedIndex < 0) return;
                    _now[name] = values[combo.SelectedIndex].ToString();
                    Changed(soon: false);
                };
                _resets.Add(() => combo.SelectedIndex = startIndex);
                return combo;
            }
            case TryInputKind.Switch:
            {
                var start = text == "true";
                var check = new CheckBox { IsChecked = start, Content = "on", Margin = new Thickness(0, 5, 0, 5), ToolTip = tip };
                check.Click += (_, _) =>
                {
                    if (_resetting) return;
                    _now[name] = check.IsChecked == true ? "true" : "false";
                    Changed(soon: false);
                };
                _resets.Add(() => check.IsChecked = start);
                return check;
            }
            case TryInputKind.Values:
                return ValuesBox(name, text, tip);
            default:
                return null;
        }
    }

    /// <summary>A list of values, one per line, worked out again as they are typed (a series setting, or a column of the data).</summary>
    private FrameworkElement ValuesBox(string name, string text, string tip)
    {
        var shownText = text.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        var box = new TextBox
        {
            Text = shownText, AcceptsReturn = true, Width = 300, MinHeight = 60, MaxHeight = 180, Margin = new Thickness(0, 2, 0, 2),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"),
            ToolTip = tip + " One value per line. Change a value, add one or delete one: the results change as you type.",
        };
        box.TextChanged += (_, _) =>
        {
            if (_resetting) return;
            _now[name] = box.Text;
            Changed(soon: true);
        };
        _resets.Add(() => box.Text = shownText);
        var noteText = CommaDecimals ? "one value per line" : "one value per line (a point for decimals: 2.5)";
        AutomationProperties.SetHelpText(box, noteText);
        var note = new TextBlock { Text = noteText, Foreground = Muted, FontSize = 11 };
        var panel = new StackPanel();
        panel.Children.Add(box);
        panel.Children.Add(note);
        return panel;
    }

    /// <summary>True where Windows writes decimals with a comma (25,1): there a comma is part of a number, not between two.</summary>
    private static bool CommaDecimals => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",";

    /// <summary>
    /// The values typed in a box: one per line, or separated by spaces, semicolons or (unless commas are decimals here) commas.
    /// Null when every one is a number; otherwise which value is not, in words for the learner ("value 4: "abc" is not a number …").
    /// </summary>
    public static string? ReadValues(string typed, out double[] values)
    {
        char[] separators = CommaDecimals ? ['\r', '\n', ';', ' ', '\t'] : ['\r', '\n', ',', ';', ' ', '\t'];
        var words = typed.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var list = new List<double>();
        for (var i = 0; i < words.Length; i++)
        {
            if (!TryNumber(words[i], out var v))
            {
                values = [];
                return $"value {i + 1}: \"{words[i]}\" is not a number. Type a number such as 5 or {(CommaDecimals ? "2,5" : "2.5")}, one per line.";
            }
            list.Add(v);
        }
        values = [.. list];
        return null;
    }

    /// <summary>Values as a task writes them, one per line (for the tests); words that are not numbers are left out.</summary>
    public static string ValuesText(string typed)
    {
        ReadValues(typed, out var values);
        return string.Join('\n', values.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
    }

    private static bool TryNumber(string text, out double value) =>
        CommaDecimals
            ? double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            : double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);

    private static double NiceStep(double raw)
    {
        if (!(raw > 0)) return 1;
        var power = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        var m = raw / power;
        return (m < 1.5 ? 1 : m < 3.5 ? 2 : m < 7.5 ? 5 : 10) * power;
    }

    private static string Format(double value, double step)
    {
        var decimals = Math.Max(0, (int)Math.Ceiling(-Math.Log10(step) - 1e-9));
        return value.ToString("F" + Math.Min(decimals, 6), CultureInfo.CurrentCulture);
    }

    // ── Working it out again ─────────────────────────────────────────────────

    /// <summary>Changes one setting as the learner would (for the tests): the name of the property and its text as a task writes it.</summary>
    public void Set(string property, string text)
    {
        _now[property] = text;
        Recalculate();
    }

    /// <summary>Puts every setting back as the example started.</summary>
    public void Reset()
    {
        _resetting = true;
        try { foreach (var r in _resets) r(); }
        finally { _resetting = false; }
        foreach (var (k, v) in _start) _now[k] = v;
        _later.Stop();
        Recalculate();
    }

    private void Changed(bool soon)
    {
        _later.Stop();
        if (soon) _later.Start();
        else Recalculate();
    }

    /// <summary>Said after every value that cannot be used: what the learner still sees is not their change.</summary>
    private const string Stale = "The figure and the results show the last values that could be worked out.";

    private void Recalculate()
    {
        // Read what was typed first, so a mistyped value is named in the learner's words, never the analysis's (UX review 2026-11-17, M2).
        var settings = new Dictionary<string, string>(_now, StringComparer.Ordinal);
        if (Current is { } last)
            foreach (var (input, property) in _spec.Resolve(last.Analysis))
            {
                var kind = input.IsColumn ? TryInputKind.Values : property is null ? TryInputKind.Unsupported : TrySpec.KindOf(property);
                var key = input.IsColumn ? input.Name : property?.XmlName;
                var label = input.IsColumn ? input.TableColumn.Column : property?.Label ?? input.Name;
                if (key is null || !_now.TryGetValue(key, out var typed)) continue;
                if (kind == TryInputKind.Values)
                {
                    if (ReadValues(typed, out var values) is { } wrong) { Say($"{label}, {wrong} {Stale}"); return; }
                    if (values.Length == 0) { Say($"{label}: type at least one value, one per line. {Stale}"); return; }
                    settings[key] = string.Join('\n', values.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
                }
                else if (kind is TryInputKind.Number or TryInputKind.Whole && !input.HasRange)
                {
                    if (typed.Trim().Length == 0 || typed == "NaN")
                    {
                        // A setting that starts blank (no alternative mean, so no power) may be left blank.
                        if (_start.TryGetValue(key, out var first) && first == "NaN") { settings[key] = "NaN"; continue; }
                        Say($"{label}: type a number. {Stale}");
                        return;
                    }
                    if (!TryNumber(typed, out var number)) { Say($"{label}: \"{typed.Trim()}\" is not a number. Type a number such as 5 or 2.5. {Stale}"); return; }
                    settings[key] = number.ToString("R", CultureInfo.InvariantCulture);
                }
            }
        BuiltTask built;
        try { built = TrySpec.Build(_task, settings); }
        catch (ContentException ex)
        {
            Say(Plain(ex.Message) + " " + Stale);
            return;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FormatException or OverflowException)
        {
            Say("That value cannot be worked out: " + ex.Message + " " + Stale);
            return;
        }
        Current = built;
        if (built.Analysis is IReportsProblem { Problem: { Length: > 0 } problem }) Say(problem);
        else _problem.Visibility = Visibility.Collapsed;

        if (built.Analysis is IHasFigures drawable && _figures.Count > 0)
        {
            var all = drawable.Figures(null);
            foreach (var panel in _figures)
                if (panel.Tag is int k && k < all.Count) panel.Show(all[k]);
        }
        ShowResults(built);
    }

    /// <summary>A task's message without the file name it starts with (the learner did not write the file).</summary>
    private static string Plain(string message)
    {
        var colon = message.IndexOf(": ", StringComparison.Ordinal);
        return "That value cannot be used. " + (colon > 0 && message[..colon].Contains(".json", StringComparison.Ordinal) ? message[(colon + 2)..] : message);
    }

    private void Say(string text)
    {
        _problem.Text = text;
        _problem.Visibility = Visibility.Visible;
        Announce(_problem);
    }

    /// <summary>Tells a screen reader that a live region changed (the problem line, the results).</summary>
    private static void Announce(UIElement element) =>
        System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(element)?
            .RaiseAutomationEvent(System.Windows.Automation.Peers.AutomationEvents.LiveRegionChanged);

    private void ShowResults(BuiltTask built)
    {
        if (_spec.Results.Count == 0)
        {
            _results.Content = TaskWorkspace.ResultsGrid(built);
            Announce(_results);
            return;
        }
        var properties = built.Analysis.Properties().ToDictionary(p => p.XmlName, StringComparer.Ordinal);
        var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 120 });
        var row = 0;
        foreach (var name in _spec.Results)
        {
            if (!properties.TryGetValue(name, out var property)) continue;
            var shown = built.Analysis.Display(property);
            var unit = UnitSystem.BaseUnit(property.Unit).Label;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock { Text = property.Label, Margin = new Thickness(0, 2, 12, 2), TextWrapping = TextWrapping.Wrap, ToolTip = property.Help.Length > 0 ? property.Help : null };
            var value = new TextBlock
            {
                Text = shown.Length == 0 ? "—" : shown + (unit.Length > 0 && shown != "—" ? " " + unit : ""),
                Margin = new Thickness(0, 2, 0, 2), FontFamily = new FontFamily("Consolas"), FontWeight = FontWeights.SemiBold,
            };
            AutomationProperties.SetName(value, $"{property.Label}: {value.Text}");
            Grid.SetRow(label, row); Grid.SetRow(value, row); Grid.SetColumn(value, 1);
            grid.Children.Add(label); grid.Children.Add(value);
            row++;
        }
        AutomationProperties.SetName(grid, "Results");
        _results.Content = grid;
        Announce(_results);
    }
}
