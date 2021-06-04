using System;
using System.Globalization;

namespace RayCarrot.Ray1Editor
{
    public class GameModeToDisplayNameConverter : BaseValueConverter<GameModeToDisplayNameConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((GameMode)value).GetAttribute<GameModeInfoAttribute>().DisplayName;
        }
    }
}