using System.ComponentModel;
using System.Globalization;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Chance;
using Statistle.Core.Methods.Distributions;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Units;

namespace Statistle.Core.Analyses;

public enum SimulationExperiment
{
    [Description("Flip coins")]
    Coins = 0,

    [Description("Roll dice")]
    Dice,

    [Description("Deal cards")]
    Cards,

    [Description("Throw darts at a square")]
    Darts,

    [Description("Sample means (central limit theorem)")]
    SampleMeans,

    [Description("Confidence intervals: how many contain μ")]
    IntervalCoverage,

    [Description("Many t-tests: the familywise error")]
    FamilywiseError,
}

public enum EventComparison
{
    [Description("equals")]
    EqualTo = 0,

    [Description("is at least")]
    AtLeast,

    [Description("is at most")]
    AtMost,
}

public enum CardOutcome
{
    [Description("Total (J, Q, K = 10; ace 1 or 11)")]
    Total = 0,

    [Description("Number of hearts")]
    Hearts,

    [Description("Number of aces")]
    Aces,
}

public enum SampledDistribution
{
    [Description("Normal (μ, σ)")]
    Normal = 0,

    [Description("Uniform (a, b)")]
    Uniform,

    [Description("Exponential (rate λ)")]
    Exponential,
}

/// <summary>
/// EGN3443 Module 3 (theoretical against experimental probability, the law of large numbers),
/// Module 6 (the central limit theorem), Module 7 (what "95 % confidence" means) and Module 8 (the
/// familywise error of many tests), by doing the experiment many times on the computer. Seeded, so
/// a course task gives the same numbers every time.
/// </summary>
[StatistleHelp(
    "Simulation: do a chance experiment many times and compare what happens with what theory " +
    "says. Choose the experiment — flip coins, roll dice, deal cards, throw darts at a square — " +
    "and the event to count (two dice summing to 7, three cards that make 21, a dart inside the " +
    "circle). The results give how many times the event happened, the experimental probability " +
    "(that count over the number of trials), the theoretical probability from counting the " +
    "equally likely cases, and the difference. Run more trials and the difference shrinks: the " +
    "law of large numbers, drawn in the Plots tab. Sample means draws many samples of size n and " +
    "shows their means piling up in a normal curve of spread σ/√n whatever the population's " +
    "shape — the central limit theorem. Confidence intervals counts how many intervals catch the " +
    "true mean (about 95 of 100 at 95 %), and Many t-tests shows how often at least one of several " +
    "tests rejects a true null hypothesis. The Seed fixes the random numbers: the same seed gives " +
    "the same results every time, and a different seed a different run. Python and R draw their " +
    "own random numbers, so the Code tab's counts differ from these, but the proportions agree " +
    "closely once the trials are many.")]
public sealed class Simulation : Analysis, IReportsProblem, IHasFigures, ICodeSource
{
    /// <summary>The most trials one run may have, so the form stays quick as it recalculates on every entry.</summary>
    public const int MaximumTrials = 100_000;

    private SimulationExperiment _experiment = SimulationExperiment.Dice;
    private double _trials = 1000;
    private double _seed = 1;
    private double _coins = 1;
    private double _dice = 2;
    private double _cards = 3;
    private CardOutcome _cardOutcome = CardOutcome.Total;
    private EventComparison _eventIs = EventComparison.EqualTo;
    private double _heads = 1;
    private double _diceTotal = 7;
    private double _cardTarget = 21;
    private SampledDistribution _distribution = SampledDistribution.Exponential;
    private double _populationMean = 50;
    private double _populationSigma = 10;
    private double _lower;
    private double _upper = 1;
    private double _rate = 1;
    private double _sampleSize = 30;
    private double _confidence = 0.95;
    private double _alpha = 0.05;
    private double _tests = 10;

    private int _trialsRun;
    private string _eventText = string.Empty;
    private int _occurrences;
    private double _experimentalProbability = double.NaN;
    private double _theoreticalProbability = double.NaN;
    private double _difference = double.NaN;
    private double _piEstimate = double.NaN;
    private double _meanOfMeans = double.NaN;
    private double _sdOfMeans = double.NaN;
    private double _theoreticalMean = double.NaN;
    private double _theoreticalStandardError = double.NaN;

    public Simulation()
    {
        Name = "Simulation";
        Recalculate();
    }

    public override string Title => "Simulation";

    public override string Summary => "Coins, dice, cards and darts against theory; sample means; interval coverage (Modules 3, 6, 7)";

    public override ToolKind Kind => ToolKind.FreeStanding;

    public override int Order => 24;

    public override bool ResultsBeside => true;

    [StatistleIgnore]
    public string? Problem { get; private set; }

    // ── Entries: the experiment ───────────────────────────────────────────────

    [StatistleProperty(Label = "Experiment", Role = VariableRole.Design,
        Help = "What to do over and over: flip coins, roll dice, deal cards or throw darts and count " +
               "an event (Module 3); draw samples and look at their means (Module 6, the central " +
               "limit theorem); build confidence intervals and count how many contain the true mean " +
               "(Module 7); or run several t-tests where every null hypothesis is true and count how " +
               "often at least one rejects (Module 8).")]
    public SimulationExperiment Experiment
    {
        get => _experiment;
        set => SetProperty(ref _experiment, value);
    }

    [StatistleProperty(Label = "Number of trials", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1, Max = MaximumTrials,
        Help = "How many times the experiment is repeated, 1 to 100 000. With 10 trials the " +
               "experimental probability can be far from the theory; with 10 000 it is usually " +
               "within about 0.01 — the law of large numbers. The error shrinks like 1/√trials, so " +
               "four times the trials halve it.")]
    public double Trials
    {
        get => _trials;
        set => SetProperty(ref _trials, value);
    }

    [StatistleProperty(Label = "Seed", Decimals = 0, Role = VariableRole.Input, Min = 0,
        Help = "A whole number that starts the random numbers. The same seed gives exactly the same " +
               "results every time, on any computer, so a run can be checked and shared; change it " +
               "to see another run of the same experiment. Python's np.random.default_rng(seed) and " +
               "R's set.seed(seed) take a seed too, but draw different numbers from the program's.")]
    public double Seed
    {
        get => _seed;
        set => SetProperty(ref _seed, value);
    }

    [StatistleProperty(Label = "Coins per trial", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1, Max = 20,
        Help = "How many fair coins are flipped in each trial, 1 to 20. The outcome of a trial is the " +
               "number of heads, from 0 to this number; in theory it is binomial with p = 0.5.")]
    public double Coins
    {
        get => _coins;
        set => SetProperty(ref _coins, value);
    }

