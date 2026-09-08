using System.Globalization;
using Statistle.Core.Analyses;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>
/// The hand-over machinery: an analysis declares which property of which other object it can
/// fill, and the editor puts a Calculate… link there without naming either. Nothing ships with a
/// hand-over yet, so what is held here is the contract every future one must satisfy.
/// </summary>
public class ValueHandoffTests
{
    [Fact]
    public void Nothing_claims_to_fill_a_property_no_analysis_provides()
    {
        Assert.Null(AnalysisCatalog.ProviderFor(
            typeof(DescriptiveStatistics), nameof(DescriptiveStatistics.Sample)));
        Assert.Null(AnalysisCatalog.ProviderFor(
            typeof(SimpleRegression), nameof(SimpleRegression.Data)));
    }

    [Fact]
    public void The_provided_result_is_a_persisted_result_property_of_the_analysis()
    {
        // The hand-over reads the result by name off the analysis's own metadata, so the name
        // must resolve and must be a result rather than something the user types.
        foreach (AnalysisDescriptor analysis in AnalysisCatalog.Discover())
        {
            foreach (ProvidedValue provided in analysis.Provides)
            {
                StatistleProperty? result = StatistleObject.PropertiesOf(analysis.ToolType)
                    .FirstOrDefault(p => p.XmlName == provided.ResultProperty);

                Assert.NotNull(result);
                Assert.True(result.IsResult, $"{analysis.Title}: '{provided.ResultProperty}' is not a result.");

                Assert.Contains(
                    StatistleObject.PropertiesOf(provided.TargetType),
                    p => p.XmlName == provided.TargetProperty);
            }
        }
    }

    [Fact]
    public void A_seed_names_a_property_on_both_sides()
    {
        foreach (AnalysisDescriptor analysis in AnalysisCatalog.Discover())
        {
            foreach (ProvidedValue provided in analysis.Provides)
            {
                foreach ((string source, string target) in provided.Seeds)
                {
                    Assert.Contains(StatistleObject.PropertiesOf(analysis.ToolType), p => p.XmlName == source);
                    Assert.Contains(StatistleObject.PropertiesOf(provided.TargetType), p => p.XmlName == target);
                }
            }
        }
    }
}

/// <summary>
/// A sample is entered one value per line, not as (x, y) pairs — declared on the property, and
/// read as a person would type or paste it.
/// </summary>
public class SingleColumnSeriesTests
{
    [Fact]
    public void A_samples_values_are_declared_as_a_single_column()
    {
        StatistleProperty sample = new DescriptiveStatistics()
            .Properties()
            .Single(p => p.XmlName == nameof(DescriptiveStatistics.Sample));

        Assert.True(sample.IsSingleColumnSeries);
        Assert.Null(sample.StepProperty);

        // A regression's data is pairs, so it stays two-column.
        StatistleProperty pairs = new SimpleRegression()
            .Properties()
            .Single(p => p.XmlName == nameof(SimpleRegression.Data));

        Assert.False(pairs.IsSingleColumnSeries);
    }

    [Fact]
    public void With_no_step_property_the_x_column_is_the_observation_number()
    {
        var analysis = new DescriptiveStatistics();
        StatistleProperty sample = analysis.Properties().Single(p => p.XmlName == nameof(DescriptiveStatistics.Sample));

        Assert.Equal(1, sample.StepOf(analysis));

        analysis.Sample.SetValues([5, 6, 7], sample.StepOf(analysis));
        Assert.Equal([0, 1, 2], analysis.Sample.XValues);
    }

    [Fact]
    public void A_column_of_values_is_read_one_per_line_skipping_blanks()
    {
        bool ok = DataSeries.TryParseColumn(
            "0.1\r\n0.4\n\n  0.9  \n0.6\n", CultureInfo.InvariantCulture, out double[] values);

        Assert.True(ok);
        Assert.Equal([0.1, 0.4, 0.9, 0.6], values);
    }

    [Fact]
    public void A_typo_fails_the_whole_column_rather_than_dropping_a_row()
    {
        // Dropping the bad row would silently shorten the sample by one and change every result.
        bool ok = DataSeries.TryParseColumn(
            "0.1\n0.4\n0..9\n0.6", CultureInfo.InvariantCulture, out double[] values);

        Assert.False(ok);
        Assert.Empty(values);
    }

    [Fact]
    public void An_empty_column_is_an_empty_series()
    {
        Assert.True(DataSeries.TryParseColumn("   \n\n", CultureInfo.InvariantCulture, out double[] values));
        Assert.Empty(values);
    }

    [Fact]
    public void Pairs_are_read_with_any_of_the_usual_separators()
    {
        Assert.True(DataSeries.TryParsePairs(
            "1,2\n3;4\n5\t6\n7 8", CultureInfo.InvariantCulture, out DataSeries pairs));

        Assert.Equal([1, 3, 5, 7], pairs.XValues);
        Assert.Equal([2, 4, 6, 8], pairs.YValues);
    }

    [Fact]
    public void A_line_that_is_not_two_numbers_rejects_the_whole_paste()
    {
        Assert.False(DataSeries.TryParsePairs(
            "1,2\n3,four", CultureInfo.InvariantCulture, out DataSeries pairs));

        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void Set_values_spaces_rows_at_the_step()
    {
        var series = new DataSeries();
        series.Add(99, 99);

        series.SetValues([1, 2, 3], 0.25);

        Assert.Equal([0, 0.25, 0.5], series.XValues);
        Assert.Equal([1, 2, 3], series.YValues);
    }

    [Fact]
    public void Respace_ignores_a_step_that_makes_no_sense()
    {
        var series = new DataSeries();
        series.SetValues([1, 2], 0.25);

        series.Respace(0);
        series.Respace(double.NaN);

        Assert.Equal([0, 0.25], series.XValues);
    }
}
