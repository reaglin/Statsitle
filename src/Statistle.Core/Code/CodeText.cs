using System.Globalization;
using System.Text;

namespace Statistle.Core.Code;

public enum CodeLanguage
{
    Python = 0,
    R,
}

/// <summary>
/// An analysis (or a method within one) that can write the code reproducing what it computed —
/// the course's Python (numpy, scipy.stats, pandas, statsmodels) and base R. The code declares
/// the data as variables, runs the analysis, and prints the same results in the same order and
/// with the same labels as the app, so a student can check them line by line.
/// </summary>
public interface ICodeSource
{
    /// <summary>The complete script for the language: imports, data, analysis, printed results.</summary>
    string Code(CodeLanguage language);
}

/// <summary>Formatting shared by every code writer: identifiers, numbers, vectors, prints.</summary>
public static class CodeText
{
    private const int WrapAt = 78;

    /// <summary>
    /// A column or table name as a variable: letters, digits and underscores, lower case, not
    /// starting with a digit — "AA-X" becomes <c>aa_x</c>, "Tensile strength" <c>tensile_strength</c>.
    /// </summary>
    public static string Identifier(string name, string fallback = "x")
    {
        StringBuilder id = new();

        foreach (char c in name.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) && c < 128)
            {
                id.Append(c);
            }
            else if (id.Length > 0 && id[^1] != '_')
            {
                id.Append('_');
            }
        }

        string result = id.ToString().Trim('_');

        if (result.Length == 0)
        {
            return fallback;
        }

        return char.IsDigit(result[0]) ? "v_" + result : result;
    }

    /// <summary>A number as source text: invariant culture, shortest round-trip form; NaN and infinity by language.</summary>
    public static string Number(double value, CodeLanguage language)
    {
        if (double.IsNaN(value))
        {
            return language == CodeLanguage.Python ? "float('nan')" : "NA";
        }

        if (double.IsInfinity(value))
        {
            return language == CodeLanguage.Python
                ? (value > 0 ? "float('inf')" : "float('-inf')")
                : (value > 0 ? "Inf" : "-Inf");
        }

        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// A vector assignment — <c>name = np.array([1, 2, 3])</c> or <c>name &lt;- c(1, 2, 3)</c> —
    /// wrapped and indented when long.
    /// </summary>
    public static string Vector(string name, IReadOnlyList<double> values, CodeLanguage language)
    {
        string open = language == CodeLanguage.Python ? $"{name} = np.array([" : $"{name} <- c(";
        string close = language == CodeLanguage.Python ? "])" : ")";
        string indent = new(' ', open.Length);
        StringBuilder text = new(open);
        int column = open.Length;

        for (int i = 0; i < values.Count; i++)
        {
            string item = Number(values[i], language) + (i < values.Count - 1 ? ", " : string.Empty);

            if (column + item.Length > WrapAt && i > 0)
            {
                text.Append('\n').Append(indent);
                column = indent.Length;
            }

            text.Append(item);
            column += item.Length;
        }

        return text.Append(close).ToString();
    }

    public static string Comment(string text, CodeLanguage language) => "# " + text;

    /// <summary>
    /// Prints a labelled number with a fixed number of decimals:
    /// <c>print(f"Mean: {np.mean(x):.4f}")</c> or <c>cat("Mean:", round(mean(x), 4), "\n")</c>.
    /// </summary>
    public static string Print(string label, string expression, int decimals, CodeLanguage language) =>
        language == CodeLanguage.Python
            ? $"print(f\"{Escape(label)}: {{{expression}:.{decimals}f}}\")"
            : $"cat(\"{Escape(label)}:\", round({expression}, {decimals}), \"\\n\")";

    /// <summary>Prints a label and an expression as-is (an integer, a list, a string).</summary>
    public static string PrintRaw(string label, string expression, CodeLanguage language) =>
        language == CodeLanguage.Python
            ? $"print(f\"{Escape(label)}: {{{expression}}}\")"
            : $"cat(\"{Escape(label)}:\", {expression}, \"\\n\")";

    /// <summary>The Python import lines the course uses for a script that needs these modules.</summary>
    public static string PythonImports(bool scipyStats = true, bool pandas = false, bool statsmodels = false, bool pyplot = false)
    {
        StringBuilder text = new("import numpy as np\n");

        if (pandas)
        {
            text.Append("import pandas as pd\n");
        }

        if (scipyStats)
        {
            text.Append("from scipy import stats\n");
        }

        if (statsmodels)
        {
            text.Append("import statsmodels.api as sm\n");
        }

        if (pyplot)
        {
            text.Append("import matplotlib.pyplot as plt\n");
        }

        return text.ToString();
    }

    private static string Escape(string text) => text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
