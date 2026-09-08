using System.Diagnostics;
using System.Globalization;
using Statistle.Core.Analyses;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Persistence;
using Statistle.Core.Reporting;

namespace Statistle.Tests;

/// <summary>
/// The Code tab: every analysis result appears in the generated Python and R under the same
/// label, a data table becomes variables, the report carries both scripts as appendices, and —
/// when a Python with numpy and scipy is on the machine — the Python prints the app's numbers.
/// </summary>
public class CodeGenerationTests
{
    [Fact]
    public void Every_descriptive_result_is_printed_by_both_scripts_under_its_own_label()
    {
        var analysis = new DescriptiveStatistics();
        string python = analysis.Code(CodeLanguage.Python);
        string r = analysis.Code(CodeLanguage.R);

        Assert.StartsWith("import numpy as np", python, StringComparison.Ordinal);
        Assert.Contains("x = np.array([3.2, 4.8, 2.9", python, StringComparison.Ordinal);
        Assert.Contains("x <- c(3.2, 4.8, 2.9", r, StringComparison.Ordinal);
        Assert.Contains("ddof=1", python, StringComparison.Ordinal);
        Assert.Contains("fivenum(x)", r, StringComparison.Ordinal);

        foreach (string name in new DescriptiveStatisticsMethod().Outputs)
        {
            StatistleProperty property = analysis.Properties().Single(p => p.XmlName == name);
            Assert.Contains(property.Label + ":", python, StringComparison.Ordinal);
            Assert.Contains(property.Label + ":", r, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_column_from_a_project_table_is_named_after_the_column()
    {
        var document = new StatistleDocument();
        DataTable table = document.AddTable();
        table.Name = "Tensile strength";
        table.Columns.AddColumn("AA-X", [495, 508, 512]);

        var analysis = new DescriptiveStatistics { Project = document, Column = DataSources.Reference(table.Id, "AA-X") };
        analysis.Recalculate();

        Assert.Null(analysis.Problem);
        Assert.Contains("# Sample: Tensile strength › AA-X", analysis.Code(CodeLanguage.Python), StringComparison.Ordinal);
        Assert.Contains("aa_x = np.array([495, 508, 512])", analysis.Code(CodeLanguage.Python), StringComparison.Ordinal);
        Assert.Contains("aa_x <- c(495, 508, 512)", analysis.Code(CodeLanguage.R), StringComparison.Ordinal);
        Assert.Contains("np.mean(aa_x)", analysis.Code(CodeLanguage.Python), StringComparison.Ordinal);

        // The report names the column as the pick-list does, not by its stored reference.
        Report report = ReportBuilder.For(analysis);
        ReportValue column = report.Sections.SelectMany(s => s.Values).Single(v => v.Label == "Column");
        Assert.Equal("Tensile strength › AA-X", column.Value);
    }

    [Fact]
    public void A_data_table_becomes_vectors_and_a_frame_when_rectangular()
    {
        var table = new DataTable { Name = "Solar panel" };
        table.Columns.AddColumn("Temperature", [15, 20, 25]);
        table.Columns.AddColumn("Efficiency", [19.1, 18.7, 18.2]);

        string python = table.Code(CodeLanguage.Python);
        string r = table.Code(CodeLanguage.R);

        Assert.Contains("import pandas as pd", python, StringComparison.Ordinal);
        Assert.Contains("temperature = np.array([15, 20, 25])", python, StringComparison.Ordinal);
        Assert.Contains("solar_panel = pd.DataFrame({'temperature': temperature, 'efficiency': efficiency})", python, StringComparison.Ordinal);
        Assert.Contains("efficiency <- c(19.1, 18.7, 18.2)", r, StringComparison.Ordinal);
        Assert.Contains("solar_panel <- data.frame(temperature, efficiency)", r, StringComparison.Ordinal);

        table.Columns.AddColumn("Extra", [1]);
        Assert.DoesNotContain("data.frame", table.Code(CodeLanguage.R), StringComparison.Ordinal);
        Assert.Contains("differ in length", table.Code(CodeLanguage.R), StringComparison.Ordinal);
        Assert.DoesNotContain("pandas", table.Code(CodeLanguage.Python), StringComparison.Ordinal);
    }

    [Fact]
    public void The_report_carries_both_scripts_as_appendices()
    {
        Report report = ReportBuilder.For(new DescriptiveStatistics());

        Assert.Equal(["Python", "R"], report.Appendices.Select(a => a.Title));
        string html = HtmlReportWriter.Write(report, null);
        Assert.Contains("<h2>Appendix — Python</h2>", html, StringComparison.Ordinal);
        Assert.Contains("<pre>", html, StringComparison.Ordinal);
        Assert.Contains("np.array([3.2, 4.8", html, StringComparison.Ordinal);
        Assert.Contains("Appendix — R", TextReportWriter.Write(report), StringComparison.Ordinal);

        Assert.Empty(ReportBuilder.For(new SimpleRegression()).Appendices);   // not yet a code source
    }

    [Fact]
    public void The_generated_Python_prints_the_apps_numbers_when_Python_is_available()
    {
        if (!PythonHas("numpy", "scipy"))
        {
            // A machine without the course's Python cannot cross-check. Set STATISTLE_REQUIRE_PYTHON
            // (a dev box with numpy and scipy on the PATH, or CI) to make that a failure instead.
            Assert.True(
                Environment.GetEnvironmentVariable("STATISTLE_REQUIRE_PYTHON") is null,
                "STATISTLE_REQUIRE_PYTHON is set but 'python' with numpy and scipy was not found on the PATH.");
            return;
        }

        var analysis = new DescriptiveStatistics();
        analysis.Sample.Clear();
        foreach (double v in new[] { 470, 485, 478, 492, 460, 473, 488, 484, 482, 475, 530.0 })
        {
            analysis.Sample.Add(analysis.Sample.Count, v);
        }

        analysis.Recalculate();

        string script = Path.Combine(Path.GetTempPath(), $"statistle-check-{Guid.NewGuid():N}.py");
        File.WriteAllText(script, analysis.Code(CodeLanguage.Python));
        string output = Run("python", script);
        File.Delete(script);

        var printed = output.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Contains(": ", StringComparison.Ordinal) && !l.StartsWith('['))
            .ToDictionary(l => l[..l.IndexOf(": ", StringComparison.Ordinal)], l => l[(l.IndexOf(": ", StringComparison.Ordinal) + 2)..]);

        foreach (string name in new DescriptiveStatisticsMethod().Outputs)
        {
            StatistleProperty property = analysis.Properties().Single(p => p.XmlName == name);
            Assert.True(printed.ContainsKey(property.Label), $"Python did not print '{property.Label}'. Output:\n{output}");
            object? value = property.Read(analysis);

            if (value is double expected && !double.IsNaN(expected))
            {
                double actual = double.Parse(printed[property.Label], CultureInfo.InvariantCulture);
                double tolerance = Math.Pow(10, -property.Decimals) * 0.51;
                Assert.True(Math.Abs(actual - expected) <= tolerance, $"{property.Label}: app {expected}, Python {actual}");
            }
            else if (value is int count)
            {
                Assert.Equal(count.ToString(CultureInfo.InvariantCulture), printed[property.Label]);
            }
        }

        // numpy prints the integer-valued outlier as [530] and a float one as [530.5]: compare the number.
        Assert.Equal(530.0, double.Parse(printed["Outliers"].Trim('[', ']'), CultureInfo.InvariantCulture), 9);
    }

    private static bool PythonHas(params string[] modules)
    {
        try
        {
            return Run("python", "-c", $"import {string.Join(", ", modules)}").Length == 0 && _lastExit == 0;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }

    private static int _lastExit;

    private static string Run(string exe, params string[] args)
    {
        var info = new ProcessStartInfo(exe)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
        };
        info.Environment["PYTHONIOENCODING"] = "utf-8";

        foreach (string arg in args)
        {
            info.ArgumentList.Add(arg);
        }

        using Process process = Process.Start(info)!;
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit(60_000);
        _lastExit = process.ExitCode;

        return process.ExitCode == 0 ? output : output + error;
    }
}
