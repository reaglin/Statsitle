using System.Reflection;
using Statistle.Core.Plotting;

namespace Statistle.Tests;

/// <summary>
/// Rule 1 of the rebuild plan: the calculation engine never references a UI assembly, so every
/// result is testable without a window. Enforced here rather than left to discipline.
/// </summary>
public class ArchitectureTests
{
    private static readonly string[] ForbiddenAssemblies =
    [
        "PresentationFramework",
        "PresentationCore",
        "WindowsBase",
        "System.Windows.Forms",
        "System.Drawing.Common",
        "Statistle.App",
        "ScottPlot",
        "ScottPlot.WPF",
    ];

    private static Assembly CoreAssembly => typeof(IPlotSurface).Assembly;

    [Fact]
    public void Core_does_not_reference_any_UI_assembly()
    {
        string[] referenced = [.. CoreAssembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty)];

        string[] violations =
        [
            .. referenced.Where(r => ForbiddenAssemblies.Contains(r, StringComparer.OrdinalIgnoreCase)),
        ];

        Assert.True(
            violations.Length == 0,
            $"Statistle.Core must not reference a UI or charting assembly, but references: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Core_targets_a_platform_neutral_framework()
    {
        var target = CoreAssembly
            .GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();

        Assert.NotNull(target);
        Assert.DoesNotContain("windows", target!.FrameworkName, StringComparison.OrdinalIgnoreCase);
    }
}
