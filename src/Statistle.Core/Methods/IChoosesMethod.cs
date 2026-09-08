namespace Statistle.Core.Methods;

/// <summary>
/// An object that computes one quantity by several methods and lets the engineer pick which
/// result counts — an analysis that estimates one quantity several ways.
///
/// <para>The editor uses this generically: every result that some method writes is offered for
/// the choice, the chosen one is highlighted and the rest greyed, and each input is marked with
/// whether the chosen method uses it. All of that comes from the methods' declared
/// <see cref="ICalculationMethod.Inputs"/> and <see cref="ICalculationMethod.Outputs"/>; no UI
/// file names a formula.</para>
///
/// <para>These are methods rather than properties so that reflection over the object's
/// properties — which is how it is persisted and edited — does not pick them up.</para>
/// </summary>
public interface IChoosesMethod
{
    /// <summary>The method whose result is currently the one to use, or null if none is.</summary>
    ICalculationMethod? ChosenMethod();

    /// <summary>Makes <paramref name="method"/>'s result the one to use, and recomputes.</summary>
    void Choose(ICalculationMethod method);
}
