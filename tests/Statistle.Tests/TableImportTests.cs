using System.IO.Compression;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>Importing data from text, CSV, DISTRIB's .xd2 and Excel .xlsx (2026-09-07).</summary>
public class TableImportTests
{
    [Fact]
    public void Delimited_text_is_read_with_a_header_recognised()
    {
        ImportedTable table = TableImport.ReadText("Year,Peak\n1990,3.2\n1991,4.8\n\n1992,2.9\n", "peaks.csv");

        Assert.True(table.HasHeader);
        Assert.Equal(["Year", "Peak"], table.Headers);
        Assert.Equal(2, table.ColumnCount);
        Assert.Equal([3.2, 4.8, 2.9], table.Column(1));
        Assert.Equal([1990, 1991, 1992], table.Column(0));
        Assert.Equal(3, table.Pairs(0, 1).Length);
    }

    [Theory]
    [InlineData("1\t2\n3\t4")]
    [InlineData("1;2\n3;4")]
    [InlineData("1 2\n3 4")]
    [InlineData("\"1\",\"2\"\r\n\"3\",\"4\"")]
    [InlineData("﻿1, 2\n3, 4\n")]
    public void Tabs_semicolons_spaces_quotes_and_a_byte_order_mark_all_read(string text)
    {
        ImportedTable table = TableImport.ReadText(text, "t");

        Assert.False(table.HasHeader);
        Assert.Equal([1.0, 3.0], table.Column(0));
        Assert.Equal([2.0, 4.0], table.Column(1));
    }

    [Fact]
    public void A_DISTRIB_xd2_file_is_one_column_of_values()
    {
        ImportedTable table = TableImport.ReadText(" 1.06 \n 1.1 \n 1.14 \n", "PANAMA.XD2");

        Assert.Equal(1, table.ColumnCount);
        Assert.Equal([1.06, 1.1, 1.14], table.Column(0));
        Assert.Equal([3], table.NumericCounts());
    }

    [Fact]
    public void Cells_that_are_not_numbers_are_skipped_not_fatal()
    {
        ImportedTable table = TableImport.ReadText("Station A\n1.5\nn/a\n2.5\n", "notes.txt");

        Assert.Equal([1.5, 2.5], table.Column(0));
    }

    [Fact]
    public void A_whole_table_import_leaves_out_a_row_with_text_in_every_column_so_pairs_stay_together()
    {
        ImportedTable table = TableImport.ReadText(
            "x,y\n1,10\n2,20\n3,NA\n4,40\n#DIV/0!,50\n6,60\n", "pairs.csv");

        TableColumnsImport imported = table.ColumnsTogether([0, 1]);

        Assert.Equal([1.0, 2, 4, 6], imported.Columns[0]);
        Assert.Equal([10.0, 20, 40, 60], imported.Columns[1]);
        Assert.Equal([4, 6], imported.LeftOut.Select(r => r.RowNumber));
        Assert.Equal(
            "Rows 4 and 6 were left out: 'NA' in y, '#DIV/0!' in x. The other rows keep their order.",
            imported.Description);
    }

    [Fact]
    public void Blank_cells_in_groups_of_different_sizes_are_only_skipped_in_their_own_column()
    {
        ImportedTable table = TableImport.ReadText("A,B,C\n1,2,3\n4,5,6\n7,,9\n10,,\n", "groups.csv");

        TableColumnsImport imported = table.ColumnsTogether([0, 1, 2]);

        Assert.Equal([1.0, 4, 7, 10], imported.Columns[0]);
        Assert.Equal([2.0, 5], imported.Columns[1]);
        Assert.Equal([3.0, 6, 9], imported.Columns[2]);
        Assert.Empty(imported.LeftOut);
        Assert.Null(imported.Description);
    }

    [Fact]
    public void One_left_out_row_is_named_in_the_singular_and_its_number_counts_blank_lines()
    {
        ImportedTable table = TableImport.ReadText("x,y\r\n1,2\r\n\r\n3,n/a\r\n5,6\r\n", "t.csv");

        TableColumnsImport imported = table.ColumnsTogether([0, 1]);

        Assert.Equal([1.0, 5], imported.Columns[0]);
        Assert.Equal("Row 4 was left out: 'n/a' in y. The other rows keep their order.", imported.Description);
    }

