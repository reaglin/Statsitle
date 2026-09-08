using Statistle.Core.Persistence;

namespace Statistle.Core.Methods;

/// <summary>
/// A calculation method is a class: what it is called, where it came from, what it consumes and
/// what it produces.
///
/// <para><b>Declaring inputs.</b> <see cref="Inputs"/> and <see cref="Outputs"/> are property
/// names on <see cref="Subject"/>, and are written with <c>nameof</c> — so renaming a property
/// moves the declaration with it and deleting one breaks the build. BMP Trains answered the same
/// question with a <c>static string[] InputVariables</c> matched by name at runtime, plus three
/// further side tables; those drift silently as types are added. Here the compiler holds them
/// together, and <see cref="MethodContract.Verify"/> is the backstop.</para>
///
/// <para>This is what lets a form show only the variables a chosen method actually uses,
/// without the form knowing anything about the method.</para>
/// </summary>
public interface ICalculationMethod
{
    string Name { get; }

    string Description { get; }

    /// <summary>Where the formula comes from — publication and date. Required, not decorative.</summary>
    string Citation { get; }

    /// <summary>The type this method reads from and writes to.</summary>
    Type Subject { get; }

    /// <summary>Property names on <see cref="Subject"/> this method consumes.</summary>
    IReadOnlyList<string> Inputs { get; }

    /// <summary>Property names on <see cref="Subject"/> this method writes.</summary>
    IReadOnlyList<string> Outputs { get; }

    /// <summary>
    /// The subset of <see cref="Outputs"/> that is otherwise entered by hand — an analysis's
    /// sample, copied from a data table column while this method applies and typed while it does not.
    /// Declared explicitly so <see cref="MethodContract"/> can tell a deliberate hand-over
    /// (the editor locks the value while this method applies) from a method quietly
    /// overwriting an input. Empty by default.
    /// </summary>
    IReadOnlyList<string> Generates => [];

    /// <summary>Recomputes the outputs on the given subject.</summary>
    void Apply(StatistleObject subject);

    /// <summary>
    /// Whether this method applies to the subject in its current state — a loss method applies
    /// only when its own loss method is the one selected.
    ///
    /// This is what lets a form show the variables pertinent to the chosen method and nothing
    /// else: the method decides, the form only asks. Defaults to always.
    /// </summary>
    bool IsApplicableTo(StatistleObject subject) => true;
}

/// <summary>Typed base: <c>Apply</c> receives the subject already cast.</summary>
public abstract class CalculationMethod<TSubject> : ICalculationMethod
    where TSubject : StatistleObject
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string Citation { get; }

    public Type Subject => typeof(TSubject);

    public abstract IReadOnlyList<string> Inputs { get; }

    public abstract IReadOnlyList<string> Outputs { get; }

    /// <inheritdoc cref="ICalculationMethod.Generates"/>
    public virtual IReadOnlyList<string> Generates => [];

    public abstract void Apply(TSubject subject);

    /// <summary>Whether this method applies to the subject as it currently stands.</summary>
    public virtual bool IsApplicableTo(TSubject subject) => true;

    void ICalculationMethod.Apply(StatistleObject subject) => Apply((TSubject)subject);

    bool ICalculationMethod.IsApplicableTo(StatistleObject subject) =>
        subject is TSubject typed && IsApplicableTo(typed);
}

/// <summary>
/// Checks that a method's declared inputs and outputs actually name properties on its subject,
/// and that its outputs are marked as results. Compile-time <c>nameof</c> catches a rename; this
/// catches the rest — a name that never existed, or an output the editor would let a user type
/// over.
/// </summary>
public static class MethodContract
{
    public static IReadOnlyList<string> Verify(ICalculationMethod method)
    {
        ArgumentNullException.ThrowIfNull(method);

        List<string> problems = [];

        IReadOnlyList<StatistleProperty> properties = StatistleObject.PropertiesOf(method.Subject);

        foreach (string name in method.Inputs.Concat(method.Outputs))
        {
            if (properties.All(p => p.XmlName != name))
            {
                problems.Add(
                    $"{method.Name}: '{name}' is not a persisted property of {method.Subject.Name}.");
            }
        }

        // An output must be a result the editor will not offer for entry — unless the method
        // declares it Generates the value and another method on the same subject takes it as
        // an input: then it is a value one method supplies and another accepts by hand (a
        // analysis's sample, copied from a data table or typed), and the editor locks it while
        // the writing method applies (MethodScope.IsLocked). Both halves are required, so a
        // method cannot quietly write over an input.
        IReadOnlyList<ICalculationMethod> siblings = MethodCatalog.For(method.Subject);

        foreach (string name in method.Outputs)
        {
            StatistleProperty? property = properties.FirstOrDefault(p => p.XmlName == name);

            bool handedOver = method.Generates.Contains(name, StringComparer.Ordinal)
                              && siblings.Any(m => m.GetType() != method.GetType()
                                                   && m.Inputs.Contains(name, StringComparer.Ordinal));

            if (property is not null && !property.IsResult && !handedOver)
            {
                problems.Add(
                    $"{method.Name}: '{name}' is written as an output but its role is " +
                    $"{property.Role}; it should be Calculated or Output so the editor does not " +
                    "offer it for entry.");
            }
        }

        foreach (string name in method.Inputs.Intersect(method.Outputs))
        {
            problems.Add($"{method.Name}: '{name}' is declared as both an input and an output.");
        }

        foreach (string name in method.Generates.Except(method.Outputs))
        {
            problems.Add($"{method.Name}: '{name}' is declared as generated but not as an output.");
        }

        return problems;
    }
}
