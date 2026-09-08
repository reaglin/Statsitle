using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace Statistle.Core.Persistence;

/// <summary>
/// A table read from a file, as text cells: a delimited text or CSV file, DISTRIB's <c>.xd2</c>,
/// or one sheet of an Excel <c>.xlsx</c> workbook. The importer picks a column (a record) or two
/// (x, y pairs) from it. Cells that are not numbers are simply not data — so a header row, a
/// label column or a note in the margin does no harm.
/// </summary>
public sealed class ImportedTable
{
    public ImportedTable(string name, IReadOnlyList<string[]> rows)
    {
        Name = name;
        Rows = rows;
        ColumnCount = rows.Count == 0 ? 0 : rows.Max(r => r.Length);
        HasHeader = rows.Count > 1 && rows[0].All(c => !IsNumber(c)) && rows[0].Any(c => c.Length > 0) && rows.Skip(1).Any(r => r.Any(IsNumber));
    }

    /// <summary>The file name, or the sheet name for a workbook.</summary>
    public string Name { get; }

    public IReadOnlyList<string[]> Rows { get; }

    public int ColumnCount { get; }

    /// <summary>True when the first row has no numbers and later rows do — a title row.</summary>
    public bool HasHeader { get; }

    /// <summary>Column headings: the first row when it is a header, else "Column 1", "Column 2"…</summary>
    public IReadOnlyList<string> Headers =>
        [.. Enumerable.Range(0, ColumnCount).Select(i =>
            HasHeader && i < Rows[0].Length && Rows[0][i].Length > 0 ? Rows[0][i] : $"Column {i + 1}")];

    /// <summary>The numbers in one column, in row order, skipping every cell that is not one.</summary>
    public double[] Column(int index)
    {
        List<double> values = [];

        foreach (string[] row in Rows.Skip(HasHeader ? 1 : 0))
        {
            if (index < row.Length && TryNumber(row[index], out double value))
            {
                values.Add(value);
            }
        }

        return [.. values];
    }

    /// <summary>(x, y) from two columns, taking only the rows where both are numbers.</summary>
    public DataPoint[] Pairs(int xIndex, int yIndex)
    {
        List<DataPoint> pairs = [];

        foreach (string[] row in Rows.Skip(HasHeader ? 1 : 0))
        {
            if (xIndex < row.Length && yIndex < row.Length
                && TryNumber(row[xIndex], out double x) && TryNumber(row[yIndex], out double y))
            {
                pairs.Add(new DataPoint(x, y));
            }
        }

        return [.. pairs];
    }

    /// <summary>How many numbers each column holds — for choosing a sensible default column.</summary>
    public int[] NumericCounts() =>
        [.. Enumerable.Range(0, ColumnCount).Select(i => Column(i).Length)];

    public static bool IsNumber(string cell) => TryNumber(cell, out _);

    public static bool TryNumber(string cell, out double value)
    {
        string trimmed = (cell ?? string.Empty).Trim().Trim('"');

        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
               || double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }
}

/// <summary>
/// Reads data files into <see cref="ImportedTable"/>s. Text and CSV are split on tabs, commas,
/// semicolons or runs of spaces (quotes stripped); an <c>.xlsx</c> workbook is read sheet by
/// sheet straight from its XML — no spreadsheet library, so the free product stays small and
/// permissively licensed. The old binary <c>.xls</c> is not read: save it as .xlsx or .csv.
/// </summary>
public static class TableImport
{
    /// <summary>The file filter for an open dialog, in the order the formats are worth offering.</summary>
    public const string FileFilter =
        "Data files (*.csv;*.txt;*.dat;*.prn;*.xd2;*.xlsx)|*.csv;*.txt;*.dat;*.prn;*.xd2;*.xlsx|" +
        "Excel workbook (*.xlsx)|*.xlsx|" +
        "Text or CSV (*.csv;*.txt;*.dat;*.prn;*.xd2)|*.csv;*.txt;*.dat;*.prn;*.xd2|" +
        "All files (*.*)|*.*";

    /// <summary>Every table in the file: one for text, one per sheet for a workbook.</summary>
    public static IReadOnlyList<ImportedTable> Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string extension = Path.GetExtension(path).ToLowerInvariant();

        if (extension == ".xls")
        {
            throw new InvalidDataException(
                "The old binary Excel format (.xls) is not read. Save the sheet as .xlsx or .csv and import that.");
        }

