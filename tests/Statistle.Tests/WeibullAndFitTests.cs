using Statistle.Core.Analyses;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Distributions;

namespace Statistle.Tests;

/// <summary>
/// Plan 10.9: the Weibull in Continuous distribution, and the parameters fitted to a data column.
/// Reference values from scipy 1.18.1 and R 4.4.3 (pweibull, uniroot), computed 2026-11-11.
/// </summary>
public sealed class WeibullAndFitTests
{
    /// <summary>Twelve failure times (hours), made for the test.</summary>
    internal static readonly double[] FailureTimes = [41.2, 65.0, 78.3, 90.1, 102.5, 110.7, 125.4, 133.0, 148.9, 160.2, 181.6, 210.3];

    internal static StatistleDocument LifeProject(out DataTable table)
    {
        var document = new StatistleDocument();
        table = document.AddTable();
        table.Name = "Bearings";
        table.Columns.AddColumn("Hours", FailureTimes);
        return document;
    }

    internal static ContinuousDistribution Fitted(ContinuousFamily family)
    {
        StatistleDocument project = LifeProject(out DataTable table);
        var c = new ContinuousDistribution
        {
            Project = project,
            Family = family,
            Source = ParameterSource.FittedToData,
            Column = DataSources.Reference(table.Id, "Hours"),
            X = 100,
            X2 = 150,
        };
        c.Recalculate();
        return c;
    }

