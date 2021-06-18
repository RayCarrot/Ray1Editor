using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
    public class EditorFieldTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                EditorIntFieldViewModel _ => (DataTemplate)Application.Current.FindResource(App.EditorIntFieldTemplateKey),
                EditorBoolFieldViewModel _ => (DataTemplate)Application.Current.FindResource(App.EditorBoolFieldTemplateKey),
                EditorDropDownFieldViewModel _ => (DataTemplate)Application.Current.FindResource(App.EditorDropDownFieldTemplateKey),
                EditorPointFieldViewModel _ => (DataTemplate)Application.Current.FindResource(App.EditorPointFieldTemplateKey),
                _ => null
            };
        }
    }
}