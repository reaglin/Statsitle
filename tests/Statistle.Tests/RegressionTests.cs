using Statistle.Core.Methods;
using Statistle.Core.Methods.Statistics;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Analyses;

namespace Statistle.Tests;

/// <summary>
/// Regression on the six common forms. Exact data recovers exact coefficients; data a form cannot
/// take is skipped and counted; and the tool ranks forms by the standard error in y units.
/// </summary>
public class PairEntryTests
{
    private static readonly System.Globalization.CultureInfo Us = System.Globalization.CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("1\t2\n3\t4")]
    [InlineData("1,2\r\n3,4")]
    [InlineData("1;2\n3;4")]
    [InlineData("1 2\n3 4")]
    [InlineData("1, 2\n\n3;\t4\n")]
    [InlineData("\"1\",\"2\"\n\"3\",\"4\"")]
    [InlineData("\uFEFF1,2,extra\n3,4,ignored")]
    public void Pairs_are_read_with_a_tab_comma_semicolon_or_space_between_them(string text)
    {
        Assert.True(DataSeries.TryParsePairs(text, Us, out DataSeries series));
        Assert.Equal([1.0, 3.0], series.XValues);
        Assert.Equal([2.0, 4.0], series.YValues);
    }

    [Theory]
    [InlineData("1,2\nx,y")]
    [InlineData("1,2\n3")]
    [InlineData("1,2\n3,4.5.6")]
    public void A_line_that_is_not_two_numbers_rejects_the_whole_read(string text)
    {
        Assert.False(DataSeries.TryParsePairs(text, Us, out DataSeries series));
        Assert.Equal(0, series.Count);
    }

    [Fact]
    public void Blank_text_is_an_empty_series_not_an_error()
    {
        Assert.True(DataSeries.TryParsePairs("  \n ", Us, out DataSeries series));
        Assert.Equal(0, series.Count);
    }
}

public class RegressionTests
{
    private static readonly double[] X = [1, 2, 3, 4, 5, 6, 7, 8];

    private static double[] Of(Func<double, double> f) => [.. X.Select(f)];

    [Theory]
    [InlineData(RegressionForm.Linear, 2.0, 3.0)]
    [InlineData(RegressionForm.Exponential, 2.0, 0.5)]
    [InlineData(RegressionForm.Power, 3.0, 1.5)]
    [InlineData(RegressionForm.Logarithmic, 1.0, 2.0)]
    [InlineData(RegressionForm.Reciprocal, 1.0, 2.0)]
    [InlineData(RegressionForm.ReciprocalY, 0.5, 0.1)]
    public void Exact_data_recovers_the_coefficients_with_r2_one_and_no_error(RegressionForm form, double a, double b)
    {
        double[] y = Of(x => new RegressionResult(form, a, b, 0, 0, 0, 0).Predict(x));

        RegressionResult? fit = Regression.Fit(form, X, y);

        Assert.NotNull(fit);
        Assert.Equal(a, fit.A, 9);
        Assert.Equal(b, fit.B, 9);
        Assert.Equal(1.0, fit.RSquared, 9);
        Assert.Equal(0.0, fit.StandardError, 7);
        Assert.Equal(X.Length, fit.PointsUsed);
        Assert.Equal(0, fit.PointsSkipped);
    }

    [Fact]
    public void Points_a_transform_cannot_take_are_skipped_and_counted_not_dropped_silently()
    {
        double[] x = [0, 1, 2, 3, 4, 5];
        double[] y = [-1, 2.7, 7.4, 20.1, 54.6, 148.4]; // e^x from x = 1, with a bad first point

        RegressionResult? exponential = Regression.Fit(RegressionForm.Exponential, x, y);
        RegressionResult? power = Regression.Fit(RegressionForm.Power, x, y);
        RegressionResult? linear = Regression.Fit(RegressionForm.Linear, x, y);

        Assert.NotNull(exponential);
        Assert.Equal(1, exponential.PointsSkipped);     // y ≤ 0
        Assert.Equal(5, exponential.PointsUsed);
        Assert.NotNull(power);
        Assert.Equal(1, power.PointsSkipped);           // x = 0 and y ≤ 0 is the same point
        Assert.NotNull(linear);
        Assert.Equal(0, linear.PointsSkipped);
    }

    [Fact]
    public void Fewer_than_three_usable_points_is_no_fit()
    {
        Assert.Null(Regression.Fit(RegressionForm.Linear, [1, 2], [1, 2]));
        Assert.Null(Regression.Fit(RegressionForm.Exponential, [1, 2, 3, 4], [-1, -1, 2, 3]));
    }

    [Fact]
    public void No_spread_in_x_is_no_fit()
    {
        Assert.Null(Regression.Fit(RegressionForm.Linear, [2, 2, 2, 2], [1, 2, 3, 4]));
    }

    [Fact]
    public void R_squared_stays_within_zero_and_one_on_noisy_data()
    {
        var random = new Random(7);
        double[] x = [.. Enumerable.Range(1, 40).Select(i => (double)i)];
        double[] y = [.. x.Select(v => 3 + (0.4 * v) + (random.NextDouble() * 4) - 2)];

        foreach (RegressionForm form in Enum.GetValues<RegressionForm>())
        {
            RegressionResult? fit = Regression.Fit(form, x, y);

            Assert.NotNull(fit);
            Assert.InRange(fit.RSquared, 0, 1);
            Assert.True(fit.StandardError >= 0);
        }
    }

