using System;
using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.Ray1Editor;

public class ColorToBrushConverter : BaseValueConverter<ColorToBrushConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new SolidColorBrush((Color)value);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((SolidColorBrush)value).Color;
    }
}