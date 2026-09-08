using System.ComponentModel;
using System.Globalization;
using System.Text;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Methods.Regression;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum VariableSelection
{
    [Description("All the predictors listed")]
    AllEntered = 0,

    [Description("Forward selection (add while p < 0.05)")]
    Forward,

    [Description("Backward elimination (drop while p ≥ 0.05)")]
    Backward,
}

/// <summary>
/// EGN3443 Modules 12 and 13: multiple linear regression of a response on several predictors
/// from one data table — the coefficient table with VIFs, R² and adjusted R², the F-test,
/// forward or backward selection, the residual diagnostics, and k-fold cross-validation.
/// </summary>
[StatistleHelp(
    "Multiple linear regression (Modules 12 and 13): ŷ = b₀ + b₁x₁ + b₂x₂ + … fitted by least " +
    "squares to columns of one data table. Choose the table, the response column and the " +
    "predictor columns (by name, separated by commas; blank means every other column). Read " +
    "the coefficient table — each coefficient is the change in y per unit of its predictor " +
    "with the others held constant, with its standard error, t, p-value and variance inflation " +
    "factor — then R² and adjusted R² (which does not rise just because a predictor was added), " +
    "the F-test of the whole model, and the residual standard error. Forward selection adds " +
    "the most significant predictor while one has p < 0.05; backward elimination starts from " +
    "all and drops the least significant while one has p ≥ 0.05. VIF above about 5 (some say " +
    "10) means predictors that overlap and coefficients that cannot be trusted one by one. " +
    "k-fold cross-validation reports the prediction error on held-out rows, the honest " +
    "measure of how the model would do on new data; compare it with the in-sample error. The " +
    "Code tab writes the fit with statsmodels' OLS and R's lm.")]
public sealed class MultipleRegression : Analysis, IHasFigures, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, ICodeSource
{
    private string _table = string.Empty;
    private string _response = string.Empty;
    private string _predictors = string.Empty;
    private VariableSelection _selection = VariableSelection.AllEntered;
    private double _confidence = 0.95;
    private double _folds = 5;

    private int _count;
    private string _predictorsUsed = string.Empty;
    private string _equation = string.Empty;
    private string _coefficientTable = string.Empty;
    private double _rSquared = double.NaN;
    private double _adjustedRSquared = double.NaN;
    private double _residualSe = double.NaN;
    private double _fStatistic = double.NaN;
    private double _fP = double.NaN;
    private double _dfRegression = double.NaN;
    private double _dfError = double.NaN;
    private double _maxVif = double.NaN;
    private string _maxVifPredictor = string.Empty;
    private double _durbinWatson = double.NaN;
    private double _shapiroW = double.NaN;
    private double _shapiroP = double.NaN;
    private double _maxCooksD = double.NaN;
    private string _influential = string.Empty;
    private double _maxLeverage = double.NaN;
    private double _inSampleRmse = double.NaN;
    private double _cvRmse = double.NaN;
    private string _selectionNote = string.Empty;

    public MultipleRegression()
    {
        Name = "Multiple regression";
        Recalculate();
    }

    public override string Title => "Multiple regression";

    public override string Summary => "Several predictors: coefficients, VIF, adjusted R², selection, diagnostics, cross-validation (Modules 12–13)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 41;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public LinearModel? Model { get; private set; }

    /// <summary>The columns fitted, in order, with the response last — for the figures and the code.</summary>
    [StatistleIgnore]
    public IReadOnlyList<DataColumn> Columns { get; private set; } = [];

