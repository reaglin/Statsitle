using System.Globalization;
using System.Text;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Core.Reporting;

/// <summary>
/// CSV of any series on any object. Like the report, it works from the property metadata, so a
/// tool gets data export by declaring a <see cref="DataSeries"/> property and nothing else.
/// </summary>
public static class CsvExport
{
    /// <summary>Every series the object holds, as one CSV with a column pair per series.</summary>
    public static string SeriesToCsv(StatistleObject subject, UnitSelection? units = null)
    {
        ArgumentNullException.ThrowIfNull(subject);

        (StatistleProperty Property, DataSeries Series)[] found =
        [
            .. subject.Properties()
                .Select(p => (Property: p, Series: p.Read(subject) as DataSeries))
                .Where(p => p.Series is not null)
                .Select(p => (p.Property, Series: p.Series!)),
        ];

        if (found.Length == 0)
        {
            return string.Empty;
        }

        var csv = new StringBuilder();

        // Header: each series contributes an x and a y column, labelled with its units.
        List<string> headers = [];
        foreach ((StatistleProperty property, _) in found)
        {
            string unit = property.Unit == ValueKind.None
                ? string.Empty
                : $" ({units?.LabelFor(property.Unit) ?? UnitSystem.BaseUnit(property.Unit).Label})";

            headers.Add(Quote("Time (hours)"));
            headers.Add(Quote(property.ReportLabel + unit));
        }

        csv.AppendLine(string.Join(',', headers));

        int rows = found.Max(f => f.Series.Count);

        for (int row = 0; row < rows; row++)
        {
            List<string> cells = [];

            foreach ((StatistleProperty property, DataSeries series) in found)
            {
                if (row < series.Count)
                {
                    DataPoint point = series.Points[row];
                    double y = units is null ? point.Y : units.ToDisplay(property.Unit, point.Y);

                    cells.Add(point.X.ToString("G10", CultureInfo.InvariantCulture));
                    cells.Add(y.ToString("G10", CultureInfo.InvariantCulture));
                }
                else
                {
                    cells.Add(string.Empty);
                    cells.Add(string.Empty);
                }
            }

            csv.AppendLine(string.Join(',', cells));
        }

        return csv.ToString();
    }

    /// <summary>A single named series as a two-column CSV.</summary>
    public static string SeriesToCsv(DataSeries series, string label)
    {
        ArgumentNullException.ThrowIfNull(series);

        var csv = new StringBuilder();
        csv.AppendLine($"{Quote("Time (hours)")},{Quote(label)}");

        foreach (DataPoint point in series.Points)
        {
            csv.Append(point.X.ToString("G10", CultureInfo.InvariantCulture))
               .Append(',')
               .AppendLine(point.Y.ToString("G10", CultureInfo.InvariantCulture));
        }

        return csv.ToString();
    }

    /// <summary>Every reported value as a two-column CSV — a report anyone can open in a sheet.</summary>
    public static string ReportToCsv(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var csv = new StringBuilder();
        csv.AppendLine($"{Quote("Section")},{Quote("Item")},{Quote("Value")},{Quote("Units")}");

        foreach (ReportSection section in report.Sections)
        {
            foreach (ReportValue value in section.Values)
            {
                csv.AppendLine(string.Join(
                    ',',
                    Quote(section.Title),
                    Quote(value.Label),
                    Quote(value.Value),
                    Quote(value.Units)));
            }
        }

        return csv.ToString();
    }

    /// <summary>
    /// CSV quoting: a field containing a comma, a quote or a newline is quoted, and embedded
    /// quotes are doubled. A label like "Manning's n" or a citation with a comma would otherwise
    /// split a row.
    /// </summary>
    private static string Quote(string? value)
    {
        string text = value ?? string.Empty;

        if (!text.Contains(',', StringComparison.Ordinal)
            && !text.Contains('"', StringComparison.Ordinal)
            && !text.Contains('\n', StringComparison.Ordinal)
            && !text.Contains('\r', StringComparison.Ordinal))
        {
            return text;
        }

        return $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
