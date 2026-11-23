using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Learning.Core;
using Learning.Core.Content;

namespace Statistle.App.Learning;

/// <summary>
/// Shows a lesson page (Learn and Reference tabs, a module's page, My progress) in the program's own window, as PLC+ does
/// (no web view). Ported from PLC+'s MarkdownView, itself from Gamify+'s: headings, paragraphs, bullet and numbered lists,
/// tables, fenced code blocks, bold, italics, `code` and https links.
/// <para>
/// A line of its own <c>[[name: argument]]</c> is a widget (<c>[[figure: task 1]]</c>, <c>[[data: task]]</c>,
/// <c>[[results: task]]</c>, <c>[[code: task]]</c>). This view does not know any widget: it hands each one to
/// <see cref="WidgetFactory"/>, which the main window sets, and shows the block it returns. A widget the factory cannot draw
/// (it returns null, or there is no factory) shows a small grey note instead.
/// </para>
/// Set <see cref="WidgetFactory"/> first, then <see cref="Markdown"/>; or call <see cref="Build"/> for a document on its own.
/// </summary>
public sealed partial class MarkdownView : FlowDocumentScrollViewer
{
    public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register(
        nameof(Markdown), typeof(string), typeof(MarkdownView),
        new PropertyMetadata(string.Empty, OnMarkdownChanged));

