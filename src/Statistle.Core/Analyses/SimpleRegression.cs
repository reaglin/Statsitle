using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Statistics;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

/// <summary>
/// Least-squares regression of y on x. The straight line is the one the course calls simple
/// linear regression; the five transformed forms — exponential, power, logarithmic, reciprocal
/// in x, reciprocal in y — are fitted alongside it, each giving its equation, r² and the
/// standard error of the estimate in y units, so a curved relation can be recognised rather
/// than forced straight. The chosen form is what the plot draws through the data.
///
/// Each form is its own <see cref="ICalculationMethod"/> writing exactly its three outputs.
/// </summary>
[StatistleHelp(
    "Fits y against x by least squares. The linear fit y = a + b·x is simple linear regression: " +
    "b is how much y changes per unit of x, a is the intercept, and r squared is the fraction " +
    "of the variation in y the line accounts for. Five transformed forms are fitted at the same " +
    "time so a curved relation is not forced straight; each is a straight line after a " +
    "transform, so one routine serves all six. Choose an x column and a y column from a data " +
    "table, or type the pairs into Data, one x, y pair per line. Compare forms by standard " +
    "error of the estimate, which is in y units and so is the fair comparison — r squared is " +
    "computed in each form's transformed variables and is not comparable across them. Plot form " +
    "chooses which curve the Plot tab draws through the points.")]
public sealed class SimpleRegression : Analysis, IHasFigures, IUsesProject, IHasChoices, ILinksTo, IReportsProblem
{
    private readonly Dictionary<RegressionForm, RegressionResult?> _results = [];

    private string _xColumn = string.Empty;
    private string _yColumn = string.Empty;
    private RegressionForm _selectedForm = RegressionForm.Linear;
    private int _count;
    private string _bestForm = string.Empty;
    private string _linearEquation = string.Empty;
    private double _linearRSquared;
    private double _linearStandardError;
    private string _exponentialEquation = string.Empty;
    private double _exponentialRSquared;
    private double _exponentialStandardError;
    private string _powerEquation = string.Empty;
    private double _powerRSquared;
    private double _powerStandardError;
    private string _logarithmicEquation = string.Empty;
    private double _logarithmicRSquared;
    private double _logarithmicStandardError;
    private string _reciprocalEquation = string.Empty;
    private double _reciprocalRSquared;
    private double _reciprocalStandardError;
    private string _reciprocalYEquation = string.Empty;
    private double _reciprocalYRSquared;
    private double _reciprocalYStandardError;

    public SimpleRegression()
    {
        Name = "Simple linear regression";

        // A short record so the analysis shows something the moment it is opened: roughly y ∝ x^1.5.
        double[] xs = [1, 2, 3, 4, 5, 6, 8, 10];
        double[] ys = [1.1, 2.7, 5.4, 7.9, 11.4, 14.5, 22.9, 31.2];
        for (int i = 0; i < xs.Length; i++)
        {
            Data.Add(xs[i], ys[i]);
        }

        Recalculate();
    }

    public override string Title => "Simple linear regression";

    public override string Summary => "Least-squares fit of y on x, with five curved forms beside it";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 40;

    // ── Inputs ────────────────────────────────────────────────────────────────

