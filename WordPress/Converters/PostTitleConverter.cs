using System;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress.Converters
{
    public class PostTitleConverter: IValueConverter
    {
        #region member variables

        StringTable _localizedStrings;

        #endregion

        #region constructors

        public PostTitleConverter()
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

            Post post = value as Post;
            if (post.IsNew)
            {
                return _localizedStrings.PageTitles.NewPost;
            }
            else
            {
                return _localizedStrings.PageTitles.EditPost;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
