using System.ComponentModel;
using System.Globalization;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

/// <summary>Which chi-square test.</summary>
public enum ChiSquareKind
{
    [Description("Goodness of fit (one list of counts)")]
    GoodnessOfFit = 0,

    [Description("Independence or homogeneity (a contingency table)")]
    Independence,
}

/// <summary>What the goodness-of-fit test compares the counts with.</summary>
public enum ExpectedProportions
{
    [Description("Equal for every category")]
    Equal = 0,

    [Description("Entered (proportions or weights)")]
    Entered,
}

/// <summary>Whether a 2 × 2 table's χ² is corrected for continuity.</summary>
public enum ContinuityCorrection
{
    [Description("None (scipy correction=False, R correct = FALSE)")]
    None = 0,

    [Description("Yates (2 × 2 tables only)")]
    Yates,
}

/// <summary>
/// EGN3443 hypothesis testing on counts (Montgomery and Runger §9-7, §9-8): Pearson's chi-square
/// test of goodness of fit to stated proportions, and of independence (or homogeneity) in an
/// r × c contingency table.
/// </summary>
[StatistleHelp(
    "Chi-square tests work on counts — how many items fell in each category — not on " +
    "measurements. Goodness of fit asks whether one list of counts follows proportions stated in " +
    "advance (a fair die, a supplier's claimed defect mix, a Poisson model). Independence asks " +
    "whether two classifications of the same items are related (job type and the pension plan " +
    "chosen; shift and the type of defect); the same arithmetic tests homogeneity, whether " +
    "several groups share one set of proportions. Each test compares every observed count O " +
    "with the count E expected under H₀ and adds (O − E)²/E; a large total χ² is evidence " +
    "against H₀. The χ² distribution is an approximation that holds when every expected count " +
    "is at least 5 — the form checks that and warns. No continuity correction is applied unless " +
    "Yates is chosen for a 2 × 2 table. The Code tab writes scipy's chisquare and " +
    "chi2_contingency and R's chisq.test.")]
public sealed class ChiSquareTest : Analysis, IHasFigures, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, ICodeSource
{
    private ChiSquareKind _test = ChiSquareKind.GoodnessOfFit;
    private double _alpha = 0.05;
    private string _observedColumn = string.Empty;
    private ExpectedProportions _proportions = ExpectedProportions.Equal;
    private string _expectedColumn = string.Empty;
    private int _estimatedParameters;
    private string _categoryNames = string.Empty;
    private string _countsTable = string.Empty;
    private string _countsColumns = string.Empty;
    private string _rowNames = string.Empty;
    private ContinuityCorrection _correction = ContinuityCorrection.None;

    private double _categories = double.NaN;
    private double _rows = double.NaN;
    private double _columns = double.NaN;
    private double _total = double.NaN;
    private string _hypotheses = string.Empty;
    private string _expectedTable = string.Empty;
    private double _smallestExpected = double.NaN;
    private int _cellsBelowFive;
    private string _expectedCheck = string.Empty;
    private double _statistic = double.NaN;
    private double _degreesOfFreedom = double.NaN;
    private double _critical = double.NaN;
    private double _p = double.NaN;
    private string _decision = string.Empty;
    private double _cramersV = double.NaN;
    private string _notes = string.Empty;

    public ChiSquareTest()
    {
        Name = "Chi-square test";

        // A die rolled 60 times, so the analysis shows something the moment it is opened.
        foreach (double count in new[] { 8.0, 12, 9, 11, 6, 14 })
        {
            Observed.Add(Observed.Count, count);
        }

        // Montgomery and Runger Example 9-14: pension plan preferred by job type.
        Counts.AddColumn("Plan 1", [160, 40]);
        Counts.AddColumn("Plan 2", [140, 60]);
        Counts.AddColumn("Plan 3", [40, 60]);
        Recalculate();
    }

    public override string Title => "Chi-square tests";

    public override string Summary => "Counts: goodness of fit and independence in a contingency table — expected counts, χ², df, p (M&R §9-7, §9-8)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 34;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public ChiSquareOutcome? Outcome { get; private set; }

    /// <summary>The observed counts as tested: the categories, or the cells row by row.</summary>
    [StatistleIgnore]
    public IReadOnlyList<double> ObservedCounts { get; private set; } = [];

    /// <summary>The expected counts in the same order as <see cref="ObservedCounts"/>.</summary>
    [StatistleIgnore]
    public IReadOnlyList<double> ExpectedCounts { get; private set; } = [];

    /// <summary>The proportions the goodness-of-fit test used, adding to 1.</summary>
    [StatistleIgnore]
    public IReadOnlyList<double> ProportionsUsed { get; private set; } = [];

    /// <summary>The category names (goodness of fit) or the cells' "row / column" names.</summary>
    [StatistleIgnore]
    public IReadOnlyList<string> CategoryLabels { get; private set; } = [];

    [StatistleIgnore]
    public IReadOnlyList<string> RowLabels { get; private set; } = [];

