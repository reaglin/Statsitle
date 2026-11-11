using System.Globalization;
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

/// <summary>
/// EGN3443 Modules 11 and 13: simple linear regression of y on x by least squares, with the
/// inference on the slope, the ANOVA table, prediction with its two intervals, the residual
/// diagnostics, and the transformations of x or y that straighten a curved relation.
/// </summary>
[StatistleHelp(
    "Simple linear regression (Modules 11 and 13): the least-squares line ŷ = b₀ + b₁x through " +
    "(x, y) pairs — two columns of a data table, or pairs typed here. Read the slope and " +
    "intercept with their standard errors, the t-test and confidence interval on the slope " +
    "(is there a relation at all?), r and R² (how much of y's variation the line explains), " +
    "the residual standard error (the typical miss), the ANOVA table with its F, and a " +
    "prediction at a chosen x with the confidence interval on the mean response and the wider " +
    "prediction interval on one new observation. Below those, the diagnostics of Module 13: " +
    "Durbin–Watson for independence of residuals, Shapiro–Wilk for their normality, the " +
    "largest Cook's distance and leverage for influential points. When the residual plot " +
    "curves, transform x or y (log, square root, reciprocal) and compare R² and the residual " +
    "plots; the fit is then linear in the transformed variable and drawn back in the original " +
    "units. The Code tab writes the fit with statsmodels and R's lm.")]
public sealed class SimpleRegression : Analysis, IHasFigures, IUsesProject, IHasChoices, ILinksTo, IReportsProblem, ICodeSource
{
    private string _xColumn = string.Empty;
    private string _yColumn = string.Empty;
    private Transformation _xTransformation = Transformation.None;
    private Transformation _yTransformation = Transformation.None;
    private double _confidence = 0.95;
    private double _predictAt = double.NaN;

    private int _count;
    private int _skipped;
    private string _equation = string.Empty;
    private double _intercept = double.NaN;
    private double _slope = double.NaN;
    private double _slopeSe = double.NaN;
    private double _slopeT = double.NaN;
    private double _slopeP = double.NaN;
    private double _slopeLower = double.NaN;
    private double _slopeUpper = double.NaN;
    private double _interceptSe = double.NaN;
    private double _interceptP = double.NaN;
    private double _correlation = double.NaN;
    private double _rSquared = double.NaN;
    private double _adjustedRSquared = double.NaN;
    private double _residualSe = double.NaN;
    private double _ssRegression = double.NaN;
    private double _ssError = double.NaN;
    private double _ssTotal = double.NaN;
    private double _fStatistic = double.NaN;
    private double _fP = double.NaN;
    private double _fittedAt = double.NaN;
    private double _meanLower = double.NaN;
    private double _meanUpper = double.NaN;
    private double _newLower = double.NaN;
    private double _newUpper = double.NaN;
    private double _durbinWatson = double.NaN;
    private double _shapiroW = double.NaN;
    private double _shapiroP = double.NaN;
    private double _maxCooksD = double.NaN;
    private string _influential = string.Empty;
    private double _maxLeverage = double.NaN;

    public SimpleRegression()
    {
        Name = "Simple linear regression";

        // A short set with an obvious trend, whose numbers are exact by hand.
        foreach ((double x, double y) in new[] { (1.0, 2.0), (2, 4), (3, 5), (4, 4), (5, 5) })
        {
            Data.Add(x, y);
        }

        Recalculate();
    }

    public override string Title => "Simple linear regression";

