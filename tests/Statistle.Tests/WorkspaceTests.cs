using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>
/// A form shows the variables the method currently in force uses, and nothing else — decided by
/// the methods, never by a field list in the UI.
/// </summary>
public class MethodScopeTests
{
    private static string[] Visible(StatistleObject subject) =>
        [.. MethodScope.VisibleProperties(subject).Select(p => p.XmlName)];

    [Fact]
    public void Properties_no_method_claims_are_always_shown()
    {
        // A name, a description, the plotted form: these belong to the object, not to a method,
        // so no choice of source can hide them.
        var withColumn = new DescriptiveStatistics { Column = DataSources.Reference(1, "x") };
        var typedIn = new DescriptiveStatistics();

        foreach (DescriptiveStatistics analysis in new[] { withColumn, typedIn })
        {
            string[] visible = Visible(analysis);

            Assert.Contains(nameof(StatistleObject.Name), visible);
            Assert.Contains(nameof(DescriptiveStatistics.Column), visible);
            Assert.Contains(nameof(DescriptiveStatistics.Mean), visible);
        }
    }

    [Fact]
    public void A_value_a_method_in_force_writes_is_shown_but_locked()
    {
        // With a column chosen, the sample is copied from it: still shown, not typed over.
        var fromTable = new DescriptiveStatistics { Column = DataSources.Reference(1, "x") };

        Assert.Contains(nameof(DescriptiveStatistics.Sample), Visible(fromTable));
        Assert.True(MethodScope.IsLocked(fromTable, nameof(DescriptiveStatistics.Sample)));

        var typedIn = new DescriptiveStatistics();
        Assert.False(MethodScope.IsLocked(typedIn, nameof(DescriptiveStatistics.Sample)));
    }

    [Fact]
    public void Every_result_of_an_analysis_is_locked_against_entry()
    {
        var analysis = new DescriptiveStatistics();

        foreach (StatistleProperty property in analysis.Properties().Where(p => p.IsResult))
        {
            Assert.True(
                property.IsResult,
                $"{property.XmlName} is written by a method, so the editor must not offer it.");
        }
    }

    [Fact]
    public void Switching_the_source_changes_what_is_locked()
    {
        var analysis = new DescriptiveStatistics();
        bool before = MethodScope.IsLocked(analysis, nameof(DescriptiveStatistics.Sample));

        analysis.Column = DataSources.Reference(1, "x");
        bool after = MethodScope.IsLocked(analysis, nameof(DescriptiveStatistics.Sample));

        Assert.NotEqual(before, after);
    }

    [Fact]
    public void An_object_with_no_methods_shows_everything()
    {
        // The output probe in this assembly has properties and no methods at all.
        var probe = new OutputProbeTool();

        Assert.Empty(MethodCatalog.For(probe.GetType()));
        Assert.Equal(probe.Properties().Count, MethodScope.VisibleProperties(probe).Count);
    }

    [Fact]
    public void A_regression_on_typed_pairs_has_one_active_method_and_hides_nothing()
    {
        // The least-squares fit always applies; the column source applies only once a column is
        // chosen, and then it locks the pairs. Scoping narrows a form only where a method says so.
        var regression = new SimpleRegression();

        Assert.Empty(MethodScope.HiddenProperties(regression));
        Assert.Single(MethodScope.ActiveMethods(regression));
        Assert.False(MethodScope.IsLocked(regression, nameof(SimpleRegression.Data)));
    }

    [Fact]
    public void A_data_table_has_no_methods_and_so_shows_its_whole_form()
    {
        var table = new DataTable();

        Assert.Empty(MethodCatalog.For(typeof(DataTable)));
        Assert.Contains(nameof(DataTable.Columns), Visible(table));
    }
}

/// <summary>The whole chain, ending where it started.</summary>
public class EndToEndTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "statistle-e2e", Guid.NewGuid().ToString("N"));

    public EndToEndTests() => Directory.CreateDirectory(_folder);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Table_analyses_save_reopen_identical()
    {
        var document = new StatistleDocument { Name = "Acceptance" };

        DataTable panel = document.AddTable("Solar panel");
        panel.Columns.AddColumn("Temperature", [15, 20, 25, 30, 35, 40, 45, 50, 55, 60]);
        panel.Columns.AddColumn("Efficiency", [19.1, 18.7, 18.2, 17.6, 17.1, 16.3, 15.8, 15.1, 14.5, 13.9]);

        DescriptiveStatistics statistics = document.Tool<DescriptiveStatistics>();
        statistics.Column = DataSources.Reference(panel.Id, "Efficiency");
        statistics.Recalculate();

        SimpleRegression regression = document.Tool<SimpleRegression>();
        regression.XColumn = DataSources.Reference(panel.Id, "Temperature");
        regression.YColumn = DataSources.Reference(panel.Id, "Efficiency");
        regression.Recalculate();

        Assert.Null(statistics.Problem);
        Assert.Null(regression.Problem);
        Assert.Equal(10, statistics.Count);
        Assert.Equal(10, regression.Count);

        // Efficiency falls with temperature, so the least-squares slope is negative and the fit
        // is close: this is the relation the sample project exists to show.
        Assert.True(regression.Slope < 0, "efficiency should fall as the cell warms");
        Assert.True(regression.RSquared > 0.99);

        string file = Path.Combine(_folder, "acceptance.statistle");
        document.Save(file);

        StatistleDocument reopened = StatistleDocument.Open(file);

        Assert.False(reopened.Diagnostics.HasWarnings, reopened.Diagnostics.ToString());
        Assert.Equal("Solar panel", reopened.Tables.Single().Name);
        Assert.Equal(10, reopened.Tables.Single().Columns.Column("Efficiency")!.Count);

        // Both analyses recompute from the reopened project and land in the same place.
        DescriptiveStatistics restoredStatistics = reopened.Tool<DescriptiveStatistics>();
        SimpleRegression restoredRegression = reopened.Tool<SimpleRegression>();

        Assert.Null(restoredStatistics.Problem);
        Assert.Equal(statistics.Mean, restoredStatistics.Mean, 9);
        Assert.Equal(statistics.StandardDeviation, restoredStatistics.StandardDeviation, 9);
        Assert.Equal(statistics.Median, restoredStatistics.Median, 9);
        Assert.Equal(regression.Equation, restoredRegression.Equation);
        Assert.Equal(regression.RSquared, restoredRegression.RSquared, 9);
    }

    [Fact]
    public void An_analysis_says_what_is_missing_rather_than_failing()
    {
        var document = new StatistleDocument();
        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();

        analysis.Column = DataSources.Reference(99, "gone");
        analysis.Recalculate();

        Assert.NotNull(analysis.Problem);
        Assert.Contains("has been deleted", analysis.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void An_analysis_offers_the_projects_own_columns()
    {
        var document = new StatistleDocument();
        DataTable first = document.AddTable("First");
        first.Columns.AddColumn("a", [1]);
        DataTable second = document.AddTable("Second");
        second.Columns.AddColumn("b", [2]);
        second.Columns.AddColumn("c", [3]);

        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();

        Assert.Equal(
            [DataSources.EnteredHere, "First › a", "Second › b", "Second › c"],
            analysis.ChoicesFor(nameof(DescriptiveStatistics.Column)).Select(c => c.Label));
    }

    [Fact]
    public void Renaming_an_artifact_notifies_so_the_tree_can_follow()
    {
        var table = new DataTable();
        List<string?> changed = [];
        table.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        table.Name = "Renamed";

        Assert.Contains(nameof(StatistleObject.Name), changed);
    }
}
