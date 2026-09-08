using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Statistle.Core.Persistence;

/// <summary>
/// How an enum value is shown to a person. A <see cref="DescriptionAttribute"/> on the member
/// wins; otherwise the name is split at its capitals, so <c>DimensionlessCurve</c> reads
/// "Dimensionless Curve" without every enum having to be annotated. The editor's drop-downs,
/// the reports and the CSV all go through here, so they cannot disagree.
/// </summary>
public static class EnumLabel
{
    public static string Of(Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string name = value.ToString();
        FieldInfo? member = value.GetType().GetField(name);

        return member?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? Humanise(name);
    }

    /// <summary>Every value's label, in declaration order.</summary>
    public static IReadOnlyList<string> LabelsOf(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        return [.. Enum.GetValues(enumType).Cast<Enum>().Select(Of)];
    }

    /// <summary>
    /// The value whose label — or raw name, so a file written with names still reads — matches.
    /// </summary>
    public static bool TryParse(Type enumType, string? text, out object? value)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        value = null;
        string wanted = (text ?? string.Empty).Trim();

        if (wanted.Length == 0)
        {
            return false;
        }

        foreach (Enum candidate in Enum.GetValues(enumType).Cast<Enum>())
        {
            if (string.Equals(Of(candidate), wanted, StringComparison.OrdinalIgnoreCase)
                || string.Equals(candidate.ToString(), wanted, StringComparison.OrdinalIgnoreCase))
            {
                value = candidate;
                return true;
            }
        }

        return false;
    }

    /// <summary>"DimensionlessCurve" → "Dimensionless Curve"; "NRCSLag" → "NRCS Lag".</summary>
    public static string Humanise(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var text = new StringBuilder(name.Length + 4);

        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];

            if (i > 0 && char.IsUpper(c))
            {
                char previous = name[i - 1];
                bool nextIsLower = i + 1 < name.Length && char.IsLower(name[i + 1]);

                if (char.IsLower(previous) || char.IsDigit(previous) || (char.IsUpper(previous) && nextIsLower))
                {
                    text.Append(' ');
                }
            }

            text.Append(c);
        }

        return text.ToString();
    }
}
