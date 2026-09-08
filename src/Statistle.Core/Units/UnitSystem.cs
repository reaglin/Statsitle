namespace Statistle.Core.Units;

/// <summary>
/// What a number is, as far as the editor and the report are concerned. Declared once on the
/// property (see <see cref="Persistence.StatistlePropertyAttribute"/>) rather than re-coded in
/// every form.
///
/// Statistics has no unit system to convert between — a mean is in whatever units the data were
/// measured in, and Statistle never sees those. What is worth declaring is the <em>kind</em> of
/// number: a percentage, a probability, a count, or a plain number in the data's own units.
/// </summary>
public enum ValueKind
{
    /// <summary>A plain number, in whatever units the data are in. The default.</summary>
    None = 0,

    /// <summary>A percentage, 0 to 100. Shown with a % after the box.</summary>
    Percent,

    /// <summary>A probability, 0 to 1.</summary>
    Probability,

    /// <summary>A whole number of things — observations, classes, degrees of freedom.</summary>
    Count,
}

/// <summary>
/// One choice of unit. <paramref name="FactorFromBase"/> converts a stored base value to this
/// unit: <c>display = base * factor</c>.
/// </summary>
public sealed record UnitOption(string Label, double FactorFromBase);

/// <summary>
/// What each <see cref="ValueKind"/> is labelled with. There is exactly one option per kind and
/// its factor is 1, so nothing ever converts: the plumbing stays (a later kind that does convert
/// needs no new machinery) but a stored number and a shown number are the same number.
/// </summary>
public static class UnitSystem
{
    private static readonly Dictionary<ValueKind, IReadOnlyList<UnitOption>> Options = new()
    {
        [ValueKind.None] = [new UnitOption(string.Empty, 1.0)],
        [ValueKind.Percent] = [new UnitOption("%", 1.0)],
        [ValueKind.Probability] = [new UnitOption(string.Empty, 1.0)],
        [ValueKind.Count] = [new UnitOption(string.Empty, 1.0)],
    };

    /// <summary>The unit a value of this kind is stored in.</summary>
    public static UnitOption BaseUnit(ValueKind kind) => OptionsFor(kind)[0];

    public static IReadOnlyList<UnitOption> OptionsFor(ValueKind kind) =>
        Options.TryGetValue(kind, out IReadOnlyList<UnitOption>? options)
            ? options
            : Options[ValueKind.None];
}