    [StatistleProperty(Label = "Dice per trial", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1, Max = 6,
        Help = "How many fair six-sided dice are rolled in each trial, 1 to 6. The outcome is their " +
               "sum; with one die it is simply the face. Two dice give sums 2 to 12, with 7 the most " +
               "likely (6 of the 36 equally likely rolls).")]
    public double Dice
    {
        get => _dice;
        set => SetProperty(ref _dice, value);
    }

    [StatistleProperty(Label = "Cards per hand", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1, Max = 5,
        Help = "How many cards are dealt from a full 52-card deck in each trial, 1 to 5, without " +
               "replacement: the deck is shuffled again before every hand.")]
    public double Cards
    {
        get => _cards;
        set => SetProperty(ref _cards, value);
    }

    [StatistleProperty(Label = "What is counted in the hand", Role = VariableRole.Design,
        Help = "The outcome of a hand. Total adds the cards' values: 2 to 10 at face value, jack, " +
               "queen and king 10, an ace 1 or 11 — 11 when that keeps the total at 21 or under — so " +
               "'total equals 21' with three cards is the course's three-card 21. Number of hearts " +
               "and number of aces count those cards; in theory they are hypergeometric.")]
    public CardOutcome CardOutcome
    {
        get => _cardOutcome;
        set => SetProperty(ref _cardOutcome, value);
    }

    [StatistleProperty(Label = "The event: the outcome", Role = VariableRole.Design,
        Help = "How the outcome of a trial is compared with the target value to decide whether the " +
               "event happened: equals (exactly 7), is at least (7 or more) or is at most (7 or less).")]
    public EventComparison EventIs
    {
        get => _eventIs;
        set => SetProperty(ref _eventIs, value);
    }

    [StatistleProperty(Label = "Number of heads", Decimals = 0, Role = VariableRole.Input, Min = 0,
        Help = "The target number of heads for the event — with one coin, 1 means 'heads'. With 3 " +
               "coins, 'is at least 2' means a majority of heads.")]
    public double Heads
    {
        get => _heads;
        set => SetProperty(ref _heads, value);
    }

    [StatistleProperty(Label = "Sum of the dice", Decimals = 0, Role = VariableRole.Input, Min = 1,
        Help = "The target sum for the event — 7 with two dice, or a face such as 6 with one die. A " +
               "sum no roll can make gives a theoretical probability of 0.")]
    public double DiceTotal
    {
        get => _diceTotal;
        set => SetProperty(ref _diceTotal, value);
    }

    [StatistleProperty(Label = "Target for the hand", Decimals = 0, Role = VariableRole.Input, Min = 0,
        Help = "The target for what is counted in the hand: 21 for a total of 21, 1 for 'exactly one " +
               "heart', and so on.")]
    public double CardTarget
    {
        get => _cardTarget;
        set => SetProperty(ref _cardTarget, value);
    }

    // ── Entries: sampling ─────────────────────────────────────────────────────

    [StatistleProperty(Label = "Population", Role = VariableRole.Design,
        Help = "The distribution each sample is drawn from. Normal is symmetric; uniform is flat; " +
               "exponential is strongly skewed to the right. The central limit theorem says the " +
               "sample mean is close to normal for all three once n is about 30 — try exponential " +
               "with n = 2 and then n = 30.")]
    public SampledDistribution Distribution
    {
        get => _distribution;
        set => SetProperty(ref _distribution, value);
    }

    [StatistleProperty(Label = "Population mean (μ)", Decimals = 4, Role = VariableRole.Input,
        Help = "The mean of the normal population the samples are drawn from — the true value an " +
               "interval is trying to catch.")]
    public double PopulationMean
    {
        get => _populationMean;
        set => SetProperty(ref _populationMean, value);
    }

    [StatistleProperty(Label = "Population standard deviation (σ)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The spread of single values in the normal population; must be greater than zero.")]
    public double PopulationSigma
    {
        get => _populationSigma;
        set => SetProperty(ref _populationSigma, value);
    }

    [StatistleProperty(Label = "Lower limit (a)", Decimals = 4, Role = VariableRole.Input,
        Help = "The smallest value of the uniform population. Its mean is (a + b)/2 and its standard " +
               "deviation (b − a)/√12.")]
    public double Lower
    {
        get => _lower;
        set => SetProperty(ref _lower, value);
    }

    [StatistleProperty(Label = "Upper limit (b)", Decimals = 4, Role = VariableRole.Input,
        Help = "The largest value of the uniform population; must be greater than a.")]
    public double Upper
    {
        get => _upper;
        set => SetProperty(ref _upper, value);
    }

