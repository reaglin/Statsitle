namespace Statistle.Tests.Harness;

/// <summary>
/// Assertions that must hold for <em>any</em> correct implementation of a routine, whatever the
/// numbers come out as.
///
/// This is the answer to the plan's highest risk: a revised engine has no external oracle, so
/// VB3 output is not a correctness standard. Once a routine is meant to produce a different
/// answer, matching the old one is a failure to apply the change. What survives a revision is
/// continuity, shape, bounds, and behaviour at the degenerate cases — so those are asserted
/// directly, over the whole input range rather than at a few points.
/// </summary>
public static class Invariants
{
    /// <summary>Default sweep resolution. Enough to catch a sign error or a discontinuity.</summary>
    public const int DefaultSteps = 200;

    /// <summary>
    /// Nothing is created or destroyed: for every input, the parts sum to the whole within
    /// tolerance — a total that must equal the sum of its components.
    /// </summary>
    public static void Conserves(
        Func<double, double> whole,
        Func<double, double> parts,
        double from,
        double to,
        string because,
        double tolerance = 1e-9,
        int steps = DefaultSteps)
    {
        foreach (double x in Sweep(from, to, steps))
        {
            double expected = whole(x);
            double actual = parts(x);

            Assert.True(
                Math.Abs(expected - actual) <= tolerance,
                $"Continuity violated at {x:G6}: whole = {expected:G10}, parts sum to " +
                $"{actual:G10}, difference {expected - actual:G6}. {because}");
        }
    }

    /// <summary>A cumulative curve never decreases; a stage-storage relation never decreases.</summary>
    public static void NonDecreasing(
        Func<double, double> f, double from, double to, string because, int steps = DefaultSteps)
    {
        double previous = f(from);

        foreach (double x in Sweep(from, to, steps).Skip(1))
        {
            double current = f(x);

            Assert.True(
                current >= previous - 1e-12,
                $"Not monotonic at {x:G6}: fell from {previous:G10} to {current:G10}. {because}");

            previous = current;
        }
    }

    /// <summary>The result stays inside physical bounds across the whole input range.</summary>
    public static void Bounded(
        Func<double, double> f,
        double from,
        double to,
        double lower,
        double upper,
        string because,
        int steps = DefaultSteps)
    {
        foreach (double x in Sweep(from, to, steps))
        {
            double y = f(x);

            Assert.True(
                y >= lower - 1e-12 && y <= upper + 1e-12,
                $"Out of bounds at {x:G6}: {y:G10} is outside [{lower:G6}, {upper:G6}]. {because}");
        }
    }

    /// <summary>Never returns NaN or infinity anywhere in the range — a routine that divides.</summary>
    public static void Finite(
        Func<double, double> f, double from, double to, string because, int steps = DefaultSteps)
    {
        foreach (double x in Sweep(from, to, steps))
        {
            double y = f(x);

            Assert.True(
                double.IsFinite(y),
                $"Not finite at {x:G6}: got {y}. {because}");
        }
    }

    /// <summary>Zero in, zero out. The degenerate case that catches an added constant.</summary>
    public static void ZeroAtZero(Func<double, double> f, string because) =>
        Assert.True(
            Math.Abs(f(0)) < 1e-12,
            $"Expected zero at zero, got {f(0):G10}. {because}");

    private static IEnumerable<double> Sweep(double from, double to, int steps)
    {
        for (int i = 0; i <= steps; i++)
        {
            yield return from + ((to - from) * i / steps);
        }
    }
}
