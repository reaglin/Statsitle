using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;
using Statistle.Core.Analyses;
using Statistle.Core.Units;

namespace Statistle.App.ViewModels;

/// <summary>
/// One editable property, driven entirely by <see cref="StatistleProperty"/> — label, decimals,
/// value kind and help all come from the attribute on the domain property. Nothing here knows
/// what a data table is, so a property added to the domain shows up in the editor with no UI
/// change.
/// </summary>
public sealed class PropertyRow : INotifyPropertyChanged
{
    /// <summary>
    /// Which analysis, if any, fills a given property — asked once per (type, property) rather
    /// than re-discovering the catalog for every row of every form.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type, string), ValueProvider?> Providers = new();

    private readonly StatistleObject _target;
    private readonly StatistleProperty _property;
    private readonly UnitSelection _units;
    private readonly Action _onEdited;
    private bool _entryRejected;
    private string _rejection = string.Empty;
    private IReadOnlyList<ChoiceOption>? _choices;
    private bool _applyingChoice;
    private Action? _tableChanged;

    public PropertyRow(StatistleObject target, StatistleProperty property, UnitSelection units, Action onEdited)
    {
        _target = target;
        _property = property;
        _units = units;
        _onEdited = onEdited;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public StatistleObject Target => _target;

    public StatistleProperty Property => _property;

    public string Label => _property.Label;

    /// <summary>
    /// Width of the label column. Results laid out beside the entry values get a narrower one,
    /// so the two columns fit side by side; everything else shares the editor's standard width.
    /// </summary>
    public double LabelWidth =>
        _target is Analysis { ResultsBeside: true } ? (_property.IsResult ? 165 : 180) : 230;

    /// <summary>A numeric result laid out beside the entry values gets a shorter value box, so both columns fit.</summary>
    public bool IsCompact => _property.IsResult && !IsString && _target is Analysis { ResultsBeside: true };

    public bool IsEnum => PropertyType.IsEnum;

    /// <summary>Free text — a name, a description — which gets the full width of value plus unit.</summary>
    public bool IsString => PropertyType == typeof(string);

    /// <summary>
    /// True when the value is picked from a list the object supplies — a column of one of the
    /// project's data tables. The editor still knows nothing about it.
    /// </summary>
    public bool IsChoice => Choices.Count > 0;

    /// <summary>
    /// The options, asked for once and kept until <see cref="Refresh"/>: the combo box's
    /// ItemsSource and its SelectedItem must be the same instances, or WPF's selection
    /// bookkeeping and the binding chase each other on every change notification.
    /// </summary>
    public IReadOnlyList<ChoiceOption> Choices =>
        _choices ??= _target is IHasChoices source ? source.ChoicesFor(_property.XmlName) : [];

    /// <summary>The option currently selected, or null when the stored value matches none.</summary>
    public ChoiceOption? SelectedChoice
    {
        get
        {
            object? value = _property.Read(_target);

            return value is null ? null : Choices.FirstOrDefault(c => Equals(c.Value, value));
        }

        set
        {
            // Null arrives when the list is re-bound; a repeat of the stored value arrives when
            // the selection is re-applied. Neither is an edit, and treating either as one would
            // recompute, refresh, re-bind and land back here.
            if (value is null || IsReadOnly || _applyingChoice || Equals(_property.Read(_target), value.Value))
            {
                return;
            }

            _applyingChoice = true;

            try
            {
                _property.Property.SetValue(_target, value.Value);
                _onEdited();
            }
            finally
            {
                _applyingChoice = false;
            }

            Raise(nameof(SelectedChoice));
            Raise(nameof(Text));
            Raise(nameof(HelpText));
        }
    }

    public bool IsSeries => PropertyType == typeof(DataSeries);

    // ── A table of named columns, edited as text ─────────────────────────────

    /// <summary>
    /// True for a <see cref="ColumnTable"/>: edited as a multi-line box like a series, but with
    /// a header line of column names above the rows.
    /// </summary>
    public bool IsTable => PropertyType == typeof(ColumnTable);

    public ColumnTable? Table => _property.Read(_target) as ColumnTable;

    /// <summary>What the table box expects, shown under its label.</summary>
    public string TableHint =>
        IsReadOnly
            ? "Generated; not editable."
            : "First line: column names. Then one row per line, columns separated by tabs, commas or semicolons.";

    /// <summary>How many columns and rows the table holds, for the help panel.</summary>
    public string TableSummary =>
        Table is ColumnTable table && table.ColumnCount > 0
            ? $"{WorkspaceViewModel.Count(table.ColumnCount, "column", "columns")}, {WorkspaceViewModel.Count(table.RowCount, "row", "rows")}"
            : string.Empty;

    /// <summary>Replaces every column — what an import does.</summary>
    public void ImportColumns(IEnumerable<DataColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        if (Table is not ColumnTable table || IsReadOnly)
        {
            return;
        }

        table.SetColumns(columns);
        Edited();
    }

    /// <summary>Unhooks from the table before the row is discarded, so a stale row never recomputes.</summary>
    public void Detach()
    {
        if (Table is ColumnTable table && _tableChanged is not null)
        {
            table.Changed -= _tableChanged;
            _tableChanged = null;
        }
    }

    /// <summary>
    /// A series entered as one value per line — a sample against its observation number.
    /// Declared on the property, not decided here; paired data (a regression's x and y) stays
    /// two-column.
    /// </summary>
    public bool IsSingleColumnSeries => IsSeries && _property.IsSingleColumnSeries;

    /// <summary>What the series box expects, in terms of the step it is spaced at.</summary>
    public string SeriesHint
    {
        get
        {
            if (IsReadOnly)
            {
                return "Generated; not editable.";
            }

            if (!IsSingleColumnSeries)
            {
                return "One pair per line: x then y, separated by a tab, a comma or a semicolon.";
            }

            if (_property.StepProperty is null)
            {
                return "One value per line.";
            }

            StatistleProperty? step = _target.Properties().FirstOrDefault(p => p.XmlName == _property.StepProperty);
            double stepValue = _property.StepOf(_target);

            string shown = step is null
                ? stepValue.ToString("G4", CultureInfo.CurrentCulture)
                : $"{_units.ToDisplay(step.Unit, stepValue).ToString("G4", CultureInfo.CurrentCulture)} " +
                  _units.LabelFor(step.Unit);

            return $"One value per line. Each line is one {(step?.Label ?? "step").ToLowerInvariant()} " +
                   $"of {shown}; the first line is at time 0.";
        }
    }

    /// <summary>
    /// Id is identity, a Calculated or Output value is a result, and a value some method that
    /// currently applies writes (an analysis's sample, copied from a data table column) would
    /// only be overwritten — all shown, not typed into. Read off the role and the methods; the
    /// editor knows no variable names.
    /// </summary>
    public bool IsReadOnly =>
        _property.XmlName == nameof(StatistleObject.Id)
        || _property.IsResult
        || MethodScope.IsLocked(_target, _property.XmlName);

    /// <summary>
    /// A two-sided range, in the units currently shown, as "30 – 100". One-sided bounds are
    /// still enforced but not shown: "at least 0" under nearly every field said nothing.
    /// </summary>
    public string RangeHint => _property.CompactRangeText(_units);

    public bool HasRange => _property.HasMin && _property.HasMax;

    /// <summary>
    /// False when the current value falls outside the range declared on the property, or when
    /// the last thing typed could not be read as a value and the box still shows it.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (_entryRejected)
            {
                return false;
            }

            object? value = _property.Read(_target);

            // Blank (NaN) is "not entered yet", not a value out of range.
            return value is double d && double.IsNaN(d)
                ? true
                : value is double or int or float
                    ? _property.IsInRange(Convert.ToDouble(value, CultureInfo.InvariantCulture))
                    : true;
        }
    }

    public bool HasUnit => _property.Unit != ValueKind.None;

    /// <summary>The suffix shown after the value — "%" for a percentage, nothing otherwise.</summary>
    public string UnitLabel => HasUnit ? _units.LabelFor(_property.Unit) : string.Empty;

    /// <summary>
    /// The analysis that can fill this property, if one declared that it does. Found by asking
    /// the catalog, so a "Calculate…" link appears beside the property without the form naming
    /// the analysis — see <see cref="ProvidedValue"/>.
    /// </summary>
    public ValueProvider? Provider =>
        IsReadOnly
            ? null
            : Providers.GetOrAdd(
                (_target.GetType(), _property.XmlName),
                key => AnalysisCatalog.ProviderFor(key.Item1, key.Item2, typeof(Analysis).Assembly));

    public bool HasCalculator => Provider is not null;

    /// <summary>An editable series or table can be filled from a file: text, CSV, .xd2 or .xlsx.</summary>
    public bool CanImport => (IsSeries || IsTable) && !IsReadOnly;

    /// <summary>Replaces a single-column series with imported values (display units in, base units stored).</summary>
    public void ImportValues(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (!CanImport || _property.Read(_target) is not DataSeries existing)
        {
            return;
        }

        existing.SetValues([.. values.Select(v => _units.ToBase(_property.Unit, v))], _property.StepOf(_target));
        Edited();
    }

    /// <summary>Replaces a two-column series with imported (x, y) pairs.</summary>
    public void ImportPairs(IReadOnlyList<DataPoint> pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);

        if (!CanImport || _property.Read(_target) is not DataSeries existing)
        {
            return;
        }

        existing.Clear();
        foreach (DataPoint point in pairs)
        {
            existing.Add(point.X, point.Y);
        }

        Edited();
    }

    /// <summary>The object this property refers to, when the target says it refers to one.</summary>
    public StatistleObject? Link => (_target as ILinksTo)?.LinkedObject(_property.XmlName);

    public bool HasLink => Link is not null;

    public string LinkHint => Link is StatistleObject linked ? $"Go to {linked.Name}" : string.Empty;

    public string CalculatorHint =>
        Provider is null ? string.Empty : $"Open {Provider.Tool.Title} and use its result here";

    /// <summary>The enum's values as people read them — "Dimensionless Curve", not "DimensionlessCurve".</summary>
    public IReadOnlyList<string> EnumValues =>
        IsEnum ? EnumLabel.LabelsOf(PropertyType) : [];

    // ── Choosing among several methods' results (IChoosesMethod) ──────────────

    private IChoosesMethod? Chooser => _target as IChoosesMethod;

    private IReadOnlyList<ICalculationMethod> Methods =>
        Chooser is null ? [] : MethodCatalog.For(_target.GetType());

    /// <summary>The method whose output this row shows, when the target chooses among methods.</summary>
    private ICalculationMethod? WrittenBy =>
        Methods.FirstOrDefault(m => m.Outputs.Contains(_property.XmlName, StringComparer.Ordinal));

    /// <summary>True for a result the user can click to make it the one that counts.</summary>
    public bool IsSelectable => _property.IsResult && WrittenBy is not null;

    /// <summary>True for the result currently chosen; the others are shown greyed.</summary>
    public bool IsChosen =>
        IsSelectable
        && Chooser?.ChosenMethod()?.Outputs.Contains(_property.XmlName, StringComparer.Ordinal) == true;

    /// <summary>Greyed: a selectable result that is not the chosen one.</summary>
    public bool IsDimmed => IsSelectable && !IsChosen;

    /// <summary>The target chooses among methods, so entry rows carry a usage column.</summary>
    public bool ShowsUsageColumn => Chooser is not null && !_property.IsResult;

    /// <summary>True for an input some method of the target reads — it gets a usage mark.</summary>
    public bool HasUsageMark =>
        ShowsUsageColumn && Methods.Any(m => m.Inputs.Contains(_property.XmlName, StringComparer.Ordinal));

    /// <summary>True when the chosen method reads this input — the mark is ticked.</summary>
    public bool IsUsedByChosen =>
        HasUsageMark
        && Chooser?.ChosenMethod()?.Inputs.Contains(_property.XmlName, StringComparer.Ordinal) == true;

    /// <summary>Makes this result the chosen one. No-op for anything that is not selectable.</summary>
    public void Choose()
    {
        if (Chooser is null || WrittenBy is not ICalculationMethod method || IsChosen)
        {
            return;
        }

        Chooser.Choose(method);
        _onEdited();
    }

    // ── Help ──────────────────────────────────────────────────────────────────

    public string HelpTitle =>
        HasUnit ? $"{Label} ({UnitLabel})" : Label;

    /// <summary>
    /// What the help panel says for this field: the property's own text, or a description built
    /// from its metadata, followed by the facts the metadata carries — role, range, the method
    /// that writes it, the option chosen, the analysis that can fill it.
    /// </summary>
    public string HelpText
    {
        get
        {
            var text = new StringBuilder();

            if (RejectionMessage.Length > 0)
            {
                text.Append(RejectionMessage).Append("\n\n");
            }

            text.Append(_property.HasHelp ? _property.Help : FallbackHelp());

            if (WrittenBy is ICalculationMethod method)
            {
                text.Append("\n\n").Append(method.Name).Append(": ").Append(method.Description)
                    .Append("\nSource: ").Append(method.Citation);
            }
            else if (Methods.Count > 0 && HasUsageMark)
            {
                string users = string.Join(
                    ", ", Methods.Where(m => m.Inputs.Contains(_property.XmlName, StringComparer.Ordinal)).Select(m => m.Name));
                text.Append("\n\nUsed by: ").Append(users).Append('.');
            }

            if (IsChoice && SelectedChoice is { Detail.Length: > 0 } chosen)
            {
                text.Append("\n\n").Append(chosen.Label).Append(": ").Append(chosen.Detail);
            }

            if (IsTable && TableSummary.Length > 0)
            {
                text.Append("\n\nThis table holds ").Append(TableSummary).Append('.');
            }

            List<string> facts = [];

            if (_property.HasRange)
            {
                facts.Add($"Range: {_property.RangeText(_units)}.");
            }

            facts.Add(_property.Role switch
            {
                VariableRole.Input => "A given value: measured, mapped or taken from a reference.",
                VariableRole.Design => "A design choice: yours to set and vary.",
                VariableRole.Calculated => "Computed by Statistle; shown, not entered.",
                VariableRole.Output => "A result computed by Statistle.",
                _ => string.Empty,
            });

            if (MethodScope.IsLocked(_target, _property.XmlName) && !_property.IsResult)
            {
                facts.Add("Generated by the selected method, so it cannot be edited here.");
            }

            if (HasCalculator)
            {
                facts.Add($"Click Calculate… to fill it from the {Provider!.Tool.Title} analysis.");
            }

            string factLine = string.Join(" ", facts.Where(f => f.Length > 0));
            if (factLine.Length > 0)
            {
                text.Append("\n\n").Append(factLine);
            }

            return text.ToString();
        }
    }

    private string FallbackHelp()
    {
        if (_property.XmlName == nameof(StatistleObject.Name))
        {
            return "The name this item is listed under in the project tree and in reports. Make it descriptive: what was measured, and on what.";
        }

        if (_property.XmlName == nameof(StatistleObject.Description))
        {
            return "Free text for your own notes about this item — where the values came from, how they were measured, which assignment they belong to. Carried into the report.";
        }

        if (_property.XmlName == nameof(StatistleObject.Id))
        {
            return "The item's number within the project. Assigned automatically.";
        }

        if (IsTable)
        {
            return $"{Label}: columns of numbers under their names. First line the names, then one row per line.";
        }

        return HasUnit
            ? $"{Label}, in {UnitLabel}."
            : $"{Label}.";
    }

    private Type PropertyType =>
        Nullable.GetUnderlyingType(_property.Property.PropertyType) ?? _property.Property.PropertyType;

    /// <summary>
    /// The value as shown and as typed. Reading converts to display units and applies the
    /// declared decimals; writing converts back to base units before it reaches the domain.
    /// </summary>
    public string Text
    {
        get
        {
            object? value = _property.Read(_target);

            return value switch
            {
                ColumnTable table => table.Save().Replace("\n", Environment.NewLine, StringComparison.Ordinal),
                DataSeries series when IsSingleColumnSeries => FormatColumn(series),
                DataSeries series => series.ToString(),
                double number when double.IsNaN(number) => IsReadOnly ? "—" : string.Empty,
                double number => _units
                    .ToDisplay(_property.Unit, number)
                    .ToString($"F{Math.Max(_property.Decimals, 0)}", CultureInfo.CurrentCulture),
                Enum choice => EnumLabel.Of(choice),
                null => string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
        }

        set
        {
            if (IsReadOnly)
            {
                return;
            }

            try
            {
                Type type = PropertyType;
                object? parsed;

                if (type == typeof(DataSeries))
                {
                    if (!TrySetSeries(value, out string? problem))
                    {
                        Reject($"{problem} The paste was not used; your earlier data is kept. " +
                               "Correct that line, or press Esc to go back to the data in use.");
                        return;
                    }

                    Edited();
                    return;
                }

                if (type == typeof(ColumnTable))
                {
                    // A cell that is neither blank nor a number rejects the whole paste; the old
                    // columns stay, and the box is flagged so the typo is visible.
                    if (_property.Read(_target) is not ColumnTable table)
                    {
                        return;
                    }

                    if (!table.TryLoad(value, CultureInfo.CurrentCulture, out string? problem))
                    {
                        Reject($"{problem} The paste was not used; your earlier data is kept. " +
                               "Correct that cell, or press Esc to go back to the data in use.");
                        return;
                    }

                    Edited();
                    return;
                }

                if (type == typeof(string))
                {
                    parsed = value;
                }
                else if (type == typeof(double))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        // Clearing a box leaves the value blank; results that need it go blank too.
                        parsed = double.NaN;
                    }
                    else if (!double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out double entered))
                    {
                        Reject(NotANumber(value, "a number"));
                        return;
                    }
                    else
                    {
                        parsed = _units.ToBase(_property.Unit, entered);
                    }
                }
                else if (type == typeof(int))
                {
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out int entered))
                    {
                        Reject(NotANumber(value, "a whole number"));
                        return;
                    }

                    parsed = entered;
                }
                else if (type.IsEnum)
                {
                    if (!EnumLabel.TryParse(type, value, out object? entered))
                    {
                        Reject($"'{value}' is not one of the choices, so Statistle is still using {ShownValue}. Pick one from the list.");
                        return;
                    }

                    parsed = entered;
                }
                else if (type == typeof(bool))
                {
                    if (!bool.TryParse(value, out bool entered))
                    {
                        Reject($"'{value}' is not True or False, so Statistle is still using {ShownValue}. Correct it or press Esc.");
                        return;
                    }

                    parsed = entered;
                }
                else
                {
                    return;
                }

                // The same value coming back — a combo box re-applying its selection, a box
                // left as it was — is not an edit; nothing is recomputed or marked dirty.
                if (Equals(parsed, _property.Read(_target)))
                {
                    ClearRejection();
                    return;
                }

                _property.Property.SetValue(_target, parsed);
                Edited();
            }
            catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
            {
                // A half-typed value is not an error; the row simply keeps what it had.
                Reject($"'{value}' could not be used here (it may be too large or in the wrong form), so Statistle is still using {ShownValue}. Correct it or press Esc.");
            }
        }
    }

    private void Edited()
    {
        _entryRejected = false;
        _rejection = string.Empty;
        Raise(nameof(RejectionMessage));
        Raise(nameof(HasRejection));
        _onEdited();
        Raise(nameof(Text));
        Raise(nameof(IsValid));
        Raise(nameof(HelpText));
    }

    public void Refresh()
    {
        // Re-rendering replaces whatever was typed with the stored value, so a rejected entry
        // is no longer on screen to be flagged.
        _entryRejected = false;
        _rejection = string.Empty;
        _choices = null;

        Raise(nameof(RejectionMessage));
        Raise(nameof(HasRejection));

        Raise(nameof(Choices));
        Raise(nameof(SelectedChoice));
        Raise(nameof(Text));
        Raise(nameof(UnitLabel));
        Raise(nameof(RangeHint));
        Raise(nameof(SeriesHint));
        Raise(nameof(IsValid));
        Raise(nameof(IsReadOnly));
        Raise(nameof(IsChosen));
        Raise(nameof(IsDimmed));
        Raise(nameof(IsUsedByChosen));
        Raise(nameof(HelpTitle));
        Raise(nameof(HelpText));
        Raise(nameof(TableHint));
        Raise(nameof(TableSummary));
        Raise(nameof(HasLink));
        Raise(nameof(LinkHint));
    }

    /// <summary>
    /// A single-column series shows only its values, one per line, in display units. Enough digits
    /// are kept that re-saving an unedited line does not round the stored value.
    /// </summary>
    private string FormatColumn(DataSeries series)
    {
        string format = "0." + new string('#', Math.Max(_property.Decimals + 2, 4));

        return string.Join(
            Environment.NewLine,
            series.YValues.Select(y => _units.ToDisplay(_property.Unit, y).ToString(format, CultureInfo.CurrentCulture)));
    }

    private bool TrySetSeries(string text, out string? problem)
    {
        problem = null;

        if (_property.Read(_target) is not DataSeries existing)
        {
            return false;
        }

        if (IsSingleColumnSeries)
        {
            if (!DataSeries.TryParseColumn(text, CultureInfo.CurrentCulture, out double[] values, out problem))
            {
                return false;
            }

            existing.SetValues(
                [.. values.Select(v => _units.ToBase(_property.Unit, v))],
                _property.StepOf(_target));

            return true;
        }

        // Pairs: a line that is not two numbers rejects the paste (red box, old data kept),
        // the same rule as the single-column box.
        if (!DataSeries.TryParsePairs(text, CultureInfo.CurrentCulture, out DataSeries parsed, out problem))
        {
            return false;
        }

        existing.Clear();
        foreach (DataPoint point in parsed.Points)
        {
            existing.Add(point.X, point.Y);
        }

        return true;
    }

    /// <summary>
    /// Why the last thing typed was not used, and what is used meanwhile — shown under the box
    /// and at the top of the help panel; empty when the entry was accepted (UX review 2026-10-31).
    /// </summary>
    public string RejectionMessage => _rejection;

    public bool HasRejection => _rejection.Length > 0;

    /// <summary>The value in use, as the person reads it, for a rejection message.</summary>
    private string ShownValue => Text is { Length: > 0 } shown ? shown : "no value (the box was blank)";

    private string NotANumber(string? typed, string what) =>
        $"'{typed}' is not {what}, so Statistle is still using {ShownValue}. Correct it or press Esc.";

    /// <summary>Keeps the stored value, flags the box, and says why, so the typo is visible rather than silent.</summary>
    private void Reject(string message)
    {
        _entryRejected = true;
        _rejection = message;
        Raise(nameof(IsValid));
        Raise(nameof(RejectionMessage));
        Raise(nameof(HasRejection));
        Raise(nameof(HelpText));
    }

    private void ClearRejection()
    {
        _entryRejected = false;
        _rejection = string.Empty;
        Raise(nameof(IsValid));
        Raise(nameof(RejectionMessage));
        Raise(nameof(HasRejection));
        Raise(nameof(HelpText));
    }

    /// <summary>Esc in a box: forget what was typed and show the value in use again.</summary>
    public void CancelEntry()
    {
        ClearRejection();
        Raise(nameof(Text));
    }

    private void Raise([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
