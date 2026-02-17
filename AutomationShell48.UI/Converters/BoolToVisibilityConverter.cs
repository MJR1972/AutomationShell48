using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutomationShell48.UI.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
            var state = value is bool b && b;
            if (invert)
            {
                state = !state;
            }

            return state ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = value is Visibility visibility && visibility == Visibility.Visible;
            var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
            return invert ? !visible : visible;
        }
    }
}