    [StatistleIgnore]
    public IReadOnlyList<string> ColumnLabels { get; private set; } = [];

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Test", Role = VariableRole.Design,
        Help = "Goodness of fit: one list of counts, one per category, compared with proportions " +
               "stated before the data were seen (a fair die, a claimed mix, a fitted model). " +
               "Independence: a table of counts classified two ways — rows by one factor, columns " +
               "by the other — asking whether the factors are related. The same table tests " +
               "homogeneity when each row is a separate sample (do three suppliers share one " +
               "defect mix?). The form shows only the entries the chosen test uses.")]
    public ChiSquareKind Test
    {
        get => _test;
        set => SetProperty(ref _test, value);
    }

    [StatistleProperty(Label = "Significance level (α)", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.0001, Max = 0.5,
        Help = "The probability of rejecting H₀ when it is true that the test allows; 0.05 is usual. " +
               "It sets the critical value χ²(α, df).")]
    public double Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    [StatistleProperty(Label = "Observed counts from", Role = VariableRole.Input,
        Help = "A column of a data table holding the counts, one per category — or 'Entered below' " +
               "to type them into this analysis. With a column chosen its values show below, read-only.")]
    public string ObservedColumn
    {
        get => _observedColumn;
        set => SetProperty(ref _observedColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Observed counts", Role = VariableRole.Input, ValuesOnly = true,
        Help = "How many items fell in each category, one count per line, in the same order as the " +
               "expected proportions. Counts, not percentages: 60 rolls of a die give six counts " +
               "that add to 60. Python stats.chisquare(observed, f_exp); R chisq.test(observed, p = p).")]
    public DataSeries Observed { get; } = new();

    [StatistleProperty(Label = "Expected proportions", Role = VariableRole.Design,
        Help = "Equal: every category is equally likely under H₀ (a fair die, no preferred day). " +
               "Entered: type the proportion H₀ gives each category — or any weights in that " +
               "ratio, such as expected counts — and they are rescaled to add to 1.")]
    public ExpectedProportions Proportions
    {
        get => _proportions;
        set => SetProperty(ref _proportions, value);
    }

    [StatistleProperty(Label = "Expected proportions from", Role = VariableRole.Input,
        Help = "A data table column holding the proportions, one per category, or 'Entered below' to type them.")]
    public string ExpectedColumn
    {
        get => _expectedColumn;
        set => SetProperty(ref _expectedColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Expected proportions (or weights)", Role = VariableRole.Input, ValuesOnly = true,
        Help = "The proportion of items H₀ puts in each category, one per line, in the same order as " +
               "the counts; each must be greater than 0. If they add to 1 (within 0.01) they are " +
               "proportions; otherwise they are read as weights — 1, 2, 1 means ¼, ½, ¼ — and the " +
               "Notes say so. Either way they are rescaled to add to exactly 1, as R's " +
               "chisq.test(rescale.p = TRUE).")]
    public DataSeries Expected { get; } = new();

    [StatistleProperty(Label = "Parameters estimated from the data (m)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0, Max = 20,
        Help = "0 when the proportions were stated in advance. When they come from a model fitted " +
               "to these same counts — a Poisson whose mean λ was estimated from the data — each " +
               "estimated parameter costs one degree of freedom: df = k − 1 − m (M&R §9-7). " +
               "Python stats.chisquare(..., ddof=m).")]
    public int EstimatedParameters
    {
        get => _estimatedParameters;
        set => SetProperty(ref _estimatedParameters, value);
    }

    [StatistleProperty(Label = "Category names", Role = VariableRole.Input,
        Help = "Optional names for the categories, separated by commas, in the order of the counts " +
               "(Mon, Tue, Wed …). Left blank they are Category 1, Category 2 ….")]
    public string CategoryNames
    {
        get => _categoryNames;
        set => SetProperty(ref _categoryNames, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Table of counts from", Role = VariableRole.Input,
        Help = "A data table whose columns are the contingency table's columns (one row per " +
               "category of the row factor) — or 'Entered below' to type the table into this " +
               "analysis. With a data table chosen its counts show below, read-only.")]
    public string CountsTable
    {
        get => _countsTable;
        set => SetProperty(ref _countsTable, TableId(value));
    }

    [StatistleProperty(Label = "Columns used", Role = VariableRole.Input,
        Help = "The data table's columns that make up the contingency table, by name and separated " +
               "by commas. Leave blank to use every column.")]
    public string CountsColumns
    {
        get => _countsColumns;
        set => SetProperty(ref _countsColumns, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Contingency table (counts)", Role = VariableRole.Input,
        Help = "First line: the column categories' names (Plan 1, Plan 2, Plan 3). Then one line " +
               "per row category with its counts, separated by tabs, commas or spaces — paste it " +
               "straight from a spreadsheet. Every cell needs a count (0 is a count). Python " +
               "stats.chi2_contingency(table, correction=False); R chisq.test(table, correct = FALSE).")]
    public ColumnTable Counts { get; } = new();

    [StatistleProperty(Label = "Row names", Role = VariableRole.Input,
        Help = "Optional names for the table's rows, separated by commas, top to bottom (Salaried, " +
               "Hourly). Left blank they are Row 1, Row 2 ….")]
    public string RowNames
    {
        get => _rowNames;
        set => SetProperty(ref _rowNames, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Continuity correction", Role = VariableRole.Design,
        Help = "None is the course's χ² and the default here. Yates subtracts ½ from each |O − E| " +
               "before squaring, which makes a 2 × 2 test more conservative (a larger p); it is " +
               "what R's chisq.test does by default on a 2 × 2 table. It is never used on a larger table.")]
    public ContinuityCorrection Correction
    {
        get => _correction;
        set => SetProperty(ref _correction, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Categories (k)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many categories the counts fall in.")]
    public double Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    [StatistleProperty(Label = "Rows (r)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many categories the row factor has.")]
    public double Rows
    {
        get => _rows;
        set => SetProperty(ref _rows, value);
    }

    [StatistleProperty(Label = "Columns (c)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many categories the column factor has.")]
    public double Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    [StatistleProperty(Label = "Total count (n)", Decimals = 0, Role = VariableRole.Calculated,
        Help = "The number of items counted: the sum of every observed count.")]
    public double Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    [StatistleProperty(Label = "Hypotheses", Role = VariableRole.Output,
        Help = "What the test decides between, in words: H₀ is the model that gives the expected counts.")]
    public string Hypotheses
    {
        get => _hypotheses;
        set => SetProperty(ref _hypotheses, value);
    }

    [StatistleProperty(Label = "Expected counts", Role = VariableRole.Output,
        Help = "The counts H₀ predicts. Goodness of fit: Eᵢ = n·pᵢ, shown with each observed count " +
               "and its contribution (O − E)²/E to χ² — the large contributions show where the " +
               "data depart from H₀. Independence: Eᵢⱼ = (row i total)(column j total)/n, one line per row.")]
    public string ExpectedTable
    {
        get => _expectedTable;
        set => SetProperty(ref _expectedTable, value);
    }

    [StatistleProperty(Label = "Smallest expected count", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The smallest Eᵢ. The χ² approximation is trusted when every expected count is at " +
               "least 5 (M&R §9-7); some texts accept a few as low as 3 or 1.")]
    public double SmallestExpected
    {
        get => _smallestExpected;
        set => SetProperty(ref _smallestExpected, value);
    }

    [StatistleProperty(Label = "Expected counts below 5", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many categories or cells expect fewer than 5 items. Above 0, read the p-value with care.")]
    public int CellsBelowFive
    {
        get => _cellsBelowFive;
        set => SetProperty(ref _cellsBelowFive, value);
    }

    [StatistleProperty(Label = "Expected-count check", Role = VariableRole.Output,
        Help = "Whether every expected count is at least 5, and what to do when one is not: combine " +
               "neighbouring categories (as M&R do in Example 9-12) or, for a 2 × 2 table, use " +
               "Fisher's exact test (Python stats.fisher_exact; R fisher.test).")]
    public string ExpectedCheck
    {
        get => _expectedCheck;
        set => SetProperty(ref _expectedCheck, value);
    }

    [StatistleProperty(Label = "Chi-square statistic (χ²)", Decimals = 4, Role = VariableRole.Output,
        Help = "χ² = Σ(O − E)²/E over every category or cell. 0 when the counts are exactly as " +
               "expected; the further they depart, the larger it is.")]
    public double Statistic
    {
        get => _statistic;
        set => SetProperty(ref _statistic, value);
    }

    [StatistleProperty(Label = "Degrees of freedom (df)", Decimals = 0, Role = VariableRole.Calculated,
        Help = "Goodness of fit: k − 1 − m. Independence: (r − 1)(c − 1). The number of counts free " +
               "to vary once the totals the expected counts use are fixed.")]
    public double DegreesOfFreedom
    {
        get => _degreesOfFreedom;
        set => SetProperty(ref _degreesOfFreedom, value);
    }

    [StatistleProperty(Label = "Critical value χ²(α, df)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The value with probability α above it in the χ² distribution with df degrees of " +
               "freedom: reject H₀ when χ² exceeds it. The table look-up of the hand method; " +
               "Python stats.chi2.ppf(1 − α, df); R qchisq(1 − α, df).")]
    public double Critical
    {
        get => _critical;
        set => SetProperty(ref _critical, value);
    }

    [StatistleProperty(Label = "p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "P(χ² ≥ the observed value) when H₀ is true — always the upper tail, since any " +
               "departure from H₀ makes χ² larger. Below α: reject H₀.")]
    public double P
    {
        get => _p;
        set => SetProperty(ref _p, value);
    }

    [StatistleProperty(Label = "Decision", Role = VariableRole.Output,
        Help = "The conclusion at the chosen α, in words.")]
    public string Decision
    {
        get => _decision;
        set => SetProperty(ref _decision, value);
    }

    [StatistleProperty(Label = "Cramér's V", Decimals = 4, Role = VariableRole.Calculated,
        Help = "√(χ²/(n·(min(r, c) − 1))), from the uncorrected χ²: how strong the association is, " +
               "from 0 (none) to 1 (each row category fixes the column). χ² says whether there is " +
               "an association; V says how strong. About 0.1 is weak, 0.3 moderate, 0.5 strong. " +
               "Python stats.contingency.association(table, method='cramer').")]
    public double CramersV
    {
        get => _cramersV;
        set => SetProperty(ref _cramersV, value);
    }

    [StatistleProperty(Label = "Notes", Role = VariableRole.Calculated,
        Help = "Anything the calculation did that you should know: proportions rescaled, names " +
               "filled in, Yates' correction used or not.")]
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    public override void Recalculate()
    {
        Outcome = null;
        ObservedCounts = ExpectedCounts = ProportionsUsed = [];
        CategoryLabels = RowLabels = ColumnLabels = [];
        Categories = Rows = Columns = double.NaN;   // blank in the test that does not use them, so a task cannot ask them there
        CellsBelowFive = 0;
        Hypotheses = ExpectedTable = ExpectedCheck = Decision = Notes = string.Empty;
        Total = SmallestExpected = Statistic = DegreesOfFreedom = Critical = P = CramersV = double.NaN;

        if (double.IsNaN(Alpha) || Alpha <= 0 || Alpha >= 1)
        {
            Problem = "Enter a significance level between 0 and 1, such as 0.05.";
            return;
        }

        Problem = Test == ChiSquareKind.GoodnessOfFit ? GoodnessOfFit() : Independence();
    }

    private string? GoodnessOfFit()
    {
        List<string> notes = [];

        if (DataSources.Split(ObservedColumn).TableId > 0)
        {
            string? pulled = DataSources.Pull(this, ObservedColumn, Observed);

            if (pulled is not null)
            {
                return pulled;
            }
        }

        double[] observed = Observed.YValues;

        if (observed.Length < 2)
        {
            return observed.Length == 0 && DataSources.Split(ObservedColumn).TableId == 0
                ? "Type the observed counts, one per category per line — at least two categories."
                : "A goodness-of-fit test needs counts for at least two categories.";
        }

        if (observed.Any(o => !double.IsFinite(o) || o < 0))
        {
            return "Counts cannot be negative. Enter how many items fell in each category.";
        }

        if (observed.Sum() <= 0)
        {
            return "The counts add to 0, so there is nothing to test. Enter how many items fell in each category.";
        }

        if (EstimatedParameters < 0)
        {
            return "The number of estimated parameters cannot be negative; enter 0 when the proportions were stated in advance.";
        }

        double[] proportions;

        if (Proportions == ExpectedProportions.Equal)
        {
            proportions = [.. observed.Select(_ => 1.0 / observed.Length)];
        }
        else
        {
            if (DataSources.Split(ExpectedColumn).TableId > 0)
            {
                string? pulled = DataSources.Pull(this, ExpectedColumn, Expected);

                if (pulled is not null)
                {
                    return pulled;
                }
            }

            double[] weights = Expected.YValues;

            if (weights.Length != observed.Length)
            {
                return $"There are {observed.Length} observed counts and {weights.Length} expected proportions. " +
                       "Enter one proportion per category, in the same order as the counts.";
            }

            if (ChiSquare.Proportions(weights) is not double[] rescaled)
            {
                return "Every expected proportion must be greater than 0 — a category H₀ says cannot happen " +
                       "cannot be tested this way. Remove it, or combine it with a neighbour.";
            }

            double sum = weights.Sum();

            if (Math.Abs(sum - 1) > 0.01)
            {
                notes.Add($"The expected entries add to {sum:G6}, not 1, so they were read as weights and rescaled to proportions that add to 1.");
            }

            proportions = rescaled;
        }

        int df = observed.Length - 1 - EstimatedParameters;

        if (df < 1)
        {
            return $"With {observed.Length} categories and {EstimatedParameters} estimated parameter(s) the degrees of freedom " +
                   $"k − 1 − m are {df}; the test needs at least 1. Use more categories or fewer estimated parameters.";
        }

        ChiSquareOutcome outcome = ChiSquare.GoodnessOfFit(observed, proportions, EstimatedParameters, Alpha, out double[] expected);
        CategoryLabels = Names(CategoryNames, observed.Length, "Category", notes, "category");
        ObservedCounts = observed;
        ExpectedCounts = expected;
        ProportionsUsed = proportions;
        Categories = observed.Length;
        Hypotheses = Proportions == ExpectedProportions.Equal
            ? $"H₀: all {observed.Length} categories are equally likely (each p = 1/{observed.Length}); H₁: they are not"
            : "H₀: the category proportions are the ones entered; H₁: at least one differs";
        ExpectedTable = string.Join("\n", observed.Select((o, i) =>
            $"{CategoryLabels[i]}: O = {o.ToString("G", CultureInfo.InvariantCulture)}, E = {expected[i].ToString("F4", CultureInfo.InvariantCulture)}, " +
            $"(O − E)²/E = {((o - expected[i]) * (o - expected[i]) / expected[i]).ToString("F4", CultureInfo.InvariantCulture)}"));
        Finish(outcome, notes,
            "the counts do not follow the expected proportions",
            "the counts are consistent with the expected proportions");
        return null;
    }

    private string? Independence()
    {
        List<string> notes = [];
        DataTable? source = CountsSource();

        if (source is null && int.TryParse(CountsTable, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return "The data table chosen for the counts is no longer in the project. Choose another, or 'Entered below'.";
        }

        if (source is not null)
        {
            string? pulled = PullCounts(source);

            if (pulled is not null)
            {
                return pulled;
            }
        }

        int c = Counts.ColumnCount, r = Counts.RowCount;

        if (c < 2 || r < 2)
        {
            return "A contingency table needs at least two rows and two columns of counts: a header line of column " +
                   "names, then one line of counts per row category.";
        }

        if (Counts.Columns.FirstOrDefault(col => col.Count != r) is DataColumn ragged)
        {
            return $"Column '{ragged.Name}' has {ragged.Count} count(s) and the table has {r} rows. Every cell needs a count; type 0 for none.";
        }

        double[,] counts = new double[r, c];

        for (int j = 0; j < c; j++)
        {
            for (int i = 0; i < r; i++)
            {
                counts[i, j] = Counts.Columns[j].Values[i];

                if (!double.IsFinite(counts[i, j]) || counts[i, j] < 0)
                {
                    return "Counts cannot be negative. Enter how many items fall in each cell.";
                }
            }
        }

        RowLabels = Names(RowNames, r, "Row", notes, "row");
        ColumnLabels = [.. Counts.Columns.Select((col, j) => string.IsNullOrWhiteSpace(col.Name) ? $"Column {j + 1}" : col.Name)];

        for (int i = 0; i < r; i++)
        {
            if (Enumerable.Range(0, c).Sum(j => counts[i, j]) <= 0)
            {
                return $"Row '{RowLabels[i]}' has no counts, so it expects none either. Remove that row.";
            }
        }

        for (int j = 0; j < c; j++)
        {
            if (Enumerable.Range(0, r).Sum(i => counts[i, j]) <= 0)
            {
                return $"Column '{ColumnLabels[j]}' has no counts, so it expects none either. Remove that column.";
            }
        }

        bool yates = Correction == ContinuityCorrection.Yates;
        ChiSquareOutcome outcome = ChiSquare.Independence(counts, yates, Alpha, out double[,] expected, out double uncorrected);

        if (outcome.YatesApplied)
        {
            notes.Add($"Yates' continuity correction was applied (2 × 2 table): χ² without it is {uncorrected:F4}.");
        }
        else if (yates)
        {
            notes.Add($"Yates' correction applies only to a 2 × 2 table; this table is {r} × {c}, so it was not used.");
        }
        else if (r == 2 && c == 2)
        {
            notes.Add("No continuity correction (the course's χ²). R's chisq.test applies Yates' by default on a 2 × 2 table; the code turns it off.");
        }

        Rows = r;
        Columns = c;
        ObservedCounts = [.. Enumerable.Range(0, r * c).Select(k => counts[k / c, k % c])];
        ExpectedCounts = [.. Enumerable.Range(0, r * c).Select(k => expected[k / c, k % c])];
        CategoryLabels = [.. Enumerable.Range(0, r * c).Select(k => $"{RowLabels[k / c]} / {ColumnLabels[k % c]}")];
        Hypotheses = "H₀: the row and column classifications are independent (every row has the same column proportions); H₁: they are not";
        ExpectedTable = "columns = " + string.Join(", ", ColumnLabels) + "\n" + string.Join("\n", Enumerable.Range(0, r).Select(i =>
            $"{RowLabels[i]}: " + string.Join(", ", Enumerable.Range(0, c).Select(j => expected[i, j].ToString("F2", CultureInfo.InvariantCulture)))));
        CramersV = ChiSquare.CramersV(uncorrected, outcome.Total, r, c);
        Finish(outcome, notes,
            "the row and column classifications are not independent",
            "no evidence against independence");
        return null;
    }

    private void Finish(ChiSquareOutcome outcome, List<string> notes, string reject, string keep)
    {
        Outcome = outcome;
        Total = outcome.Total;
        Statistic = outcome.Statistic;
        DegreesOfFreedom = outcome.DegreesOfFreedom;
        Critical = outcome.Critical;
        P = outcome.PValue;
        SmallestExpected = outcome.SmallestExpected;
        CellsBelowFive = outcome.CellsBelowFive;
        string a = Alpha.ToString("G3", CultureInfo.InvariantCulture);
        Decision = outcome.PValue < Alpha
            ? $"Reject H₀ at α = {a} — {reject} (χ² = {outcome.Statistic:F4} > {outcome.Critical:F4}, p = {outcome.PValue:G3})"
            : $"Do not reject H₀ at α = {a} — {keep} (χ² = {outcome.Statistic:F4} ≤ {outcome.Critical:F4}, p = {outcome.PValue:G3})";
        int cells = ExpectedCounts.Count;
        ExpectedCheck = outcome.CellsBelowFive == 0
            ? $"Every expected count is at least 5 (smallest {outcome.SmallestExpected:F2}), so the χ² approximation can be trusted."
            : $"Warning — {outcome.CellsBelowFive} of {cells} expected counts are below 5 (smallest {outcome.SmallestExpected:F2}), " +
              "so the χ² approximation may be poor. Combine neighbouring categories, or for a 2 × 2 table use Fisher's exact test.";
        Notes = string.Join(" ", notes);
    }

    /// <summary>Names from a comma list, filled with "Prefix i" where missing; a note when the list does not fit.</summary>
    private static string[] Names(string list, int count, string prefix, List<string> notes, string what)
    {
        string[] given = string.IsNullOrWhiteSpace(list) ? [] : list.Split(',', StringSplitOptions.TrimEntries);
        string[] names = [.. Enumerable.Range(0, count).Select(i => i < given.Length && given[i].Length > 0 ? given[i] : $"{prefix} {i + 1}")];

        if (given.Length > 0 && given.Length != count)
        {
            notes.Add($"{given.Length} {what} name(s) were given for {count} {what}(s); {(given.Length < count ? "the rest are numbered" : "the extra names were not used")}.");
        }

        return names;
    }

    /// <summary>Copies the chosen columns of the data table into <see cref="Counts"/>, changing it only when they differ.</summary>
    private string? PullCounts(DataTable source)
    {
        List<DataColumn> columns = [];

        if (string.IsNullOrWhiteSpace(CountsColumns))
        {
            columns.AddRange(source.Columns.Columns);
        }
        else
        {
            foreach (string name in CountsColumns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (source.Columns.Column(name) is not DataColumn column)
                {
                    return $"'{source.Name}' has no column named '{name}'. Its columns are: {string.Join(", ", source.Columns.Names)}.";
                }

                columns.Add(column);
            }
        }

        var copy = new ColumnTable(columns.Select(col => new DataColumn(col.Name, col.Values)));

        if (copy.Save() != Counts.Save())
        {
            Counts.SetColumns(copy.Columns.Select(col => new DataColumn(col.Name, col.Values)));
        }

        return null;
    }

    /// <summary>The stored table id: a reference "id␟column" (as a task's "@Table/" writes) keeps only the table.</summary>
    private static string TableId(string? value)
    {
        (int id, _) = DataSources.Split(value);
        return id > 0 ? id.ToString(CultureInfo.InvariantCulture) : value ?? string.Empty;
    }

    private DataTable? CountsSource() =>
        int.TryParse(CountsTable, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? Project?.Tables.FirstOrDefault(t => t.Id == id) : null;

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) => propertyName switch
    {
        nameof(ObservedColumn) or nameof(ExpectedColumn) => DataSources.ColumnChoices(this),
        nameof(CountsTable) =>
        [
            new ChoiceOption(string.Empty, DataSources.EnteredHere),
            .. (Project?.Tables ?? []).Select(t => new ChoiceOption(t.Id.ToString(CultureInfo.InvariantCulture), t.Name, $"{t.Columns.ColumnCount} column(s). {t.Description}".Trim())),
        ],
        _ => [],
    };

    public StatistleObject? LinkedObject(string propertyName) => propertyName switch
    {
        nameof(ObservedColumn) or nameof(Observed) => DataSources.Table(this, ObservedColumn),
        nameof(ExpectedColumn) or nameof(Expected) => DataSources.Table(this, ExpectedColumn),
        nameof(CountsTable) or nameof(CountsColumns) or nameof(Counts) => CountsSource(),
        _ => null,
    };

    /// <summary>Figure 1: the χ² distribution with the rejection region and the observed χ². Figure 2: observed and expected counts.</summary>
    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Outcome is null)
        {
            return [];
        }

        string axis = Test == ChiSquareKind.GoodnessOfFit ? "category: " : "cell, row by row: ";
        axis += string.Join(", ", CategoryLabels.Select((n, i) => $"{i + 1} = {n}"));

        if (axis.Length > 140)
        {
            axis = axis[..137] + "…";
        }

        return
        [
            ChiSquareFigures.Reference($"{Name} — χ² distribution under H₀ (df = {Outcome.DegreesOfFreedom:G4}), rejection region shaded", Outcome.DegreesOfFreedom, Outcome.Statistic, Outcome.Critical),
            ChiSquareFigures.ObservedAndExpected($"{Name} — observed and expected counts", axis, ObservedCounts, ExpectedCounts),
        ];
    }

    public string Code(CodeLanguage language)
    {
        if (Outcome is null)
        {
            return string.Empty;
        }

        var s = new ScriptBuilder(language);
        s.Imports(scipyStats: true);

        if (Test == ChiSquareKind.GoodnessOfFit)
        {
            GoodnessOfFitCode(s);
        }
        else
        {
            IndependenceCode(s);
        }

        s.Print("Smallest expected count", "expected.min()", "min(expected)", 4);
        s.PrintRaw("Expected counts below 5", "int((expected < 5).sum())", "sum(expected < 5)");
        s.PrintText("Expected-count check", ExpectedCheck);
        s.Print("Chi-square statistic (χ²)", "chi2_stat", "chi2_stat", 4);
        s.PrintRaw("Degrees of freedom (df)", "int(df)", "df");
        s.Print("Critical value χ²(α, df)", "crit", "crit", 4);
        s.Print("p-value", "p_value", "p_value", 4);
        s.PrintText("Decision", Decision);

        if (Test == ChiSquareKind.Independence)
        {
            s.Print("Cramér's V", "cramer_v", "cramer_v", 4);
        }

        s.PrintText("Notes", Notes);
        return s.ToString();
    }

    private static string Quoted(IEnumerable<string> names) =>
        string.Join(", ", names.Select(n => "\"" + n.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\""));

    private void GoodnessOfFitCode(ScriptBuilder s)
    {
        s.Line();
        s.Comment("Observed counts, one per category");
        s.Vector("observed", ObservedCounts);
        s.Either($"names = [{Quoted(CategoryLabels)}]", $"names <- c({Quoted(CategoryLabels)})");

        if (Proportions == ExpectedProportions.Equal)
        {
            s.Comment("H₀: every category equally likely");
            s.Either("p = np.full(len(observed), 1 / len(observed))", "p <- rep(1 / length(observed), length(observed))");
        }
        else
        {
            s.Comment("H₀'s proportions as entered, rescaled to add to 1 (R: rescale.p = TRUE)");
            s.Vector("p", Expected.YValues);
            s.Either("p = p / p.sum()", "p <- p / sum(p)");
        }

        s.Assign("alpha", Math.Round(Alpha, 10));
        s.Assign("m", EstimatedParameters);
        s.Comment("m = parameters estimated from these counts; each costs one degree of freedom");
        s.Line();
        s.Comment("Expected counts E = n·p and χ² = Σ(O − E)²/E on k − 1 − m degrees of freedom");
        s.Either("n = observed.sum()", "n <- sum(observed)");
        s.Either("expected = n * p", "expected <- n * p");
        s.Either("chi2_stat, p_value = stats.chisquare(observed, f_exp=expected, ddof=m)",
                 "test <- suppressWarnings(chisq.test(observed, p = p)); chi2_stat <- unname(test$statistic)");
        s.Either("df = len(observed) - 1 - m", "df <- length(observed) - 1 - m");
        s.Either(string.Empty, "p_value <- pchisq(chi2_stat, df, lower.tail = FALSE)   # chisq.test's own p uses k - 1; this allows for m");
        s.Either("crit = stats.chi2.ppf(1 - alpha, df)", "crit <- qchisq(1 - alpha, df)");
        s.Line();
        s.PrintRaw("Categories (k)", "len(observed)", "length(observed)");
        s.Print("Total count (n)", "n", "n", 0);
        s.PrintText("Hypotheses", Hypotheses);
        s.Either("print(\"Expected counts:\")", "cat(\"Expected counts:\\n\")");
        s.Either("for name, o, e in zip(names, observed, expected):\n    print(f\"  {name}: O = {o:g}, E = {e:.4f}, (O - E)^2/E = {(o - e) ** 2 / e:.4f}\")",
                 "for (i in seq_along(observed)) cat(sprintf(\"  %s: O = %g, E = %.4f, (O - E)^2/E = %.4f\\n\", names[i], observed[i], expected[i], (observed[i] - expected[i])^2 / expected[i]))");
    }

    private void IndependenceCode(ScriptBuilder s)
    {
        int r = RowLabels.Count, c = ColumnLabels.Count;
        bool yates = Outcome!.YatesApplied;
        s.Line();
        s.Comment($"The contingency table, row by row ({r} rows × {c} columns)");
        s.Vector("counts", ObservedCounts);
        s.Either($"table = counts.reshape({r}, {c})",
                 $"tab <- matrix(counts, nrow = {r}, byrow = TRUE, dimnames = list(c({Quoted(RowLabels)}), c({Quoted(ColumnLabels)})))");
        s.Either($"rows = [{Quoted(RowLabels)}]", string.Empty);
        s.Either($"cols = [{Quoted(ColumnLabels)}]", string.Empty);
        s.Assign("alpha", Math.Round(Alpha, 10));
        s.Line();
        s.Comment("E = row total × column total / n; χ² = ΣΣ(O − E)²/E on (r − 1)(c − 1) degrees of freedom");
        s.Comment(yates ? "Yates' continuity correction on this 2 × 2 table" : "No continuity correction (the course's χ²)");
        s.Either($"chi2_stat, p_value, df, expected = stats.chi2_contingency(table, correction={(yates ? "True" : "False")})",
                 $"test <- suppressWarnings(chisq.test(tab, correct = {(yates ? "TRUE" : "FALSE")}))");
        s.Either(string.Empty, "chi2_stat <- unname(test$statistic); df <- unname(test$parameter); p_value <- test$p.value; expected <- test$expected");
        s.Either("n = table.sum()", "n <- sum(tab)");
        s.Either("crit = stats.chi2.ppf(1 - alpha, df)", "crit <- qchisq(1 - alpha, df)");
        s.Comment("Cramér's V from the uncorrected χ² (scipy: stats.contingency.association(table, method='cramer'))");
        s.Either("cramer_v = np.sqrt(stats.chi2_contingency(table, correction=False)[0] / (n * (min(table.shape) - 1)))",
                 "cramer_v <- sqrt(unname(suppressWarnings(chisq.test(tab, correct = FALSE))$statistic) / (n * (min(dim(tab)) - 1)))");
        s.Line();
        s.PrintRaw("Rows (r)", "table.shape[0]", "nrow(tab)");
        s.PrintRaw("Columns (c)", "table.shape[1]", "ncol(tab)");
        s.Print("Total count (n)", "n", "n", 0);
        s.PrintText("Hypotheses", Hypotheses);
        s.Either("print(\"Expected counts:\")", "cat(\"Expected counts:\\n\")");
        s.Either("print(\"  columns = \" + \", \".join(cols))", "cat(\"  columns =\", paste(colnames(tab), collapse = \", \"), \"\\n\")");
        s.Either("for name, row in zip(rows, expected):\n    print(f\"  {name}: \" + \", \".join(f\"{e:.2f}\" for e in row))",
                 "for (i in seq_len(nrow(tab))) cat(sprintf(\"  %s: %s\\n\", rownames(tab)[i], paste(sprintf(\"%.2f\", expected[i, ]), collapse = \", \")))");
    }
}

/// <summary>While a data table column is chosen, the observed counts come from it and are locked.</summary>
public sealed class ChiSquareObservedFromColumn : CalculationMethod<ChiSquareTest>
{
    public override string Name => "Observed counts from a data table column";

    public override string Description => "The observed counts are copied from the chosen data table column.";

    public override string Citation => "Statistics+ project data.";

    public override bool IsApplicableTo(ChiSquareTest subject) =>
        subject is not null && subject.Test == ChiSquareKind.GoodnessOfFit && DataSources.Split(subject.ObservedColumn).TableId > 0;

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(ChiSquareTest.Observed)];

    public override IReadOnlyList<string> Generates => Outputs;

    public override void Apply(ChiSquareTest subject) => subject.Recalculate();
}

/// <summary>While a data table column is chosen, the expected proportions come from it and are locked.</summary>
public sealed class ChiSquareExpectedFromColumn : CalculationMethod<ChiSquareTest>
{
    public override string Name => "Expected proportions from a data table column";

    public override string Description => "The expected proportions (or weights) are copied from the chosen data table column.";

    public override string Citation => "Statistics+ project data.";

    public override bool IsApplicableTo(ChiSquareTest subject) =>
        subject is not null && subject.Test == ChiSquareKind.GoodnessOfFit && subject.Proportions == ExpectedProportions.Entered
        && DataSources.Split(subject.ExpectedColumn).TableId > 0;

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(ChiSquareTest.Expected)];

    public override IReadOnlyList<string> Generates => Outputs;

    public override void Apply(ChiSquareTest subject) => subject.Recalculate();
}

