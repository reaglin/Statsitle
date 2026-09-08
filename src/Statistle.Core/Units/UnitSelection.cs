using Statistle.Core.Persistence;

namespace Statistle.Core.Units;

/// <summary>
/// How a project shows its numbers. Statistics has nothing to convert — a mean carries the units
/// of the data, which Statistle never sees — so every <see cref="ValueKind"/> has exactly one
/// option and every conversion here is the identity.
///
/// It stays because the editor, the report writers and the figures all ask a value's kind for
/// its label, and because a kind that does need converting can be added without touching them.
/// </summary>
public sealed class UnitSelection : StatistleObject
{
    public UnitOption Selected(ValueKind kind) => UnitSystem.OptionsFor(kind)[0];

    /// <summary>The suffix shown after a value of this kind: "%", or nothing.</summary>
    public string LabelFor(ValueKind kind) => Selected(kind).Label;

    /// <summary>Stored value to shown value. Nothing converts; the number is the number.</summary>
    public double ToDisplay(ValueKind kind, double baseValue) =>
        baseValue * Selected(kind).FactorFromBase;

    /// <summary>Entered value to stored value.</summary>
    public double ToBase(ValueKind kind, double displayValue) =>
        displayValue / Selected(kind).FactorFromBase;

    /// <summary>There is only ever one option per kind, so this is index 0.</summary>
    public int IndexOf(ValueKind kind) => 0;
}
