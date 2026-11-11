namespace Statistle.Core.Methods.Chance;

/// <summary>
/// One trial of each simple experiment, drawn from a <see cref="RandomStream"/>, and the exact
/// probabilities of its outcomes found by counting the equally likely cases. The analysis repeats
/// a trial many times; the theory is what the repetitions should approach.
///
/// <para>A card is a number 0–51: its rank is <c>c % 13 + 1</c> (1 = ace, 11, 12, 13 = jack, queen,
/// king) and its suit <c>c / 13</c> (0 = hearts). Jack, queen and king count 10; an ace counts 1,
/// or 11 when that does not take the hand past 21 (only one ace can ever count 11).</para>
/// </summary>
public static class Experiments
{
    public const int DeckSize = 52;

    /// <summary>The number of heads in <paramref name="coins"/> fair coin flips.</summary>
    public static int Heads(RandomStream random, int coins)
    {
        int heads = 0;

        for (int i = 0; i < coins; i++)
        {
            heads += random.NextInt(2);
        }

        return heads;
    }

    /// <summary>The sum of <paramref name="dice"/> fair six-sided dice.</summary>
    public static int DiceSum(RandomStream random, int dice)
    {
        int sum = 0;

        for (int i = 0; i < dice; i++)
        {
            sum += random.NextInt(6) + 1;
        }

        return sum;
    }

    /// <summary>A fresh deck, cards 0–51 in order.</summary>
    public static int[] NewDeck() => [.. Enumerable.Range(0, DeckSize)];

    /// <summary>
    /// Deals <paramref name="cards"/> cards without replacement by a partial Fisher–Yates shuffle
    /// of <paramref name="deck"/>: the hand is the first <paramref name="cards"/> places. The deck
    /// is left shuffled, which is fine — a Fisher–Yates pass over any order deals a uniform hand.
    /// </summary>
    public static ReadOnlySpan<int> Deal(RandomStream random, int[] deck, int cards)
    {
        for (int i = 0; i < cards; i++)
        {
            int j = i + random.NextInt(deck.Length - i);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        return deck.AsSpan(0, cards);
    }

    public static int Rank(int card) => card % 13 + 1;

    public static bool IsHeart(int card) => card / 13 == 0;

    /// <summary>A hand's best total: J, Q, K count 10, an ace 1 — or 11 if that keeps the total at 21 or under.</summary>
    public static int BestTotal(ReadOnlySpan<int> ranks)
    {
        int total = 0;
        bool ace = false;

        foreach (int rank in ranks)
        {
            total += Math.Min(rank, 10);
            ace |= rank == 1;
        }

        return ace && total + 10 <= 21 ? total + 10 : total;
    }

    /// <summary>The ranks of a dealt hand.</summary>
    public static int[] Ranks(ReadOnlySpan<int> hand)
    {
        int[] ranks = new int[hand.Length];

        for (int i = 0; i < hand.Length; i++)
        {
            ranks[i] = Rank(hand[i]);
        }

        return ranks;
    }

    /// <summary>P(heads = k) for k = 0…coins: the binomial with p = ½, as C(coins, k) / 2^coins.</summary>
    public static IReadOnlyDictionary<int, double> CoinTheory(int coins)
    {
        Dictionary<int, double> p = [];
        double ways = 1;   // C(coins, k), built up exactly for coins ≤ 60

        for (int k = 0; k <= coins; k++)
        {
            p[k] = ways / Math.Pow(2, coins);
            ways = ways * (coins - k) / (k + 1);
        }

        return p;
    }

    /// <summary>P(sum = s) for <paramref name="dice"/> dice: the ways to roll s, counted by convolution, over 6^dice.</summary>
    public static IReadOnlyDictionary<int, double> DiceTheory(int dice)
    {
        long[] ways = [1];   // ways[i]: rolls with sum (number of dice so far) + i

        for (int d = 0; d < dice; d++)
        {
            long[] next = new long[ways.Length + 5];

            for (int i = 0; i < ways.Length; i++)
            {
                for (int face = 0; face < 6; face++)
                {
                    next[i + face] += ways[i];
                }
            }

            ways = next;
        }

        double all = Math.Pow(6, dice);
        Dictionary<int, double> p = [];

        for (int i = 0; i < ways.Length; i++)
        {
            p[dice + i] = ways[i] / all;
        }

        return p;
    }

    /// <summary>
    /// P(best total = t) for a hand of <paramref name="cards"/> cards, counted exactly: every
    /// ordered deal of ranks, weighted by the number of ways the deck can give it
    /// (4 · 3 · … for repeats of a rank), over 52 · 51 · … · (52 − cards + 1).
    /// </summary>
    public static IReadOnlyDictionary<int, double> CardTotalTheory(int cards)
    {
        Dictionary<int, double> weights = [];
        int[] ranks = new int[cards];
        Array.Fill(ranks, 1);
        double all = 0;

        while (true)
        {
            double weight = 1;

            for (int j = 0; j < cards && weight > 0; j++)
            {
                int earlier = 0;

                for (int i = 0; i < j; i++)
                {
                    earlier += ranks[i] == ranks[j] ? 1 : 0;
                }

                weight *= 4 - earlier;
            }

            if (weight > 0)
            {
                int total = BestTotal(ranks);
                weights[total] = weights.GetValueOrDefault(total) + weight;
                all += weight;
            }

            // The next ordered deal of ranks, like an odometer in base 13.
            int at = cards - 1;

            while (at >= 0 && ranks[at] == 13)
            {
                ranks[at] = 1;
                at--;
            }

            if (at < 0)
            {
                break;
            }

            ranks[at]++;
        }

        return weights.OrderBy(w => w.Key).ToDictionary(w => w.Key, w => w.Value / all);
    }

    /// <summary>
    /// P(k marked cards) in a hand of <paramref name="cards"/> from a deck of 52 with
    /// <paramref name="marked"/> marked cards (13 hearts, 4 aces): the hypergeometric,
    /// C(marked, k) · C(52 − marked, cards − k) / C(52, cards).
    /// </summary>
    public static IReadOnlyDictionary<int, double> MarkedCardTheory(int marked, int cards)
    {
        Dictionary<int, double> p = [];
        double all = Choose(DeckSize, cards);

        for (int k = 0; k <= Math.Min(marked, cards); k++)
        {
            p[k] = Choose(marked, k) * Choose(DeckSize - marked, cards - k) / all;
        }

        return p;
    }

    private static double Choose(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }

        double c = 1;

        for (int i = 1; i <= k; i++)
        {
            c = c * (n - k + i) / i;
        }

        return Math.Round(c);
    }
}
