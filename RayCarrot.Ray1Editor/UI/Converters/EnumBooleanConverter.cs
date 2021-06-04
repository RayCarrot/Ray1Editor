using System;
using System.Globalization;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Converts an <see cref="Enum"/> to a <see cref="Boolean"/> which is true if the value equals the parameter value.
    /// </summary>
    public class EnumBooleanConverter : BaseValueConverter<EnumBooleanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object parameterValue = Enum.Parse(value.GetType(), (string)parameter);

            return parameterValue.Equals(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, (string)parameter);
        }
    }
}