using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Statistle.Core.Persistence;

namespace Statistle.App.Views;

/// <summary>
/// Shows what a data file holds and asks which column (a record) or columns (x, y pairs) to
/// take. Defaults are guessed — the last numeric column for a record, the first two for pairs,
/// a header row recognised — and can be changed before importing.
/// </summary>
public partial class ImportDialog : Window
{
    /// <summary>What the import fills: one column of values, (x, y) pairs, or a whole data table.</summary>
    public enum ImportKind
    {
        Values,
        Pairs,
        Table,
    }

    private const int PreviewRows = 40;

    private readonly IReadOnlyList<ImportedTable> _tables;
    private readonly bool _pairs;
    private readonly bool _wholeTable;
    private readonly int _columnsNow;
    private ImportedTable _table;

    public ImportDialog(IReadOnlyList<ImportedTable> tables, bool pairs, string target, string fileName)
        : this(tables, pairs ? ImportKind.Pairs : ImportKind.Values, target, fileName, 0)
    {
    }

    /// <param name="columnsNow">For a whole table: how many columns it has now, which the import replaces.</param>
    public ImportDialog(IReadOnlyList<ImportedTable> tables, ImportKind kind, string target, string fileName, int columnsNow)
    {
        ArgumentNullException.ThrowIfNull(tables);

        if (tables.Count == 0)
        {
            throw new ArgumentException("Nothing to import.", nameof(tables));
        }

        InitializeComponent();

        _tables = tables;
        _pairs = kind == ImportKind.Pairs;
        _wholeTable = kind == ImportKind.Table;
        _columnsNow = columnsNow;
        _table = tables[0];

        Heading.Text = $"Import into {target}";
        Summary.Text = kind switch
        {
            ImportKind.Pairs => $"{fileName}: choose the x and y columns. Rows where either is not a number are skipped; a header row is recognized.",
            ImportKind.Table => $"{fileName}: every column shown below becomes a column of the table, replacing the columns it has now. " +
                                "A row with text in a number column (such as NA or #DIV/0!) is left out of every column, so the rows stay lined up; " +
                                "a blank cell is skipped in its own column only. A header row gives the column names." +
                                (tables.Count > 1 ? " Choose the sheet to import." : string.Empty),
            _ => $"{fileName}: choose the column of values. Cells that are not numbers are skipped; a header row is recognized.",
        };

        SheetLabel.Visibility = tables.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        SheetBox.Visibility = SheetLabel.Visibility;
        SheetBox.ItemsSource = tables.Select(t => t.Name).ToList();
        SheetBox.SelectedIndex = 0;

        SecondLabel.Visibility = _pairs ? Visibility.Visible : Visibility.Collapsed;
        SecondBox.Visibility = SecondLabel.Visibility;
        FirstLabel.Text = _pairs ? "x column" : "Value column";
        FirstLabel.Visibility = _wholeTable ? Visibility.Collapsed : Visibility.Visible;
        FirstBox.Visibility = FirstLabel.Visibility;
        FirstBox.ToolTip = _pairs ? "The column that holds the x values" : "The column whose numbers are imported";
        System.Windows.Automation.AutomationProperties.SetName(FirstBox, FirstLabel.Text);

        ShowTable(_table);
    }

    /// <summary>The values chosen, for a single-column series. Set when the dialog closes with Import.</summary>
    public double[]? Values { get; private set; }

    /// <summary>The pairs chosen, for an (x, y) series.</summary>
    public DataPoint[]? Pairs { get; private set; }

    /// <summary>The sheet chosen, for a whole table. Set when the dialog closes with Import.</summary>
    public ImportedTable? Table { get; private set; }

    /// <summary>The columns of a sheet that hold numbers — the ones a whole-table import takes.</summary>
    public static int[] NumericColumns(ImportedTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        int[] counts = table.NumericCounts();
        return [.. Enumerable.Range(0, table.ColumnCount).Where(i => counts[i] > 0)];
    }

    /// <summary>F1 in the dialog: the web help page on data tables and importing.</summary>
    private void Help_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
        WebHelp.Open(this, "data.html");

