namespace Statistle.Core.Methods.Descriptive;

/// <summary>Three quartiles by one convention; the interquartile range and Tukey's fences follow.</summary>
public sealed record Quartiles(double Q1, double Median, double Q3)
{
    public double Iqr => Q3 - Q1;

    /// <summary>Q1 − 1.5·IQR: a value below it is an outlier by Tukey's rule.</summary>
    public double LowerFence => Q1 - 1.5 * Iqr;

    /// <summary>Q3 + 1.5·IQR: a value above it is an outlier by Tukey's rule.</summary>
    public double UpperFence => Q3 + 1.5 * Iqr;
}

/// <summary>One class of a frequency table: [Lower, Upper), the last class closed.</summary>
public sealed record FrequencyClass(
    double Lower,
    double Upper,
    int Frequency,
    double RelativeFrequency,
    int CumulativeFrequency,
    double CumulativeRelativeFrequency)
{
    public double Mark => (Lower + Upper) / 2;
}

/// <summary>
/// The descriptive statistics of EGN3443 Module 2 (Montgomery &amp; Runger Ch. 6; NIST e-Handbook
/// §1.3.5), each in the course's notation so the formula the student reads is the one that runs.
/// A statistic that needs more values than it has is NaN, never an exception.
/// </summary>
public static class SummaryStatistics
{
    public static double Mean(IReadOnlyList<double> x) => x.Count == 0 ? double.NaN : x.Sum() / x.Count;

    public static double Median(IReadOnlyList<double> x)
    {
        if (x.Count == 0)
        {
            return double.NaN;
        }

        double[] s = [.. x.Order()];
        int n = s.Length;

        return n % 2 == 1 ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2;
    }

    /// <summary>The most frequent value(s); empty when every value occurs once.</summary>
    public static IReadOnlyList<double> Modes(IReadOnlyList<double> x)
    {
        if (x.Count == 0)
        {
            return [];
        }

        var counts = x.GroupBy(v => v).Select(g => (Value: g.Key, Count: g.Count())).ToList();
        int most = counts.Max(c => c.Count);

        return most < 2 ? [] : [.. counts.Where(c => c.Count == most).Select(c => c.Value).Order()];
    }

    public static double Min(IReadOnlyList<double> x) => x.Count == 0 ? double.NaN : x.Min();

    public static double Max(IReadOnlyList<double> x) => x.Count == 0 ? double.NaN : x.Max();

    public static double Range(IReadOnlyList<double> x) => Max(x) - Min(x);

    /// <summary>s² = Σ(xᵢ − x̄)² / (n − 1).</summary>
    public static double SampleVariance(IReadOnlyList<double> x) =>
        x.Count < 2 ? double.NaN : SumOfSquaredDeviations(x) / (x.Count - 1);

    public static double SampleStandardDeviation(IReadOnlyList<double> x) => Math.Sqrt(SampleVariance(x));

    /// <summary>σ² = Σ(xᵢ − μ)² / n — when the values are the whole population.</summary>
    public static double PopulationVariance(IReadOnlyList<double> x) =>
        x.Count == 0 ? double.NaN : SumOfSquaredDeviations(x) / x.Count;

    public static double PopulationStandardDeviation(IReadOnlyList<double> x) => Math.Sqrt(PopulationVariance(x));

    /// <summary>s / x̄ as a fraction; NaN when the mean is zero.</summary>
    public static double CoefficientOfVariation(IReadOnlyList<double> x)
    {
        double mean = Mean(x);
        return mean == 0 ? double.NaN : SampleStandardDeviation(x) / mean;
    }

    /// <summary>s / √n — the standard deviation of the sample mean.</summary>
    public static double StandardError(IReadOnlyList<double> x) =>
        x.Count == 0 ? double.NaN : SampleStandardDeviation(x) / Math.Sqrt(x.Count);

    /// <summary>
    /// Tukey's hinges as R's <c>fivenum</c> computes them: the medians of the lower and upper
    /// halves, each half including the overall median when n is odd.
    /// </summary>
    public static Quartiles TukeyHinges(IReadOnlyList<double> x)
    {
        if (x.Count == 0)
        {
            return new Quartiles(double.NaN, double.NaN, double.NaN);
        }

        double[] s = [.. x.Order()];
        int n = s.Length;
        double n4 = Math.Floor((n + 3) / 2.0) / 2;
        double At(double position) => (s[(int)Math.Floor(position) - 1] + s[(int)Math.Ceiling(position) - 1]) / 2;

        return new Quartiles(At(n4), At((n + 1) / 2.0), At(n + 1 - n4));
    }

