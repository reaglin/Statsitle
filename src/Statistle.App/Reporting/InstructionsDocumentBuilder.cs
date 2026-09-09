using System.Windows;
using System.Windows.Documents;
using Paragraph = System.Windows.Documents.Paragraph;
using List = System.Windows.Documents.List;
using System.Windows.Media;
using Statistle.Core.Instructions;

namespace Statistle.App.Reporting;

/// <summary>
/// An instruction page on screen: the same <see cref="InstructionSet"/> the HTML and text writers
/// render, as a WPF flow document. Nothing here knows which item is described.
/// </summary>
public static class InstructionsDocumentBuilder
{
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(0x6B, 0x76, 0x80));
    private static readonly Brush Ink = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x33));
    private static readonly Brush Accent = new SolidColorBrush(Color.FromRgb(0x3B, 0x6E, 0xA5));

    public static FlowDocument Build(InstructionSet set)
    {
        ArgumentNullException.ThrowIfNull(set);

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 13,
            Foreground = Ink,
            PagePadding = new Thickness(28, 20, 28, 24),
            ColumnWidth = double.PositiveInfinity,
            LineHeight = 20,
        };

        document.Blocks.Add(new Paragraph(new Run(set.Title))
        {
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 2),
        });

        if (!string.IsNullOrWhiteSpace(set.Subtitle))
        {
            document.Blocks.Add(new Paragraph(new Run(set.Subtitle)) { Foreground = Muted, Margin = new Thickness(0, 0, 0, 10) });
        }

        foreach (InstructionBlock block in set.Blocks)
        {
            switch (block)
            {
                case Heading h:
                    document.Blocks.Add(new Paragraph(new Run(h.Text.ToUpperInvariant()))
                    {
                        FontSize = 11.5,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Accent,
                        Margin = new Thickness(0, 16, 0, 4),
                    });
                    break;

                case Core.Instructions.Paragraph p:
                    document.Blocks.Add(new Paragraph(new Run(p.Text)) { Margin = new Thickness(0, 0, 0, 8), TextAlignment = TextAlignment.Left });
                    break;

                case Bullets b:
                    document.Blocks.Add(ListOf(b.Items, TextMarkerStyle.Disc));
                    break;

                case Steps s:
                    document.Blocks.Add(ListOf(s.Items, TextMarkerStyle.Decimal));
                    break;

                case Definitions d:
                    foreach ((string term, string text) in d.Items)
                    {
                        var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 6), TextAlignment = TextAlignment.Left };
                        paragraph.Inlines.Add(new Run(term) { FontWeight = FontWeights.SemiBold });
                        paragraph.Inlines.Add(new Run("  " + text) { Foreground = new SolidColorBrush(Color.FromRgb(0x3D, 0x48, 0x52)) });
                        document.Blocks.Add(paragraph);
                    }

                    break;

                default:
                    break;
            }
        }

        return document;
    }

    private static List ListOf(IReadOnlyList<string> items, TextMarkerStyle marker)
    {
        var list = new List { MarkerStyle = marker, Margin = new Thickness(18, 0, 0, 8), Padding = new Thickness(0) };

        foreach (string item in items)
        {
            list.ListItems.Add(new ListItem(new Paragraph(new Run(item)) { Margin = new Thickness(0, 0, 0, 3), TextAlignment = TextAlignment.Left }));
        }

        return list;
    }
}
