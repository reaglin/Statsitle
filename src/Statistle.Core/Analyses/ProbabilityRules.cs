using System.ComponentModel;
using System.Globalization;
using MathNet.Numerics;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

/// <summary>What is known about how the two events relate — each is a method with its own rule.</summary>
public enum EventRelationship
{
    [Description("Independent — P(A ∩ B) = P(A)·P(B)")]
    Independent = 0,

    [Description("Mutually exclusive — A and B cannot both occur")]
    MutuallyExclusive,

    [Description("P(A ∩ B) is known")]
    KnownIntersection,

    [Description("P(A | B) is known")]
    KnownConditional,

    [Description("P(B | A) and P(B | A′) are known — Bayes")]
    KnownLikelihoods,
}

/// <summary>
/// EGN3443 Module 3: the probability rules for two events — complement, addition,
/// multiplication, conditional probability, independence and Bayes — plus the counting rules.
/// </summary>
[StatistleHelp(
    "The probability rules for two events A and B (Module 3). Enter P(A) and P(B) and say how " +
    "the events relate — independent, mutually exclusive, or with P(A ∩ B), P(A | B), or the " +
    "likelihoods P(B | A) and P(B | A′) known — and the complements, the intersection, the " +
    "union, both conditional probabilities and an independence check follow from the addition " +
    "and multiplication rules. With the likelihoods known, P(B) comes from the total probability " +
    "rule and P(A | B) is Bayes' theorem — the 'given a positive test, what is the chance of " +
    "the fault' question. Below, the counting rules: the permutations and combinations of r " +
    "items chosen from n. The Code tab shows the same arithmetic in Python and R.")]
public sealed class ProbabilityRules : Analysis, IReportsProblem, ICodeSource
{
    private EventRelationship _relationship = EventRelationship.Independent;
    private double _probabilityA = 0.3;
    private double _probabilityB = 0.5;
    private double _intersection = 0.15;
    private double _conditionalAGivenB = 0.3;
    private double _likelihoodGivenA = 0.95;
    private double _likelihoodGivenNotA = 0.05;
    private double _items = 10;
    private double _chosen = 3;

    private double _complementA = double.NaN;
    private double _complementB = double.NaN;
    private double _probabilityBResult = double.NaN;
    private double _intersectionResult = double.NaN;
    private double _union = double.NaN;
    private double _aGivenB = double.NaN;
    private double _bGivenA = double.NaN;
    private string _independence = string.Empty;
    private double _permutations = double.NaN;
    private double _combinations = double.NaN;

    public ProbabilityRules()
    {
        Name = "Probability rules";
        Recalculate();
    }

    public override string Title => "Probability rules";

    public override string Summary => "Complement, addition, multiplication, conditional, Bayes and counting (Module 3)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 20;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public string? Problem { get; private set; }

