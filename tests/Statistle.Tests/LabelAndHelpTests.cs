using Statistle.Core.Analyses;
using Statistle.Core.Domain;
using Statistle.Core.Methods.Statistics;
using Statistle.Core.Persistence;
using Statistle.Core.Reporting;
using Statistle.Core.Units;

namespace Statistle.Tests;

/// <summary>
/// Enum values read as words wherever they appear — drop-down, report, CSV — because one place
/// decides how a member is labelled.
/// </summary>
public class EnumLabelTests
{
    [Fact]
    public void A_pascal_case_name_reads_as_words()
    {
        Assert.Equal("Reciprocal", EnumLabel.Of(RegressionForm.Reciprocal));
        Assert.Equal("Standard Error", EnumLabel.Humanise("StandardError"));
        Assert.Equal("NRCS Lag", EnumLabel.Humanise("NRCSLag"));
    }

    [Fact]
    public void A_description_attribute_wins_over_the_split_name()
    {
        // Without it this would read "Reciprocal Y".
        Assert.Equal("Reciprocal-y", EnumLabel.Of(RegressionForm.ReciprocalY));
    }

    [Fact]
    public void The_drop_down_offers_the_labels_not_the_member_names()
    {
        IReadOnlyList<string> labels = EnumLabel.LabelsOf(typeof(RegressionForm));

        Assert.Contains("Reciprocal-y", labels);
        Assert.DoesNotContain("ReciprocalY", labels);
        Assert.Equal(Enum.GetValues<RegressionForm>().Length, labels.Count);
    }

    [Fact]
    public void A_label_or_a_raw_name_parses_back_to_the_value()
    {
        Assert.True(EnumLabel.TryParse(typeof(RegressionForm), "Reciprocal-y", out object? byLabel));
        Assert.Equal(RegressionForm.ReciprocalY, byLabel);

        Assert.True(EnumLabel.TryParse(typeof(RegressionForm), "reciprocaly", out object? byName));
        Assert.Equal(RegressionForm.ReciprocalY, byName);

        Assert.False(EnumLabel.TryParse(typeof(RegressionForm), "Made up", out _));
    }

    [Fact]
    public void Reports_show_the_label_not_the_member_name()
    {
        var regression = new SimpleRegression { Name = "R", SelectedForm = RegressionForm.ReciprocalY };
        StatistleProperty form = regression.Properties().Single(p => p.XmlName == nameof(SimpleRegression.SelectedForm));

        Assert.Equal("Reciprocal-y", regression.Display(form));

        Report report = ReportBuilder.For(regression, new UnitSelection());
        Assert.Contains(report.Sections.SelectMany(s => s.Values), v => v.Value == "Reciprocal-y");
    }
}

/// <summary>
/// Statistle is a learning tool, so a field with no explanation is a defect. Every shipped item
/// says what it is for, and every field of it says what it means and how to choose it.
/// </summary>
public class HelpTextTests
{
    private static IEnumerable<Type> ShippedTypes()
    {
        yield return typeof(DataTable);

        foreach (AnalysisDescriptor analysis in AnalysisCatalog.Discover(typeof(Analysis).Assembly))
        {
            yield return analysis.ToolType;
        }
    }

    [Fact]
    public void Every_shipped_item_has_overall_help()
    {
        foreach (Type type in ShippedTypes())
        {
            Assert.False(
                string.IsNullOrWhiteSpace(StatistleObject.HelpOf(type)),
                $"{type.Name} has no [StatistleHelp].");
        }
    }

    [Fact]
    public void Every_field_of_every_shipped_item_has_help()
    {
        // Name, Description and Id share one fallback in the editor; everything else must say
        // what it is, in the student's terms, on the property itself.
        string[] shared =
        [
            nameof(StatistleObject.Id),
            nameof(StatistleObject.Name),
            nameof(StatistleObject.Description),
        ];

        foreach (Type type in ShippedTypes())
        {
            IEnumerable<StatistleProperty> properties = StatistleObject
                .PropertiesOf(type)
                .Where(p => !shared.Contains(p.XmlName) && !p.Hidden);

            foreach (StatistleProperty property in properties)
            {
                Assert.True(property.HasHelp, $"{type.Name}.{property.XmlName} has no help text.");
            }
        }
    }

    [Fact]
    public void Help_is_written_in_sentences_rather_than_a_repeated_label()
    {
        // A one-word "help" that only repeats the label is worse than the fallback.
        foreach (Type type in ShippedTypes())
        {
            foreach (StatistleProperty property in StatistleObject.PropertiesOf(type).Where(p => p.HasHelp))
            {
                Assert.True(
                    property.Help.Length > 25,
                    $"{type.Name}.{property.XmlName} has help of {property.Help.Length} characters; " +
                    "say what it is and how to choose it.");
            }
        }
    }

    [Fact]
    public void Every_shipped_analysis_has_a_title_and_a_one_line_summary()
    {
        foreach (AnalysisDescriptor analysis in AnalysisCatalog.Discover(typeof(Analysis).Assembly))
        {
            Assert.False(string.IsNullOrWhiteSpace(analysis.Title), $"{analysis.Key} has no title.");
            Assert.False(string.IsNullOrWhiteSpace(analysis.Summary), $"{analysis.Key} has no summary.");
        }
    }
}
