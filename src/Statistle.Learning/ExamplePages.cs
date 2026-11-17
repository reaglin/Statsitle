using System.Text.RegularExpressions;

namespace Statistle.Learning;

/// <summary>
/// The Example tab's written examples (Learning Core plan 3.22, the pattern for every PlusBook; Ron, 2026-11-17: "the Try it goes
/// into the Example section and always contains some type of figure … if there are multiple larger examples, we can add tabs for
/// Example 1, Example 2"). An item's file holds them after its Learn page, each starting with a line of its own:
/// <code>
/// [[example: Four days of rejects, and a bad one]]
/// What to try, then the interactive example …
/// [[try: st.m2.central-tendency.try Sample | Mean Median]]
/// </code>
/// A section runs to the next <c>[[example: …]]</c> line or the end of the file. Everything before the first is the Learn page. On
/// the Learn page, <c>[[see-example: 1]]</c> is a button that opens Example 1. A copy of PLC+'s <c>Plc.Grading.ExamplePages</c>,
/// same syntax, so the two can move into the Learning Core together (its plan 3.22).
/// </summary>
public static partial class ExamplePages
{
    /// <summary>One written example: its title and its Markdown (the widgets in it included).</summary>
    public sealed record Page(string Title, string Markdown);

    /// <summary>The widgets that count as an example's figure: something to look at or to move (not the code).</summary>
    public static readonly IReadOnlyList<string> FigureWidgets = ["try", "figure", "data", "results"];

    [GeneratedRegex(@"^\s*\[\[\s*example\s*:\s*(?<title>.*?)\s*\]\]\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex Start();

    /// <summary>The Learn page and the written examples of an item's Markdown.</summary>
    public static (string Learn, IReadOnlyList<Page> Examples) Split(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var learn = new List<string>();
        var pages = new List<Page>();
        string? title = null;
        var body = new List<string>();
        void Close()
        {
            if (title is not null) pages.Add(new Page(title, string.Join("\n", body).Trim() + "\n"));
            body.Clear();
        }
        foreach (var line in lines)
        {
            var m = Start().Match(line);
            if (m.Success)
            {
                Close();
                title = m.Groups["title"].Value;
                continue;
            }
            (title is null ? learn : body).Add(line);
        }
        Close();
        return (string.Join("\n", learn).TrimEnd() + "\n", pages);
    }
}
