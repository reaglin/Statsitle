using Statistle.Core.Domain;
using Statistle.Core.Persistence;
using Statistle.Core.Reporting;
using Statistle.Core.Analyses;
using Statistle.Core.Units;

namespace Statistle.Tests;

/// <summary>
/// An analysis written in the ordinary style and nothing more: properties, a title, a summary.
/// It has no report code, no export code and no plot code of its own. It gets all three anyway,
/// off the metadata alone.
/// </summary>
public sealed class OutputProbeTool : Analysis
{
    private double _given = 12.5;
    private double _chosen = 0.75;
    private double _result;

    public OutputProbeTool()
    {
        Name = "Output probe";
        Readings.Add(0.00, 0.25);
        Readings.Add(0.25, 0.80);
        Readings.Add(0.50, 0.35);
        Recalculate();
    }

    public override string Title => "Output probe";

    public override string Summary => "Proves output needs no per-analysis code";

    public override ToolKind Kind => ToolKind.FreeStanding;

    [StatistleProperty(Label = "Given value", ReportLabel = "Given measured value", Decimals = 2,
        Role = VariableRole.Input, Min = 0)]
    public double Given
    {
        get => _given;
        set => SetProperty(ref _given, value);
    }

    [StatistleProperty(Label = "Chosen fraction", Decimals = 3, Role = VariableRole.Design,
        Min = 0, Max = 1)]
    public double Chosen
    {
        get => _chosen;
        set => SetProperty(ref _chosen, value);
    }

    [StatistleProperty(Label = "Computed value", Decimals = 3, Role = VariableRole.Output)]
    public double Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    [StatistleProperty(Label = "Readings", Role = VariableRole.Input)]
    public DataSeries Readings { get; } = new();

    public override void Recalculate() => Result = Given * Chosen;
}

public class GenericOutputTests
{
    private static Report ReportFor(StatistleObject subject, UnitSelection? units = null) =>
        ReportBuilder.For(subject, units);

    [Fact]
    public void An_analysis_with_no_output_code_still_reports()
    {
        Report report = ReportFor(new OutputProbeTool());

        Assert.Contains("Output probe", report.Title, StringComparison.Ordinal);

        // The ReportLabel is preferred over the interface label.
        Assert.Contains(
            report.Sections.SelectMany(s => s.Values),
            v => v.Label == "Given measured value");
    }

    [Fact]
    public void An_analysis_with_no_output_code_still_exports_csv()
    {
        string csv = CsvExport.SeriesToCsv(new OutputProbeTool());

        Assert.Contains("Readings", csv, StringComparison.Ordinal);
        Assert.Contains("0.8", csv, StringComparison.Ordinal);
    }

    [Fact]
    public void An_analysis_with_no_output_code_still_writes_a_pdf()
    {
        string file = Path.Combine(Path.GetTempPath(), $"statistle-{Guid.NewGuid():N}.pdf");

        try
        {
            PdfReportWriter.Write(ReportFor(new OutputProbeTool()), file);

            Assert.True(File.Exists(file));
            Assert.True(new FileInfo(file).Length > 500, "The PDF is suspiciously small.");

            // A real PDF starts with %PDF.
            byte[] header = new byte[4];
            using (FileStream stream = File.OpenRead(file))
            {
                stream.ReadExactly(header);
            }

            Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(header));
        }
        finally
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    [Fact]
    public void An_empty_title_falls_back_to_describing_the_subject()
    {
        // An empty string is not null: the report was left headed by nothing but its timestamp,
        // and the PDF had no document title.
        Report report = ReportBuilder.For(new OutputProbeTool(), null, "   ");

        Assert.Contains("Output probe", report.Title, StringComparison.Ordinal);
        Assert.Contains($"<h1>{report.Title}</h1>", HtmlReportWriter.Write(report), StringComparison.Ordinal);
    }

