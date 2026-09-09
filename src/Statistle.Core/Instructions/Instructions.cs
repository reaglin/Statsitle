using System.Text;
using Statistle.Core.Analyses;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;

namespace Statistle.Core.Instructions;

/// <summary>One piece of an instruction page. Writers render each kind; nothing else knows the layout.</summary>
public abstract record InstructionBlock;

public sealed record Heading(string Text) : InstructionBlock;

public sealed record Paragraph(string Text) : InstructionBlock;

/// <summary>An unordered list.</summary>
public sealed record Bullets(IReadOnlyList<string> Items) : InstructionBlock;

/// <summary>A numbered list — the steps to take in the tool.</summary>
public sealed record Steps(IReadOnlyList<string> Items) : InstructionBlock;

/// <summary>Term–explanation pairs — the entries of a form, the results, the methods.</summary>
public sealed record Definitions(IReadOnlyList<(string Term, string Text)> Items) : InstructionBlock;

/// <summary>The instructions for one item: what it is, when to use it, how, and what everything on its form means.</summary>
public sealed class InstructionSet
{
    public string Title { get; init; } = string.Empty;

    public string Subtitle { get; init; } = string.Empty;

    public IReadOnlyList<InstructionBlock> Blocks { get; init; } = [];
}

/// <summary>
/// The hand-written part of an item's instructions: the judgement a student needs that no
/// property carries — when the statistic is the right one, the order to do things in, how to
/// read what comes out, and what goes wrong. Everything else on the page is generated from the
/// metadata the item already declares.
/// </summary>
public sealed record Guide(
    string WhenToUse,
    IReadOnlyList<string> Steps,
    string ReadingResults,
    IReadOnlyList<string> Pitfalls,
    string Example,
    string Plots,
    string Code);

/// <summary>Builds the instruction page for an item type from its metadata and its guide.</summary>
public static class InstructionsBuilder
{
    /// <summary>The page for an item type — an analysis or the data table.</summary>
    public static InstructionSet For(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Guide guide = GuideCatalog.For(type) ?? throw new ArgumentException($"{type.Name} has no guide.", nameof(type));
        AnalysisDescriptor? descriptor = AnalysisCatalog.Discover(type.Assembly).FirstOrDefault(d => d.ToolType == type);
        string title = descriptor?.Title ?? (type == typeof(DataTable) ? "Data table" : type.Name);
        string subtitle = descriptor?.Summary ?? string.Empty;
        List<InstructionBlock> blocks = [];

        string overall = StatistleObject.HelpOf(type);

        if (overall.Length > 0)
        {
            blocks.Add(new Heading("What it is"));
            blocks.Add(new Paragraph(overall));
        }

        blocks.Add(new Heading("When to use it"));
        blocks.Add(new Paragraph(guide.WhenToUse));
        blocks.Add(new Heading("How to use it in Statistle"));
        blocks.Add(new Steps(guide.Steps));
        blocks.Add(new Heading("Reading the results"));
        blocks.Add(new Paragraph(guide.ReadingResults));

        if (guide.Pitfalls.Count > 0)
        {
            blocks.Add(new Heading("Watch out for"));
            blocks.Add(new Bullets(guide.Pitfalls));
        }

        if (guide.Example.Length > 0)
        {
            blocks.Add(new Heading("Worked example"));
            blocks.Add(new Paragraph(guide.Example));
        }

        IReadOnlyList<StatistleProperty> properties = StatistleObject.PropertiesOf(type);
        IReadOnlyList<ICalculationMethod> methods = MethodCatalog.For(type);
        HashSet<string> scoped = EverHidden(type);
        List<(string, string)> entries = [.. properties.Where(p => !p.IsResult && !p.Hidden && p.XmlName is not (nameof(StatistleObject.Id) or nameof(StatistleObject.Name) or nameof(StatistleObject.Description))).Select(p => (p.Label, Describe(p, methods, scoped)))];

        if (entries.Count > 0)
        {
            blocks.Add(new Heading("The entries"));
            blocks.Add(new Paragraph("Every field on the form, what it means and how to choose it. A field that belongs to one method only is shown while that method applies and hidden otherwise."));
            blocks.Add(new Definitions(entries));
        }

        List<(string, string)> results = [.. properties.Where(p => p.IsResult && !p.Hidden).Select(p => (p.Label, Describe(p, methods, scoped)))];

        if (results.Count > 0)
        {
            blocks.Add(new Heading("The results"));
            blocks.Add(new Paragraph("Every result the form can show, in the order it appears, with what it means."));
            blocks.Add(new Definitions(results));
        }

        if (methods.Count > 0)
        {
            blocks.Add(new Heading("Methods and sources"));
            blocks.Add(new Paragraph("The calculations behind the results, each with its formula in brief and the source it follows. The report cites whichever of these were in force."));
            blocks.Add(new Definitions([.. methods.Select(m => (m.Name, $"{m.Description} Source: {m.Citation}"))]));
        }

        if (guide.Plots.Length > 0)
        {
            blocks.Add(new Heading("The plots"));
            blocks.Add(new Paragraph(guide.Plots));
        }

        if (guide.Code.Length > 0)
        {
            blocks.Add(new Heading("The code"));
            blocks.Add(new Paragraph(guide.Code));
            blocks.Add(new Paragraph("Copy the script from the Code tab or save it as a .py or .R file, run it in Colab or R, and compare its printed lines with the Results column: every result is printed under the same label. The script belongs in the appendix of your report."));
        }

        return new InstructionSet { Title = title, Subtitle = subtitle, Blocks = blocks };
    }