    public MarkdownView()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        IsToolBarVisible = false;
        Document = new FlowDocument();
    }

    /// <summary>
    /// Draws a widget line: return the block to show (wrap a control with <see cref="Wrap"/>), or null when it cannot be drawn.
    /// Setting it redraws the page.
    /// </summary>
    public Func<WidgetTag, Block?>? WidgetFactory
    {
        get => _widgetFactory;
        set
        {
            _widgetFactory = value;
            if (!string.IsNullOrEmpty(Markdown)) Render(Markdown);
        }
    }

    private Func<WidgetTag, Block?>? _widgetFactory;

    /// <summary>The page's Markdown. Setting it draws the page.</summary>
    public string Markdown
    {
        get => (string)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    /// <summary>A control as a block of the page, with the space a widget has above and below it.</summary>
    public static Block Wrap(UIElement element) => new BlockUIContainer(element) { Margin = new Thickness(0, 4, 0, 12) };

    /// <summary>The small grey note shown in place of a widget that cannot be drawn.</summary>
    public static Block CannotDraw(WidgetTag widget)
    {
        ArgumentNullException.ThrowIfNull(widget);
        return new Paragraph(new Run($"(This page shows a {widget.Name} here that {LearningApp.Name} cannot draw: {widget.Argument})"))
        {
            FontSize = 12,
            Foreground = Brushes.DimGray,
            FontStyle = FontStyles.Italic,
            Margin = new Thickness(0, 2, 0, 10),
        };
    }

    [GeneratedRegex(@"^\s*(```|~~~)")]
    private static partial Regex FencePattern();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex BoldPattern();

    [GeneratedRegex(@"`(.+?)`")]
    private static partial Regex CodePattern();

    [GeneratedRegex(@"(?<![*\w])\*(?!\s)(.+?)(?<!\s)\*(?![*\w])")]
    private static partial Regex ItalicPattern();

    /// <summary>A numbered step: "1. Press Add game". The guidance pages are written as steps.</summary>
    [GeneratedRegex(@"^(\d{1,2})[.)]\s+(.*)$")]
    private static partial Regex NumberedPattern();

    private static void OnMarkdownChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
        ((MarkdownView)sender).Render((string)(e.NewValue ?? string.Empty));

    private void Render(string markdown)
    {
        _blocks.Clear();
        _marked.Clear();
        var document = Build(markdown, _widgetFactory, _blocks);
        MarkTerms();
        Document = document;
    }

    // ── My notes (Learning Core plan 3.2; plan N.1, Ron 2026-11-23) ─────────
    // Each paragraph, list entry and heading keeps the words it was made from, so a note can be put on it and found again by
    // its words (Learning.Core.Notes.BlockText). Tables and widgets take no notes of their own. From PythonProgramming+.

    private readonly List<(Paragraph Block, string Source)> _blocks = [];
    private readonly List<(Paragraph Block, Inline Mark)> _marked = [];

    /// <summary>The page's blocks' source words, in order: what <see cref="Learning.Core.Notes.NoteBook.Place"/> needs.</summary>
    public IReadOnlyList<string> BlockSources => [.. _blocks.Select(b => b.Source)];

    /// <summary>The learner asked to write a note: on a block (its index in <see cref="BlockSources"/>), or on the page (null).</summary>
    public event Action<int?>? NoteRequested;

    /// <summary>The learner clicked the ✎ after a block: show its notes (the block's index in <see cref="BlockSources"/>).</summary>
    public event Action<int>? NoteMarkClicked;

    private int? _menuBlock;

    /// <summary>On the pages a learner can write notes on (Learn and Reference), the right-click menu offers it.</summary>
    public void EnableNotes()
    {
        var here = new MenuItem { Header = "Add a _note to this paragraph…" };
        var page = new MenuItem { Header = "Add a note to this _page…", ToolTip = "Write your own note on this page as a whole. Only you see it." };
        var copy = new MenuItem { Header = "_Copy", Command = ApplicationCommands.Copy, ToolTip = "Copies the words you selected" };
        ToolTipService.SetShowOnDisabled(here, true);
        here.Click += (_, _) => NoteRequested?.Invoke(_menuBlock);
        page.Click += (_, _) => NoteRequested?.Invoke(null);
        ContextMenu = new ContextMenu();
        foreach (var part in new Control[] { here, page, new Separator(), copy }) ContextMenu.Items.Add(part);
        ToolTipService.SetShowOnDisabled(page, true);
        ContextMenuOpening += (_, e) =>
        {
            _menuBlock = BlockAt(e.OriginalSource) ?? BlockAt(Selection?.Start.Paragraph);
            var signedIn = CanNote?.Invoke() ?? true;
            here.IsEnabled = signedIn && _menuBlock is not null;
            page.IsEnabled = signedIn;
            here.ToolTip = !signedIn ? SignInFirst
                : _menuBlock is null ? "Right-click on a paragraph (not a table or a figure) to put a note on it"
                : "Write your own note on this paragraph. Only you see it; it is kept in your progress file.";
            page.ToolTip = signedIn ? "Write your own note on this page as a whole. Only you see it." : SignInFirst;
        };
    }

    private const string SignInFirst = "Sign in first (Learner ▸ Sign in): notes are kept in your own progress file";

    /// <summary>Whether a note can be written now (someone is signed in); the menu items are off, with the reason, otherwise.</summary>
    public Func<bool>? CanNote { get; set; }

    /// <summary>The block a click landed in (a run, a bold word, a link), or null if it is not one that takes notes.</summary>
    private int? BlockAt(object? source)
    {
        for (var element = source as DependencyObject; element is not null; element = LogicalTreeHelper.GetParent(element))
            if (element is Paragraph p && _blocks.FindIndex(b => b.Block == p) is var i and >= 0) return i;
        return null;
    }

    /// <summary>
    /// Shows where the learner's notes are (Ron, 2026-11-15: "a small note icon at the end of the paragraph is sufficient"): a ✎
    /// after the last word of each block with a note (✎ 2 for two), the notes as its tooltip, and a click on it opens them on the
    /// My notes tab. Clears the last call's marks first.
    /// </summary>
    public void MarkNotes(IEnumerable<(int Index, string Tip)> notes)
    {
        foreach (var (block, mark) in _marked) block.Inlines.Remove(mark);
        _marked.Clear();
        foreach (var group in notes.Where(n => n.Index >= 0 && n.Index < _blocks.Count).GroupBy(n => n.Index))
        {
            var block = _blocks[group.Key].Block;
            var count = group.Count();
            var index = group.Key;
            var link = new Hyperlink(new Run(count == 1 ? " ✎" : $" ✎ {count}"))
            {
                TextDecorations = null,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                ToolTip = string.Join(Environment.NewLine, group.Select(n => "✎ My note: " + n.Tip))
                          + Environment.NewLine + "Click ✎ to open it on the My notes tab.",
            };
            link.Foreground = LearningTheme.Brush("AccentBrush");
            System.Windows.Automation.AutomationProperties.SetName(link, count == 1 ? "My note on this paragraph" : $"My {count} notes on this paragraph");
            link.Click += (_, _) => NoteMarkClicked?.Invoke(index);
            block.Inlines.Add(link);
            _marked.Add((block, link));
        }
    }

    /// <summary>The ✎ marks on the page now (for the tests).</summary>
    public int NoteMarkCount => _marked.Count;

    /// <summary>Scrolls a block into view and selects it, so the learner sees which paragraph a note is on.</summary>
    public void ShowBlock(int index)
    {
        if (index < 0 || index >= _blocks.Count) return;
        var block = _blocks[index].Block;
        block.BringIntoView();
        Selection?.Select(block.ContentStart, block.ContentEnd);
        Focus();
    }

    // ── Key terms (Learning Core plan 3.13; Q17) ─────────────────────────────

    /// <summary>The book's glossary: the first use of each key term on the page is marked, with its definition as a tooltip.</summary>
    public Glossary? Glossary { get; set; }

    /// <summary>Which terms to mark (an item's own key terms); null marks every one.</summary>
    public Func<GlossaryEntry, bool>? MarkOnly { get; set; }

    /// <summary>A marked key term was clicked: the window shows it in the Glossary.</summary>
    public event Action<GlossaryEntry>? TermClicked;

    /// <summary>The key terms marked on the page now (for the tests).</summary>
    public int TermCount { get; private set; }

    private static readonly TextDecorationCollection Dotted = MakeDotted();

    private static TextDecorationCollection MakeDotted()
    {
        var pen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyles.Dot };
        pen.Freeze();
        var decorations = new TextDecorationCollection { new TextDecoration(TextDecorationLocation.Underline, pen, 1, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended) };
        decorations.Freeze();
        return decorations;
    }

    /// <summary>
    /// Marks the first use of each key term on the page: a dotted underline (a shape, not colour alone), the definition as its
    /// tooltip, and a click to the Glossary. Words in code, links and headings are left alone.
    /// </summary>
    private void MarkTerms()
    {
        TermCount = 0;
        if (Glossary is not { Entries.Count: > 0 } glossary) return;
        var done = new HashSet<GlossaryEntry>();
        foreach (var (block, _) in _blocks)
        {
            if (block.FontSize > 14.5) continue; // a heading
            foreach (var run in RunsIn(block.Inlines).ToList())
            {
                var matches = glossary.Matches(run.Text).Where(m => (MarkOnly?.Invoke(m.Entry) ?? true) && done.Add(m.Entry)).ToList();
                if (matches.Count == 0) continue;
                var parent = (run.Parent as Span)?.Inlines ?? (run.Parent as Paragraph)?.Inlines;
                if (parent is null) continue;
                var text = run.Text;
                var at = 0;
                foreach (var m in matches)
                {
                    if (m.Start > at) parent.InsertBefore(run, Copy(run, text[at..m.Start]));
                    var term = new Hyperlink(Copy(run, text.Substring(m.Start, m.Length)))
                    {
                        TextDecorations = Dotted, Cursor = Cursors.Help,
                        ToolTip = $"Key term · {m.Entry.Term}: {m.Entry.Definition}{Environment.NewLine}Click to see it in the Glossary.",
                    };
                    term.Foreground = LearningTheme.Brush("InkBrush");
                    System.Windows.Automation.AutomationProperties.SetName(term, $"Key term {m.Entry.Term}: {m.Entry.Definition}");
                    var entry = m.Entry;
                    term.Click += (_, _) => TermClicked?.Invoke(entry);
                    parent.InsertBefore(run, term);
                    at = m.Start + m.Length;
                    TermCount++;
                }
                if (at < text.Length) parent.InsertBefore(run, Copy(run, text[at..]));
                parent.Remove(run);
            }
        }
    }

    private static Run Copy(Run like, string text) => new(text) { FontWeight = like.FontWeight, FontStyle = like.FontStyle };

    /// <summary>The plain runs of a block, not those in a link or set as code.</summary>
    private static IEnumerable<Run> RunsIn(InlineCollection inlines)
    {
        foreach (var inline in inlines)
        {
            if (inline is Run run && run.FontFamily.Source != "Consolas") yield return run;
            else if (inline is Span span and not Hyperlink)
                foreach (var inner in RunsIn(span.Inlines)) yield return inner;
        }
    }

    /// <summary>The page as a document: the same drawing the view does, for a page shown or printed elsewhere.</summary>
    /// <param name="blocks">Filled with each paragraph, list entry and heading and the words it was made from (for notes).</param>
    public static FlowDocument Build(string markdown, Func<WidgetTag, Block?>? widgetFactory, List<(Paragraph Block, string Source)>? blocks = null)
    {
        Paragraph Noted(Paragraph block, string source)
        {
            blocks?.Add((block, source));
            return block;
        }

        ArgumentNullException.ThrowIfNull(markdown);
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(18, 14, 18, 24),
            LineHeight = 20,

            // A FlowDocument justifies by default, which opens rivers of white space in a narrow
            // column and makes a page of instructions harder to read than it needs to be.
            TextAlignment = TextAlignment.Left,
        };

        string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var paragraph = new List<string>();
        var bullets = new List<string>();
        var numbered = new List<string>();
        var table = new List<string>();
        List<string>? code = null; // inside a ``` fence: its lines, as written

        void FlushParagraph()
        {
            if (paragraph.Count > 0)
            {
                var source = string.Join(' ', paragraph);
                document.Blocks.Add(Noted(Text(source, 0, 0, 8), source));
                paragraph.Clear();
            }
        }

        void FlushBullets()
        {
            if (bullets.Count == 0)
            {
                return;
            }

            var list = new List { MarkerStyle = TextMarkerStyle.Disc, Margin = new Thickness(18, 0, 0, 8) };

            foreach (string bullet in bullets)
            {
                list.ListItems.Add(new ListItem(Noted(Text(bullet, 0, 0, 2), bullet)));
            }

            document.Blocks.Add(list);
            bullets.Clear();
        }

        // Steps are numbered for a reason — "do 3 before 4" — so they are rendered as a numbered
        // list rather than folded into one paragraph, which is what happened before.
        void FlushNumbered()
        {
            if (numbered.Count == 0)
            {
                return;
            }

            var list = new List { MarkerStyle = TextMarkerStyle.Decimal, Margin = new Thickness(18, 0, 0, 8) };

            foreach (string step in numbered)
            {
                list.ListItems.Add(new ListItem(Noted(Text(step, 0, 0, 4), step)));
            }

            document.Blocks.Add(list);
            numbered.Clear();
        }

        void FlushTable()
        {
            if (table.Count == 0)
            {
                return;
            }

            document.Blocks.Add(BuildTable(table));
            table.Clear();
        }

        foreach (string raw in lines)
        {
            string line = raw.TrimEnd();

            // A fenced code block (```python … ```) is shown as written, in a fixed-width font: nothing in it is Markdown.
            if (code is not null)
            {
                if (FencePattern().IsMatch(line))
                {
                    document.Blocks.Add(CodeBlock(code));
                    code = null;
                }
                else
                {
                    code.Add(line);
                }

                continue;
            }

            if (FencePattern().IsMatch(line))
            {
                FlushParagraph();
                FlushBullets();
                FlushNumbered();
                FlushTable();
                code = [];
                continue;
            }

            if (line.TrimStart().StartsWith('|'))
            {
                FlushParagraph();
                FlushBullets();
                FlushNumbered();
                table.Add(line.Trim());
                continue;
            }

            FlushTable();

            if (Widgets.On(line) is { } widget)
            {
                FlushParagraph();
                FlushBullets();
                FlushNumbered();
                Block? part;
                try
                {
                    part = widgetFactory?.Invoke(widget);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or FormatException or ContentException)
                {
                    part = null; // a widget that fails is a note on the page, never a crash of the page
                }

                document.Blocks.Add(part ?? CannotDraw(widget));
                continue;
            }

            if (line.Trim().Length == 0)
            {
                FlushParagraph();
                FlushBullets();
                FlushNumbered();
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
            {
                FlushParagraph();
                FlushNumbered();
                bullets.Add(line[2..]);
                continue;
            }

            if (NumberedPattern().Match(line) is { Success: true } step)
            {
                FlushParagraph();
                FlushBullets();
                numbered.Add(step.Groups[2].Value);
                continue;
            }

            // A step that runs onto the next line belongs to that step, as with a bullet.
            if (numbered.Count > 0 && paragraph.Count == 0 && bullets.Count == 0 && raw.StartsWith(' '))
            {
                numbered[^1] += " " + line.Trim();
                continue;
            }

            // A bullet that runs onto the next line belongs to that bullet, not to a paragraph of
            // its own — otherwise the second half of a bullet is printed before the list it is in.
            if (bullets.Count > 0 && paragraph.Count == 0 && raw.StartsWith(' '))
            {
                bullets[^1] += " " + line.Trim();
                continue;
            }

            if (line.StartsWith('#'))
            {
                FlushParagraph();
                FlushNumbered();
                FlushBullets();

                int level = line.TakeWhile(c => c == '#').Count();
                Paragraph heading = Text(line[level..].Trim(),
                    level switch { 1 => 21, 2 => 17, _ => 15 }, level == 1 ? 0 : 14, 6);
                heading.FontWeight = FontWeights.SemiBold;
                document.Blocks.Add(Noted(heading, line[level..].Trim()));
                continue;
            }

            paragraph.Add(line.Trim());
        }

        FlushParagraph();
        FlushBullets();
        FlushNumbered();
        FlushTable();
        if (code is not null)
        {
            document.Blocks.Add(CodeBlock(code)); // a fence never closed: the code is still shown
        }

        return document;
    }

    private static Paragraph CodeBlock(IReadOnlyList<string> lines)
    {
        var paragraph = new Paragraph
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13,
            LineHeight = 18,
            Background = new SolidColorBrush(Color.FromRgb(0xF2, 0xF4, 0xF6)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xD5, 0xDB, 0xE1)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(0, 2, 0, 12),
        };

        for (int i = 0; i < lines.Count; i++)
        {
            if (i > 0)
            {
                paragraph.Inlines.Add(new LineBreak());
            }

            paragraph.Inlines.Add(new Run(lines[i]));
        }

        return paragraph;
    }

    /// <summary>A table row's cells. A bar inside `code` (such as `x | y`) or written \| (such as P(A \| B)) stays in its cell.</summary>
    public static string[] SplitRow(string row)
    {
        var cells = new List<string>();
        var cell = new System.Text.StringBuilder();
        var inCode = false;
        for (int i = 0; i < row.Length; i++)
        {
            char c = row[i];
            if (c == '\\' && i + 1 < row.Length && row[i + 1] == '|')
            {
                cell.Append('|');   // "\|" is a bar inside a cell, as in P(A \| B)
                i++;
                continue;
            }

            if (c == '`') inCode = !inCode;
            if (c == '|' && !inCode)
            {
                cells.Add(cell.ToString().Trim());
                cell.Clear();
                continue;
            }
            cell.Append(c);
        }
        cells.Add(cell.ToString().Trim());
        return [.. cells];
    }

    private static Table BuildTable(IReadOnlyList<string> rows)
    {
        // The line of dashes under a Markdown header row is a rule, not data.
        List<string[]> cells = [.. rows
            .Select(row => SplitRow(row.Trim().Trim('|')))
            .Where(row => !row.All(c => c.Length > 0 && c.All(ch => ch is '-' or ':')))];

        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 2, 0, 12) };
        int columns = cells.Count == 0 ? 1 : cells.Max(r => r.Length);

        for (int i = 0; i < columns; i++)
        {
            // A first column of short words (symbols, names) is narrow; one of longer text is as wide as the rest.
            bool shortFirst = cells.Skip(1).All(r => r.Length == 0 || r[0].Length <= 16);
            table.Columns.Add(new TableColumn { Width = new GridLength(i == 0 && shortFirst ? 1 : 2, GridUnitType.Star) });
        }

        var group = new TableRowGroup();
        bool first = true;

        foreach (string[] row in cells)
        {
            var tableRow = new TableRow();

            for (int i = 0; i < columns; i++)
            {
                Paragraph content = Text(i < row.Length ? row[i] : string.Empty, 0, 0, 0);
                content.FontWeight = first ? FontWeights.SemiBold : FontWeights.Normal;

                tableRow.Cells.Add(new TableCell(content)
                {
                    Padding = new Thickness(8, 5, 8, 5),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xD5, 0xDB, 0xE1)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                });
            }

            group.Rows.Add(tableRow);
            first = false;
        }

        table.RowGroups.Add(group);
        return table;
    }

    /// <summary>One paragraph, with <c>**bold**</c> and <c>`code`</c> turned into runs.</summary>
    private static Paragraph Text(string markdown, double size, double above, double below)
    {
        var paragraph = new Paragraph { Margin = new Thickness(0, above, 0, below) };

        if (size > 0)
        {
            paragraph.FontSize = size;
        }

        foreach (Inline inline in Inlines(markdown))
        {
            paragraph.Inlines.Add(inline);
        }

        return paragraph;
    }

    private static IEnumerable<Inline> Inlines(string markdown)
    {
        int at = 0;

        foreach (Match match in BoldPattern().Matches(markdown).Cast<Match>())
        {
            if (match.Index > at)
            {
                foreach (Inline inline in Code(markdown[at..match.Index]))
                {
                    yield return inline;
                }
            }

            yield return new Run(match.Groups[1].Value) { FontWeight = FontWeights.SemiBold };
            at = match.Index + match.Length;
        }

        foreach (Inline inline in Code(markdown[at..]))
        {
            yield return inline;
        }
    }

    private static IEnumerable<Inline> Code(string text)
    {
        int at = 0;

        foreach (Match match in CodePattern().Matches(text).Cast<Match>())
        {
            if (match.Index > at)
            {
                foreach (Inline inline in Italics(text[at..match.Index]))
                {
                    yield return inline;
                }
            }

            yield return new Run(match.Groups[1].Value) { FontFamily = new FontFamily("Consolas"), Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xF1, 0xF4)) };
            at = match.Index + match.Length;
        }

        if (at < text.Length)
        {
            foreach (Inline inline in Italics(text[at..]))
            {
                yield return inline;
            }
        }
    }

    private static IEnumerable<Inline> Italics(string text)
    {
        int at = 0;

        foreach (Match match in ItalicPattern().Matches(text).Cast<Match>())
        {
            foreach (Inline inline in Links(text[at..match.Index]))
            {
                yield return inline;
            }

            yield return new Run(match.Groups[1].Value) { FontStyle = FontStyles.Italic };
            at = match.Index + match.Length;
        }

        foreach (Inline inline in Links(text[at..]))
        {
            yield return inline;
        }
    }

    /// <summary>A web address written out in a page (the screen help ends with one) becomes a link that opens the browser.</summary>
    private static IEnumerable<Inline> Links(string text)
    {
        int at = 0;

        foreach (Match match in LinkPattern().Matches(text).Cast<Match>())
        {
            if (match.Index > at)
            {
                yield return new Run(text[at..match.Index]);
            }

            var link = new Hyperlink(new Run(match.Value)) { NavigateUri = new Uri(match.Value) };
            link.RequestNavigate += (_, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true })?.Dispose();
                }
                catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
                {
                    MessageBox.Show($"Windows could not open a web browser. The address is {e.Uri.AbsoluteUri}",
                        $"{LearningApp.Name} — Open a web page", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                e.Handled = true;
            };
            yield return link;
            at = match.Index + match.Length;
        }

        if (at < text.Length)
        {
            yield return new Run(text[at..]);
        }
    }

    /// <summary>An https address, without the full stop or bracket that ends the sentence it is in.</summary>
    [GeneratedRegex(@"https://[^\s)]+[^\s).,;:]")]
    private static partial Regex LinkPattern();
}
