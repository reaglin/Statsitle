using System.Globalization;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>
/// UX review 2026-10-31: a paste that is refused says which line and cell, and an analysis that
/// reads a table says what to do when the project has none.
/// </summary>
public class ParseProblemTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    [Fact]
    public void A_table_cell_that_is_not_a_number_is_named_by_line_and_column()
    {
        Assert.False(ColumnTable.TryParse("x,y\r\n1,2\r\n\r\n3,abc", Invariant, out _, out string? problem));
        Assert.Equal("Line 4, column 'y': 'abc' is not a number.", problem);

        Assert.True(ColumnTable.TryParse("x,y\n1,2", Invariant, out List<DataColumn> columns, out problem));
        Assert.Null(problem);
        Assert.Equal(2, columns.Count);
    }

    [Fact]
    public void A_series_line_that_is_not_a_number_is_named()
    {
        Assert.False(DataSeries.TryParseColumn("1\n2\nseven\n", Invariant, out _, out string? problem));
        Assert.Equal("Line 3: 'seven' is not a number.", problem);

        Assert.False(DataSeries.TryParsePairs("1 2\n3", Invariant, out _, out problem));
        Assert.Equal("Line 2: '3' is not two numbers (x, then y).", problem);
    }

    [Fact]
    public void An_analysis_in_a_project_without_tables_says_how_to_add_one()
    {
        var document = new StatistleDocument();
        var anova = (OneWayAnova)document.Tool(
            AnalysisCatalog.Discover(typeof(Analysis).Assembly).Single(d => d.ToolType == typeof(OneWayAnova)));
        anova.Recalculate();

        Assert.Equal(DataSources.NoTablesYet, anova.Problem);

        document.AddTable();
        anova.Recalculate();
        Assert.Equal("Choose the data table.", anova.Problem);
    }
}