    // ── Entries ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "How A and B relate", Role = VariableRole.Design,
        Help = "Independent: knowing A happened says nothing about B, so P(A ∩ B) = P(A)·P(B) — " +
               "two coin tosses, two components that fail separately. Mutually exclusive: they " +
               "cannot both happen, so P(A ∩ B) = 0 and P(A ∪ B) = P(A) + P(B) — a circuit " +
               "failing from the capacitor or from the resistor. Otherwise enter what is known: " +
               "the joint probability, the conditional P(A | B), or the two likelihoods of B " +
               "for Bayes' theorem.")]
    public EventRelationship Relationship
    {
        get => _relationship;
        set => SetProperty(ref _relationship, value);
    }

    [StatistleProperty(Label = "P(A)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The probability of event A, between 0 (impossible) and 1 (certain). In a Bayes " +
               "problem this is the prior — the chance of the fault before any test.")]
    public double ProbabilityA
    {
        get => _probabilityA;
        set => SetProperty(ref _probabilityA, value);
    }

    [StatistleProperty(Label = "P(B)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The probability of event B, between 0 and 1.")]
    public double ProbabilityB
    {
        get => _probabilityB;
        set => SetProperty(ref _probabilityB, value);
    }

    [StatistleProperty(Label = "P(A ∩ B)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The joint probability that both A and B occur. It can never exceed the smaller " +
               "of P(A) and P(B).")]
    public double Intersection
    {
        get => _intersection;
        set => SetProperty(ref _intersection, value);
    }

    [StatistleProperty(Label = "P(A | B)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The conditional probability of A given that B has occurred. The multiplication " +
               "rule turns it into the joint probability: P(A ∩ B) = P(A | B)·P(B).")]
    public double ConditionalAGivenB
    {
        get => _conditionalAGivenB;
        set => SetProperty(ref _conditionalAGivenB, value);
    }

    [StatistleProperty(Label = "P(B | A)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The probability of B when A is true — a test's sensitivity: the chance of a " +
               "positive result when the fault is present.")]
    public double LikelihoodGivenA
    {
        get => _likelihoodGivenA;
        set => SetProperty(ref _likelihoodGivenA, value);
    }

    [StatistleProperty(Label = "P(B | A′)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The probability of B when A is false — a test's false-positive rate: the chance " +
               "of a positive result when there is no fault.")]
    public double LikelihoodGivenNotA
    {
        get => _likelihoodGivenNotA;
        set => SetProperty(ref _likelihoodGivenNotA, value);
    }

    [StatistleProperty(Label = "Items to choose from (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0, Max = 170,
        Help = "For the counting rules: how many distinct items there are.")]
    public double Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    [StatistleProperty(Label = "Items chosen (r)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 0, Max = 170,
        Help = "How many of the n items are chosen. Permutations count the orderings " +
               "(n!/(n − r)!), combinations count the subsets (n!/(r!(n − r)!)).")]
    public double Chosen
    {
        get => _chosen;
        set => SetProperty(ref _chosen, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "P(A′)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The complement, 1 − P(A): the probability that A does not occur.")]
    public double ComplementA
    {
        get => _complementA;
        set => SetProperty(ref _complementA, value);
    }

    [StatistleProperty(Label = "P(B′)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The complement, 1 − P(B): the probability that B does not occur.")]
    public double ComplementB
    {
        get => _complementB;
        set => SetProperty(ref _complementB, value);
    }

    [StatistleProperty(Label = "P(B) used", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Calculated,
        Help = "The probability of B the rules used: as entered, or — with the likelihoods known " +
               "— by the total probability rule, P(B) = P(B | A)·P(A) + P(B | A′)·P(A′).")]
    public double ProbabilityBResult
    {
        get => _probabilityBResult;
        set => SetProperty(ref _probabilityBResult, value);
    }

    [StatistleProperty(Label = "P(A ∩ B)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability that both occur: P(A)·P(B) when independent, 0 when mutually " +
               "exclusive, P(A | B)·P(B) or P(B | A)·P(A) by the multiplication rule otherwise.")]
    public double IntersectionResult
    {
        get => _intersectionResult;
        set => SetProperty(ref _intersectionResult, value);
    }

    [StatistleProperty(Label = "P(A ∪ B)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability that at least one occurs, by the addition rule " +
               "P(A ∪ B) = P(A) + P(B) − P(A ∩ B). The subtraction stops the overlap being counted twice.")]
    public double Union
    {
        get => _union;
        set => SetProperty(ref _union, value);
    }

    [StatistleProperty(Label = "P(A | B)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of A given B: P(A ∩ B) / P(B). With the likelihoods known this " +
               "is Bayes' theorem — the posterior probability of the fault given a positive test. " +
               "Blank when P(B) is zero.")]
    public double AGivenB
    {
        get => _aGivenB;
        set => SetProperty(ref _aGivenB, value);
    }

    [StatistleProperty(Label = "P(B | A)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The probability of B given A: P(A ∩ B) / P(A). Blank when P(A) is zero.")]
    public double BGivenA
    {
        get => _bGivenA;
        set => SetProperty(ref _bGivenA, value);
    }

    [StatistleProperty(Label = "Independence", Role = VariableRole.Output,
        Help = "Whether P(A ∩ B) equals P(A)·P(B). Independent events are the exception in " +
               "engineering data — shared causes make failures cluster — so check rather than assume.")]
    public string Independence
    {
        get => _independence;
        set => SetProperty(ref _independence, value);
    }

    [StatistleProperty(Label = "Permutations (nPr)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "The number of ordered arrangements of r items from n: n!/(n − r)!. Order " +
               "matters — first, second, third place. Python math.perm(n, r); R factorial(n)/factorial(n − r).")]
    public double Permutations
    {
        get => _permutations;
        set => SetProperty(ref _permutations, value);
    }

    [StatistleProperty(Label = "Combinations (nCr)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "The number of subsets of r items from n: n!/(r!(n − r)!) — the binomial " +
               "coefficient. Order does not matter — a committee, a sample. Python math.comb(n, r); R choose(n, r).")]
    public double Combinations
    {
        get => _combinations;
        set => SetProperty(ref _combinations, value);
    }

    public override void Recalculate()
    {
        Problem = null;
        ComplementA = ComplementB = ProbabilityBResult = IntersectionResult = Union = AGivenB = BGivenA = double.NaN;
        Independence = string.Empty;
        Permutations = Combinations = double.NaN;

        Counting();

        double a = ProbabilityA;

        if (double.IsNaN(a))
        {
            Problem = "Enter P(A).";
            return;
        }

        double b, joint;

        switch (Relationship)
        {
            case EventRelationship.KnownLikelihoods:
                if (double.IsNaN(LikelihoodGivenA) || double.IsNaN(LikelihoodGivenNotA))
                {
                    Problem = "Enter P(B | A) and P(B | A′).";
                    return;
                }

                b = LikelihoodGivenA * a + LikelihoodGivenNotA * (1 - a);
                joint = LikelihoodGivenA * a;
                break;

            default:
                b = ProbabilityB;

                if (double.IsNaN(b))
                {
                    Problem = "Enter P(B).";
                    return;
                }

                joint = Relationship switch
                {
                    EventRelationship.Independent => a * b,
                    EventRelationship.MutuallyExclusive => 0,
                    EventRelationship.KnownIntersection => Intersection,
                    _ => ConditionalAGivenB * b,
                };
                break;
        }

        if (double.IsNaN(joint))
        {
            Problem = Relationship == EventRelationship.KnownIntersection ? "Enter P(A ∩ B)." : "Enter P(A | B).";
            return;
        }

        if (Relationship == EventRelationship.MutuallyExclusive && a + b > 1 + 1e-9)
        {
            Problem = $"Mutually exclusive events cannot have P(A) + P(B) = {a + b:F4} > 1.";
            return;
        }

        if (joint > Math.Min(a, b) + 1e-9)
        {
            Problem = $"P(A ∩ B) = {joint:F4} exceeds the smaller of P(A) and P(B); both must contain the intersection.";
            return;
        }

        ComplementA = 1 - a;
        ComplementB = 1 - b;
        ProbabilityBResult = b;
        IntersectionResult = joint;
        Union = a + b - joint;
        AGivenB = b > 0 ? joint / b : double.NaN;
        BGivenA = a > 0 ? joint / a : double.NaN;
        Independence = Math.Abs(joint - a * b) < 1e-9
            ? $"Independent: P(A ∩ B) = P(A)·P(B) = {a * b:F4}"
            : $"Not independent: P(A ∩ B) = {joint:F4} but P(A)·P(B) = {a * b:F4}";
    }

    private void Counting()
    {
        if (double.IsNaN(Items) || double.IsNaN(Chosen))
        {
            return;
        }

        int n = (int)Math.Round(Items), r = (int)Math.Round(Chosen);

        if (r > n)
        {
            Problem = "Cannot choose more items than there are (r > n).";
            return;
        }

        double permutations = 1;

        for (int i = 0; i < r; i++)
        {
            permutations *= n - i;
        }

        Permutations = permutations;
        Combinations = Math.Round(SpecialFunctions.Binomial(n, r));
    }

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        s.Either("import math", "# Base R needs no packages for these rules");
        s.Line();
        s.Comment("The two events");
        s.Assign("p_a", ProbabilityA);

        switch (Relationship)
        {
            case EventRelationship.KnownLikelihoods:
                s.Assign("p_b_given_a", LikelihoodGivenA);
                s.Assign("p_b_given_not_a", LikelihoodGivenNotA);
                s.Comment("Total probability: P(B) = P(B|A)·P(A) + P(B|A')·P(A')");
                s.Assign("p_b", "p_b_given_a * p_a + p_b_given_not_a * (1 - p_a)");
                s.Assign("p_ab", "p_b_given_a * p_a");
                break;
            case EventRelationship.Independent:
                s.Assign("p_b", ProbabilityB);
                s.Comment("Independent: P(A ∩ B) = P(A)·P(B)");
                s.Assign("p_ab", "p_a * p_b");
                break;
            case EventRelationship.MutuallyExclusive:
                s.Assign("p_b", ProbabilityB);
                s.Comment("Mutually exclusive: the events cannot both occur");
                s.Assign("p_ab", "0.0");
                break;
            case EventRelationship.KnownIntersection:
                s.Assign("p_b", ProbabilityB);
                s.Assign("p_ab", Intersection);
                break;
            default:
                s.Assign("p_b", ProbabilityB);
                s.Assign("p_a_given_b", ConditionalAGivenB);
                s.Comment("Multiplication rule: P(A ∩ B) = P(A|B)·P(B)");
                s.Assign("p_ab", "p_a_given_b * p_b");
                break;
        }

        s.Line();
        s.Print("P(A′)", "1 - p_a", "1 - p_a", 4);
        s.Print("P(B′)", "1 - p_b", "1 - p_b", 4);
        s.Print("P(B) used", "p_b", "p_b", 4);
        s.Print("P(A ∩ B)", "p_ab", "p_ab", 4);
        s.Comment("Addition rule");
        s.Print("P(A ∪ B)", "p_a + p_b - p_ab", "p_a + p_b - p_ab", 4);
        s.Comment("Conditional probability (Bayes' theorem when the likelihoods were given)");
        s.Print("P(A | B)", "p_ab / p_b", "p_ab / p_b", 4);
        s.Print("P(B | A)", "p_ab / p_a", "p_ab / p_a", 4);
        s.Either(
            "print(\"Independence: \" + (\"independent\" if abs(p_ab - p_a * p_b) < 1e-9 else \"not independent\"))",
            "cat(\"Independence:\", if (abs(p_ab - p_a * p_b) < 1e-9) \"independent\" else \"not independent\", \"\\n\")");

        if (!double.IsNaN(Items) && !double.IsNaN(Chosen))
        {
            s.Line();
            s.Comment("Counting rules");
            s.Assign("n", (int)Math.Round(Items));
            s.Assign("r", (int)Math.Round(Chosen));
            s.PrintRaw("Permutations (nPr)", "math.perm(n, r)", "factorial(n) / factorial(n - r)");
            s.PrintRaw("Combinations (nCr)", "math.comb(n, r)", "choose(n, r)");
        }

        return s.ToString();
    }
}

