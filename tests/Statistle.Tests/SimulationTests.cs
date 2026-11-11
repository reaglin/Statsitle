using System.Xml.Linq;
using Statistle.Core.Analyses;
using Statistle.Core.Code;
using Statistle.Core.Methods;
using Statistle.Core.Methods.Chance;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;

namespace Statistle.Tests;

/// <summary>
/// Plan 10.7: the Simulation analysis. The random numbers are the program's own, checked against
/// the published reference outputs; a seeded run is pinned so a course task's expected numbers
/// never change; the theory is checked against counting; and the proportion converges.
/// </summary>
public class SimulationTests
{
    /// <summary>Recalculates after the object initializer has set the entries, as the form does after each edit.</summary>
    private static Simulation Run(Simulation simulation)
    {
        simulation.Recalculate();
        return simulation;
    }

    // ── The random numbers ────────────────────────────────────────────────────

    [Fact]
    public void Xoshiro256_star_star_gives_the_reference_implementations_outputs()
    {
        // From state {1, 2, 3, 4}: the outputs of Blackman and Vigna's xoshiro256starstar.c (also rand_xoshiro's test vector).
        var random = RandomStream.FromState(1, 2, 3, 4);
        ulong[] expected = [11520, 0, 1509978240, 1215971899390074240, 1216172134540287360, 607988272756665600];

        ulong[] actual = [.. expected.Select(_ => random.NextUInt64())];
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitMix64_seeding_gives_the_reference_outputs()
    {
        ulong state = 0;
        Assert.Equal(0xE220A8397B1DCDAFUL, RandomStream.SplitMix64(ref state));   // SplitMix64's first output from 0

        var random = new RandomStream(1);   // seed 1, as a task's default
        Assert.Equal(0xB3F2AF6D0FC710C5UL, random.NextUInt64());
        Assert.Equal(0x853B559647364CEAUL, random.NextUInt64());
        Assert.Equal(0x92F89756082A4514UL, random.NextUInt64());
    }

    [Fact]
    public void Draws_stay_in_range_and_cover_it()
    {
        var random = new RandomStream(7);
        int[] faces = new int[6];

        for (int i = 0; i < 60_000; i++)
        {
            double u = random.NextDouble();
            Assert.InRange(u, 0, 1 - 1e-17);
            faces[random.NextInt(6)]++;
        }

        Assert.All(faces, f => Assert.InRange(f, 9_500, 10_500));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(0));
    }

    // ── Seeded results, pinned: a course task grades these numbers ────────────

    public static TheoryData<string, Simulation, int, double> Pinned() => new()
    {
        { "two dice, sum 7", new Simulation(), 168, 1.0 / 6 },
        { "one coin, heads", new Simulation { Experiment = SimulationExperiment.Coins }, 499, 0.5 },
        { "three cards make 21", new Simulation { Experiment = SimulationExperiment.Cards }, 93, 2012.0 / 22100 },
        { "one heart in three cards", new Simulation { Experiment = SimulationExperiment.Cards, CardOutcome = CardOutcome.Hearts, CardTarget = 1 }, 455, 9633.0 / 22100 },
        { "10 000 darts", new Simulation { Experiment = SimulationExperiment.Darts, Trials = 10_000 }, 7914, Math.PI / 4 },
        { "95 % intervals, n = 10", new Simulation { Experiment = SimulationExperiment.IntervalCoverage, SampleSize = 10 }, 949, 0.95 },
        { "10 t-tests at 0.05", new Simulation { Experiment = SimulationExperiment.FamilywiseError, SampleSize = 10 }, 410, 1 - Math.Pow(0.95, 10) },
    };

    [Theory]
    [MemberData(nameof(Pinned))]
    public void A_seeded_run_gives_the_same_count_forever(string name, Simulation simulation, int occurrences, double theory)
    {
        simulation.Recalculate();

        Assert.True(simulation.Problem is null, $"{name}: {simulation.Problem}");
        Assert.Equal(occurrences, simulation.Occurrences);
        Assert.Equal((double)occurrences / simulation.TrialsRun, simulation.ExperimentalProbability, 12);
        Assert.Equal(theory, simulation.TheoreticalProbability, 12);
        Assert.Equal(simulation.ExperimentalProbability - theory, simulation.Difference, 12);
    }

