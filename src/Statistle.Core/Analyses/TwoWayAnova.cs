using System.Globalization;
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

/// <summary>
/// EGN3443 Module 14: two-factor analysis of variance for a balanced design — the main effects
/// of A and B, their interaction, and the interaction plot that shows what the table means.
/// </summary>
[StatistleHelp(
    "Two-way ANOVA (Module 14): the response measured at every combination of two factors — " +
    "material and temperature, say — with the same number of replicates in each cell. The " +
    "variation splits four ways: factor A, factor B, their interaction (does the effect of A " +
    "depend on the level of B?), and error, each tested by F against the error mean square. " +
    "Read the interaction first: when it is significant, the main effects cannot be judged " +
    "separately, and the interaction plot — one line per level of B across the levels of A — " +
    "tells the story; parallel lines mean no interaction. The table is three columns: level " +
    "codes for A, level codes for B, and the response, one row per observation. With one " +
    "observation per cell the interaction cannot be separated from error, and the additive " +
    "model is fitted. The Code tab writes statsmodels' ols and anova_lm and R's aov.")]
public sealed class TwoWayAnova : Analysis, IHasFigures, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, ICodeSource
{
    private string _table = string.Empty;
    private string _factorA = string.Empty;
    private string _factorB = string.Empty;
    private string _response = string.Empty;
    private double _alpha = 0.05;

    private int _levelsA;
    private int _levelsB;
    private int _replicates;
    private int _n;
    private string _cellTable = string.Empty;
    private double _grandMean = double.NaN;
    private double _ssA = double.NaN;
    private double _ssB = double.NaN;
    private double _ssAB = double.NaN;
    private double _ssError = double.NaN;
    private double _ssTotal = double.NaN;
    private double _dfA = double.NaN;
    private double _dfB = double.NaN;
    private double _dfAB = double.NaN;
    private double _dfError = double.NaN;
    private double _msA = double.NaN;
    private double _msB = double.NaN;
    private double _msAB = double.NaN;
    private double _msError = double.NaN;
    private double _fA = double.NaN;
    private double _fB = double.NaN;
    private double _fAB = double.NaN;
    private double _pA = double.NaN;
    private double _pB = double.NaN;
    private double _pAB = double.NaN;
    private string _decisionA = string.Empty;
    private string _decisionB = string.Empty;
    private string _decisionAB = string.Empty;
    private double _shapiroW = double.NaN;
    private double _shapiroP = double.NaN;

    public TwoWayAnova()
    {
        Name = "Two-way ANOVA";
        Recalculate();
    }

    public override string Title => "Two-way ANOVA";

    public override string Summary => "Two factors with replication: main effects, interaction, interaction plot (Module 14)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 51;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public TwoWayResult? Result { get; private set; }

