using Statistle.Core.Code;
using Statistle.Core.Methods.Descriptive;

namespace Statistle.Tests;

/// <summary>Module 2: every descriptive statistic against a published value or an invariant.</summary>
public class SummaryStatisticsTests
{
    // Excel's documented SKEW and KURT example data.
    private static readonly double[] ExcelExample = [3, 4, 5, 2, 3, 4, 5, 6, 4, 7];

    // A small set whose statistics are exact by hand: mean 5, population SD 2.
    private static readonly double[] Small = [2, 4, 4, 4, 5, 5, 7, 9];

    [Fact]
    public void Centre_and_spread_match_hand_arithmetic()
    {
        Assert.Equal(5, SummaryStatistics.Mean(Small), 12);
        Assert.Equal(4.5, SummaryStatistics.Median(Small), 12);
        Assert.Equal([4], SummaryStatistics.Modes(Small));
        Assert.Equal(7, SummaryStatistics.Range(Small), 12);
        Assert.Equal(2, SummaryStatistics.PopulationStandardDeviation(Small), 12);
        Assert.Equal(32.0 / 7, SummaryStatistics.SampleVariance(Small), 12);
        Assert.Equal(Math.Sqrt(32.0 / 7), SummaryStatistics.SampleStandardDeviation(Small), 12);
        Assert.Equal(Math.Sqrt(32.0 / 7) / 5, SummaryStatistics.CoefficientOfVariation(Small), 12);
        Assert.Equal(Math.Sqrt(32.0 / 7) / Math.Sqrt(8), SummaryStatistics.StandardError(Small), 12);
        Assert.Empty(SummaryStatistics.Modes([1, 2, 3]));
        Assert.Equal([2, 5], SummaryStatistics.Modes([5, 2, 2, 5, 9]));
    }

    [Fact]
    public void Quartiles_follow_R_fivenum_and_type_7()
    {
        Quartiles hinges = SummaryStatistics.TukeyHinges(Small);     // fivenum(c(2,4,4,4,5,5,7,9)) = 2 4 4.5 6 9
        Assert.Equal(4, hinges.Q1, 12);
        Assert.Equal(4.5, hinges.Median, 12);
        Assert.Equal(6, hinges.Q3, 12);

        Quartiles type7 = SummaryStatistics.Type7Quartiles(Small);   // quantile(x) = 4 4.5 5.5
        Assert.Equal(4, type7.Q1, 12);
        Assert.Equal(5.5, type7.Q3, 12);
        Assert.Equal(1.5, type7.Iqr, 12);
        Assert.Equal(4 - 2.25, type7.LowerFence, 12);
        Assert.Equal(9, SummaryStatistics.Percentile(Small, 100), 12);
        Assert.Equal(2, SummaryStatistics.Percentile(Small, 0), 12);

        // Odd n: fivenum includes the median in both halves; quantile interpolates.
        double[] odd = [1, 2, 3, 4, 5];
        Assert.Equal(2, SummaryStatistics.TukeyHinges(odd).Q1, 12);
        Assert.Equal(2, SummaryStatistics.Type7Quartiles(odd).Q1, 12);
        Assert.Equal([40], SummaryStatistics.Outliers([1, 2, 3, 4, 5, 40], SummaryStatistics.Type7Quartiles([1, 2, 3, 4, 5, 40])));
    }

    [Fact]
    public void Skewness_and_kurtosis_match_Excels_documented_example()
    {
        Assert.Equal(0.359543, SummaryStatistics.Skewness(ExcelExample), 5);
        Assert.Equal(-0.151799, SummaryStatistics.ExcessKurtosis(ExcelExample), 5);

        Assert.Equal(0, SummaryStatistics.Skewness([1, 2, 3, 4, 5]), 12);
        Assert.True(SummaryStatistics.Skewness([1, 1, 1, 1, 20]) > 0, "a long right tail is positive skew");
        Assert.True(double.IsNaN(SummaryStatistics.Skewness([1, 2])));
        Assert.True(double.IsNaN(SummaryStatistics.ExcessKurtosis([1, 2, 3])));
        Assert.True(double.IsNaN(SummaryStatistics.Skewness([3, 3, 3])));
    }