    /// <summary>The page shown when nothing is selected: how Statistle is put together.</summary>
    public static InstructionSet Overview()
    {
        Guide guide = GuideCatalog.Overview;
        List<InstructionBlock> blocks =
        [
            new Heading("What it is"),
            new Paragraph(guide.WhenToUse),
            new Heading("How to use it"),
            new Steps(guide.Steps),
            new Heading("The five tabs"),
            new Paragraph(guide.ReadingResults),
            new Heading("Good habits"),
            new Bullets(guide.Pitfalls),
            new Heading("The sample project"),
            new Paragraph(guide.Example),
            new Heading("The analyses, in course order"),
            new Definitions([.. AnalysisCatalog.Discover(typeof(Analysis).Assembly).OrderBy(d => d.Order).Select(d => (d.Title, d.Summary + "."))]),
            new Heading("The code"),
            new Paragraph(guide.Code),
        ];

        return new InstructionSet { Title = "Statistle", Subtitle = "Statistics for engineers — how to use it", Blocks = blocks };
    }

    /// <summary>
    /// The properties a choice on the form can hide: each enum-valued design property is stepped
    /// through its values on a fresh instance and whatever <see cref="MethodScope"/> hides under any
    /// of them is collected. Those are the entries worth saying "shown with" about.
    /// </summary>
    private static HashSet<string> EverHidden(Type type)
    {
        HashSet<string> hidden = [];

        if (Activator.CreateInstance(type) is not StatistleObject subject)
        {
            return hidden;
        }

        foreach (StatistleProperty selector in StatistleObject.PropertiesOf(type).Where(p => p.Property.PropertyType.IsEnum && p.Property.CanWrite))
        {
            object? original = selector.Read(subject);

            foreach (object value in Enum.GetValues(selector.Property.PropertyType))
            {
                selector.Property.SetValue(subject, value);
                hidden.UnionWith(MethodScope.HiddenProperties(subject).Select(p => p.XmlName));
            }

            selector.Property.SetValue(subject, original);
        }

        return hidden;
    }

    private static string Describe(StatistleProperty property, IReadOnlyList<ICalculationMethod> methods, HashSet<string> scoped)
    {
        var text = new StringBuilder(property.HasHelp ? property.Help : "No description.");

        if (property.Property.PropertyType.IsEnum)
        {
            text.Append(" Choices: ").Append(string.Join("; ", EnumLabel.LabelsOf(property.Property.PropertyType))).Append('.');
        }

        if (property.HasRange)
        {
            text.Append(" Range ").Append(property.CompactRangeText()).Append('.');
        }

        if (scoped.Contains(property.XmlName))
        {
            string[] owners = [.. methods.Where(m => m.Inputs.Contains(property.XmlName, StringComparer.Ordinal) || m.Outputs.Contains(property.XmlName, StringComparer.Ordinal)).Select(m => m.Name)];

            if (owners.Length > 0)
            {
                text.Append(" Shown with: ").Append(string.Join(", ", owners)).Append('.');
            }
        }

        return text.ToString();
    }
}

