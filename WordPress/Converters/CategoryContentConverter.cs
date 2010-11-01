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

            string selectCategories = _localizedStrings.ControlsText.SelectCategories;

            IEnumerable<string> categories = value as IEnumerable<string>;
            if (0 == categories.Count())
            {
                return selectCategories;
            }
            
            StringBuilder builder = new StringBuilder();
            builder.Append(selectCategories);
            builder.Append(": \r\n");

            string separator = ", ";
            categories.OrderBy(category => category).ToList().ForEach(category =>
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
