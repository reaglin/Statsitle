using System.Globalization;
using System.Text;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Descriptive;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

/// <summary>
/// EGN3443 Module 2: the summary numbers for one sample — how many, where its centre is, how
/// spread out it is, how far from symmetric — with a histogram and a box plot, and the Python
/// and R that print the same numbers. Every other analysis in the course starts here.
/// </summary>
[StatistleHelp(
    "Descriptive statistics for one sample (Module 2). Choose a column of a data table, or " +
    "type the values into Sample. The results give the count, the centre (mean, median, mode), " +
    "the spread (range, standard deviation and variance in both the sample and the population " +
    "forms, standard error, coefficient of variation), the position measures (quartiles two " +
    "ways, interquartile range, Tukey's fences and the outliers beyond them) and the shape " +
    "(skewness and kurtosis). The Plots tab draws the histogram with a normal curve of the same " +
    "mean and standard deviation, and the box plot. The Code tab writes the Python and R that " +
    "print every one of these numbers, in this order, with the same labels. Use the sample " +
    "forms (n − 1) when the data are a sample from a larger population, which is nearly always; " +
    "the population forms only when the values are the whole population.")]
public sealed class DescriptiveStatistics : Analysis, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, IHasFigures, ICodeSource
{
    private string _column = string.Empty;
    private double _classes = double.NaN;

    private int _count;
    private double _mean = double.NaN;
    private double _median = double.NaN;
    private string _mode = string.Empty;
    private double _modeValue = double.NaN;
    private double _minimum = double.NaN;
    private double _maximum = double.NaN;
    private double _range = double.NaN;
    private double _standardDeviation = double.NaN;
    private double _variance = double.NaN;
    private double _populationStandardDeviation = double.NaN;
    private double _populationVariance = double.NaN;
    private double _standardError = double.NaN;
    private double _coefficientOfVariation = double.NaN;
    private double _firstQuartile = double.NaN;
    private double _thirdQuartile = double.NaN;
    private double _interquartileRange = double.NaN;
    private double _lowerHinge = double.NaN;
    private double _upperHinge = double.NaN;
    private double _lowerFence = double.NaN;
    private double _upperFence = double.NaN;
    private string _outliers = string.Empty;
    private int _outlierCount;
    private double _skewness = double.NaN;
    private double _kurtosis = double.NaN;
    private double _lowerControlLimit = double.NaN;
    private double _upperControlLimit = double.NaN;
    private int _pointsOutsideLimits;
    private int _classesUsed;

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