    [StatistleProperty(Label = "Rate (λ)", Decimals = 4, Role = VariableRole.Input, Min = 0,
        Help = "The rate of the exponential population — events per unit time. Its mean and its " +
               "standard deviation are both 1/λ. numpy's exponential takes the scale 1/λ instead.")]
    public double Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
    }

    [StatistleProperty(Label = "Sample size (n)", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1, Max = 1000,
        Help = "How many values each sample has, 1 to 1000 (at least 2 for an interval or a t-test, " +
               "which need a standard deviation). The standard error σ/√n falls as n grows.")]
    public double SampleSize
    {
        get => _sampleSize;
        set => SetProperty(ref _sampleSize, value);
    }

    [StatistleProperty(Label = "Confidence level", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The confidence level of each t interval, such as 0.95. In the long run that share " +
               "of intervals contains μ: the confidence is in the method, not in any one interval.")]
    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    [StatistleProperty(Label = "Significance level (α)", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Input, Min = 0, Max = 1,
        Help = "The significance level of each two-sided t-test, such as 0.05: the chance that one " +
               "test rejects a null hypothesis that is true (a type I error).")]
    public double Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    [StatistleProperty(Label = "Tests per trial", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Input, Min = 1, Max = 100,
        Help = "How many independent t-tests are run in each trial, 1 to 100, every one on data " +
               "where the null hypothesis is true. With 10 tests at α = 0.05 at least one rejects " +
               "about 40 % of the time — why many comparisons need a correction such as Bonferroni's α/m.")]
    public double Tests
    {
        get => _tests;
        set => SetProperty(ref _tests, value);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [StatistleProperty(Label = "Trials run", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Calculated,
        Help = "How many times the experiment was repeated: the number of trials entered.")]
    public int TrialsRun
    {
        get => _trialsRun;
        set => SetProperty(ref _trialsRun, value);
    }

    [StatistleProperty(Label = "Event", Role = VariableRole.Output,
        Help = "The event being counted, in words, as the settings above define it.")]
    public string EventText
    {
        get => _eventText;
        set => SetProperty(ref _eventText, value);
    }

    [SimulatedResult]
    [StatistleProperty(Label = "Times the event happened", Decimals = 0, Unit = ValueKind.Count, Role = VariableRole.Output,
        Help = "In how many of the trials the event happened. The same seed always gives the same " +
               "count; Python and R give a different count of about the same size.")]
    public int Occurrences
    {
        get => _occurrences;
        set => SetProperty(ref _occurrences, value);
    }

    [SimulatedResult]
    [StatistleProperty(Label = "Experimental probability", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The relative frequency: times the event happened ÷ number of trials. It estimates the " +
               "probability and approaches it as the trials grow (the law of large numbers). Python " +
               "np.mean(event); R mean(event).")]
    public double ExperimentalProbability
    {
        get => _experimentalProbability;
        set => SetProperty(ref _experimentalProbability, value);
    }

    [StatistleProperty(Label = "Theoretical probability", Decimals = 4, Unit = ValueKind.Probability, Role = VariableRole.Output,
        Help = "The exact probability of the event, from counting equally likely cases (favourable ÷ " +
               "possible) or from the distribution: binomial for coins, all 6ⁿ rolls for dice, every " +
               "possible hand for cards, π/4 for the darts, the confidence level for intervals, " +
               "1 − (1 − α)ᵐ for m independent tests.")]
    public double TheoreticalProbability
    {
        get => _theoreticalProbability;
        set => SetProperty(ref _theoreticalProbability, value);
    }

    [SimulatedResult]
    [StatistleProperty(Label = "Experimental − theoretical", Decimals = 4, Role = VariableRole.Output,
        Help = "How far the experiment is from the theory. It is rarely zero, and is usually within " +
               "about 2√(p(1 − p)/trials) of it — about ±0.024 for p = 1/6 and 1000 trials.")]
    public double Difference
    {
        get => _difference;
        set => SetProperty(ref _difference, value);
    }

    [SimulatedResult]
    [StatistleProperty(Label = "Estimate of π (4 × proportion)", Decimals = 4, Role = VariableRole.Output,
        Help = "The circle of radius 1 inside the 2 × 2 square covers π/4 of it, so four times the " +
               "proportion of darts inside estimates π = 3.1416. Slowly: 10 000 darts are typically " +
               "within about 0.03.")]
    public double PiEstimate
    {
        get => _piEstimate;
        set => SetProperty(ref _piEstimate, value);
    }

    [SimulatedResult]
    [StatistleProperty(Label = "Mean of the sample means", Decimals = 4, Role = VariableRole.Output,
        Help = "The average of all the x̄ drawn. It is close to the population mean μ: the sample " +
               "mean is unbiased. Python np.mean(means); R mean(means).")]
    public double MeanOfMeans
    {
        get => _meanOfMeans;
        set => SetProperty(ref _meanOfMeans, value);
    }

    [SimulatedResult]
    [StatistleProperty(Label = "Standard deviation of the sample means", Decimals = 4, Role = VariableRole.Output,
        Help = "The spread of the x̄ drawn (dividing by trials − 1). It is close to σ/√n, the " +
               "standard error, and much smaller than the spread σ of single values. Python " +
               "np.std(means, ddof=1); R sd(means).")]
    public double SdOfMeans
    {
        get => _sdOfMeans;
        set => SetProperty(ref _sdOfMeans, value);
    }

    [StatistleProperty(Label = "Theoretical mean of x̄ (μ)", Decimals = 4, Role = VariableRole.Output,
        Help = "The population's mean, which is also the mean of the sampling distribution of x̄: μ " +
               "for the normal, (a + b)/2 for the uniform, 1/λ for the exponential.")]
    public double TheoreticalMean
    {
        get => _theoreticalMean;
        set => SetProperty(ref _theoreticalMean, value);
    }

    [StatistleProperty(Label = "Theoretical standard error (σ/√n)", Decimals = 4, Role = VariableRole.Output,
        Help = "The population's standard deviation over √n — σ for the normal, (b − a)/√12 for the " +
               "uniform, 1/λ for the exponential — the standard deviation the central limit theorem " +
               "gives the sample mean.")]
    public double TheoreticalStandardError
    {
        get => _theoreticalStandardError;
        set => SetProperty(ref _theoreticalStandardError, value);
    }

    // ── What the figures draw ─────────────────────────────────────────────────

    /// <summary>The proportion of trials with the event so far, at up to about 1100 trial numbers.</summary>
    [StatistleIgnore]
    public IReadOnlyList<(double Trial, double Proportion)> RunningProportion { get; private set; } = [];

    /// <summary>Each outcome's simulated share and theoretical probability, and whether it is the event.</summary>
    [StatistleIgnore]
    public IReadOnlyList<(int Outcome, double Share, double Theory, bool IsEvent)> OutcomeTable { get; private set; } = [];

    /// <summary>The first 2000 darts.</summary>
    [StatistleIgnore]
    public IReadOnlyList<(double X, double Y, bool Inside)> DartsThrown { get; private set; } = [];

    /// <summary>The first 100 intervals.</summary>
    [StatistleIgnore]
    public IReadOnlyList<(double Lower, double Upper, bool Covers)> IntervalsBuilt { get; private set; } = [];

    /// <summary>Every sample mean drawn.</summary>
    [StatistleIgnore]
    public IReadOnlyList<double> SampleMeansDrawn { get; private set; } = [];

    /// <summary>True for the experiments that count an event (all but Sample means).</summary>
    [StatistleIgnore]
    public bool CountsAnEvent => Experiment != SimulationExperiment.SampleMeans;

    public override void Recalculate()
    {
        Problem = null;
        TrialsRun = 0;
        Occurrences = 0;
        EventText = string.Empty;
        ExperimentalProbability = TheoreticalProbability = Difference = PiEstimate = double.NaN;
        MeanOfMeans = SdOfMeans = TheoreticalMean = TheoreticalStandardError = double.NaN;
        RunningProportion = [];
        OutcomeTable = [];
        DartsThrown = [];
        IntervalsBuilt = [];
        SampleMeansDrawn = [];

        if (!IsWhole(Trials, 1, MaximumTrials))
        {
            Problem = $"Enter the number of trials, a whole number from 1 to {MaximumTrials:N0}.";
            return;
        }

        if (!IsWhole(Seed, 0, 9_007_199_254_740_992))
        {
            Problem = "Enter the seed, a whole number of 0 or more.";
            return;
        }

        int trials = (int)Trials;
        var random = new RandomStream((ulong)Seed);

        Problem = Experiment switch
        {
            SimulationExperiment.Coins => RunCoins(random, trials),
            SimulationExperiment.Dice => RunDice(random, trials),
            SimulationExperiment.Cards => RunCards(random, trials),
            SimulationExperiment.Darts => RunDarts(random, trials),
            SimulationExperiment.SampleMeans => RunSampleMeans(random, trials),
            SimulationExperiment.IntervalCoverage => RunCoverage(random, trials),
            SimulationExperiment.FamilywiseError => RunFamilywise(random, trials),
            _ => "Choose an experiment.",
        };

        if (Problem is null)
        {
            TrialsRun = trials;
        }
    }

    // ── The experiments ───────────────────────────────────────────────────────

    private string? RunCoins(RandomStream random, int trials)
    {
        if (!IsWhole(Coins, 1, 20))
        {
            return "Enter the coins per trial, a whole number from 1 to 20.";
        }

        if (!IsWhole(Heads, 0, 1000))
        {
            return "Enter the number of heads for the event, a whole number of 0 or more.";
        }

        int coins = (int)Coins;
        EventText = $"the number of heads in {Plural(coins, "coin flip")} {Compare()} {(int)Heads}";
        CountEvent(trials, () => Experiments.Heads(random, coins), Experiments.CoinTheory(coins));
        return null;
    }

    private string? RunDice(RandomStream random, int trials)
    {
        if (!IsWhole(Dice, 1, 6))
        {
            return "Enter the dice per trial, a whole number from 1 to 6.";
        }

        if (!IsWhole(DiceTotal, 0, 1000))
        {
            return "Enter the sum of the dice for the event, a whole number.";
        }

        int dice = (int)Dice;
        EventText = dice == 1
            ? $"the face of 1 die {Compare()} {(int)DiceTotal}"
            : $"the sum of {dice} dice {Compare()} {(int)DiceTotal}";
        CountEvent(trials, () => Experiments.DiceSum(random, dice), Experiments.DiceTheory(dice));
        return null;
    }

    private string? RunCards(RandomStream random, int trials)
    {
        if (!IsWhole(Cards, 1, 5))
        {
            return "Enter the cards per hand, a whole number from 1 to 5.";
        }

        if (!IsWhole(CardTarget, 0, 1000))
        {
            return "Enter the target for the hand, a whole number.";
        }

        int cards = (int)Cards;
        int[] deck = Experiments.NewDeck();
        string hand = Plural(cards, "card");

        (Func<int> outcome, IReadOnlyDictionary<int, double> theory, string what) = CardOutcome switch
        {
            CardOutcome.Hearts => ((Func<int>)(() => CountOf(Experiments.Deal(random, deck, cards), Experiments.IsHeart)),
                Experiments.MarkedCardTheory(13, cards), $"the number of hearts in {hand}"),
            CardOutcome.Aces => (() => CountOf(Experiments.Deal(random, deck, cards), c => Experiments.Rank(c) == 1),
                Experiments.MarkedCardTheory(4, cards), $"the number of aces in {hand}"),
            _ => (() => Experiments.BestTotal(Experiments.Ranks(Experiments.Deal(random, deck, cards))),
                Experiments.CardTotalTheory(cards), $"the total of {hand} (J, Q, K count 10; an ace 1 or 11)"),
        };

        EventText = $"{what} {Compare()} {(int)CardTarget}";
        CountEvent(trials, outcome, theory);
        return null;
    }

    private string? RunDarts(RandomStream random, int trials)
    {
        List<(double, double, bool)> shown = [];
        EventText = "a dart thrown at random at a 2 × 2 square lands inside the circle of radius 1";

        CountEvent(trials, () =>
        {
            double x = 2 * random.NextDouble() - 1, y = 2 * random.NextDouble() - 1;
            bool inside = x * x + y * y <= 1;

            if (shown.Count < 2000)
            {
                shown.Add((x, y, inside));
            }

            return inside ? 1 : 0;
        }, new Dictionary<int, double> { [0] = 1 - Math.PI / 4, [1] = Math.PI / 4 }, outcome => outcome == 1);

        DartsThrown = shown;
        PiEstimate = 4 * ExperimentalProbability;
        return null;
    }

    private string? RunSampleMeans(RandomStream random, int trials)
    {
        if (!IsWhole(SampleSize, 1, 1000))
        {
            return "Enter the sample size, a whole number from 1 to 1000.";
        }

        int n = (int)SampleSize;
        Func<double> draw;
        double mu, sigma;

        switch (Distribution)
        {
            case SampledDistribution.Normal:
                if (double.IsNaN(PopulationMean) || double.IsNaN(PopulationSigma) || PopulationSigma <= 0)
                {
                    return "Enter the population mean and a standard deviation greater than zero.";
                }

                (mu, sigma) = (PopulationMean, PopulationSigma);
                draw = () => mu + sigma * random.NextNormal();
                break;

            case SampledDistribution.Uniform:
                if (double.IsNaN(Lower) || double.IsNaN(Upper) || Upper <= Lower)
                {
                    return "Enter the lower and upper limits, with the upper greater than the lower.";
                }

                (double a, double b) = (Lower, Upper);
                (mu, sigma) = ((a + b) / 2, (b - a) / Math.Sqrt(12));
                draw = () => a + (b - a) * random.NextDouble();
                break;

            default:
                if (double.IsNaN(Rate) || Rate <= 0)
                {
                    return "Enter the rate λ, greater than zero.";
                }

                double rate = Rate;
                (mu, sigma) = (1 / rate, 1 / rate);
                draw = () => random.NextExponential(rate);
                break;
        }

        double[] means = new double[trials];

        for (int t = 0; t < trials; t++)
        {
            double sum = 0;

            for (int i = 0; i < n; i++)
            {
                sum += draw();
            }

            means[t] = sum / n;
        }

        SampleMeansDrawn = means;
        MeanOfMeans = means.Average();
        SdOfMeans = trials < 2 ? double.NaN : Math.Sqrt(means.Sum(m => (m - MeanOfMeans) * (m - MeanOfMeans)) / (trials - 1));
        TheoreticalMean = mu;
        TheoreticalStandardError = sigma / Math.Sqrt(n);
        return null;
    }

    private string? RunCoverage(RandomStream random, int trials)
    {
        if (!IsWhole(SampleSize, 2, 1000))
        {
            return "Enter the sample size, a whole number from 2 to 1000 (an interval needs a standard deviation).";
        }

        if (double.IsNaN(PopulationMean) || double.IsNaN(PopulationSigma) || PopulationSigma <= 0)
        {
            return "Enter the population mean and a standard deviation greater than zero.";
        }

        if (double.IsNaN(Confidence) || Confidence <= 0 || Confidence >= 1)
        {
            return "Enter a confidence level between 0 and 1, such as 0.95.";
        }

        int n = (int)SampleSize;
        double mu = PopulationMean, sigma = PopulationSigma;
        double tStar = new StudentTModel(n - 1).Quantile(1 - (1 - Confidence) / 2);
        List<(double, double, bool)> shown = [];
        EventText = $"a {Percent(Confidence)} t interval from a sample of {n} contains μ = {Show(mu)}";

        CountEvent(trials, () =>
        {
            (double mean, double sd) = NormalSample(random, n, mu, sigma);
            double margin = tStar * sd / Math.Sqrt(n);
            bool covers = mean - margin <= mu && mu <= mean + margin;

            if (shown.Count < 100)
            {
                shown.Add((mean - margin, mean + margin, covers));
            }

            return covers ? 1 : 0;
        }, new Dictionary<int, double> { [0] = 1 - Confidence, [1] = Confidence }, outcome => outcome == 1);

        IntervalsBuilt = shown;
        return null;
    }

    private string? RunFamilywise(RandomStream random, int trials)
    {
        if (!IsWhole(SampleSize, 2, 1000))
        {
            return "Enter the sample size, a whole number from 2 to 1000 (a t-test needs a standard deviation).";
        }

        if (double.IsNaN(Alpha) || Alpha <= 0 || Alpha >= 1)
        {
            return "Enter a significance level between 0 and 1, such as 0.05.";
        }

        if (!IsWhole(Tests, 1, 100))
        {
            return "Enter the tests per trial, a whole number from 1 to 100.";
        }

        int n = (int)SampleSize, m = (int)Tests;
        double tStar = new StudentTModel(n - 1).Quantile(1 - Alpha / 2);
        EventText = $"at least one of {Plural(m, "t-test")} at α = {Show(Alpha)}, each on a sample of {n} where the null hypothesis is true, rejects it";

        CountEvent(trials, () =>
        {
            int rejections = 0;

            for (int test = 0; test < m; test++)
            {
                (double mean, double sd) = NormalSample(random, n, 0, 1);
                rejections += Math.Abs(mean / (sd / Math.Sqrt(n))) > tStar ? 1 : 0;
            }

            return rejections;
        }, RejectionTheory(m, Alpha), outcome => outcome >= 1);

        TheoreticalProbability = 1 - Math.Pow(1 - Alpha, m);   // exactly, rather than as 1 − the sum of the others
        Difference = ExperimentalProbability - TheoreticalProbability;
        return null;
    }

    /// <summary>
    /// Runs the trials, counting the event, recording the running proportion, and tabulating each
    /// outcome against its theoretical probability. The event is the comparison chosen on the form
    /// unless <paramref name="isEvent"/> fixes it.
    /// </summary>
    private void CountEvent(int trials, Func<int> trial, IReadOnlyDictionary<int, double> theory, Func<int, bool>? isEvent = null)
    {
        isEvent ??= IsEventByComparison;
        Dictionary<int, int> counts = [];
        List<(double, double)> running = [];
        int step = Math.Max(1, trials / 1000), happened = 0;

        for (int t = 1; t <= trials; t++)
        {
            int outcome = trial();
            counts[outcome] = counts.GetValueOrDefault(outcome) + 1;
            happened += isEvent(outcome) ? 1 : 0;

            if (t <= 100 || t % step == 0 || t == trials)
            {
                running.Add((t, (double)happened / t));
            }
        }

        Occurrences = happened;
        ExperimentalProbability = (double)happened / trials;
        TheoreticalProbability = theory.Where(p => isEvent(p.Key)).Sum(p => p.Value);
        Difference = ExperimentalProbability - TheoreticalProbability;
        RunningProportion = running;
        OutcomeTable =
        [
            .. theory.Keys.Union(counts.Keys).Order()
                .Select(k => (k, (double)counts.GetValueOrDefault(k) / trials, theory.GetValueOrDefault(k), isEvent(k))),
        ];
    }

    private bool IsEventByComparison(int outcome)
    {
        double target = Experiment switch
        {
            SimulationExperiment.Coins => Heads,
            SimulationExperiment.Dice => DiceTotal,
            _ => CardTarget,
        };

        return EventIs switch
        {
            EventComparison.AtLeast => outcome >= target,
            EventComparison.AtMost => outcome <= target,
            _ => outcome == target,
        };
    }

    /// <summary>The number of m independent tests that reject a true null, each with probability α: binomial(m, α).</summary>
    private static Dictionary<int, double> RejectionTheory(int m, double alpha)
    {
        var binomial = new BinomialModel(m, alpha);
        return Enumerable.Range(0, m + 1).ToDictionary(k => k, binomial.Pmf);
    }

    /// <summary>The mean and sample standard deviation (n − 1) of n values from a normal population.</summary>
    private static (double Mean, double Sd) NormalSample(RandomStream random, int n, double mu, double sigma)
    {
        double sum = 0, sumSquares = 0;
        double[] x = new double[n];

        for (int i = 0; i < n; i++)
        {
            x[i] = mu + sigma * random.NextNormal();
            sum += x[i];
        }

        double mean = sum / n;

        for (int i = 0; i < n; i++)
        {
            sumSquares += (x[i] - mean) * (x[i] - mean);
        }

        return (mean, Math.Sqrt(sumSquares / (n - 1)));
    }

    private static int CountOf(ReadOnlySpan<int> hand, Func<int, bool> marked)
    {
        int count = 0;

        foreach (int card in hand)
        {
            count += marked(card) ? 1 : 0;
        }

        return count;
    }

    private static bool IsWhole(double value, double min, double max) =>
        !double.IsNaN(value) && value >= min && value <= max && value == Math.Floor(value);

    private string Compare() => EventIs switch
    {
        EventComparison.AtLeast => "is at least",
        EventComparison.AtMost => "is at most",
        _ => "equals",
    };

    private string Operator => EventIs switch
    {
        EventComparison.AtLeast => ">=",
        EventComparison.AtMost => "<=",
        _ => "==",
    };

    private static string Plural(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count} {noun}s";

    private static string Show(double value) => value.ToString("G6", CultureInfo.InvariantCulture);

    private static string Percent(double p) => (100 * p).ToString("G4", CultureInfo.InvariantCulture) + " %";

    // ── Figures ───────────────────────────────────────────────────────────────

    /// <summary>
    /// For an experiment that counts an event: (1) the running proportion against theory, then
    /// (2) the outcomes against theory — or, for the darts, the darts on the square, and for
    /// intervals, the first 100 intervals. For sample means: (1) the histogram of x̄ with the
    /// central limit theorem's normal curve.
    /// </summary>
    public IReadOnlyList<PlotSpec> Figures(UnitSelection? units)
    {
        if (TrialsRun == 0)
        {
            return [];
        }

        if (Experiment == SimulationExperiment.SampleMeans)
        {
            int classes = Math.Clamp((int)Math.Ceiling(Math.Sqrt(SampleMeansDrawn.Count)), 10, 40);
            return [SimulationFigures.SampleMeans($"{Name} — {SampleMeansDrawn.Count} sample means of n = {(int)SampleSize}", SampleMeansDrawn, classes, TheoreticalMean, TheoreticalStandardError)];
        }

        PlotSpec running = SimulationFigures.RunningProportion($"{Name} — the proportion settles on the theory", RunningProportion, TheoreticalProbability);

        PlotSpec second = Experiment switch
        {
            SimulationExperiment.Darts => SimulationFigures.Darts($"{Name} — the first {DartsThrown.Count} darts", DartsThrown),
            SimulationExperiment.IntervalCoverage => SimulationFigures.Intervals($"{Name} — the first {IntervalsBuilt.Count} intervals", IntervalsBuilt, PopulationMean),
            _ => SimulationFigures.Outcomes($"{Name} — each outcome, simulated and in theory", OutcomeName, OutcomeTable),
        };

        return [running, second];
    }

    private string OutcomeName => Experiment switch
    {
        SimulationExperiment.Coins => "number of heads",
        SimulationExperiment.Dice => (int)Dice == 1 ? "face" : "sum of the dice",
        SimulationExperiment.Cards => CardOutcome switch
        {
            CardOutcome.Hearts => "number of hearts",
            CardOutcome.Aces => "number of aces",
            _ => "total of the hand",
        },
        SimulationExperiment.FamilywiseError => "number of tests that reject",
        _ => "outcome",
    };

    // ── Code ──────────────────────────────────────────────────────────────────

    public string Code(CodeLanguage language)
    {
        var s = new ScriptBuilder(language);
        bool py = s.IsPython;

        if (py)
        {
            s.Line("import itertools");
        }

        s.Imports(scipyStats: true);
        s.Comment($"Simulation: {Title} — {EnumLabel.Of(Experiment)}");
        s.Comment("The program and " + (py ? "numpy" : "R") + " draw different random numbers, so your counts differ from the program's.");
        s.Comment(CountsAnEvent
            ? $"With many trials the experimental probability should be close to the theoretical, {Show4(TheoreticalProbability)}."
            : $"With many trials the mean and standard deviation of the sample means should be close to {Show4(TheoreticalMean)} and {Show4(TheoreticalStandardError)}.");
        s.Assign("trials", TrialsRun > 0 ? TrialsRun : Whole(Trials, 1000));
        s.Assign("seed", Whole(Seed, 1));
        s.Either("rng = np.random.default_rng(seed)", "set.seed(seed)");
        s.Line();

        switch (Experiment)
        {
            case SimulationExperiment.Coins: CoinsCode(s); break;
            case SimulationExperiment.Dice: DiceCode(s); break;
            case SimulationExperiment.Cards: CardsCode(s); break;
            case SimulationExperiment.Darts: DartsCode(s); break;
            case SimulationExperiment.SampleMeans: SampleMeansCode(s); break;
            case SimulationExperiment.IntervalCoverage: CoverageCode(s); break;
            default: FamilywiseCode(s); break;
        }

        s.Line();
        s.PrintRaw("Trials run", "trials", "trials");

        if (CountsAnEvent)
        {
            s.PrintText("Event", EventText);
            s.PrintRaw("Times the event happened", "event.sum()", "sum(event)");
            s.Print("Experimental probability", "np.mean(event)", "mean(event)", 4);
            s.Print("Theoretical probability", "p_theory", "p_theory", 4);
            s.Print("Experimental − theoretical", "np.mean(event) - p_theory", "mean(event) - p_theory", 4);

            if (Experiment == SimulationExperiment.Darts)
            {
                s.Print("Estimate of π (4 × proportion)", "4 * np.mean(event)", "4 * mean(event)", 4);
            }
        }
        else
        {
            s.Print("Mean of the sample means", "np.mean(means)", "mean(means)", 4);
            s.Print("Standard deviation of the sample means", "np.std(means, ddof=1)", "sd(means)", 4);
            s.Print("Theoretical mean of x̄ (μ)", "mu", "mu", 4);
            s.Print("Theoretical standard error (σ/√n)", "sigma / np.sqrt(n)", "sigma / sqrt(n)", 4);
        }

        return s.ToString();
    }

    private void CoinsCode(ScriptBuilder s)
    {
        s.Assign("coins", Whole(Coins, 1)).Assign("target", Whole(Heads, 1));
        s.Comment("Each row is one trial; 1 = heads");
        s.Either("flips = rng.integers(0, 2, size=(trials, coins))", "flips <- matrix(sample(0:1, trials * coins, replace = TRUE), nrow = trials)");
        s.Either("outcome = flips.sum(axis=1)", "outcome <- rowSums(flips)");
        s.Either($"event = outcome {Operator} target", $"event <- outcome {Operator} target");
        s.Comment("Theory: the number of heads in fair flips is binomial(coins, 0.5)");
        s.Either("k = np.arange(coins + 1)", "k <- 0:coins");
        s.Either($"p_theory = stats.binom.pmf(k, coins, 0.5)[k {Operator} target].sum()", $"p_theory <- sum(dbinom(k, coins, 0.5)[k {Operator} target])");
    }

    private void DiceCode(ScriptBuilder s)
    {
        s.Assign("dice", Whole(Dice, 2)).Assign("target", Whole(DiceTotal, 7));
        s.Comment("Each row is one trial: the dice rolled, then their sum");
        s.Either("rolls = rng.integers(1, 7, size=(trials, dice))", "rolls <- matrix(sample(1:6, trials * dice, replace = TRUE), nrow = trials)");
        s.Either("outcome = rolls.sum(axis=1)", "outcome <- rowSums(rolls)");
        s.Either($"event = outcome {Operator} target", $"event <- outcome {Operator} target");
        s.Comment("Theory: list all 6^dice equally likely rolls and count the favourable ones");
        s.Either("all_rolls = np.array(list(itertools.product(range(1, 7), repeat=dice)))", "all_rolls <- expand.grid(rep(list(1:6), dice))");
        s.Either($"p_theory = np.mean(all_rolls.sum(axis=1) {Operator} target)", $"p_theory <- mean(rowSums(all_rolls) {Operator} target)");
    }

    private void CardsCode(ScriptBuilder s)
    {
        bool py = s.IsPython;
        s.Assign("cards", Whole(Cards, 3)).Assign("target", Whole(CardTarget, 21));
        s.Comment("Card c = 0..51: rank c % 13 + 1 (1 = ace, 11-13 = J, Q, K), suit c // 13 (0 = hearts)");
        s.Comment("Each row is one hand, dealt without replacement from a full deck");
        s.Either("hands = np.array([rng.choice(52, size=cards, replace=False) for _ in range(trials)])",
            "hands <- matrix(replicate(trials, sample(0:51, cards)), ncol = cards, byrow = TRUE)");
        s.Either("rank = hands % 13 + 1", "rank <- hands %% 13 + 1");

        switch (CardOutcome)
        {
            case CardOutcome.Hearts:
            case CardOutcome.Aces:
                bool hearts = CardOutcome == CardOutcome.Hearts;
                int marked = hearts ? 13 : 4;
                s.Either(hearts ? "outcome = (hands // 13 == 0).sum(axis=1)" : "outcome = (rank == 1).sum(axis=1)",
                    hearts ? "outcome <- rowSums(hands %/% 13 == 0)" : "outcome <- rowSums(rank == 1)");
                s.Either($"event = outcome {Operator} target", $"event <- outcome {Operator} target");
                s.Comment($"Theory: the number of {(hearts ? "hearts" : "aces")} is hypergeometric ({marked} of the 52 cards)");
                s.Either("k = np.arange(cards + 1)", "k <- 0:cards");
                s.Either($"p_theory = stats.hypergeom.pmf(k, 52, {marked}, cards)[k {Operator} target].sum()",
                    $"p_theory <- sum(dhyper(k, {marked}, {52 - marked}, cards)[k {Operator} target])");
                break;

            default:
                s.Comment("Total: J, Q, K count 10; one ace counts 11 when that keeps the total at 21 or under");
                if (py)
                {
                    s.Line("def best_total(rank):");
                    s.Line("    total = np.minimum(rank, 10).sum(axis=1)");
                    s.Line("    return np.where((rank == 1).any(axis=1) & (total + 10 <= 21), total + 10, total)");
                }
                else
                {
                    s.Line("best_total <- function(rank) {");
                    s.Line("  total <- rowSums(pmin(rank, 10))");
                    s.Line("  ifelse(rowSums(rank == 1) > 0 & total + 10 <= 21, total + 10, total)");
                    s.Line("}");
                }

                s.Either("outcome = best_total(rank)", "outcome <- best_total(rank)");
                s.Either($"event = outcome {Operator} target", $"event <- outcome {Operator} target");
                s.Comment("Theory: every ordered deal of ranks, weighted by the ways the deck can give it (4 x 3 x ... for repeats)");
                if (py)
                {
                    s.Line("ranks = np.array(list(itertools.product(range(1, 14), repeat=cards)))");
                    s.Line("weight = np.ones(len(ranks))");
                    s.Line("for j in range(cards):");
                    s.Line("    weight *= 4 - (ranks[:, :j] == ranks[:, [j]]).sum(axis=1)");
                    s.Line($"p_theory = weight[best_total(ranks) {Operator} target].sum() / weight.sum()");
                }
                else
                {
                    s.Line("ranks <- as.matrix(expand.grid(rep(list(1:13), cards)))");
                    s.Line("weight <- rep(1, nrow(ranks))");
                    s.Line("for (j in seq_len(cards)) weight <- weight * (4 - rowSums(ranks[, seq_len(j - 1), drop = FALSE] == ranks[, j]))");
                    s.Line($"p_theory <- sum(weight[best_total(ranks) {Operator} target]) / sum(weight)");
                }

                break;
        }
    }

    private static void DartsCode(ScriptBuilder s)
    {
        s.Comment("A dart lands uniformly on the square from -1 to 1; the event is landing inside the circle of radius 1");
        s.Either("x = rng.uniform(-1, 1, trials)", "x <- runif(trials, -1, 1)");
        s.Either("y = rng.uniform(-1, 1, trials)", "y <- runif(trials, -1, 1)");
        s.Either("event = x**2 + y**2 <= 1", "event <- x^2 + y^2 <= 1");
        s.Comment("Theory: the circle's area over the square's, pi / 4");
        s.Either("p_theory = np.pi / 4", "p_theory <- pi / 4");
    }

    private void SampleMeansCode(ScriptBuilder s)
    {
        s.Assign("n", Whole(SampleSize, 30));

        switch (Distribution)
        {
            case SampledDistribution.Normal:
                s.Assign("mu", PopulationMean).Assign("sigma", PopulationSigma);
                s.Either("samples = rng.normal(mu, sigma, size=(trials, n))", "means <- replicate(trials, mean(rnorm(n, mu, sigma)))");
                break;
            case SampledDistribution.Uniform:
                s.Assign("a", Lower).Assign("b", Upper);
                s.Either("mu = (a + b) / 2; sigma = (b - a) / np.sqrt(12)", "mu <- (a + b) / 2; sigma <- (b - a) / sqrt(12)");
                s.Either("samples = rng.uniform(a, b, size=(trials, n))", "means <- replicate(trials, mean(runif(n, a, b)))");
                break;
            default:
                s.Assign("rate", Rate);
                s.Either("mu = 1 / rate; sigma = 1 / rate", "mu <- 1 / rate; sigma <- 1 / rate");
                s.Either("samples = rng.exponential(1 / rate, size=(trials, n))   # numpy takes the scale, 1/λ", "means <- replicate(trials, mean(rexp(n, rate)))");
                break;
        }

        if (s.IsPython)
        {
            s.Line("means = samples.mean(axis=1)   # one x̄ per row (sample)");
        }
    }

    private void CoverageCode(ScriptBuilder s)
    {
        s.Assign("mu", PopulationMean).Assign("sigma", PopulationSigma).Assign("n", Whole(SampleSize, 10)).Assign("conf", Confidence);
        s.Comment("Each trial: a sample of n from the normal population, its t interval, and whether it contains mu");
        if (s.IsPython)
        {
            s.Line("x = rng.normal(mu, sigma, size=(trials, n))");
            s.Line("xbar = x.mean(axis=1); s = x.std(axis=1, ddof=1)");
            s.Line("margin = stats.t.ppf(1 - (1 - conf) / 2, n - 1) * s / np.sqrt(n)");
            s.Line("event = (xbar - margin <= mu) & (mu <= xbar + margin)");
        }
        else
        {
            s.Line("event <- replicate(trials, {");
            s.Line("  ci <- t.test(rnorm(n, mu, sigma), conf.level = conf)$conf.int");
            s.Line("  ci[1] <= mu && mu <= ci[2]");
            s.Line("})");
        }

        s.Comment("Theory: in the long run the confidence level is the share of intervals that contain mu");
        s.Assign("p_theory", "conf");
    }

    private void FamilywiseCode(ScriptBuilder s)
    {
        s.Assign("tests", Whole(Tests, 10)).Assign("n", Whole(SampleSize, 10)).Assign("alpha", Alpha);
        s.Comment("Each trial: several two-sided one-sample t-tests of mu = 0 on standard normal data, so every null is true");
        if (s.IsPython)
        {
            s.Line("x = rng.standard_normal((trials, tests, n))");
            s.Line("t = x.mean(axis=2) / (x.std(axis=2, ddof=1) / np.sqrt(n))");
            s.Line("rejections = (np.abs(t) > stats.t.ppf(1 - alpha / 2, n - 1)).sum(axis=1)");
            s.Line("event = rejections >= 1");
        }
        else
        {
            s.Line("rejections <- replicate(trials, sum(replicate(tests, t.test(rnorm(n))$p.value < alpha)))");
            s.Line("event <- rejections >= 1");
        }

        s.Comment("Theory: the chance that none of the independent tests rejects is (1 - alpha)^tests");
        s.Either("p_theory = 1 - (1 - alpha) ** tests", "p_theory <- 1 - (1 - alpha)^tests");
    }

    private static int Whole(double value, int fallback) => double.IsNaN(value) ? fallback : (int)Math.Round(value);

    private static string Show4(double value) => double.IsNaN(value) ? "—" : value.ToString("F4", CultureInfo.InvariantCulture);
}