    [Fact]
    public void Seeded_sample_means_and_the_darts_estimate_of_pi_are_pinned()
    {
        var means = Run(new Simulation { Experiment = SimulationExperiment.SampleMeans });   // exponential, λ = 1, n = 30, 1000 samples, seed 1
        Assert.Null(means.Problem);
        Assert.Equal(0.9962, means.MeanOfMeans, 4);
        Assert.Equal(0.1790, means.SdOfMeans, 4);
        Assert.Equal(1, means.TheoreticalMean, 12);
        Assert.Equal(1 / Math.Sqrt(30), means.TheoreticalStandardError, 12);

        var darts = Run(new Simulation { Experiment = SimulationExperiment.Darts, Trials = 10_000 });
        Assert.Equal(3.1656, darts.PiEstimate, 12);
    }

    [Fact]
    public void The_same_seed_repeats_and_another_seed_does_not()
    {
        var a = new Simulation { Seed = 42 };
        var b = new Simulation { Seed = 42 };
        var c = new Simulation { Seed = 43 };
        a.Recalculate();
        b.Recalculate();
        c.Recalculate();

        Assert.Equal(a.Occurrences, b.Occurrences);
        Assert.Equal(a.RunningProportion, b.RunningProportion);
        Assert.NotEqual(a.RunningProportion, c.RunningProportion);
    }

    // ── Theory, by counting ───────────────────────────────────────────────────

    [Fact]
    public void The_theory_is_what_counting_the_cases_gives()
    {
        Assert.Equal(64.0 / 1326, Experiments.CardTotalTheory(2)[21], 12);        // two-card 21: 4 aces × 16 tens over C(52, 2)
        Assert.Equal(2012.0 / 22100, Experiments.CardTotalTheory(3)[21], 12);     // three-card 21, by brute force over the 22 100 hands
        Assert.Equal(6.0 / 36, Experiments.DiceTheory(2)[7], 12);
        Assert.Equal(27.0 / 216, Experiments.DiceTheory(3)[10], 12);
        Assert.Equal(3.0 / 8, Experiments.CoinTheory(3)[2], 12);
        Assert.Equal(4.0 / 52, Experiments.MarkedCardTheory(4, 1)[1], 12);

        foreach (IReadOnlyDictionary<int, double> theory in new[]
                 {
                     Experiments.CoinTheory(20), Experiments.DiceTheory(6), Experiments.CardTotalTheory(5),
                     Experiments.MarkedCardTheory(13, 5), Experiments.MarkedCardTheory(4, 5),
                 })
        {
            Assert.Equal(1, theory.Values.Sum(), 10);
        }

        var majority = Run(new Simulation { Experiment = SimulationExperiment.Coins, Coins = 3, EventIs = EventComparison.AtLeast, Heads = 2 });
        Assert.Equal(0.5, majority.TheoreticalProbability, 12);
        var atMost = Run(new Simulation { DiceTotal = 4, EventIs = EventComparison.AtMost });
        Assert.Equal(6.0 / 36, atMost.TheoreticalProbability, 12);
        var impossible = Run(new Simulation { DiceTotal = 13 });
        Assert.Equal(0, impossible.TheoreticalProbability);
        Assert.Equal(0, impossible.Occurrences);
    }

    [Fact]
    public void The_experimental_probability_settles_on_the_theory_with_many_trials()
    {
        foreach (int seed in new[] { 1, 2, 3 })
        {
            var dice = Run(new Simulation { Trials = Simulation.MaximumTrials, Seed = seed });
            Assert.True(Math.Abs(dice.Difference) < 0.01, $"seed {seed}: {dice.Difference}");

            var cards = Run(new Simulation { Experiment = SimulationExperiment.Cards, Trials = Simulation.MaximumTrials, Seed = seed });
            Assert.True(Math.Abs(cards.Difference) < 0.01, $"seed {seed}: {cards.Difference}");
        }

        var means = Run(new Simulation { Experiment = SimulationExperiment.SampleMeans, Distribution = SampledDistribution.Uniform, Lower = 2, Upper = 8, SampleSize = 12, Trials = 20_000 });
        Assert.Equal(5, means.MeanOfMeans, 1);
        Assert.Equal(means.TheoreticalStandardError, means.SdOfMeans, 2);   // (8 − 2)/√12/√12 = 0.5
        Assert.Equal(0.5, means.TheoreticalStandardError, 12);

        var normal = Run(new Simulation { Experiment = SimulationExperiment.SampleMeans, Distribution = SampledDistribution.Normal, Trials = 20_000 });
        Assert.Equal(50, normal.MeanOfMeans, 1);
        Assert.Equal(10 / Math.Sqrt(30), normal.SdOfMeans, 1);
    }