    [Fact]
    public void An_xlsx_workbook_is_read_sheet_by_sheet_with_shared_strings()
    {
        string file = Path.Combine(Path.GetTempPath(), $"statistle-import-{Guid.NewGuid():N}.xlsx");

        try
        {
            WriteWorkbook(file);

            IReadOnlyList<ImportedTable> tables = TableImport.Read(file);

            Assert.Equal(2, tables.Count);
            Assert.Equal("Storm", tables[0].Name);
            Assert.True(tables[0].HasHeader);
            Assert.Equal(["Time (h)", "Depth (in)"], tables[0].Headers);
            Assert.Equal([0.1, 0.4, 0.9], tables[0].Column(1));
            Assert.Equal([0.0, 0.25, 0.5], tables[0].Column(0));

            // The second sheet has a gap (no B2) and an inline string; pairs skip the gap.
            Assert.Equal("Pairs", tables[1].Name);
            Assert.Equal(2, tables[1].Pairs(0, 1).Length);
            Assert.Equal(new DataPoint(3, 30), tables[1].Pairs(0, 1)[1]);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void The_old_binary_xls_is_refused_with_advice()
    {
        string file = Path.Combine(Path.GetTempPath(), $"statistle-import-{Guid.NewGuid():N}.xls");
        File.WriteAllBytes(file, [1, 2, 3]);

        try
        {
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => TableImport.Read(file));
            Assert.Contains(".xlsx", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void Cell_references_parse()
    {
        Assert.True(TableImport.TryParseReference("B12", out int row, out int column));
        Assert.Equal(12, row);
        Assert.Equal(2, column);

        Assert.True(TableImport.TryParseReference("AA1", out _, out int aa));
        Assert.Equal(27, aa);

        Assert.False(TableImport.TryParseReference("12", out _, out _));
    }

    /// <summary>A minimal Office Open XML workbook: two sheets, shared strings, one inline string.</summary>
    private static void WriteWorkbook(string path)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        const string rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);

        Add(zip, "[Content_Types].xml", """
            <?xml version="1.0" encoding="UTF-8"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
            </Types>
            """);
        Add(zip, "_rels/.rels", """
            <?xml version="1.0" encoding="UTF-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
            </Relationships>
            """);
        Add(zip, "xl/workbook.xml", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <workbook xmlns="{ns}" xmlns:r="{rel}">
              <sheets>
                <sheet name="Storm" sheetId="1" r:id="rId1"/>
                <sheet name="Pairs" sheetId="2" r:id="rId2"/>
              </sheets>
            </workbook>
            """);
        Add(zip, "xl/_rels/workbook.xml.rels", """
            <?xml version="1.0" encoding="UTF-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
              <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="/xl/worksheets/sheet2.xml"/>
              <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
            </Relationships>
            """);
        Add(zip, "xl/sharedStrings.xml", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <sst xmlns="{ns}" count="2" uniqueCount="2">
              <si><t>Time (h)</t></si>
              <si><r><t>Depth </t></r><r><t>(in)</t></r></si>
            </sst>
            """);
        Add(zip, "xl/worksheets/sheet1.xml", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <worksheet xmlns="{ns}">
              <sheetData>
                <row r="1"><c r="A1" t="s"><v>0</v></c><c r="B1" t="s"><v>1</v></c></row>
                <row r="2"><c r="A2"><v>0</v></c><c r="B2"><v>0.1</v></c></row>
                <row r="3"><c r="A3"><v>0.25</v></c><c r="B3"><v>0.4</v></c></row>
                <row r="4"><c r="A4"><v>0.5</v></c><c r="B4"><v>0.9</v></c></row>
              </sheetData>
            </worksheet>
            """);
        Add(zip, "xl/worksheets/sheet2.xml", $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <worksheet xmlns="{ns}">
              <sheetData>
                <row r="1"><c r="A1"><v>1</v></c><c r="B1"><v>10</v></c></row>
                <row r="2"><c r="A2"><v>2</v></c><c r="B2" t="inlineStr"><is><t>missing</t></is></c></row>
                <row r="3"><c r="A3"><v>3</v></c><c r="B3"><v>30</v></c></row>
              </sheetData>
            </worksheet>
            """);
    }

    private static void Add(ZipArchive zip, string name, string content)
    {
        ZipArchiveEntry entry = zip.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open());
        writer.Write(content.Trim());
    }
}
