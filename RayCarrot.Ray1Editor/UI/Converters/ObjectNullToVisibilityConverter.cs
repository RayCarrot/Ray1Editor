using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.Ray1Editor
{
    public class ObjectNullToVisibilityConverter : BaseValueConverter<ObjectNullToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
            value == null ? Visibility.Visible : Visibility.Collapsed;
    }
}