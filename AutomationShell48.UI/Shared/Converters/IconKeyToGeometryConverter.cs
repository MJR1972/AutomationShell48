using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutomationShell48.UI.Shared.Converters
{
    public class IconKeyToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = value as string;
            if (string.IsNullOrWhiteSpace(key) || Application.Current == null)
            {
                return Geometry.Empty;
            }

            return Application.Current.TryFindResource(key) as Geometry ?? Geometry.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
