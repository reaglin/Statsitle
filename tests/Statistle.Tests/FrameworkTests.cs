using Statistle.Core.Document;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;
using Statistle.Core.Analyses;

namespace Statistle.Tests;

/// <summary>
/// An analysis defined here, in the <em>test</em> assembly, with no registration anywhere. If
/// discovery finds it and its parameters round-trip, then adding an analysis really is one class
/// file.
/// </summary>
public sealed class ProbeTool : Analysis
{
    private double _someParameter = 1.5;

    public override string Title => "Probe";

    public override string Summary => "Exists only to prove discovery needs no registration";

    public override ToolKind Kind => ToolKind.FreeStanding;

    [StatistleProperty(Label = "Some parameter", Decimals = 2, Min = 0, Max = 10)]
    public double SomeParameter
    {
        get => _someParameter;
        set => SetProperty(ref _someParameter, value);
    }
}

public class ToolDiscoveryTests
{
    [Fact]
    public void A_tool_class_is_found_with_no_registration()
    {
        IReadOnlyList<AnalysisDescriptor> found = AnalysisCatalog.Discover(typeof(ProbeTool).Assembly);

        AnalysisDescriptor probe = Assert.Single(found, t => t.ToolType == typeof(ProbeTool));

        Assert.Equal("Probe", probe.Title);
        Assert.Equal(ToolKind.FreeStanding, probe.Kind);
    }

    [Fact]
    public void The_shipped_tools_are_discovered()
    {
        IReadOnlyList<AnalysisDescriptor> found = AnalysisCatalog.Discover(typeof(Analysis).Assembly);

        Assert.Contains(found, t => t.ToolType == typeof(DescriptiveStatistics));
        Assert.Contains(found, t => t.ToolType == typeof(SimpleRegression));
    }

    [Fact]
    public void Abstract_tools_are_not_offered()
    {
        IReadOnlyList<AnalysisDescriptor> found = AnalysisCatalog.Discover(typeof(Analysis).Assembly);

        Assert.All(found, t => Assert.False(t.ToolType.IsAbstract));
    }

    [Fact]
    public void Tools_are_grouped_by_kind_then_ordered()
    {
        IReadOnlyList<AnalysisDescriptor> found = AnalysisCatalog.Discover(typeof(Analysis).Assembly);

        Assert.Equal([.. found.OrderBy(t => t.Kind).ThenBy(t => t.Order)], found);
    }

    [Fact]
    public void A_tool_gets_the_generic_editor_from_its_own_properties()
    {
        // No UI code knows what a Probe is; the row it shows comes from the property.
        StatistleProperty parameter = new ProbeTool()
            .Properties()
            .Single(p => p.XmlName == nameof(ProbeTool.SomeParameter));

        Assert.Equal("Some parameter", parameter.Label);
        Assert.True(parameter.HasRange);
        Assert.Equal("between 0 and 10", parameter.RangeText());
    }

    [Fact]
    public void A_tools_parameters_round_trip_through_a_project()
    {
        string file = Path.Combine(
            Path.GetTempPath(), "statistle-tests", Guid.NewGuid().ToString("N"), "tools.statistle");

        var document = new StatistleDocument();
        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();
        analysis.Sample.SetValues([2, 4, 4, 4, 5, 5, 7, 9], 1);
        analysis.Recalculate();

        document.Save(file);

        try
        {
            StatistleDocument reopened = StatistleDocument.Open(file);
            DescriptiveStatistics restored = reopened.Tool<DescriptiveStatistics>();

            Assert.Equal(8, restored.Sample.Count);
            Assert.Equal([2, 4, 4, 4, 5, 5, 7, 9], restored.Sample.YValues);

            // Outputs are recomputed on open rather than trusted from the file.
            Assert.Equal(analysis.Mean, restored.Mean, 10);
            Assert.Equal(analysis.StandardDeviation, restored.StandardDeviation, 10);
            Assert.False(reopened.Diagnostics.HasWarnings, reopened.Diagnostics.ToString());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
        }
    }
}

public class MethodContractTests
{
    [Fact]
    public void Every_method_in_the_build_declares_a_valid_contract()
    {
        // Sweeps the whole build, so a method cannot be added and quietly go unchecked.
        IReadOnlyList<ICalculationMethod> methods = MethodCatalog.Discover(typeof(Analysis).Assembly);

        Assert.NotEmpty(methods);

        List<string> problems = [.. methods.SelectMany(MethodContract.Verify)];

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Every_method_cites_its_source()
    {
        // A revised engine has no external oracle, so where a formula came from is part of it.
        foreach (ICalculationMethod method in MethodCatalog.Discover(typeof(Analysis).Assembly))
        {
            Assert.False(
                string.IsNullOrWhiteSpace(method.Citation),
                $"{method.Name} has no citation.");
        }
    }

    [Fact]
    public void A_method_naming_a_property_that_does_not_exist_is_caught()
    {
        IReadOnlyList<string> problems = MethodContract.Verify(new MisdeclaredMethod());

        Assert.Contains(problems, p => p.Contains("NotAProperty", StringComparison.Ordinal));
    }

    [Fact]
    public void A_method_writing_to_an_editable_property_is_caught()
    {
        // An output the user could type over would be silently overwritten on the next
        // recalculation. The contract refuses it.
        IReadOnlyList<string> problems = MethodContract.Verify(new WritesToAnInputMethod());

        Assert.Contains(problems, p => p.Contains("its role is Design", StringComparison.Ordinal));
    }

    private sealed class MisdeclaredMethod : CalculationMethod<SimpleRegression>
    {
        public override string Name => "Misdeclared";

        public override string Description => "Names an input that does not exist";

        public override string Citation => "n/a";

        public override IReadOnlyList<string> Inputs => ["NotAProperty"];

        public override IReadOnlyList<string> Outputs => [];

        public override void Apply(SimpleRegression subject)
        {
        }
    }

    private sealed class WritesToAnInputMethod : CalculationMethod<SimpleRegression>
    {
        public override string Name => "Writes to an input";

        public override string Description => "Declares a Design property as an output";

        public override string Citation => "n/a";

        public override IReadOnlyList<string> Inputs => [];

        public override IReadOnlyList<string> Outputs => [nameof(SimpleRegression.SelectedForm)];

        public override void Apply(SimpleRegression subject)
        {
        }
    }
}
