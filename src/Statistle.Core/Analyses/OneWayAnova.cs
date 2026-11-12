using System.ComponentModel;
using System.Globalization;
using System.Text;
using MathNet.Numerics.Distributions;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum GroupLayout
{
    [Description("Each column is a group")]
    ColumnsAreGroups = 0,

    [Description("A factor column and a response column")]
    FactorAndResponse,
}

/// <summary>
/// EGN3443 Module 14: single-factor analysis of variance — the ANOVA table and F, effect size
/// and power, Tukey's HSD on every pair, the assumption checks, and Kruskal–Wallis beside it.
/// </summary>
[StatistleHelp(
    "One-way ANOVA (Module 14): do k groups share one mean? The total variation of the " +
    "response is split into the part between the group means and the part within the groups; " +
    "F = MS_between/MS_within is large when the means spread more than the scatter inside the " +
    "groups explains. The groups are either columns of a data table (one per alloy, say) or a " +
    "factor column of level codes with a response column. Read the ANOVA table, the p-value " +
    "and decision, η² (the share of variation the factor explains) and the power of the test. " +
    "A significant F says only that some mean differs: Tukey's HSD then compares every pair " +
    "with one simultaneous confidence level. Bartlett and Levene check the equal-variance " +
    "assumption, Shapiro–Wilk on the residuals the normality; Kruskal–Wallis is the rank-based " +
    "test to fall back on when they fail. The Code tab writes scipy's f_oneway and tukey_hsd, " +
    "statsmodels' anova_lm and pairwise_tukeyhsd, and R's aov and TukeyHSD.")]
public sealed class OneWayAnova : Analysis, IHasFigures, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, ICodeSource
{
    private string _table = string.Empty;
    private GroupLayout _layout = GroupLayout.ColumnsAreGroups;
    private string _groups = string.Empty;
    private string _factorColumn = string.Empty;
    private string _responseColumn = string.Empty;
    private double _alpha = 0.05;

    private int _k;
    private int _n;
    private string _groupTable = string.Empty;
    private double _grandMean = double.NaN;
    private double _ssBetween = double.NaN;
    private double _ssWithin = double.NaN;
    private double _ssTotal = double.NaN;
    private double _dfBetween = double.NaN;
    private double _dfWithin = double.NaN;
    private double _msBetween = double.NaN;
    private double _msWithin = double.NaN;
    private double _f = double.NaN;
    private double _fCritical = double.NaN;
    private double _p = double.NaN;
    private string _decision = string.Empty;
    private double _etaSquared = double.NaN;
    private double _omegaSquared = double.NaN;
    private double _cohensF = double.NaN;
    private double _power = double.NaN;
    private double _tukeyQ = double.NaN;
    private string _tukeyTable = string.Empty;
    private double _tukeyMinP = double.NaN;
    private int _tukeySignificant;
    private double _bartlett = double.NaN;
    private double _bartlettP = double.NaN;
    private double _levene = double.NaN;
    private double _leveneP = double.NaN;
    private double _shapiroW = double.NaN;
    private double _shapiroP = double.NaN;
    private double _kruskalH = double.NaN;
    private double _kruskalP = double.NaN;

    public OneWayAnova()
    {
        Name = "One-way ANOVA";
        Recalculate();
    }

    public override string Title => "One-way ANOVA";

    public override string Summary => "k group means: ANOVA table, F, η², power, Tukey HSD, Bartlett, Levene, Kruskal–Wallis (Module 14)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 50;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public OneWayResult? Result { get; private set; }

    [StatistleIgnore]
    public IReadOnlyList<TukeyPair> Pairs { get; private set; } = [];

    /// <summary>The groups as fitted: names and values, for the figures and the code.</summary>
    [StatistleIgnore]
    public IReadOnlyList<string> GroupNames { get; private set; } = [];

    [StatistleIgnore]
    public IReadOnlyList<IReadOnlyList<double>> GroupValues { get; private set; } = [];

