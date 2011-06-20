using System;
using System.Windows;
using System.Windows.Data;

namespace WordPress.Converters
{
    /// <summary>
    /// This class is used to convert a bool to a System.Windows.Visibility value; true will return a Visible value
    /// whereas false will return a Collapsed value.
    /// </summary>
    public class BooleanToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibleState = Visibility.Collapsed;
            if (value is bool)
            {
                if ((bool)value)
                {
                    visibleState = Visibility.Visible;
                }
            }
            return visibleState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
