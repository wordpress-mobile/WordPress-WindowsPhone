using System;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress.Converters
{
    public class ItemTitleInListConverter: IValueConverter
    {
        #region member variables

        StringTable _localizedStrings;

        #endregion

        #region constructors

        public ItemTitleInListConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return _localizedStrings.ControlsText.NoTitle;

            if (!(value is string))
            {
                throw new ArgumentException("value is not a string", "title");
            }

            string postTile = value as string;
            if (postTile.Equals(""))
                return _localizedStrings.ControlsText.NoTitle;
            else return postTile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
