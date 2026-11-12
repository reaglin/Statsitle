using System.Globalization;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>
/// The one artifact a project holds, and how an analysis takes a column of it: the text form a
/// person pastes, the ragged columns that form allows, the paste it must refuse, and the lock
/// the editor puts on a series that is being copied from a table.
/// </summary>
public class DataTableTests
{
    private static StatistleDocument ProjectWithData()
    {
        var document = new StatistleDocument();

        DataTable strength = document.AddTable("Tensile strength");
        strength.Columns.AddColumn("AA2024", [470, 485, 478, 492, 460, 473, 488, 484, 482, 475]);
        strength.Columns.AddColumn("AA7075", [505, 518, 523, 508, 515, 522, 510, 529, 520, 514]);

        DataTable panel = document.AddTable("Solar panel");
        panel.Columns.AddColumn("Temperature", [15, 20, 25, 30, 35]);
        panel.Columns.AddColumn("Efficiency", [19.1, 18.7, 18.2, 17.6, 17.1]);

        return document;
    }

    // ── The text form ─────────────────────────────────────────────────────────

    [Fact]
    public void A_table_round_trips_through_its_text_form()
    {
        var table = new ColumnTable();
        table.AddColumn("x", [1, 2.5, 3]);
        table.AddColumn("y", [10, 20, 30.75]);

        string text = table.Save();

        Assert.StartsWith("x\ty", text, StringComparison.Ordinal);

        var reloaded = new ColumnTable();
        reloaded.Load(text);

        Assert.Equal(["x", "y"], reloaded.Names);
        Assert.Equal([1, 2.5, 3], reloaded.Column("x")!.ToArray());
        Assert.Equal([10, 20, 30.75], reloaded.Column("y")!.ToArray());
        Assert.Equal(text, reloaded.Save());
    }

    [Fact]
    public void Columns_may_differ_in_length()
    {
        // A blank cell is not a value for that column — which is how three samples of unequal
        // size sit side by side.
        var table = new ColumnTable();

        Assert.True(table.TryLoad("a\tb\tc\n1\t10\t100\n2\t20\n3", CultureInfo.InvariantCulture));

        Assert.Equal(3, table.ColumnCount);
        Assert.Equal([1, 2, 3], table.Column("a")!.ToArray());
        Assert.Equal([10, 20], table.Column("b")!.ToArray());
        Assert.Equal([100], table.Column("c")!.ToArray());
        Assert.Equal(3, table.RowCount);

        // And it survives the round trip, gaps and all.
        var reloaded = new ColumnTable();
        reloaded.Load(table.Save());
        Assert.Equal([10, 20], reloaded.Column("b")!.ToArray());
    }

    [Theory]
    [InlineData("a,b\n1,2\n3,4")]
    [InlineData("a;b\n1;2\n3;4")]
    [InlineData("a b\n1 2\n3 4")]
    [InlineData("a\tb\n1\t2\n3\t4")]
    public void Tabs_commas_semicolons_and_spaces_all_separate_columns(string text)
    {
        var table = new ColumnTable();

        Assert.True(table.TryLoad(text, CultureInfo.InvariantCulture));

        Assert.Equal(["a", "b"], table.Names);
        Assert.Equal([1, 3], table.Column("a")!.ToArray());
        Assert.Equal([2, 4], table.Column("b")!.ToArray());
    }

    [Fact]
    public void A_cell_that_is_not_a_number_rejects_the_paste_and_keeps_the_old_data()
    {
        var table = new ColumnTable();
        table.AddColumn("kept", [1, 2, 3]);

        Assert.False(table.TryLoad("a\tb\n1\t2\n3\tnot a number", CultureInfo.InvariantCulture));

        Assert.Equal(["kept"], table.Names);
        Assert.Equal([1, 2, 3], table.Column("kept")!.ToArray());
    }

    [Fact]
    public void The_first_line_names_the_columns_and_a_blank_name_is_numbered()
    {
        var table = new ColumnTable();

        Assert.True(table.TryLoad("Temperature\t\tEfficiency\n15\t1\t19.1", CultureInfo.InvariantCulture));

        Assert.Equal(["Temperature", "Column 2", "Efficiency"], table.Names);
        Assert.Equal([19.1], table.Column("Efficiency")!.ToArray());

        // Names are matched without regard to case, as a person typing one would expect.
        Assert.NotNull(table.Column("temperature"));
    }

