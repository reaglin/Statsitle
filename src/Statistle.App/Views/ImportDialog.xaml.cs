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
    private const int PreviewRows = 40;

    private readonly IReadOnlyList<ImportedTable> _tables;
    private readonly bool _pairs;
    private ImportedTable _table;

    public ImportDialog(IReadOnlyList<ImportedTable> tables, bool pairs, string target, string fileName)
    {
        ArgumentNullException.ThrowIfNull(tables);

        if (tables.Count == 0)
        {
            throw new ArgumentException("Nothing to import.", nameof(tables));
        }

        InitializeComponent();

        _tables = tables;
        _pairs = pairs;
        _table = tables[0];

        Heading.Text = $"Import into {target}";
        Summary.Text = pairs
            ? $"{fileName}: choose the x and y columns. Rows where either is not a number are skipped; a header row is recognised."
            : $"{fileName}: choose the column of values. Cells that are not numbers are skipped; a header row is recognised.";

        SheetLabel.Visibility = tables.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        SheetBox.Visibility = SheetLabel.Visibility;
        SheetBox.ItemsSource = tables.Select(t => t.Name).ToList();
        SheetBox.SelectedIndex = 0;

        SecondLabel.Visibility = pairs ? Visibility.Visible : Visibility.Collapsed;
        SecondBox.Visibility = SecondLabel.Visibility;
        FirstLabel.Text = pairs ? "x column" : "Value column";

        ShowTable(_table);
    }

    /// <summary>The values chosen, for a single-column series. Set when the dialog closes with Import.</summary>
    public double[]? Values { get; private set; }

    /// <summary>The pairs chosen, for an (x, y) series.</summary>
    public DataPoint[]? Pairs { get; private set; }

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

        if (_pairs)
        {
            int count = FirstBox.SelectedIndex >= 0 && SecondBox.SelectedIndex >= 0
                ? _table.Pairs(FirstBox.SelectedIndex, SecondBox.SelectedIndex).Length
                : 0;
            Outcome.Text = $"{count} pair(s) will be imported" + (_table.HasHeader ? " (header row skipped)." : ".");
            OkButton.IsEnabled = count > 0;
        }
        else
        {
            int count = FirstBox.SelectedIndex >= 0 ? _table.Column(FirstBox.SelectedIndex).Length : 0;
            Outcome.Text = $"{count} value(s) will be imported" + (_table.HasHeader ? " (header row skipped)." : ".");
            OkButton.IsEnabled = count > 0;
        }
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
        if (_pairs)
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