    [Fact]
    public void Equations_read_with_the_right_sign()
    {
        Assert.Equal("y = 2 + 3·x", Regression.Equation(RegressionForm.Linear, 2, 3));
        Assert.Equal("y = 2 − 3·x", Regression.Equation(RegressionForm.Linear, 2, -3));
        Assert.Equal("y = 2·e^(-0.5·x)", Regression.Equation(RegressionForm.Exponential, 2, -0.5));
        Assert.Equal("y = 3·x^1.5", Regression.Equation(RegressionForm.Power, 3, 1.5));
        Assert.Equal("y = 1 + 2·ln x", Regression.Equation(RegressionForm.Logarithmic, 1, 2));
        Assert.Equal("y = 1 + 2 / x", Regression.Equation(RegressionForm.Reciprocal, 1, 2));
        Assert.Equal("y = 1 / (0.5 + 0.1·x)", Regression.Equation(RegressionForm.ReciprocalY, 0.5, 0.1));
    }
}

public class SimpleRegressionTests
{
    private static SimpleRegression With(RegressionForm form, double a, double b)
    {
        var tool = new SimpleRegression();
        tool.Data.Clear();

        var shape = new RegressionResult(form, a, b, 0, 0, 0, 0);
        foreach (double x in new[] { 1.0, 2, 3, 4, 5, 6, 8, 10 })
        {
            tool.Data.Add(x, shape.Predict(x));
        }

        tool.Recalculate();
        return tool;
    }

    [Fact]
    public void The_tool_is_discovered_as_a_free_standing_calculator()
    {
        Assert.Contains(AnalysisCatalog.OfKind(ToolKind.FreeStanding), t => t.ToolType == typeof(SimpleRegression));
    }

    [Fact]
    public void Six_methods_each_write_their_own_three_outputs_and_nothing_is_hidden()
    {
        var tool = new SimpleRegression();
        IReadOnlyList<ICalculationMethod> methods = MethodScope.ActiveMethods(tool);

        Assert.Equal(6, methods.Count);
        Assert.Empty(MethodScope.HiddenProperties(tool));
        Assert.All(methods, m => Assert.Empty(MethodContract.Verify(m)));
        Assert.All(methods, m => Assert.Equal(3, m.Outputs.Count));

        // No two methods claim the same output.
        string[] outputs = [.. methods.SelectMany(m => m.Outputs)];
        Assert.Equal(outputs.Length, outputs.Distinct().Count());
    }

    [Theory]
    [InlineData(RegressionForm.Power, 3.0, 1.5)]
    [InlineData(RegressionForm.Exponential, 2.0, 0.3)]
    [InlineData(RegressionForm.Logarithmic, 1.0, 2.0)]
    public void The_best_form_is_the_one_the_data_came_from(RegressionForm form, double a, double b)
    {
        SimpleRegression tool = With(form, a, b);

        Assert.Contains(form.ToString(), tool.BestForm, StringComparison.Ordinal);
        Assert.Equal(1.0, tool.Results[form]!.RSquared, 9);
    }

    [Fact]
    public void Exact_power_data_fills_the_power_block_and_leaves_the_others_imperfect()
    {
        SimpleRegression tool = With(RegressionForm.Power, 3, 1.5);

        Assert.Equal("y = 3·x^1.5", tool.PowerEquation);
        Assert.Equal(1.0, tool.PowerRSquared, 9);
        Assert.Equal(0.0, tool.PowerStandardError, 7);
        Assert.True(tool.LinearStandardError > tool.PowerStandardError);
        Assert.Equal(8, tool.Count);
    }

    [Fact]
    public void Too_little_data_says_so_rather_than_failing()
    {
        var tool = new SimpleRegression();
        tool.Data.Clear();
        tool.Data.Add(1, 1);
        tool.Data.Add(2, 2);
        tool.Recalculate();

        Assert.Contains("at least 3", tool.LinearEquation, StringComparison.Ordinal);
        Assert.Equal(0, tool.LinearRSquared);
        Assert.Equal(string.Empty, tool.BestForm);
        Assert.NotNull(tool.Figure); // the points still draw, with no curve
    }

    [Fact]
    public void The_figure_draws_the_observations_and_the_selected_form()
    {
        SimpleRegression tool = With(RegressionForm.Power, 3, 1.5);
        tool.SelectedForm = RegressionForm.Power;

        PlotSpec? figure = tool.Figure;

        Assert.NotNull(figure);
        MarkerSeries points = Assert.Single(figure.Series.OfType<MarkerSeries>());
        LineSeries curve = Assert.Single(figure.Series.OfType<LineSeries>());

        Assert.Equal(8, points.X.Count);
        Assert.True(curve.X.Count > 50);
        Assert.Equal(tool.PowerEquation, curve.Name);
        Assert.Contains("r² = 1.0000", figure.Title, StringComparison.Ordinal);

        // The curve runs over the data's x range and reproduces it there.
        Assert.Equal(1.0, curve.X[0], 9);
        Assert.Equal(10.0, curve.X[^1], 9);
        Assert.Equal(3 * Math.Pow(10, 1.5), curve.Y[^1], 6);
    }

    [Fact]
    public void Pairs_can_be_pasted_with_commas_or_spaces_as_well_as_tabs()
    {
        DataSeries series = DataSeries.Parse("1, 2.5\n2  4.75\n3;7\n4\t9.5", "test");

        Assert.Equal([1, 2, 3, 4], series.XValues);
        Assert.Equal([2.5, 4.75, 7, 9.5], series.YValues);
    }

    [Fact]
    public void The_data_series_is_two_column_not_single()
    {
        StatistleProperty data = new SimpleRegression().Properties().Single(p => p.XmlName == nameof(SimpleRegression.Data));

        Assert.False(data.IsSingleColumnSeries);
    }
}