/// <summary>
/// Each experiment is a method naming the entries it uses, so the form shows only those: the
/// coins per trial for coins, the population for sample means, and so on.
/// </summary>
public abstract class SimulationMethod : CalculationMethod<Simulation>
{
    public abstract SimulationExperiment Selects { get; }

    public override bool IsApplicableTo(Simulation subject) => subject is not null && subject.Experiment == Selects;

    public override string Citation =>
        "EGN3443 Modules 3, 6, 7 and 8; Montgomery, D. C. and Runger, G. C., Applied Statistics and " +
        "Probability for Engineers, 7th ed., Wiley, 2018, §2-1 (relative frequency), §7-2 (the central " +
        "limit theorem), §8-2 (interpreting a confidence interval); random numbers by xoshiro256** " +
        "(Blackman, D. and Vigna, S., Scrambled linear pseudorandom number generators, ACM TOMS 47(4), 2021).";

    /// <summary>The results of an experiment that counts an event.</summary>
    protected static IReadOnlyList<string> EventResults =>
    [
        nameof(Simulation.EventText),
        nameof(Simulation.Occurrences),
        nameof(Simulation.ExperimentalProbability),
        nameof(Simulation.TheoreticalProbability),
        nameof(Simulation.Difference),
    ];

    public override IReadOnlyList<string> Outputs => EventResults;

