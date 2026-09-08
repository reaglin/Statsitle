using Statistle.Core.Analyses;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Tests;

/// <summary>
/// An analysis whose properties carry the whole range of metadata, so the tests below exercise
/// the mechanism rather than whatever a shipped analysis happens to declare today.
/// </summary>
public sealed class MetadataProbe : Analysis
{
    private double _confidence = 95;
    private double _sampleSize = 30;
    private double _mean;

    public override string Title => "Metadata probe";

    public override string Summary => "Exists only to exercise ranges, roles and labels";

    public override ToolKind Kind => ToolKind.FreeStanding;

    [StatistleProperty(Label = "Confidence level", ReportLabel = "Confidence level (1 - alpha)",
        Decimals = 1, Unit = ValueKind.Percent, Role = VariableRole.Design, Min = 50, Max = 99.9,
        Help = "How sure the interval is meant to be.")]
    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    [StatistleProperty(Label = "Sample size", Decimals = 0, Unit = ValueKind.Count,
        Role = VariableRole.Input, Min = 0,
        Help = "How many measurements were taken.")]
    public double SampleSize
    {
        get => _sampleSize;
        set => SetProperty(ref _sampleSize, value);
    }

    [StatistleProperty(Label = "Sample mean", Decimals = 4, Role = VariableRole.Output,
        Help = "The mean of the sample.")]
    public double Mean
    {
        get => _mean;
        set => SetProperty(ref _mean, value);
    }
}

/// <summary>
/// Ranges and roles are declared on the variable, so one implementation enforces them
/// everywhere. The approach follows BMP Trains' <c>Meta</c> attribute.
/// </summary>
public class VariableMetadataTests
{
    private static StatistleProperty PropertyOf<T>(string name)
        where T : StatistleObject, new() =>
        new T().Properties().Single(p => p.XmlName == name);

    [Fact]
    public void A_property_without_a_declared_range_is_unbounded()
    {
        StatistleProperty description = PropertyOf<MetadataProbe>(nameof(StatistleObject.Description));

        Assert.False(description.HasRange);
        Assert.True(description.IsInRange(1e9));
    }

    [Theory]
    [InlineData(50, true)]     // bounds are inclusive
    [InlineData(95, true)]
    [InlineData(99.9, true)]
    [InlineData(49.9, false)]
    [InlineData(100, false)]
    public void A_declared_range_is_enforced_inclusively(double value, bool expected)
    {
        StatistleProperty confidence = PropertyOf<MetadataProbe>(nameof(MetadataProbe.Confidence));

        Assert.Equal(expected, confidence.IsInRange(value));
    }

    [Fact]
    public void A_one_sided_range_only_constrains_that_side()
    {
        StatistleProperty size = PropertyOf<MetadataProbe>(nameof(MetadataProbe.SampleSize));

        Assert.True(size.HasMin);
        Assert.False(size.HasMax);
        Assert.False(size.IsInRange(-1));
        Assert.True(size.IsInRange(1e6));
    }

    [Fact]
    public void Validation_reports_every_violation_on_the_object()
    {
        var probe = new MetadataProbe
        {
            Confidence = 120,
            SampleSize = -5,
        };

        IReadOnlyList<string> violations = probe.Validate();

        Assert.Equal(2, violations.Count);
        Assert.Contains(violations, v => v.Contains("Confidence level", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.Contains("Sample size", StringComparison.Ordinal));
    }

    [Fact]
    public void A_valid_object_reports_nothing()
    {
        Assert.Empty(new MetadataProbe { Confidence = 90, SampleSize = 12 }.Validate());
    }

    [Fact]
    public void A_one_sided_range_reads_naturally()
    {
        StatistleProperty size = PropertyOf<MetadataProbe>(nameof(MetadataProbe.SampleSize));

        Assert.Contains("at least 0", size.RangeText(new UnitSelection()), StringComparison.Ordinal);
    }

    [Fact]
    public void A_two_sided_range_reads_naturally()
    {
        StatistleProperty confidence = PropertyOf<MetadataProbe>(nameof(MetadataProbe.Confidence));

        Assert.Equal("between 50 and 99.9", confidence.RangeText());
    }

    [Fact]
    public void A_violation_message_names_the_variable_and_its_range()
    {
        StatistleProperty confidence = PropertyOf<MetadataProbe>(nameof(MetadataProbe.Confidence));

        string? message = confidence.RangeViolation(120);

        Assert.Equal("Confidence level must be between 50 and 99.9.", message);
        Assert.Null(confidence.RangeViolation(95));
    }

    [Fact]
    public void Results_are_distinguishable_from_inputs_by_role()
    {
        // The editor uses this to decide what is typed into, without knowing any variable name.
        StatistleProperty design = PropertyOf<MetadataProbe>(nameof(MetadataProbe.Confidence));
        StatistleProperty input = PropertyOf<MetadataProbe>(nameof(MetadataProbe.SampleSize));
        StatistleProperty output = PropertyOf<MetadataProbe>(nameof(MetadataProbe.Mean));

        Assert.Equal(VariableRole.Design, design.Role);
        Assert.Equal(VariableRole.Input, input.Role);
        Assert.False(design.IsResult);
        Assert.False(input.IsResult);
        Assert.True(output.IsResult);
    }

    [Fact]
    public void Report_label_falls_back_to_the_interface_label()
    {
        StatistleProperty withBoth = PropertyOf<MetadataProbe>(nameof(MetadataProbe.Confidence));
        StatistleProperty withOne = PropertyOf<MetadataProbe>(nameof(MetadataProbe.SampleSize));

        Assert.Equal("Confidence level (1 - alpha)", withBoth.ReportLabel);
        Assert.Equal("Confidence level", withBoth.Label);
        Assert.Equal(withOne.Label, withOne.ReportLabel);
    }

    [Fact]
    public void Range_metadata_does_not_change_what_is_stored()
    {
        // Ranges guide entry; they must not silently rewrite a value already in a file. An
        // out-of-range value loads as it is and is reported by Validate().
        var probe = new MetadataProbe();
        StatistleXml.Read(
            probe,
            new System.Xml.Linq.XElement("analysis", new System.Xml.Linq.XElement("Confidence", "150")),
            null);

        Assert.Equal(150, probe.Confidence);
        Assert.Single(probe.Validate());
    }
}
