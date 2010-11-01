using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;
using System.Collections.ObjectModel;

namespace WordPress.Converters
{
    /// <summary>
    /// Converts an ObservableCollection of strings into a string representation
    /// </summary>
    public class CategoryContentConverter: IValueConverter
    {
        #region member variables

        StringTable _localizedStrings;

        #endregion

        #region constructors

        public CategoryContentConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value.GetType().IsAssignableFrom(typeof(ObservableCollection<string>))))
            {
                throw new ArgumentException("Value cannot be assigned as ObservableCollection<string>", "value");
            }

            List<string> categories = ((ObservableCollection<string>)value).OrderBy(category => category).ToList();
            if (0 == categories.Count())
            {
                return _localizedStrings.ControlsText.SelectCategories;
            }
            
            StringBuilder builder = new StringBuilder();
            builder.Append(_localizedStrings.ControlsText.SelectedCategories);
            builder.Append(": \r\n");

            string separator = ", ";            
            categories.ForEach(category =>
            {
                builder.Append(category);
                if (categories.Last() != category)
                {
                    builder.Append(separator);
                }
            });

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
