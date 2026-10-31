using System.Globalization;
using System.Windows.Data;

namespace Statistle.App.ViewModels;

/// <summary>True when the two bound values are equal — the rail's button for the analysis open now.</summary>
public sealed class SameValueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Length == 2 && values[0] is not null && Equals(values[0], values[1]);

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