    // ── Blank and wrong entries ───────────────────────────────────────────────

    public static TheoryData<string, Simulation> BadEntries() => new()
    {
        { "no trials", new Simulation { Trials = double.NaN } },
        { "a fraction of a trial", new Simulation { Trials = 2.5 } },
        { "too many trials", new Simulation { Trials = Simulation.MaximumTrials + 1 } },
        { "a negative seed", new Simulation { Seed = -1 } },
        { "21 coins", new Simulation { Experiment = SimulationExperiment.Coins, Coins = 21 } },
        { "seven dice", new Simulation { Dice = 7 } },
        { "no target", new Simulation { DiceTotal = double.NaN } },
        { "six cards", new Simulation { Experiment = SimulationExperiment.Cards, Cards = 6 } },
        { "upper below lower", new Simulation { Experiment = SimulationExperiment.SampleMeans, Distribution = SampledDistribution.Uniform, Lower = 3, Upper = 1 } },
        { "no rate", new Simulation { Experiment = SimulationExperiment.SampleMeans, Rate = double.NaN } },
        { "an interval from one value", new Simulation { Experiment = SimulationExperiment.IntervalCoverage, SampleSize = 1 } },
        { "confidence 1", new Simulation { Experiment = SimulationExperiment.IntervalCoverage, Confidence = 1 } },
        { "α blank", new Simulation { Experiment = SimulationExperiment.FamilywiseError, Alpha = double.NaN } },
    };

    [Theory]
    [MemberData(nameof(BadEntries))]
    public void A_bad_entry_says_what_is_wrong_and_leaves_the_results_blank(string name, Simulation simulation)
    {
        simulation.Recalculate();

        Assert.False(string.IsNullOrEmpty(simulation.Problem), name);
        Assert.Equal(0, simulation.TrialsRun);
        Assert.True(double.IsNaN(simulation.ExperimentalProbability), name);
        Assert.True(double.IsNaN(simulation.MeanOfMeans), name);
        Assert.Empty(simulation.Figures(null));
    }

    // ── The form, the figures, the code, the file ─────────────────────────────

    [Fact]
    public void The_form_shows_only_the_entries_and_results_of_the_chosen_experiment()
    {
        static HashSet<string> Visible(Simulation s) => [.. MethodScope.VisibleProperties(s).Select(p => p.XmlName)];

        HashSet<string> coins = Visible(new Simulation { Experiment = SimulationExperiment.Coins });
        Assert.Contains(nameof(Simulation.Coins), coins);
        Assert.Contains(nameof(Simulation.Heads), coins);
        Assert.Contains(nameof(Simulation.Seed), coins);
        Assert.DoesNotContain(nameof(Simulation.Dice), coins);
        Assert.DoesNotContain(nameof(Simulation.PiEstimate), coins);
        Assert.DoesNotContain(nameof(Simulation.MeanOfMeans), coins);

        Assert.Contains(nameof(Simulation.PiEstimate), Visible(new Simulation { Experiment = SimulationExperiment.Darts }));

        HashSet<string> exponential = Visible(new Simulation { Experiment = SimulationExperiment.SampleMeans });
        Assert.Contains(nameof(Simulation.Rate), exponential);
        Assert.DoesNotContain(nameof(Simulation.PopulationSigma), exponential);
        Assert.DoesNotContain(nameof(Simulation.EventIs), exponential);
        Assert.DoesNotContain(nameof(Simulation.ExperimentalProbability), exponential);

        HashSet<string> coverage = Visible(new Simulation { Experiment = SimulationExperiment.IntervalCoverage });
        Assert.Contains(nameof(Simulation.PopulationSigma), coverage);
        Assert.Contains(nameof(Simulation.Confidence), coverage);
        Assert.DoesNotContain(nameof(Simulation.Distribution), coverage);

        Assert.All(MethodCatalog.For(typeof(Simulation)), m => Assert.Empty(MethodContract.Verify(m)));
    }

