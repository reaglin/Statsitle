using System.Reflection;

namespace Statistle.Core.Analyses;

/// <summary>A discovered analysis type, with the metadata the rail needs before one is created.</summary>
public sealed record AnalysisDescriptor(
    Type ToolType,
    string Title,
    string Summary,
    ToolKind Kind,
    int Order,
    IReadOnlyList<ProvidedValue>? Provides = null)
{
    /// <summary>Stable key, used as the analysis's part name inside a project document.</summary>
    public string Key => ToolType.Name;

    /// <summary>What this analysis can hand to other objects. See <see cref="ProvidedValue"/>.</summary>
    public IReadOnlyList<ProvidedValue> Provides { get; init; } = Provides ?? [];

    public Analysis Create() => (Analysis)Activator.CreateInstance(ToolType)!;
}

/// <summary>An analysis that can fill a given property, and which of its own properties it fills it from.</summary>
public sealed record ValueProvider(AnalysisDescriptor Tool, ProvidedValue Value);

/// <summary>
/// Finds analyses by reflection. There is no registration list, no dispatcher and no menu table
/// to keep in step — the acceptance for this is that adding one class file is the only edit
/// needed to get a working analysis.
/// </summary>
public static class AnalysisCatalog
{
    /// <summary>Every analysis in the assemblies given, or in Statistle.Core if none are.</summary>
    public static IReadOnlyList<AnalysisDescriptor> Discover(params Assembly[] assemblies)
    {
        Assembly[] toScan = assemblies.Length > 0 ? assemblies : [typeof(AnalysisCatalog).Assembly];

        List<AnalysisDescriptor> found = [];

        foreach (Assembly assembly in toScan.Distinct())
        {
            foreach (Type type in ConcreteToolsIn(assembly))
            {
                // The descriptor's metadata lives on the analysis, so it takes an instance to read
                // it. Analyses are cheap to construct and must have a parameterless constructor.
                var probe = (Analysis)Activator.CreateInstance(type)!;

                found.Add(new AnalysisDescriptor(
                    type, probe.Title, probe.Summary, probe.Kind, probe.Order, probe.Provides));
            }
        }

        return
        [
            .. found
                .OrderBy(t => t.Kind)
                .ThenBy(t => t.Order)
                .ThenBy(t => t.Title, StringComparer.CurrentCulture),
        ];
    }

    public static IReadOnlyList<AnalysisDescriptor> OfKind(ToolKind kind, params Assembly[] assemblies) =>
        [.. Discover(assemblies).Where(t => t.Kind == kind)];

    /// <summary>
    /// The analysis, if any, that can fill <paramref name="propertyName"/> on an object of
    /// <paramref name="targetType"/>. The editor asks this for every row it builds; the answer
    /// comes from what the analyses declared, so no form names one.
    /// </summary>
    public static ValueProvider? ProviderFor(Type targetType, string propertyName, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        foreach (AnalysisDescriptor tool in Discover(assemblies))
        {
            ProvidedValue? match = tool.Provides.FirstOrDefault(p => p.Fills(targetType, propertyName));

            if (match is not null)
            {
                return new ValueProvider(tool, match);
            }
        }

        return null;
    }

    private static IEnumerable<Type> ConcreteToolsIn(Assembly assembly)
    {
        Type[] types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // A half-loadable assembly still yields the types that did load.
            types = [.. ex.Types.OfType<Type>()];
        }

        return types.Where(t =>
            typeof(Analysis).IsAssignableFrom(t)
            && !t.IsAbstract
            && t.GetConstructor(Type.EmptyTypes) is not null);
    }
}