    /// <summary>
    /// Quartiles by linear interpolation between order statistics, position 1 + (n − 1)p — R's
    /// default <c>quantile</c> (type 7) and NumPy's <c>percentile</c>.
    /// </summary>
    public static Quartiles Type7Quartiles(IReadOnlyList<double> x) =>
        new(Percentile(x, 25), Percentile(x, 50), Percentile(x, 75));

    /// <summary>The p-th percentile (0–100) by the type-7 rule.</summary>
    public static double Percentile(IReadOnlyList<double> x, double p)
    {
        if (x.Count == 0 || p < 0 || p > 100)
        {
            return double.NaN;
        }

        double[] s = [.. x.Order()];
        double position = (s.Length - 1) * p / 100;
        int low = (int)Math.Floor(position);
        int high = Math.Min(low + 1, s.Length - 1);

        return s[low] + (s[high] - s[low]) * (position - low);
    }

    /// <summary>The values outside Tukey's fences, in order.</summary>
    public static IReadOnlyList<double> Outliers(IReadOnlyList<double> x, Quartiles quartiles) =>
        [.. x.Where(v => v < quartiles.LowerFence || v > quartiles.UpperFence).Order()];

    /// <summary>
    /// The limits of an individuals control chart, x̄ ∓ 3s with the sample standard deviation;
    /// NaN for fewer than two values.
    /// </summary>
    public static (double Lower, double Upper) ControlLimits(IReadOnlyList<double> x)
    {
        double mean = Mean(x), s = SampleStandardDeviation(x);
        return (mean - 3 * s, mean + 3 * s);
    }

    /// <summary>
    /// Sample skewness, adjusted for bias: g₁ = [n / ((n − 1)(n − 2))] Σ((xᵢ − x̄)/s)³ — what
    /// Excel's SKEW, <c>scipy.stats.skew(x, bias=False)</c> and <c>e1071::skewness(type = 2)</c>
    /// give. Positive means a long right tail.
    /// </summary>
    public static double Skewness(IReadOnlyList<double> x)
    {
        int n = x.Count;
        double s = SampleStandardDeviation(x);

        if (n < 3 || !(s > 0))
        {
            return double.NaN;
        }

        double mean = Mean(x);
        double sum = x.Sum(v => Math.Pow((v - mean) / s, 3));

        return (double)n / ((n - 1) * (n - 2)) * sum;
    }

    /// <summary>
    /// Excess kurtosis, adjusted for bias — Excel's KURT, <c>scipy.stats.kurtosis(x, bias=False)</c>,
    /// <c>e1071::kurtosis(type = 2)</c>: zero for a normal distribution, positive for heavier tails.
    /// </summary>
    public static double ExcessKurtosis(IReadOnlyList<double> x)
    {
        int n = x.Count;
        double s = SampleStandardDeviation(x);

        if (n < 4 || !(s > 0))
        {
            return double.NaN;
        }

        double mean = Mean(x);
        double sum = x.Sum(v => Math.Pow((v - mean) / s, 4));

        return (double)n * (n + 1) / ((n - 1.0) * (n - 2) * (n - 3)) * sum
             - 3.0 * (n - 1) * (n - 1) / ((n - 2.0) * (n - 3));
    }

    /// <summary>Sturges' rule for the number of classes: ⌈log₂ n + 1⌉.</summary>
    public static int SturgesClasses(int n) => n < 1 ? 1 : (int)Math.Ceiling(Math.Log2(n) + 1);

    /// <summary>The square-root rule: ⌈√n⌉.</summary>
    public static int SquareRootClasses(int n) => n < 1 ? 1 : (int)Math.Ceiling(Math.Sqrt(n));

    /// <summary>
    /// A frequency table of equal-width classes from the minimum to the maximum: [lower, upper)
    /// with the last class closed, so every value lands in exactly one class.
    /// </summary>
    public static IReadOnlyList<FrequencyClass> FrequencyTable(IReadOnlyList<double> x, int classes)
    {
        if (x.Count == 0 || classes < 1)
        {
            return [];
        }

        double min = Min(x), max = Max(x);
        double width = max > min ? (max - min) / classes : 1;
        int[] counts = new int[classes];

        foreach (double v in x)
        {
            int i = Math.Clamp((int)Math.Floor((v - min) / width), 0, classes - 1);
            counts[i]++;
        }

        List<FrequencyClass> table = [];
        int cumulative = 0;

        for (int i = 0; i < classes; i++)
        {
            cumulative += counts[i];
            table.Add(new FrequencyClass(
                min + i * width,
                min + (i + 1) * width,
                counts[i],
                (double)counts[i] / x.Count,
                cumulative,
                (double)cumulative / x.Count));
        }

        return table;
    }

    private static double SumOfSquaredDeviations(IReadOnlyList<double> x)
    {
        double mean = Mean(x);
        return x.Sum(v => (v - mean) * (v - mean));
    }
}