    [Fact]
    public void The_figures_are_the_running_proportion_then_the_outcomes()
    {
        var dice = new Simulation();
        IReadOnlyList<PlotSpec> figures = dice.Figures(null);

        Assert.Equal(2, figures.Count);
        Assert.Contains("running proportion", figures[0].Title, StringComparison.Ordinal);
        LineSeries running = figures[0].Series.OfType<LineSeries>().Single(s => s.Name == "Proportion so far");
        Assert.Equal(dice.ExperimentalProbability, running.Y[^1], 12);
        Assert.Equal(1000, running.X[^1]);
        Assert.Equal(11, dice.OutcomeTable.Count);   // sums 2 … 12
        Assert.Equal(1, dice.OutcomeTable.Sum(r => r.Share), 12);
        Assert.Single(dice.OutcomeTable, r => r.IsEvent);

        Assert.Equal(2000, Run(new Simulation { Experiment = SimulationExperiment.Darts, Trials = 5000 }).DartsThrown.Count);
        Assert.Equal(100, Run(new Simulation { Experiment = SimulationExperiment.IntervalCoverage }).IntervalsBuilt.Count);

        IReadOnlyList<PlotSpec> means = Run(new Simulation { Experiment = SimulationExperiment.SampleMeans }).Figures(null);
        Assert.Single(means);
        Assert.Contains(means[0].Series, s => s is BarSeries);
        Assert.Contains(means[0].Series, s => s is LineSeries);
    }

    [Fact]
    public void The_code_seeds_numpy_and_R_and_says_its_counts_will_differ()
    {
        var simulation = new Simulation { Seed = 7 };
        string python = simulation.Code(CodeLanguage.Python);
        string r = simulation.Code(CodeLanguage.R);

        Assert.Contains("seed = 7", python, StringComparison.Ordinal);
        Assert.Contains("np.random.default_rng(seed)", python, StringComparison.Ordinal);
        Assert.Contains("set.seed(seed)", r, StringComparison.Ordinal);
        Assert.Contains("your counts differ from the program's", python, StringComparison.Ordinal);
        Assert.Contains("close to the theoretical, 0.1667", r, StringComparison.Ordinal);

        Assert.Single(typeof(Simulation).GetProperty(nameof(Simulation.Occurrences))!.GetCustomAttributes(typeof(SimulatedResultAttribute), false));
        Assert.Empty(typeof(Simulation).GetProperty(nameof(Simulation.TheoreticalProbability))!.GetCustomAttributes(typeof(SimulatedResultAttribute), false));
    }

    [Fact]
    public void A_saved_simulation_reloads_its_settings_and_gives_the_same_results()
    {
        var original = new Simulation
        {
            Experiment = SimulationExperiment.Cards,
            CardOutcome = CardOutcome.Aces,
            Cards = 5,
            EventIs = EventComparison.AtLeast,
            CardTarget = 1,
            Trials = 2500,
            Seed = 2026,
        };
        original.Recalculate();

        XElement written = StatistleXml.Write(original, "analysis");
        var reloaded = new Simulation();
        var diagnostics = new LoadDiagnostics();
        StatistleXml.Read(reloaded, written, diagnostics);
        reloaded.Recalculate();

        Assert.False(diagnostics.HasWarnings, diagnostics.ToString());
        Assert.Equal(original.Experiment, reloaded.Experiment);
        Assert.Equal(original.CardOutcome, reloaded.CardOutcome);
        Assert.Equal(2026, reloaded.Seed);
        Assert.Equal(original.Occurrences, reloaded.Occurrences);
        Assert.Equal(1 - 1712304.0 / 2598960, reloaded.TheoreticalProbability, 12);   // at least one ace in five cards: 1 − C(48, 5)/C(52, 5)
    }
}
