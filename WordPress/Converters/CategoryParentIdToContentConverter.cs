using System;
using System.Linq;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress.Converters
{
    /// <summary>
    /// Searches the current Blog object for a "parent" Category object, and if found returns
    /// a string representation of the parent.
    /// </summary>
    public class CategoryParentIdToContentConverter: IValueConverter
    {
        #region member variables

        private StringTable _localizedStrings;

        #endregion

        public CategoryParentIdToContentConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is int))
            {
                throw new ArgumentException("Value is not an int", "value");
            }
                        
            int parentId = (int)value;

            if (0 >= parentId)
            {
                return _localizedStrings.ControlsText.None;
            }

            Category parent = App.MasterViewModel.CurrentBlog.Categories.Single(c => c.CategoryId == parentId);
            if (null == parent)
            {
                return _localizedStrings.ControlsText.None;
            }
            else
            {
                return parent.CategoryName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
