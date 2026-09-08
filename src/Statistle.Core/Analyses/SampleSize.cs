using System.ComponentModel;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Inference;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum SizeFor
{
    [Description("A mean — σ known or estimated")]
    Mean = 0,

    [Description("A proportion")]
    Proportion,
}

/// <summary>EGN3443 Module 7: how many observations a confidence interval of a given width needs.</summary>
[StatistleHelp(
    "The sample size for a margin of error (Module 7). Say how close the estimate must come to " +
    "the true value (the margin of error E, half the width of the confidence interval), at what " +
    "confidence, and how variable the population is — σ for a mean, a guess at p for a " +
    "proportion (0.5 when nothing is known, the largest n) — and read the smallest n that " +
    "achieves it: n = (z·σ/E)² for a mean, n = (z/E)²·p(1 − p) for a proportion, rounded up. " +
    "Halving E quadruples n; a higher confidence raises z and n with it. The Code tab writes " +
    "the same two lines in Python and R.")]
public sealed class SampleSize : Analysis, IReportsProblem, ICodeSource
{
    private SizeFor _target = SizeFor.Mean;
    private double _confidence = 0.95;
    private double _margin = 0.5;
    private double _sigma = 1;
    private double _proportionGuess = 0.5;

    private double _critical = double.NaN;
    private int _sampleSizeNeeded;
    private string _note = string.Empty;

    public SampleSize()
    {
        Name = "Sample size";
        Recalculate();
    }

    public override string Title => "Sample size";

    public override string Summary => "How many observations a margin of error needs (Module 7)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 31;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public string? Problem { get; private set; }

    [StatistleProperty(Label = "Sample size for", Role = VariableRole.Design,
        Help = "Whether the interval is for a mean (a measurement) or a proportion (a fraction of items).")]
    public SizeFor Target
    {
        get => _target;
        set => SetProperty(ref _target, value);
    }

    [StatistleProperty(Label = "Confidence level", Decimals = 3, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0.5, Max = 0.9999,
        Help = "The confidence the interval is to have: 0.90, 0.95 (the usual) or 0.99. Higher " +
               "confidence means a larger z and a larger n for the same margin.")]
    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    [StatistleProperty(Label = "Margin of error (E)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "How far the estimate may be from the true value: the half-width of the interval, " +
               "in the units of the mean or as a proportion (0.05 for ±5 percentage points).")]
    public double Margin
    {
        get => _margin;
        set => SetProperty(ref _margin, value);
    }

    [StatistleProperty(Label = "Population standard deviation (σ)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The spread of the measurement — known from history, or a pilot sample's s, or " +
               "(range)/4 as a rough guess.")]
    public double Sigma
    {
        get => _sigma;
        set => SetProperty(ref _sigma, value);
    }

    [StatistleProperty(Label = "Expected proportion (p)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "A guess at the proportion from earlier data. Use 0.5 when nothing is known: " +
               "p(1 − p) is largest there, so the n is safe for any true p.")]
    public double ProportionGuess
    {
        get => _proportionGuess;
        set => SetProperty(ref _proportionGuess, value);
    }

    [StatistleProperty(Label = "Critical value z", Decimals = 4, Role = VariableRole.Calculated,
        Help = "z_{α/2} for the confidence level: 1.645 for 90 %, 1.960 for 95 %, 2.576 for 99 %.")]
    public double Critical
    {
        get => _critical;
        set => SetProperty(ref _critical, value);
    }

    [StatistleProperty(Label = "Sample size needed (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "The smallest whole n that keeps the margin of error at or below E — always " +
               "rounded up. Python and R: math.ceil / ceiling of the formula.")]
    public int SampleSizeNeeded
    {
        get => _sampleSizeNeeded;
        set => SetProperty(ref _sampleSizeNeeded, value);
    }