    private void ShowTable(ImportedTable table)
    {
        _table = table;

        Preview.Columns.Clear();
        IReadOnlyList<string> headers = table.Headers;

        for (int i = 0; i < table.ColumnCount; i++)
        {
            Preview.Columns.Add(new DataGridTextColumn
            {
                Header = headers[i],
                Binding = new Binding($"[{i}]"),
                MinWidth = 70,
            });
        }

        Preview.ItemsSource = table.Rows.Skip(table.HasHeader ? 1 : 0).Take(PreviewRows)
            .Select(r => PadRow(r, table.ColumnCount)).ToList();

        FirstBox.ItemsSource = headers;
        SecondBox.ItemsSource = headers;

        int[] counts = table.NumericCounts();
        int lastNumeric = Array.FindLastIndex(counts, c => c > 0);
        int firstNumeric = Array.FindIndex(counts, c => c > 0);

        if (_pairs)
        {
            FirstBox.SelectedIndex = Math.Max(firstNumeric, 0);
            int second = Array.FindIndex(counts, Math.Max(firstNumeric, 0) + 1, c => c > 0);
            SecondBox.SelectedIndex = second >= 0 ? second : Math.Min(1, table.ColumnCount - 1);
        }
        else
        {
            FirstBox.SelectedIndex = Math.Max(lastNumeric, 0);
        }

        DescribeOutcome();
    }

    private static string[] PadRow(string[] row, int width)
    {
        if (row.Length >= width)
        {
            return row;
        }

        string[] padded = new string[width];
        Array.Fill(padded, string.Empty);
        row.CopyTo(padded, 0);
        return padded;
    }

    private void DescribeOutcome()
    {
        if (_table.ColumnCount == 0)
        {
            Outcome.Text = "The file holds no cells.";
            OkButton.IsEnabled = false;
            return;
        }

        string header = _table.HasHeader ? " (header row skipped)." : ".";

        if (_wholeTable)
        {
            int[] numeric = NumericColumns(_table);
            int columns = numeric.Length;
            int textOnly = _table.ColumnCount - columns;
            TableColumnsImport together = _table.ColumnsTogether(numeric);
            string? leftOut = together.Describe(preview: true);
            string? lengths = together.LengthNote([.. numeric.Select(i => _table.Headers[i])], preview: true);
            Outcome.Text = $"{ViewModels.WorkspaceViewModel.Count(columns, "column", "columns")} will be imported" +
                           (textOnly > 0 ? $" ({ViewModels.WorkspaceViewModel.Count(textOnly, "column", "columns")} with no numbers left out)" : string.Empty) +
                           (_columnsNow > 0 ? $", replacing the {ViewModels.WorkspaceViewModel.Count(_columnsNow, "column", "columns")} the table has now" : string.Empty) +
                           ". " + (leftOut is null ? string.Empty : leftOut + " ") +
                           (lengths is null ? string.Empty : lengths + " ") +
                           "Edit ▸ Undo (Ctrl+Z) puts the old data back.";
            OkButton.IsEnabled = columns > 0;
        }
        else if (_pairs)
        {
            int count = FirstBox.SelectedIndex >= 0 && SecondBox.SelectedIndex >= 0
                ? _table.Pairs(FirstBox.SelectedIndex, SecondBox.SelectedIndex).Length
                : 0;
            Outcome.Text = $"{ViewModels.WorkspaceViewModel.Count(count, "pair", "pairs")} will be imported" + header;
            OkButton.IsEnabled = count > 0;
        }
        else
        {
            int count = FirstBox.SelectedIndex >= 0 ? _table.Column(FirstBox.SelectedIndex).Length : 0;
            Outcome.Text = $"{ViewModels.WorkspaceViewModel.Count(count, "value", "values")} will be imported" + header;
            OkButton.IsEnabled = count > 0;
        }

        OkButton.ToolTip = OkButton.IsEnabled
            ? "Imports what is described on the left and closes this window"
            : "Nothing to import: the chosen column or sheet has no numbers";
    }

    private void SheetBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SheetBox.SelectedIndex >= 0 && SheetBox.SelectedIndex < _tables.Count)
        {
            ShowTable(_tables[SheetBox.SelectedIndex]);
        }
    }

    private void Column_SelectionChanged(object sender, SelectionChangedEventArgs e) => DescribeOutcome();

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (_wholeTable)
        {
            Table = _table;
        }
        else if (_pairs)
        {
            Pairs = _table.Pairs(FirstBox.SelectedIndex, SecondBox.SelectedIndex);
        }
        else
        {
            Values = _table.Column(FirstBox.SelectedIndex);
        }

        DialogResult = true;
    }
}