    [Fact]
    public void Frequency_classes_partition_the_data()
    {
        Assert.Equal(4, SummaryStatistics.SturgesClasses(8));       // ⌈3 + 1⌉
        Assert.Equal(6, SummaryStatistics.SturgesClasses(20));      // ⌈4.32 + 1⌉
        Assert.Equal(3, SummaryStatistics.SquareRootClasses(8));

        IReadOnlyList<FrequencyClass> table = SummaryStatistics.FrequencyTable(Small, 4);
        Assert.Equal(4, table.Count);
        Assert.Equal(8, table.Sum(c => c.Frequency));
        Assert.Equal(1, table[^1].CumulativeRelativeFrequency, 12);
        Assert.Equal(8, table[^1].CumulativeFrequency);
        Assert.Equal(2, table[0].Lower, 12);
        Assert.Equal(9, table[^1].Upper, 12);
        Assert.Equal(1, table[^1].Frequency);                       // the maximum lands in the closed last class
        Assert.Equal(2 + 1.75 / 2, table[0].Mark, 12);   // [2, 3.75)

        Assert.Single(SummaryStatistics.FrequencyTable([5, 5, 5], 3), c => c.Frequency > 0);
    }

    [Fact]
    public void Empty_and_tiny_samples_give_blanks_not_exceptions()
    {
        double[] none = [];
        Assert.True(double.IsNaN(SummaryStatistics.Mean(none)));
        Assert.True(double.IsNaN(SummaryStatistics.Median(none)));
        Assert.True(double.IsNaN(SummaryStatistics.SampleVariance([7])));
        Assert.Equal(0, SummaryStatistics.PopulationVariance([7]), 12);
        Assert.True(double.IsNaN(SummaryStatistics.CoefficientOfVariation([-1, 1])));
        Assert.Empty(SummaryStatistics.FrequencyTable(none, 5));
        Assert.True(double.IsNaN(SummaryStatistics.TukeyHinges(none).Q1));
    }

    [Fact]
    public void Code_text_makes_safe_identifiers_and_readable_vectors()
    {
        Assert.Equal("aa_x", CodeText.Identifier("AA-X"));
        Assert.Equal("tensile_strength", CodeText.Identifier("Tensile strength "));
        Assert.Equal("v_2024", CodeText.Identifier("2024"));
        Assert.Equal("x", CodeText.Identifier("***"));

        Assert.Equal("x = np.array([1, 2.5, 3])", CodeText.Vector("x", [1, 2.5, 3], CodeLanguage.Python));
        Assert.Equal("x <- c(1, 2.5, 3)", CodeText.Vector("x", [1, 2.5, 3], CodeLanguage.R));
        Assert.Contains("NA", CodeText.Vector("x", [1, double.NaN], CodeLanguage.R), StringComparison.Ordinal);
        Assert.Contains("float('nan')", CodeText.Vector("x", [1, double.NaN], CodeLanguage.Python), StringComparison.Ordinal);

        string longVector = CodeText.Vector("values", [.. Enumerable.Range(1, 60).Select(i => i * 1.25)], CodeLanguage.Python);
        Assert.All(longVector.Split('\n'), line => Assert.True(line.Length <= 90, line));
        Assert.EndsWith("])", longVector, StringComparison.Ordinal);

        Assert.Equal("print(f\"Mean: {np.mean(x):.4f}\")", CodeText.Print("Mean", "np.mean(x)", 4, CodeLanguage.Python));
        Assert.Equal("cat(\"Mean:\", round(mean(x), 4), \"\\n\")", CodeText.Print("Mean", "mean(x)", 4, CodeLanguage.R));
    }
}
