using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace KCY_Accounting.UI.Converters;

/// <summary>
/// Returns true when the binding value (string) equals the ConverterParameter.
/// Used by the sidebar to highlight the active navigation button.
/// </summary>
public class StringEqualConverter : IValueConverter
{
    public static readonly StringEqualConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && s == parameter as string;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

