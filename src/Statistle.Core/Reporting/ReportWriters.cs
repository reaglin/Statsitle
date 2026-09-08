using System.Globalization;
using System.Net;
using System.Text;
using Statistle.Core.Plotting;

namespace Statistle.Core.Reporting;

/// <summary>Figure sizes shared by the writers, so the HTML, PDF and screen agree.</summary>
public static class ReportFigureSize
{
    public const int Width = 1200;

    public const int Height = 720;
}

/// <summary>Renders a <see cref="Report"/> as plain text — for a console, a log, or a paste.</summary>
public static class TextReportWriter
{
    public static string Write(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var text = new StringBuilder();

        text.AppendLine(report.Title);
        text.AppendLine(new string('=', report.Title.Length));

        if (!string.IsNullOrWhiteSpace(report.Subtitle))
        {
            text.AppendLine(report.Subtitle);
        }

        text.AppendLine(
            CultureInfo.CurrentCulture,
            $"Generated {report.GeneratedUtc.ToLocalTime():yyyy-MM-dd HH:mm}");
        text.AppendLine();

        foreach (ReportSection section in report.Sections)
        {
            text.AppendLine(section.Title);
            text.AppendLine(new string('-', section.Title.Length));

            int width = section.Values.Max(v => v.Label.Length);

            foreach (ReportValue value in section.Values)
            {
                text.Append(value.Label.PadRight(width + 2))
                    .Append(value.Value);

                if (!string.IsNullOrEmpty(value.Units))
                {
                    text.Append(' ').Append(value.Units);
                }

                text.AppendLine();
            }

            text.AppendLine();
        }

        if (report.Series.Count > 0)
        {
            text.AppendLine("Series");
            text.AppendLine("------");

            foreach (ReportSeries series in report.Series)
            {
                text.AppendLine(
                    CultureInfo.CurrentCulture,
                    $"{series.Label}: {series.Count} value(s), total {series.Total:F3} {series.Units}");
            }

            text.AppendLine();
        }

        if (report.Figures.Count > 0)
        {
            text.AppendLine("Figures");
            text.AppendLine("-------");

            foreach (ReportFigure figure in report.Figures)
            {
                text.AppendLine(figure.Title);
            }

            text.AppendLine();
        }

        if (report.Citations.Count > 0)
        {
            text.AppendLine("Methods");
            text.AppendLine("-------");

            foreach (ReportCitation citation in report.Citations)
            {
                text.AppendLine(CultureInfo.CurrentCulture, $"{citation.Method}");
                text.AppendLine(CultureInfo.CurrentCulture, $"    {citation.Citation}");
            }
        }

        foreach (ReportAppendix appendix in report.Appendices)
        {
            text.AppendLine();
            text.AppendLine(CultureInfo.CurrentCulture, $"Appendix — {appendix.Title}");
            text.AppendLine(new string('-', appendix.Title.Length + 11));
            text.AppendLine(appendix.Text.TrimEnd());
        }

        return text.ToString();
    }
}

/// <summary>
/// Renders a <see cref="Report"/> as a self-contained HTML page. Given a renderer, the report's
/// figures are embedded as images; without one they are listed by title.
/// </summary>
public static class HtmlReportWriter
{
    public static string Write(Report report, IFigureRenderer? renderer = null)
    {
        ArgumentNullException.ThrowIfNull(report);

        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\">");
        html.AppendLine(CultureInfo.InvariantCulture, $"<title>{Escape(report.Title)}</title>");
        html.AppendLine("<style>");
        html.AppendLine("""
            body { font: 13px/1.5 "Segoe UI", system-ui, sans-serif; color: #1f2933;
                   margin: 32px auto; max-width: 760px; }
            h1 { font-size: 20px; margin: 0 0 2px; }
            h2 { font-size: 13px; text-transform: uppercase; letter-spacing: .06em;
                 color: #6b7680; margin: 26px 0 6px; }
            .sub { color: #6b7680; margin: 0 0 2px; }
            .when { color: #93a1ad; font-size: 11px; margin: 0 0 8px; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 4px 8px; border-bottom: 1px solid #e6eaee; vertical-align: top; }
            td.label { width: 55%; }
            td.value { text-align: right; font-variant-numeric: tabular-nums; white-space: nowrap; }
            td.units { color: #6b7680; width: 90px; }
            .cite { color: #6b7680; font-size: 11px; margin: 0 0 8px; }
            pre { background: #f4f6f8; border: 1px solid #e6eaee; padding: 10px 12px; font: 12px Consolas, monospace; overflow-x: auto; }
            .cite b { color: #1f2933; font-weight: 600; }
            figure { margin: 10px 0 18px; }
            figure img { width: 100%; height: auto; border: 1px solid #e6eaee; }
            figcaption { color: #6b7680; font-size: 11px; margin-top: 4px; }
            """);
        html.AppendLine("</style></head><body>");

