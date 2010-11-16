using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

using WordPress.Localization;

namespace WordPress.Converters
{
    public class ThumbnailSizeToStringConverter: IValueConverter
    {
        #region member variables

        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public ThumbnailSizeToStringConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is int))
            {
                throw new ArgumentException("value is not int", "value");
            }

            int width = (int)value;
            if (0 == width)
            {
                return _localizedStrings.ControlsText.OriginalSizeAbbr;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
