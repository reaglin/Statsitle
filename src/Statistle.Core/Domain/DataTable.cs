using System.Text;
using Statistle.Core.Code;
using Statistle.Core.Document;
using Statistle.Core.Persistence;

namespace Statistle.Core.Domain;

/// <summary>
/// The one artifact a Statistle project holds: a named table of columns of numbers. Every
/// analysis reads its data from a column of one of these, so a data set is entered once, saved
/// with the project, and every analysis of it follows when it is edited.
/// </summary>
[StatistleHelp(
    "A named table of columns of numbers — one column for a single sample, two for x–y pairs, " +
    "several for groups or for predictors and a response. Columns may differ in length, so three " +
    "samples of unequal size sit side by side quite happily. Paste it straight out of a " +
    "spreadsheet: the first line is the column names, then one row per line. Give the table a " +
    "name that says what was measured and the columns names you will recognise in an analysis's " +
    "pick-list.")]
public sealed class DataTable : StatistleObject, ICodeSource
{
    [StatistleProperty(
        Label = "Data",
        ReportLabel = "Data columns",
        Role = VariableRole.Input,
        Help = "The table itself. First line: the column names. Then one row per line, with the " +
               "cells separated by tabs, commas or semicolons — whatever your spreadsheet " +
               "pastes. Leave a cell blank where a column has no value in that row; that is how " +
               "columns come to differ in length. A cell that is neither blank nor a number " +
               "rejects the whole paste and keeps the data you had, so a typo cannot silently " +
               "shift a column. Import… fills the table from a text, CSV or Excel file.")]
    public ColumnTable Columns { get; } = new();

    /// <summary>How many columns the table holds.</summary>
    [StatistleIgnore]
    public int ColumnCount => Columns.ColumnCount;

    /// <summary>The longest column's length.</summary>
    [StatistleIgnore]
    public int RowCount => Columns.RowCount;

    /// <summary>
    /// The table as variables: one vector per column, and a data frame when the columns are
    /// the same length — the two shapes the course's code works with.
    /// </summary>
    public string Code(CodeLanguage language)
    {
        StringBuilder code = new();
        string frame = CodeText.Identifier(Name, "data");
        bool rectangular = Columns.ColumnCount > 0 && Columns.Columns.All(c => c.Count == Columns.RowCount);
        var names = new List<string>();

        if (language == CodeLanguage.Python)
        {
            code.Append(CodeText.PythonImports(scipyStats: false, pandas: rectangular)).Append('\n');
        }

        code.Append(CodeText.Comment($"Data table: {Name}", language)).Append('\n');

        foreach (DataColumn column in Columns.Columns)
        {
            string variable = CodeText.Identifier(column.Name, $"column{names.Count + 1}");

            while (names.Contains(variable))
            {
                variable += "_";
            }

            names.Add(variable);
            code.Append(CodeText.Vector(variable, column.Values, language)).Append('\n');
        }

        if (rectangular)
        {
            code.Append('\n');
            code.Append(CodeText.Comment("The columns side by side, one row per observation", language)).Append('\n');
            code.Append(language == CodeLanguage.Python
                ? $"{frame} = pd.DataFrame({{{string.Join(", ", names.Select(n => $"'{n}': {n}"))}}})\nprint({frame})\n"
                : $"{frame} <- data.frame({string.Join(", ", names)})\nprint({frame})\n");
        }
        else if (Columns.ColumnCount > 1)
        {
            code.Append('\n');
            code.Append(CodeText.Comment("The columns differ in length, so they stay separate vectors rather than one data frame", language)).Append('\n');
        }

        return code.ToString();
    }
}

/// <summary>
/// Where an analysis gets its numbers: a column of one of the project's data tables, or values
/// typed into the analysis itself.
///
/// A column is named by a single stored string — "3␟efficiency" — so one drop-down picks
/// both the table and the column, and the analysis declares a method that <c>Generates</c> its
/// series while a column is chosen, which is what makes the editor lock the series and put an
/// Edit link to the table beside it.
/// </summary>
public static class DataSources
{
    /// <summary>The label of the "no table" option, and the value that means it.</summary>
    public const string EnteredHere = "Entered below";

    /// <summary>Separates the table id from the column name in a stored reference.</summary>
    private const char Separator = '␟';

