using Statistle.Core.Persistence;

namespace Statistle.Core.Analyses;

/// <summary>How an analysis gets its inputs.</summary>
public enum ToolKind
{
    /// <summary>Reads named project artifacts — a column of one of the project's data tables.</summary>
    ProjectBound = 0,

    /// <summary>Computes from its own inputs. Needs no project, but can push a result into one.</summary>
    FreeStanding = 1,
}

/// <summary>
/// A value an analysis can hand to a property of some other object: "the descriptive-statistics
/// analysis's <c>Mean</c> can fill another analysis's assumed population mean".
///
/// Declared on the analysis, so the editor can put a "Calculate…" link beside any property some
/// analysis provides without the property, or the form, naming the analysis.
/// </summary>
/// <param name="TargetType">The type of object that receives the value.</param>
/// <param name="TargetProperty">The property on it that is filled, by <c>nameof</c>.</param>
/// <param name="ResultProperty">The property on the analysis that holds the value to hand over.</param>
public sealed record ProvidedValue(Type TargetType, string TargetProperty, string ResultProperty)
{
    /// <summary>
    /// Inputs the analysis takes from the target when opened from it — (analysis property,
    /// target property) pairs. An analysis opened from another object starts with what that
    /// object already knows rather than whatever was last typed.
    /// </summary>
    public IReadOnlyList<(string ToolProperty, string TargetProperty)> Seeds { get; init; } = [];

    public bool Fills(Type targetType, string propertyName) =>
        TargetType.IsAssignableFrom(targetType)
        && string.Equals(TargetProperty, propertyName, StringComparison.Ordinal);
}

/// <summary>
/// An analysis is a class. Deriving from this and giving it properties is the entire cost of
/// adding one: it appears in the rail, its parameters persist, and it gets the generic editor
/// with the labels, ranges, roles and help those properties declare.
///
/// The second rule of the design is what this exists to keep: if adding an analysis ever means
/// editing a dispatcher, a menu and a labels dictionary as well, the list stops growing at about
/// four.
/// </summary>
public abstract class Analysis : StatistleObject
{
    /// <summary>Name shown in the rail.</summary>
    public abstract string Title { get; }

    /// <summary>One line describing what it does.</summary>
    public abstract string Summary { get; }

    public abstract ToolKind Kind { get; }

    /// <summary>Sort order within its group. Equal orders fall back to title.</summary>
    public virtual int Order => 100;

    /// <summary>
    /// Properties of other objects this analysis can fill in. Empty by default; an analysis
    /// that produces, say, a sample standard deviation lists the property it can supply, and
    /// the editor offers the hand-over beside that property.
    /// </summary>
    public virtual IReadOnlyList<ProvidedValue> Provides => [];

    /// <summary>
    /// True to lay the form out in two columns — entry values on the left, results on the
    /// right — rather than one list. Right for an analysis whose results are compared side by
    /// side; a data form reads better as one column.
    /// </summary>
    public virtual bool ResultsBeside => false;

    /// <summary>
    /// Recomputes this analysis's outputs from its inputs. The default does nothing, so an
    /// analysis that only collects parameters need not override it.
    /// </summary>
    public virtual void Recalculate()
    {
    }

    /// <summary>
    /// Clears the worked example an analysis opens with, for an instance that belongs to one
    /// object (the analysis opened from another object's Calculate link): every numeric entry
    /// becomes blank, every series empty, every table without columns. Read straight off the
    /// metadata, so a new analysis gets it without writing anything. Results recompute to blank.
    /// </summary>
    public virtual void StartBlank()
    {
        foreach (StatistleProperty property in Properties())
        {
            if (property.IsResult || property.Hidden || property.XmlName is nameof(Id) or nameof(Name) or nameof(Description))
            {
                continue;
            }

            object? value = property.Read(this);

            switch (value)
            {
                case DataSeries series:
                    series.Clear();
                    break;
                case ColumnTable table:
                    table.Clear();
                    break;
                case double when property.Property.CanWrite:
                    property.Property.SetValue(this, double.NaN);
                    break;
                default:
                    break;
            }
        }

        Recalculate();
    }
}