/// <summary>The instruction page as plain text.</summary>
public static class InstructionsTextWriter
{
    public static string Write(InstructionSet set)
    {
        ArgumentNullException.ThrowIfNull(set);
        var text = new StringBuilder();
        text.AppendLine(set.Title.ToUpperInvariant());

        if (set.Subtitle.Length > 0)
        {
            text.AppendLine(set.Subtitle);
        }

        foreach (InstructionBlock block in set.Blocks)
        {
            switch (block)
            {
                case Heading h:
                    text.AppendLine().AppendLine(h.Text).AppendLine(new string('-', h.Text.Length));
                    break;
                case Paragraph p:
                    text.AppendLine(p.Text).AppendLine();
                    break;
                case Bullets b:
                    foreach (string item in b.Items)
                    {
                        text.Append("- ").AppendLine(item);
                    }

                    text.AppendLine();
                    break;
                case Steps s:
                    for (int i = 0; i < s.Items.Count; i++)
                    {
                        text.Append(i + 1).Append(". ").AppendLine(s.Items[i]);
                    }

                    text.AppendLine();
                    break;
                case Definitions d:
                    foreach ((string term, string body) in d.Items)
                    {
                        text.Append(term).Append(": ").AppendLine(body);
                    }

                    text.AppendLine();
                    break;
                default:
                    break;
            }
        }

        return text.ToString();
    }
}

/// <summary>The instruction page as a self-contained HTML file, for the browser or for printing.</summary>
public static class InstructionsHtmlWriter
{
    public static string Write(InstructionSet set)
    {
        ArgumentNullException.ThrowIfNull(set);
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">");
        html.Append("<title>").Append(E(set.Title)).AppendLine(" — instructions</title>");
        html.AppendLine("<style>body{font-family:'Segoe UI',Arial,sans-serif;font-size:14px;line-height:1.5;color:#1f2933;max-width:860px;margin:32px auto;padding:0 20px}" +
                        "h1{font-size:24px;margin:0 0 2px}h2{font-size:16px;margin:26px 0 6px;color:#3b6ea5;text-transform:uppercase;letter-spacing:.04em}" +
                        ".sub{color:#6b7680;margin:0 0 14px}dl{margin:0}dt{font-weight:600;margin-top:8px}dd{margin:0 0 4px 0;color:#3d4852}ol,ul{padding-left:22px}li{margin:3px 0}</style></head><body>");
        html.Append("<h1>").Append(E(set.Title)).AppendLine("</h1>");

        if (set.Subtitle.Length > 0)
        {
            html.Append("<p class=\"sub\">").Append(E(set.Subtitle)).AppendLine("</p>");
        }

        foreach (InstructionBlock block in set.Blocks)
        {
            switch (block)
            {
                case Heading h:
                    html.Append("<h2>").Append(E(h.Text)).AppendLine("</h2>");
                    break;
                case Paragraph p:
                    html.Append("<p>").Append(E(p.Text)).AppendLine("</p>");
                    break;
                case Bullets b:
                    html.AppendLine("<ul>");
                    foreach (string item in b.Items)
                    {
                        html.Append("<li>").Append(E(item)).AppendLine("</li>");
                    }

                    html.AppendLine("</ul>");
                    break;
                case Steps s:
                    html.AppendLine("<ol>");
                    foreach (string item in s.Items)
                    {
                        html.Append("<li>").Append(E(item)).AppendLine("</li>");
                    }

                    html.AppendLine("</ol>");
                    break;
                case Definitions d:
                    html.AppendLine("<dl>");
                    foreach ((string term, string body) in d.Items)
                    {
                        html.Append("<dt>").Append(E(term)).Append("</dt><dd>").Append(E(body)).AppendLine("</dd>");
                    }

                    html.AppendLine("</dl>");
                    break;
                default:
                    break;
            }
        }

        html.AppendLine("</body></html>");
        return html.ToString();
    }

    private static string E(string text) => System.Net.WebUtility.HtmlEncode(text);
}