/// <summary>Pearson's goodness-of-fit test.</summary>
public sealed class ChiSquareGoodnessOfFit : CalculationMethod<ChiSquareTest>
{
    public override string Name => "Goodness of fit";

    public override string Description =>
        "Eᵢ = n·pᵢ with the proportions rescaled to add to 1 (all 1/k when equal); χ² = Σ(Oᵢ − Eᵢ)²/Eᵢ against χ²(k − 1 − m), " +
        "m the parameters estimated from the data; p = P(χ² ≥ observed); reject when χ² > χ²(α, k − 1 − m). Expected counts below 5 are flagged.";

    public override string Citation =>
        "Pearson, K., Philosophical Magazine 50, 1900, 157–175; Montgomery, D. C. and Runger, G. C., Applied Statistics and " +
        "Probability for Engineers, 7th ed., 2018, §9-7 (testing for goodness of fit; Example 9-12).";

    public override bool IsApplicableTo(ChiSquareTest subject) => subject is not null && subject.Test == ChiSquareKind.GoodnessOfFit;

    public override IReadOnlyList<string> Inputs =>
    [
        nameof(ChiSquareTest.ObservedColumn), nameof(ChiSquareTest.Observed), nameof(ChiSquareTest.Proportions),
        nameof(ChiSquareTest.EstimatedParameters), nameof(ChiSquareTest.CategoryNames),
    ];