    [Fact]
    public void Sections_come_from_the_variable_roles()
    {
        // A report separates what was given from what was chosen from what was computed,
        // because the roles already say which is which.
        Report report = ReportFor(new OutputProbeTool());

        string[] headings = [.. report.Sections.Select(s => s.Title)];

        Assert.Contains("Given", headings);
        Assert.Contains("Design values", headings);
        Assert.Contains("Results", headings);
    }

    [Fact]
    public void A_percentage_is_reported_with_its_sign()
    {
        // The only value kind that carries a label: a percentage shows "%" after the number.
        var units = new UnitSelection();

        Assert.Equal("%", units.LabelFor(ValueKind.Percent));
        Assert.Equal(string.Empty, units.LabelFor(ValueKind.None));

        // Nothing converts, so a stored value and a shown value are the same number.
        Assert.Equal(42.0, units.ToDisplay(ValueKind.Percent, 42.0));
        Assert.Equal(42.0, units.ToBase(ValueKind.Percent, 42.0));
    }

    [Fact]
    public void A_series_is_summarised_rather_than_listed()
    {
        Report report = ReportFor(new OutputProbeTool());

        ReportSeries series = Assert.Single(report.Series);

        Assert.Equal(3, series.Count);
        Assert.Equal(1.40, series.Total, 6);
    }

    [Fact]
    public void Every_shipped_analysis_reports_without_throwing()
    {
        // Sweeps the build: an analysis added later cannot quietly lack output.
        foreach (AnalysisDescriptor descriptor in AnalysisCatalog.Discover(typeof(Analysis).Assembly))
        {
            Analysis analysis = descriptor.Create();

            Report report = ReportFor(analysis);

            Assert.NotEmpty(report.Sections);
            Assert.NotEmpty(TextReportWriter.Write(report));
            Assert.Contains("<html", HtmlReportWriter.Write(report), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_report_carries_the_citation_of_every_method_that_made_it()
    {
        // Provenance travels with the numbers: a report says where its formulas came from.
        Report report = ReportFor(new DescriptiveStatistics());

        Assert.Contains(report.Citations, c => c.Method == "Sample statistics");
        Assert.Contains(report.Citations, c => c.Citation.Contains("Montgomery", StringComparison.Ordinal));
    }

    [Fact]
    public void An_analysis_reports_only_the_fields_its_chosen_source_uses()
    {
        // The report agrees with the form: same scoping, same fields. With no column chosen the
        // sample is typed in, so it is on the form and in the report.
        string[] labels =
        [
            .. ReportFor(new DescriptiveStatistics()).Sections.SelectMany(s => s.Values).Select(v => v.Label),
        ];

        Assert.Contains("Mean", labels);
        Assert.Contains("Standard deviation (s)", labels);
    }

    [Fact]
    public void Report_text_and_html_describe_the_same_values()
    {
        Report report = ReportFor(new OutputProbeTool());

        string text = TextReportWriter.Write(report);
        string html = HtmlReportWriter.Write(report);

        foreach (ReportValue value in report.Sections.SelectMany(s => s.Values))
        {
            Assert.Contains(value.Label, text, StringComparison.Ordinal);
            Assert.Contains(value.Label, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Csv_quoting_survives_a_label_containing_a_comma()
    {
        // A citation or a label with a comma would otherwise split the row.
        var report = new Report
        {
            Title = "Quoting",
            Sections = [new ReportSection("S", [new ReportValue("A label, with a comma", "1", "in")])],
        };

        string csv = CsvExport.ReportToCsv(report);

        Assert.Contains("\"A label, with a comma\"", csv, StringComparison.Ordinal);
    }

    [Fact]
    public void An_object_holding_no_series_still_exports_its_report_values()
    {
        // SeriesToCsv is empty for such an object, so the caller falls back to the report.
        var table = new DataTable { Name = "No series" };
        table.Columns.AddColumn("A", [1, 2, 3]);

        Assert.Empty(CsvExport.SeriesToCsv(table));
        Assert.Contains("A", CsvExport.ReportToCsv(ReportFor(table)), StringComparison.Ordinal);
    }
}