    public override string Summary => "Count, centre, spread, position and shape of one sample (Module 2)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 10;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

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
               "results. Paste a column straight from a spreadsheet, or Import… a file.")]
    public DataSeries Sample { get; } = new();

    [StatistleProperty(Label = "Histogram classes", Decimals = 0, Role = VariableRole.Design, Min = 1, Max = 100,
        Help = "How many equal-width classes (bins) the histogram and frequency table use. Leave " +
               "blank for Sturges' rule, k = ⌈log₂ n + 1⌉ — 4 classes for 8 values, 6 for 20, 8 " +
               "for 100. Too few classes hide the shape; too many show noise. The square-root " +
               "rule, ⌈√n⌉, is the other common choice.")]
    public double Classes
    {
        get => _classes;
        set => SetProperty(ref _classes, value);
    }

    // ── Results: count and centre ─────────────────────────────────────────────

    [StatistleProperty(Label = "Observations (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many values were read. Everything below is computed from these n numbers.")]
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    [StatistleProperty(Label = "Mean", Decimals = 4, Role = VariableRole.Output,
        Help = "The arithmetic mean, x̄ = Σxᵢ / n: the balance point of the sample and the " +
               "estimate of the population mean μ. It moves with an outlier, which is why the " +
               "median is worth reading beside it. Python np.mean(x); R mean(x).")]
    public double Mean
    {
        get => _mean;
        set => SetProperty(ref _mean, value);
    }

    [StatistleProperty(Label = "Median", Decimals = 4, Role = VariableRole.Output,
        Help = "The middle value once the sample is sorted (the average of the middle two when n " +
               "is even). It barely moves when one value is extreme, so a mean far from the " +
               "median is a sign of skew or an outlier. Python np.median(x); R median(x).")]
    public double Median
    {
        get => _median;
        set => SetProperty(ref _median, value);
    }

    [StatistleProperty(Label = "Mode", Role = VariableRole.Output,
        Help = "The most frequent value, or values when several tie; 'none' when every value " +
               "occurs once, which is usual for measured data. The mode matters most for " +
               "counts and categories. Python scipy.stats.mode; R table(x).")]
    public string Mode
    {
        get => _mode;
        set => SetProperty(ref _mode, value);
    }

    [StatistleProperty(Label = "Mode, when there is one", Decimals = 4, Role = VariableRole.Output,
        Help = "The mode as a number, when exactly one value occurs more often than every other. " +
               "Blank when every value occurs once (no mode) or when two or more values tie for " +
               "most frequent (read them in Mode above). Python np.unique(x, return_counts=True); " +
               "R table(x).")]
    public double ModeValue
    {
        get => _modeValue;
        set => SetProperty(ref _modeValue, value);
    }

    // ── Spread ────────────────────────────────────────────────────────────────

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
               "measurements, so a poor measure of spread for anything but a quick check.")]
    public double Range
    {
        get => _range;
        set => SetProperty(ref _range, value);
    }

    [StatistleProperty(Label = "Standard deviation (s)", Decimals = 4, Role = VariableRole.Output,
        Help = "The sample standard deviation, s = √[Σ(xᵢ − x̄)² / (n − 1)]: the typical distance " +
               "of a measurement from the mean, in the units of the data. Dividing by n − 1 " +
               "makes s² an unbiased estimate of the population variance σ². Python " +
               "np.std(x, ddof=1); R sd(x). Needs two values.")]
    public double StandardDeviation
    {
        get => _standardDeviation;
        set => SetProperty(ref _standardDeviation, value);
    }

    [StatistleProperty(Label = "Variance (s²)", Decimals = 4, Role = VariableRole.Output,
        Help = "The square of the sample standard deviation, in squared units. Variances add " +
               "when independent quantities are summed, which is why the theory is written in " +
               "variance. Python np.var(x, ddof=1); R var(x).")]
    public double Variance
    {
        get => _variance;
        set => SetProperty(ref _variance, value);
    }

    [StatistleProperty(Label = "Population standard deviation (σ)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "√[Σ(xᵢ − x̄)² / n] — the standard deviation when these values are the whole " +
               "population, not a sample of one. Always a little smaller than s. Python " +
               "np.std(x); R sqrt(mean((x − mean(x))²)).")]
    public double PopulationStandardDeviation
    {
        get => _populationStandardDeviation;
        set => SetProperty(ref _populationStandardDeviation, value);
    }

    [StatistleProperty(Label = "Population variance (σ²)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Σ(xᵢ − x̄)² / n, the population form of the variance. Python np.var(x).")]
    public double PopulationVariance
    {
        get => _populationVariance;
        set => SetProperty(ref _populationVariance, value);
    }

    [StatistleProperty(Label = "Standard error of the mean", Decimals = 4, Role = VariableRole.Output,
        Help = "s / √n: the standard deviation of the sample mean itself — how far x̄ is likely " +
               "to sit from μ. It is the spread of the mean, not of the data, and shrinks as √n: " +
               "four times the measurements halve it. It is the yardstick of every confidence " +
               "interval and test on a mean. Python scipy.stats.sem(x); R sd(x)/sqrt(n).")]
    public double StandardError
    {
        get => _standardError;
        set => SetProperty(ref _standardError, value);
    }

    [StatistleProperty(Label = "Coefficient of variation", Decimals = 2, Unit = ValueKind.Percent, Role = VariableRole.Output,
        Help = "100 · s / x̄: the spread as a percentage of the mean, so samples in different " +
               "units can be compared for relative variability. Blank when the mean is zero; " +
               "meaningless when it is near zero or the scale has no true zero (°C).")]
    public double CoefficientOfVariation
    {
        get => _coefficientOfVariation;
        set => SetProperty(ref _coefficientOfVariation, value);
    }

    // ── Position ──────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "First quartile (Q1)", Decimals = 4, Role = VariableRole.Output,
        Help = "The value a quarter of the way up the sorted sample, by linear interpolation " +
               "between neighbouring values at position 1 + (n − 1)/4 — the rule NumPy's " +
               "percentile and R's quantile use by default (type 7). Textbooks differ; the " +
               "hinges below are Tukey's version.")]
    public double FirstQuartile
    {
        get => _firstQuartile;
        set => SetProperty(ref _firstQuartile, value);
    }

    [StatistleProperty(Label = "Third quartile (Q3)", Decimals = 4, Role = VariableRole.Output,
        Help = "The value three quarters of the way up the sorted sample, interpolated (type 7).")]
    public double ThirdQuartile
    {
        get => _thirdQuartile;
        set => SetProperty(ref _thirdQuartile, value);
    }

    [StatistleProperty(Label = "Interquartile range", Decimals = 4, Role = VariableRole.Output,
        Help = "Q3 − Q1: the width of the middle half of the sample. Unaffected by the extremes, " +
               "so it is the spread to quote when the sample has outliers, and the box of the " +
               "box plot.")]
    public double InterquartileRange
    {
        get => _interquartileRange;
        set => SetProperty(ref _interquartileRange, value);
    }

    [StatistleProperty(Label = "Lower hinge (Tukey)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Tukey's lower hinge: the median of the lower half of the sorted sample, the half " +
               "including the overall median when n is odd — what R's fivenum(x) gives and " +
               "what a hand-drawn box plot uses. It equals Q1 for many samples and differs by a " +
               "little for others; both conventions are correct, so say which you used.")]
    public double LowerHinge
    {
        get => _lowerHinge;
        set => SetProperty(ref _lowerHinge, value);
    }

    [StatistleProperty(Label = "Upper hinge (Tukey)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Tukey's upper hinge: the median of the upper half (R fivenum).")]
    public double UpperHinge
    {
        get => _upperHinge;
        set => SetProperty(ref _upperHinge, value);
    }

    [StatistleProperty(Label = "Lower fence", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Q1 − 1.5·IQR. A value below it is flagged as an outlier by Tukey's rule — a " +
               "value worth checking, not necessarily a value to discard.")]
    public double LowerFence
    {
        get => _lowerFence;
        set => SetProperty(ref _lowerFence, value);
    }

    [StatistleProperty(Label = "Upper fence", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Q3 + 1.5·IQR. A value above it is flagged as an outlier.")]
    public double UpperFence
    {
        get => _upperFence;
        set => SetProperty(ref _upperFence, value);
    }

    [StatistleProperty(Label = "Outliers", Role = VariableRole.Output,
        Help = "The values beyond Tukey's fences, or 'none'. Look at each: a recording error is " +
               "corrected or dropped, a real extreme is kept and reported. The box plot draws them " +
               "as points beyond the whiskers.")]
    public string Outliers
    {
        get => _outliers;
        set => SetProperty(ref _outliers, value);
    }

    [StatistleProperty(Label = "Number of outliers", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "How many values lie beyond the fences — below Q1 − 1.5·IQR or above Q3 + 1.5·IQR, " +
               "with the type-7 quartiles above — 0 when there are none. These are the points the " +
               "box plot draws beyond its whiskers. Python outliers.size; R length(outliers).")]
    public int OutlierCount
    {
        get => _outlierCount;
        set => SetProperty(ref _outlierCount, value);
    }

    // ── Shape ─────────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Skewness", Decimals = 4, Role = VariableRole.Output,
        Help = "The adjusted sample skewness g₁ = [n/((n−1)(n−2))] Σ((xᵢ − x̄)/s)³ — Excel's SKEW, " +
               "scipy.stats.skew(x, bias=False). Near zero the sample is symmetric; positive " +
               "means a long tail of large values (mean above median), negative the reverse. " +
               "Beyond about ±1 the skew is strong and a transformation or a non-parametric " +
               "method may be wiser. Needs three values.")]
    public double Skewness
    {
        get => _skewness;
        set => SetProperty(ref _skewness, value);
    }

    [StatistleProperty(Label = "Excess kurtosis", Decimals = 4, Role = VariableRole.Output,
        Help = "The adjusted sample excess kurtosis — Excel's KURT, scipy.stats.kurtosis(x, " +
               "bias=False): zero for a normal distribution, positive for heavier tails and a " +
               "sharper peak (more outliers than a normal would give), negative for lighter " +
               "tails (a flat or bounded distribution). Needs four values.")]
    public double Kurtosis
    {
        get => _kurtosis;
        set => SetProperty(ref _kurtosis, value);
    }

    // ── Control limits ────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Lower control limit (LCL)", Decimals = 4, Role = VariableRole.Output,
        Help = "x̄ − 3s: the lower line of an individuals control chart. A process that is in " +
               "control almost never (about 0.13 % of the time, for normal data) gives a value " +
               "below it, so one that does is a signal to look for a cause. Here s is the sample " +
               "standard deviation; quality-control texts often estimate σ from the average moving " +
               "range instead, which gives slightly different limits. Needs two values.")]
    public double LowerControlLimit
    {
        get => _lowerControlLimit;
        set => SetProperty(ref _lowerControlLimit, value);
    }

    [StatistleProperty(Label = "Upper control limit (UCL)", Decimals = 4, Role = VariableRole.Output,
        Help = "x̄ + 3s: the upper line of an individuals control chart. Python np.mean(x) + 3 * " +
               "np.std(x, ddof=1); R mean(x) + 3 * sd(x).")]
    public double UpperControlLimit
    {
        get => _upperControlLimit;
        set => SetProperty(ref _upperControlLimit, value);
    }

    [StatistleProperty(Label = "Points outside the control limits", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "How many values lie below the LCL or above the UCL — the points the control chart " +
               "marks in red. Each is a signal that something out of the ordinary happened at that " +
               "point in the sequence.")]
    public int PointsOutsideLimits
    {
        get => _pointsOutsideLimits;
        set => SetProperty(ref _pointsOutsideLimits, value);
    }

    [StatistleProperty(Label = "Classes used", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "The number of histogram classes actually drawn: your entry, or Sturges' rule.")]
    public int ClassesUsed
    {
        get => _classesUsed;
        set => SetProperty(ref _classesUsed, value);
    }

    /// <summary>The frequency table behind the histogram, for the figure and the code.</summary>
    [StatistleIgnore]
    public IReadOnlyList<FrequencyClass> FrequencyTable { get; set; } = [];

    public override void Recalculate()
    {
        Problem = DataSources.Pull(this, Column, Sample);
        new DescriptiveStatisticsMethod().Apply(this);
    }

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName == nameof(Column) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Column) or nameof(Sample) ? DataSources.Table(this, Column) : null;

    /// <summary>The histogram with a normal curve of the same mean and s, and the box plot.</summary>
    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        double[] values = Sample.YValues;

        if (values.Length == 0)
        {
            return [];
        }

        return
        [
            DescriptiveFigures.Histogram($"{Name} — histogram", FrequencyTable, Mean, StandardDeviation),
            DescriptiveFigures.BoxPlot($"{Name} — box plot", values, FirstQuartile, Median, ThirdQuartile, LowerFence, UpperFence),

            // Appended after the histogram and the box plot, so course pages that cite figures 1 and 2 keep their numbers.
            .. double.IsNaN(LowerControlLimit)
                ? Array.Empty<PlotSpec>()
                : [DescriptiveFigures.ControlChart($"{Name} — control chart", values, Mean, LowerControlLimit, UpperControlLimit)],
        ];
    }

    /// <summary>What the sample is called in the code and in the comment above it.</summary>
    [StatistleIgnore]
    public (string Variable, string Source) SampleName
    {
        get
        {
            (int tableId, string columnName) = DataSources.Split(Column);
            DataTable? table = DataSources.Table(this, Column);

            return tableId > 0 && table is not null
                ? (CodeText.Identifier(columnName), $"{table.Name} › {columnName}")
                : ("x", "values entered in the analysis");
        }
    }

    /// <summary>
    /// The script that prints every result above, in the same order and with the same labels.
    /// The result labels are the property labels, so the two cannot drift apart.
    /// </summary>
    public string Code(CodeLanguage language)
    {
        (string x, string source) = SampleName;
        int k = ClassesUsed > 0 ? ClassesUsed : SummaryStatistics.SturgesClasses(Math.Max(Sample.Count, 1));
        StringBuilder code = new();
        void Line(string text = "") => code.Append(text).Append('\n');
        void Print(string label, string python, string r, int decimals) =>
            Line(CodeText.Print(label, language == CodeLanguage.Python ? python : r, decimals, language));

        void ControlLimits()
        {
            Line(CodeText.Comment("Control limits of an individuals chart: the mean ± 3 sample standard deviations", language));

            if (language == CodeLanguage.Python)
            {
                Line($"lcl = np.mean({x}) - 3 * np.std({x}, ddof=1)");
                Line($"ucl = np.mean({x}) + 3 * np.std({x}, ddof=1)");
            }
            else
            {
                Line($"lcl <- mean({x}) - 3 * sd({x})");
                Line($"ucl <- mean({x}) + 3 * sd({x})");
            }

            Print("Lower control limit (LCL)", "lcl", "lcl", 4);
            Print("Upper control limit (UCL)", "ucl", "ucl", 4);
        }

        if (language == CodeLanguage.Python)
        {
            Line(CodeText.PythonImports(scipyStats: true));
            Line(CodeText.Comment($"Sample: {source}", language));
            Line(CodeText.Vector(x, Sample.YValues, language));
            Line($"n = len({x})");
            Line();
            Line("print(f\"Observations (n): {n}\")");
        }
        else
        {
            Line(CodeText.Comment($"Sample: {source}", language));
            Line(CodeText.Vector(x, Sample.YValues, language));
            Line($"n <- length({x})");
            Line();
            Line("cat(\"Observations (n):\", n, \"\\n\")");
        }

        Print("Mean", $"np.mean({x})", $"mean({x})", 4);
        Print("Median", $"np.median({x})", $"median({x})", 4);

        if (language == CodeLanguage.Python)
        {
            Line($"mode = stats.mode({x}, keepdims=False)");
            Line("print(f\"Mode: {mode.mode if mode.count > 1 else 'none'}\")");
            Line(CodeText.Comment("The mode as a number only when one value is more frequent than all others", language));
            Line($"values, counts = np.unique({x}, return_counts=True)");
            Line("top = values[counts == counts.max()]");
            Line("mode_value = top[0] if counts.max() > 1 and top.size == 1 else float('nan')");
        }
        else
        {
            Line($"tab <- table({x})");
            Line("cat(\"Mode:\", if (max(tab) > 1) as.numeric(names(tab)[tab == max(tab)]) else \"none\", \"\\n\")");
            Line(CodeText.Comment("The mode as a number only when one value is more frequent than all others", language));
            Line("mode_value <- if (max(tab) > 1 && sum(tab == max(tab)) == 1) as.numeric(names(tab)[tab == max(tab)]) else NA");
        }

        Print("Mode, when there is one", "mode_value", "mode_value", 4);

        Print("Minimum", $"np.min({x})", $"min({x})", 4);
        Print("Maximum", $"np.max({x})", $"max({x})", 4);
        Print("Range", $"np.ptp({x})", $"max({x}) - min({x})", 4);
        Line(CodeText.Comment("ddof=1 / sd(): the sample form, dividing by n - 1", language));
        Print("Standard deviation (s)", $"np.std({x}, ddof=1)", $"sd({x})", 4);
        Print("Variance (s²)", $"np.var({x}, ddof=1)", $"var({x})", 4);
        Print("Population standard deviation (σ)", $"np.std({x})", $"sqrt(mean(({x} - mean({x}))^2))", 4);
        Print("Population variance (σ²)", $"np.var({x})", $"mean(({x} - mean({x}))^2)", 4);
        Print("Standard error of the mean", $"stats.sem({x})", $"sd({x}) / sqrt(n)", 4);
        Print("Coefficient of variation", $"100 * np.std({x}, ddof=1) / np.mean({x})", $"100 * sd({x}) / mean({x})", 2);
        Line();
        Line(CodeText.Comment("Quartiles by linear interpolation (R type 7, NumPy's default)", language));

        if (language == CodeLanguage.Python)
        {
            Line($"q1, q3 = np.percentile({x}, [25, 75])");
            Line("iqr = q3 - q1");
        }
        else
        {
            Line($"q <- quantile({x}, c(0.25, 0.75))");
            Line("q1 <- q[[1]]; q3 <- q[[2]]");
            Line("iqr <- q3 - q1");
        }

        Print("First quartile (Q1)", "q1", "q1", 4);
        Print("Third quartile (Q3)", "q3", "q3", 4);
        Print("Interquartile range", "iqr", "iqr", 4);
        Line(CodeText.Comment("Tukey's hinges: the medians of the two halves (R fivenum)", language));

        if (language == CodeLanguage.Python)
        {
            Line("def hinges(v):");
            Line("    v = np.sort(v); m = len(v); n4 = np.floor((m + 3) / 2) / 2");
            Line("    at = lambda p: (v[int(np.floor(p)) - 1] + v[int(np.ceil(p)) - 1]) / 2");
            Line("    return at(n4), at(m + 1 - n4)");
            Line($"lower_hinge, upper_hinge = hinges({x})");
        }
        else
        {
            Line($"h <- fivenum({x})");
            Line("lower_hinge <- h[2]; upper_hinge <- h[4]");
        }

        Print("Lower hinge (Tukey)", "lower_hinge", "lower_hinge", 4);
        Print("Upper hinge (Tukey)", "upper_hinge", "upper_hinge", 4);
        Print("Lower fence", "q1 - 1.5 * iqr", "q1 - 1.5 * iqr", 4);
        Print("Upper fence", "q3 + 1.5 * iqr", "q3 + 1.5 * iqr", 4);

        if (language == CodeLanguage.Python)
        {
            Line($"outliers = {x}[({x} < q1 - 1.5 * iqr) | ({x} > q3 + 1.5 * iqr)]");
            Line("print(f\"Outliers: {np.sort(outliers).tolist() if outliers.size else 'none'}\")");
            Line("print(f\"Number of outliers: {outliers.size}\")");
            Line();
            Line(CodeText.Comment("bias=False: the adjusted sample estimates, as Excel's SKEW and KURT", language));
            Print("Skewness", $"stats.skew({x}, bias=False)", string.Empty, 4);
            Print("Excess kurtosis", $"stats.kurtosis({x}, bias=False)", string.Empty, 4);
            Line();
            ControlLimits();
            Line($"print(f\"Points outside the control limits: {{np.sum(({x} < lcl) | ({x} > ucl))}}\")");
            Line();
            Line(CodeText.Comment("Frequency table with k classes of equal width (Sturges' rule: ceil(log2(n) + 1))", language));
            Line($"k = {k}");
            Line($"counts, edges = np.histogram({x}, bins=k)");
            Line("print(f\"Classes used: {k}\")");
            Line("for lower, upper, count in zip(edges[:-1], edges[1:], counts):");
            Line("    print(f\"  [{lower:.4g}, {upper:.4g}): {count}\")");
        }
        else
        {
            Line($"outliers <- sort({x}[{x} < q1 - 1.5 * iqr | {x} > q3 + 1.5 * iqr])");
            Line("cat(\"Outliers:\", if (length(outliers) > 0) outliers else \"none\", \"\\n\")");
            Line("cat(\"Number of outliers:\", length(outliers), \"\\n\")");
            Line();
            Line(CodeText.Comment("Adjusted sample skewness and excess kurtosis (as Excel's SKEW and KURT; e1071 type = 2)", language));
            Line($"m <- mean({x}); s <- sd({x})");
            Line($"g1 <- n / ((n - 1) * (n - 2)) * sum((({x} - m) / s)^3)");
            Line($"g2 <- n * (n + 1) / ((n - 1) * (n - 2) * (n - 3)) * sum((({x} - m) / s)^4) - 3 * (n - 1)^2 / ((n - 2) * (n - 3))");
            Print("Skewness", string.Empty, "g1", 4);
            Print("Excess kurtosis", string.Empty, "g2", 4);
            Line();
            ControlLimits();
            Line($"cat(\"Points outside the control limits:\", sum({x} < lcl | {x} > ucl), \"\\n\")");
            Line();
            Line(CodeText.Comment("Frequency table with k classes of equal width (Sturges' rule: ceiling(log2(n) + 1))", language));
            Line($"k <- {k}");
            Line($"breaks <- seq(min({x}), max({x}), length.out = k + 1)");
            Line($"h <- hist({x}, breaks = breaks, right = FALSE, include.lowest = TRUE, plot = FALSE)");
            Line("cat(\"Classes used:\", k, \"\\n\")");
            Line("print(data.frame(lower = head(h$breaks, -1), upper = tail(h$breaks, -1), count = h$counts))");
        }

        return code.ToString();
    }
}

