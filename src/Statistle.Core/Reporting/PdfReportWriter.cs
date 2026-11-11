using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Statistle.Core.Plotting;

namespace Statistle.Core.Reporting;

/// <summary>
/// Renders a <see cref="Report"/> to PDF. Same report object as the HTML and text writers, so
/// the three cannot disagree about what a tool reports.
///
/// PDFsharp is MIT-licensed, which matters for a Store submission; it also draws rather than
/// converts, so there is no browser engine in the package.
/// </summary>
public static class PdfReportWriter
{
    private const double Margin = 56;      // points; 56 pt is a little under 20 mm
    private const double LineHeight = 15;
    private const double SectionGap = 18;

    public static void Write(Report report, string path, IFigureRenderer? renderer = null)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // PDFsharp carries no fonts of its own and throws on the first XFont without a
        // resolver, so one is installed before any font is created.
        StatistleFontResolver.Install();

        using var document = new PdfDocument();
        document.Info.Title = report.Title;
        document.Info.Creator = "Statistics+";

        var fonts = new Fonts();
        PdfPage page = NewPage(document);
        XGraphics gfx = XGraphics.FromPdfPage(page);
        double y = Margin;

        y = DrawHeading(gfx, fonts, report, y);

        foreach (ReportSection section in report.Sections)
        {
            (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight * 3);

            y += SectionGap;
            gfx.DrawString(section.Title.ToUpperInvariant(), fonts.SectionHeading, XBrushes.Gray,
                new XPoint(Margin, y));
            y += LineHeight * 0.5;

            gfx.DrawLine(XPens.LightGray, Margin, y, page.Width.Point - Margin, y);
            y += LineHeight * 0.7;

            foreach (ReportValue value in section.Values)
            {
                (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight);

                gfx.DrawString(value.Label, fonts.Body, XBrushes.Black, new XPoint(Margin, y));

                string shown = string.IsNullOrEmpty(value.Units)
                    ? value.Value
                    : $"{value.Value} {value.Units}";

                gfx.DrawString(
                    shown,
                    fonts.Body,
                    XBrushes.Black,
                    new XRect(Margin, y - LineHeight, page.Width.Point - (2 * Margin), LineHeight),
                    XStringFormats.BottomRight);

                y += LineHeight;
            }
        }

        if (report.Series.Count > 0)
        {
            (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight * 3);

            y += SectionGap;
            gfx.DrawString("SERIES", fonts.SectionHeading, XBrushes.Gray, new XPoint(Margin, y));
            y += LineHeight;

            foreach (ReportSeries series in report.Series)
            {
                (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight);

                gfx.DrawString(
                    string.Create(
                        CultureInfo.CurrentCulture,
                        $"{series.Label}: {series.Count} value(s), total {series.Total:F3} {series.Units}"),
                    fonts.Body,
                    XBrushes.Black,
                    new XPoint(Margin, y));

                y += LineHeight;
            }
        }

        if (report.Figures.Count > 0)
        {
            double width = page.Width.Point - (2 * Margin);
            double height = width * ReportFigureSize.Height / ReportFigureSize.Width;

            (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight * 3);
            y += SectionGap;
            gfx.DrawString("FIGURES", fonts.SectionHeading, XBrushes.Gray, new XPoint(Margin, y));
            y += LineHeight * 0.5;

            foreach (ReportFigure figure in report.Figures)
            {
                if (renderer is null)
                {
                    (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight);
                    y += LineHeight;
                    gfx.DrawString(figure.Title, fonts.Body, XBrushes.Black, new XPoint(Margin, y));
                    continue;
                }

                (gfx, page, y) = EnsureRoom(document, gfx, page, y, height + LineHeight * 2.5);
                y += LineHeight * 0.5;

                byte[] png = renderer.RenderPng(figure.Figure, ReportFigureSize.Width, ReportFigureSize.Height);
                using (var stream = new MemoryStream(png))
                using (XImage image = XImage.FromStream(stream))
                {
                    gfx.DrawImage(image, Margin, y, width, height);
                }

                y += height + LineHeight * 0.9;
                gfx.DrawString(figure.Title, fonts.Small, XBrushes.Gray, new XPoint(Margin, y));
                y += LineHeight * 0.6;
            }
        }

