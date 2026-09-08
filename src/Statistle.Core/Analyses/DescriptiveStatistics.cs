using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Statistics;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

/// <summary>
/// The summary numbers for one sample: how many, where its centre is, how spread out it is, and
/// how far from symmetric it is. Every other analysis in the course starts here.
///
/// The sample is a column of one of the project's data tables, or values typed into the analysis
/// itself.
/// </summary>
[StatistleHelp(
    "Descriptive statistics for one sample: the count, the centre (mean and median), the spread " +
    "(standard deviation, variance, range and interquartile range), and the shape (skewness). " +
    "Choose a column of one of the project's data tables, or type the values into Sample, one " +
    "per line. The standard deviation and the variance are the sample forms, dividing by n - 1, " +
    "which is what you want when the data are a sample from a larger population rather than the " +
    "whole population.")]
public sealed class DescriptiveStatistics : Analysis, IUsesProject, IHasChoices, ILinksTo, IReportsProblem
{
    private string _column = string.Empty;

    private int _count;
    private double _mean;
    private double _median;
    private double _standardDeviation;
    private double _variance;
    private double _standardError;
    private double _coefficientOfVariation;
    private double _minimum;
    private double _maximum;
    private double _range;
    private double _firstQuartile;
    private double _thirdQuartile;
    private double _interquartileRange;
    private double _skewness;

    public DescriptiveStatistics()
    {
        Name = "Descriptive statistics";

        // A short sample, so the analysis shows something the moment it is opened.
        foreach (double value in new[] { 3.2, 4.8, 2.9, 6.1, 5.4, 3.7, 8.2, 4.1, 5.9, 4.4 })
        {
            Sample.Add(Sample.Count, value);
        }

        Recalculate();
    }

    public override string Title => "Descriptive statistics";

