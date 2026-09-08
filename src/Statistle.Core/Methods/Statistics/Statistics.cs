namespace Statistle.Core.Methods.Statistics;

/// <summary>
/// The summary numbers of one sample — centre, spread and shape.
///
/// These have exact answers, so they are checked against closed forms and against worked cases
/// from the textbook rather than against any predecessor implementation.
/// </summary>
public static class SeriesStatistics
{
    public static double Mean(IReadOnlyList<double> values)
    {
        Require(values, 1);

        return values.Average();
    }

    /// <summary>Sample standard deviation, dividing by n − 1.</summary>
    public static double StandardDeviation(IReadOnlyList<double> values)
    {
        Require(values, 2);

        double mean = values.Average();
        double sumSquares = values.Sum(v => (v - mean) * (v - mean));

        return Math.Sqrt(sumSquares / (values.Count - 1));
    }

    public static double Variance(IReadOnlyList<double> values)
    {
        double s = StandardDeviation(values);

        return s * s;
    }

    /// <summary>Sample skew coefficient, with the standard unbiasing factor.</summary>
    public static double Skew(IReadOnlyList<double> values)
    {
        Require(values, 3);

        int n = values.Count;
        double mean = values.Average();
        double s = StandardDeviation(values);

        if (s == 0)
        {
            return 0;
        }

        double sumCubes = values.Sum(v => Math.Pow(v - mean, 3));

        return n * sumCubes / ((n - 1.0) * (n - 2.0) * Math.Pow(s, 3));
    }

    /// <summary>The middle value of the sorted sample; the mean of the middle two when n is even.</summary>
    public static double Median(IReadOnlyList<double> values) => Percentile(values, 0.5);

    /// <summary>
    /// The value below which the given fraction of the sample lies, interpolated linearly
    /// between the order statistics: rank = p·(n − 1), the convention a spreadsheet's
    /// PERCENTILE and QUARTILE use, so a hand check against Excel agrees.
    /// </summary>
    public static double Percentile(IReadOnlyList<double> values, double fraction)
    {
        Require(values, 1);

        if (fraction is < 0 or > 1 || double.IsNaN(fraction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(fraction), fraction, "A percentile fraction must be between zero and one.");
        }

        double[] sorted = [.. values.OrderBy(v => v)];

        if (sorted.Length == 1)
        {
            return sorted[0];
        }

        double rank = fraction * (sorted.Length - 1);
        int lower = (int)Math.Floor(rank);
        int upper = Math.Min(lower + 1, sorted.Length - 1);

        return sorted[lower] + ((rank - lower) * (sorted[upper] - sorted[lower]));
    }

    /// <summary>Values sorted largest first.</summary>
    public static double[] Ranked(IReadOnlyList<double> values)
    {
        Require(values, 1);

        return [.. values.OrderByDescending(v => v)];
    }

    private static void Require(IReadOnlyList<double> values, int minimum)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count < minimum)
        {
            throw new ArgumentException(
                $"At least {minimum} value(s) are needed; got {values.Count}.", nameof(values));
        }
    }
}