    /// <summary>Builds the stored form of a column reference.</summary>
    public static string Reference(int tableId, string columnName) =>
        tableId <= 0 ? string.Empty : $"{tableId}{Separator}{columnName}";

    /// <summary>Splits a stored reference. Table id 0 means "entered below".</summary>
    public static (int TableId, string ColumnName) Split(string? reference)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return (0, string.Empty);
        }

        int at = reference.IndexOf(Separator);

        return at > 0 && int.TryParse(reference[..at], out int id)
            ? (id, reference[(at + 1)..])
            : (0, string.Empty);
    }

    /// <summary>
    /// Every column of every table in the project, as "Table › Column", with "Entered below"
    /// first. The stored value of an option is its reference string.
    /// </summary>
    public static IReadOnlyList<ChoiceOption> ColumnChoices(IUsesProject analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        List<ChoiceOption> options = [new ChoiceOption(string.Empty, EnteredHere)];

        foreach (DataTable table in analysis.Project?.Tables ?? [])
        {
            foreach (DataColumn column in table.Columns.Columns)
            {
                options.Add(new ChoiceOption(
                    Reference(table.Id, column.Name),
                    $"{table.Name} › {column.Name}",
                    $"{column.Count} value(s) in '{table.Name}'. {table.Description}".Trim()));
            }
        }

        return options;
    }

    /// <summary>The table a reference names, or null.</summary>
    public static DataTable? Table(IUsesProject analysis, string? reference)
    {
        (int id, _) = Split(reference);

        return id <= 0 ? null : analysis?.Project?.Tables.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>The column a reference names, or null.</summary>
    public static DataColumn? Column(IUsesProject analysis, string? reference)
    {
        (_, string name) = Split(reference);

        return Table(analysis, reference)?.Columns.Column(name);
    }

    /// <summary>
    /// Copies one column into <paramref name="into"/>, one value per row. Returns a sentence
    /// saying what is wrong, or null when all is well.
    /// </summary>
    public static string? PullColumn(IUsesProject analysis, int tableId, string columnName, DataSeries into)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(into);

        if (tableId <= 0)
        {
            return null;
        }

        DataTable? table = analysis.Project?.Tables.FirstOrDefault(t => t.Id == tableId);

        if (table is null)
        {
            return $"Data table {tableId} is no longer in the project; the values shown are as last copied.";
        }

        DataColumn? column = table.Columns.Column(columnName);

        if (column is null)
        {
            return $"'{table.Name}' no longer has a column called '{columnName}'; the values shown are as last copied.";
        }

        into.SetValues(column.ToArray(), 1);
        return column.Count == 0 ? $"'{table.Name} › {column.Name}' has no values yet." : null;
    }

    /// <summary>Copies the column a reference names. See <see cref="PullColumn"/>.</summary>
    public static string? Pull(IUsesProject analysis, string? reference, DataSeries into)
    {
        (int id, string name) = Split(reference);

        return PullColumn(analysis, id, name, into);
    }

    /// <summary>
    /// Copies two columns into <paramref name="into"/> as (x, y) pairs, taking only the rows
    /// where both columns have a value.
    /// </summary>
    public static string? PullPairs(IUsesProject analysis, string? xReference, string? yReference, DataSeries into)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(into);

        (int xId, string xName) = Split(xReference);
        (int yId, string yName) = Split(yReference);

        if (xId <= 0 || yId <= 0)
        {
            return null;
        }

        DataColumn? x = analysis.Project?.Tables.FirstOrDefault(t => t.Id == xId)?.Columns.Column(xName);
        DataColumn? y = analysis.Project?.Tables.FirstOrDefault(t => t.Id == yId)?.Columns.Column(yName);

        if (x is null || y is null)
        {
            return "A chosen column is no longer in the project; the pairs shown are as last copied.";
        }

        int rows = Math.Min(x.Count, y.Count);

        into.Clear();
        for (int i = 0; i < rows; i++)
        {
            into.Add(x.Values[i], y.Values[i]);
        }

        return rows switch
        {
            0 => "The chosen columns have no rows in common yet.",
            _ when x.Count != y.Count =>
                $"'{x.Name}' has {x.Count} value(s) and '{y.Name}' has {y.Count}; the first {rows} of each were paired.",
            _ => null,
        };
    }
}
