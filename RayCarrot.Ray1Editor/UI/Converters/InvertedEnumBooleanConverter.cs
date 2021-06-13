using System;
using System.Globalization;

namespace RayCarrot.Ray1Editor
{
    public class InvertedEnumBooleanConverter : BaseValueConverter<InvertedEnumBooleanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object parameterValue = Enum.Parse(value.GetType(), (string)parameter);

            return !parameterValue.Equals(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, (string)parameter);
        }
    }
}