    [StatistleIgnore]
    public (DataColumn A, DataColumn B, DataColumn Y)? Columns { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Data table", Role = VariableRole.Input,
        Help = "The table with the two factor columns and the response, one row per observation.")]
    public string Table
    {
        get => _table;
        set => SetProperty(ref _table, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Factor A column", Role = VariableRole.Input,
        Help = "Level codes of the first factor (1, 2, 3 …). Each distinct value is a level; " +
               "it is the factor along the bottom of the interaction plot.")]
    public string FactorA
    {
        get => _factorA;
        set => SetProperty(ref _factorA, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Factor B column", Role = VariableRole.Input,
        Help = "Level codes of the second factor; one line per level on the interaction plot.")]
    public string FactorB
    {
        get => _factorB;
        set => SetProperty(ref _factorB, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Response column", Role = VariableRole.Input,
        Help = "The measured response for each row.")]
    public string Response
    {
        get => _response;
        set => SetProperty(ref _response, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Significance level (α)", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.0001, Max = 0.5,
        Help = "The level of the three F tests; 0.05 is usual.")]
    public double Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Levels of A (a)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many distinct levels factor A has.")]
    public int LevelsA
    {
        get => _levelsA;
        set => SetProperty(ref _levelsA, value);
    }

    [StatistleProperty(Label = "Levels of B (b)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many distinct levels factor B has.")]
    public int LevelsB
    {
        get => _levelsB;
        set => SetProperty(ref _levelsB, value);
    }

    [StatistleProperty(Label = "Replicates per cell (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "Observations at each combination of levels; the design must be balanced (the same n everywhere).")]
    public int Replicates
    {
        get => _replicates;
        set => SetProperty(ref _replicates, value);
    }

    [StatistleProperty(Label = "Observations (N)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "abn, the total number of observations.")]
    public int N
    {
        get => _n;
        set => SetProperty(ref _n, value);
    }

    [StatistleProperty(Label = "Cell means", Role = VariableRole.Output,
        Help = "The mean response at each combination of levels, and the marginal means of each " +
               "level of A and of B — the numbers the interaction plot draws.")]
    public string CellTable
    {
        get => _cellTable;
        set => SetProperty(ref _cellTable, value);
    }

    [StatistleProperty(Label = "Grand mean", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The mean of every observation.")]
    public double GrandMean
    {
        get => _grandMean;
        set => SetProperty(ref _grandMean, value);
    }

    [StatistleProperty(Label = "SS factor A", Decimals = 4, Role = VariableRole.Output,
        Help = "SS_A = bnΣ(ȳᵢ.. − ȳ)²: the variation between the levels of A.")]
    public double SsA
    {
        get => _ssA;
        set => SetProperty(ref _ssA, value);
    }

    [StatistleProperty(Label = "SS factor B", Decimals = 4, Role = VariableRole.Output,
        Help = "SS_B = anΣ(ȳ.ⱼ. − ȳ)²: the variation between the levels of B.")]
    public double SsB
    {
        get => _ssB;
        set => SetProperty(ref _ssB, value);
    }

    [StatistleProperty(Label = "SS interaction", Decimals = 4, Role = VariableRole.Output,
        Help = "SS_AB = nΣΣ(ȳᵢⱼ. − ȳᵢ.. − ȳ.ⱼ. + ȳ)²: what the cell means do beyond adding the two main effects.")]
    public double SsAB
    {
        get => _ssAB;
        set => SetProperty(ref _ssAB, value);
    }

    [StatistleProperty(Label = "SS error", Decimals = 4, Role = VariableRole.Output,
        Help = "SS_E = ΣΣΣ(yᵢⱼₖ − ȳᵢⱼ.)²: the scatter of the replicates within each cell.")]
    public double SsError
    {
        get => _ssError;
        set => SetProperty(ref _ssError, value);
    }

    [StatistleProperty(Label = "SS total", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_T = SS_A + SS_B + SS_AB + SS_E.")]
    public double SsTotal
    {
        get => _ssTotal;
        set => SetProperty(ref _ssTotal, value);
    }

    [StatistleProperty(Label = "df A", Decimals = 0, Role = VariableRole.Calculated,
        Help = "a − 1 degrees of freedom for factor A.")]
    public double DfA
    {
        get => _dfA;
        set => SetProperty(ref _dfA, value);
    }

    [StatistleProperty(Label = "df B", Decimals = 0, Role = VariableRole.Calculated,
        Help = "b − 1 degrees of freedom for factor B.")]
    public double DfB
    {
        get => _dfB;
        set => SetProperty(ref _dfB, value);
    }

    [StatistleProperty(Label = "df interaction", Decimals = 0, Role = VariableRole.Calculated,
        Help = "(a − 1)(b − 1) degrees of freedom for the interaction.")]
    public double DfAB
    {
        get => _dfAB;
        set => SetProperty(ref _dfAB, value);
    }

    [StatistleProperty(Label = "df error", Decimals = 0, Role = VariableRole.Calculated,
        Help = "ab(n − 1) degrees of freedom for error; (a − 1)(b − 1) in the additive model.")]
    public double DfError
    {
        get => _dfError;
        set => SetProperty(ref _dfError, value);
    }

    [StatistleProperty(Label = "MS A", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_A/(a − 1): the variance among the level means of A, scaled to the observation level.")]
    public double MsA
    {
        get => _msA;
        set => SetProperty(ref _msA, value);
    }

    [StatistleProperty(Label = "MS B", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_B/(b − 1): the variance among the level means of B, scaled to the observation level.")]
    public double MsB
    {
        get => _msB;
        set => SetProperty(ref _msB, value);
    }

    [StatistleProperty(Label = "MS interaction", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_AB/((a − 1)(b − 1)): the interaction variation per degree of freedom.")]
    public double MsAB
    {
        get => _msAB;
        set => SetProperty(ref _msAB, value);
    }

    [StatistleProperty(Label = "MS error", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_E/(ab(n − 1)): the estimate of σ² every F is measured against.")]
    public double MsError
    {
        get => _msError;
        set => SetProperty(ref _msError, value);
    }

    [StatistleProperty(Label = "F for A", Decimals = 4, Role = VariableRole.Output,
        Help = "MS_A/MS_E on a − 1 and the error degrees of freedom.")]
    public double FA
    {
        get => _fA;
        set => SetProperty(ref _fA, value);
    }

    [StatistleProperty(Label = "F for B", Decimals = 4, Role = VariableRole.Output,
        Help = "MS_B/MS_E on b − 1 and the error degrees of freedom.")]
    public double FB
    {
        get => _fB;
        set => SetProperty(ref _fB, value);
    }

    [StatistleProperty(Label = "F for interaction", Decimals = 4, Role = VariableRole.Output,
        Help = "MS_AB/MS_E on (a − 1)(b − 1) and the error degrees of freedom. Read this one first.")]
    public double FAB
    {
        get => _fAB;
        set => SetProperty(ref _fAB, value);
    }

    [StatistleProperty(Label = "p-value for A", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "Below α: the levels of A have different mean responses.")]
    public double PA
    {
        get => _pA;
        set => SetProperty(ref _pA, value);
    }

    [StatistleProperty(Label = "p-value for B", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "Below α: the levels of B have different mean responses.")]
    public double PB
    {
        get => _pB;
        set => SetProperty(ref _pB, value);
    }

    [StatistleProperty(Label = "p-value for interaction", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "Below α: the effect of one factor depends on the level of the other; judge the " +
               "factors together, from the interaction plot, not separately.")]
    public double PAB
    {
        get => _pAB;
        set => SetProperty(ref _pAB, value);
    }

    [StatistleProperty(Label = "Decision on A", Role = VariableRole.Output,
        Help = "The conclusion of the F test on factor A at the chosen α.")]
    public string DecisionA
    {
        get => _decisionA;
        set => SetProperty(ref _decisionA, value);
    }

    [StatistleProperty(Label = "Decision on B", Role = VariableRole.Output,
        Help = "The conclusion of the F test on factor B at the chosen α.")]
    public string DecisionB
    {
        get => _decisionB;
        set => SetProperty(ref _decisionB, value);
    }

    [StatistleProperty(Label = "Decision on the interaction", Role = VariableRole.Output,
        Help = "The conclusion of the F test on the interaction, or a note that it could not be tested.")]
    public string DecisionAB
    {
        get => _decisionAB;
        set => SetProperty(ref _decisionAB, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk W of residuals", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Normality of the residuals yᵢⱼₖ − ȳᵢⱼ.; read with the Q–Q plot.")]
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

    // ── Calculation ───────────────────────────────────────────────────────────

    public override void Recalculate()
    {
        Result = null;
        Columns = null;
        LevelsA = LevelsB = Replicates = N = 0;
        CellTable = DecisionA = DecisionB = DecisionAB = string.Empty;
        GrandMean = SsA = SsB = SsAB = SsError = SsTotal = DfA = DfB = DfAB = DfError = double.NaN;
        MsA = MsB = MsAB = MsError = FA = FB = FAB = PA = PB = PAB = ShapiroW = ShapiroP = double.NaN;

        DataTable? table = TableChosen();

        if (table is null)
        {
            Problem = "Choose the data table.";
            return;
        }

        DataColumn? a = table.Columns.Column(FactorA), b = table.Columns.Column(FactorB), y = table.Columns.Column(Response);

        if (a is null || b is null || y is null)
        {
            Problem = a is null ? "Choose the column of factor A." : b is null ? "Choose the column of factor B." : "Choose the response column.";
            return;
        }

        if (ReferenceEquals(a, b) || ReferenceEquals(a, y) || ReferenceEquals(b, y))
        {
            Problem = "The two factors and the response must be three different columns.";
            return;
        }

        if (double.IsNaN(Alpha) || Alpha <= 0 || Alpha >= 1)
        {
            Problem = "Enter a significance level between 0 and 1.";
            return;
        }

        (TwoWayResult? r, Problem) = Anova.TwoWay(a.Values, b.Values, y.Values);

        if (r is null)
        {
            return;
        }

        Result = r;
        Columns = (a, b, y);
        LevelsA = r.LevelsA.Count;
        LevelsB = r.LevelsB.Count;
        Replicates = r.Replicates;
        N = r.N;
        List<string> lines = [];

        for (int i = 0; i < LevelsA; i++)
        {
            lines.Add($"{a.Name} = {r.LevelsA[i]:G6}: " + string.Join(", ", Enumerable.Range(0, LevelsB).Select(j => $"{b.Name} = {r.LevelsB[j]:G6} → {r.CellMeans[i, j]:G6}")) + $"; mean {r.MeansA[i]:G6}");
        }

        lines.Add($"{b.Name} means: " + string.Join(", ", Enumerable.Range(0, LevelsB).Select(j => $"{b.Name} = {r.LevelsB[j]:G6} → {r.MeansB[j]:G6}")));
        CellTable = string.Join("\n", lines);
        GrandMean = r.GrandMean;
        SsA = r.SsA;
        SsB = r.SsB;
        SsAB = r.SsAB;
        SsError = r.SsError;
        SsTotal = r.SsTotal;
        DfA = r.DfA;
        DfB = r.DfB;
        DfAB = r.Additive ? double.NaN : r.DfAB;
        DfError = r.DfError;
        MsA = r.MsA;
        MsB = r.MsB;
        MsAB = r.MsAB;
        MsError = r.MsError;
        FA = r.FA;
        FB = r.FB;
        FAB = r.FAB;
        PA = r.PA;
        PB = r.PB;
        PAB = r.PAB;
        DecisionA = Decide(a.Name, r.PA);
        DecisionB = Decide(b.Name, r.PB);
        DecisionAB = r.Additive
            ? "Not testable with one observation per cell — the additive model was fitted"
            : r.PAB < Alpha
                ? $"Reject H₀ at α = {Alpha:G3} — the factors interact (p = {r.PAB:G3}); judge them together"
                : $"Do not reject H₀ at α = {Alpha:G3} — no evidence of interaction (p = {r.PAB:G3})";
        AssumptionCheck normality = Nonparametric.ShapiroWilk(r.Residuals);
        ShapiroW = normality.Statistic;
        ShapiroP = normality.PValue;
    }

    private string Decide(string factor, double p) => p < Alpha
        ? $"Reject H₀ at α = {Alpha:G3} — the levels of {factor} differ (p = {p:G3})"
        : $"Do not reject H₀ at α = {Alpha:G3} — no evidence the levels of {factor} differ (p = {p:G3})";

    private DataTable? TableChosen() =>
        int.TryParse(Table, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? Project?.Tables.FirstOrDefault(t => t.Id == id) : null;

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName)
    {
        if (propertyName == nameof(Table))
        {
            return Project is null ? [] : [.. Project.Tables.Select(t => new ChoiceOption(t.Id.ToString(CultureInfo.InvariantCulture), t.Name))];
        }

        if (propertyName is nameof(FactorA) or nameof(FactorB) or nameof(Response))
        {
            return TableChosen() is DataTable table ? [.. table.Columns.Columns.Select(c => new ChoiceOption(c.Name, c.Name))] : [];
        }

        return [];
    }

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Table) or nameof(FactorA) or nameof(FactorB) or nameof(Response) ? TableChosen() : null;

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Result is null || Columns is null)
        {
            return [];
        }

        (DataColumn a, DataColumn b, DataColumn y) = Columns.Value;

        return
        [
            AnovaFigures.Interaction($"{Name} — interaction plot", a.Name, b.Name, y.Name, Result),
            AnovaFigures.ResidualQq($"{Name} — normal Q–Q plot of residuals", Result.Residuals),
        ];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);

        if (Result is null || Columns is null)
        {
            return string.Empty;
        }

        (DataColumn a, DataColumn b, DataColumn y) = Columns.Value;
        bool additive = Result.Additive;
        s.Imports(scipyStats: true, pandas: true, statsmodels: true);
        s.Either("from statsmodels.formula.api import ols\nfrom statsmodels.stats.anova import anova_lm", "# Base R: aov / anova on factors");
        s.Line();
        s.Comment($"One row per observation: level of {a.Name}, level of {b.Name}, {y.Name}");
        s.Vector("a", a.Values).Vector("b", b.Values).Vector("y", y.Values);
        s.Assign("alpha", Math.Round(Alpha, 10));
        s.Either("data = pd.DataFrame({'a': a, 'b': b, 'y': y})", "data <- data.frame(a = factor(a), b = factor(b), y = y)");

        if (additive)
        {
            s.Comment("One observation per cell: the additive model, the interaction being the error");
            s.Either("fit = ols('y ~ C(a) + C(b)', data=data).fit()", "fit <- lm(y ~ a + b, data = data)");
        }
        else
        {
            s.Either("fit = ols('y ~ C(a) * C(b)', data=data).fit()", "fit <- lm(y ~ a * b, data = data)");
        }

        s.Either("tab = anova_lm(fit, typ=2)   # balanced, so type I, II and III agree", "tab <- anova(fit)");
        s.Either("levels_a, levels_b = np.unique(a), np.unique(b)", "levels_a <- sort(unique(a)); levels_b <- sort(unique(b))");
        s.Either("n_rep = len(y) // (len(levels_a) * len(levels_b))", "n_rep <- length(y) %/% (length(levels_a) * length(levels_b))");
        s.Line();
        s.PrintRaw("Levels of A (a)", "len(levels_a)", "length(levels_a)");
        s.PrintRaw("Levels of B (b)", "len(levels_b)", "length(levels_b)");
        s.PrintRaw("Replicates per cell (n)", "n_rep", "n_rep");
        s.PrintRaw("Observations (N)", "len(y)", "length(y)");
        s.Either("print(\"Cell means:\")", "cat(\"Cell means:\\n\")");
        s.Either("print(data.pivot_table(index='a', columns='b', values='y', aggfunc='mean', margins=True))", "print(tapply(y, list(a = a, b = b), mean))");
        s.Print("Grand mean", "y.mean()", "mean(y)", 4);
        s.Print("SS factor A", "tab.loc['C(a)', 'sum_sq']", "tab[\"a\", \"Sum Sq\"]", 4);
        s.Print("SS factor B", "tab.loc['C(b)', 'sum_sq']", "tab[\"b\", \"Sum Sq\"]", 4);

        if (!additive)
        {
            s.Print("SS interaction", "tab.loc['C(a):C(b)', 'sum_sq']", "tab[\"a:b\", \"Sum Sq\"]", 4);
        }

        s.Print("SS error", "tab.loc['Residual', 'sum_sq']", "tab[\"Residuals\", \"Sum Sq\"]", 4);
        s.Print("SS total", "tab['sum_sq'].sum()", "sum(tab[, \"Sum Sq\"])", 4);
        s.PrintRaw("df A", "int(tab.loc['C(a)', 'df'])", "tab[\"a\", \"Df\"]");
        s.PrintRaw("df B", "int(tab.loc['C(b)', 'df'])", "tab[\"b\", \"Df\"]");

        if (!additive)
        {
            s.PrintRaw("df interaction", "int(tab.loc['C(a):C(b)', 'df'])", "tab[\"a:b\", \"Df\"]");
        }

        s.PrintRaw("df error", "int(tab.loc['Residual', 'df'])", "tab[\"Residuals\", \"Df\"]");
        s.Print("MS A", "tab.loc['C(a)', 'sum_sq'] / tab.loc['C(a)', 'df']", "tab[\"a\", \"Mean Sq\"]", 4);
        s.Print("MS B", "tab.loc['C(b)', 'sum_sq'] / tab.loc['C(b)', 'df']", "tab[\"b\", \"Mean Sq\"]", 4);

        if (!additive)
        {
            s.Print("MS interaction", "tab.loc['C(a):C(b)', 'sum_sq'] / tab.loc['C(a):C(b)', 'df']", "tab[\"a:b\", \"Mean Sq\"]", 4);
        }

        s.Print("MS error", "tab.loc['Residual', 'sum_sq'] / tab.loc['Residual', 'df']", "tab[\"Residuals\", \"Mean Sq\"]", 4);
        s.Print("F for A", "tab.loc['C(a)', 'F']", "tab[\"a\", \"F value\"]", 4);
        s.Print("F for B", "tab.loc['C(b)', 'F']", "tab[\"b\", \"F value\"]", 4);

        if (!additive)
        {
            s.Print("F for interaction", "tab.loc['C(a):C(b)', 'F']", "tab[\"a:b\", \"F value\"]", 4);
        }

        s.Print("p-value for A", "tab.loc['C(a)', 'PR(>F)']", "tab[\"a\", \"Pr(>F)\"]", 4);
        s.Print("p-value for B", "tab.loc['C(b)', 'PR(>F)']", "tab[\"b\", \"Pr(>F)\"]", 4);

        if (!additive)
        {
            s.Print("p-value for interaction", "tab.loc['C(a):C(b)', 'PR(>F)']", "tab[\"a:b\", \"Pr(>F)\"]", 4);
        }

        s.PrintText("Decision on A", DecisionA);
        s.PrintText("Decision on B", DecisionB);
        s.PrintText("Decision on the interaction", DecisionAB);
        s.Either("w, p_w = stats.shapiro(fit.resid)", "sw <- shapiro.test(residuals(fit)); w <- sw$statistic; p_w <- sw$p.value");
        s.Print("Shapiro–Wilk W of residuals", "w", "w", 4);
        s.Print("Shapiro–Wilk p of residuals", "p_w", "p_w", 4);
        s.Line();
        s.Either("print(tab)", "print(tab)");
        return s.ToString();
    }
}

/// <summary>The two-factor ANOVA for a balanced design.</summary>
public sealed class TwoFactorFTests : CalculationMethod<TwoWayAnova>
{
    public override string Name => "Two-factor ANOVA with replication";

    public override string Description =>
        "SS_T = SS_A + SS_B + SS_AB + SS_E for a balanced design; F_A = MS_A/MS_E, F_B = MS_B/MS_E, F_AB = MS_AB/MS_E, each " +
        "against F with its own and the error degrees of freedom ab(n − 1). With n = 1 the additive model, error on (a − 1)(b − 1).";

    public override string Citation =>
        "EGN3443 Module 14; Montgomery, D. C. and Runger, G. C., Applied Statistics and Probability for Engineers, 7th ed., " +
        "2018, §14-3 (two-factor factorial experiments) and §13-4 (randomized complete block design, the n = 1 case).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(TwoWayAnova.Table), nameof(TwoWayAnova.FactorA), nameof(TwoWayAnova.FactorB), nameof(TwoWayAnova.Response), nameof(TwoWayAnova.Alpha)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(TwoWayAnova.LevelsA), nameof(TwoWayAnova.LevelsB), nameof(TwoWayAnova.Replicates), nameof(TwoWayAnova.N), nameof(TwoWayAnova.CellTable),
        nameof(TwoWayAnova.GrandMean), nameof(TwoWayAnova.SsA), nameof(TwoWayAnova.SsB), nameof(TwoWayAnova.SsAB), nameof(TwoWayAnova.SsError), nameof(TwoWayAnova.SsTotal),
        nameof(TwoWayAnova.DfA), nameof(TwoWayAnova.DfB), nameof(TwoWayAnova.DfAB), nameof(TwoWayAnova.DfError),
        nameof(TwoWayAnova.MsA), nameof(TwoWayAnova.MsB), nameof(TwoWayAnova.MsAB), nameof(TwoWayAnova.MsError),
        nameof(TwoWayAnova.FA), nameof(TwoWayAnova.FB), nameof(TwoWayAnova.FAB), nameof(TwoWayAnova.PA), nameof(TwoWayAnova.PB), nameof(TwoWayAnova.PAB),
        nameof(TwoWayAnova.DecisionA), nameof(TwoWayAnova.DecisionB), nameof(TwoWayAnova.DecisionAB),
    ];

    public override void Apply(TwoWayAnova subject) => subject.Recalculate();
}

/// <summary>Normality of the residuals.</summary>
public sealed class TwoWayResidualCheck : CalculationMethod<TwoWayAnova>
{
    public override string Name => "Residual normality";

    public override string Description => "Shapiro–Wilk on the residuals yᵢⱼₖ − ȳᵢⱼ. (or from the additive fit), with the Q–Q plot.";

    public override string Citation => "Shapiro and Wilk, Biometrika 52, 1965, by Royston's AS R94 (1995); Montgomery and Runger §14-3.2 (model adequacy checking).";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(TwoWayAnova.ShapiroW), nameof(TwoWayAnova.ShapiroP)];

    public override void Apply(TwoWayAnova subject) => subject.Recalculate();
}