    [StatistleProperty(Label = "Note", Role = VariableRole.Output,
        Help = "The formula used, with the numbers in it.")]
    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public override void Recalculate()
    {
        Problem = null;
        Critical = double.NaN;
        SampleSizeNeeded = 0;
        Note = string.Empty;

        if (double.IsNaN(Confidence) || Confidence <= 0 || Confidence >= 1 || double.IsNaN(Margin) || Margin <= 0)
        {
            Problem = "Enter a confidence level between 0 and 1 and a margin of error greater than zero.";
            return;
        }

        Critical = Intervals.ZCritical(Confidence);

        if (Target == SizeFor.Mean)
        {
            if (double.IsNaN(Sigma) || Sigma <= 0)
            {
                Problem = "Enter the population standard deviation, greater than zero.";
                return;
            }

            SampleSizeNeeded = Intervals.SampleSizeForMean(Sigma, Margin, Confidence);
            Note = $"n = (z·σ/E)² = ({Critical:F3} × {Sigma:G4} / {Margin:G4})² = {Math.Pow(Critical * Sigma / Margin, 2):F2}, rounded up";
        }
        else
        {
            if (double.IsNaN(ProportionGuess) || ProportionGuess <= 0 || ProportionGuess >= 1)
            {
                Problem = "Enter an expected proportion between 0 and 1 (0.5 when unknown).";
                return;
            }

            SampleSizeNeeded = Intervals.SampleSizeForProportion(ProportionGuess, Margin, Confidence);
            Note = $"n = (z/E)²·p(1 − p) = ({Critical:F3} / {Margin:G4})² × {ProportionGuess:G4} × {1 - ProportionGuess:G4} = {Math.Pow(Critical / Margin, 2) * ProportionGuess * (1 - ProportionGuess):F2}, rounded up";
        }
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        s.Either("import math\nfrom scipy import stats", "# Base R");
        s.Assign("confidence", Confidence).Assign("E", Margin);
        s.Either("z = stats.norm.ppf(1 - (1 - confidence) / 2)", "z <- qnorm(1 - (1 - confidence) / 2)");
        s.Print("Critical value z", "z", "z", 4);

        if (Target == SizeFor.Mean)
        {
            s.Assign("sigma", Sigma);
            s.Comment("n = (z·σ/E)², rounded up");
            s.PrintRaw("Sample size needed (n)", "math.ceil((z * sigma / E) ** 2)", "ceiling((z * sigma / E)^2)");
        }
        else
        {
            s.Assign("p", ProportionGuess);
            s.Comment("n = (z/E)²·p(1 − p), rounded up; p = 0.5 is the conservative choice");
            s.PrintRaw("Sample size needed (n)", "math.ceil((z / E) ** 2 * p * (1 - p))", "ceiling((z / E)^2 * p * (1 - p))");
        }

        s.PrintText("Note", Note);
        return s.ToString();
    }
}

public abstract class SampleSizeMethod : CalculationMethod<SampleSize>
{
    public abstract SizeFor Selects { get; }

    public override bool IsApplicableTo(SampleSize subject) => subject is not null && subject.Target == Selects;

    public override string Citation =>
        "EGN3443 Module 7 (Point Estimation and Confidence Intervals); Montgomery, D. C. and Runger, G. C., " +
        "Applied Statistics and Probability for Engineers, 7th ed., 2018, §8-1.3 and §8-4.";

    public override IReadOnlyList<string> Outputs => [nameof(SampleSize.Critical), nameof(SampleSize.SampleSizeNeeded), nameof(SampleSize.Note)];

    public override void Apply(SampleSize subject) => subject.Recalculate();
}

public sealed class SampleSizeForMeanMethod : SampleSizeMethod
{
    public override SizeFor Selects => SizeFor.Mean;

    public override string Name => "Sample size for a mean";

    public override string Description => "n = (z_{α/2}·σ/E)².";

    public override IReadOnlyList<string> Inputs => [nameof(SampleSize.Confidence), nameof(SampleSize.Margin), nameof(SampleSize.Sigma)];
}

public sealed class SampleSizeForProportionMethod : SampleSizeMethod
{
    public override SizeFor Selects => SizeFor.Proportion;

    public override string Name => "Sample size for a proportion";

    public override string Description => "n = (z_{α/2}/E)²·p(1 − p).";

    public override IReadOnlyList<string> Inputs => [nameof(SampleSize.Confidence), nameof(SampleSize.Margin), nameof(SampleSize.ProportionGuess)];
}