/// <summary>Each relationship is a method: it names the entries it needs and cites its rule.</summary>
public abstract class RelationshipMethod : CalculationMethod<ProbabilityRules>
{
    public abstract EventRelationship Selects { get; }

    public override bool IsApplicableTo(ProbabilityRules subject) => subject is not null && subject.Relationship == Selects;

    public override string Citation =>
        "EGN3443 Module 3 (Probability Concepts); Montgomery, D. C. and Runger, G. C., Applied " +
        "Statistics and Probability for Engineers, 7th ed., 2018, §2-3 to 2-7.";

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(ProbabilityRules.ComplementA),
        nameof(ProbabilityRules.ComplementB),
        nameof(ProbabilityRules.ProbabilityBResult),
        nameof(ProbabilityRules.IntersectionResult),
        nameof(ProbabilityRules.Union),
        nameof(ProbabilityRules.AGivenB),
        nameof(ProbabilityRules.BGivenA),
        nameof(ProbabilityRules.Independence),
    ];

    public override void Apply(ProbabilityRules subject) => subject.Recalculate();
}

public sealed class IndependentEvents : RelationshipMethod
{
    public override EventRelationship Selects => EventRelationship.Independent;

    public override string Name => "Independent events";

    public override string Description => "P(A ∩ B) = P(A)·P(B); P(A ∪ B) = P(A) + P(B) − P(A)·P(B).";

