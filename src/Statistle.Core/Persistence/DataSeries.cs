using System.Globalization;
using System.Text;

namespace Statistle.Core.Persistence;

/// <summary>One (x, y) row of a series — time and depth, time and flow.</summary>
public readonly record struct DataPoint(double X, double Y);

/// <summary>
/// An ordered series stored as text: one row per line, columns separated by a tab. The layout
/// is Statistle 10's, so the shape stays familiar, but it is a real type here rather than a bare
/// <c>string</c> property that every caller had to split by hand.
/// </summary>
public sealed class DataSeries
{
    private readonly List<DataPoint> _points = [];

    public DataSeries() { }

    public DataSeries(IEnumerable<DataPoint> points) => _points.AddRange(points);

    public IReadOnlyList<DataPoint> Points => _points;

    public int Count => _points.Count;

    public void Add(double x, double y) => _points.Add(new DataPoint(x, y));

    public void Clear() => _points.Clear();

    public double[] XValues => [.. _points.Select(p => p.X)];

    public double[] YValues => [.. _points.Select(p => p.Y)];

    /// <summary>
    /// Re-spaces the x column at a new step, keeping every value: row i sits at i·step. This is
    /// what keeps a series in step with its spacing when the spacing is changed after the
    /// values were entered.
    /// </summary>
    public void Respace(double step)
    {
        if (step <= 0 || double.IsNaN(step))
        {
            return;
        }

        for (int i = 0; i < _points.Count; i++)
        {
            _points[i] = new DataPoint(i * step, _points[i].Y);
        }
    }

    /// <summary>
    /// Replaces the contents with one value per step: row i is (i·step, values[i]).
    /// </summary>
    public void SetValues(IReadOnlyList<double> values, double step)
    {
        ArgumentNullException.ThrowIfNull(values);

        _points.Clear();
        for (int i = 0; i < values.Count; i++)
        {
            _points.Add(new DataPoint(i * step, values[i]));
        }
    }

    /// <summary>
    /// Reads a single column of numbers, one per line, as typed by a person. Blank lines are
    /// skipped; anything else that is not a number fails the whole read, so a typo cannot
    /// silently drop a row and shift every later value by one step.
    /// </summary>
    public static bool TryParseColumn(string? text, IFormatProvider culture, out double[] values) =>
        TryParseColumn(text, culture, out values, out _);

    /// <summary>As <see cref="TryParseColumn(string?, IFormatProvider, out double[])"/>, naming the line that stopped the read.</summary>
    public static bool TryParseColumn(string? text, IFormatProvider culture, out double[] values, out string? problem)
    {
        List<double> parsed = [];
        values = [];
        problem = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            values = [.. parsed];
            return true;
        }

        string[] lines = text.Split('\n');

        for (int number = 1; number <= lines.Length; number++)
        {
            string line = lines[number - 1].Trim();

            if (line.Length == 0)
            {
                continue;
            }

            if (!double.TryParse(line, NumberStyles.Float | NumberStyles.AllowThousands, culture, out double value))
            {
                problem = $"Line {number}: '{line}' is not a number.";
                return false;
            }

            parsed.Add(value);
        }

        values = [.. parsed];
        return true;
    }

    /// <summary>
    /// Reads (x, y) pairs as typed or pasted by a person: one pair per line, x then y, with a
    /// tab, a comma, a semicolon or spaces between them — any mix, so a column pasted from a
    /// spreadsheet, a CSV export and a hand-typed list all work. Quotes (as a CSV export may
    /// add) are stripped; extra columns after the second are ignored; blank lines are skipped.
    /// A line that does not yield two numbers fails the whole read, so a typo cannot silently
    /// drop a point. Thousands separators are not accepted, since the comma is a delimiter.
    /// </summary>
    public static bool TryParsePairs(string? text, IFormatProvider culture, out DataSeries series) =>
        TryParsePairs(text, culture, out series, out _);

    /// <summary>As <see cref="TryParsePairs(string?, IFormatProvider, out DataSeries)"/>, naming the line that stopped the read.</summary>
    public static bool TryParsePairs(string? text, IFormatProvider culture, out DataSeries series, out string? problem)
    {
        series = new DataSeries();
        problem = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        string[] rawLines = text.Split('\n');

        for (int number = 1; number <= rawLines.Length; number++)
        {
            string raw = rawLines[number - 1];
            string line = raw.Trim().TrimStart('\uFEFF');

            if (line.Length == 0)
            {
                continue;
            }

            string[] columns = line.Split(
                ['\t', ',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (columns.Length < 2
                || !double.TryParse(columns[0].Trim('"'), NumberStyles.Float, culture, out double x)
                || !double.TryParse(columns[1].Trim('"'), NumberStyles.Float, culture, out double y))
            {
                problem = $"Line {number}: '{line}' is not two numbers (x, then y).";
                series = new DataSeries();
                return false;
            }

            series.Add(x, y);
        }

        return true;
    }

    /// <summary>Builds a series from values at a fixed step, x running 0, step, 2·step…</summary>
    public static DataSeries FromValues(IEnumerable<double> values, double step)
    {
        var series = new DataSeries();
        int i = 0;
        foreach (double value in values)
        {
            series.Add(i * step, value);
            i++;
        }

        return series;
    }

    public override string ToString()
    {
        var text = new StringBuilder();
        foreach (DataPoint point in _points)
        {
            text.Append(point.X.ToString("R", CultureInfo.InvariantCulture))
                .Append('\t')
                .Append(point.Y.ToString("R", CultureInfo.InvariantCulture))
                .Append('\n');
        }

        return text.ToString();
    }

    /// <summary>
    /// Reads the text form. A malformed row is skipped and reported through
    /// <paramref name="diagnostics"/> rather than silently dropped — Statistle 10 swallowed every
    /// conversion failure, so bad data became an invisible no-op.
    /// </summary>
    public static DataSeries Parse(string? text, string context, LoadDiagnostics? diagnostics = null)
    {
        var series = new DataSeries();
        if (string.IsNullOrWhiteSpace(text))
        {
            return series;
        }

        string[] lines = text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            // Tab is what is written; a person pasting x, y pairs may separate them with a
            // comma, a semicolon or spaces, and all of those are accepted.
            string[] columns = lines[i].Split(
                ['\t', ',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (columns.Length == 1
                && double.TryParse(columns[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double only))
            {
                // A single column is a value at its row index.
                series.Add(i, only);
                continue;
            }

            if (columns.Length >= 2
                && double.TryParse(columns[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x)
                && double.TryParse(columns[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
            {
                series.Add(x, y);
                continue;
            }

            diagnostics?.Add(context, lines[i], $"row {i + 1} is not a number pair; skipped");
        }

        return series;
    }
}
