using System.Globalization;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Regression;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>Pairs typed or pasted into a regression are read leniently but never silently wrongly.</summary>
public class PairEntryTests
{
    private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("1\t2\n3\t4")]
    [InlineData("1,2\r\n3,4")]
    [InlineData("1;2\n3;4")]
    [InlineData("1 2\n3 4")]
    [InlineData("1, 2\n\n3;\t4\n")]
    [InlineData("\"1\",\"2\"\n\"3\",\"4\"")]
    [InlineData("﻿1,2,extra\n3,4,ignored")]
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

/// <summary>Modules 11–13: the OLS engine against hand arithmetic, and the two regression analyses.</summary>
public class RegressionTests
{
    [Fact]
    public void The_engine_matches_the_textbook_hand_example()
    {
        // x = 1..5, y = 2, 4, 5, 4, 5: b₁ = 0.6, b₀ = 2.2, SS_E = 2.4, σ̂² = 0.8, se(b₁) = √(0.8/10), R² = 0.6, F = 4.5.
        LinearModel? m = LinearModel.Fit(["x"], [[1, 2, 3, 4, 5]], [2, 4, 5, 4, 5]);
        Assert.NotNull(m);
        Assert.Equal(2.2, m.Intercept, 9);
        Assert.Equal(0.6, m.Coefficients[1].Estimate, 9);
        Assert.Equal(2.4, m.SumSquaresError, 9);
        Assert.Equal(6.0, m.SumSquaresTotal, 9);
        Assert.Equal(0.8, m.MeanSquareError, 9);
        Assert.Equal(Math.Sqrt(0.08), m.Coefficients[1].StandardError, 9);
        Assert.Equal(0.6 / Math.Sqrt(0.08), m.Coefficients[1].T, 9);
        Assert.Equal(0.6, m.RSquared, 9);
        Assert.Equal(4.5, m.F, 9);
        Assert.Equal(m.Coefficients[1].PValue, m.FPValue, 9);
        Assert.Equal(3, m.DegreesOfFreedomError);
        Assert.Equal(2, m.Leverage.Sum(), 9);                      // Σhᵢᵢ = p
        Assert.Equal(0.6, m.Leverage[0], 9);                        // 1/5 + (1 − 3)²/10
        Assert.Equal(1, m.Coefficients[1].Vif, 9);

        // Prediction at x = 3 (the mean): the narrowest interval, se = σ̂/√n.
        Prediction at = m.Predict([3]);
        Assert.Equal(4.0, at.Fitted, 9);
        Assert.Equal(Math.Sqrt(0.8 / 5), at.StandardError, 9);
        Assert.True(at.ObservationUpper - at.ObservationLower > at.MeanUpper - at.MeanLower);

        // An exact plane is recovered with R² = 1; a collinear design is refused.
        double[] x1 = [1, 2, 3, 4, 5, 6], x2 = [2, 1, 4, 3, 6, 5];
        LinearModel? plane = LinearModel.Fit(["a", "b"], [x1, x2], [.. x1.Zip(x2, (a, b) => 1 + 2 * a + 3 * b)]);
        Assert.NotNull(plane);
        Assert.Equal(2, plane.Coefficients[1].Estimate, 9);
        Assert.Equal(3, plane.Coefficients[2].Estimate, 9);
        Assert.Equal(1, plane.RSquared, 9);
        Assert.Null(LinearModel.Fit(["a", "2a"], [x1, [.. x1.Select(v => 2 * v)]], x2));
        Assert.Null(LinearModel.Fit(["x"], [[1, 2]], [1, 2]));
        Assert.Null(LinearModel.Fit(["x"], [[2, 2, 2, 2]], [1, 2, 3, 4]));

        // Cross-validation: contiguous folds; leave-one-out on the hand example.
        (double inSample, double cv) = LinearModel.CrossValidate([[1, 2, 3, 4, 5]], [2, 4, 5, 4, 5], 5);
        Assert.Equal(Math.Sqrt(2.4 / 5), inSample, 9);
        Assert.True(cv > inSample);
    }

    [Fact]
    public void The_simple_regression_analysis_reports_the_fit_and_transforms()
    {
        var r = new SimpleRegression();   // the hand example
        Assert.Null(r.Problem);
        Assert.Equal(0.6, r.Slope, 9);
        Assert.Equal(2.2, r.Intercept, 9);
        Assert.Equal(0.6, r.RSquared, 9);
        Assert.Equal(Math.Sqrt(0.6), r.Correlation, 9);
        Assert.Equal(4.5, r.FStatistic, 9);
        Assert.Contains("y = 2.2 + 0.6·x", r.Equation, StringComparison.Ordinal);
        Assert.Equal(3, r.Figures(null).Count);
        Assert.True(double.IsNaN(r.FittedAt));

        r.PredictAt = 3;
        r.Recalculate();
        Assert.Equal(4.0, r.FittedAt, 9);
        Assert.True(r.NewLower < r.MeanLower && r.NewUpper > r.MeanUpper);

        // An exponential relation becomes a line under ln y, drawn back in original units.
        var e = new SimpleRegression { YTransformation = Transformation.Log, PredictAt = 2.5 };
        e.Data.Clear();
        foreach (double x in new[] { 1.0, 2, 3, 4, 5, 6 })
        {
            e.Data.Add(x, 3 * Math.Exp(0.4 * x));
        }

        e.Recalculate();
        Assert.Null(e.Problem);
        Assert.Equal(0.4, e.Slope, 9);
        Assert.Equal(Math.Log(3), e.Intercept, 9);
        Assert.Equal(1, e.RSquared, 9);
        Assert.Equal(3 * Math.Exp(1.0), e.FittedAt, 6);
        Assert.Equal(3 * Math.Exp(1.0), e.FittedValue(2.5), 6);
        Assert.StartsWith("ln(y) =", e.Equation, StringComparison.Ordinal);

        e.Data.Add(7, -1);   // a log cannot take it
        e.Recalculate();
        Assert.Equal(1, e.Skipped);
        Assert.Equal(6, e.Count);

        Assert.Equal(2, MethodCatalog.For(typeof(SimpleRegression)).Count);
        Assert.All(MethodCatalog.For(typeof(SimpleRegression)), m => Assert.Empty(MethodContract.Verify(m)));
    }