    public override IReadOnlyList<string> Inputs => [nameof(ProbabilityRules.ProbabilityA), nameof(ProbabilityRules.ProbabilityB)];
}

public sealed class MutuallyExclusiveEvents : RelationshipMethod
{
    public override EventRelationship Selects => EventRelationship.MutuallyExclusive;

    public override string Name => "Mutually exclusive events";

    public override string Description => "P(A ∩ B) = 0; P(A ∪ B) = P(A) + P(B).";

    public override IReadOnlyList<string> Inputs => [nameof(ProbabilityRules.ProbabilityA), nameof(ProbabilityRules.ProbabilityB)];
}

public sealed class KnownIntersectionEvents : RelationshipMethod
{
    public override EventRelationship Selects => EventRelationship.KnownIntersection;

    public override string Name => "Joint probability known";

    public override string Description => "General addition rule P(A ∪ B) = P(A) + P(B) − P(A ∩ B); conditionals P(A | B) = P(A ∩ B)/P(B).";

    public override IReadOnlyList<string> Inputs => [nameof(ProbabilityRules.ProbabilityA), nameof(ProbabilityRules.ProbabilityB), nameof(ProbabilityRules.Intersection)];
}

public sealed class KnownConditionalEvents : RelationshipMethod
{
    public override EventRelationship Selects => EventRelationship.KnownConditional;

