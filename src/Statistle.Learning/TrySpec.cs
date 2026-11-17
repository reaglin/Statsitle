using System.Globalization;
using Learning.Core.Content;
using Statistle.Core.Analyses;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;

namespace Statistle.Learning;

/// <summary>What kind of control a setting gets in a <c>[[try: …]]</c> example.</summary>
public enum TryInputKind { Unsupported, Number, Whole, Choice, Switch, Values }

/// <summary>
/// A setting the learner can change: by its property name, with the range of its slider when one is given; or a column of one of the
/// task's tables, written <c>@Table/Column</c> (the data of a regression, an ANOVA or a two-sample test from columns).
/// </summary>
public sealed record TryInput(string Name, double? Low, double? High)
{
    public bool HasRange => Low is not null && High is not null;

    /// <summary>True for <c>@Table/Column</c>: a column of the task's own data, changed as a list of values.</summary>
    public bool IsColumn => Name.StartsWith('@');

    /// <summary>The table and the column of an <see cref="IsColumn"/> input.</summary>
    public (string Table, string Column) TableColumn
    {
        get
        {
            var slash = Name.IndexOf('/', StringComparison.Ordinal);
            return slash < 0 ? (Name[1..], "") : (Name[1..slash], Name[(slash + 1)..]);
        }
    }
}

/// <summary>
/// An interactive example (Learning Core plan 3.21-3.22, Statistics+'s own try- widget): a task's analysis with some of its settings
/// to change, recalculated as they change, its figure and the results to watch drawn beside them.
/// <code>
/// [[try: st.m2.central-tendency.try Sample | Mean Median]]
/// [[try: st.m5.normal.try Mean=40..60 StandardDeviation=1..10 | ProbabilityBetween figure=1]]
/// </code>
/// The first word is the task; then the settings to change, each a property name (a number with <c>=low..high</c> as a slider) or
/// <c>@Table/Column</c> for a column of the task's table;
/// after <c>|</c> the results to show (all of them when none is named) and <c>figure=N</c> (1 = the first, the default;
/// <c>none</c>, or <c>all</c>).
/// </summary>
public sealed record TrySpec(string TaskId, IReadOnlyList<TryInput> Inputs, IReadOnlyList<string> Results, int Figure)
{
    /// <summary><see cref="Figure"/> for every figure.</summary>
    public const int AllFigures = -1;