    public override string Summary => "Count, centre, spread and shape of one sample";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 10;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleProperty(Label = "Column", Role = VariableRole.Input,
        Help = "Which column of which data table to summarise — add one with + Data table in " +
               "the rail and paste your measurements there — or 'Entered below' to type the " +
               "values into this analysis. With a column chosen its values show below, " +
               "read-only; Edit takes you to the table.")]
    public string Column
    {
        get => _column;
        set => SetProperty(ref _column, value ?? string.Empty);
    }

    /// <summary>The sample. One value per line; the index column is the observation number.</summary>
    [StatistleProperty(Label = "Sample", Role = VariableRole.Input, ValuesOnly = true,
        Help = "The measurements, one per line, in any order — order never affects any of these " +
               "results. Paste a column straight from a spreadsheet.")]
    public DataSeries Sample { get; } = new();

    [StatistleProperty(Label = "Observations (n)", Decimals = 0, Unit = ValueKind.Count,
        Role = VariableRole.Calculated,
        Help = "How many values were read. Everything below is computed from these n numbers.")]
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    [StatistleProperty(Label = "Mean", Decimals = 4, Role = VariableRole.Output,
        Help = "The arithmetic mean, x̄ = Σx / n: the balance point of the sample. It moves with " +
               "an outlier, which is why the median is worth reading beside it.")]
    public double Mean
    {
        get => _mean;
        set => SetProperty(ref _mean, value);
    }

    [StatistleProperty(Label = "Median", Decimals = 4, Role = VariableRole.Output,
        Help = "The middle value once the sample is sorted (the average of the middle two when n " +
               "is even). Unlike the mean it barely moves when one value is extreme, so a mean " +
               "far from the median is a sign of a skewed sample or an outlier.")]
    public double Median
    {
        get => _median;
        set => SetProperty(ref _median, value);
    }

    [StatistleProperty(Label = "Standard deviation (s)", Decimals = 4, Role = VariableRole.Output,
        Help = "The sample standard deviation, s = √(Σ(x - x̄)² / (n - 1)): the typical distance " +
               "of a measurement from the mean, in the units of the data. The n - 1 makes it an " +
               "unbiased estimate of the population value.")]
    public double StandardDeviation
    {
        get => _standardDeviation;
        set => SetProperty(ref _standardDeviation, value);
    }

    [StatistleProperty(Label = "Variance (s²)", Decimals = 4, Role = VariableRole.Output,
        Help = "The square of the standard deviation. It is what adds when independent " +
               "quantities are summed, which is why so much of the theory is written in variance " +
               "rather than in standard deviation.")]
    public double Variance
    {
        get => _variance;
        set => SetProperty(ref _variance, value);
    }

    [StatistleProperty(Label = "Standard error of the mean", Decimals = 4, Role = VariableRole.Output,
        Help = "s / √n: how far the sample mean is likely to sit from the true population mean. " +
               "It is the spread of the mean, not of the data, and it shrinks as √n — four times " +
               "the measurements halve it.")]
    public double StandardError
    {
        get => _standardError;
        set => SetProperty(ref _standardError, value);
    }

    [StatistleProperty(Label = "Coefficient of variation", Decimals = 2, Unit = ValueKind.Percent,
        Role = VariableRole.Output,
        Help = "100 · s / x̄: the spread as a percentage of the mean, so two samples measured in " +
               "different units can be compared for relative variability. Meaningless when the " +
               "mean is near zero.")]
    public double CoefficientOfVariation
    {
        get => _coefficientOfVariation;
        set => SetProperty(ref _coefficientOfVariation, value);
    }

    [StatistleProperty(Label = "Minimum", Decimals = 4, Role = VariableRole.Output,
        Help = "The smallest value in the sample.")]
    public double Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }

    [StatistleProperty(Label = "Maximum", Decimals = 4, Role = VariableRole.Output,
        Help = "The largest value in the sample.")]
    public double Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }

    [StatistleProperty(Label = "Range", Decimals = 4, Role = VariableRole.Output,
        Help = "Maximum minus minimum. Simple, and entirely decided by the two most extreme " +
               "measurements, so it is a poor measure of spread for anything but a quick check.")]
    public double Range
    {
        get => _range;
        set => SetProperty(ref _range, value);
    }

    [StatistleProperty(Label = "First quartile (Q1)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The value a quarter of the way up the sorted sample: a quarter of the " +
               "measurements are below it. Interpolated between neighbouring values.")]
    public double FirstQuartile
    {
        get => _firstQuartile;
        set => SetProperty(ref _firstQuartile, value);
    }

    [StatistleProperty(Label = "Third quartile (Q3)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The value three quarters of the way up the sorted sample.")]
    public double ThirdQuartile
    {
        get => _thirdQuartile;
        set => SetProperty(ref _thirdQuartile, value);
    }

    [StatistleProperty(Label = "Interquartile range", Decimals = 4, Role = VariableRole.Output,
        Help = "Q3 - Q1: the width of the middle half of the sample. Unaffected by the extremes, " +
               "so it is the measure of spread to quote when the sample has outliers.")]
    public double InterquartileRange
    {
        get => _interquartileRange;
        set => SetProperty(ref _interquartileRange, value);
    }

    [StatistleProperty(Label = "Skewness", Decimals = 4, Role = VariableRole.Output,
        Help = "The sample coefficient of skewness. Near zero the sample is symmetric; positive " +
               "means a long tail of large values (mean above median), negative the reverse. " +
               "Needs at least three values.")]
    public double Skewness
    {
        get => _skewness;
        set => SetProperty(ref _skewness, value);
    }

    public override void Recalculate()
    {
        Problem = DataSources.Pull(this, Column, Sample);
        new DescriptiveStatisticsMethod().Apply(this);
    }

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName == nameof(Column) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Column) or nameof(Sample) ? DataSources.Table(this, Column) : null;
}

/// <summary>While a project column is chosen, the sample comes from it and is locked.</summary>
public sealed class DescriptiveStatisticsSourceMethod : CalculationMethod<DescriptiveStatistics>
{
    public override string Name => "Data table column";