    [StatistleIgnore]
    public StatistleDocument? Project { get; set; }

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleProperty(Label = "x column", Role = VariableRole.Input,
        Help = "Which column of which data table holds x, the predictor — the quantity you set " +
               "or measure first. Choose the y column too and the pairs are read row by row. " +
               "'Entered below' types the pairs into this analysis instead.")]
    public string XColumn
    {
        get => _xColumn;
        set => SetProperty(ref _xColumn, value ?? string.Empty);
    }

    [StatistleProperty(Label = "y column", Role = VariableRole.Input,
        Help = "Which column holds y, the response — the quantity you expect to depend on x. " +
               "Where the two columns differ in length only the rows they have in common are " +
               "paired, and the analysis says so.")]
    public string YColumn
    {
        get => _yColumn;
        set => SetProperty(ref _yColumn, value ?? string.Empty);
    }

    /// <summary>The observations, one (x, y) pair per line.</summary>
    [StatistleProperty(Label = "Data (x, y)", ReportLabel = "Observations (x, y)", Role = VariableRole.Input,
        Help = "The observations, one pair per line: x then y, separated by a tab, a comma or a " +
               "semicolon (a space works too). Paste two columns straight from a spreadsheet or a " +
               "CSV file; a line that is not two numbers rejects the paste and keeps the old data. " +
               "At least three pairs are needed.")]
    public DataSeries Data { get; } = new();

    /// <summary>The form drawn through the data on the plot, and named in the report.</summary>
    [StatistleProperty(Label = "Plot form", ReportLabel = "Form plotted", Role = VariableRole.Design,
        Help = "Which fitted form the Plot tab draws through the data, and which the report names.")]
    public RegressionForm SelectedForm
    {
        get => _selectedForm;
        set => SetProperty(ref _selectedForm, value);
    }

    // ── Summary ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Observations", Decimals = 0, Unit = ValueKind.Count,
        Role = VariableRole.Calculated,
        Help = "How many x, y pairs were read.")]
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    /// <summary>The form with the smallest standard error in y units — the fair comparison.</summary>
    [StatistleProperty(Label = "Best form (lowest std. error)", ReportLabel = "Best form by standard error",
        Role = VariableRole.Output,
        Help = "The form with the smallest standard error of the estimate in y units - the one that " +
               "predicts y most closely over these data. It is not automatically the right one: " +
               "prefer the form your theory suggests unless the data argue strongly otherwise.")]
    public string BestForm
    {
        get => _bestForm;
        set => SetProperty(ref _bestForm, value);
    }

    // ── One block per form: equation, r², standard error ─────────────────────

    [StatistleProperty(Label = "Linear  y = a + b·x", ReportLabel = "Linear, y = a + b·x", Role = VariableRole.Output,
        Help = "Simple linear regression: the least-squares straight line. b is the change in y " +
               "per unit change in x; a is the value of y the line gives at x = 0, which is only " +
               "meaningful if x = 0 is within the range of your data.")]
    public string LinearEquation { get => _linearEquation; set => SetProperty(ref _linearEquation, value); }

    [StatistleProperty(Label = "Linear r²", Decimals = 4, Role = VariableRole.Output,
        Help = "The coefficient of determination: the fraction of the variation in y the straight " +
               "line accounts for, between 0 and 1. It measures fit, not causation, and a high " +
               "value over a narrow range of x says little about behaviour outside it.")]
    public double LinearRSquared { get => _linearRSquared; set => SetProperty(ref _linearRSquared, value); }

    [StatistleProperty(Label = "Linear std. error", ReportLabel = "Linear standard error of estimate",
        Decimals = 4, Role = VariableRole.Output,
        Help = "The standard error of the estimate, √(Σ(y - ŷ)² / (n - 2)): the typical size of a " +
               "residual, in y units. Compare forms by this.")]
    public double LinearStandardError { get => _linearStandardError; set => SetProperty(ref _linearStandardError, value); }

    [StatistleProperty(Label = "Exponential  y = a·e^(b·x)", ReportLabel = "Exponential, y = a·e^(b·x)",
        Role = VariableRole.Output,
        Help = "Fitted as ln y against x, so y must be positive. The form for a quantity that " +
               "grows or decays by a constant fraction per unit of x.")]
    public string ExponentialEquation { get => _exponentialEquation; set => SetProperty(ref _exponentialEquation, value); }

    [StatistleProperty(Label = "Exponential r²", Decimals = 4, Role = VariableRole.Output,
        Help = "r² of the straight line of ln y against x. Not comparable with another form's r².")]
    public double ExponentialRSquared { get => _exponentialRSquared; set => SetProperty(ref _exponentialRSquared, value); }

    [StatistleProperty(Label = "Exponential std. error", ReportLabel = "Exponential standard error of estimate",
        Decimals = 4, Role = VariableRole.Output,
        Help = "The standard error of the estimate in y units, so it can be compared with the other forms.")]
    public double ExponentialStandardError { get => _exponentialStandardError; set => SetProperty(ref _exponentialStandardError, value); }

    [StatistleProperty(Label = "Power  y = a·x^b", ReportLabel = "Power, y = a·x^b", Role = VariableRole.Output,
        Help = "Fitted as ln y against ln x, so both must be positive. The form most physical " +
               "scaling laws take; b is the exponent.")]
    public string PowerEquation { get => _powerEquation; set => SetProperty(ref _powerEquation, value); }

    [StatistleProperty(Label = "Power r²", Decimals = 4, Role = VariableRole.Output,
        Help = "r² of the straight line of ln y against ln x.")]
    public double PowerRSquared { get => _powerRSquared; set => SetProperty(ref _powerRSquared, value); }

    [StatistleProperty(Label = "Power std. error", ReportLabel = "Power standard error of estimate",
        Decimals = 4, Role = VariableRole.Output,
        Help = "The standard error of the estimate in y units.")]
    public double PowerStandardError { get => _powerStandardError; set => SetProperty(ref _powerStandardError, value); }

    [StatistleProperty(Label = "Logarithmic  y = a + b·ln x", ReportLabel = "Logarithmic, y = a + b·ln x",
        Role = VariableRole.Output,
        Help = "Fitted as y against ln x, so x must be positive. The form for a response that " +
               "keeps rising but by less and less.")]
    public string LogarithmicEquation { get => _logarithmicEquation; set => SetProperty(ref _logarithmicEquation, value); }

    [StatistleProperty(Label = "Logarithmic r²", Decimals = 4, Role = VariableRole.Output,
        Help = "r² of the straight line of y against ln x.")]
    public double LogarithmicRSquared { get => _logarithmicRSquared; set => SetProperty(ref _logarithmicRSquared, value); }

    [StatistleProperty(Label = "Logarithmic std. error", ReportLabel = "Logarithmic standard error of estimate",
        Decimals = 4, Role = VariableRole.Output,
        Help = "The standard error of the estimate in y units.")]
    public double LogarithmicStandardError { get => _logarithmicStandardError; set => SetProperty(ref _logarithmicStandardError, value); }

    [StatistleProperty(Label = "Reciprocal  y = a + b / x", ReportLabel = "Reciprocal, y = a + b / x",
        Role = VariableRole.Output,
        Help = "Fitted as y against 1/x, so x must not be zero. The form for a response that " +
               "levels off towards a as x grows.")]
    public string ReciprocalEquation { get => _reciprocalEquation; set => SetProperty(ref _reciprocalEquation, value); }

    [StatistleProperty(Label = "Reciprocal r²", Decimals = 4, Role = VariableRole.Output,
        Help = "r² of the straight line of y against 1/x.")]
    public double ReciprocalRSquared { get => _reciprocalRSquared; set => SetProperty(ref _reciprocalRSquared, value); }

    [StatistleProperty(Label = "Reciprocal std. error", ReportLabel = "Reciprocal standard error of estimate",
        Decimals = 4, Role = VariableRole.Output,
        Help = "The standard error of the estimate in y units.")]
    public double ReciprocalStandardError { get => _reciprocalStandardError; set => SetProperty(ref _reciprocalStandardError, value); }

    [StatistleProperty(Label = "Reciprocal-y  y = 1 / (a + b·x)", ReportLabel = "Reciprocal-y, y = 1 / (a + b·x)",
        Role = VariableRole.Output,
        Help = "Fitted as 1/y against x, so y must not be zero. The form for a response that " +
               "approaches a ceiling.")]
    public string ReciprocalYEquation { get => _reciprocalYEquation; set => SetProperty(ref _reciprocalYEquation, value); }

    [StatistleProperty(Label = "Reciprocal-y r²", Decimals = 4, Role = VariableRole.Output,
        Help = "r² of the straight line of 1/y against x.")]
    public double ReciprocalYRSquared { get => _reciprocalYRSquared; set => SetProperty(ref _reciprocalYRSquared, value); }

    [StatistleProperty(Label = "Reciprocal-y std. error", ReportLabel = "Reciprocal-y standard error of estimate",
        Decimals = 4, Role = VariableRole.Output,
        Help = "The standard error of the estimate in y units.")]
    public double ReciprocalYStandardError { get => _reciprocalYStandardError; set => SetProperty(ref _reciprocalYStandardError, value); }

    /// <summary>Every form's fit, for the figure and for tests. Not persisted — derived.</summary>
    [StatistleIgnore]
    public IReadOnlyDictionary<RegressionForm, RegressionResult?> Results => _results;

    /// <summary>The data with the selected form drawn through it, or null with nothing fitted.</summary>
    [StatistleIgnore]
    public PlotSpec? Figure =>
        Data.Count == 0
            ? null
            : RegressionFigure.Build(Data, _results.GetValueOrDefault(SelectedForm), Name);

    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units) =>
        Figure is PlotSpec figure ? [figure] : [];

    public IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName) =>
        propertyName is nameof(XColumn) or nameof(YColumn) ? DataSources.ColumnChoices(this) : [];

    public StatistleObject? LinkedObject(string propertyName) => propertyName switch
    {
        nameof(XColumn) or nameof(Data) => DataSources.Table(this, XColumn),
        nameof(YColumn) => DataSources.Table(this, YColumn),
        _ => null,
    };

    public override void Recalculate()
    {
        Problem = DataSources.PullPairs(this, XColumn, YColumn, Data);
        Count = Data.Count;

        foreach (ICalculationMethod method in MethodCatalog.For(typeof(SimpleRegression)))
        {
            method.Apply(this);
        }

        RegressionResult? best = _results.Values
            .Where(r => r is not null && double.IsFinite(r.StandardError))
            .OrderBy(r => r!.StandardError)
            .FirstOrDefault();

        BestForm = best is null ? string.Empty : $"{Regression.Template(best.Form)}  ({best.Form})";
    }

    /// <summary>Writes one form's fit into its three properties. Called by that form's method.</summary>
    public void SetResult(RegressionForm form, RegressionResult? result)
    {
        _results[form] = result;

        string equation = result?.Equation ?? "needs at least 3 usable points";
        if (result is { PointsSkipped: > 0 })
        {
            equation += $"  ({result.PointsSkipped} point(s) skipped)";
        }

        double r2 = result?.RSquared ?? 0;
        double se = result is null || !double.IsFinite(result.StandardError) ? 0 : result.StandardError;

        switch (form)
        {
            case RegressionForm.Linear:
                (LinearEquation, LinearRSquared, LinearStandardError) = (equation, r2, se);
                break;
            case RegressionForm.Exponential:
                (ExponentialEquation, ExponentialRSquared, ExponentialStandardError) = (equation, r2, se);
                break;
            case RegressionForm.Power:
                (PowerEquation, PowerRSquared, PowerStandardError) = (equation, r2, se);
                break;
            case RegressionForm.Logarithmic:
                (LogarithmicEquation, LogarithmicRSquared, LogarithmicStandardError) = (equation, r2, se);
                break;
            case RegressionForm.Reciprocal:
                (ReciprocalEquation, ReciprocalRSquared, ReciprocalStandardError) = (equation, r2, se);
                break;
            case RegressionForm.ReciprocalY:
                (ReciprocalYEquation, ReciprocalYRSquared, ReciprocalYStandardError) = (equation, r2, se);
                break;
            default:
                break;
        }
    }
}