    public override void Apply(Simulation subject) => subject.Recalculate();
}

public sealed class CoinsSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.Coins;

    public override string Name => "Coin flips";

    public override string Description => "Heads in each trial of fair flips; in theory binomial(coins, ½).";

    public override IReadOnlyList<string> Inputs => [nameof(Simulation.Coins), nameof(Simulation.EventIs), nameof(Simulation.Heads)];
}

public sealed class DiceSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.Dice;

    public override string Name => "Dice";

    public override string Description => "The sum of fair dice; in theory the count of the 6ⁿ equally likely rolls with that sum.";

    public override IReadOnlyList<string> Inputs => [nameof(Simulation.Dice), nameof(Simulation.EventIs), nameof(Simulation.DiceTotal)];
}

public sealed class CardsSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.Cards;

    public override string Name => "Cards";

    public override string Description =>
        "Hands dealt without replacement from 52 cards; in theory every possible hand counted (the total) or the hypergeometric (hearts, aces).";

    public override IReadOnlyList<string> Inputs =>
        [nameof(Simulation.Cards), nameof(Simulation.CardOutcome), nameof(Simulation.EventIs), nameof(Simulation.CardTarget)];
}

public sealed class DartsSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.Darts;

    public override string Name => "Darts";

    public override string Description => "Points uniform on a 2 × 2 square; P(inside the unit circle) = π/4, so 4 × the proportion estimates π.";

    public override IReadOnlyList<string> Inputs => [];

    public override IReadOnlyList<string> Outputs => [.. EventResults, nameof(Simulation.PiEstimate)];
}

