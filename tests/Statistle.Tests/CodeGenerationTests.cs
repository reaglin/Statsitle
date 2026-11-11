using System.Diagnostics;
using System.Globalization;
using Statistle.Core.Analyses;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Methods.Regression;
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
    public void Every_descriptive_result_is_printed_under_its_own_label_with_the_conventions_stated()
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

        Assert.Equal(2, ReportBuilder.For(new SimpleRegression()).Appendices.Count);
    }

    /// <summary>Every analysis that writes code, in the configurations worth checking.</summary>
    public static IEnumerable<object[]> CodeSources()
    {
        var descriptive = new DescriptiveStatistics();
        descriptive.Sample.Clear();
        foreach (double v in new[] { 470, 485, 478, 492, 460, 473, 488, 484, 482, 475, 530.0 })
        {
            descriptive.Sample.Add(descriptive.Sample.Count, v);
        }

        descriptive.Recalculate();
        yield return ["Descriptive statistics", descriptive];

        yield return ["Probability rules, independent", new ProbabilityRules()];
        yield return ["Probability rules, Bayes", new ProbabilityRules { Relationship = EventRelationship.KnownLikelihoods, ProbabilityA = 0.01, LikelihoodGivenA = 0.95, LikelihoodGivenNotA = 0.05 }];
        yield return ["Probability rules, conditional", new ProbabilityRules { Relationship = EventRelationship.KnownConditional, ConditionalAGivenB = 0.2 }];

        yield return ["Binomial", new DiscreteDistribution { K2 = 4 }];
        yield return ["Poisson", new DiscreteDistribution { Family = DiscreteFamily.Poisson, Rate = 3.5, K = 2, K2 = 5 }];
        yield return ["Geometric", new DiscreteDistribution { Family = DiscreteFamily.Geometric, SuccessProbability = 0.2, K = 3 }];
        yield return ["Negative binomial", new DiscreteDistribution { Family = DiscreteFamily.NegativeBinomial, SuccessesWanted = 3, SuccessProbability = 0.5, K = 5, K2 = 7 }];
        yield return ["Hypergeometric", new DiscreteDistribution { Family = DiscreteFamily.Hypergeometric, PopulationSize = 50, PopulationSuccesses = 5, Draws = 10, K = 1, K2 = 3 }];
        yield return ["Typed table", new DiscreteDistribution { Family = DiscreteFamily.Table, K = 1, K2 = 2 }];

        yield return ["Normal", new ContinuousDistribution()];
        yield return ["Uniform", new ContinuousDistribution { Family = ContinuousFamily.Uniform, Lower = 2, Upper = 5, X = 3, X2 = 4.5, Probability = 0.9 }];
        yield return ["Exponential", new ContinuousDistribution { Family = ContinuousFamily.Exponential, Rate = 2, X = 1, X2 = 2 }];
        yield return ["Lognormal", new ContinuousDistribution { Family = ContinuousFamily.LogNormal, LogMu = 3, LogSigma = 0.4, X = 20, X2 = 30 }];
        yield return ["Student's t", new ContinuousDistribution { Family = ContinuousFamily.StudentT, DegreesOfFreedom = 10, X = 1.5, X2 = double.NaN, Probability = 0.975 }];
        yield return ["Chi-squared", new ContinuousDistribution { Family = ContinuousFamily.ChiSquared, DegreesOfFreedom = 10, X = 12, X2 = 20 }];
        yield return ["F", new ContinuousDistribution { Family = ContinuousFamily.F, NumeratorDf = 5, DenominatorDf = 10, X = 2, X2 = 4 }];

        yield return ["Sampling distribution of the mean", new SamplingDistribution { Value2 = 4350 }];
        yield return ["Sampling distribution of a proportion", new SamplingDistribution { Statistic = SamplingStatistic.SampleProportion, PopulationProportion = 0.15, SampleSize = 100, Value = 0.2, Value2 = 0.25 }];

        // Simulation: the random results are labelled in both scripts but not compared (numpy draws other numbers); the theory is.
        yield return ["Simulation, two dice", new Simulation()];
        yield return ["Simulation, coins at least", new Simulation { Experiment = SimulationExperiment.Coins, Coins = 5, EventIs = EventComparison.AtLeast, Heads = 3 }];
        yield return ["Simulation, three-card 21", new Simulation { Experiment = SimulationExperiment.Cards }];
        yield return ["Simulation, hearts at most", new Simulation { Experiment = SimulationExperiment.Cards, CardOutcome = CardOutcome.Hearts, Cards = 5, EventIs = EventComparison.AtMost, CardTarget = 1 }];
        yield return ["Simulation, aces", new Simulation { Experiment = SimulationExperiment.Cards, CardOutcome = CardOutcome.Aces, Cards = 2, CardTarget = 1 }];
        yield return ["Simulation, darts", new Simulation { Experiment = SimulationExperiment.Darts }];
        yield return ["Simulation, means of a normal", new Simulation { Experiment = SimulationExperiment.SampleMeans, Distribution = SampledDistribution.Normal }];
        yield return ["Simulation, means of a uniform", new Simulation { Experiment = SimulationExperiment.SampleMeans, Distribution = SampledDistribution.Uniform, Lower = 2, Upper = 8, SampleSize = 5 }];
        yield return ["Simulation, means of an exponential", new Simulation { Experiment = SimulationExperiment.SampleMeans, SampleSize = 2 }];
        yield return ["Simulation, interval coverage", new Simulation { Experiment = SimulationExperiment.IntervalCoverage, SampleSize = 8, Confidence = 0.9, Trials = 300 }];
        yield return ["Simulation, familywise error", new Simulation { Experiment = SimulationExperiment.FamilywiseError, SampleSize = 6, Tests = 5, Trials = 300 }];

        StatistleDocument project = Phase3AnalysisTests.CatalystProject(out DataTable table);
        string a = DataSources.Reference(table.Id, "A"), b = DataSources.Reference(table.Id, "B"), pass = DataSources.Reference(table.Id, "Pass");

        yield return ["CI mean t", new ConfidenceInterval()];
        yield return ["CI mean z", new ConfidenceInterval { Target = IntervalFor.MeanSigmaKnown, Sigma = 3.5 }];
        yield return ["CI proportion", new ConfidenceInterval { Target = IntervalFor.Proportion, Successes = 10, SampleSize = 85 }];
        yield return ["CI variance", new ConfidenceInterval { Target = IntervalFor.Variance, SampleSd = 2, SampleSize = 20 }];
        yield return ["CI pooled, columns", new ConfidenceInterval { Project = project, Target = IntervalFor.TwoMeansPooled, Source = DataSource.Columns, Column = a, Column2 = b }];
        yield return ["CI Welch", new ConfidenceInterval { Target = IntervalFor.TwoMeansWelch, SampleMean = 92.255, SampleSd = 2.39, SampleSize = 8 }];
        yield return ["CI paired, columns", new ConfidenceInterval { Project = project, Target = IntervalFor.PairedMeans, Source = DataSource.Columns, Column = a, Column2 = b }];
        yield return ["CI two proportions", new ConfidenceInterval { Target = IntervalFor.TwoProportions, Successes = 27, SampleSize = 300, Successes2 = 19, Sample2Size = 300 }];
        yield return ["CI proportion, column", new ConfidenceInterval { Project = project, Target = IntervalFor.Proportion, Source = DataSource.Columns, Column = pass }];

        yield return ["Sample size, mean", new SampleSize()];
        yield return ["Sample size, proportion", new SampleSize { Target = SizeFor.Proportion, Margin = 0.05, ProportionGuess = 0.12 }];

        yield return ["One-sample z", new OneSampleTest()];
        yield return ["One-sample z, greater", new OneSampleTest { AlternativeHypothesis = Alternative.Greater }];
        yield return ["One-sample t", new OneSampleTest { TestKind = OneSampleTestKind.MeanSigmaUnknown, SampleMean = 0.83725, SampleSd = 0.02456, SampleSize = 15, NullValue = 0.82, AlternativeHypothesis = Alternative.Greater, AlternativeValue = 0.83 }];
        yield return ["One-sample t, column", new OneSampleTest { Project = project, TestKind = OneSampleTestKind.MeanSigmaUnknown, Source = DataSource.Columns, Column = a, NullValue = 90, AlternativeValue = double.NaN }];
        yield return ["One-sample proportion", new OneSampleTest { TestKind = OneSampleTestKind.Proportion, Successes = 5, SampleSize = 200, NullValue = 0.05, AlternativeHypothesis = Alternative.Less }];
        yield return ["One-sample proportion, column", new OneSampleTest { Project = project, TestKind = OneSampleTestKind.Proportion, Source = DataSource.Columns, Column = pass, NullValue = 0.6 }];
        yield return ["One-sample variance", new OneSampleTest { TestKind = OneSampleTestKind.Variance, SampleSd = Math.Sqrt(0.0153), SampleSize = 20, NullValue = 0.1, AlternativeHypothesis = Alternative.Greater }];

        yield return ["Two-sample pooled", new TwoSampleTest()];
        yield return ["Two-sample pooled, columns", new TwoSampleTest { Project = project, Source = DataSource.Columns, Column = a, Column2 = b }];
        yield return ["Two-sample Welch, columns, less", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.WelchT, Source = DataSource.Columns, Column = a, Column2 = b, AlternativeHypothesis = Alternative.Less }];
        yield return ["Two-sample paired, columns", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.PairedT, Source = DataSource.Columns, Column = a, Column2 = b }];
        yield return ["Two-sample z", new TwoSampleTest { TestKind = TwoSampleTestKind.TwoSampleZ }];
        yield return ["Two proportions", new TwoSampleTest { TestKind = TwoSampleTestKind.TwoProportions, Successes = 27, SampleSize = 300, Successes2 = 19, Sample2Size = 300 }];
        yield return ["Two variances F", new TwoSampleTest { TestKind = TwoSampleTestKind.VarianceRatio }];
        yield return ["Two variances F, columns", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.VarianceRatio, Source = DataSource.Columns, Column = a, Column2 = b, AlternativeHypothesis = Alternative.Greater }];
        yield return ["Mann–Whitney, columns", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.MannWhitney, Source = DataSource.Columns, Column = a, Column2 = b }];
        yield return ["Mann–Whitney, columns, greater", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.MannWhitney, Source = DataSource.Columns, Column = a, Column2 = b, AlternativeHypothesis = Alternative.Greater }];
        yield return ["Wilcoxon, columns", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.WilcoxonSignedRank, Source = DataSource.Columns, Column = a, Column2 = b }];
        yield return ["Wilcoxon, columns, less", new TwoSampleTest { Project = project, TestKind = TwoSampleTestKind.WilcoxonSignedRank, Source = DataSource.Columns, Column = a, Column2 = b, AlternativeHypothesis = Alternative.Less }];

        // Modules 11–13: the regressions, against statsmodels.
        yield return ["Simple regression, hand example", new SimpleRegression()];
        yield return ["Simple regression, prediction", new SimpleRegression { PredictAt = 3.5, Confidence = 0.9 }];
        yield return ["Simple regression, ln y", Curved(new SimpleRegression { YTransformation = Transformation.Log, PredictAt = 2.5 })];
        yield return ["Simple regression, ln x", Curved(new SimpleRegression { XTransformation = Transformation.Log, PredictAt = 4 })];
        yield return ["Simple regression, 1/y on √x", Curved(new SimpleRegression { YTransformation = Transformation.Inverse, XTransformation = Transformation.SquareRoot, PredictAt = 3 })];
        yield return ["Simple regression, columns", new SimpleRegression { Project = project, XColumn = a, YColumn = b, PredictAt = 5 }];

        StatistleDocument housingProject = RegressionTests.HousingProject(out DataTable housing);
        string housingId = housing.Id.ToString(CultureInfo.InvariantCulture);
        yield return ["Multiple regression, all", new MultipleRegression { Project = housingProject, Table = housingId, Response = "price" }];
        yield return ["Multiple regression, forward", new MultipleRegression { Project = housingProject, Table = housingId, Response = "price", Selection = VariableSelection.Forward, Folds = 10, Confidence = 0.99 }];
        yield return ["Multiple regression, backward, no CV", new MultipleRegression { Project = housingProject, Table = housingId, Response = "price", Selection = VariableSelection.Backward, Folds = 0 }];
        yield return ["Multiple regression, one predictor, LOO", new MultipleRegression { Project = housingProject, Table = housingId, Response = "price", Predictors = "size", Folds = 20 }];
        yield return ["Multiple regression, two predictors", new MultipleRegression { Project = housingProject, Table = housingId, Response = "distance", Predictors = "size, bedrooms", Folds = 4 }];

        // Module 14: the ANOVAs, against scipy (f_oneway, tukey_hsd, studentized_range, ncf, kruskal) and statsmodels (anova_lm).
        StatistleDocument alloyProject = AnovaTests.AlloyProject(out DataTable alloys);
        string alloyId = alloys.Id.ToString(CultureInfo.InvariantCulture);
        yield return ["One-way ANOVA, columns", new OneWayAnova { Project = alloyProject, Table = alloyId }];
        yield return ["One-way ANOVA, two columns, α = 0.01", new OneWayAnova { Project = alloyProject, Table = alloyId, Groups = "AA2024, AA-X", Alpha = 0.01 }];
        DataTable stacked = alloyProject.AddTable("Stacked");
        stacked.Columns.AddColumn("alloy", [.. Enumerable.Range(0, 30).Select(i => (double)(i / 10 + 1))]);
        stacked.Columns.AddColumn("strength", [.. alloys.Columns.Columns.SelectMany(c => c.Values)]);
        yield return ["One-way ANOVA, factor and response", new OneWayAnova { Project = alloyProject, Table = stacked.Id.ToString(CultureInfo.InvariantCulture), Layout = GroupLayout.FactorAndResponse, FactorColumn = "alloy", ResponseColumn = "strength" }];
        DataTable ragged = alloyProject.AddTable("Ragged");
        ragged.Columns.AddColumn("P", [12.1, 13.4, 11.9, 12.8, 13.0]);
        ragged.Columns.AddColumn("Q", [14.2, 13.9, 15.1, 14.8, 14.0, 13.7, 14.4]);
        ragged.Columns.AddColumn("R", [12.9, 13.1, 13.6]);
        ragged.Columns.AddColumn("S", [15.0, 15.4, 14.9, 15.8]);
        yield return ["One-way ANOVA, four unequal groups", new OneWayAnova { Project = alloyProject, Table = ragged.Id.ToString(CultureInfo.InvariantCulture) }];

        StatistleDocument batteryProject = AnovaTests.BatteryProject(out DataTable battery);
        yield return ["Two-way ANOVA, 3 × 2 with replication", new TwoWayAnova { Project = batteryProject, Table = battery.Id.ToString(CultureInfo.InvariantCulture), FactorA = "temperature", FactorB = "material", Response = "life" }];
        DataTable blocks = batteryProject.AddTable("Blocks");
        blocks.Columns.AddColumn("treatment", [1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4]);
        blocks.Columns.AddColumn("block", [1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3]);
        blocks.Columns.AddColumn("yield", [73, 68, 74, 73, 67, 75, 75, 68, 78, 73, 71, 75]);
        yield return ["Two-way ANOVA, one per cell (blocks)", new TwoWayAnova { Project = batteryProject, Table = blocks.Id.ToString(CultureInfo.InvariantCulture), FactorA = "treatment", FactorB = "block", Response = "yield", Alpha = 0.1 }];
    }

    /// <summary>Eight pairs that grow roughly exponentially, for the transformations.</summary>
    private static SimpleRegression Curved(SimpleRegression regression)
    {
        regression.Data.Clear();
        foreach ((double x, double y) in new[] { (1.0, 4.6), (2, 6.5), (3, 10.2), (4, 14.4), (5, 22.9), (6, 32.0), (7, 50.5), (8, 72.1) })
        {
            regression.Data.Add(x, y);
        }

        return regression;
    }

    [Theory]
    [MemberData(nameof(CodeSources))]
    public void Every_visible_result_is_printed_by_both_scripts_under_its_own_label(string name, Analysis analysis)
    {
        analysis.Recalculate();
        Assert.True((analysis as IReportsProblem)?.Problem is null, $"{name}: {(analysis as IReportsProblem)?.Problem}");
        string python = ((ICodeSource)analysis).Code(CodeLanguage.Python);
        string r = ((ICodeSource)analysis).Code(CodeLanguage.R);

        foreach (StatistleProperty property in MethodScope.VisibleProperties(analysis).Where(p => p.IsResult))
        {
            if (property.Read(analysis) is double d && double.IsNaN(d))
            {
                continue;   // a blank result (no k₂ given) need not be printed
            }

            Assert.True(python.Contains(property.Label + ":", StringComparison.Ordinal), $"{name}: Python does not print '{property.Label}'");
            Assert.True(r.Contains(property.Label + ":", StringComparison.Ordinal), $"{name}: R does not print '{property.Label}'");
        }
    }

    [Theory]
    [MemberData(nameof(CodeSources))]
    public void The_generated_R_uses_no_Python_names(string name, Analysis analysis)
    {
        analysis.Recalculate();
        string r = ((ICodeSource)analysis).Code(CodeLanguage.R);
        foreach (var python in new[] { "stats.", "np.", "scipy" })
        {
            foreach (var line in r.Split('\n').Where(l => !l.TrimStart().StartsWith('#')))
            {
                Assert.False(line.Contains(python, StringComparison.Ordinal), $"{name}: R line uses '{python}': {line.Trim()}");
            }
        }
    }

    [Theory]
    [MemberData(nameof(CodeSources))]
    public void The_generated_Python_prints_the_apps_numbers_when_Python_is_available(string name, Analysis analysis)
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

        analysis.Recalculate();
        string script = Path.Combine(Path.GetTempPath(), $"statistle-check-{Guid.NewGuid():N}.py");
        File.WriteAllText(script, ((ICodeSource)analysis).Code(CodeLanguage.Python));
        string output = Run("python", script);
        File.Delete(script);

        var printed = output.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Contains(": ", StringComparison.Ordinal) && !l.StartsWith('['))
            .ToDictionary(l => l[..l.IndexOf(": ", StringComparison.Ordinal)], l => l[(l.IndexOf(": ", StringComparison.Ordinal) + 2)..]);

        foreach (StatistleProperty property in MethodScope.VisibleProperties(analysis).Where(p => p.IsResult))
        {
            object? value = property.Read(analysis);

            if (property.Property.IsDefined(typeof(SimulatedResultAttribute), false))
            {
                // Drawn from the program's random numbers: Python draws its own, so only the label is required.
                Assert.True(printed.ContainsKey(property.Label), $"{name}: Python did not print '{property.Label}'. Output:\n{output}");
                continue;
            }

            if (value is double expected)
            {
                if (double.IsNaN(expected))
                {
                    continue;
                }

                Assert.True(printed.ContainsKey(property.Label), $"{name}: Python did not print '{property.Label}'. Output:\n{output}");
                double actual = double.Parse(printed[property.Label], CultureInfo.InvariantCulture);
                double tolerance = Math.Pow(10, -property.Decimals) * 0.51;
                Assert.True(Math.Abs(actual - expected) <= tolerance, $"{name} — {property.Label}: app {expected}, Python {actual}");
            }
            else if (value is int count)
            {
                Assert.True(printed.ContainsKey(property.Label), $"{name}: Python did not print '{property.Label}'. Output:\n{output}");
                Assert.Equal(count.ToString(CultureInfo.InvariantCulture), printed[property.Label]);
            }
        }

        if (analysis is DescriptiveStatistics)
        {
            // numpy prints the integer-valued outlier as [530] and a float one as [530.5]: compare the number.
            Assert.Equal(530.0, double.Parse(printed["Outliers"].Trim('[', ']'), CultureInfo.InvariantCulture), 9);
        }
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
