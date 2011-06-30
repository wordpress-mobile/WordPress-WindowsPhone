using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WordPress.Converters
{
    public class NegativeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(double))
            {
                return -System.Convert.ToDouble(value);
            }
            else if (targetType == typeof(Thickness))
            {
                double doubleValue = -System.Convert.ToDouble(value);
                switch ((string)parameter)
                {
                    case "Left": return new Thickness(doubleValue, 0, 0, 0);
                    case "Top": return new Thickness(0, doubleValue, 0, 0);
                    case "Right": return new Thickness(0, 0, doubleValue, 0);
                    case "Bottom": return new Thickness(0, 0, 0, doubleValue);
                    default: return new Thickness(doubleValue);
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}