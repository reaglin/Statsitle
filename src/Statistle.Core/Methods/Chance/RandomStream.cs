namespace Statistle.Core.Methods.Chance;

/// <summary>
/// The program's own random numbers: <b>xoshiro256**</b> (Blackman and Vigna, 2018), seeded by
/// <b>SplitMix64</b> as its authors recommend. Written out here, rather than taken from
/// <see cref="System.Random"/>, because a course task with a seed grades the numbers a simulation
/// gives, and those must be the same on every computer and in every future version of .NET.
/// <see cref="System.Random"/>'s sequence is not a documented contract (it changed in .NET 6);
/// this one is fixed by the published algorithm and the tests that pin its first values.
///
/// <para>Integer draws (<see cref="NextInt"/>) use only integer arithmetic, so coins, dice and
/// cards are exact everywhere. <see cref="NextDouble"/> is an exact multiple of 2⁻⁵³.
/// <see cref="NextNormal"/> and <see cref="NextExponential"/> use <see cref="Math.Log"/>,
/// <see cref="Math.Sqrt"/>, <see cref="Math.Cos"/> and <see cref="Math.Sin"/>; Sqrt is exact by IEEE 754
/// and the others are within an ulp or so on every platform .NET runs on, far below the four
/// decimals any result is shown to.</para>
/// </summary>
public sealed class RandomStream
{
    private ulong _s0, _s1, _s2, _s3;
    private double _spareNormal = double.NaN;

    public RandomStream(ulong seed)
    {
        ulong x = seed;
        _s0 = SplitMix64(ref x);
        _s1 = SplitMix64(ref x);
        _s2 = SplitMix64(ref x);
        _s3 = SplitMix64(ref x);
    }

    private RandomStream(ulong s0, ulong s1, ulong s2, ulong s3) => (_s0, _s1, _s2, _s3) = (s0, s1, s2, s3);

    /// <summary>A stream started from a raw xoshiro256** state — for checking against the reference implementation's published outputs.</summary>
    public static RandomStream FromState(ulong s0, ulong s1, ulong s2, ulong s3) => new(s0, s1, s2, s3);

    /// <summary>SplitMix64 (Steele, Lea and Flood, 2014): one step, advancing <paramref name="state"/>.</summary>
    public static ulong SplitMix64(ref ulong state)
    {
        unchecked
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>The next 64 random bits (xoshiro256**).</summary>
    public ulong NextUInt64()
    {
        unchecked
        {
            ulong result = RotateLeft(_s1 * 5, 7) * 9;
            ulong t = _s1 << 17;

            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;
            _s2 ^= t;
            _s3 = RotateLeft(_s3, 45);

            return result;
        }
    }

    /// <summary>A uniform number in [0, 1): the top 53 bits times 2⁻⁵³.</summary>
    public double NextDouble() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));

    /// <summary>
    /// A uniform whole number from 0 to <paramref name="bound"/> − 1, without bias: draws whose top
    /// bits fall in the incomplete last block are rejected and drawn again.
    /// </summary>
    public int NextInt(int bound)
    {
        if (bound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), "The bound must be at least 1.");
        }

        ulong n = (ulong)bound;
        ulong limit = ulong.MaxValue - (ulong.MaxValue % n);   // the largest multiple of n not above 2⁶⁴ − 1

        while (true)
        {
            ulong r = NextUInt64();

            if (r < limit)
            {
                return (int)(r % n);
            }
        }
    }

    /// <summary>A standard normal number, by the Box–Muller transform; the second of each pair is kept for the next call.</summary>
    public double NextNormal()
    {
        if (!double.IsNaN(_spareNormal))
        {
            double spare = _spareNormal;
            _spareNormal = double.NaN;
            return spare;
        }

        double u1 = 1.0 - NextDouble();   // (0, 1], so the logarithm is finite
        double u2 = NextDouble();
        double radius = Math.Sqrt(-2.0 * Math.Log(u1));
        double angle = 2.0 * Math.PI * u2;
        _spareNormal = radius * Math.Sin(angle);
        return radius * Math.Cos(angle);
    }

    /// <summary>An exponential number with the given rate, −ln(1 − U)/λ (the inverse of the CDF).</summary>
    public double NextExponential(double rate) => -Math.Log(1.0 - NextDouble()) / rate;

    private static ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));
}