    /// <summary>Reads a widget's argument; null with a sentence saying what to change when it cannot.</summary>
    public static TrySpec? Parse(string argument, out string? problem)
    {
        problem = null;
        var bar = argument.IndexOf('|', StringComparison.Ordinal);
        var left = Words(bar < 0 ? argument : argument[..bar]);
        var right = bar < 0 ? [] : Words(argument[(bar + 1)..]);
        if (left.Length == 0) { problem = "[[try: …]] needs a task: [[try: task-id Setting Setting=low..high | Result Result]]."; return null; }

        var inputs = new List<TryInput>();
        foreach (var token in left.Skip(1))
        {
            var eq = token.IndexOf('=', StringComparison.Ordinal);
            if (eq < 0) { inputs.Add(new TryInput(token, null, null)); continue; }
            var name = token[..eq];
            var range = token[(eq + 1)..].Split("..", StringSplitOptions.TrimEntries);
            if (range.Length != 2 || !Number(range[0], out var low) || !Number(range[1], out var high) || !(low < high))
            {
                problem = $"[[try: …]]: \"{token}\" should be Name=low..high with low below high, such as Mean=40..60.";
                return null;
            }
            inputs.Add(new TryInput(name, low, high));
        }
        if (inputs.Count == 0) { problem = $"[[try: {argument.Trim()}]] names no setting to change. Name one or more after the task, such as Sample or Mean=40..60."; return null; }
        if (inputs.GroupBy(i => i.Name, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1) is { } twice)
        {
            problem = $"[[try: …]] names the setting {twice.Key} twice.";
            return null;
        }

        var results = new List<string>();
        var figure = 1;
        foreach (var token in right)
        {
            if (token.StartsWith("figure=", StringComparison.OrdinalIgnoreCase))
            {
                var value = token["figure=".Length..];
                if (value.Equals("none", StringComparison.OrdinalIgnoreCase)) figure = 0;
                else if (value.Equals("all", StringComparison.OrdinalIgnoreCase)) figure = AllFigures;
                else if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 1) figure = n;
                else { problem = $"[[try: …]]: \"{token}\" should be figure=1 (or another number), figure=none or figure=all."; return null; }
                continue;
            }
            results.Add(token);
        }
        return new TrySpec(left[0], inputs, results, figure);
    }

    /// <summary>The words of a widget's argument: split at spaces, except inside double quotes ("@Turning/Feed (mm/rev)").</summary>
    public static string[] Words(string text)
    {
        var words = new List<string>();
        var word = new System.Text.StringBuilder();
        var quoted = false;
        foreach (var c in text)
        {
            if (c == '"') { quoted = !quoted; continue; }
            if (char.IsWhiteSpace(c) && !quoted)
            {
                if (word.Length > 0) { words.Add(word.ToString()); word.Clear(); }
                continue;
            }
            word.Append(c);
        }
        if (word.Length > 0) words.Add(word.ToString());
        return [.. words];
    }

    private static bool Number(string text, out double value) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    /// <summary>The control a setting gets: a number (with a slider when it has a range), a whole number, a list, a switch or values.</summary>
    public static TryInputKind KindOf(StatistleProperty property)
    {
        if (property.XmlName is "Id") return TryInputKind.Unsupported; // the analysis's identity in a project, not a setting
        var type = Nullable.GetUnderlyingType(property.Property.PropertyType) ?? property.Property.PropertyType;
        if (type == typeof(double) || type == typeof(float)) return TryInputKind.Number;
        if (type == typeof(int) || type == typeof(long)) return TryInputKind.Whole;
        if (type == typeof(bool)) return TryInputKind.Switch;
        if (type.IsEnum) return TryInputKind.Choice;
        if (type == typeof(DataSeries) && property.IsSingleColumnSeries) return TryInputKind.Values;
        return TryInputKind.Unsupported;
    }

    /// <summary>A setting's value now, as a task's settings write it (a series one value per line).</summary>
    public static string TextOf(Analysis analysis, StatistleProperty property) => property.Read(analysis) switch
    {
        null => "",
        DataSeries series => string.Join('\n', series.YValues.Select(v => v.ToString("R", CultureInfo.InvariantCulture))),
        double d => double.IsNaN(d) ? "NaN" : d.ToString("R", CultureInfo.InvariantCulture),
        float f => f.ToString("R", CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false",
        Enum e => e.ToString(),
        IFormattable x => x.ToString(null, CultureInfo.InvariantCulture),
        var other => other.ToString() ?? "",
    };

    /// <summary>The task with some settings changed (property name → text as a task writes it), set up and calculated.</summary>
    /// <exception cref="ContentException">A value cannot be read, or the analysis cannot be set up with it.</exception>
    public static BuiltTask Build(StatsTask task, IReadOnlyDictionary<string, string> changed)
    {
        var settings = new Dictionary<string, string>(task.Settings, StringComparer.Ordinal);
        var tables = task.Tables.ToList();
        foreach (var (name, text) in changed)
        {
            if (!name.StartsWith('@')) { settings[name] = text; continue; }
            // @Table/Column: the column's values, one per line.
            var (table, column) = new TryInput(name, null, null).TableColumn;
            var t = tables.FindIndex(x => x.Name.Equals(table, StringComparison.OrdinalIgnoreCase));
            if (t < 0) throw new ContentException($"{task.File}: there is no table \"{table}\".");
            var values = new List<double>();
            foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                values.Add(double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) || double.TryParse(line, NumberStyles.Float, CultureInfo.CurrentCulture, out v)
                    ? v : throw new ContentException($"{task.File}: \"{line}\" in {column} is not a number."));
            tables[t] = tables[t] with { Columns = [.. tables[t].Columns.Select(c => c.Name.Equals(column, StringComparison.OrdinalIgnoreCase) ? (c.Name, values.ToArray()) : c)] };
        }
        var copy = new StatsTask
        {
            Id = task.Id, Title = task.Title, Analysis = task.Analysis, Intro = task.Intro, Settings = settings, Tables = tables,
            Hints = task.Hints, File = task.File, Json = task.Json,
        };
        return StatsTaskRunner.Build(copy);
    }

    /// <summary>A task table's column as the example starts, one value per line (null when the task has no such column).</summary>
    public static string? ColumnText(StatsTask task, TryInput input)
    {
        var (table, column) = input.TableColumn;
        var found = task.Tables.FirstOrDefault(t => t.Name.Equals(table, StringComparison.OrdinalIgnoreCase))?
            .Columns.FirstOrDefault(c => c.Name.Equals(column, StringComparison.OrdinalIgnoreCase));
        return found is { Values: { } values } ? string.Join('\n', values.Select(v => v.ToString("R", CultureInfo.InvariantCulture))) : null;
    }

    /// <summary>The properties this example changes, in its order (null for a name the analysis does not have).</summary>
    public IReadOnlyList<(TryInput Input, StatistleProperty? Property)> Resolve(Analysis analysis)
    {
        var properties = analysis.Properties().ToDictionary(p => p.XmlName, StringComparer.Ordinal);
        return [.. Inputs.Select(i => (i, properties.TryGetValue(i.Name, out var p) ? p : null))];
    }

    /// <summary>What is wrong with this example in a package, one sentence each (the validator's check).</summary>
    public IEnumerable<string> Problems(StatsContent content)
    {
        if (content.Find(TaskId) is not { } task) { yield return $"[[try: …]] names the task {TaskId}, which is not in tasks/."; yield break; }
        if (task.IsExercise)
            yield return $"[[try: …]] uses {TaskId}, an exercise: an example to try never works the exercise (PLC+ D21). Give it a task of its own, without \"ask\".";
        BuiltTask built;
        try { built = StatsTaskRunner.Build(task); }
        catch (ContentException) { yield break; } // reported with the task
        var analysis = built.Analysis;
        foreach (var (input, property) in Resolve(analysis))
        {
            if (input.IsColumn)
            {
                if (input.HasRange) yield return $"[[try: {TaskId} …]]: {input.Name} is a column of values, so it takes no range.";
                if (ColumnText(task, input) is null)
                    yield return $"[[try: {TaskId} …]]: {input.Name} is not a column of the task's tables (it has: {string.Join(", ", task.Tables.SelectMany(t => t.Columns.Select(c => $"@{t.Name}/{c.Name}")))}).";
                continue;
            }
            if (property is null || property.Hidden)
            {
                yield return $"[[try: {TaskId} …]]: {analysis.Title} has no setting \"{input.Name}\". Its settings are: {string.Join(", ", analysis.Properties().Where(p => !p.IsResult && !p.Hidden && KindOf(p) != TryInputKind.Unsupported).Select(p => p.XmlName))}.";
                continue;
            }
            if (property.IsResult) { yield return $"[[try: {TaskId} …]]: \"{input.Name}\" is a result of {analysis.Title}, so it cannot be changed. Name it after the |."; continue; }
            var kind = KindOf(property);
            if (kind == TryInputKind.Unsupported) { yield return $"[[try: {TaskId} …]]: \"{input.Name}\" cannot be changed in an example (only numbers, choices, switches and a list of values can)."; continue; }
            if (input.HasRange)
            {
                if (kind is not (TryInputKind.Number or TryInputKind.Whole)) { yield return $"[[try: {TaskId} …]]: \"{input.Name}\" is not a number, so it takes no range."; continue; }
                var now = Convert.ToDouble(property.Read(analysis), CultureInfo.InvariantCulture);
                if (double.IsNaN(now) || now < input.Low || now > input.High)
                    yield return $"[[try: {TaskId} …]]: {input.Name} starts at {now.ToString("G6", CultureInfo.InvariantCulture)}, outside its slider {input.Low}..{input.High}. Widen the range, or start the task inside it.";
            }
        }
        var results = analysis.Properties().Where(p => p.IsResult && !p.Hidden).ToDictionary(p => p.XmlName, StringComparer.Ordinal);
        foreach (var name in Results)
            if (!results.ContainsKey(name))
                yield return $"[[try: {TaskId} …]]: \"{name}\" is not a result of {analysis.Title}. Its results are: {string.Join(", ", results.Keys)}.";
        var figures = analysis is IHasFigures f ? f.Figures(null).Count : 0;
        if (Figure > figures)
            yield return $"[[try: {TaskId} …]]: figure={Figure}, but {analysis.Title} draws {figures} figure(s) here.";
        if (Figure == 0 && Results.Count == 0 && figures == 0)
            yield return $"[[try: {TaskId} …]] shows nothing that moves: name results after the |, or let it draw its figure.";
    }
}
