using Statistle.Core.Analyses;
using Statistle.Core.Methods.Statistics;
using Statistle.Tests.Harness;

namespace Statistle.Tests;

/// <summary>
/// The summary statistics have exact closed forms, so they are checked against those and against
/// invariants that hold for any correct implementation — not against any predecessor's output.
/// </summary>
public class SeriesStatisticsTests
{
    // A worked sample small enough to check by hand: n = 8, mean 5, s = 2.
    private static readonly double[] Sample = [2, 4, 4, 4, 5, 5, 7, 9];

    [Fact]
    public void Mean_and_sample_standard_deviation_match_the_closed_form()
    {
        Assert.Equal(5.0, SeriesStatistics.Mean(Sample), 12);

        // Σ(x − x̄)² = 32, divided by n − 1 = 7.
        Assert.Equal(Math.Sqrt(32.0 / 7.0), SeriesStatistics.StandardDeviation(Sample), 12);
        Assert.Equal(32.0 / 7.0, SeriesStatistics.Variance(Sample), 12);
    }

    [Fact]
    public void The_median_is_the_middle_value_and_the_mean_of_the_middle_two_when_n_is_even()
    {
        Assert.Equal(4.5, SeriesStatistics.Median(Sample), 12);          // (4 + 5) / 2
        Assert.Equal(4.0, SeriesStatistics.Median([1, 4, 9]), 12);
        Assert.Equal(7.0, SeriesStatistics.Median([7]), 12);
    }

    [Fact]
    public void Quartiles_interpolate_between_the_order_statistics()
    {
        // Sorted: 2 4 4 4 5 5 7 9. Q1 sits at rank 0.25·7 = 1.75, between 4 and 4; Q3 at 5.25,
        // a quarter of the way from 5 to 7.
        Assert.Equal(4.0, SeriesStatistics.Percentile(Sample, 0.25), 12);
        Assert.Equal(5.5, SeriesStatistics.Percentile(Sample, 0.75), 12);
        Assert.Equal(2.0, SeriesStatistics.Percentile(Sample, 0.0), 12);
        Assert.Equal(9.0, SeriesStatistics.Percentile(Sample, 1.0), 12);
    }

    [Fact]
    public void A_symmetric_sample_has_no_skew_and_a_long_right_tail_has_positive_skew()
    {
        Assert.Equal(0.0, SeriesStatistics.Skew([1, 2, 3, 4, 5]), 12);
        Assert.True(SeriesStatistics.Skew([1, 1, 1, 2, 10]) > 0);
        Assert.True(SeriesStatistics.Skew([1, 9, 10, 10, 10]) < 0);
    }

    [Fact]
    public void A_sample_with_no_spread_has_zero_standard_deviation_and_zero_skew()
    {
        Assert.Equal(0.0, SeriesStatistics.StandardDeviation([3, 3, 3, 3]), 12);
        Assert.Equal(0.0, SeriesStatistics.Skew([3, 3, 3, 3]), 12);
    }

    [Fact]
    public void Too_few_values_is_an_error_rather_than_a_wrong_number()
    {
        Assert.Throws<ArgumentException>(() => SeriesStatistics.Mean([]));
        Assert.Throws<ArgumentException>(() => SeriesStatistics.StandardDeviation([1]));
        Assert.Throws<ArgumentException>(() => SeriesStatistics.Skew([1, 2]));
        Assert.Throws<ArgumentOutOfRangeException>(() => SeriesStatistics.Percentile(Sample, 1.5));
    }

    [Fact]
    public void Ranked_sorts_largest_first()
    {
        Assert.Equal([9, 7, 5, 5, 4, 4, 4, 2], SeriesStatistics.Ranked(Sample));
    }

    // ── Invariants, swept across the input range ──────────────────────────────

    [Fact]
    public void Shifting_every_value_shifts_the_mean_and_the_median_by_the_same_amount()
    {
        Invariants.Conserves(
            shift => SeriesStatistics.Mean(Sample) + shift,
            shift => SeriesStatistics.Mean([.. Sample.Select(v => v + shift)]),
            -1000,
            1000,
            "The mean is location-equivariant: adding c to every value adds c to the mean.",
            tolerance: 1e-9);

        Invariants.Conserves(
            shift => SeriesStatistics.Median(Sample) + shift,
            shift => SeriesStatistics.Median([.. Sample.Select(v => v + shift)]),
            -1000,
            1000,
            "So is the median.",
            tolerance: 1e-9);
    }

    [Fact]
    public void Shifting_every_value_leaves_the_spread_alone()
    {
        double s = SeriesStatistics.StandardDeviation(Sample);

        Invariants.Conserves(
            _ => s,
            shift => SeriesStatistics.StandardDeviation([.. Sample.Select(v => v + shift)]),
            -1000,
            1000,
            "The standard deviation measures spread, which a shift does not change.",
            tolerance: 1e-9);
    }

    [Fact]
    public void Scaling_every_value_scales_the_standard_deviation_by_the_same_factor()
    {
        double s = SeriesStatistics.StandardDeviation(Sample);

        Invariants.Conserves(
            scale => s * scale,
            scale => SeriesStatistics.StandardDeviation([.. Sample.Select(v => v * scale)]),
            1,
            100,
            "The standard deviation is in the units of the data, so it scales with them.",
            tolerance: 1e-9);
    }

