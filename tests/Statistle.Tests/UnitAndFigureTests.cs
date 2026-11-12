using Statistle.Core.Analyses;
using Statistle.Core.Domain;
using Statistle.Core.Methods.Regression;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Tests;

/// <summary>
/// Statistics has no unit system to convert between: a mean carries the units of the data, which
/// Statistle never sees. What a property declares is the <em>kind</em> of number it is, and the
/// only kind that shows a label is a percentage. The conversion plumbing stays, and these tests
/// hold it to the identity, so a later kind that does convert has somewhere sound to land.
/// </summary>
public class ValueKindTests
{
    [Fact]
    public void Nothing_converts_between_a_stored_value_and_a_shown_value()
    {
        var units = new UnitSelection();

        foreach (ValueKind kind in Enum.GetValues<ValueKind>())
        {
            Assert.Equal(123.456, units.ToDisplay(kind, 123.456), 12);
            Assert.Equal(123.456, units.ToBase(kind, 123.456), 12);
            Assert.Equal(123.456, units.ToBase(kind, units.ToDisplay(kind, 123.456)), 12);
        }
    }

    [Fact]
    public void Every_kind_offers_exactly_one_option_whose_factor_is_one()
    {
        foreach (ValueKind kind in Enum.GetValues<ValueKind>())
        {
            UnitOption only = Assert.Single(UnitSystem.OptionsFor(kind));

            Assert.Equal(1.0, only.FactorFromBase);
            Assert.Same(only, UnitSystem.BaseUnit(kind));
            Assert.Equal(0, units_IndexOf(kind));
        }

        static int units_IndexOf(ValueKind kind) => new UnitSelection().IndexOf(kind);
    }

    [Fact]
    public void Only_a_percentage_shows_a_label()
    {
        var units = new UnitSelection();

        Assert.Equal("%", units.LabelFor(ValueKind.Percent));
        Assert.Equal(string.Empty, units.LabelFor(ValueKind.None));
        Assert.Equal(string.Empty, units.LabelFor(ValueKind.Probability));
        Assert.Equal(string.Empty, units.LabelFor(ValueKind.Count));
    }

    [Fact]
    public void A_property_declares_its_kind_and_the_editor_reads_it_from_there()
    {
        IReadOnlyList<StatistleProperty> properties = new DescriptiveStatistics().Properties();

        Assert.Equal(
            ValueKind.Percent,
            properties.Single(p => p.XmlName == nameof(DescriptiveStatistics.CoefficientOfVariation)).Unit);
        Assert.Equal(
            ValueKind.Count,
            properties.Single(p => p.XmlName == nameof(DescriptiveStatistics.Count)).Unit);
        Assert.Equal(
            ValueKind.None,
            properties.Single(p => p.XmlName == nameof(DescriptiveStatistics.Mean)).Unit);
    }

    [Fact]
    public void Identity_properties_lead_the_property_list()
    {
        // Reflection lists a type's own properties first, which would bury Name at the bottom
        // of every editor and report.
        IReadOnlyList<StatistleProperty> properties = new DataTable().Properties();

        Assert.Equal(nameof(StatistleObject.Id), properties[0].XmlName);
        Assert.Equal(nameof(StatistleObject.Name), properties[1].XmlName);
    }
}

public class RegressionFigureTests
{
    private static DataSeries Points()
    {
        var data = new DataSeries();
        data.Add(1, 2.1);
        data.Add(2, 3.9);
        data.Add(3, 6.2);
        data.Add(4, 7.8);
        return data;
    }

    [Fact]
    public void The_observations_are_drawn_as_markers()
    {
        DataSeries data = Points();
        PlotSpec spec = RegressionFigures.Scatter("Fit", "x", "y", data.XValues, data.YValues, fitted: null);

        var markers = (MarkerSeries)Assert.Single(spec.Series);
        Assert.Equal(4, markers.Y.Count);
        Assert.Equal("Fit", spec.Title);
    }

    [Fact]
    public void A_fitted_line_is_drawn_through_them()
    {
        DataSeries data = Points();
        LinearModel model = LinearModel.Fit(["x"], [data.XValues], data.YValues)!;

        PlotSpec spec = RegressionFigures.Scatter("Fit", "x", "y", data.XValues, data.YValues, x => model.Intercept + model.Coefficients[1].Estimate * x);

        Assert.Equal(2, spec.Series.Count);
        Assert.IsType<LineSeries>(spec.Series[1]);
    }

    [Fact]
    public void The_legend_keeps_clear_of_the_last_observations()
    {
        // Rising data end in the upper right, so the legend goes to the lower right; falling data, the reverse.
        double[] x = [1, 2, 3, 10], up = [2, 4, 5, 20], down = [20, 9, 7, 1];

        Assert.Equal(LegendCorner.LowerRight, RegressionFigures.Scatter("Fit", "x", "y", x, up, v => 2 * v).Legend);
        Assert.Equal(LegendCorner.UpperRight, RegressionFigures.Scatter("Fit", "x", "y", x, down, v => 20 - 2 * v).Legend);
        Assert.Equal(LegendCorner.LowerRight, RegressionFigures.Scatter("Fit", "x", "y", x, up, fitted: null).Legend);
        Assert.Equal(LegendCorner.UpperRight, RegressionFigures.Scatter("Fit", "x", "y", x, down, fitted: null).Legend);
    }

    [Fact]
    public void A_curve_in_one_over_x_does_not_cross_zero()
    {
        // Cost per part against 1/batch: the 3 % margin past the data would reach x < 0 and the curve 1/x would shoot off.
        double[] x = [10, 20, 50, 100, 400], y = [43, 23.5, 11.8, 7.9, 5];
        PlotSpec spec = RegressionFigures.Scatter("Fit", "x", "y", x, y, v => 4 + 390 / v);

        var curve = Assert.IsType<LineSeries>(spec.Series[1]);
        Assert.Equal(10, curve.X.Min(), 12);
        Assert.True(curve.X.Max() > 400);
        Assert.InRange(curve.Y.Max(), 42.9, 43.1);
    }

    [Fact]
    public void An_analysis_declares_its_own_figures_and_nothing_else_names_them()
    {
        var regression = new SimpleRegression();

        IReadOnlyList<PlotSpec> figures = regression.Figures(new UnitSelection());

        Assert.Equal(3, figures.Count);
        Assert.All(figures, f => Assert.NotEmpty(f.Series));
    }

    [Fact]
    public void An_empty_series_still_produces_a_drawable_figure()
    {
        PlotSpec spec = RegressionFigures.Scatter("Fit", "x", "y", [], [], fitted: null);

        Assert.NotEmpty(spec.Axes);
        Assert.Single(spec.Series);
    }
}