        return extension == ".xlsx" || extension == ".xlsm"
            ? ReadWorkbook(path)
            : [ReadText(File.ReadAllText(path), Path.GetFileName(path))];
    }

    /// <summary>Delimited text: tab, comma, semicolon or spaces between cells, one row per line.</summary>
    public static ImportedTable ReadText(string text, string name)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<string[]> rows = [];

        foreach (string raw in text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            string line = raw.TrimStart('﻿').Trim();

            if (line.Length == 0)
            {
                continue;
            }

            // Tabs, commas and semicolons are hard delimiters; spaces only where no other is
            // present, so "New York, NY" style text does not explode but "1 2 3" still splits.
            char[] hard = ['\t', ',', ';'];
            string[] cells = line.IndexOfAny(hard) >= 0
                ? line.Split(hard, StringSplitOptions.TrimEntries)
                : line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            rows.Add([.. cells.Select(c => c.Trim().Trim('"'))]);
        }

        return new ImportedTable(name, rows);
    }

    /// <summary>
    /// Reads every worksheet of an Office Open XML workbook: shared strings for text cells,
    /// the value element for numbers. Cells are placed by their A1 references, so gaps are kept.
    /// </summary>
    public static IReadOnlyList<ImportedTable> ReadWorkbook(string path)
    {
        using ZipArchive zip = ZipFile.OpenRead(path);

        XElement workbook = Load(zip, "xl/workbook.xml")
            ?? throw new InvalidDataException($"{Path.GetFileName(path)} is not an Excel workbook (no xl/workbook.xml).");
        XElement? rels = Load(zip, "xl/_rels/workbook.xml.rels");

        List<string> shared = [];
        if (Load(zip, "xl/sharedStrings.xml") is XElement strings)
        {
            foreach (XElement si in strings.Elements().Where(e => e.Name.LocalName == "si"))
            {
                shared.Add(string.Concat(si.Descendants().Where(e => e.Name.LocalName == "t").Select(e => e.Value)));
            }
        }

        List<ImportedTable> tables = [];

        foreach (XElement sheet in workbook.Descendants().Where(e => e.Name.LocalName == "sheet"))
        {
            string sheetName = (string?)sheet.Attribute("name") ?? $"Sheet {tables.Count + 1}";
            string? relId = sheet.Attributes().FirstOrDefault(a => a.Name.LocalName == "id")?.Value;
            string? target = rels?.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Relationship" && (string?)e.Attribute("Id") == relId)
                ?.Attribute("Target")?.Value;

            if (target is null)
            {
                continue;
            }

            string part = target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target;
            XElement? data = Load(zip, part);

            if (data is null)
            {
                continue;
            }

            tables.Add(new ImportedTable(sheetName, ReadSheet(data, shared)));
        }

        if (tables.Count == 0)
        {
            throw new InvalidDataException($"{Path.GetFileName(path)} has no readable worksheets.");
        }

        return tables;
    }

    private static List<string[]> ReadSheet(XElement sheet, IReadOnlyList<string> shared)
    {
        Dictionary<int, Dictionary<int, string>> grid = [];
        int maxRow = 0;
        int maxColumn = 0;

        foreach (XElement cell in sheet.Descendants().Where(e => e.Name.LocalName == "c"))
        {
            string? reference = (string?)cell.Attribute("r");

            if (reference is null || !TryParseReference(reference, out int row, out int column))
            {
                continue;
            }

            string type = (string?)cell.Attribute("t") ?? "n";
            string? raw = cell.Elements().FirstOrDefault(e => e.Name.LocalName == "v")?.Value;

            string value = type switch
            {
                "s" when raw is not null && int.TryParse(raw, out int index) && index >= 0 && index < shared.Count => shared[index],
                "inlineStr" => string.Concat(cell.Descendants().Where(e => e.Name.LocalName == "t").Select(e => e.Value)),
                "b" => raw == "1" ? "TRUE" : "FALSE",
                _ => raw ?? string.Empty,
            };

            if (!grid.TryGetValue(row, out Dictionary<int, string>? cells))
            {
                cells = [];
                grid[row] = cells;
            }

            cells[column] = value;
            maxRow = Math.Max(maxRow, row);
            maxColumn = Math.Max(maxColumn, column);
        }

        List<string[]> rows = [];

        for (int r = 1; r <= maxRow; r++)
        {
            string[] line = new string[maxColumn];
            Array.Fill(line, string.Empty);

            if (grid.TryGetValue(r, out Dictionary<int, string>? cells))
            {
                foreach ((int c, string value) in cells)
                {
                    line[c - 1] = value;
                }
            }

            if (line.Any(v => v.Length > 0))
            {
                rows.Add(line);
            }
        }

        return rows;
    }

    /// <summary>"B12" → row 12, column 2.</summary>
    public static bool TryParseReference(string reference, out int row, out int column)
    {
        row = 0;
        column = 0;
        int i = 0;

        while (i < reference.Length && char.IsLetter(reference[i]))
        {
            column = column * 26 + (char.ToUpperInvariant(reference[i]) - 'A' + 1);
            i++;
        }

        return column > 0 && int.TryParse(reference[i..], out row) && row > 0;
    }

    private static XElement? Load(ZipArchive zip, string part)
    {
        ZipArchiveEntry? entry = zip.GetEntry(part)
            ?? zip.Entries.FirstOrDefault(e => string.Equals(e.FullName, part, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            return null;
        }

        using Stream stream = entry.Open();
        return XDocument.Load(stream).Root;
    }
}
