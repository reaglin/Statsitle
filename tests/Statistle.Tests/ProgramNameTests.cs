using System.Text.RegularExpressions;

namespace Statistle.Tests;

/// <summary>
/// Plan 7.3 (Ron, 2026-11-11): the program is called Statistics+ in front of a person and keeps its code name,
/// Statistle, in the code, the .statistle files and the Documents\Statistle folder.
/// </summary>
public partial class ProgramNameTests
{
    // "Statistle" as a word a person reads: not a namespace (Statistle.Core), not a path (\Statistle, /statistle).
    [GeneratedRegex(@"(?<![\\/\w.])Statistle(?![\w]|\.[A-Za-z])")]
    private static partial Regex ReadableName();

    [GeneratedRegex("\"(?:[^\"\\\\\\n]|\\\\.)*\"")]
    private static partial Regex Quoted();

    private static string Root()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return dir ?? throw new InvalidOperationException("Statistle.sln not found above the test folder.");
    }

    [Fact]
    public void No_text_a_person_reads_says_Statistle()
    {
        var root = Root();
        var offenders = new List<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".xaml", StringComparison.Ordinal))
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                 && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var code = line.TrimStart();
                if (code.StartsWith("//", StringComparison.Ordinal) || code.StartsWith("<!--", StringComparison.Ordinal)) continue;
                if (line.Contains("FolderName", StringComparison.Ordinal) || line.Contains("SpecialFolder", StringComparison.Ordinal)
                    || line.Contains("x:Class", StringComparison.Ordinal) || line.Contains("clr-namespace", StringComparison.Ordinal)
                    || line.Contains("LearnerFileOptions", StringComparison.Ordinal)) continue;
                if (Quoted().Matches(line).Any(m => ReadableName().IsMatch(m.Value)))
                    offenders.Add($"{Path.GetRelativePath(root, file)}:{i + 1}");
            }
        }
        Assert.True(offenders.Count == 0, "Say Statistics+ to a person: " + string.Join(", ", offenders));
    }

    [Fact]
    public void The_package_manifest_shows_Statistics_plus()
    {
        var manifest = File.ReadAllText(Path.Combine(Root(), "packaging", "AppxManifest.xml"));
        Assert.Contains("<DisplayName>Statistics+</DisplayName>", manifest, StringComparison.Ordinal);
        Assert.Contains("DisplayName=\"Statistics+\"", manifest, StringComparison.Ordinal);
        Assert.Contains("Version=\"2.0.0.0\"", manifest, StringComparison.Ordinal);
    }
}
