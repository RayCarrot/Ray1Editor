using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.Ray1Editor;

public class InvertedObjectNullToHiddenVisibilityConverter : BaseValueConverter<InvertedObjectNullToHiddenVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        value == null ? Visibility.Hidden : Visibility.Visible;
}