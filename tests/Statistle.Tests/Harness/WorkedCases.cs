using System.Globalization;

namespace Statistle.Tests.Harness;

/// <summary>One case from a data file: inputs, the expected answer, and where it came from.</summary>
public sealed record WorkedCase(
    IReadOnlyList<double> Inputs,
    double Expected,
    double Tolerance,
    string Source,
    string Notes)
{
    public double Input(int index) => Inputs[index];

    /// <summary>xUnit shows this in the runner, so a failure names the case, not just a row.</summary>
    public override string ToString() =>
        $"{string.Join(", ", Inputs.Select(i => i.ToString("G6", CultureInfo.InvariantCulture)))} " +
        $"=> {Expected.ToString("G6", CultureInfo.InvariantCulture)} [{Source}]";
}

/// <summary>
/// Loads worked cases from a CSV beside the tests. Tier 2 of the verification plan: cases that
/// are checkable against a closed form or a published source.
///
/// Keeping them as data rather than code is the point — a case from the textbook can be added
/// by editing one line, which is what makes it practical to have a lot of them.
/// </summary>
public static class WorkedCases
{
    public static IReadOnlyList<WorkedCase> Load(string fileName, int inputCount)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "WorkedCases", fileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Worked-case file not found: {path}", path);
        }

        List<WorkedCase> cases = [];
        int lineNumber = 0;

        foreach (string raw in File.ReadLines(path))
        {
            lineNumber++;
            string line = raw.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            string[] fields = [.. line.Split(',').Select(f => f.Trim())];

            // inputs..., expected, tolerance, source, notes
            int required = inputCount + 3;
            if (fields.Length < required)
            {
                throw new InvalidDataException(
                    $"{fileName} line {lineNumber}: expected at least {required} fields, found {fields.Length}.");
            }

            double[] inputs = [.. fields.Take(inputCount).Select(ParseNumber)];
            double expected = ParseNumber(fields[inputCount]);
            double tolerance = ParseNumber(fields[inputCount + 1]);
            string source = fields[inputCount + 2];
            string notes = fields.Length > required ? string.Join(", ", fields.Skip(required)) : string.Empty;

            // A number with no provenance is not evidence.
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidDataException(
                    $"{fileName} line {lineNumber}: every worked case must name its source.");
            }

            cases.Add(new WorkedCase(inputs, expected, tolerance, source, notes));
        }

        if (cases.Count == 0)
        {
            throw new InvalidDataException($"{fileName} contains no cases.");
        }

        return cases;
    }

    /// <summary>Wraps cases for xUnit's <c>MemberData</c>.</summary>
    public static IEnumerable<object[]> AsTheoryData(string fileName, int inputCount) =>
        Load(fileName, inputCount).Select(c => new object[] { c });

    private static double ParseNumber(string text) =>
        double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
}