/// <summary>While a project column is chosen, the sample comes from it and is locked.</summary>
public sealed class DescriptiveStatisticsSourceMethod : CalculationMethod<DescriptiveStatistics>
{
    public override string Name => "Data table column";

    public override string Description => "The sample is copied from the chosen data table column.";

    public override string Citation => "Statistics+ project data.";

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
        "Count, mean, median, mode; range, sample and population standard deviation and variance, " +
        "standard error, coefficient of variation; quartiles (interpolated and Tukey's hinges), " +
        "interquartile range, fences and outliers; adjusted skewness and excess kurtosis; a " +
        "frequency table of equal-width classes.";

    public override string Citation =>
        "EGN3443 Module 2 (Descriptive Statistics); Montgomery, D. C. and Runger, G. C., Applied " +
        "Statistics and Probability for Engineers, 7th ed., Wiley, 2018, Chapter 6; NIST/SEMATECH " +
        "e-Handbook of Statistical Methods, §1.3.5 (quantitative techniques).";

    public override IReadOnlyList<string> Inputs => [nameof(DescriptiveStatistics.Sample), nameof(DescriptiveStatistics.Classes)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(DescriptiveStatistics.Count),
        nameof(DescriptiveStatistics.Mean),
        nameof(DescriptiveStatistics.Median),
        nameof(DescriptiveStatistics.Mode),
        nameof(DescriptiveStatistics.ModeValue),
        nameof(DescriptiveStatistics.Minimum),
        nameof(DescriptiveStatistics.Maximum),
        nameof(DescriptiveStatistics.Range),
        nameof(DescriptiveStatistics.StandardDeviation),
        nameof(DescriptiveStatistics.Variance),
        nameof(DescriptiveStatistics.PopulationStandardDeviation),
        nameof(DescriptiveStatistics.PopulationVariance),
        nameof(DescriptiveStatistics.StandardError),
        nameof(DescriptiveStatistics.CoefficientOfVariation),
        nameof(DescriptiveStatistics.FirstQuartile),
        nameof(DescriptiveStatistics.ThirdQuartile),
        nameof(DescriptiveStatistics.InterquartileRange),
        nameof(DescriptiveStatistics.LowerHinge),
        nameof(DescriptiveStatistics.UpperHinge),
        nameof(DescriptiveStatistics.LowerFence),
        nameof(DescriptiveStatistics.UpperFence),
        nameof(DescriptiveStatistics.Outliers),
        nameof(DescriptiveStatistics.OutlierCount),
        nameof(DescriptiveStatistics.Skewness),
        nameof(DescriptiveStatistics.Kurtosis),
        nameof(DescriptiveStatistics.LowerControlLimit),
        nameof(DescriptiveStatistics.UpperControlLimit),
        nameof(DescriptiveStatistics.PointsOutsideLimits),
        nameof(DescriptiveStatistics.ClassesUsed),
    ];

