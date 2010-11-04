using System;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress.Converters
{
    /// <summary>
    /// Converts the given eStatisticType value to and from a localized string.
    /// </summary>
    public class StatisticTypeToStringConverter:IValueConverter
    {
        #region member variables

        StringTable _localizedStrings;

        #endregion


        #region constructors

        public StatisticTypeToStringConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is eStatisticType))
            {
                throw new ArgumentException("Value is not eStatisticType", "value");
            }
            
            eStatisticType statisticType = (eStatisticType)value;
            string result = null;

            switch (statisticType)
            {
                case eStatisticType.Views:
                    result = _localizedStrings.Options.StatisticType_Views;
                    break;
                case eStatisticType.PostViews:
                    result = _localizedStrings.Options.StatisticType_PostViews;
                    break;
                case eStatisticType.Referrers:
                    result = _localizedStrings.Options.StatisticType_Referrers;
                    break;
                case eStatisticType.SearchTerms:
                    result = _localizedStrings.Options.StatisticType_SearchTerms;
                    break;
                case eStatisticType.Clicks:
                    result = _localizedStrings.Options.StatisticType_Clicks;
                    break;
                default:
                    result = _localizedStrings.Options.StatisticType_Views;
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string))
            {
                throw new ArgumentException("Value is not a string", "value");
            }

            //set result to Views by default, only check for remaining items
            eStatisticType result = eStatisticType.Views;

            if (value.Equals(_localizedStrings.Options.StatisticType_PostViews))
            {
                result = eStatisticType.PostViews;
            }
            else if (value.Equals(_localizedStrings.Options.StatisticType_Referrers))
            {
                result = eStatisticType.Referrers;
            }
            else if (value.Equals(_localizedStrings.Options.StatisticType_SearchTerms))
            {
                result = eStatisticType.SearchTerms;
            }
            else if (value.Equals(_localizedStrings.Options.StatisticType_Clicks))
            {
                result = eStatisticType.Clicks;
            }

            return result;
        }

        #endregion
    }
}