    public override IReadOnlyList<string> Outputs => [nameof(ChiSquareTest.Categories)];

    public override void Apply(ChiSquareTest subject) => subject.Recalculate();
}

/// <summary>The proportions typed or read from a column, rather than all equal.</summary>
public sealed class ChiSquareEnteredProportions : CalculationMethod<ChiSquareTest>
{
    public override string Name => "Entered proportions";

    public override string Description =>
        "pᵢ = wᵢ/Σw: the entries are proportions when they add to 1 (within 0.01), otherwise weights; both are rescaled to add to exactly 1, " +
        "as R's chisq.test(rescale.p = TRUE).";

    public override string Citation => "R Core Team, ?chisq.test (rescale.p); Montgomery and Runger, 7th ed., §9-7.";

    public override bool IsApplicableTo(ChiSquareTest subject) =>
        subject is not null && subject.Test == ChiSquareKind.GoodnessOfFit && subject.Proportions == ExpectedProportions.Entered;

    public override IReadOnlyList<string> Inputs => [nameof(ChiSquareTest.ExpectedColumn), nameof(ChiSquareTest.Expected)];

    public override IReadOnlyList<string> Outputs => [];

    public override void Apply(ChiSquareTest subject) => subject.Recalculate();
}

/// <summary>While a data table is chosen, the contingency table comes from its columns and is locked.</summary>
public sealed class ChiSquareCountsFromTable : CalculationMethod<ChiSquareTest>
{
    public override string Name => "Contingency table from a data table";