    public override string Name => "Conditional probability known";

    public override string Description => "Multiplication rule P(A ∩ B) = P(A | B)·P(B), then the addition rule.";

    public override IReadOnlyList<string> Inputs => [nameof(ProbabilityRules.ProbabilityA), nameof(ProbabilityRules.ProbabilityB), nameof(ProbabilityRules.ConditionalAGivenB)];
}

public sealed class BayesEvents : RelationshipMethod
{
    public override EventRelationship Selects => EventRelationship.KnownLikelihoods;

    public override string Name => "Bayes' theorem";

    public override string Description =>
        "Total probability P(B) = P(B | A)·P(A) + P(B | A′)·P(A′); Bayes P(A | B) = P(B | A)·P(A) / P(B).";

    public override IReadOnlyList<string> Inputs => [nameof(ProbabilityRules.ProbabilityA), nameof(ProbabilityRules.LikelihoodGivenA), nameof(ProbabilityRules.LikelihoodGivenNotA)];
}

/// <summary>The counting rules, always in force.</summary>
public sealed class CountingRules : CalculationMethod<ProbabilityRules>
{
    public override string Name => "Counting rules";

    public override string Description => "Permutations n!/(n − r)! and combinations n!/(r!(n − r)!).";

    public override string Citation => "Montgomery and Runger, 7th ed., §2-1.4 (counting techniques).";

    public override IReadOnlyList<string> Inputs => [nameof(ProbabilityRules.Items), nameof(ProbabilityRules.Chosen)];

    public override IReadOnlyList<string> Outputs => [nameof(ProbabilityRules.Permutations), nameof(ProbabilityRules.Combinations)];

    public override void Apply(ProbabilityRules subject) => subject.Recalculate();
}