    /// <summary>The synthetic housing table of the sample project (the shape of Module 12's problem).</summary>
    internal static StatistleDocument HousingProject(out DataTable housing)
    {
        var document = new StatistleDocument();
        housing = document.AddTable("Housing");
        housing.Columns.AddColumn("size", [1610, 1120, 3170, 2690, 1190, 2160, 2900, 1840, 1820, 2610, 2070, 2390, 1780, 1850, 2430, 2030, 1150, 2330, 2250, 2030]);
        housing.Columns.AddColumn("age", [13, 31, 27, 27, 27, 4, 24, 34, 2, 30, 39, 33, 28, 28, 37, 9, 28, 2, 4, 25]);
        housing.Columns.AddColumn("bedrooms", [4, 2, 5, 4, 2, 2, 4, 4, 2, 4, 5, 5, 3, 4, 4, 3, 4, 3, 3, 5]);
        housing.Columns.AddColumn("distance", [5.5, 29.4, 26.3, 13.1, 25.3, 5.3, 24.1, 23.4, 7.5, 16.8, 19.6, 7.5, 3.3, 13.4, 5.5, 3.1, 4.8, 12.5, 14.5, 7.2]);
        housing.Columns.AddColumn("price", [234.0, 89.1, 391.8, 331.9, 88.9, 310.7, 345.5, 215.5, 280.7, 313.4, 261.4, 310.4, 230.6, 238.7, 329.2, 261.5, 199.4, 326.4, 298.7, 296.5]);
        return document;
    }

    [Fact]
    public void The_multiple_regression_analysis_fits_selects_and_cross_validates()
    {
        StatistleDocument project = HousingProject(out DataTable housing);
        var m = new MultipleRegression { Project = project, Table = housing.Id.ToString(CultureInfo.InvariantCulture), Response = "price" };
        m.Recalculate();

        Assert.Null(m.Problem);
        Assert.Equal(20, m.Count);
        Assert.Equal("size, age, bedrooms, distance", m.PredictorsUsed);
        Assert.InRange(m.RSquared, 0.9, 1.0);
        Assert.True(m.AdjustedRSquared < m.RSquared);
        Assert.Equal(4, m.DfRegression);
        Assert.Equal(15, m.DfError);
        Assert.True(m.MaxVif >= 1);
        Assert.Contains("size:", m.CoefficientTable, StringComparison.Ordinal);
        Assert.False(double.IsNaN(m.CvRmse));
        Assert.True(m.CvRmse > m.InSampleRmse);
        Assert.Equal(3, m.Figures(null).Count);
        LinearModel direct = LinearModel.Fit(["size", "age", "bedrooms", "distance"],
            [.. housing.Columns.Columns.Take(4).Select(c => (IReadOnlyList<double>)c.Values)], housing.Columns.Column("price")!.Values)!;
        Assert.Equal(direct.RSquared, m.RSquared, 12);

        m.Predictors = "size, age";
        m.Recalculate();
        Assert.Equal(2, m.DfRegression);
        Assert.Contains("price = ", m.Equation, StringComparison.Ordinal);

        m.Predictors = "size, colour";
        m.Recalculate();
        Assert.Contains("colour", m.Problem, StringComparison.Ordinal);

        m.Predictors = string.Empty;
        m.Selection = VariableSelection.Forward;
        m.Recalculate();
        Assert.Null(m.Problem);
        Assert.Contains("added size", m.SelectionNote, StringComparison.Ordinal);
        Assert.True(m.DfRegression >= 1 && m.DfRegression <= 4);

        m.Selection = VariableSelection.Backward;
        m.Recalculate();
        Assert.Null(m.Problem);
        Assert.Contains("size", m.PredictorsUsed, StringComparison.Ordinal);

        m.Folds = 0;
        m.Recalculate();
        Assert.True(double.IsNaN(m.CvRmse));

        Assert.Contains(m.ChoicesFor(nameof(MultipleRegression.Table)), c => c.Label == "Housing");
        Assert.Contains(m.ChoicesFor(nameof(MultipleRegression.Response)), c => c.Label == "distance");
        Assert.Same(housing, m.LinkedObject(nameof(MultipleRegression.Table)));
        Assert.Equal(3, MethodCatalog.For(typeof(MultipleRegression)).Count);
        Assert.All(MethodCatalog.For(typeof(MultipleRegression)), mm => Assert.Empty(MethodContract.Verify(mm)));

        var blank = new MultipleRegression();
        Assert.NotNull(blank.Problem);
        Assert.Empty(blank.Figures(null));
    }
}
