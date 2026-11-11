using Statistle.Core.Analyses;
using Statistle.Core.Plotting;

namespace Statistle.Tests;

/// <summary>
/// Plan 10.11 and 10.16: the control limits and the control chart, the number of outliers and the
/// mode as a number, so an exercise can ask for them.
/// </summary>
public class DescriptiveAdditionsTests
{
    private static DescriptiveStatistics With(params double[] values)
    {
        var analysis = new DescriptiveStatistics();
        analysis.Sample.Clear();

        foreach (double v in values)
        {
            analysis.Sample.Add(analysis.Sample.Count, v);
        }

        analysis.Recalculate();
        return analysis;
    }

    [Fact]
    public void The_control_limits_are_the_mean_plus_and_minus_three_sample_standard_deviations()
    {
        // 2, 4, 4, 4, 5, 5, 7, 9: mean 5, sample s = √(32/7)
        DescriptiveStatistics d = With(2, 4, 4, 4, 5, 5, 7, 9);
        double s = Math.Sqrt(32.0 / 7);

        Assert.Equal(5 - 3 * s, d.LowerControlLimit, 12);
        Assert.Equal(5 + 3 * s, d.UpperControlLimit, 12);
        Assert.Equal(0, d.PointsOutsideLimits);
    }

    [Fact]
    public void A_point_beyond_a_limit_is_counted_and_marked_on_the_chart()
    {
        // Twenty values near 10 and one at 30: x̄ ± 3s still leaves 30 outside.
        double[] values = [.. Enumerable.Range(0, 20).Select(i => 10 + (i % 5 - 2) * 0.1), 30];
        DescriptiveStatistics d = With(values);

        Assert.True(d.UpperControlLimit < 30);
        Assert.Equal(1, d.PointsOutsideLimits);

        IReadOnlyList<PlotSpec> figures = d.Figures(null);
        Assert.Equal(3, figures.Count);
        Assert.EndsWith("histogram", figures[0].Title, StringComparison.Ordinal);   // figure numbers 1 and 2 unchanged for the course pages
        Assert.EndsWith("box plot", figures[1].Title, StringComparison.Ordinal);
        Assert.EndsWith("control chart", figures[2].Title, StringComparison.Ordinal);

        MarkerSeries outside = figures[2].Series.OfType<MarkerSeries>().Single(m => m.Name == "Outside the limits");
        Assert.Equal([21.0], outside.X);
        Assert.Equal([30.0], outside.Y);
        Assert.Contains(figures[2].Series, s => s.Name.StartsWith("UCL = ", StringComparison.Ordinal));
        Assert.Contains(figures[2].Series, s => s.Name.StartsWith("LCL = ", StringComparison.Ordinal));
    }

    [Fact]
    public void With_one_value_there_are_no_limits_and_no_chart()
    {
        DescriptiveStatistics d = With(4.2);

        Assert.True(double.IsNaN(d.LowerControlLimit));
        Assert.True(double.IsNaN(d.UpperControlLimit));
        Assert.Equal(0, d.PointsOutsideLimits);
        Assert.Equal(2, d.Figures(null).Count);
    }

    [Fact]
    public void The_number_of_outliers_counts_the_values_beyond_the_type_7_fences()
    {
        DescriptiveStatistics d = With(470, 485, 478, 492, 460, 473, 488, 484, 482, 475, 530);
        Assert.Equal(1, d.OutlierCount);
        Assert.Equal("530", d.Outliers);

        DescriptiveStatistics two = With(1, 2, 3, 4, 5, 6, 7, 8, -40, 60);
        Assert.Equal(2, two.OutlierCount);
        Assert.Equal(0, With(3.2, 4.8, 2.9, 6.1).OutlierCount);
    }

    [Fact]
    public void The_mode_is_a_number_only_when_one_value_is_the_most_frequent()
    {
        DescriptiveStatistics one = With(3, 5, 5, 7, 8);
        Assert.Equal(5, one.ModeValue);
        Assert.Equal("5", one.Mode);

        DescriptiveStatistics tie = With(3, 3, 5, 5, 7);
        Assert.True(double.IsNaN(tie.ModeValue));
        Assert.Contains(",", tie.Mode, StringComparison.Ordinal);

        DescriptiveStatistics none = With(1, 2, 3);
        Assert.True(double.IsNaN(none.ModeValue));
        Assert.Equal("none", none.Mode);
    }
}
