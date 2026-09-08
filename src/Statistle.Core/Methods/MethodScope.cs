using Statistle.Core.Persistence;

namespace Statistle.Core.Methods;

/// <summary>
/// Which properties of an object are worth showing, given the method currently selected on it.
///
/// <para>The rule has three parts, and none of them names a variable:</para>
/// <list type="number">
///   <item>A property no method claims is <b>always shown</b> — a name, a total area, a slope.
///     These belong to the object, not to a method.</item>
///   <item>A property claimed by a method that <b>applies</b> is shown.</item>
///   <item>A property claimed only by methods that <b>do not apply</b> is hidden. Choosing a data
///     table column locks the sample box; typing the values in leaves it editable.</item>
/// </list>
///
/// <para>BMP Trains answered the same question with a <c>static string[] InputVariables</c> per
/// type plus three more side tables in the catalog, matched by string; here the method declares
/// it with <c>nameof</c> and the form only asks.</para>
/// </summary>
public static class MethodScope
{
    /// <summary>The properties to show for this object, in declaration order.</summary>
    public static IReadOnlyList<StatistleProperty> VisibleProperties(StatistleObject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        IReadOnlyList<ICalculationMethod> methods = MethodCatalog.For(subject.GetType());

        if (methods.Count == 0)
        {
            return subject.Properties();
        }

        HashSet<string> claimed = [];
        HashSet<string> active = [];

        foreach (ICalculationMethod method in methods)
        {
            bool applies = method.IsApplicableTo(subject);

            foreach (string name in method.Inputs.Concat(method.Outputs))
            {
                claimed.Add(name);

                if (applies)
                {
                    active.Add(name);
                }
            }
        }

        return
        [
            .. subject.Properties()
                .Where(p => !claimed.Contains(p.XmlName) || active.Contains(p.XmlName)),
        ];
    }

    /// <summary>
    /// True when a method that currently applies writes this property — an analysis's sample
    /// while a data table column is supplying it. The editor shows such a value but does not let
    /// it be typed over, since the next recompute would replace it. Decided by the methods'
    /// declared outputs; the editor names no variable.
    /// </summary>
    public static bool IsLocked(StatistleObject subject, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return ActiveMethods(subject).Any(m => m.Outputs.Contains(propertyName, StringComparer.Ordinal));
    }

    /// <summary>The methods that currently apply to this object.</summary>
    public static IReadOnlyList<ICalculationMethod> ActiveMethods(StatistleObject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return [.. MethodCatalog.For(subject.GetType()).Where(m => m.IsApplicableTo(subject))];
    }

    /// <summary>
    /// Properties hidden because the method that claims them is not selected. Useful for saying
    /// so in the interface rather than leaving a reader wondering where a field went.
    /// </summary>
    public static IReadOnlyList<StatistleProperty> HiddenProperties(StatistleObject subject)
    {
        IReadOnlyList<StatistleProperty> visible = VisibleProperties(subject);

        return [.. subject.Properties().Except(visible)];
    }
}