/// <summary>Base for the six forms, so each concrete one is only its identity.</summary>
public abstract class RegressionMethod : CalculationMethod<SimpleRegression>
{
    public abstract RegressionForm Form { get; }

    public override string Description => $"Least squares on the straight line behind {Regression.Template(Form)}.";

    public override string Citation =>
        "Montgomery, D. C. and Runger, G. C., Applied Statistics and Probability for Engineers, " +
        "7th ed., Wiley, 2018, chapter 11 (simple linear regression and transformations to linearity).";

    public override IReadOnlyList<string> Inputs => [nameof(SimpleRegression.Data)];

    public override void Apply(SimpleRegression s)
    {
        ArgumentNullException.ThrowIfNull(s);

        s.SetResult(Form, Regression.Fit(Form, s.Data.XValues, s.Data.YValues));
    }
}

public sealed class LinearRegressionMethod : RegressionMethod
{
    public override string Name => "Simple linear regression";

    public override RegressionForm Form => RegressionForm.Linear;

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.LinearEquation),
        nameof(SimpleRegression.LinearRSquared),
        nameof(SimpleRegression.LinearStandardError),
    ];
}

public sealed class ExponentialRegressionMethod : RegressionMethod
{
    public override string Name => "Exponential regression";

    public override RegressionForm Form => RegressionForm.Exponential;

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.ExponentialEquation),
        nameof(SimpleRegression.ExponentialRSquared),
        nameof(SimpleRegression.ExponentialStandardError),
    ];
}

