using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Statistle.Core.Plotting;
using Statistle.Core.Reporting;

namespace Statistle.App.Reporting;

/// <summary>
/// The report on screen: the same <see cref="Report"/> the HTML, text and PDF writers render,
/// laid out as a WPF flow document with the figures in place. Nothing here knows what is being
/// reported; a new analysis's report appears with no change.
/// </summary>
public static class ReportDocumentBuilder
{
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(0x6B, 0x76, 0x80));
    private static readonly Brush Ink = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x33));
    private static readonly Brush Rule = new SolidColorBrush(Color.FromRgb(0xE6, 0xEA, 0xEE));

    public static FlowDocument Build(Report report, IFigureRenderer? renderer)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 13,
            Foreground = Ink,
            PagePadding = new Thickness(28, 20, 28, 24),
            ColumnWidth = double.PositiveInfinity,
            LineHeight = 19,
        };

        document.Blocks.Add(new Paragraph(new Run(report.Title))
        {
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 2),
        });

        if (!string.IsNullOrWhiteSpace(report.Subtitle))
        {
            document.Blocks.Add(new Paragraph(new Run(report.Subtitle)) { Foreground = Muted, Margin = new Thickness(0) });
        }

        document.Blocks.Add(new Paragraph(new Run($"Generated {report.GeneratedUtc.ToLocalTime():yyyy-MM-dd HH:mm}"))
        {
            Foreground = Muted,
            FontSize = 11,
            Margin = new Thickness(0, 0, 0, 10),
        });

        foreach (ReportSection section in report.Sections)
        {
            document.Blocks.Add(Heading(section.Title));

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(55, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(25, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(20, GridUnitType.Star) });

            var rows = new TableRowGroup();

            foreach (ReportValue value in section.Values)
            {
                var row = new TableRow();
                row.Cells.Add(Cell(value.Label, TextAlignment.Left));
                row.Cells.Add(Cell(value.Value, TextAlignment.Right));
                row.Cells.Add(Cell(value.Units, TextAlignment.Left, Muted));
                rows.Rows.Add(row);
            }

            table.RowGroups.Add(rows);
            document.Blocks.Add(table);
        }

        if (report.Series.Count > 0)
        {
            document.Blocks.Add(Heading("Series"));

            foreach (ReportSeries series in report.Series)
            {
                document.Blocks.Add(new Paragraph(new Run(
                    $"{series.Label}: {series.Count} value(s), total {series.Total:F3} {series.Units}".TrimEnd()))
                {
                    Margin = new Thickness(0, 0, 0, 2),
                });
            }
        }

        if (report.Figures.Count > 0)
        {
            document.Blocks.Add(Heading("Figures"));

            foreach (ReportFigure figure in report.Figures)
            {
                if (renderer is not null && ToImage(renderer, figure.Figure) is ImageSource image)
                {
                    var picture = new System.Windows.Controls.Image
                    {
                        Source = image,
                        Stretch = Stretch.Uniform,
                        MaxWidth = 760,
                        Margin = new Thickness(0, 4, 0, 2),
                    };

                    document.Blocks.Add(new BlockUIContainer(picture) { Margin = new Thickness(0) });
                }

                document.Blocks.Add(new Paragraph(new Run(figure.Title))
                {
                    Foreground = Muted,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 12),
                });
            }
        }

        if (report.Citations.Count > 0)
        {
            document.Blocks.Add(Heading("Methods"));

            foreach (ReportCitation citation in report.Citations)
            {
                var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 8), FontSize = 11 };
                paragraph.Inlines.Add(new Run(citation.Method) { FontWeight = FontWeights.SemiBold });
                paragraph.Inlines.Add(new LineBreak());
                paragraph.Inlines.Add(new Run(citation.Citation) { Foreground = Muted });
                document.Blocks.Add(paragraph);
            }
        }

        foreach (ReportAppendix appendix in report.Appendices)
        {
            document.Blocks.Add(Heading($"Appendix — {appendix.Title}"));

            var code = new Paragraph
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11.5,
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(10, 8, 10, 8),
                Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF6, 0xF8)),
                BorderBrush = Rule,
                BorderThickness = new Thickness(1),
            };

            string[] lines = appendix.Text.TrimEnd().Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                code.Inlines.Add(new Run(lines[i]));

                if (i < lines.Length - 1)
                {
                    code.Inlines.Add(new LineBreak());
                }
            }

            document.Blocks.Add(code);
        }

        return document;
    }

    private static Paragraph Heading(string text) =>
        new(new Run(text.ToUpperInvariant()))
        {
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = Muted,
            Margin = new Thickness(0, 18, 0, 4),
        };

    private static TableCell Cell(string text, TextAlignment alignment, Brush? foreground = null)
    {
        var paragraph = new Paragraph(new Run(text)) { TextAlignment = alignment, Margin = new Thickness(0) };

        if (foreground is not null)
        {
            paragraph.Foreground = foreground;
        }

        return new TableCell(paragraph)
        {
            Padding = new Thickness(6, 3, 6, 3),
            BorderBrush = Rule,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
    }

    private static ImageSource? ToImage(IFigureRenderer renderer, PlotSpec figure)
    {
        try
        {
            byte[] png = renderer.RenderPng(figure, ReportFigureSize.Width, ReportFigureSize.Height);

            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(png))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or InvalidOperationException)
        {
            return null;
        }
    }
}