    [StatistleIgnore]
    public string ResponseName { get; private set; } = "value";

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Data table", Role = VariableRole.Input,
        Help = "The table holding the groups — as one column per group, or as a factor column " +
               "and a response column, one row per observation.")]
    public string Table
    {
        get => _table;
        set => SetProperty(ref _table, TableId(value));
    }

    [StatistleProperty(Label = "Layout", Role = VariableRole.Design,
        Help = "How the groups sit in the table. Columns: each column is one group (the " +
               "sample problem's three alloys). Factor and response: a column of level codes " +
               "(1, 2, 3 …) and a column of measurements, one row each.")]
    public GroupLayout Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    [StatistleProperty(Label = "Group columns", Role = VariableRole.Input,
        Help = "The columns that are the groups, by name and separated by commas. Leave blank " +
               "to use every column of the table. Groups may differ in size.")]
    public string Groups
    {
        get => _groups;
        set => SetProperty(ref _groups, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Factor column", Role = VariableRole.Input,
        Help = "The column of level codes that says which group each row belongs to. Each " +
               "distinct value is a level.")]
    public string FactorColumn
    {
        get => _factorColumn;
        set => SetProperty(ref _factorColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Response column", Role = VariableRole.Input,
        Help = "The measured response, one value per row, in the same order as the factor column.")]
    public string ResponseColumn
    {
        get => _responseColumn;
        set => SetProperty(ref _responseColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Significance level (α)", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.0001, Max = 0.5,
        Help = "The level of the F test and of Tukey's simultaneous intervals; 0.05 is usual.")]
    public double Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    // ── Results: the table ────────────────────────────────────────────────────

    [StatistleProperty(Label = "Groups (k)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many groups (levels of the factor) were compared.")]
    public int K
    {
        get => _k;
        set => SetProperty(ref _k, value);
    }

    [StatistleProperty(Label = "Observations (N)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "The total number of observations over all the groups.")]
    public int N
    {
        get => _n;
        set => SetProperty(ref _n, value);
    }

    [StatistleProperty(Label = "Group summaries", Role = VariableRole.Output,
        Help = "Each group's size, mean and standard deviation — the first look: are the means " +
               "far apart compared with the spread inside each group?")]
    public string GroupTable
    {
        get => _groupTable;
        set => SetProperty(ref _groupTable, value);
    }

    [StatistleProperty(Label = "Grand mean", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The mean of every observation, ȳ, which the group means are measured from.")]
    public double GrandMean
    {
        get => _grandMean;
        set => SetProperty(ref _grandMean, value);
    }

    [StatistleProperty(Label = "SS between groups", Decimals = 4, Role = VariableRole.Output,
        Help = "SS_Treatments = Σnᵢ(ȳᵢ − ȳ)²: the variation of the group means about the grand mean, weighted by group size.")]
    public double SsBetween
    {
        get => _ssBetween;
        set => SetProperty(ref _ssBetween, value);
    }

    [StatistleProperty(Label = "SS within groups", Decimals = 4, Role = VariableRole.Output,
        Help = "SS_E = ΣΣ(yᵢⱼ − ȳᵢ)²: the scatter of the observations about their own group means — the noise.")]
    public double SsWithin
    {
        get => _ssWithin;
        set => SetProperty(ref _ssWithin, value);
    }

    [StatistleProperty(Label = "SS total", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_T = ΣΣ(yᵢⱼ − ȳ)² = SS between + SS within.")]
    public double SsTotal
    {
        get => _ssTotal;
        set => SetProperty(ref _ssTotal, value);
    }

    [StatistleProperty(Label = "df between", Decimals = 0, Role = VariableRole.Calculated,
        Help = "k − 1 degrees of freedom for the treatments.")]
    public double DfBetween
    {
        get => _dfBetween;
        set => SetProperty(ref _dfBetween, value);
    }

    [StatistleProperty(Label = "df within", Decimals = 0, Role = VariableRole.Calculated,
        Help = "N − k degrees of freedom for the error.")]
    public double DfWithin
    {
        get => _dfWithin;
        set => SetProperty(ref _dfWithin, value);
    }

    [StatistleProperty(Label = "MS between", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS between / (k − 1): the variance of the means, scaled up to the observation level.")]
    public double MsBetween
    {
        get => _msBetween;
        set => SetProperty(ref _msBetween, value);
    }

    [StatistleProperty(Label = "MS within", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS within / (N − k): the pooled estimate of σ², the common variance inside the groups.")]
    public double MsWithin
    {
        get => _msWithin;
        set => SetProperty(ref _msWithin, value);
    }

    [StatistleProperty(Label = "F", Decimals = 4, Role = VariableRole.Output,
        Help = "MS between / MS within, against F(k − 1, N − k). Near 1 when all the means are " +
               "equal; large when they differ. Python stats.f_oneway; R summary(aov()).")]
    public double F
    {
        get => _f;
        set => SetProperty(ref _f, value);
    }

    [StatistleProperty(Label = "F critical", Decimals = 4, Role = VariableRole.Calculated,
        Help = "F_{α; k−1, N−k}: reject H₀ when F exceeds it (the table look-up of the hand method).")]
    public double FCritical
    {
        get => _fCritical;
        set => SetProperty(ref _fCritical, value);
    }

    [StatistleProperty(Label = "p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "P(F ≥ observed) when every mean is equal. Below α: at least one group's mean differs.")]
    public double P
    {
        get => _p;
        set => SetProperty(ref _p, value);
    }

    [StatistleProperty(Label = "Decision", Role = VariableRole.Output,
        Help = "The conclusion of the F test at the chosen α, in words.")]
    public string Decision
    {
        get => _decision;
        set => SetProperty(ref _decision, value);
    }

    [StatistleProperty(Label = "η² (eta squared)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "SS between / SS total: the fraction of the response's variation the factor " +
               "accounts for. About 0.01 is small, 0.06 medium, 0.14 large (Cohen).")]
    public double EtaSquared
    {
        get => _etaSquared;
        set => SetProperty(ref _etaSquared, value);
    }

    [StatistleProperty(Label = "ω² (omega squared)", Decimals = 4, Role = VariableRole.Calculated,
        Help = "(SS between − (k − 1)MS within)/(SS total + MS within): η² corrected for its " +
               "optimism in small samples; the better estimate of the population effect.")]
    public double OmegaSquared
    {
        get => _omegaSquared;
        set => SetProperty(ref _omegaSquared, value);
    }

    [StatistleProperty(Label = "Cohen's f", Decimals = 4, Role = VariableRole.Calculated,
        Help = "√(η²/(1 − η²)): the effect size the power calculation uses. 0.1 small, 0.25 medium, 0.4 large.")]
    public double CohensF
    {
        get => _cohensF;
        set => SetProperty(ref _cohensF, value);
    }

    [StatistleProperty(Label = "Power (observed effect)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability the test detects an effect of the observed size with these " +
               "group sizes at α, from the noncentral F with λ = f²N. Python statsmodels " +
               "FTestAnovaPower; R pwr.anova.test or pf(…, ncp).")]
    public double Power
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    // ── Tukey ─────────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Tukey q critical", Decimals = 4, Role = VariableRole.Calculated,
        Help = "q_{α; k, N−k} from the studentized range distribution: the yardstick for every " +
               "pair, so all the comparisons together hold the level α. Python " +
               "stats.studentized_range.ppf; R qtukey.")]
    public double TukeyQ
    {
        get => _tukeyQ;
        set => SetProperty(ref _tukeyQ, value);
    }

    [StatistleProperty(Label = "Tukey HSD pairs", Role = VariableRole.Output,
        Help = "Every pair: the difference of means (first − second), the simultaneous " +
               "confidence interval, the adjusted p, and * when the interval excludes zero. " +
               "Pairs whose intervals exclude zero differ honestly.")]
    public string TukeyTable
    {
        get => _tukeyTable;
        set => SetProperty(ref _tukeyTable, value);
    }

    [StatistleProperty(Label = "Smallest Tukey p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "The adjusted p of the pair that differs most.")]
    public double TukeyMinP
    {
        get => _tukeyMinP;
        set => SetProperty(ref _tukeyMinP, value);
    }

    [StatistleProperty(Label = "Pairs that differ", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "How many of the k(k − 1)/2 pairs differ at level α by Tukey's test.")]
    public int TukeySignificant
    {
        get => _tukeySignificant;
        set => SetProperty(ref _tukeySignificant, value);
    }

    // ── Checks ────────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Bartlett statistic", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Bartlett's test of equal variances against χ²(k − 1). It assumes normality; " +
               "a small p may mean unequal variances or non-normal data. Python stats.bartlett; R bartlett.test.")]
    public double Bartlett
    {
        get => _bartlett;
        set => SetProperty(ref _bartlett, value);
    }

    [StatistleProperty(Label = "Bartlett p", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: no evidence that the variances differ.")]
    public double BartlettP
    {
        get => _bartlettP;
        set => SetProperty(ref _bartlettP, value);
    }

    [StatistleProperty(Label = "Levene W", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Levene's test of equal variances, median-centered (Brown–Forsythe), robust to " +
               "non-normality; the check the course uses. Python stats.levene; R car::leveneTest.")]
    public double Levene
    {
        get => _levene;
        set => SetProperty(ref _levene, value);
    }

    [StatistleProperty(Label = "Levene p", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: the equal-variance assumption stands.")]
    public double LeveneP
    {
        get => _leveneP;
        set => SetProperty(ref _leveneP, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk W of residuals", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Normality of the residuals yᵢⱼ − ȳᵢ pooled over the groups; read with the Q–Q plot.")]
    public double ShapiroW
    {
        get => _shapiroW;
        set => SetProperty(ref _shapiroW, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk p of residuals", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: no evidence against normal errors.")]
    public double ShapiroP
    {
        get => _shapiroP;
        set => SetProperty(ref _shapiroP, value);
    }

    [StatistleProperty(Label = "Kruskal–Wallis H", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The rank-based test of equal group distributions, corrected for ties, against " +
               "χ²(k − 1): the fall-back when normality or equal variances fail. Python stats.kruskal; R kruskal.test.")]
    public double KruskalH
    {
        get => _kruskalH;
        set => SetProperty(ref _kruskalH, value);
    }

    [StatistleProperty(Label = "Kruskal–Wallis p", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "Below α: the groups' distributions are not all the same.")]
    public double KruskalP
    {
        get => _kruskalP;
        set => SetProperty(ref _kruskalP, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    public override void Recalculate()
    {
        Result = null;
        Pairs = [];
        GroupNames = [];
        GroupValues = [];
        ResponseName = "value";
        K = N = TukeySignificant = 0;
        GroupTable = Decision = TukeyTable = string.Empty;
        GrandMean = SsBetween = SsWithin = SsTotal = DfBetween = DfWithin = MsBetween = MsWithin = F = FCritical = P = double.NaN;
        EtaSquared = OmegaSquared = CohensF = Power = TukeyQ = TukeyMinP = double.NaN;
        Bartlett = BartlettP = Levene = LeveneP = ShapiroW = ShapiroP = KruskalH = KruskalP = double.NaN;
        Problem = Gather(out List<string> names, out List<IReadOnlyList<double>> groups);

        if (Problem is not null)
        {
            return;
        }

        if (double.IsNaN(Alpha) || Alpha <= 0 || Alpha >= 1)
        {
            Problem = "Enter a significance level between 0 and 1.";
            return;
        }

        OneWayResult? r = Anova.OneWay(names, groups);

        if (r is null)
        {
            Problem = groups.Count < 2 ? "At least two groups are needed." : "Every group needs at least two values, and there must be some variation within the groups.";
            return;
        }

        Result = r;
        GroupNames = names;
        GroupValues = groups;
        K = groups.Count;
        N = r.N;
        GroupTable = string.Join("\n", r.Groups.Select(g => $"{g.Name}: n = {g.N}, mean = {g.Mean:G6}, sd = {g.StandardDeviation:G5}"));
        GrandMean = r.GrandMean;
        SsBetween = r.SsBetween;
        SsWithin = r.SsWithin;
        SsTotal = r.SsTotal;
        DfBetween = r.DfBetween;
        DfWithin = r.DfWithin;
        MsBetween = r.MsBetween;
        MsWithin = r.MsWithin;
        F = r.F;
        FCritical = FisherSnedecor.InvCDF(r.DfBetween, r.DfWithin, 1 - Alpha);
        P = r.PValue;
        Decision = r.PValue < Alpha
            ? $"Reject H₀ at α = {Alpha:G3} — at least one group mean differs (p = {r.PValue:G3})"
            : $"Do not reject H₀ at α = {Alpha:G3} — no evidence the means differ (p = {r.PValue:G3})";
        EtaSquared = r.EtaSquared;
        OmegaSquared = r.OmegaSquared;
        CohensF = r.EtaSquared < 1 ? Math.Sqrt(r.EtaSquared / (1 - r.EtaSquared)) : double.PositiveInfinity;
        Power = Anova.Power(K, N, CohensF, Alpha);

        (Pairs, TukeyQ) = Anova.TukeyHsd(r, Alpha);
        TukeyTable = string.Join("\n", Pairs.Select(p =>
            $"{p.A} − {p.B}: diff = {p.Difference:G6}, CI [{p.Lower:G6}, {p.Upper:G6}], p = {p.PValue:F4}{(p.Significant ? " *" : string.Empty)}"));
        TukeyMinP = Pairs.Count > 0 ? Pairs.Min(p => p.PValue) : double.NaN;
        TukeySignificant = Pairs.Count(p => p.Significant);

        AssumptionCheck bartlett = Anova.Bartlett(groups);
        Bartlett = bartlett.Statistic;
        BartlettP = bartlett.PValue;
        AssumptionCheck levene = Nonparametric.Levene(groups);
        Levene = levene.Statistic;
        LeveneP = levene.PValue;
        AssumptionCheck normality = Nonparametric.ShapiroWilk(r.Residuals);
        ShapiroW = normality.Statistic;
        ShapiroP = normality.PValue;
        AssumptionCheck kruskal = Anova.KruskalWallis(groups);
        KruskalH = kruskal.Statistic;
        KruskalP = kruskal.PValue;
    }

    private string? Gather(out List<string> names, out List<IReadOnlyList<double>> groups)
    {
        names = [];
        groups = [];
        DataTable? table = TableChosen();

        if (table is null)
        {
            return DataSources.ChooseOrAdd(this, "Choose the data table.");
        }

        if (Layout == GroupLayout.ColumnsAreGroups)
        {
            List<DataColumn> columns = [];

            if (string.IsNullOrWhiteSpace(Groups))
            {
                columns.AddRange(table.Columns.Columns);
            }
            else
            {
                foreach (string name in Groups.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    DataColumn? column = table.Columns.Column(name);

                    if (column is null)
                    {
                        return $"The table has no column named '{name}'.";
                    }

                    columns.Add(column);
                }
            }

            foreach (DataColumn column in columns)
            {
                names.Add(column.Name);
                groups.Add(column.Values);
            }

            return columns.Count < 2 ? "At least two group columns are needed." : null;
        }

        DataColumn? factor = table.Columns.Column(FactorColumn);
        DataColumn? response = table.Columns.Column(ResponseColumn);

        if (factor is null)
        {
            return "Choose the factor column.";
        }

        if (response is null)
        {
            return "Choose the response column.";
        }

        if (factor.Count != response.Count)
        {
            return "The factor and response columns must have the same number of rows.";
        }

        ResponseName = response.Name;

        foreach (double level in factor.Values.Distinct().Order())
        {
            names.Add($"{factor.Name} = {level:G6}");
            groups.Add([.. response.Values.Where((_, i) => factor.Values[i] == level)]);
        }

        return groups.Count < 2 ? "The factor column has fewer than two distinct levels." : null;
    }

    /// <summary>
    /// The table's id. A column reference ("id␟column", as a lesson's "@Table/" setting is written) is read as
    /// the table it points to, as the Chi-square tests read theirs.
    /// </summary>
    private static string TableId(string? value)
    {
        (int id, _) = DataSources.Split(value);
        return id > 0 ? id.ToString(CultureInfo.InvariantCulture) : value ?? string.Empty;
    }

    private DataTable? TableChosen() =>
        int.TryParse(Table, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? Project?.Tables.FirstOrDefault(t => t.Id == id) : null;

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName)
    {
        if (propertyName == nameof(Table))
        {
            return Project is null ? [] : [.. Project.Tables.Select(t => new ChoiceOption(t.Id.ToString(CultureInfo.InvariantCulture), t.Name))];
        }

        if (propertyName is nameof(FactorColumn) or nameof(ResponseColumn))
        {
            return TableChosen() is DataTable table ? [.. table.Columns.Columns.Select(c => new ChoiceOption(c.Name, c.Name))] : [];
        }

        return [];
    }

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Table) or nameof(Groups) or nameof(FactorColumn) or nameof(ResponseColumn) ? TableChosen() : null;

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Result is null)
        {
            return [];
        }

        double t = StudentT.InvCDF(0, 1, Result.DfWithin, 1 - Alpha / 2);
        double[] lower = [.. Result.Groups.Select(g => g.Mean - t * Math.Sqrt(Result.MsWithin / g.N))];
        double[] upper = [.. Result.Groups.Select(g => g.Mean + t * Math.Sqrt(Result.MsWithin / g.N))];

        return
        [
            AnovaFigures.Boxes($"{Name} — the groups", ResponseName, GroupNames, GroupValues),
            AnovaFigures.Means($"{Name} — group means with {1 - Alpha:P0} intervals (pooled σ)", ResponseName, Result.Groups, lower, upper),
            AnovaFigures.ResidualQq($"{Name} — normal Q–Q plot of residuals", Result.Residuals),
        ];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);

        if (Result is null)
        {
            return string.Empty;
        }

        int k = GroupNames.Count;
        s.Imports(scipyStats: true, pandas: true, statsmodels: true);
        s.Either("from statsmodels.formula.api import ols\nfrom statsmodels.stats.anova import anova_lm\nfrom statsmodels.stats.multicomp import pairwise_tukeyhsd",
                 "# Base R plus the formulas; car::leveneTest and pwr::pwr.anova.test give the same checks and power");
        s.Line();
        s.Comment("The groups");

        for (int i = 0; i < k; i++)
        {
            s.Comment(GroupNames[i]);
            s.Vector($"g{i + 1}", GroupValues[i]);
        }

        string list = string.Join(", ", Enumerable.Range(1, k).Select(i => $"g{i}"));
        string quoted = string.Join(", ", GroupNames.Select(n => "\"" + n.Replace("\"", "\\\"", StringComparison.Ordinal) + "\""));
        s.Either($"groups = [np.asarray(g, dtype=float) for g in [{list}]]", $"groups <- list({list})");
        s.Either($"names = [{quoted}]", $"names <- c({quoted})");
        s.Assign("alpha", Math.Round(Alpha, 10));
        s.Either("k = len(groups); N = sum(len(g) for g in groups)", "k <- length(groups); N <- sum(sapply(groups, length))");
        s.Either("y = np.concatenate(groups)", "y <- unlist(groups)");
        s.Either("labels = np.repeat(names, [len(g) for g in groups])", "g <- factor(rep(names, sapply(groups, length)), levels = names)");
        s.Line();
        s.Comment("The ANOVA table by the formulas, then the library's F as a check");
        s.Either("grand = y.mean()", "grand <- mean(y)");
        s.Either("ss_between = sum(len(g) * (g.mean() - grand) ** 2 for g in groups)", "ss_between <- sum(sapply(groups, function(v) length(v) * (mean(v) - grand)^2))");
        s.Either("ss_within = sum(((g - g.mean()) ** 2).sum() for g in groups)", "ss_within <- sum(sapply(groups, function(v) sum((v - mean(v))^2)))");
        s.Either("ss_total = ((y - grand) ** 2).sum()", "ss_total <- sum((y - grand)^2)");
        s.Either("df_between, df_within = k - 1, N - k", "df_between <- k - 1; df_within <- N - k");
        s.Either("ms_between, ms_within = ss_between / df_between, ss_within / df_within", "ms_between <- ss_between / df_between; ms_within <- ss_within / df_within");
        s.Either("f_stat, p_value = stats.f_oneway(*groups)", "fit <- aov(y ~ g); an <- anova(fit); f_stat <- an[\"g\", \"F value\"]; p_value <- an[\"g\", \"Pr(>F)\"]");
        s.Either("f_crit = stats.f.ppf(1 - alpha, df_between, df_within)", "f_crit <- qf(1 - alpha, df_between, df_within)");
        s.Either("eta = ss_between / ss_total", "eta <- ss_between / ss_total");
        s.Either("omega = max(0, (ss_between - df_between * ms_within) / (ss_total + ms_within))", "omega <- max(0, (ss_between - df_between * ms_within) / (ss_total + ms_within))");
        s.Either("cohen_f = np.sqrt(eta / (1 - eta))", "cohen_f <- sqrt(eta / (1 - eta))");
        s.Either("power = stats.ncf.sf(f_crit, df_between, df_within, cohen_f ** 2 * N)", "power <- 1 - pf(f_crit, df_between, df_within, ncp = cohen_f^2 * N)");
        s.Line();
        s.PrintRaw("Groups (k)", "k", "k");
        s.PrintRaw("Observations (N)", "N", "N");
        s.Either("print(\"Group summaries:\")", "cat(\"Group summaries:\\n\")");
        s.Either("for name, g in zip(names, groups):\n    print(f\"  {name}: n = {len(g)}, mean = {g.mean():.4f}, sd = {g.std(ddof=1):.4f}\")",
                 "for (i in seq_along(groups)) cat(sprintf(\"  %s: n = %d, mean = %.4f, sd = %.4f\\n\", names[i], length(groups[[i]]), mean(groups[[i]]), sd(groups[[i]])))");
        s.Print("Grand mean", "grand", "grand", 4);
        s.Print("SS between groups", "ss_between", "ss_between", 4);
        s.Print("SS within groups", "ss_within", "ss_within", 4);
        s.Print("SS total", "ss_total", "ss_total", 4);
        s.PrintRaw("df between", "df_between", "df_between");
        s.PrintRaw("df within", "df_within", "df_within");
        s.Print("MS between", "ms_between", "ms_between", 4);
        s.Print("MS within", "ms_within", "ms_within", 4);
        s.Print("F", "f_stat", "f_stat", 4);
        s.Print("F critical", "f_crit", "f_crit", 4);
        s.Print("p-value", "p_value", "p_value", 4);
        s.PrintText("Decision", Decision);
        s.Print("η² (eta squared)", "eta", "eta", 4);
        s.Print("ω² (omega squared)", "omega", "omega", 4);
        s.Print("Cohen's f", "cohen_f", "cohen_f", 4);
        s.Print("Power (observed effect)", "power", "power", 4);
        s.Line();
        s.Comment("Tukey's HSD on every pair: q = |ȳᵢ − ȳⱼ| / √(MS_within/2 · (1/nᵢ + 1/nⱼ)) against the studentized range q(k, N − k)");
        s.Either("q_crit = stats.studentized_range.ppf(1 - alpha, k, df_within)", "q_crit <- qtukey(1 - alpha, k, df_within)");
        s.Either("tk = stats.tukey_hsd(*groups); ci = tk.confidence_interval(1 - alpha)", "means <- sapply(groups, mean); sizes <- sapply(groups, length); p_min <- 1; n_sig <- 0");
        s.Print("Tukey q critical", "q_crit", "q_crit", 4);
        s.Either("print(\"Tukey HSD pairs:\")", "cat(\"Tukey HSD pairs:\\n\")");
        s.Either("for i in range(k):\n    for j in range(i + 1, k):\n        star = \" *\" if tk.pvalue[i, j] < alpha else \"\"\n        print(f\"  {names[i]} - {names[j]}: diff = {tk.statistic[i, j]:.4f}, CI [{ci.low[i, j]:.4f}, {ci.high[i, j]:.4f}], p = {tk.pvalue[i, j]:.4f}{star}\")",
                 "for (i in 1:(k - 1)) for (j in (i + 1):k) {\n  d <- means[i] - means[j]; se <- sqrt(ms_within / 2 * (1 / sizes[i] + 1 / sizes[j]))\n  p <- ptukey(abs(d) / se, k, df_within, lower.tail = FALSE); p_min <- min(p_min, p); n_sig <- n_sig + (p < alpha)\n  cat(sprintf(\"  %s - %s: diff = %.4f, CI [%.4f, %.4f], p = %.4f%s\\n\", names[i], names[j], d, d - q_crit * se, d + q_crit * se, p, ifelse(p < alpha, \" *\", \"\")))\n}");
        s.Print("Smallest Tukey p-value", "min(tk.pvalue[i, j] for i in range(k) for j in range(i + 1, k))", "p_min", 4);
        s.PrintRaw("Pairs that differ", "sum(tk.pvalue[i, j] < alpha for i in range(k) for j in range(i + 1, k))", "n_sig");
        s.Line();
        s.Comment("Assumption checks and the rank-based alternative");
        s.Either("bart, p_bart = stats.bartlett(*groups)", "bt <- bartlett.test(groups); bart <- bt$statistic; p_bart <- bt$p.value");
        s.Either("lev, p_lev = stats.levene(*groups)   # center='median' is the default", "# Levene, median-centered, by hand (car::leveneTest(y ~ g) gives the same)");
        s.Either(string.Empty, "z <- lapply(groups, function(v) abs(v - median(v))); zall <- unlist(z)");
        s.Either(string.Empty, "between <- sum(sapply(z, function(v) length(v) * (mean(v) - mean(zall))^2)); within <- sum(sapply(z, function(v) sum((v - mean(v))^2)))");
        s.Either(string.Empty, "lev <- (N - k) / (k - 1) * between / within; p_lev <- 1 - pf(lev, k - 1, N - k)");
        s.Either("w, p_w = stats.shapiro(np.concatenate([g - g.mean() for g in groups]))", "sw <- shapiro.test(residuals(fit)); w <- sw$statistic; p_w <- sw$p.value");
        s.Either("h, p_h = stats.kruskal(*groups)", "kw <- kruskal.test(groups); h <- kw$statistic; p_h <- kw$p.value");
        s.Print("Bartlett statistic", "bart", "bart", 4);
        s.Print("Bartlett p", "p_bart", "p_bart", 4);
        s.Print("Levene W", "lev", "lev", 4);
        s.Print("Levene p", "p_lev", "p_lev", 4);
        s.Print("Shapiro–Wilk W of residuals", "w", "w", 4);
        s.Print("Shapiro–Wilk p of residuals", "p_w", "p_w", 4);
        s.Print("Kruskal–Wallis H", "h", "h", 4);
        s.Print("Kruskal–Wallis p", "p_h", "p_h", 4);
        s.Line();
        s.Comment("The course's library calls, for comparison");
        s.Either("data = pd.DataFrame({'y': y, 'g': labels})", "print(summary(fit))");
        s.Either("print(anova_lm(ols('y ~ C(g)', data=data).fit(), typ=2))", "print(TukeyHSD(fit, conf.level = 1 - alpha))");
        s.Either("print(pairwise_tukeyhsd(y, labels, alpha=alpha))", string.Empty);
        return s.ToString();
    }
}

/// <summary>The groups are columns of the table.</summary>
public sealed class GroupsFromColumns : CalculationMethod<OneWayAnova>
{
    public override string Name => "Groups as columns";

    public override string Description => "Each chosen column is one group; the columns may differ in length.";

    public override string Citation => "Statistics+ project data.";

    public override bool IsApplicableTo(OneWayAnova subject) => subject is not null && subject.Layout == GroupLayout.ColumnsAreGroups;

    public override IReadOnlyList<string> Inputs => [nameof(OneWayAnova.Table), nameof(OneWayAnova.Groups)];

    public override IReadOnlyList<string> Outputs => [];

    public override void Apply(OneWayAnova subject) => subject.Recalculate();
}

/// <summary>The groups are the levels of a factor column, with the response beside it.</summary>
public sealed class GroupsFromFactor : CalculationMethod<OneWayAnova>
{
    public override string Name => "Groups from a factor column";

    public override string Description => "Each distinct value of the factor column is a level; the response column's rows are split by it.";

    public override string Citation => "Statistics+ project data.";

    public override bool IsApplicableTo(OneWayAnova subject) => subject is not null && subject.Layout == GroupLayout.FactorAndResponse;

    public override IReadOnlyList<string> Inputs => [nameof(OneWayAnova.Table), nameof(OneWayAnova.FactorColumn), nameof(OneWayAnova.ResponseColumn)];

    public override IReadOnlyList<string> Outputs => [];

    public override void Apply(OneWayAnova subject) => subject.Recalculate();
}

/// <summary>The single-factor F test with its effect sizes and power.</summary>
public sealed class SingleFactorFTest : CalculationMethod<OneWayAnova>
{
    public override string Name => "Single-factor ANOVA (F test)";

    public override string Description =>
        "SS_T = SS_Treatments + SS_E; F = MS_Treatments/MS_E on k − 1 and N − k degrees of freedom; η² = SS_Treatments/SS_T; " +
        "ω² = (SS_Treatments − (k − 1)MS_E)/(SS_T + MS_E); power from the noncentral F with λ = f²N.";

    public override string Citation =>
        "EGN3443 Module 14; Montgomery, D. C. and Runger, G. C., Applied Statistics and Probability for Engineers, 7th ed., " +
        "2018, §13-2 (the completely randomized single-factor experiment); Cohen, J., Statistical Power Analysis for the " +
        "Behavioral Sciences, 2nd ed., 1988, Ch. 8 (f, and the power of the F test).";

    public override IReadOnlyList<string> Inputs => [nameof(OneWayAnova.Alpha)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(OneWayAnova.K), nameof(OneWayAnova.N), nameof(OneWayAnova.GroupTable), nameof(OneWayAnova.GrandMean),
        nameof(OneWayAnova.SsBetween), nameof(OneWayAnova.SsWithin), nameof(OneWayAnova.SsTotal),
        nameof(OneWayAnova.DfBetween), nameof(OneWayAnova.DfWithin), nameof(OneWayAnova.MsBetween), nameof(OneWayAnova.MsWithin),
        nameof(OneWayAnova.F), nameof(OneWayAnova.FCritical), nameof(OneWayAnova.P), nameof(OneWayAnova.Decision),
        nameof(OneWayAnova.EtaSquared), nameof(OneWayAnova.OmegaSquared), nameof(OneWayAnova.CohensF), nameof(OneWayAnova.Power),
    ];

    public override void Apply(OneWayAnova subject) => subject.Recalculate();
}

/// <summary>Tukey's honestly significant difference on every pair.</summary>
public sealed class TukeyHonestDifference : CalculationMethod<OneWayAnova>
{
    public override string Name => "Tukey's HSD";

    public override string Description =>
        "For each pair, |ȳᵢ − ȳⱼ| / √(MS_E/2 · (1/nᵢ + 1/nⱼ)) against the studentized range q(k, N − k); the simultaneous " +
        "interval ȳᵢ − ȳⱼ ± q_{α;k,N−k}·√(MS_E/2 · (1/nᵢ + 1/nⱼ)) (Tukey–Kramer when sizes differ). The distribution " +
        "function is Copenhaver and Holland's algorithm, as R's ptukey.";

    public override string Citation =>
        "Montgomery and Runger, 7th ed., §13-2.3 (Tukey's test); Copenhaver, M. D. and Holland, B. S., Journal of " +
        "Statistical Computation and Simulation 30, 1988, 1–15 (ptukey, AS 190).";

    public override IReadOnlyList<string> Inputs => [nameof(OneWayAnova.Alpha)];

    public override IReadOnlyList<string> Outputs =>
        [nameof(OneWayAnova.TukeyQ), nameof(OneWayAnova.TukeyTable), nameof(OneWayAnova.TukeyMinP), nameof(OneWayAnova.TukeySignificant)];

    public override void Apply(OneWayAnova subject) => subject.Recalculate();
}

/// <summary>The checks of the ANOVA assumptions.</summary>
public sealed class AnovaAssumptionChecks : CalculationMethod<OneWayAnova>
{
    public override string Name => "Assumption checks";

    public override string Description =>
        "Bartlett's χ² and Levene's median-centered F for equal variances; Shapiro–Wilk on the pooled residuals for normality.";

    public override string Citation =>
        "Bartlett, M. S., Proc. Royal Society A 160, 1937; Levene, H., 1960, with Brown and Forsythe's median centring " +
        "(1974); Shapiro and Wilk, Biometrika 52, 1965, by Royston's AS R94 (1995). Montgomery and Runger §13-2.4 " +
        "(residual analysis and model checking).";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs =>
        [nameof(OneWayAnova.Bartlett), nameof(OneWayAnova.BartlettP), nameof(OneWayAnova.Levene), nameof(OneWayAnova.LeveneP), nameof(OneWayAnova.ShapiroW), nameof(OneWayAnova.ShapiroP)];

    public override void Apply(OneWayAnova subject) => subject.Recalculate();
}

/// <summary>The rank-based alternative to the F test.</summary>
public sealed class KruskalWallisTest : CalculationMethod<OneWayAnova>
{
    public override string Name => "Kruskal–Wallis";

    public override string Description =>
        "H = 12/(N(N + 1)) · ΣRᵢ²/nᵢ − 3(N + 1) on the pooled mid-ranks, divided by 1 − Σ(t³ − t)/(N³ − N) for ties, against χ²(k − 1).";

    public override string Citation =>
        "Kruskal, W. H. and Wallis, W. A., Journal of the American Statistical Association 47, 1952; Montgomery and " +
        "Runger, 7th ed., §13-2.5 (the Kruskal–Wallis test).";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(OneWayAnova.KruskalH), nameof(OneWayAnova.KruskalP)];

    public override void Apply(OneWayAnova subject) => subject.Recalculate();
}