    public override void Apply(DescriptiveStatistics s)
    {
        ArgumentNullException.ThrowIfNull(s);

        double[] x = s.Sample.YValues;

        s.Count = x.Length;
        s.Mean = SummaryStatistics.Mean(x);
        s.Median = SummaryStatistics.Median(x);
        IReadOnlyList<double> modes = SummaryStatistics.Modes(x);
        s.Mode = x.Length == 0 ? string.Empty : Describe(modes);
        s.ModeValue = modes.Count == 1 ? modes[0] : double.NaN;
        s.Minimum = SummaryStatistics.Min(x);
        s.Maximum = SummaryStatistics.Max(x);
        s.Range = SummaryStatistics.Range(x);
        s.StandardDeviation = SummaryStatistics.SampleStandardDeviation(x);
        s.Variance = SummaryStatistics.SampleVariance(x);
        s.PopulationStandardDeviation = SummaryStatistics.PopulationStandardDeviation(x);
        s.PopulationVariance = SummaryStatistics.PopulationVariance(x);
        s.StandardError = SummaryStatistics.StandardError(x);
        s.CoefficientOfVariation = 100 * SummaryStatistics.CoefficientOfVariation(x);

        Quartiles quartiles = SummaryStatistics.Type7Quartiles(x);
        Quartiles hinges = SummaryStatistics.TukeyHinges(x);
        s.FirstQuartile = quartiles.Q1;
        s.ThirdQuartile = quartiles.Q3;
        s.InterquartileRange = quartiles.Iqr;
        s.LowerHinge = hinges.Q1;
        s.UpperHinge = hinges.Q3;
        s.LowerFence = quartiles.LowerFence;
        s.UpperFence = quartiles.UpperFence;
        IReadOnlyList<double> outliers = SummaryStatistics.Outliers(x, quartiles);
        s.Outliers = x.Length == 0 ? string.Empty : Describe(outliers);
        s.OutlierCount = outliers.Count;

        s.Skewness = SummaryStatistics.Skewness(x);
        s.Kurtosis = SummaryStatistics.ExcessKurtosis(x);

        (s.LowerControlLimit, s.UpperControlLimit) = SummaryStatistics.ControlLimits(x);
        double lcl = s.LowerControlLimit, ucl = s.UpperControlLimit;
        s.PointsOutsideLimits = double.IsNaN(lcl) ? 0 : x.Count(v => v < lcl || v > ucl);

        int classes = s.Classes > 0 ? (int)Math.Round(s.Classes) : SummaryStatistics.SturgesClasses(x.Length);
        s.ClassesUsed = x.Length == 0 ? 0 : classes;
        s.FrequencyTable = SummaryStatistics.FrequencyTable(x, classes);
    }

    private static string Describe(IReadOnlyList<double> values) =>
        values.Count == 0 ? "none" : string.Join(", ", values.Select(v => v.ToString("G6", CultureInfo.CurrentCulture)));
}
