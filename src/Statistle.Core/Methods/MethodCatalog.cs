using System.Collections.Concurrent;
using System.Reflection;

namespace Statistle.Core.Methods;

/// <summary>
/// Finds calculation methods the same way tools are found — by reflection, with no registry.
/// The verification harness uses this to sweep every method in the build, so a routine cannot
/// be added and quietly go untested.
/// </summary>
public static class MethodCatalog
{
    public static IReadOnlyList<ICalculationMethod> Discover(params Assembly[] assemblies)
    {
        Assembly[] toScan = assemblies.Length > 0 ? assemblies : [typeof(MethodCatalog).Assembly];

        List<ICalculationMethod> found = [];

        foreach (Assembly assembly in toScan.Distinct())
        {
            foreach (Type type in ConcreteMethodsIn(assembly))
            {
                found.Add((ICalculationMethod)Activator.CreateInstance(type)!);
            }
        }

        return [.. found.OrderBy(m => m.Name, StringComparer.Ordinal)];
    }

    private static readonly ConcurrentDictionary<Type, IReadOnlyList<ICalculationMethod>> BySubject = new();

    /// <summary>
    /// The methods that operate on a given domain type. Methods are stateless, so the default
    /// (Statistle.Core) answer is discovered once and shared — the editor asks for every row.
    /// </summary>
    public static IReadOnlyList<ICalculationMethod> For(Type subject, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return assemblies.Length == 0
            ? BySubject.GetOrAdd(subject, s => [.. Discover().Where(m => m.Subject == s)])
            : [.. Discover(assemblies).Where(m => m.Subject == subject)];
    }

    private static IEnumerable<Type> ConcreteMethodsIn(Assembly assembly)
    {
        Type[] types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = [.. ex.Types.OfType<Type>()];
        }

        return types.Where(t =>
            typeof(ICalculationMethod).IsAssignableFrom(t)
            && !t.IsAbstract
            && !t.IsInterface
            && t.GetConstructor(Type.EmptyTypes) is not null);
    }
}
