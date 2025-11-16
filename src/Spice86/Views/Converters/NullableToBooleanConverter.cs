namespace Spice86.Views.Converters;

using Avalonia.Data;
using Avalonia.Data.Converters;

using System.Globalization;

/// <summary>
/// Represents the NullableToBooleanConverter class.
/// </summary>
public class NullableToBooleanConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is not null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingOperations.DoNothing;
    }
}