    [Fact]
    public void The_median_always_lies_between_the_smallest_and_largest_value()
    {
        Invariants.Bounded(
            shift => SeriesStatistics.Median([.. Sample.Select(v => v + shift)]) - shift,
            -100,
            100,
            Sample.Min(),
            Sample.Max(),
            "A median is one of the sample's own values, or between two of them.");
    }

    [Fact]
    public void The_standard_deviation_is_finite_and_never_negative_however_the_sample_is_scaled()
    {
        Invariants.Finite(
            scale => SeriesStatistics.StandardDeviation([.. Sample.Select(v => v * scale)]),
            0,
            1e6,
            "A spread computed from finite numbers is finite.");

        Invariants.Bounded(
            scale => SeriesStatistics.StandardDeviation([.. Sample.Select(v => v * scale)]),
            0,
            1e6,
            0,
            double.MaxValue,
            "A standard deviation is a distance, so it cannot be negative.");
    }

    [Fact]
    public void The_quartiles_never_cross_as_the_fraction_rises()
    {
        Invariants.NonDecreasing(
            fraction => SeriesStatistics.Percentile(Sample, fraction),
            0,
            1,
            "A higher percentile can never be a smaller value.");
    }
}

/// <summary>The analysis writes exactly what the closed forms give, and stays sane when empty.</summary>
public class DescriptiveStatisticsTests
{
    private static DescriptiveStatistics Of(params double[] values)
    {
        var analysis = new DescriptiveStatistics();
        analysis.Sample.SetValues(values, 1);
        analysis.Recalculate();
        return analysis;
    }

    [Fact]
    public void Every_summary_number_matches_the_engine()
    {
        DescriptiveStatistics analysis = Of(2, 4, 4, 4, 5, 5, 7, 9);

        Assert.Equal(8, analysis.Count);
        Assert.Equal(5.0, analysis.Mean, 12);
        Assert.Equal(4.5, analysis.Median, 12);
        Assert.Equal(Math.Sqrt(32.0 / 7.0), analysis.StandardDeviation, 12);
        Assert.Equal(32.0 / 7.0, analysis.Variance, 12);
        Assert.Equal(Math.Sqrt(32.0 / 7.0) / Math.Sqrt(8), analysis.StandardError, 12);
        Assert.Equal(100 * Math.Sqrt(32.0 / 7.0) / 5.0, analysis.CoefficientOfVariation, 12);
        Assert.Equal(2, analysis.Minimum);
        Assert.Equal(9, analysis.Maximum);
        Assert.Equal(7, analysis.Range);
        Assert.Equal(4.0, analysis.FirstQuartile, 12);
        Assert.Equal(5.5, analysis.ThirdQuartile, 12);
        Assert.Equal(1.5, analysis.InterquartileRange, 12);
    }

    [Fact]
    public void An_empty_sample_leaves_every_result_blank_rather_than_throwing()
    {
        DescriptiveStatistics analysis = Of();

        Assert.Equal(0, analysis.Count);
        Assert.True(double.IsNaN(analysis.Mean));
        Assert.True(double.IsNaN(analysis.StandardDeviation));
        Assert.True(double.IsNaN(analysis.Skewness));
        Assert.Equal(string.Empty, analysis.Mode);
        Assert.Empty(analysis.Figures(null));
    }

    [Fact]
    public void One_value_gives_a_centre_but_no_spread()
    {
        DescriptiveStatistics analysis = Of(7);

        Assert.Equal(1, analysis.Count);
        Assert.Equal(7, analysis.Mean);
        Assert.Equal(7, analysis.Median);
        Assert.Equal(7, analysis.Minimum);
        Assert.True(double.IsNaN(analysis.StandardDeviation), "one value has no spread to estimate");
        Assert.True(double.IsNaN(analysis.StandardError));
    }

    [Fact]
    public void Two_values_give_a_spread_but_no_skew()
    {
        DescriptiveStatistics analysis = Of(4, 6);

        Assert.Equal(5, analysis.Mean, 12);
        Assert.Equal(Math.Sqrt(2), analysis.StandardDeviation, 12);
        Assert.True(double.IsNaN(analysis.Skewness));
    }

    [Fact]
    public void A_mean_of_zero_leaves_the_coefficient_of_variation_undefined_rather_than_infinite()
    {
        DescriptiveStatistics analysis = Of(-1, 0, 1);

        Assert.Equal(0, analysis.Mean, 12);
        Assert.True(double.IsNaN(analysis.CoefficientOfVariation), "blank, not infinite or zero");
    }

    [Fact]
    public void The_sample_project_numbers_come_out_as_the_course_expects()
    {
        // AA2024 tensile strength, MPa: the first table of samples/demo-statistics.statistle.
        DescriptiveStatistics analysis = Of(470, 485, 478, 492, 460, 473, 488, 484, 482, 475);

        Assert.Equal(10, analysis.Count);
        Assert.Equal(478.7, analysis.Mean, 9);
        Assert.Equal(480.0, analysis.Median, 9);
        Assert.Equal(460, analysis.Minimum);
        Assert.Equal(492, analysis.Maximum);
        Assert.InRange(analysis.StandardDeviation, 9.0, 10.5);
    }
}
