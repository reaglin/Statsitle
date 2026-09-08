using System.Globalization;
using System.Text;

namespace Statistle.Core.Persistence;

/// <summary>
/// A property value that persists itself as text and is mutated in place — like a
/// <see cref="DataSeries"/>, but for types the XML layer does not know. A property of such a
/// type needs no setter; the file carries what <see cref="Save"/> returns.
/// </summary>
public interface ITextPersisted
{
    string Save();

    void Load(string text);
}

/// <summary>
/// One named column of numbers. Columns of the same table may differ in length — three samples
/// of unequal size sit side by side quite happily.
/// </summary>
public sealed class DataColumn
{
    public DataColumn(string name, IEnumerable<double>? values = null)
    {
        Name = name ?? string.Empty;

        if (values is not null)
        {
            Values.AddRange(values);
        }
    }

    /// <summary>The heading, as it appears in an analysis's pick-list: "AA2024", "Temperature".</summary>
    public string Name { get; set; }

    public List<double> Values { get; } = [];

    public int Count => Values.Count;

    public double[] ToArray() => [.. Values];

    public override string ToString() => $"{Name} ({Count})";
}

/// <summary>
/// Columns of numbers under their names — one column for a single sample, two for x–y pairs,
/// several for groups or for predictors and a response.
///
/// The text form is what a person pastes out of a spreadsheet: a header line of column names,
/// then one row per line with the cells separated by tabs. Reading is forgiving about the
/// separator (tab, comma, semicolon, or runs of spaces) and about ragged rows — a blank cell is
/// simply not a value for that column, which is how columns come to differ in length. It is not
/// forgiving about a cell that is neither blank nor a number: that rejects the whole paste and
/// keeps what was there, so a typo cannot silently shift a column.
/// </summary>
public sealed class ColumnTable : ITextPersisted
{
    private readonly List<DataColumn> _columns = [];

    public ColumnTable()
    {
    }

    public ColumnTable(IEnumerable<DataColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);
        _columns.AddRange(columns);
    }

    /// <summary>Raised when the columns are replaced or edited.</summary>
    public event Action? Changed;

    public IReadOnlyList<DataColumn> Columns => _columns;

    public int ColumnCount => _columns.Count;

    /// <summary>The longest column's length — how many rows the text form has.</summary>
    public int RowCount => _columns.Count == 0 ? 0 : _columns.Max(c => c.Count);

    public IReadOnlyList<string> Names => [.. _columns.Select(c => c.Name)];

    /// <summary>The column of that name, or null. Names are matched without regard to case.</summary>
    public DataColumn? Column(string? name) =>
        name is null
            ? null
            : _columns.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

    public DataColumn AddColumn(string name, IEnumerable<double>? values = null)
    {
        var column = new DataColumn(name, values);
        _columns.Add(column);
        Changed?.Invoke();
        return column;
    }

    public void Clear()
    {
        _columns.Clear();
        Changed?.Invoke();
    }

    /// <summary>Replaces every column at once — what an import does.</summary>
    public void SetColumns(IEnumerable<DataColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        _columns.Clear();
        _columns.AddRange(columns);
        Changed?.Invoke();
    }

    /// <summary>
    /// The header line, then one row per line with tabs between the cells. A column that has run
    /// out of values contributes an empty cell, so a ragged table round-trips.
    /// </summary>
    public string Save()
    {
        if (_columns.Count == 0)
        {
            return string.Empty;
        }

        var text = new StringBuilder();
        text.Append(string.Join('\t', _columns.Select(c => c.Name.Replace('\t', ' '))));

        int rows = RowCount;
        for (int row = 0; row < rows; row++)
        {
            text.Append('\n');
            text.Append(string.Join(
                '\t',
                _columns.Select(c => row < c.Count
                    ? c.Values[row].ToString("R", CultureInfo.InvariantCulture)
                    : string.Empty)));
        }

        return text.ToString();
    }

    /// <summary>
    /// Reads the text form, keeping what was there if it cannot be read. Used on load, where a
    /// malformed part must not throw the whole project away.
    /// </summary>
    public void Load(string text)
    {
        if (TryParse(text, CultureInfo.InvariantCulture, out List<DataColumn> parsed)
            || TryParse(text, CultureInfo.CurrentCulture, out parsed))
        {
            SetColumns(parsed);
        }
    }

    /// <summary>
    /// Reads the text form as typed or pasted by a person. Returns false — leaving the table
    /// untouched — when a data cell is neither blank nor a number.
    /// </summary>
    public bool TryLoad(string? text, IFormatProvider culture)
    {
        if (!TryParse(text, culture, out List<DataColumn> parsed))
        {
            return false;
        }

        SetColumns(parsed);
        return true;
    }

    /// <summary>
    /// Header line, then data rows. Empty text gives no columns. A header cell left blank is
    /// named "Column n", so every column can still be picked from a list.
    /// </summary>
    public static bool TryParse(string? text, IFormatProvider culture, out List<DataColumn> columns)
    {
        columns = [];

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        string[] lines = text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return true;
        }

        string[] headers = SplitRow(lines[0]);

        for (int i = 0; i < headers.Length; i++)
        {
            string name = headers[i].Trim().Trim('"');
            columns.Add(new DataColumn(name.Length > 0 ? name : $"Column {i + 1}"));
        }

        for (int line = 1; line < lines.Length; line++)
        {
            string[] cells = SplitRow(lines[line]);

            for (int i = 0; i < cells.Length; i++)
            {
                string cell = cells[i].Trim().Trim('"');

                if (cell.Length == 0)
                {
                    // A gap: this column simply has no value in this row.
                    continue;
                }

                if (!double.TryParse(cell, NumberStyles.Float, culture, out double value))
                {
                    columns = [];
                    return false;
                }

                // A row wider than the header still has to go somewhere.
                while (i >= columns.Count)
                {
                    columns.Add(new DataColumn($"Column {columns.Count + 1}"));
                }

                columns[i].Values.Add(value);
            }
        }

        return true;
    }

    /// <summary>
    /// Splits one line into cells. Tabs, commas and semicolons are hard delimiters and keep the
    /// empty cells between them (so a gap stays a gap); with none of those present, a run of
    /// spaces separates.
    /// </summary>
    private static string[] SplitRow(string line)
    {
        string trimmed = line.TrimStart('﻿').TrimEnd();
        char[] hard = ['\t', ',', ';'];

        return trimmed.IndexOfAny(hard) >= 0
            ? trimmed.Split(hard)
            : trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    public override string ToString() => Save();
}
