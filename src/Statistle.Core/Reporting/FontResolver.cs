using System.Collections.Concurrent;
using PdfSharp.Fonts;

namespace Statistle.Core.Reporting;

/// <summary>
/// Supplies font files to PDFsharp.
///
/// The core PDFsharp package carries no fonts and does not resolve platform ones by itself — it
/// throws "No appropriate font found" the first time a font is asked for. Rather than take the
/// GDI or WPF flavour of the package (which would put a UI dependency in <c>Statistle.Core</c> and
/// break the architecture rule), this reads the font files directly.
///
/// It is file IO, not a UI reference, so Core stays platform-neutral in what it links against
/// while still finding real fonts on the machine it runs on.
/// </summary>
public sealed class StatistleFontResolver : IFontResolver
{
    private static readonly ConcurrentDictionary<string, byte[]> Cache = new();

    /// <summary>Families tried in order, each as (regular, bold) file names.</summary>
    private static readonly (string Family, string Regular, string Bold)[] Candidates =
    [
        ("segoe ui", "segoeui.ttf", "segoeuib.ttf"),
        ("arial", "arial.ttf", "arialbd.ttf"),
        ("dejavu sans", "DejaVuSans.ttf", "DejaVuSans-Bold.ttf"),
        ("liberation sans", "LiberationSans-Regular.ttf", "LiberationSans-Bold.ttf"),
    ];

    private static readonly string[] SearchFolders =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"),
        "/usr/share/fonts/truetype/dejavu",
        "/usr/share/fonts/truetype/liberation",
        "/Library/Fonts",
    ];

    /// <summary>Installs this resolver once, before any font is used.</summary>
    public static void Install()
    {
        if (GlobalFontSettings.FontResolver is null)
        {
            GlobalFontSettings.FontResolver = new StatistleFontResolver();
        }
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        foreach ((string family, string regular, string bold) in Candidates)
        {
            string file = isBold ? bold : regular;

            if (Find(file) is not null)
            {
                return new FontResolverInfo($"{family}|{(isBold ? "b" : "r")}");
            }
        }

        return null;
    }

    public byte[]? GetFont(string faceName)
    {
        return Cache.GetOrAdd(faceName, name =>
        {
            string[] parts = name.Split('|');
            bool bold = parts.Length > 1 && parts[1] == "b";

            foreach ((string family, string regular, string boldFile) in Candidates)
            {
                if (!string.Equals(family, parts[0], StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string? path = Find(bold ? boldFile : regular) ?? Find(regular);

                if (path is not null)
                {
                    return File.ReadAllBytes(path);
                }
            }

            // Whatever is available, rather than failing the whole report for a missing face.
            foreach ((_, string regular, _) in Candidates)
            {
                if (Find(regular) is string fallback)
                {
                    return File.ReadAllBytes(fallback);
                }
            }

            return [];
        });
    }

    private static string? Find(string fileName)
    {
        foreach (string folder in SearchFolders)
        {
            if (string.IsNullOrEmpty(folder))
            {
                continue;
            }

            string candidate = Path.Combine(folder, fileName);

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
