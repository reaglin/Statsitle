namespace Statistle.Core.Persistence;

/// <summary>
/// One option in a pick-list: the value stored, the label shown, and an optional longer note
/// (a curve's source and duration) that the help panel shows for the selected option.
/// </summary>
public sealed record ChoiceOption(object Value, string Label, string Detail = "")
{
    public override string ToString() => Label;
}

/// <summary>
/// A property whose value is chosen from a list the object supplies — a column of one of the
/// project's data tables. The editor still knows nothing about it: it asks, and shows a
/// drop-down.
/// </summary>
public interface IHasChoices
{
    /// <summary>Options for a property. Empty if it is not a choice.</summary>
    IReadOnlyList<ChoiceOption> ChoicesFor(string propertyName);
}
