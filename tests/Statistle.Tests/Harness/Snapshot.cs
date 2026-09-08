using System.Runtime.CompilerServices;
using System.Text;

namespace Statistle.Tests.Harness;

/// <summary>
/// Self-pinning. Once a routine's output has been reviewed and accepted, it is written to a
/// golden file and becomes the project's own oracle — so a later change surfaces as an explicit
/// diff to approve, rather than as silent drift.
///
/// A missing snapshot is written and the test <em>fails</em>, with the file named. That is
/// deliberate: a snapshot must be looked at by a person once before it can pin anything.
/// </summary>
public static class Snapshot
{
    /// <summary>Compares text against the accepted snapshot of that name.</summary>
    public static void Match(string name, string actual, [CallerFilePath] string callerPath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string folder = Path.Combine(Path.GetDirectoryName(callerPath)!, "Snapshots");
        Directory.CreateDirectory(folder);

        string file = Path.Combine(folder, name + ".txt");
        string normalised = Normalise(actual);

        if (!File.Exists(file))
        {
            File.WriteAllText(file, normalised);

            Assert.Fail(
                $"Snapshot '{name}' did not exist and has been written to:\n  {file}\n" +
                "Review the numbers, and if they are right commit the file. It is the oracle " +
                "from now on.");
        }

        string accepted = Normalise(File.ReadAllText(file));

        if (accepted == normalised)
        {
            return;
        }

        Assert.Fail(
            $"Snapshot '{name}' does not match.\n{Diff(accepted, normalised)}\n" +
            $"If the change is intended, delete {file} and re-run to regenerate it.");
    }

    /// <summary>Formats a sweep of a routine as a stable table, ready to pin.</summary>
    public static string Table(
        string header, Func<double, double> f, double from, double to, int steps, string format = "F6")
    {
        var text = new StringBuilder();
        text.AppendLine(header);

        for (int i = 0; i <= steps; i++)
        {
            double x = from + ((to - from) * i / steps);
            text.Append(x.ToString(format, System.Globalization.CultureInfo.InvariantCulture))
                .Append('\t')
                .AppendLine(f(x).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }

    private static string Normalise(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd();

    private static string Diff(string expected, string actual)
    {
        string[] before = expected.Split('\n');
        string[] after = actual.Split('\n');

        var report = new StringBuilder();
        int shown = 0;

        for (int i = 0; i < Math.Max(before.Length, after.Length) && shown < 10; i++)
        {
            string a = i < before.Length ? before[i] : "<missing>";
            string b = i < after.Length ? after[i] : "<missing>";

            if (a != b)
            {
                report.AppendLine($"  line {i + 1}: accepted [{a}] but got [{b}]");
                shown++;
            }
        }

        if (shown == 0)
        {
            report.AppendLine("  (differs only in trailing content)");
        }

        return report.ToString();
    }
}
