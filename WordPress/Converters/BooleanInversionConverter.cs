using System;
using System.Windows.Data;

namespace WordPress.Converters
{
    /// <summary>
    /// This class is used to help databind checkbox and radio button controls to the opposite of a boolean value.
    /// 
    /// </summary>
    public class BooleanInversionConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ArgumentException("Value must be a boolean");
            }
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ArgumentException("Value must be a boolean");
            }
            return !((bool)value);
        }
    }
}
