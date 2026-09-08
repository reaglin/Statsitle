using System.Globalization;
using System.Text;

namespace Statistle.Core.Code;

/// <summary>
/// Builds one script line by line in either language, so an analysis writes each result once:
/// <c>Print("Mean", "np.mean(x)", "mean(x)", 4)</c> becomes the right print for the language.
/// </summary>
public sealed class ScriptBuilder(CodeLanguage language)
{
    private readonly StringBuilder _text = new();

    public CodeLanguage Language { get; } = language;

    public bool IsPython => Language == CodeLanguage.Python;

    public ScriptBuilder Line(string text = "")
    {
        _text.Append(text).Append('\n');
        return this;
    }

    /// <summary>One line in the language: <paramref name="python"/> or <paramref name="r"/>.</summary>
    public ScriptBuilder Either(string python, string r) => Line(IsPython ? python : r);

    public ScriptBuilder Comment(string text) => Line(CodeText.Comment(text, Language));

    public ScriptBuilder Imports(bool scipyStats = true, bool pandas = false, bool statsmodels = false, bool pyplot = false) =>
        IsPython ? Line(CodeText.PythonImports(scipyStats, pandas, statsmodels, pyplot)) : this;

    public ScriptBuilder Vector(string name, IReadOnlyList<double> values) => Line(CodeText.Vector(name, values, Language));

    /// <summary>An assignment: <c>name = value</c> or <c>name &lt;- value</c>.</summary>
    public ScriptBuilder Assign(string name, string value) => Line(IsPython ? $"{name} = {value}" : $"{name} <- {value}");

    public ScriptBuilder Assign(string name, double value) => Assign(name, CodeText.Number(value, Language));

    public ScriptBuilder Assign(string name, int value) => Assign(name, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Prints a labelled number to the given decimals — the same label the app shows.</summary>
    public ScriptBuilder Print(string label, string python, string r, int decimals) =>
        Line(CodeText.Print(label, IsPython ? python : r, decimals, Language));

    /// <summary>Prints a labelled expression as-is (an integer, a string).</summary>
    public ScriptBuilder PrintRaw(string label, string python, string r) =>
        Line(CodeText.PrintRaw(label, IsPython ? python : r, Language));

    /// <summary>Prints a label with a literal text — a note the app also shows.</summary>
    public ScriptBuilder PrintText(string label, string text)
    {
        string quoted = "\"" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
        return Line(IsPython ? $"print(\"{label}: \" + {quoted})" : $"cat(\"{label}:\", {quoted}, \"\\n\")");
    }

    public override string ToString() => _text.ToString();
}
