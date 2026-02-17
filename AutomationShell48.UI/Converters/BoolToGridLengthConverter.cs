using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutomationShell48.UI.Converters
{
    public class BoolToGridLengthConverter : IValueConverter
    {
        public GridLength ExpandedWidth { get; set; } = new GridLength(250);
        public GridLength CollapsedWidth { get; set; } = new GridLength(72);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isCollapsed = value is bool b && b;
            return isCollapsed ? CollapsedWidth : ExpandedWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength length)
            {
                return Math.Abs(length.Value - CollapsedWidth.Value) < 0.1;
            }

            return false;
        }
    }
}