    public override string Description => "The chosen columns of the data table are the contingency table's columns, one row per row category.";

    public override string Citation => "Statistics+ project data.";

    public override bool IsApplicableTo(ChiSquareTest subject) =>
        subject is not null && subject.Test == ChiSquareKind.Independence
        && int.TryParse(subject.CountsTable, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) && id > 0;

    public override IReadOnlyList<string> Inputs => [nameof(ChiSquareTest.CountsColumns)];

    public override IReadOnlyList<string> Outputs => [nameof(ChiSquareTest.Counts)];

    public override IReadOnlyList<string> Generates => Outputs;

    public override void Apply(ChiSquareTest subject) => subject.Recalculate();
}

/// <summary>Pearson's test of independence (or homogeneity) in an r × c table.</summary>
public sealed class ChiSquareIndependence : CalculationMethod<ChiSquareTest>
{
    public override string Name => "Independence (contingency table)";

    public override string Description =>
        "Eᵢⱼ = (row i total)(column j total)/n; χ² = ΣΣ(Oᵢⱼ − Eᵢⱼ)²/Eᵢⱼ against χ²((r − 1)(c − 1)); no continuity correction unless " +
        "Yates is chosen on a 2 × 2 table (each |O − E| reduced by min(½, |O − E|)); Cramér's V = √(χ²/(n(min(r, c) − 1))) from the uncorrected χ².";

    public override string Citation =>
        "Pearson, K., 1900; Montgomery and Runger, 7th ed., §9-8 (contingency table tests; Example 9-14); Yates, F., Supplement to " +
        "the Journal of the Royal Statistical Society 1, 1934, 217–235; Cramér, H., Mathematical Methods of Statistics, 1946, §21.9.";

    public override bool IsApplicableTo(ChiSquareTest subject) => subject is not null && subject.Test == ChiSquareKind.Independence;

    public override IReadOnlyList<string> Inputs =>
        [nameof(ChiSquareTest.CountsTable), nameof(ChiSquareTest.Counts), nameof(ChiSquareTest.RowNames), nameof(ChiSquareTest.Correction)];

    public override IReadOnlyList<string> Outputs => [nameof(ChiSquareTest.Rows), nameof(ChiSquareTest.Columns), nameof(ChiSquareTest.CramersV)];

    public override void Apply(ChiSquareTest subject) => subject.Recalculate();
}
