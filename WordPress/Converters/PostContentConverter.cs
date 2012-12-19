using System;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress.Converters
{
    public class PostContentConverter: IValueConverter
    {
        #region member variables

        StringTable _localizedStrings;

        #endregion

        #region constructors

        public PostContentConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return string.Empty;

            if (!(value is String))
            {
                throw new ArgumentException("value is not a string", "value");
            }

            String stringValue = (string)value;
            if (stringValue.Length > 150)
                return stringValue.Substring(0, 147) + "...\n...";

            else
                return stringValue;// +"\n" + _localizedStrings.ControlsText.TapToEdit; ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
