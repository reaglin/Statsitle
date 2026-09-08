using Statistle.Core.Units;

namespace Statistle.Core.Persistence;

/// <summary>
/// What part a variable plays in a calculation. Carried over from BMP Trains' <c>VariableRole</c>,
/// which draws the distinction that matters to an engineer: a value you are <em>given</em> is not
/// the same as a value you <em>choose</em> as part of the design.
///
/// The generic editor uses it without knowing what any particular variable means — a result is
/// displayed, not typed into — and phase 3's design plots use it to decide what can be swept.
/// </summary>
public enum VariableRole
{
    /// <summary>Unclassified. The default, so an untagged property behaves as before.</summary>
    Unspecified = 0,

    /// <summary>Supplied and not theirs to change — a measurement, a sample, a tabulated constant.</summary>
    Input = 1,

    /// <summary>A design choice the engineer sets and varies. These are the sweepable variables.</summary>
    Design = 2,

    /// <summary>An intermediate computed value: neither an input nor a headline result.</summary>
    Calculated = 3,

    /// <summary>A headline result — a sample mean, a fitted slope, a p-value.</summary>
    Output = 4,
}

/// <summary>
/// Everything the rest of the program needs to know about a domain property, declared once.
///
/// Statistle 10 kept the same information in three places that could drift apart: a
/// <c>PropertyLabels()</c> dictionary, a <c>PropertyDecimalPlaces()</c> dictionary, and unit
/// conversions hand-written into each form's <c>getValues</c>/<c>setValues</c> pair. This
/// attribute collapses all three onto the property itself.
///
/// It is optional — an unattributed property still persists, labelled by its own name.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class StatistlePropertyAttribute : Attribute
{
    /// <summary>Human-readable label. Defaults to the property name.</summary>
    public string? Label { get; init; }

    /// <summary>Decimal places for <em>display</em>. Storage is always full precision.</summary>
    public int Decimals { get; init; } = 3;

    /// <summary>What the number measures, and so how it converts for display.</summary>
    public ValueKind Unit { get; init; } = ValueKind.None;

    /// <summary>
    /// A previous XML name for this property. Files written before a rename still load — in
    /// Statistle 10 renaming a property silently orphaned the data in every saved file.
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// A longer label for reports, where there is room for it. Defaults to <see cref="Label"/>.
    /// </summary>
    public string? ReportLabel { get; init; }

    /// <summary>What part this variable plays. See <see cref="VariableRole"/>.</summary>
    public VariableRole Role { get; init; } = VariableRole.Unspecified;

    /// <summary>
    /// Inclusive bounds, in <em>base</em> units — the units the value is stored in, so a range
    /// means the same thing whichever units the project is displayed in. <see cref="double.NaN"/>
    /// is "no bound", which is the default, so an untagged property stays unconstrained.
    ///
    /// The range belongs to the variable, not to a form: one implementation in the editor
    /// enforces every range that is declared here.
    /// </summary>
    public double Min { get; init; } = double.NaN;

    /// <inheritdoc cref="Min"/>
    public double Max { get; init; } = double.NaN;

    /// <summary>
    /// For a <see cref="DataSeries"/>: the sibling property whose value is the spacing of the
    /// x column. The series is then entered as one value per line, with x running
    /// 0, step, 2·step…, so a series is typed as one value per line against a single spacing
    /// rather than as (x, y) pairs. Null means both columns are entered.
    /// </summary>
    public string? StepProperty { get; init; }

    /// <summary>
    /// For a <see cref="DataSeries"/> with no step: one value per line, x being the row index —
    /// a sample. Implied by <see cref="StepProperty"/>. Leave false for paired data such as a
    /// regression's x and y.
    /// </summary>
    public bool ValuesOnly { get; init; }

    /// <summary>
    /// What the value means and how to choose it, for the help panel under the form. Written
    /// for the engineer, not the programmer: what it is, typical values, where it comes from.
    /// Falls back to a description built from the label, unit, range and role.
    /// </summary>
    public string? Help { get; init; }

    /// <summary>
    /// Not offered in the editor — still persisted and reported. For a value the form sets some
    /// other way, such as the time-of-concentration formula chosen by clicking its result.
    /// </summary>
    public bool Hidden { get; init; }
}

/// <summary>
/// Overall help for an object type — what a data table is for, what an analysis does — shown
/// in the help panel when no field is selected. Declared on the class, next to what it describes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StatistleHelpAttribute(string text) : Attribute
{
    public string Text { get; } = text;
}

/// <summary>Excludes a public property from persistence — collections owned by the document, mostly.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class StatistleIgnoreAttribute : Attribute;