    public override string Summary => "Least-squares line, slope inference, ANOVA, prediction, diagnostics, transformations (Modules 11, 13)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 40;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleIgnore]
    public LinearModel? Model { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "x column", Role = VariableRole.Input,
        Help = "The data table column for x, the predictor (the variable you control or know " +
               "first), or 'Entered below' to type the pairs.")]
    public string XColumn
    {
        get => _xColumn;
        set => SetProperty(ref _xColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "y column", Role = VariableRole.Input,
        Help = "The column for y, the response — the variable being predicted. Rows are paired " +
               "as far as the shorter column goes.")]
    public string YColumn
    {
        get => _yColumn;
        set => SetProperty(ref _yColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "Data (x, y)", ReportLabel = "Observations (x, y)", Role = VariableRole.Input,
        Help = "The observations, one pair per line: x then y, separated by a tab, a comma or a " +
               "semicolon. Paste two columns from a spreadsheet. At least three pairs.")]
    public DataSeries Data { get; } = new();

    [StatistleProperty(Label = "Transform x", Role = VariableRole.Design,
        Help = "A transformation of x before fitting: ln x for a relation that flattens out, √x " +
               "or 1/x for other curvatures (Module 13). Values a transform cannot take (a log of " +
               "zero) are skipped and counted. None is the plain line.")]
    public Transformation XTransformation
    {
        get => _xTransformation;
        set => SetProperty(ref _xTransformation, value);
    }

    [StatistleProperty(Label = "Transform y", Role = VariableRole.Design,
        Help = "A transformation of y: ln y turns exponential growth into a line and evens out " +
               "spread that grows with y; 1/y for a saturating relation. Predictions are turned " +
               "back into the original units.")]
    public Transformation YTransformation
    {
        get => _yTransformation;
        set => SetProperty(ref _yTransformation, value);
    }

    [StatistleProperty(Label = "Confidence level", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.5, Max = 0.9999,
        Help = "For the intervals on the slope, the mean response and a new observation; 0.95 is usual.")]
    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    [StatistleProperty(Label = "Predict at x₀, optional", Decimals = 4, Role = VariableRole.Input,
        Help = "A value of x to predict y at. Stay inside the range of the data: the line says " +
               "nothing reliable beyond it. Leave blank to skip.")]
    public double PredictAt
    {
        get => _predictAt;
        set => SetProperty(ref _predictAt, value);
    }

    // ── Results: the fit ──────────────────────────────────────────────────────

    [StatistleProperty(Label = "Observations (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many pairs were fitted.")]
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    [StatistleProperty(Label = "Pairs skipped by the transform", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "Pairs a transformation could not take — a log or root of a non-positive value, a reciprocal of zero — left out of the fit.")]
    public int Skipped
    {
        get => _skipped;
        set => SetProperty(ref _skipped, value);
    }

    [StatistleProperty(Label = "Equation", Role = VariableRole.Output,
        Help = "The fitted line, in the transformed variables when a transform is in force.")]
    public string Equation
    {
        get => _equation;
        set => SetProperty(ref _equation, value);
    }

    [StatistleProperty(Label = "Intercept (b₀)", Decimals = 4, Role = VariableRole.Output,
        Help = "The fitted y when x = 0: b₀ = ȳ − b₁x̄. Often outside the data's range, and then " +
               "only a fitting constant, not a physical statement.")]
    public double Intercept
    {
        get => _intercept;
        set => SetProperty(ref _intercept, value);
    }

    [StatistleProperty(Label = "Slope (b₁)", Decimals = 4, Role = VariableRole.Output,
        Help = "The change in y per unit of x: b₁ = S_xy/S_xx. The number the engineering " +
               "question is usually about.")]
    public double Slope
    {
        get => _slope;
        set => SetProperty(ref _slope, value);
    }

    [StatistleProperty(Label = "Slope standard error", Decimals = 4, Role = VariableRole.Output,
        Help = "se(b₁) = σ̂/√S_xx: how far the fitted slope is likely to be from the true one. " +
               "Python fit.bse[1]; R summary(fit).")]
    public double SlopeSe
    {
        get => _slopeSe;
        set => SetProperty(ref _slopeSe, value);
    }

    [StatistleProperty(Label = "Slope t", Decimals = 4, Role = VariableRole.Output,
        Help = "t = b₁/se(b₁) for H₀: β₁ = 0, against t with n − 2 degrees of freedom. Its " +
               "square is the ANOVA F.")]
    public double SlopeT
    {
        get => _slopeT;
        set => SetProperty(ref _slopeT, value);
    }

    [StatistleProperty(Label = "Slope p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "Two-sided p for the slope: below 0.05, x and y are related (the slope is not " +
               "zero). It says nothing about how strong or useful the relation is — read R².")]
    public double SlopeP
    {
        get => _slopeP;
        set => SetProperty(ref _slopeP, value);
    }

    [StatistleProperty(Label = "Slope confidence interval lower", Decimals = 4, Role = VariableRole.Calculated,
        Help = "b₁ ± t_{α/2,n−2}·se(b₁): the range of true slopes consistent with the data.")]
    public double SlopeLower
    {
        get => _slopeLower;
        set => SetProperty(ref _slopeLower, value);
    }

    [StatistleProperty(Label = "Slope confidence interval upper", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The upper end of the slope's confidence interval.")]
    public double SlopeUpper
    {
        get => _slopeUpper;
        set => SetProperty(ref _slopeUpper, value);
    }

    [StatistleProperty(Label = "Intercept standard error", Decimals = 4, Role = VariableRole.Calculated,
        Help = "se(b₀) = σ̂√(1/n + x̄²/S_xx): how far the fitted intercept is likely to be from the true one.")]
    public double InterceptSe
    {
        get => _interceptSe;
        set => SetProperty(ref _interceptSe, value);
    }

    [StatistleProperty(Label = "Intercept p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "Two-sided p for H₀: β₀ = 0 — rarely the interesting question.")]
    public double InterceptP
    {
        get => _interceptP;
        set => SetProperty(ref _interceptP, value);
    }

    [StatistleProperty(Label = "Correlation (r)", Decimals = 4, Role = VariableRole.Output,
        Help = "The sample correlation coefficient, −1 to 1: the sign of the slope with the " +
               "strength of the linear relation. r² = R².")]
    public double Correlation
    {
        get => _correlation;
        set => SetProperty(ref _correlation, value);
    }

    [StatistleProperty(Label = "R²", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The coefficient of determination, SS_R/SS_T: the fraction of y's variation the " +
               "line explains. 0.877 means 87.7 %. A high R² is not proof of a good model — " +
               "look at the residual plot.")]
    public double RSquared
    {
        get => _rSquared;
        set => SetProperty(ref _rSquared, value);
    }

    [StatistleProperty(Label = "Adjusted R²", Decimals = 4, Role = VariableRole.Calculated,
        Help = "R² penalised for the number of coefficients: 1 − (1 − R²)(n − 1)/(n − 2). " +
               "Matters when comparing models with different numbers of terms.")]
    public double AdjustedRSquared
    {
        get => _adjustedRSquared;
        set => SetProperty(ref _adjustedRSquared, value);
    }

    [StatistleProperty(Label = "Residual standard error (σ̂)", Decimals = 4, Role = VariableRole.Output,
        Help = "√(SS_E/(n − 2)): the typical distance of an observation from the line, in y's " +
               "units — the estimate of σ. Python np.sqrt(fit.mse_resid); R summary(fit)$sigma.")]
    public double ResidualSe
    {
        get => _residualSe;
        set => SetProperty(ref _residualSe, value);
    }

    [StatistleProperty(Label = "SS regression", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_R = b₁S_xy: the variation of y explained by the line (1 degree of freedom).")]
    public double SsRegression
    {
        get => _ssRegression;
        set => SetProperty(ref _ssRegression, value);
    }

    [StatistleProperty(Label = "SS error", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_E = Σ(yᵢ − ŷᵢ)²: the variation left over (n − 2 degrees of freedom).")]
    public double SsError
    {
        get => _ssError;
        set => SetProperty(ref _ssError, value);
    }

    [StatistleProperty(Label = "SS total", Decimals = 4, Role = VariableRole.Calculated,
        Help = "SS_T = Σ(yᵢ − ȳ)² = SS_R + SS_E (n − 1 degrees of freedom).")]
    public double SsTotal
    {
        get => _ssTotal;
        set => SetProperty(ref _ssTotal, value);
    }

    [StatistleProperty(Label = "ANOVA F", Decimals = 4, Role = VariableRole.Output,
        Help = "F = MS_R/MS_E with 1 and n − 2 degrees of freedom: the test of the whole model, " +
               "the square of the slope's t for a single predictor.")]
    public double FStatistic
    {
        get => _fStatistic;
        set => SetProperty(ref _fStatistic, value);
    }

    [StatistleProperty(Label = "ANOVA p-value", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The p-value of the F test — equal to the slope's p-value in simple regression.")]
    public double FP
    {
        get => _fP;
        set => SetProperty(ref _fP, value);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Fitted y at x₀", Decimals = 4, Role = VariableRole.Output,
        Help = "ŷ₀ = b₀ + b₁x₀, turned back into the original units when y was transformed.")]
    public double FittedAt
    {
        get => _fittedAt;
        set => SetProperty(ref _fittedAt, value);
    }

    [StatistleProperty(Label = "Mean response lower", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The confidence interval on the mean of y at x₀: ŷ₀ ± t·σ̂√(1/n + (x₀ − x̄)²/S_xx). " +
               "Narrowest at x̄, wider towards the ends of the data.")]
    public double MeanLower
    {
        get => _meanLower;
        set => SetProperty(ref _meanLower, value);
    }

    [StatistleProperty(Label = "Mean response upper", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The upper end of the confidence interval on the mean response at x₀.")]
    public double MeanUpper
    {
        get => _meanUpper;
        set => SetProperty(ref _meanUpper, value);
    }

    [StatistleProperty(Label = "New observation lower", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The prediction interval for one new observation at x₀: ŷ₀ ± t·σ̂√(1 + 1/n + " +
               "(x₀ − x̄)²/S_xx) — wider than the mean's interval by the scatter of single values.")]
    public double NewLower
    {
        get => _newLower;
        set => SetProperty(ref _newLower, value);
    }

    [StatistleProperty(Label = "New observation upper", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The upper end of the prediction interval at x₀.")]
    public double NewUpper
    {
        get => _newUpper;
        set => SetProperty(ref _newUpper, value);
    }

    // ── Diagnostics ───────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Durbin–Watson", Decimals = 4, Role = VariableRole.Calculated,
        Help = "Σ(eᵢ − eᵢ₋₁)²/Σeᵢ², from 0 to 4: near 2 the residuals are independent; well " +
               "below 2 successive residuals are alike (a trend the model misses, or time " +
               "order in the data). Python statsmodels durbin_watson; R by the formula.")]
    public double DurbinWatson
    {
        get => _durbinWatson;
        set => SetProperty(ref _durbinWatson, value);
    }

    [StatistleProperty(Label = "Shapiro–Wilk W of residuals", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The normality check of the residuals, which the t and F tests assume. Read with the Q–Q plot.")]
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
        Help = "How much the fit moves when the most influential observation is left out. Above " +
               "about 1 (some say 4/n) the point deserves a look: an error, or a real extreme " +
               "that steers the line.")]
    public double MaxCooksD
    {
        get => _maxCooksD;
        set => SetProperty(ref _maxCooksD, value);
    }

    [StatistleProperty(Label = "Most influential observation", Role = VariableRole.Calculated,
        Help = "Which pair has the largest Cook's distance, by its position in the data and its x.")]
    public string Influential
    {
        get => _influential;
        set => SetProperty(ref _influential, value);
    }

    [StatistleProperty(Label = "Largest leverage", Decimals = 4, Role = VariableRole.Calculated,
        Help = "The largest diagonal of the hat matrix, hᵢᵢ: how far an x sits from the others. " +
               "Leverages average 2/n; above 2·(2/n) is high.")]
    public double MaxLeverage
    {
        get => _maxLeverage;
        set => SetProperty(ref _maxLeverage, value);
    }

    // ── Calculation ───────────────────────────────────────────────────────────

    /// <summary>The fitted pairs after transformation, in original units, in the order fitted.</summary>
    [StatistleIgnore]
    public (double[] X, double[] Y) Fitted { get; private set; } = ([], []);

    public override void Recalculate()
    {
        string? note = DataSources.PullPairs(this, XColumn, YColumn, Data);
        string? problem = Fit();
        Problem = note is null ? problem : problem is null ? note : note + " " + problem;
    }

    /// <summary>Fits from whatever pairs are in Data; returns why it could not.</summary>
    private string? Fit()
    {
        Model = null;
        Count = Skipped = 0;
        Equation = Influential = string.Empty;
        Intercept = Slope = SlopeSe = SlopeT = SlopeP = SlopeLower = SlopeUpper = InterceptSe = InterceptP = double.NaN;
        Correlation = RSquared = AdjustedRSquared = ResidualSe = SsRegression = SsError = SsTotal = FStatistic = FP = double.NaN;
        FittedAt = MeanLower = MeanUpper = NewLower = NewUpper = double.NaN;
        DurbinWatson = ShapiroW = ShapiroP = MaxCooksD = MaxLeverage = double.NaN;
        Fitted = ([], []);
        List<double> xs = [], ys = [], xt = [], yt = [];

        foreach (DataPoint p in Data.Points)
        {
            if (double.IsNaN(p.X) || double.IsNaN(p.Y))
            {
                continue;
            }

            if (!Transformations.Accepts(XTransformation, p.X) || !Transformations.Accepts(YTransformation, p.Y))
            {
                Skipped++;
                continue;
            }

            xs.Add(p.X);
            ys.Add(p.Y);
            xt.Add(Transformations.Apply(XTransformation, p.X));
            yt.Add(Transformations.Apply(YTransformation, p.Y));
        }

        Count = xs.Count;

        if (Count < 3)
        {
            return "At least three usable pairs are needed for a line and its standard error.";
        }

        if (double.IsNaN(Confidence) || Confidence <= 0 || Confidence >= 1)
        {
            return "Enter a confidence level between 0 and 1.";
        }

        string xName = Transformations.Symbol(XTransformation, "x"), yName = Transformations.Symbol(YTransformation, "y");
        Model = LinearModel.Fit([xName], [xt], yt, Confidence);

        if (Model is null)
        {
            return "The x values have no spread, so no line can be fitted.";
        }

        Fitted = ([.. xs], [.. ys]);
        LinearModel m = Model;
        Coefficient b0 = m.Coefficients[0], b1 = m.Coefficients[1];
        Intercept = b0.Estimate;
        Slope = b1.Estimate;
        SlopeSe = b1.StandardError;
        SlopeT = b1.T;
        SlopeP = b1.PValue;
        SlopeLower = b1.Lower;
        SlopeUpper = b1.Upper;
        InterceptSe = b0.StandardError;
        InterceptP = b0.PValue;
        RSquared = m.RSquared;
        Correlation = Math.Sign(Slope) * Math.Sqrt(Math.Max(0, m.RSquared));
        AdjustedRSquared = m.AdjustedRSquared;
        ResidualSe = m.ResidualStandardError;
        SsRegression = m.SumSquaresRegression;
        SsError = m.SumSquaresError;
        SsTotal = m.SumSquaresTotal;
        FStatistic = m.F;
        FP = m.FPValue;
        Equation = $"{yName} = {Intercept:G6} {(Slope < 0 ? "−" : "+")} {Math.Abs(Slope):G6}·{xName}";
        DurbinWatson = m.DurbinWatson;
        AssumptionCheck normality = Nonparametric.ShapiroWilk(m.Residuals);
        ShapiroW = normality.Statistic;
        ShapiroP = normality.PValue;
        int worst = Array.IndexOf(m.CooksDistance, m.CooksDistance.Max());
        MaxCooksD = m.CooksDistance[worst];
        Influential = $"observation {worst + 1} (x = {xs[worst]:G6}, y = {ys[worst]:G6})";
        MaxLeverage = m.Leverage.Max();

        if (!double.IsNaN(PredictAt))
        {
            if (!Transformations.Accepts(XTransformation, PredictAt))
            {
                return $"x₀ = {PredictAt:G6} cannot be transformed by {Transformations.Symbol(XTransformation, "x")}.";
            }

            Prediction at = m.Predict([Transformations.Apply(XTransformation, PredictAt)]);
            FittedAt = Transformations.Undo(YTransformation, at.Fitted);
            (MeanLower, MeanUpper) = Ordered(Transformations.Undo(YTransformation, at.MeanLower), Transformations.Undo(YTransformation, at.MeanUpper));
            (NewLower, NewUpper) = Ordered(Transformations.Undo(YTransformation, at.ObservationLower), Transformations.Undo(YTransformation, at.ObservationUpper));
        }

        return null;
    }

    private static (double, double) Ordered(double a, double b) => a <= b ? (a, b) : (b, a);

    /// <summary>The fitted curve in original units: y = g⁻¹(b₀ + b₁·f(x)).</summary>
    public double FittedValue(double x) =>
        Model is null || !Transformations.Accepts(XTransformation, x) ? double.NaN
        : Transformations.Undo(YTransformation, Intercept + Slope * Transformations.Apply(XTransformation, x));

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName is nameof(XColumn) or nameof(YColumn) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) => propertyName switch
    {
        nameof(XColumn) => DataSources.Table(this, XColumn),
        nameof(YColumn) => DataSources.Table(this, YColumn),
        nameof(Data) => DataSources.Table(this, XColumn) ?? DataSources.Table(this, YColumn),
        _ => null,
    };

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (Model is null)
        {
            return Data.Count == 0 ? [] : [RegressionFigures.Scatter($"{Name} — observations", "x", "y", Data.XValues, Data.YValues, null)];
        }

        return
        [
            RegressionFigures.Scatter($"{Name} — {Equation}", "x", "y", Fitted.X, Fitted.Y, FittedValue),
            RegressionFigures.ResidualsVersusFitted($"{Name} — residuals against fitted values", Model),
            RegressionFigures.NormalQq($"{Name} — normal Q–Q plot of residuals", Model),
        ];
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        s.Imports(scipyStats: true, statsmodels: true);
        s.Either("from statsmodels.stats.stattools import durbin_watson", "# Base R plus the formulas; car::durbinWatonTest and car::vif give the same diagnostics");
        s.Line();
        s.Comment("The observations");
        s.Vector("x", Fitted.X.Length > 0 ? Fitted.X : Data.XValues).Vector("y", Fitted.Y.Length > 0 ? Fitted.Y : Data.YValues);
        s.Assign("alpha", Math.Round(1 - Confidence, 10));
        string xt = Transformations.Python(XTransformation, "x"), yt = Transformations.Python(YTransformation, "y");
        string xr = Transformations.R(XTransformation, "x"), yr = Transformations.R(YTransformation, "y");

        if (XTransformation != Transformation.None || YTransformation != Transformation.None)
        {
            s.Comment("The transformation (Module 13); the fit is linear in the transformed variables");
        }

        s.Either($"xt, yt = {xt}, {yt}", $"xt <- {xr}; yt <- {yr}");
        s.Either("fit = sm.OLS(yt, sm.add_constant(xt)).fit()", "fit <- lm(yt ~ xt)");
        s.Either("b0, b1 = fit.params", "b0 <- coef(fit)[[1]]; b1 <- coef(fit)[[2]]");
        s.Either("ci = fit.conf_int(alpha)", "ci <- confint(fit, level = 1 - alpha)");
        s.Either("n = int(fit.nobs)", "n <- length(x)");
        s.Line();
        s.PrintRaw("Observations (n)", "n", "n");
        s.PrintRaw("Pairs skipped by the transform", Skipped.ToString(CultureInfo.InvariantCulture), Skipped.ToString(CultureInfo.InvariantCulture));
        s.PrintText("Equation", Equation);
        s.Print("Intercept (b₀)", "b0", "b0", 4);
        s.Print("Slope (b₁)", "b1", "b1", 4);
        s.Print("Slope standard error", "fit.bse[1]", "summary(fit)$coefficients[2, 2]", 4);
        s.Print("Slope t", "fit.tvalues[1]", "summary(fit)$coefficients[2, 3]", 4);
        s.Print("Slope p-value", "fit.pvalues[1]", "summary(fit)$coefficients[2, 4]", 4);
        s.Print("Slope confidence interval lower", "ci[1, 0]", "ci[2, 1]", 4);
        s.Print("Slope confidence interval upper", "ci[1, 1]", "ci[2, 2]", 4);
        s.Print("Intercept standard error", "fit.bse[0]", "summary(fit)$coefficients[1, 2]", 4);
        s.Print("Intercept p-value", "fit.pvalues[0]", "summary(fit)$coefficients[1, 4]", 4);
        s.Print("Correlation (r)", "np.sign(b1) * np.sqrt(fit.rsquared)", "cor(xt, yt)", 4);
        s.Print("R²", "fit.rsquared", "summary(fit)$r.squared", 4);
        s.Print("Adjusted R²", "fit.rsquared_adj", "summary(fit)$adj.r.squared", 4);
        s.Print("Residual standard error (σ̂)", "np.sqrt(fit.mse_resid)", "summary(fit)$sigma", 4);
        s.Print("SS regression", "fit.ess", "sum((fitted(fit) - mean(yt))^2)", 4);
        s.Print("SS error", "fit.ssr", "sum(residuals(fit)^2)", 4);
        s.Print("SS total", "fit.centered_tss", "sum((yt - mean(yt))^2)", 4);
        s.Print("ANOVA F", "fit.fvalue", "summary(fit)$fstatistic[[1]]", 4);
        s.Print("ANOVA p-value", "fit.f_pvalue", "anova(fit)$`Pr(>F)`[1]", 4);

        if (!double.IsNaN(FittedAt))
        {
            s.Line();
            s.Comment("Prediction at x₀: the interval on the mean response and on a new observation");
            s.Assign("x0", PredictAt);
            s.Either($"pred = fit.get_prediction(np.array([[1.0, {Transformations.Python(XTransformation, "x0")}]])).summary_frame(alpha=alpha)",
                     $"new <- data.frame(xt = {Transformations.R(XTransformation, "x0")})");
            s.Either(string.Empty, "pm <- predict(fit, new, interval = \"confidence\", level = 1 - alpha); po <- predict(fit, new, interval = \"prediction\", level = 1 - alpha)");
            string UndoPy(string e) => Transformations.PythonUndo(YTransformation, e);
            string UndoR(string e) => Transformations.RUndo(YTransformation, e);
            s.Print("Fitted y at x₀", UndoPy("pred['mean'].iloc[0]"), UndoR("pm[1, 1]"), 4);
            s.Print("Mean response lower", $"min({UndoPy("pred['mean_ci_lower'].iloc[0]")}, {UndoPy("pred['mean_ci_upper'].iloc[0]")})", $"min({UndoR("pm[1, 2]")}, {UndoR("pm[1, 3]")})", 4);
            s.Print("Mean response upper", $"max({UndoPy("pred['mean_ci_lower'].iloc[0]")}, {UndoPy("pred['mean_ci_upper'].iloc[0]")})", $"max({UndoR("pm[1, 2]")}, {UndoR("pm[1, 3]")})", 4);
            s.Print("New observation lower", $"min({UndoPy("pred['obs_ci_lower'].iloc[0]")}, {UndoPy("pred['obs_ci_upper'].iloc[0]")})", $"min({UndoR("po[1, 2]")}, {UndoR("po[1, 3]")})", 4);
            s.Print("New observation upper", $"max({UndoPy("pred['obs_ci_lower'].iloc[0]")}, {UndoPy("pred['obs_ci_upper'].iloc[0]")})", $"max({UndoR("po[1, 2]")}, {UndoR("po[1, 3]")})", 4);
        }

        s.Line();
        s.Comment("Diagnostics (Module 13)");
        s.Either("resid = fit.resid", "resid <- residuals(fit)");
        s.Print("Durbin–Watson", "durbin_watson(resid)", "sum(diff(resid)^2) / sum(resid^2)", 4);
        s.Either("w, p_w = stats.shapiro(resid)", "sw <- shapiro.test(resid); w <- sw$statistic; p_w <- sw$p.value");
        s.Print("Shapiro–Wilk W of residuals", "w", "w", 4);
        s.Print("Shapiro–Wilk p of residuals", "p_w", "p_w", 4);
        s.Either("influence = fit.get_influence()", "cd <- cooks.distance(fit); h <- hatvalues(fit)");
        s.Print("Largest Cook's distance", "influence.cooks_distance[0].max()", "max(cd)", 4);
        s.Either("print(\"Most influential observation: \" + str(int(np.argmax(influence.cooks_distance[0])) + 1))", "cat(\"Most influential observation:\", which.max(cd), \"\\n\")");
        s.Print("Largest leverage", "influence.hat_matrix_diag.max()", "max(h)", 4);
        s.Line();
        s.Either("print(fit.summary())", "print(summary(fit))");

        return s.ToString();
    }
}

/// <summary>While columns are chosen, the pairs come from them and are locked.</summary>
public sealed class SimpleRegressionSource : CalculationMethod<SimpleRegression>
{
    public override string Name => "Data table columns";

    public override string Description => "The (x, y) pairs are copied from the chosen columns, paired row by row.";

    public override string Citation => "Statistics+ project data.";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(SimpleRegression.Data)];

    public override IReadOnlyList<string> Generates => [nameof(SimpleRegression.Data)];

    public override bool IsApplicableTo(SimpleRegression subject) =>
        subject is not null && (DataSources.Split(subject.XColumn).TableId > 0 || DataSources.Split(subject.YColumn).TableId > 0);

    public override void Apply(SimpleRegression subject) => subject.Recalculate();
}

/// <summary>The least-squares fit and everything read off it.</summary>
public sealed class LeastSquaresLine : CalculationMethod<SimpleRegression>
{
    public override string Name => "Least-squares line";

    public override string Description =>
        "b₁ = S_xy/S_xx, b₀ = ȳ − b₁x̄; σ̂² = SS_E/(n − 2); se(b₁) = σ̂/√S_xx; t = b₁/se(b₁) on n − 2 degrees of " +
        "freedom; R² = SS_R/SS_T; intervals on the mean response and a new observation at x₀; residual " +
        "diagnostics; optional transformation of x or y.";

    public override string Citation =>
        "EGN3443 Modules 11 and 13; Montgomery, D. C. and Runger, G. C., Applied Statistics and Probability " +
        "for Engineers, 7th ed., 2018, Chapter 11 (simple linear regression and correlation) and §12-5 " +
        "(residual analysis, influential observations).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(SimpleRegression.Data), nameof(SimpleRegression.XTransformation), nameof(SimpleRegression.YTransformation), nameof(SimpleRegression.Confidence), nameof(SimpleRegression.PredictAt)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.Count), nameof(SimpleRegression.Skipped), nameof(SimpleRegression.Equation),
        nameof(SimpleRegression.Intercept), nameof(SimpleRegression.Slope), nameof(SimpleRegression.SlopeSe), nameof(SimpleRegression.SlopeT),
        nameof(SimpleRegression.SlopeP), nameof(SimpleRegression.SlopeLower), nameof(SimpleRegression.SlopeUpper),
        nameof(SimpleRegression.InterceptSe), nameof(SimpleRegression.InterceptP), nameof(SimpleRegression.Correlation),
        nameof(SimpleRegression.RSquared), nameof(SimpleRegression.AdjustedRSquared), nameof(SimpleRegression.ResidualSe),
        nameof(SimpleRegression.SsRegression), nameof(SimpleRegression.SsError), nameof(SimpleRegression.SsTotal),
        nameof(SimpleRegression.FStatistic), nameof(SimpleRegression.FP),
        nameof(SimpleRegression.FittedAt), nameof(SimpleRegression.MeanLower), nameof(SimpleRegression.MeanUpper),
        nameof(SimpleRegression.NewLower), nameof(SimpleRegression.NewUpper),
        nameof(SimpleRegression.DurbinWatson), nameof(SimpleRegression.ShapiroW), nameof(SimpleRegression.ShapiroP),
        nameof(SimpleRegression.MaxCooksD), nameof(SimpleRegression.Influential), nameof(SimpleRegression.MaxLeverage),
    ];

    public override void Apply(SimpleRegression subject) => subject.Recalculate();
}
