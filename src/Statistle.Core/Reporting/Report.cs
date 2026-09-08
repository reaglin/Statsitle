using System.Globalization;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Analyses;
using Statistle.Core.Units;

namespace Statistle.Core.Reporting;

/// <summary>One reported value: what it is called, what it is, and what it is measured in.</summary>
public sealed record ReportValue(string Label, string Value, string Units);

/// <summary>A group of values under a heading.</summary>
public sealed record ReportSection(string Title, IReadOnlyList<ReportValue> Values);

/// <summary>Where a number came from.</summary>
public sealed record ReportCitation(string Method, string Citation);

/// <summary>A series included in the report, summarised rather than listed in full.</summary>
public sealed record ReportSeries(string Label, int Count, double Total, string Units);

/// <summary>
/// A figure that belongs with the report — the plots the object itself declares. Carried as
/// the figure specification; a writer with a renderer draws it, one without lists its title.
/// </summary>
public sealed record ReportFigure(string Title, PlotSpec Figure);

/// <summary>
/// A finished report, ready for any writer. It holds no formatting decisions — HTML, text and
/// PDF all render this same object, which is what stops the three drifting apart.
/// </summary>
public sealed class Report
{
    public required string Title { get; init; }

    public string Subtitle { get; init; } = string.Empty;

    public DateTime GeneratedUtc { get; init; } = DateTime.UtcNow;

    public IReadOnlyList<ReportSection> Sections { get; init; } = [];

    public IReadOnlyList<ReportSeries> Series { get; init; } = [];

    public IReadOnlyList<ReportCitation> Citations { get; init; } = [];

    public IReadOnlyList<ReportFigure> Figures { get; init; } = [];
}

/// <summary>
/// Builds a report from any <see cref="StatistleObject"/>, using nothing but the metadata already on
/// its properties.
///
/// <para>The section headings come from <see cref="VariableRole"/>, so a report separates what
/// was given from what was chosen from what was computed — the distinction an engineer reading it
/// actually needs. Labels come from <c>ReportLabel</c>, precision from <c>Decimals</c>, units from
/// <c>Unit</c> and the project's selection, and the visible set from <see cref="MethodScope"/>, so
/// a report shows the same fields the form did.</para>
///
/// <para>Nothing here names a property or a type: a field added to an analysis appears in its
/// report with no edit to the writer.</para>
/// </summary>
public static class ReportBuilder
{
    public static Report For(StatistleObject subject, UnitSelection? units = null, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(subject);

        IReadOnlyList<StatistleProperty> properties = MethodScope.VisibleProperties(subject);

        List<ReportSection> sections = [];
        List<ReportSeries> series = [];

        foreach ((string heading, VariableRole[] roles) in Groups)
        {
            List<ReportValue> values = [];

            foreach (StatistleProperty property in properties.Where(p => roles.Contains(p.Role)))
            {
                if (property.Read(subject) is DataSeries data)
                {
                    series.Add(new ReportSeries(
                        property.ReportLabel,
                        data.Count,
                        data.Points.Sum(p => p.Y),
                        UnitLabel(property, units)));

                    continue;
                }

                // A table of columns reports as one line per column: its name, and how many
                // values it holds. The numbers themselves belong in an export, not in a report.
                if (property.Read(subject) is ColumnTable table)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        values.Add(new ReportValue(
                            column.Name,
                            column.Count.ToString(CultureInfo.CurrentCulture),
                            "value(s)"));
                    }

                    continue;
                }

                values.Add(new ReportValue(
                    property.ReportLabel,
                    Format(subject, property, units),
                    UnitLabel(property, units)));
            }

            if (values.Count > 0)
            {
                sections.Add(new ReportSection(heading, values));
            }
        }

        List<ReportCitation> citations =
        [
            .. MethodScope.ActiveMethods(subject)
                .Select(m => new ReportCitation(m.Name, m.Citation))
                .DistinctBy(c => c.Method),
        ];

        List<ReportFigure> figures =
            subject is IHasFigures drawable
                ? [.. drawable.Figures(units).Select(f => new ReportFigure(f.Title, f))]
                : [];

        return new Report
        {
            // Whitespace counts as absent: an empty title left the report headed by nothing
            // but its timestamp, and the PDF with no document title at all.
            Title = string.IsNullOrWhiteSpace(title) ? DescribeSubject(subject) : title,
            Subtitle = subject.Description,
            Sections = sections,
            Series = series,
            Citations = citations,
            Figures = figures,
        };
    }

    /// <summary>Section headings, and the roles that belong under each.</summary>
    private static readonly (string Heading, VariableRole[] Roles)[] Groups =
    [
        ("Identification", [VariableRole.Unspecified]),
        ("Given", [VariableRole.Input]),
        ("Design values", [VariableRole.Design]),
        ("Intermediate", [VariableRole.Calculated]),
        ("Results", [VariableRole.Output]),
    ];

    private static string DescribeSubject(StatistleObject subject)
    {
        if (subject is Analysis tool)
        {
            return string.IsNullOrWhiteSpace(subject.Name) || subject.Name == tool.Title
                ? tool.Title
                : $"{tool.Title} — {subject.Name}";
        }

        return string.IsNullOrWhiteSpace(subject.Name)
            ? subject.GetType().Name
            : $"{subject.GetType().Name} — {subject.Name}";
    }

    private static string UnitLabel(StatistleProperty property, UnitSelection? units) =>
        property.Unit == ValueKind.None
            ? string.Empty
            : units?.LabelFor(property.Unit) ?? UnitSystem.BaseUnit(property.Unit).Label;

    private static string Format(StatistleObject subject, StatistleProperty property, UnitSelection? units)
    {
        object? value = property.Read(subject);

        return value switch
        {
            double number when double.IsNaN(number) => "—",
            double number => (units is null ? number : units.ToDisplay(property.Unit, number))
                .ToString($"F{Math.Max(property.Decimals, 0)}", CultureInfo.CurrentCulture),
            Enum choice => EnumLabel.Of(choice),
            null => string.Empty,
            _ => value.ToString() ?? string.Empty,
        };
    }
}