    [StatistleIgnore]
    public DataColumn? ResponseColumn { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Data table", Role = VariableRole.Input,
        Help = "The table holding the response and the predictors as columns of equal length, " +
               "one row per observation.")]
    public string Table
    {
        get => _table;
        set => SetProperty(ref _table, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Response column (y)", Role = VariableRole.Input,
        Help = "The column being predicted — the outcome the predictors are meant to explain.")]
    public string Response
    {
        get => _response;
        set => SetProperty(ref _response, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Predictor columns", Role = VariableRole.Input,
        Help = "The predictors' column names, separated by commas — 'size, age, bedrooms'. Leave " +
               "blank to use every column of the table except the response. A name the table " +
               "does not have is reported.")]
    public string Predictors
    {
        get => _predictors;
        set => SetProperty(ref _predictors, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Variable selection", Role = VariableRole.Design,
        Help = "Fit all the predictors listed, or let the data choose: forward selection adds " +
               "one predictor at a time (the one with the smallest p-value) while it is " +
               "significant at 0.05; backward elimination starts with all and removes the " +
               "least significant while its p ≥ 0.05. Selection is exploratory — report that " +
               "you used it.")]
    public VariableSelection Selection
    {
        get => _selection;
        set => SetProperty(ref _selection, value);
    }

    [StatistleProperty(Label = "Confidence level", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.5, Max = 0.9999,
        Help = "For the coefficients' confidence intervals in the table.")]
    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    [StatistleProperty(Label = "Cross-validation folds (k)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Design, Min = 0, Max = 50,
        Help = "How many folds for k-fold cross-validation: the rows are cut into k contiguous " +
               "blocks (as scikit-learn's KFold without shuffling), each held out in turn while " +
               "the model is fitted to the rest. 5 or 10 are usual; n gives leave-one-out; 0 " +
               "skips it.")]
    public double Folds
    {
        get => _folds;
        set => SetProperty(ref _folds, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Observations (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many complete rows were fitted.")]
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    [StatistleProperty(Label = "Predictors in the model", Role = VariableRole.Output,
        Help = "The predictors the fitted model uses — all those listed, or those selection kept.")]
    public string PredictorsUsed
    {
        get => _predictorsUsed;
        set => SetProperty(ref _predictorsUsed, value);
    }

    [StatistleProperty(Label = "Equation", Role = VariableRole.Output,
        Help = "The fitted equation with its coefficients.")]
    public string Equation
    {
        get => _equation;
        set => SetProperty(ref _equation, value);
    }

    [StatistleProperty(Label = "Coefficients", Role = VariableRole.Output,
        Help = "One line per coefficient: estimate, standard error, t, p-value, the confidence " +
               "interval, and the variance inflation factor. A small p says the predictor adds " +
               "information beyond the others; a large VIF says it overlaps them.")]
    public string CoefficientTable
    {
        get => _coefficientTable;
        set => SetProperty(ref _coefficientTable, value);
    }

    [StatistleProperty(Label = "R²", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The fraction of y's variation the model explains. It never falls when a " +
               "predictor is added, however useless — hence adjusted R².")]
    public double RSquared
    {
        get => _rSquared;
        set => SetProperty(ref _rSquared, value);
    }

    [StatistleProperty(Label = "Adjusted R²", Decimals = 4, Role = VariableRole.Output,
        Help = "1 − (1 − R²)(n − 1)/(n − p): R² charged for each coefficient. It falls when a " +
               "predictor adds less than chance would; the number to compare models by.")]
    public double AdjustedRSquared
    {
        get => _adjustedRSquared;
        set => SetProperty(ref _adjustedRSquared, value);
    }

    [StatistleProperty(Label = "Residual standard error (σ̂)", Decimals = 4, Role = VariableRole.Output,
        Help = "√(SS_E/(n − p)): the typical miss of the model, in y's units.")]
    public double ResidualSe
    {
        get => _residualSe;
        set => SetProperty(ref _residualSe, value);
    }

    [StatistleProperty(Label = "ANOVA F", Decimals = 4, Role = VariableRole.Output,
        Help = "MS_R/MS_E: the test that at least one coefficient is not zero — that the model " +
               "explains anything at all.")]
    public double FStatistic
    {
        get => _fStatistic;
        set => SetProperty(ref _fStatistic, value);
    }

    [StatistleProperty(Label = "ANOVA p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The p-value of the overall F-test.")]
    public double FP
    {
        get => _fP;
        set => SetProperty(ref _fP, value);
    }

    [StatistleProperty(Label = "Regression degrees of freedom", Decimals = 0, Role = VariableRole.Calculated,
        Help = "k, the number of predictors in the fitted model (the numerator of F).")]
    public double DfRegression
    {
        get => _dfRegression;
        set => SetProperty(ref _dfRegression, value);
    }

    [StatistleProperty(Label = "Error degrees of freedom", Decimals = 0, Role = VariableRole.Calculated,
        Help = "n − k − 1: the observations left after the coefficients are estimated (the denominator of F).")]
    public double DfError
    {
        get => _dfError;
        set => SetProperty(ref _dfError, value);
    }

    [StatistleProperty(Label = "Largest VIF", Decimals = 4, Role = VariableRole.Output,
        Help = "The largest variance inflation factor, 1/(1 − Rⱼ²) where Rⱼ² is from regressing " +
               "predictor j on the others. 1 means no overlap; above 5 to 10, multicollinearity " +
               "inflates the standard errors and the coefficients wobble.")]
    public double MaxVif
    {
        get => _maxVif;
        set => SetProperty(ref _maxVif, value);
    }

    [StatistleProperty(Label = "Predictor with the largest VIF", Role = VariableRole.Calculated,
        Help = "Which predictor overlaps the others most.")]
    public string MaxVifPredictor
    {
        get => _maxVifPredictor;
        set => SetProperty(ref _maxVifPredictor, value);
    }

    [StatistleProperty(Label = "Durbin–Watson", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Independence of the residuals in row order: near 2 is good.")]
    public double DurbinWatson
    {
        get => _durbinWatson;
        set => SetProperty(ref _durbinWatson, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk W of residuals", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Normality of the residuals; read with the Q–Q plot.")]
    public double ShapiroW
    {
        get => _shapiroW;
        set => SetProperty(ref _shapiroW, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk p of residuals", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "p > 0.05: no evidence against normal residuals.")]
    public double ShapiroP
    {
        get => _shapiroP;
        set => SetProperty(ref _shapiroP, value);
    }

    [StatistleProperty(Label = "Largest Cook's distance", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The influence of the single most influential row on the fit; above about 1 deserves a look.")]
    public double MaxCooksD
    {
        get => _maxCooksD;
        set => SetProperty(ref _maxCooksD, value);
    }

    [StatistleProperty(Label = "Most influential observation", Role = VariableRole.Calculated,
        Help = "The row with the largest Cook's distance.")]
    public string Influential
    {
        get => _influential;
        set => SetProperty(ref _influential, value);
    }

    [StatistleProperty(Label = "Largest leverage", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The largest hat-matrix diagonal; leverages average p/n, above 2p/n is high.")]
    public double MaxLeverage
    {
        get => _maxLeverage;
        set => SetProperty(ref _maxLeverage, value);
    }

    [StatistleProperty(Label = "In-sample RMSE", Decimals = 4, Role = VariableRole.Calculated,
        Help = "√(SS_E/n): the root-mean-square error on the rows the model was fitted to — " +
               "optimistic, since the fit has seen them.")]
    public double InSampleRmse
    {
        get => _inSampleRmse;
        set => SetProperty(ref _inSampleRmse, value);
    }

    [StatistleProperty(Label = "Cross-validated RMSE", Decimals = 4, Role = VariableRole.Output,
        Help = "The root-mean-square error on held-out rows over the k folds: the model's " +
               "expected error on new data. Much above the in-sample RMSE means overfitting.")]
    public double CvRmse
    {
        get => _cvRmse;
        set => SetProperty(ref _cvRmse, value);
    }

    [StatistleProperty(Label = "Selection", Role = VariableRole.Calculated,
        Help = "What the selection procedure did, step by step.")]
    public string SelectionNote
    {
        get => _selectionNote;
        set => SetProperty(ref _selectionNote, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    public override void Recalculate()
    {
        Problem = null;
        Model = null;
        Columns = [];
        ResponseColumn = null;
        Count = 0;
        PredictorsUsed = Equation = CoefficientTable = MaxVifPredictor = Influential = SelectionNote = string.Empty;
        RSquared = AdjustedRSquared = ResidualSe = FStatistic = FP = DfRegression = DfError = MaxVif = double.NaN;
        DurbinWatson = ShapiroW = ShapiroP = MaxCooksD = MaxLeverage = InSampleRmse = CvRmse = double.NaN;

        DataTable? table = TableChosen();

        if (table is null)
        {
            Problem = "Choose the data table.";
            return;
        }

        DataColumn? y = table.Columns.Column(Response);

        if (y is null)
        {
            Problem = "Choose the response column.";
            return;
        }

        List<DataColumn> predictors = [];

        if (string.IsNullOrWhiteSpace(Predictors))
        {
            predictors.AddRange(table.Columns.Columns.Where(c => !ReferenceEquals(c, y)));
        }
        else
        {
            foreach (string name in Predictors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                DataColumn? column = table.Columns.Column(name);

                if (column is null)
                {
                    Problem = $"The table has no column named '{name}'.";
                    return;
                }

                if (ReferenceEquals(column, y))
                {
                    Problem = $"'{name}' is the response; it cannot also be a predictor.";
                    return;
                }

                predictors.Add(column);
            }
        }

        if (predictors.Count == 0)
        {
            Problem = "Name at least one predictor column.";
            return;
        }

        int n = y.Count;

        if (predictors.Any(c => c.Count != n))
        {
            Problem = "The response and every predictor must have the same number of rows.";
            return;
        }

        if (n < predictors.Count + 2)
        {
            Problem = $"At least {predictors.Count + 2} rows are needed for {predictors.Count} predictor(s).";
            return;
        }

        if (double.IsNaN(Confidence) || Confidence <= 0 || Confidence >= 1)
        {
            Problem = "Enter a confidence level between 0 and 1.";
            return;
        }

        ResponseColumn = y;
        List<DataColumn> chosen;

        try
        {
            chosen = MethodScope.ActiveMethods(this).OfType<ISelectionMethod>().First().Choose(predictors, y, Confidence, out string note);
            SelectionNote = note;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Problem = ex.Message;
            return;
        }

        if (chosen.Count == 0)
        {
            Problem = "Selection kept no predictor: none is significant at 0.05. Fit them all to see why.";
            return;
        }

        Model = LinearModel.Fit([.. chosen.Select(c => c.Name)], [.. chosen.Select(c => (IReadOnlyList<double>)c.Values)], y.Values, Confidence);

        if (Model is null)
        {
            Problem = "The predictors are collinear (one is a combination of the others) or have no spread; drop one.";
            return;
        }

        Columns = [.. chosen, y];
        LinearModel m = Model;
        Count = m.N;
        PredictorsUsed = string.Join(", ", chosen.Select(c => c.Name));
        StringBuilder equation = new($"{y.Name} = {m.Intercept:G6}");
        StringBuilder rows = new("term: estimate, se, t, p, CI lower, CI upper, VIF");

        foreach (Coefficient c in m.Coefficients)
        {
            if (c.Name != "(Intercept)")
            {
                equation.Append($" {(c.Estimate < 0 ? "−" : "+")} {Math.Abs(c.Estimate):G6}·{c.Name}");
            }

            rows.Append('\n').Append($"{c.Name}: {c.Estimate:G6}, {c.StandardError:G4}, {c.T:F3}, {c.PValue:F4}, {c.Lower:G6}, {c.Upper:G6}, {(double.IsNaN(c.Vif) ? "—" : c.Vif.ToString("F3", CultureInfo.CurrentCulture))}");
        }

        Equation = equation.ToString();
        CoefficientTable = rows.ToString();
        RSquared = m.RSquared;
        AdjustedRSquared = m.AdjustedRSquared;
        ResidualSe = m.ResidualStandardError;
        FStatistic = m.F;
        FP = m.FPValue;
        DfRegression = m.DegreesOfFreedomRegression;
        DfError = m.DegreesOfFreedomError;
        Coefficient worstVif = m.Coefficients.Skip(1).OrderByDescending(c => c.Vif).First();
        MaxVif = worstVif.Vif;
        MaxVifPredictor = worstVif.Name;
        DurbinWatson = m.DurbinWatson;
        AssumptionCheck normality = Nonparametric.ShapiroWilk(m.Residuals);
        ShapiroW = normality.Statistic;
        ShapiroP = normality.PValue;
        int worst = Array.IndexOf(m.CooksDistance, m.CooksDistance.Max());
        MaxCooksD = m.CooksDistance[worst];
        Influential = $"row {worst + 1} ({y.Name} = {y.Values[worst]:G6})";
        MaxLeverage = m.Leverage.Max();

        int folds = double.IsNaN(Folds) ? 0 : (int)Math.Round(Folds);
        (InSampleRmse, CvRmse) = LinearModel.CrossValidate([.. chosen.Select(c => (IReadOnlyList<double>)c.Values)], y.Values, folds);
    }

    private DataTable? TableChosen() =>
        int.TryParse(Table, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? Project?.Tables.FirstOrDefault(t => t.Id == id) : null;

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName)
    {
        if (propertyName == nameof(Table))
        {
            return Project is null ? [] : [.. Project.Tables.Select(t => new ChoiceOption(t.Id.ToString(CultureInfo.InvariantCulture), t.Name))];
        }

        if (propertyName == nameof(Response))
        {
            return TableChosen() is DataTable table ? [.. table.Columns.Columns.Select(c => new ChoiceOption(c.Name, c.Name))] : [];
        }

        return [];
    }

    public StatistleObject? LinkedObject(string propertyName) =>
        propertyName is nameof(Table) or nameof(Response) or nameof(Predictors) ? TableChosen() : null;

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Model is null || ResponseColumn is null)
        {
            return [];
        }

        return
        [
            RegressionFigures.ObservedVersusPredicted($"{Name} — observed against predicted {ResponseColumn.Name}", Model, ResponseColumn.Values),
            RegressionFigures.ResidualsVersusFitted($"{Name} — residuals against fitted values", Model),
            RegressionFigures.NormalQq($"{Name} — normal Q–Q plot of residuals", Model),
        ];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);

        if (Model is null || ResponseColumn is null)
        {
            return string.Empty;
        }

        IReadOnlyList<DataColumn> predictors = [.. Columns.Take(Columns.Count - 1)];
        string[] names = [.. predictors.Select(c => CodeText.Identifier(c.Name))];
        string yName = CodeText.Identifier(ResponseColumn.Name, "y");
        s.Imports(scipyStats: true, pandas: true, statsmodels: true);
        s.Either("from statsmodels.stats.stattools import durbin_watson\nfrom statsmodels.stats.outliers_influence import variance_inflation_factor", "# Base R plus the formulas; car::vif and car::durbinWatsonTest give the same diagnostics");
        s.Line();
        s.Comment($"The data: {ResponseColumn.Name} on {string.Join(", ", predictors.Select(c => c.Name))}");

        foreach ((DataColumn column, string name) in predictors.Zip(names))
        {
            s.Vector(name, column.Values);
        }

        s.Vector(yName, ResponseColumn.Values);
        s.Assign("alpha", Math.Round(1 - Confidence, 10));

        if (Selection != VariableSelection.AllEntered)
        {
            s.Comment($"Predictors chosen by {(Selection == VariableSelection.Forward ? "forward selection" : "backward elimination")} at α = 0.05: {PredictorsUsed}");
        }

        s.Either($"X = pd.DataFrame({{{string.Join(", ", names.Select(n => $"'{n}': {n}"))}}})", $"data <- data.frame({string.Join(", ", names)}, {yName})");
        s.Either($"fit = sm.OLS({yName}, sm.add_constant(X)).fit()", $"fit <- lm({yName} ~ {string.Join(" + ", names)}, data = data)");
        s.Either("n, p = int(fit.nobs), len(fit.params)", $"n <- length({yName}); p <- length(coef(fit))");
        s.Line();
        s.PrintRaw("Observations (n)", "n", "n");
        s.PrintText("Predictors in the model", PredictorsUsed);
        s.PrintText("Equation", Equation);
        s.Either("print(\"Coefficients:\"); print(fit.summary2(alpha=alpha).tables[1])", "cat(\"Coefficients:\\n\"); print(cbind(summary(fit)$coefficients, confint(fit, level = 1 - alpha)))");
        s.Print("R²", "fit.rsquared", "summary(fit)$r.squared", 4);
        s.Print("Adjusted R²", "fit.rsquared_adj", "summary(fit)$adj.r.squared", 4);
        s.Print("Residual standard error (σ̂)", "np.sqrt(fit.mse_resid)", "summary(fit)$sigma", 4);
        s.Print("ANOVA F", "fit.fvalue", "summary(fit)$fstatistic[[1]]", 4);
        s.Print("ANOVA p-value", "fit.f_pvalue", "pf(summary(fit)$fstatistic[[1]], summary(fit)$fstatistic[[2]], summary(fit)$fstatistic[[3]], lower.tail = FALSE)", 4);
        s.PrintRaw("Regression degrees of freedom", "int(fit.df_model)", "p - 1");
        s.PrintRaw("Error degrees of freedom", "int(fit.df_resid)", "n - p");
        s.Line();
        s.Comment("Variance inflation factors (1 for a single predictor)");

        if (names.Length > 1)
        {
            s.Either("Xc = sm.add_constant(X); vif = [variance_inflation_factor(Xc.values, j) for j in range(1, Xc.shape[1])]",
                     $"vif <- sapply(seq_along(coef(fit))[-1], function(j) {{ Xj <- model.matrix(fit)[, -1]; r2 <- summary(lm(Xj[, j - 1] ~ Xj[, -(j - 1)]))$r.squared; 1 / (1 - r2) }})");
        }
        else
        {
            s.Either("vif = [1.0]", "vif <- c(1)");
        }

        s.Print("Largest VIF", "max(vif)", "max(vif)", 4);
        s.Either("print(\"Predictor with the largest VIF: \" + X.columns[int(np.argmax(vif))])", $"cat(\"Predictor with the largest VIF:\", names(coef(fit))[-1][which.max(vif)], \"\\n\")");
        s.Line();
        s.Comment("Diagnostics (Module 13)");
        s.Either("resid = fit.resid", "resid <- residuals(fit)");
        s.Print("Durbin–Watson", "durbin_watson(resid)", "sum(diff(resid)^2) / sum(resid^2)", 4);
        s.Either("w, p_w = stats.shapiro(resid)", "sw <- shapiro.test(resid); w <- sw$statistic; p_w <- sw$p.value");
        s.Print("Shapiro–Wilk W of residuals", "w", "w", 4);
        s.Print("Shapiro–Wilk p of residuals", "p_w", "p_w", 4);
        s.Either("influence = fit.get_influence()", "cd <- cooks.distance(fit); h <- hatvalues(fit)");
        s.Print("Largest Cook's distance", "influence.cooks_distance[0].max()", "max(cd)", 4);
        s.Either("print(\"Most influential observation: row \" + str(int(np.argmax(influence.cooks_distance[0])) + 1))", "cat(\"Most influential observation: row\", which.max(cd), \"\\n\")");
        s.Print("Largest leverage", "influence.hat_matrix_diag.max()", "max(h)", 4);
        s.Print("In-sample RMSE", "np.sqrt(fit.ssr / n)", "sqrt(sum(resid^2) / n)", 4);
        s.PrintText("Selection", SelectionNote);

        if (!double.IsNaN(CvRmse))
        {
            int k = (int)Math.Round(Folds);
            s.Line();
            s.Comment($"{k}-fold cross-validation with contiguous folds (scikit-learn's KFold(shuffle=False))");
            s.Assign("k", k);
            s.Either("Xc = sm.add_constant(X).values; yv = np.asarray(" + yName + ", dtype=float); sq = 0.0",
                     $"Xc <- cbind(1, as.matrix(data[, seq_len(ncol(data) - 1)])); yv <- {yName}; sq <- 0");
            s.Either("for f in range(k):", "for (f in 0:(k - 1)) {");
            s.Either("    start, end = f * n // k, (f + 1) * n // k", "  start <- floor(f * n / k) + 1; end <- floor((f + 1) * n / k)");
            s.Either("    test = np.arange(start, end); train = np.setdiff1d(np.arange(n), test)", "  test <- start:end; train <- setdiff(seq_len(n), test)");
            s.Either("    beta = np.linalg.lstsq(Xc[train], yv[train], rcond=None)[0]", "  beta <- qr.solve(Xc[train, , drop = FALSE], yv[train])");
            s.Either("    sq += np.sum((yv[test] - Xc[test] @ beta) ** 2)", "  sq <- sq + sum((yv[test] - Xc[test, , drop = FALSE] %*% beta)^2)");
            s.Either(string.Empty, "}");
            s.Print("Cross-validated RMSE", "np.sqrt(sq / n)", "sqrt(sq / n)", 4);
        }

        s.Line();
        s.Either("print(fit.summary())", "print(summary(fit))");
        return s.ToString();
    }
}

/// <summary>A selection procedure is a method: it says which predictors the model keeps.</summary>
public interface ISelectionMethod : ICalculationMethod
{
    List<DataColumn> Choose(IReadOnlyList<DataColumn> candidates, DataColumn response, double confidence, out string note);
}

public abstract class SelectionMethod : CalculationMethod<MultipleRegression>, ISelectionMethod
{
    public abstract VariableSelection Selects { get; }

    public override bool IsApplicableTo(MultipleRegression subject) => subject is not null && subject.Selection == Selects;

    public override string Citation =>
        "EGN3443 Modules 12–13; Montgomery, D. C. and Runger, G. C., Applied Statistics and Probability for " +
        "Engineers, 7th ed., 2018, Chapter 12 (multiple linear regression; §12-6 aspects of multiple regression " +
        "modeling: multicollinearity and variable selection).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(MultipleRegression.Table), nameof(MultipleRegression.Response), nameof(MultipleRegression.Predictors), nameof(MultipleRegression.Confidence), nameof(MultipleRegression.Folds)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(MultipleRegression.Count), nameof(MultipleRegression.PredictorsUsed), nameof(MultipleRegression.Equation), nameof(MultipleRegression.CoefficientTable),
        nameof(MultipleRegression.RSquared), nameof(MultipleRegression.AdjustedRSquared), nameof(MultipleRegression.ResidualSe),
        nameof(MultipleRegression.FStatistic), nameof(MultipleRegression.FP), nameof(MultipleRegression.DfRegression), nameof(MultipleRegression.DfError),
        nameof(MultipleRegression.MaxVif), nameof(MultipleRegression.MaxVifPredictor), nameof(MultipleRegression.DurbinWatson),
        nameof(MultipleRegression.ShapiroW), nameof(MultipleRegression.ShapiroP), nameof(MultipleRegression.MaxCooksD), nameof(MultipleRegression.Influential),
        nameof(MultipleRegression.MaxLeverage), nameof(MultipleRegression.InSampleRmse), nameof(MultipleRegression.CvRmse), nameof(MultipleRegression.SelectionNote),
    ];

    public override void Apply(MultipleRegression subject) => subject.Recalculate();

    public abstract List<DataColumn> Choose(IReadOnlyList<DataColumn> candidates, DataColumn response, double confidence, out string note);

    protected static LinearModel? FitOf(IReadOnlyList<DataColumn> predictors, DataColumn response, double confidence) =>
        LinearModel.Fit([.. predictors.Select(c => c.Name)], [.. predictors.Select(c => (IReadOnlyList<double>)c.Values)], response.Values, confidence);
}

public sealed class AllPredictors : SelectionMethod
{
    public override VariableSelection Selects => VariableSelection.AllEntered;

    public override string Name => "Multiple linear regression";

    public override string Description => "β̂ = (XᵀX)⁻¹Xᵀy on every predictor listed; coefficient tests on n − p degrees of freedom; VIFs; residual diagnostics; k-fold cross-validation.";

    public override List<DataColumn> Choose(IReadOnlyList<DataColumn> candidates, DataColumn response, double confidence, out string note)
    {
        note = "All the predictors listed were fitted.";
        return [.. candidates];
    }
}

public sealed class ForwardSelection : SelectionMethod
{
    public override VariableSelection Selects => VariableSelection.Forward;

    public override string Name => "Forward selection";

    public override string Description => "Starting from no predictors, add the one with the smallest p-value while that p < 0.05.";

    public override List<DataColumn> Choose(IReadOnlyList<DataColumn> candidates, DataColumn response, double confidence, out string note)
    {
        List<DataColumn> chosen = [];
        List<string> steps = [];

        while (true)
        {
            DataColumn? best = null;
            double bestP = double.PositiveInfinity;

            foreach (DataColumn candidate in candidates.Where(c => !chosen.Contains(c)))
            {
                LinearModel? trial = FitOf([.. chosen, candidate], response, confidence);
                double p = trial?.Coefficients[^1].PValue ?? double.PositiveInfinity;

                if (p < bestP)
                {
                    bestP = p;
                    best = candidate;
                }
            }

            if (best is null || bestP >= 0.05)
            {
                steps.Add(best is null ? "no candidate left" : $"stopped: adding {best.Name} would give p = {bestP:F4}");
                break;
            }

            chosen.Add(best);
            steps.Add($"added {best.Name} (p = {bestP:F4})");
        }

        note = string.Join("; ", steps);
        return chosen;
    }
}

public sealed class BackwardElimination : SelectionMethod
{
    public override VariableSelection Selects => VariableSelection.Backward;

    public override string Name => "Backward elimination";

    public override string Description => "Starting from all the predictors, drop the one with the largest p-value while that p ≥ 0.05.";

    public override List<DataColumn> Choose(IReadOnlyList<DataColumn> candidates, DataColumn response, double confidence, out string note)
    {
        List<DataColumn> chosen = [.. candidates];
        List<string> steps = [];

        while (chosen.Count > 0)
        {
            LinearModel? model = FitOf(chosen, response, confidence);

            if (model is null)
            {
                throw new ArgumentOutOfRangeException(nameof(candidates), "The predictors are collinear; drop one before selecting.");
            }

            Coefficient worst = model.Coefficients.Skip(1).OrderByDescending(c => c.PValue).First();

            if (worst.PValue < 0.05)
            {
                steps.Add($"stopped: every remaining p < 0.05 (largest {worst.Name}, p = {worst.PValue:F4})");
                break;
            }

            chosen.Remove(chosen.First(c => c.Name == worst.Name));
            steps.Add($"dropped {worst.Name} (p = {worst.PValue:F4})");
        }

        note = string.Join("; ", steps);
        return chosen;
    }
}
