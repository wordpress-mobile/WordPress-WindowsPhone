using System;
using System.Windows.Data;

namespace WordPress.Converters
{
    public class DateToShortDateStringConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("value is not DateTime", "value");
            }

            DateTime date = (DateTime)value;
            return date.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