public sealed class PowerRegressionMethod : RegressionMethod
{
    public override string Name => "Power regression";

    public override RegressionForm Form => RegressionForm.Power;

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.PowerEquation),
        nameof(SimpleRegression.PowerRSquared),
        nameof(SimpleRegression.PowerStandardError),
    ];
}

public sealed class LogarithmicRegressionMethod : RegressionMethod
{
    public override string Name => "Logarithmic regression";

    public override RegressionForm Form => RegressionForm.Logarithmic;

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.LogarithmicEquation),
        nameof(SimpleRegression.LogarithmicRSquared),
        nameof(SimpleRegression.LogarithmicStandardError),
    ];
}

public sealed class ReciprocalRegressionMethod : RegressionMethod
{
    public override string Name => "Reciprocal regression";

    public override RegressionForm Form => RegressionForm.Reciprocal;

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.ReciprocalEquation),
        nameof(SimpleRegression.ReciprocalRSquared),
        nameof(SimpleRegression.ReciprocalStandardError),
    ];
}

public sealed class ReciprocalYRegressionMethod : RegressionMethod
{
    public override string Name => "Reciprocal-y regression";

    public override RegressionForm Form => RegressionForm.ReciprocalY;

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(SimpleRegression.ReciprocalYEquation),
        nameof(SimpleRegression.ReciprocalYRSquared),
        nameof(SimpleRegression.ReciprocalYStandardError),
    ];
}

/// <summary>While a pair of project columns is chosen, the pairs come from them and are locked.</summary>
public sealed class RegressionSourceMethod : CalculationMethod<SimpleRegression>
{
    public override string Name => "Data table columns";

    public override string Description => "The (x, y) pairs are read row by row from the chosen columns.";

    public override string Citation => "Statistle project data.";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [nameof(SimpleRegression.Data)];

    public override IReadOnlyList<string> Generates => [nameof(SimpleRegression.Data)];

    public override bool IsApplicableTo(SimpleRegression subject) =>
        subject is not null
        && DataSources.Split(subject.XColumn).TableId > 0
        && DataSources.Split(subject.YColumn).TableId > 0;

    public override void Apply(SimpleRegression subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        DataSources.PullPairs(subject, subject.XColumn, subject.YColumn, subject.Data);
    }
}
