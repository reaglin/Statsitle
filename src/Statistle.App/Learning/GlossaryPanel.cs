using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Learning.Core.Content;

namespace Statistle.App.Learning;

/// <summary>
/// The Glossary page in the path (Learning Core plan 3.13): every key term, A to Z, its definition, and the items that use
/// it, each opening the item. A search box finds a term by its words or its definition's.
/// </summary>
public sealed class GlossaryPanel : UserControl
{
    private readonly TextBox _search = new() { Width = 480, HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(4, 3, 4, 3) };
    private readonly TextBlock _found = new();
    private readonly StackPanel _list = new();
    private readonly Dictionary<GlossaryEntry, FrameworkElement> _rows = [];
    private ContentPackage? _package;
    private IReadOnlyDictionary<GlossaryEntry, IReadOnlyList<LearningItem>> _used = new Dictionary<GlossaryEntry, IReadOnlyList<LearningItem>>();

    private readonly Button _back = new();

    /// <summary>Go back to the item and tab the learner came from.</summary>
    public Action Back { get; set; } = () => { };

    /// <summary>Shows "◂ Back to 5.2 (Learn)" when the learner came from an item, or hides it.</summary>
    public void SetBack(string? label)
    {
        _back.Content = label;
        _back.ToolTip = label is null ? null : "Goes back to the item and tab you were reading";
        _back.Visibility = label is null ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>Open an item where a term is used.</summary>
    public Action<LearningItem, GlossaryEntry> Open { get; set; } = (_, _) => { };

    public GlossaryPanel()
    {
        var top = new StackPanel();
        var title = new TextBlock { Text = "Glossary", TextWrapping = TextWrapping.Wrap };
        title.SetResourceReference(StyleProperty, "SectionTitle");
        AutomationProperties.SetHeadingLevel(title, AutomationHeadingLevel.Level1);
        top.Children.Add(title);
        var lead = new TextBlock { Text = "The book's key terms, A to Z, with what each means and the items that use it. In a lesson, a key term " +
            "has a dotted underline the first time a page uses it: point at it to read its meaning, click it to come here. Press an item number under Used in to open that item." };
        lead.SetResourceReference(StyleProperty, "Lead");
        top.Children.Add(lead);
        _back.Visibility = Visibility.Collapsed;
        _back.HorizontalAlignment = HorizontalAlignment.Left;
        _back.Margin = new Thickness(0, 0, 0, 8);
        _back.Click += (_, _) => Back();
        top.Children.Add(_back);
        var label = new TextBlock { Text = "Find a term", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 2) };
        top.Children.Add(label);
        _search.ToolTip = "Type part of a term or of its meaning. Empty shows every term.";
        AutomationProperties.SetName(_search, "Find a term");
        _search.TextChanged += (_, _) => Draw();
        top.Children.Add(_search);
        _found.SetResourceReference(StyleProperty, "Lead");
        _found.Margin = new Thickness(0, 4, 0, 6);
        AutomationProperties.SetLiveSetting(_found, AutomationLiveSetting.Polite);
        top.Children.Add(_found);
        var dock = new DockPanel();
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);
        dock.Children.Add(new ScrollViewer { Content = _list, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Focusable = false });
        Content = dock;
        AutomationProperties.SetName(_list, "Key terms");
    }

    public void Load(ContentPackage package)
    {
        _package = package;
        _used = package.Glossary.UsedIn(package);
        Draw();
    }

    /// <summary>Shows one term: the search is cleared, and the term is marked and scrolled to.</summary>
    public void ShowTerm(GlossaryEntry entry)
    {
        _search.Text = "";
        Draw(entry);
        if (_rows.TryGetValue(entry, out var row)) Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () => row.BringIntoView());
    }

    public void FocusSearch() => _search.Focus();

    private void Draw(GlossaryEntry? marked = null)
    {
        _list.Children.Clear();
        _rows.Clear();
        if (_package is null) return;
        var all = _package.Glossary.Entries;
        var query = _search.Text.Trim();
        var shown = query.Length == 0 ? all : [.. all.Where(e => (e.Term + " " + string.Join(" ", e.Also) + " " + e.Definition).Contains(query, StringComparison.CurrentCultureIgnoreCase))];
        _found.Text = all.Count == 0 ? "This book has no glossary yet."
            : query.Length == 0 ? $"{all.Count} key terms."
            : shown.Count == 0 ? $"No term has “{query}”. Try fewer letters." : $"{shown.Count} of {all.Count} terms have “{query}”.";
        foreach (var e in shown)
        {
            var panel = new StackPanel();
            if (e == marked) panel.Children.Add(new TextBlock { Text = "▶ The term you chose", FontSize = 12, FontWeight = FontWeights.SemiBold });
            panel.Children.Add(new TextBlock { Text = e.Term, FontSize = 16, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            if (e.Also.Count > 0)
            {
                var also = new TextBlock { Text = "Also: " + string.Join(", ", e.Also), FontSize = 12, TextWrapping = TextWrapping.Wrap };
                also.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                panel.Children.Add(also);
            }
            panel.Children.Add(new TextBlock { Text = e.Definition, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 4) });
            var items = _used.GetValueOrDefault(e) ?? [];
            if (items.Count > 0)
            {
                var row = new WrapPanel();
                var usedIn = new TextBlock { Text = "Used in:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0), FontSize = 12 };
                usedIn.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                row.Children.Add(usedIn);
                foreach (var item in items)
                {
                    var b = new Button { Content = item.Number, ToolTip = $"Opens {item.Number} {item.Title}", Padding = new Thickness(8, 1, 8, 1), Margin = new Thickness(0, 0, 4, 2) };
                    AutomationProperties.SetName(b, $"Open {item.Number} {item.Title}");
                    var entry = e;
                    b.Click += (_, _) => Open(item, entry);
                    row.Children.Add(b);
                }
                panel.Children.Add(row);
            }
            var card = new Border
            {
                Child = panel, Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = e == marked ? new Thickness(6, 3, 3, 3) : new Thickness(1),
            };
            card.SetResourceReference(Border.BorderBrushProperty, e == marked ? "AccentBrush" : "LineBrush");
            card.SetResourceReference(Border.BackgroundProperty, "PageBrush");
            AutomationProperties.SetName(card, $"{e.Term}: {e.Definition}");
            _rows[e] = card;
            _list.Children.Add(card);
        }
    }
}