    [Fact]
    public void The_Weibull_gives_Rs_and_scipys_probabilities_moments_and_hazard()
    {
        var c = new ContinuousDistribution { Family = ContinuousFamily.Weibull, Shape = 1.5, Scale = 1000, X = 500, X2 = 1500, Probability = 0.9 };
        c.Recalculate();
        Assert.Null(c.Problem);
        Assert.Equal(0.000744783376439, c.Density, 12);
        Assert.Equal(0.297811498673, c.AtMost, 11);
        Assert.Equal(0.702188501327, c.MoreThan, 11);
        Assert.Equal(0.542912592837, c.Between, 11);
        Assert.Equal(1743.7215136, c.Quantile, 6);
        Assert.Equal(902.745292951, c.Mean, 8);
        Assert.Equal(375690.284814, c.Variance, 5);
        Assert.Equal(Math.Sqrt(375690.284814), c.StandardDeviation, 6);
        Assert.Equal(783.219768775, c.Median, 7);
        Assert.Equal(0.00106066017178, c.HazardRate, 13);
        Assert.Equal(2, c.Figures(null).Count);
        Assert.Contains("Weibull(β = 1.5, η = 1000)", c.ModelDescription, StringComparison.Ordinal);

        // β = 1 is the exponential with λ = 1/η, whose hazard is λ at every age.
        var w = new WeibullModel(1, 50);
        var e = new ExponentialModel(0.02);
        Assert.Equal(e.Cdf(30), w.Cdf(30), 12);
        Assert.Equal(0.02, w.Hazard(30), 12);
        Assert.Equal(1 - Math.Exp(-1), new WeibullModel(3.2, 70).Cdf(70), 12);   // 63.2 % by η whatever β

        c.Shape = 0;
        c.Recalculate();
        Assert.Contains("greater than zero", c.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void The_Weibull_shows_only_its_parameters_and_the_hazard_rate()
    {
        var c = new ContinuousDistribution { Family = ContinuousFamily.Weibull };
        c.Recalculate();
        string[] visible = [.. MethodScope.VisibleProperties(c).Select(p => p.XmlName)];
        Assert.Contains(nameof(ContinuousDistribution.Shape), visible);
        Assert.Contains(nameof(ContinuousDistribution.Scale), visible);
        Assert.Contains(nameof(ContinuousDistribution.HazardRate), visible);
        Assert.DoesNotContain(nameof(ContinuousDistribution.Mu), visible);
        Assert.DoesNotContain(nameof(ContinuousDistribution.Column), visible);
        Assert.DoesNotContain(nameof(ContinuousDistribution.QqCorrelation), visible);

        c.Family = ContinuousFamily.Normal;
        c.Recalculate();
        Assert.DoesNotContain(nameof(ContinuousDistribution.HazardRate), MethodScope.VisibleProperties(c).Select(p => p.XmlName).ToArray());
        Assert.True(double.IsNaN(c.HazardRate));
    }

    [Fact]
    public void The_Weibull_fit_solves_the_likelihood_equation_as_R_does()
    {
        (double shape, double scale) = ContinuousFit.Weibull(FailureTimes);
        Assert.Equal(2.7914457116, shape, 9);     // R uniroot; scipy brentq the same
        Assert.Equal(135.727722292, scale, 7);
        Assert.Equal(2.79144290156, shape, 4);    // scipy weibull_min.fit(floc=0), a general optimizer: about six figures

        // A wide spread (β below 1) and a narrow one (β large) converge too.
        (double small, _) = ContinuousFit.Weibull([0.01, 0.2, 1, 3, 9, 40, 250, 1300]);
        Assert.InRange(small, 0.2, 0.6);
        (double large, double eta) = ContinuousFit.Weibull([99.0, 99.5, 100, 100.2, 100.6, 101]);
        Assert.InRange(large, 50, 500);
        Assert.InRange(eta, 100, 101);
    }

    [Fact]
    public void Each_family_fitted_to_a_column_matches_scipy()
    {
        ContinuousDistribution weibull = Fitted(ContinuousFamily.Weibull);
        Assert.Null(weibull.Problem);
        Assert.Equal(12, weibull.FitCount);
        Assert.Equal(2.7914457116, weibull.FitShape, 9);
        Assert.Equal(135.727722292, weibull.FitScale, 7);
        Assert.Equal(weibull.FitShape, weibull.Shape, 12);
        Assert.Equal(weibull.FitScale, weibull.Scale, 12);
        Assert.Equal(0.999419410158, weibull.QqCorrelation, 9);   // R cor(qweibull(p, ...), sort(x))
        Assert.Equal(0.0677312638739, weibull.KsStatistic, 9);    // R ks.test(...)$statistic
        Assert.True(MethodScope.IsLocked(weibull, nameof(ContinuousDistribution.Shape)));
        Assert.True(MethodScope.IsLocked(weibull, nameof(ContinuousDistribution.Scale)));
        Assert.Contains("fitted to 12 values", weibull.ModelDescription, StringComparison.Ordinal);

        ContinuousDistribution normal = Fitted(ContinuousFamily.Normal);
        Assert.Equal(120.6, normal.FitMu, 9);
        Assert.Equal(47.3055845893, normal.FitSigma, 9);     // scipy norm.fit: divides by n
        Assert.Equal(0.997580786908, normal.QqCorrelation, 9);
        Assert.Equal(0.0828842379126, normal.KsStatistic, 9);

        ContinuousDistribution lognormal = Fitted(ContinuousFamily.LogNormal);
        Assert.Equal(4.70357310098, lognormal.FitLogMu, 9);
        Assert.Equal(0.443378156832, lognormal.FitLogSigma, 9);
        Assert.Equal(0.983049011814, lognormal.QqCorrelation, 9);
        Assert.Equal(0.113536048459, lognormal.KsStatistic, 9);

        ContinuousDistribution exponential = Fitted(ContinuousFamily.Exponential);
        Assert.Equal(0.00829187396352, exponential.FitRate, 12);
        Assert.Equal(0.951433822403, exponential.QqCorrelation, 9);
        Assert.Equal(0.333318928845, exponential.KsStatistic, 9);
        Assert.Equal(exponential.FitRate, exponential.HazardRate, 12);

        ContinuousDistribution uniform = Fitted(ContinuousFamily.Uniform);
        Assert.Equal(41.2, uniform.FitLower, 12);
        Assert.Equal(210.3, uniform.FitUpper, 12);
        Assert.Equal(0.990755152017, uniform.QqCorrelation, 9);
        Assert.Equal(0.129607727183, uniform.KsStatistic, 9);

        // The Weibull fits these failure times best, by both measures.
        Assert.True(weibull.QqCorrelation > normal.QqCorrelation && weibull.KsStatistic < normal.KsStatistic);
    }

    [Fact]
    public void A_fit_adds_the_histogram_and_the_QQ_plot_after_the_first_two_figures()
    {
        ContinuousDistribution c = Fitted(ContinuousFamily.Weibull);
        var figures = c.Figures(null);
        Assert.Equal(4, figures.Count);
        Assert.Contains("Weibull", figures[0].Title, StringComparison.Ordinal);       // 1: density, shaded
        Assert.Contains("cumulative", figures[1].Title, StringComparison.Ordinal);    // 2: cumulative
        Assert.Contains("fitted curve", figures[2].Title, StringComparison.Ordinal);  // 3: histogram + fitted density
        Assert.Contains("Q–Q plot", figures[3].Title, StringComparison.Ordinal);      // 4: Q–Q plot

        var markers = Assert.Single(figures[3].Series.OfType<Statistle.Core.Plotting.MarkerSeries>());
        Assert.Equal(FailureTimes.Length, markers.X.Count);
        Assert.Equal(FailureTimes.Order(), markers.Y);
    }

    [Fact]
    public void A_fit_says_what_is_wrong_in_plain_words()
    {
        var unfitted = new ContinuousDistribution { Family = ContinuousFamily.Weibull, Source = ParameterSource.FittedToData };
        unfitted.Recalculate();
        Assert.Contains("Choose the column", unfitted.Problem, StringComparison.Ordinal);

        ContinuousDistribution t = Fitted(ContinuousFamily.StudentT);
        Assert.Contains("offered for the uniform, normal, exponential, lognormal and Weibull", t.Problem, StringComparison.Ordinal);

        StatistleDocument project = LifeProject(out DataTable table);
        table.Columns.AddColumn("Change", [-2.0, 1.5, 3.1]);
        var negative = new ContinuousDistribution
        {
            Project = project,
            Family = ContinuousFamily.Weibull,
            Source = ParameterSource.FittedToData,
            Column = DataSources.Reference(table.Id, "Change"),
        };
        negative.Recalculate();
        Assert.Contains("above zero", negative.Problem, StringComparison.Ordinal);
        Assert.DoesNotContain("Parameter", negative.Problem, StringComparison.Ordinal);
        Assert.True(double.IsNaN(negative.FitShape));
    }

    [Fact]
    public void A_fitted_analysis_survives_save_and_reopen()
    {
        StatistleDocument document = LifeProject(out DataTable table);
        ContinuousDistribution analysis = document.Tool<ContinuousDistribution>();
        analysis.Family = ContinuousFamily.Weibull;
        analysis.Source = ParameterSource.FittedToData;
        analysis.Column = DataSources.Reference(table.Id, "Hours");
        analysis.X = 100;
        analysis.X2 = 150;
        analysis.Recalculate();

        string file = Path.Combine(Path.GetTempPath(), $"statistle-weibull-{Guid.NewGuid():N}.statistle");

        try
        {
            document.Save(file);
            StatistleDocument reopened = StatistleDocument.Open(file);
            ContinuousDistribution restored = reopened.Tool<ContinuousDistribution>();
            Assert.Equal(ContinuousFamily.Weibull, restored.Family);
            Assert.Equal(ParameterSource.FittedToData, restored.Source);
            Assert.Equal(analysis.Column, restored.Column);
            Assert.Null(restored.Problem);
            Assert.Equal(analysis.FitShape, restored.FitShape, 12);
            Assert.Equal(analysis.AtMost, restored.AtMost, 12);
            Assert.False(reopened.Diagnostics.HasWarnings, reopened.Diagnostics.ToString());
        }
        finally
        {
            File.Delete(file);
        }

        var typed = new ContinuousDistribution { Family = ContinuousFamily.Weibull, Shape = 1.5, Scale = 1000 };
        var copy = new ContinuousDistribution();
        Statistle.Core.Persistence.StatistleXml.Read(copy, Statistle.Core.Persistence.StatistleXml.Write(typed, "analysis"), new Statistle.Core.Persistence.LoadDiagnostics());
        Assert.Equal(1.5, copy.Shape);
        Assert.Equal(1000, copy.Scale);
        Assert.Equal(ContinuousFamily.Weibull, copy.Family);
    }

    [Fact]
    public void The_fit_code_fits_the_data_and_prints_every_fitted_result()
    {
        ContinuousDistribution c = Fitted(ContinuousFamily.Weibull);
        string python = c.Code(CodeLanguage.Python), r = c.Code(CodeLanguage.R);
        Assert.Contains("brentq", python, StringComparison.Ordinal);
        Assert.Contains("stats.weibull_min(c=shape, scale=scale)", python, StringComparison.Ordinal);
        Assert.Contains("uniroot(shape_equation", r, StringComparison.Ordinal);
        Assert.Contains("ks.test(data", r, StringComparison.Ordinal);
        Assert.DoesNotContain("shape <- 2.79", r, StringComparison.Ordinal);   // fitted in the script, not typed in
    }
}
