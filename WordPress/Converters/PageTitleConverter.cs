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
using WordPress.Model;

namespace WordPress.Converters
{
    public class PageTitleConverter:IValueConverter
    {
        #region member variables

        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public PageTitleConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return string.Empty;

            if (!(value is Post))
            {
                throw new ArgumentException("value is not Post", "value");
            }

            Post page = value as Post;
            if (page.IsNew)
            {
                return _localizedStrings.PageTitles.NewPage;
            }
            else
            {
                return _localizedStrings.PageTitles.EditPage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