        if (report.Citations.Count > 0)
        {
            (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight * 4);

            y += SectionGap;
            gfx.DrawString("METHODS", fonts.SectionHeading, XBrushes.Gray, new XPoint(Margin, y));
            y += LineHeight;

            foreach (ReportCitation citation in report.Citations)
            {
                (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight * 2);

                gfx.DrawString(citation.Method, fonts.SmallBold, XBrushes.Black, new XPoint(Margin, y));
                y += LineHeight * 0.85;

                foreach (string line in Wrap(gfx, citation.Citation, fonts.Small,
                             page.Width.Point - (2 * Margin)))
                {
                    (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight);

                    gfx.DrawString(line, fonts.Small, XBrushes.Gray, new XPoint(Margin, y));
                    y += LineHeight * 0.85;
                }

                y += LineHeight * 0.4;
            }
        }

        foreach (ReportAppendix appendix in report.Appendices)
        {
            (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight * 4);

            y += SectionGap;
            gfx.DrawString($"APPENDIX — {appendix.Title.ToUpperInvariant()}", fonts.SectionHeading, XBrushes.Gray, new XPoint(Margin, y));
            y += LineHeight;

            foreach (string codeLine in appendix.Text.TrimEnd().Split('\n'))
            {
                foreach (string line in Wrap(gfx, codeLine.Length == 0 ? " " : codeLine, fonts.Code, page.Width.Point - (2 * Margin)))
                {
                    (gfx, page, y) = EnsureRoom(document, gfx, page, y, LineHeight);
                    gfx.DrawString(line, fonts.Code, XBrushes.Black, new XPoint(Margin, y));
                    y += LineHeight * 0.8;
                }
            }
        }

        gfx.Dispose();

        string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        document.Save(path);
    }

    private static double DrawHeading(XGraphics gfx, Fonts fonts, Report report, double y)
    {
        gfx.DrawString(report.Title, fonts.Title, XBrushes.Black, new XPoint(Margin, y));
        y += LineHeight * 1.4;

        if (!string.IsNullOrWhiteSpace(report.Subtitle))
        {
            gfx.DrawString(report.Subtitle, fonts.Body, XBrushes.Gray, new XPoint(Margin, y));
            y += LineHeight;
        }

        gfx.DrawString(
            string.Create(
                CultureInfo.CurrentCulture,
                $"Generated {report.GeneratedUtc.ToLocalTime():yyyy-MM-dd HH:mm}"),
            fonts.Small,
            XBrushes.Gray,
            new XPoint(Margin, y));

        return y + (LineHeight * 0.5);
    }

    private static PdfPage NewPage(PdfDocument document)
    {
        PdfPage page = document.AddPage();
        page.Size = PdfSharp.PageSize.Letter;

        return page;
    }

    /// <summary>Starts a new page when the next block would not fit on this one.</summary>
    private static (XGraphics Gfx, PdfPage Page, double Y) EnsureRoom(
        PdfDocument document, XGraphics gfx, PdfPage page, double y, double needed)
    {
        if (y + needed <= page.Height.Point - Margin)
        {
            return (gfx, page, y);
        }

        gfx.Dispose();

        PdfPage next = NewPage(document);

        return (XGraphics.FromPdfPage(next), next, Margin);
    }

    private static IEnumerable<string> Wrap(XGraphics gfx, string text, XFont font, double width)
    {
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line = new System.Text.StringBuilder();

        foreach (string word in words)
        {
            string candidate = line.Length == 0 ? word : $"{line} {word}";

            if (gfx.MeasureString(candidate, font).Width > width && line.Length > 0)
            {
                yield return line.ToString();
                line.Clear().Append(word);
            }
            else
            {
                line.Clear().Append(candidate);
            }
        }

        if (line.Length > 0)
        {
            yield return line.ToString();
        }
    }

    private sealed class Fonts
    {
        public XFont Title { get; } = new("Segoe UI", 16, XFontStyleEx.Bold);

        public XFont SectionHeading { get; } = new("Segoe UI", 8, XFontStyleEx.Bold);

        public XFont Body { get; } = new("Segoe UI", 10);

        public XFont SmallBold { get; } = new("Segoe UI", 8, XFontStyleEx.Bold);

        public XFont Small { get; } = new("Segoe UI", 8);

        public XFont Code { get; } = new("Segoe UI", 7.5);
    }
}