    [Fact]
    public void An_empty_text_leaves_no_columns()
    {
        var table = new ColumnTable();
        table.AddColumn("gone", [1]);

        Assert.True(table.TryLoad("   ", CultureInfo.InvariantCulture));

        Assert.Equal(0, table.ColumnCount);
        Assert.Equal(string.Empty, table.Save());
    }

    // ── The document ──────────────────────────────────────────────────────────

    [Fact]
    public void Data_tables_are_saved_with_the_project_and_read_back()
    {
        string file = Path.Combine(Path.GetTempPath(), $"statistle-data-{Guid.NewGuid():N}.statistle");

        try
        {
            StatistleDocument document = ProjectWithData();
            document.Save(file);

            StatistleDocument reopened = StatistleDocument.Open(file);

            Assert.False(reopened.Diagnostics.HasWarnings, reopened.Diagnostics.ToString());
            Assert.Equal(2, reopened.Tables.Count);
            Assert.Equal("Tensile strength", reopened.Tables[0].Name);
            Assert.Equal(["AA2024", "AA7075"], reopened.Tables[0].Columns.Names);
            Assert.Equal(10, reopened.Tables[0].Columns.Column("AA2024")!.Count);
            Assert.Equal(17.1, reopened.Tables[1].Columns.Column("Efficiency")!.Values[^1]);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void A_project_holds_its_tables_and_its_analyses_together()
    {
        string file = Path.Combine(Path.GetTempPath(), $"statistle-both-{Guid.NewGuid():N}.statistle");

        try
        {
            StatistleDocument document = ProjectWithData();
            DescriptiveStatistics statistics = document.Tool<DescriptiveStatistics>();
            statistics.Column = DataSources.Reference(document.Tables[0].Id, "AA7075");
            statistics.Recalculate();

            SimpleRegression regression = document.Tool<SimpleRegression>();
            regression.XColumn = DataSources.Reference(document.Tables[1].Id, "Temperature");
            regression.YColumn = DataSources.Reference(document.Tables[1].Id, "Efficiency");
            regression.Recalculate();

            document.Save(file);

            StatistleDocument reopened = StatistleDocument.Open(file);

            Assert.False(reopened.Diagnostics.HasWarnings, reopened.Diagnostics.ToString());
            Assert.Equal(2, reopened.Tables.Count);
            Assert.Equal(2, reopened.Tools.Count);

            // The analyses were recomputed against the reloaded tables, not trusted from the file.
            Assert.Equal(10, reopened.Tool<DescriptiveStatistics>().Count);
            Assert.Equal(statistics.Mean, reopened.Tool<DescriptiveStatistics>().Mean, 10);
            Assert.Equal(5, reopened.Tool<SimpleRegression>().Count);
        }
        finally
        {
            File.Delete(file);
        }
    }

    // ── Choosing a column ─────────────────────────────────────────────────────

    [Fact]
    public void An_analysis_offers_every_column_of_every_table()
    {
        StatistleDocument document = ProjectWithData();
        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();

        IReadOnlyList<ChoiceOption> choices = analysis.ChoicesFor(nameof(DescriptiveStatistics.Column));

        Assert.Equal(DataSources.EnteredHere, choices[0].Label);
        Assert.Contains(choices, c => c.Label == "Tensile strength › AA2024");
        Assert.Contains(choices, c => c.Label == "Solar panel › Efficiency");
        Assert.Equal(5, choices.Count);
    }

    [Fact]
    public void Descriptive_statistics_reads_a_chosen_column_and_locks_its_sample()
    {
        StatistleDocument document = ProjectWithData();
        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();

        Assert.False(MethodScope.IsLocked(analysis, nameof(DescriptiveStatistics.Sample)));

        analysis.Column = DataSources.Reference(document.Tables[0].Id, "AA2024");
        analysis.Recalculate();

        Assert.Null(analysis.Problem);
        Assert.Equal(10, analysis.Count);
        Assert.Equal(478.7, analysis.Mean, 9);
        Assert.True(
            MethodScope.IsLocked(analysis, nameof(DescriptiveStatistics.Sample)),
            "a chosen column locks the sample");
        Assert.Same(document.Tables[0], analysis.LinkedObject(nameof(DescriptiveStatistics.Column)));
        Assert.Same(document.Tables[0], analysis.LinkedObject(nameof(DescriptiveStatistics.Sample)));

        // Edit the table: the analysis follows.
        document.Tables[0].Columns.Column("AA2024")!.Values.Clear();
        document.Tables[0].Columns.Column("AA2024")!.Values.AddRange([1, 2, 3]);
        analysis.Recalculate();
        Assert.Equal(2.0, analysis.Mean, 9);

        // Back to entered values: unlocked, and the last copied values stay.
        analysis.Column = string.Empty;
        analysis.Recalculate();
        Assert.False(MethodScope.IsLocked(analysis, nameof(DescriptiveStatistics.Sample)));
        Assert.Null(analysis.LinkedObject(nameof(DescriptiveStatistics.Column)));
        Assert.Equal(3, analysis.Count);
    }

    [Fact]
    public void Regression_takes_an_x_column_and_a_y_column_the_same_way()
    {
        StatistleDocument document = ProjectWithData();
        SimpleRegression regression = document.Tool<SimpleRegression>();

        Assert.Contains(
            regression.ChoicesFor(nameof(SimpleRegression.XColumn)),
            c => c.Label == "Solar panel › Temperature");

        regression.XColumn = DataSources.Reference(document.Tables[1].Id, "Temperature");
        regression.YColumn = DataSources.Reference(document.Tables[1].Id, "Efficiency");
        regression.Recalculate();

        Assert.Null(regression.Problem);
        Assert.Equal(5, regression.Count);
        Assert.True(MethodScope.IsLocked(regression, nameof(SimpleRegression.Data)));
        Assert.Same(document.Tables[1], regression.LinkedObject(nameof(SimpleRegression.Data)));

        Assert.All(MethodCatalog.For(typeof(DescriptiveStatistics)), m => Assert.Empty(MethodContract.Verify(m)));
        Assert.All(MethodCatalog.For(typeof(SimpleRegression)), m => Assert.Empty(MethodContract.Verify(m)));
    }

    [Fact]
    public void Columns_of_unequal_length_are_not_paired_and_the_analysis_says_why()
    {
        var document = new StatistleDocument();
        DataTable table = document.AddTable("Ragged");
        table.Columns.AddColumn("x", [1, 2, 3, 4, 5]);
        table.Columns.AddColumn("y", [2, 4, 6, 8]);

        SimpleRegression regression = document.Tool<SimpleRegression>();
        regression.XColumn = DataSources.Reference(table.Id, "x");
        regression.YColumn = DataSources.Reference(table.Id, "y");
        regression.Recalculate();

        Assert.Equal(0, regression.Count);
        Assert.Equal(0, regression.Data.Count);
        Assert.True(double.IsNaN(regression.Slope));
        Assert.Equal(
            "'Ragged › x' has 5 values and 'Ragged › y' has 4 values, so these pairs may not belong together. " +
            "'Ragged › y' is the shorter: check it for a missing or blank cell. The analysis is not run until the columns are the same length.",
            regression.Problem);
    }

    [Fact]
    public void A_deleted_table_is_reported_and_the_copied_values_kept()
    {
        StatistleDocument document = ProjectWithData();
        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();
        analysis.Column = DataSources.Reference(document.Tables[0].Id, "AA2024");
        analysis.Recalculate();

        document.Tables.Clear();
        analysis.Recalculate();

        Assert.NotNull(analysis.Problem);
        Assert.Contains("has been deleted", analysis.Problem, StringComparison.Ordinal);
        Assert.Equal(10, analysis.Count);
    }

    [Fact]
    public void A_renamed_column_is_reported_rather_than_silently_emptying_the_analysis()
    {
        StatistleDocument document = ProjectWithData();
        DescriptiveStatistics analysis = document.Tool<DescriptiveStatistics>();
        analysis.Column = DataSources.Reference(document.Tables[0].Id, "AA2024");
        analysis.Recalculate();

        document.Tables[0].Columns.Column("AA2024")!.Name = "Alloy 2024";
        analysis.Recalculate();

        Assert.NotNull(analysis.Problem);
        Assert.Contains("no longer has a column", analysis.Problem, StringComparison.Ordinal);
        Assert.Equal(10, analysis.Count);
    }

    [Fact]
    public void A_reference_round_trips_through_its_stored_form()
    {
        string reference = DataSources.Reference(7, "Efficiency");

        (int id, string name) = DataSources.Split(reference);

        Assert.Equal(7, id);
        Assert.Equal("Efficiency", name);

        // "Entered below" is the empty string, and splits to no table.
        Assert.Equal(string.Empty, DataSources.Reference(0, "anything"));
        Assert.Equal((0, string.Empty), DataSources.Split(string.Empty));
        Assert.Equal((0, string.Empty), DataSources.Split(null));
    }
}
