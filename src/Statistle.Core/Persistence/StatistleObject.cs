using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Statistle.Core.Units;

namespace Statistle.Core.Persistence;

/// <summary>One persisted property, with everything the UI and the serializer need.</summary>
public sealed record StatistleProperty(
    PropertyInfo Property,
    string XmlName,
    string Label,
    int Decimals,
    ValueKind Unit,
    string? Alias,
    string ReportLabel,
    VariableRole Role,
    double Min,
    double Max,
    string? StepProperty = null,
    bool ValuesOnly = false,
    string Help = "",
    bool Hidden = false)
{
    public object? Read(object target) => Property.GetValue(target);

    /// <summary>
    /// True for a series entered as one value per line rather than as (x, y) pairs: x is the
    /// row index, times the sibling step when <see cref="StepProperty"/> names one. See
    /// <see cref="StatistlePropertyAttribute.StepProperty"/> and <see cref="StatistlePropertyAttribute.ValuesOnly"/>.
    /// </summary>
    public bool IsSingleColumnSeries => ValuesOnly || StepProperty is not null;

    /// <summary>The step a single-column series is spaced at: the sibling property, or 1 (the row index).</summary>
    public double StepOf(StatistleObject target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (StepProperty is null)
        {
            return 1;
        }

        StatistleProperty? step = target.Properties().FirstOrDefault(p => p.XmlName == StepProperty);

        return step?.Read(target) is double value && value > 0 ? value : 1;
    }

    /// <summary>True when the property declares its own help text rather than relying on the fallback.</summary>
    public bool HasHelp => Help.Length > 0;

    public bool HasMin => !double.IsNaN(Min);

    public bool HasMax => !double.IsNaN(Max);

    public bool HasRange => HasMin || HasMax;

    /// <summary>A result is shown, not typed into.</summary>
    public bool IsResult => Role is VariableRole.Calculated or VariableRole.Output;

    /// <summary>Checks a value in base units. Bounds are inclusive.</summary>
    public bool IsInRange(double baseValue) =>
        (!HasMin || baseValue >= Min) && (!HasMax || baseValue <= Max);

    /// <summary>
    /// Why a value is out of range, in the units it is being shown in, or null if it is fine.
    /// </summary>
    public string? RangeViolation(double baseValue, UnitSelection? units = null)
    {
        if (IsInRange(baseValue))
        {
            return null;
        }

        return $"{Label} must be {RangeText(units)}.";
    }

    /// <summary>
    /// A two-sided range as "30 – 100" in display units, for showing beside a label. Empty when
    /// only one bound is declared: "at least 0" on nearly every field is noise, not information.
    /// </summary>
    public string CompactRangeText(UnitSelection? units = null) =>
        HasMin && HasMax ? $"{ShowBound(Min, units)} – {ShowBound(Max, units)}" : string.Empty;

    /// <summary>The declared range rendered in display units, e.g. "between 30 and 100".</summary>
    public string RangeText(UnitSelection? units = null)
    {
        string Show(double baseValue) => ShowBound(baseValue, units);

        return (HasMin, HasMax) switch
        {
            (true, true) => $"between {Show(Min)} and {Show(Max)}",
            (true, false) => $"at least {Show(Min)}",
            (false, true) => $"at most {Show(Max)}",
            _ => string.Empty,
        };
    }

    private string ShowBound(double baseValue, UnitSelection? units)
    {
        double shown = units is null ? baseValue : units.ToDisplay(Unit, baseValue);
        return shown.ToString($"G{Math.Max(Decimals, 1) + 3}", CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// The backbone, carried over from Statistle 10's <c>XmlPropertyObject</c>: adding a public
/// property is the entire cost of persisting it, and there are no serialization attributes to
/// maintain. What is new here is change notification (so the UI can bind instead of
/// hand-marshalling values in and out of controls), property aliases, and reads that report
/// what they could not parse.
/// </summary>
public abstract class StatistleObject : INotifyPropertyChanged
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<StatistleProperty>> Cache = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = string.Empty;
    private string _description = string.Empty;

    /// <summary>Identity within its collection; also the artifact's file name inside the document.</summary>
    [StatistleProperty(Label = "Id", Decimals = 0)]
    public int Id { get; set; }

    /// <summary>
    /// Notifies, so renaming an artifact in the editor reaches the project tree without anything
    /// having to push it there.
    /// </summary>
    [StatistleProperty(Label = "Name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [StatistleProperty(Label = "Description")]
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>The persisted properties of a type, discovered once and cached.</summary>
    public static IReadOnlyList<StatistleProperty> PropertiesOf(Type type) =>
        Cache.GetOrAdd(type, Discover);

    public IReadOnlyList<StatistleProperty> Properties() => PropertiesOf(GetType());

    /// <summary>The type's overall help, from its <see cref="StatistleHelpAttribute"/>; empty if none.</summary>
    public static string HelpOf(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetCustomAttribute<StatistleHelpAttribute>()?.Text ?? string.Empty;
    }

    /// <summary>Formats a property for display: its decimals, and its unit if one is selected.</summary>
    public string Display(StatistleProperty property, UnitSelection? units = null)
    {
        object? value = property.Read(this);

        if (value is double number)
        {
            if (double.IsNaN(number))
            {
                return "—";
            }

            double shown = units is null
                ? number
                : units.ToDisplay(property.Unit, number);

            return shown.ToString($"F{Math.Max(property.Decimals, 0)}", CultureInfo.CurrentCulture);
        }

        return value switch
        {
            Enum choice => EnumLabel.Of(choice),
            ITextPersisted table => table.Save(),
            null => string.Empty,
            _ => value.ToString() ?? string.Empty,
        };
    }

    /// <summary>
    /// Every declared range this object currently violates. One implementation covers every
    /// property that declares a range — no form writes a bounds check of its own.
    /// </summary>
    public IReadOnlyList<string> Validate(UnitSelection? units = null)
    {
        List<string> violations = [];

        foreach (StatistleProperty property in Properties())
        {
            if (!property.HasRange)
            {
                continue;
            }

            object? raw = property.Read(this);
            if (raw is not (double or int or float))
            {
                continue;
            }

            double value = Convert.ToDouble(raw, CultureInfo.InvariantCulture);

            if (property.RangeViolation(value, units) is string violation)
            {
                violations.Add(violation);
            }
        }

        return violations;
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static IReadOnlyList<StatistleProperty> Discover(Type type)
    {
        List<StatistleProperty> found = [];

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (property.GetCustomAttribute<StatistleIgnoreAttribute>() is not null)
            {
                continue;
            }

            var attribute = property.GetCustomAttribute<StatistlePropertyAttribute>();

            // A DataSeries or a nested StatistleObject is mutated in place, so it needs no setter.
            // A declared result (Calculated / Output) may be a derived getter with no setter: it
            // is written to the file for a reader's benefit and recomputed rather than read
            // back. Everything else needs a setter to persist.
            bool mutatedInPlace = typeof(DataSeries).IsAssignableFrom(property.PropertyType)
                                  || typeof(StatistleObject).IsAssignableFrom(property.PropertyType)
                                  || typeof(ITextPersisted).IsAssignableFrom(property.PropertyType);
            bool derivedResult = attribute?.Role is VariableRole.Calculated or VariableRole.Output;
            if (!property.CanRead || !(property.CanWrite || mutatedInPlace || derivedResult))
            {
                continue;
            }

            string label = attribute?.Label ?? property.Name;

            found.Add(new StatistleProperty(
                property,
                property.Name,
                label,
                attribute?.Decimals ?? 3,
                attribute?.Unit ?? ValueKind.None,
                attribute?.Alias,
                attribute?.ReportLabel ?? label,
                attribute?.Role ?? VariableRole.Unspecified,
                attribute?.Min ?? double.NaN,
                attribute?.Max ?? double.NaN,
                attribute?.StepProperty,
                attribute?.ValuesOnly ?? false,
                attribute?.Help ?? string.Empty,
                attribute?.Hidden ?? false));
        }

        // Reflection returns a type's own properties before those it inherits, which would put
        // Name and Description at the bottom of every editor and report. Identity leads, then
        // each base class before the class that derives from it — the shared shape first,
        // then what the concrete type adds. Declaration order is kept within a class.
        static int Depth(Type? type)
        {
            int depth = 0;
            for (Type? t = type; t is not null; t = t.BaseType)
            {
                depth++;
            }

            return depth;
        }

        return [.. found.OrderBy(p => Depth(p.Property.DeclaringType))];
    }
}

/// <summary>Reflection-driven XML for any <see cref="StatistleObject"/>.</summary>
public static class StatistleXml
{
    public static XElement Write(StatistleObject source, string elementName)
    {
        ArgumentNullException.ThrowIfNull(source);

        var element = new XElement(elementName);

        foreach (StatistleProperty property in source.Properties())
        {
            object? value = property.Read(source);

            // A nested StatistleObject becomes a child element - the unit selection inside a
            // project, for instance.
            if (value is StatistleObject nested)
            {
                element.Add(Write(nested, property.XmlName));
                continue;
            }

            element.Add(new XElement(property.XmlName, Format(value)));
        }

        return element;
    }

    /// <summary>
    /// Reads into <paramref name="target"/>. Missing elements keep their defaults, unknown
    /// elements are ignored, and unparseable values leave the default and raise a warning.
    /// </summary>
    public static void Read(StatistleObject target, XElement element, LoadDiagnostics? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(element);

        foreach (StatistleProperty property in target.Properties())
        {
            XElement? source = element.Element(property.XmlName);

            // Fall back to the previous name so a rename does not orphan saved data.
            if (source is null && property.Alias is not null)
            {
                source = element.Element(property.Alias);
            }

            if (source is null)
            {
                continue;
            }

            if (property.Read(target) is StatistleObject nested)
            {
                Read(nested, source, diagnostics);
                continue;
            }

            if (property.Read(target) is ITextPersisted text)
            {
                text.Load(source.Value);
                continue;
            }

            // A derived result has no setter; it is recomputed from what is read, not read.
            if (!property.Property.CanWrite && !typeof(DataSeries).IsAssignableFrom(property.Property.PropertyType))
            {
                continue;
            }

            Assign(target, property, source.Value, diagnostics);
        }
    }

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        ITextPersisted text => text.Save(),

        // Storage is full precision. Decimals are a display concern - Statistle 10 used them for
        // both, so saving a project quietly rounded its own data.
        double number => number.ToString("R", CultureInfo.InvariantCulture),
        float number => number.ToString("R", CultureInfo.InvariantCulture),
        bool flag => flag ? "true" : "false",
        DateTime moment => moment.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static void Assign(
        StatistleObject target, StatistleProperty property, string raw, LoadDiagnostics? diagnostics)
    {
        Type type = Nullable.GetUnderlyingType(property.Property.PropertyType)
                    ?? property.Property.PropertyType;

        string where = $"{target.GetType().Name}.{property.XmlName}";

        try
        {
            if (type == typeof(DataSeries))
            {
                DataSeries parsed = DataSeries.Parse(raw, where, diagnostics);
                if (property.Read(target) is DataSeries existing)
                {
                    existing.Clear();
                    foreach (DataPoint point in parsed.Points)
                    {
                        existing.Add(point.X, point.Y);
                    }
                }
                else
                {
                    property.Property.SetValue(target, parsed);
                }

                return;
            }

            if (type == typeof(string))
            {
                property.Property.SetValue(target, raw);
                return;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                // An empty element means "not set" - leave the default rather than write a zero.
                return;
            }

            object converted = type switch
            {
                _ when type == typeof(double) =>
                    double.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture),
                _ when type == typeof(float) =>
                    float.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture),
                _ when type == typeof(int) =>
                    int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture),
                _ when type == typeof(long) =>
                    long.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture),
                _ when type == typeof(bool) => ParseBool(raw),
                _ when type == typeof(DateTime) =>
                    DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                _ when type.IsEnum => Enum.Parse(type, raw, ignoreCase: true),
                _ => Convert.ChangeType(raw, type, CultureInfo.InvariantCulture),
            };

            property.Property.SetValue(target, converted);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
                                       or ArgumentException or InvalidCastException)
        {
            diagnostics?.Add(where, raw, $"not a valid {type.Name}; kept the default");
        }
    }

    private static bool ParseBool(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "true" or "1" or "yes" => true,
            "false" or "0" or "no" => false,
            _ => bool.Parse(raw),
        };
}