    public override string Description => "The sample is copied from the chosen data table column.";

    public override string Citation => "Statistle project data.";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(DescriptiveStatistics.Sample)];

    public override IReadOnlyList<string> Generates => [nameof(DescriptiveStatistics.Sample)];

    public override bool IsApplicableTo(DescriptiveStatistics subject) =>
        subject is not null && DataSources.Split(subject.Column).TableId > 0;

    public override void Apply(DescriptiveStatistics subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        DataSources.Pull(subject, subject.Column, subject.Sample);
    }
}

/// <summary>The summary numbers themselves — all of them from the one sample.</summary>
public sealed class DescriptiveStatisticsMethod : CalculationMethod<DescriptiveStatistics>
{
    public override string Name => "Sample statistics";

    public override string Description =>
        "Count, mean, median, sample standard deviation and variance, standard error of the " +
        "mean, coefficient of variation, minimum, maximum, range, quartiles and skewness.";

    public override string Citation =>
        "Montgomery, D. C. and Runger, G. C., Applied Statistics and Probability for Engineers, " +
        "7th ed., Wiley, 2018, chapter 6 (descriptive statistics); quartiles by linear " +
        "interpolation between order statistics.";

    public override IReadOnlyList<string> Inputs => [nameof(DescriptiveStatistics.Sample)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(DescriptiveStatistics.Count),
        nameof(DescriptiveStatistics.Mean),
        nameof(DescriptiveStatistics.Median),
        nameof(DescriptiveStatistics.StandardDeviation),
        nameof(DescriptiveStatistics.Variance),
        nameof(DescriptiveStatistics.StandardError),
        nameof(DescriptiveStatistics.CoefficientOfVariation),
        nameof(DescriptiveStatistics.Minimum),
        nameof(DescriptiveStatistics.Maximum),
        nameof(DescriptiveStatistics.Range),
        nameof(DescriptiveStatistics.FirstQuartile),
        nameof(DescriptiveStatistics.ThirdQuartile),
        nameof(DescriptiveStatistics.InterquartileRange),
        nameof(DescriptiveStatistics.Skewness),
    ];

    public override void Apply(DescriptiveStatistics s)
    {
        ArgumentNullException.ThrowIfNull(s);

        double[] values = s.Sample.YValues;

        s.Count = values.Length;
        s.Mean = 0;
        s.Median = 0;
        s.StandardDeviation = 0;
        s.Variance = 0;
        s.StandardError = 0;
        s.CoefficientOfVariation = 0;
        s.Minimum = 0;
        s.Maximum = 0;
        s.Range = 0;
        s.FirstQuartile = 0;
        s.ThirdQuartile = 0;
        s.InterquartileRange = 0;
        s.Skewness = 0;

        if (values.Length == 0)
        {
            return;
        }

        s.Mean = SeriesStatistics.Mean(values);
        s.Median = SeriesStatistics.Median(values);
        s.Minimum = values.Min();
        s.Maximum = values.Max();
        s.Range = s.Maximum - s.Minimum;
        s.FirstQuartile = SeriesStatistics.Percentile(values, 0.25);
        s.ThirdQuartile = SeriesStatistics.Percentile(values, 0.75);
        s.InterquartileRange = s.ThirdQuartile - s.FirstQuartile;

        if (values.Length < 2)
        {
            return;
        }

        s.StandardDeviation = SeriesStatistics.StandardDeviation(values);
        s.Variance = s.StandardDeviation * s.StandardDeviation;
        s.StandardError = s.StandardDeviation / Math.Sqrt(values.Length);

        if (s.Mean != 0)
        {
            s.CoefficientOfVariation = 100.0 * s.StandardDeviation / Math.Abs(s.Mean);
        }

        if (values.Length >= 3)
        {
            s.Skewness = SeriesStatistics.Skew(values);
        }
    }
}