/// <summary>The central limit theorem: many samples' means, against a normal of mean μ and standard deviation σ/√n.</summary>
public abstract class SampleMeansSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.SampleMeans;

    public abstract SampledDistribution Population { get; }

    public override bool IsApplicableTo(Simulation subject) => base.IsApplicableTo(subject) && subject.Distribution == Population;

    public override string Description => "x̄ of many samples of size n; the central limit theorem: x̄ ≈ N(μ, σ²/n).";

    protected IReadOnlyList<string> Shared => [nameof(Simulation.Distribution), nameof(Simulation.SampleSize)];

    public override IReadOnlyList<string> Outputs =>
    [
        nameof(Simulation.MeanOfMeans),
        nameof(Simulation.SdOfMeans),
        nameof(Simulation.TheoreticalMean),
        nameof(Simulation.TheoreticalStandardError),
    ];
}

public sealed class NormalSampleMeans : SampleMeansSimulation
{
    public override SampledDistribution Population => SampledDistribution.Normal;

    public override string Name => "Sample means from a normal population";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(Simulation.PopulationMean), nameof(Simulation.PopulationSigma)];
}

public sealed class UniformSampleMeans : SampleMeansSimulation
{
    public override SampledDistribution Population => SampledDistribution.Uniform;

    public override string Name => "Sample means from a uniform population";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(Simulation.Lower), nameof(Simulation.Upper)];
}

public sealed class ExponentialSampleMeans : SampleMeansSimulation
{
    public override SampledDistribution Population => SampledDistribution.Exponential;

    public override string Name => "Sample means from an exponential population";

    public override IReadOnlyList<string> Inputs => [.. Shared, nameof(Simulation.Rate)];
}

public sealed class CoverageSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.IntervalCoverage;

    public override string Name => "Confidence interval coverage";

    public override string Description => "t intervals x̄ ± t(α/2, n − 1)·s/√n from normal samples; the share containing μ approaches the confidence level.";

    public override IReadOnlyList<string> Inputs =>
        [nameof(Simulation.PopulationMean), nameof(Simulation.PopulationSigma), nameof(Simulation.SampleSize), nameof(Simulation.Confidence)];
}

public sealed class FamilywiseSimulation : SimulationMethod
{
    public override SimulationExperiment Selects => SimulationExperiment.FamilywiseError;

    public override string Name => "Familywise error of many tests";

    public override string Description => "m independent two-sided t-tests of true null hypotheses; P(at least one rejects) = 1 − (1 − α)ᵐ.";

    public override IReadOnlyList<string> Inputs => [nameof(Simulation.SampleSize), nameof(Simulation.Alpha), nameof(Simulation.Tests)];
}