        html.AppendLine(CultureInfo.InvariantCulture, $"<h1>{Escape(report.Title)}</h1>");

        if (!string.IsNullOrWhiteSpace(report.Subtitle))
        {
            html.AppendLine(CultureInfo.InvariantCulture, $"<p class=\"sub\">{Escape(report.Subtitle)}</p>");
        }

        html.AppendLine(
            CultureInfo.InvariantCulture,
            $"<p class=\"when\">Generated {report.GeneratedUtc.ToLocalTime():yyyy-MM-dd HH:mm}</p>");

        foreach (ReportSection section in report.Sections)
        {
            html.AppendLine(CultureInfo.InvariantCulture, $"<h2>{Escape(section.Title)}</h2><table>");

            foreach (ReportValue value in section.Values)
            {
                html.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<tr><td class=\"label\">{Escape(value.Label)}</td>" +
                    $"<td class=\"value\">{Escape(value.Value)}</td>" +
                    $"<td class=\"units\">{Escape(value.Units)}</td></tr>");
            }

            html.AppendLine("</table>");
        }

        if (report.Series.Count > 0)
        {
            html.AppendLine("<h2>Series</h2><table>");

            foreach (ReportSeries series in report.Series)
            {
                html.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<tr><td class=\"label\">{Escape(series.Label)}</td>" +
                    $"<td class=\"value\">{series.Count} value(s), total {series.Total:F3}</td>" +
                    $"<td class=\"units\">{Escape(series.Units)}</td></tr>");
            }

            html.AppendLine("</table>");
        }

        if (report.Figures.Count > 0)
        {
            html.AppendLine("<h2>Figures</h2>");

            foreach (ReportFigure figure in report.Figures)
            {
                if (renderer is null)
                {
                    html.AppendLine(CultureInfo.InvariantCulture, $"<p class=\"cite\">{Escape(figure.Title)}</p>");
                    continue;
                }

                string png = Convert.ToBase64String(
                    renderer.RenderPng(figure.Figure, ReportFigureSize.Width, ReportFigureSize.Height));

                html.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<figure><img alt=\"{Escape(figure.Title)}\" src=\"data:image/png;base64,{png}\">" +
                    $"<figcaption>{Escape(figure.Title)}</figcaption></figure>");
            }
        }

        if (report.Citations.Count > 0)
        {
            html.AppendLine("<h2>Methods</h2>");

            foreach (ReportCitation citation in report.Citations)
            {
                html.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p class=\"cite\"><b>{Escape(citation.Method)}</b><br>{Escape(citation.Citation)}</p>");
            }
        }

        foreach (ReportAppendix appendix in report.Appendices)
        {
            html.AppendLine(CultureInfo.InvariantCulture, $"<h2>Appendix — {Escape(appendix.Title)}</h2>");
            html.AppendLine(CultureInfo.InvariantCulture, $"<pre>{Escape(appendix.Text.TrimEnd())}</pre>");
        }

        html.AppendLine("</body></html>");

        return html.ToString();
    }

    private static string Escape(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
