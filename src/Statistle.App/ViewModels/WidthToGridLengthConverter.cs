using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Statistle.App.ViewModels;

/// <summary>A pixel width from a row (its label column) as a grid column length.</summary>
public sealed class WidthToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double width ? new GridLength(width) : new GridLength(230);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
