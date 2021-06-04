using System;
using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
    public class AppViewToContentConverter : BaseValueConverter<AppViewToContentConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vm = AppViewModel.Instance.CurrentAppViewViewModel;

            UserControl view = (AppViewModel.AppView)value switch
            {
                AppViewModel.AppView.Editor => new EditorView((EditorViewModel)vm),
                AppViewModel.AppView.LoadMap => new LoadMapView(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };

            view.DataContext = vm;

            return view;
        }
    }
}