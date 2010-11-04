using System;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress.Converters
{
    public class StatisticPeriodToStringConverter:IValueConverter
    {
        #region member variables

        StringTable _localizedStrings;

        #endregion

        #region constructors

        public StatisticPeriodToStringConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is eStatisticPeriod))
            {
                throw new ArgumentException("Value is not eStatisticPeriod", "value");
            }
            
            eStatisticPeriod statisticPeriod = (eStatisticPeriod)value;
            string result = null;

            switch (statisticPeriod)
            {
                case eStatisticPeriod.LastWeek:
                    result = _localizedStrings.Options.StatisticPeriod_LastWeek;
                    break;
                case eStatisticPeriod.LastMonth:
                    result = _localizedStrings.Options.StatisticPeriod_LastMonth;
                    break;
                case eStatisticPeriod.LastQuarter:
                    result = _localizedStrings.Options.StatisticPeriod_LastQuarter;
                    break;
                case eStatisticPeriod.LastYear:
                    result = _localizedStrings.Options.StatisticPeriod_LastYear;
                    break;
                case eStatisticPeriod.AllTime:
                    result = _localizedStrings.Options.StatisticPeriod_AllTime;
                    break;
                default:
                    result = _localizedStrings.Options.StatisticPeriod_LastWeek;
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
            
            //set result to LastWeek by default, only check for remaining items
            eStatisticPeriod result = eStatisticPeriod.LastWeek;

            if (value.Equals(_localizedStrings.Options.StatisticPeriod_LastMonth))
            {
                result = eStatisticPeriod.LastMonth;
            }
            else if (value.Equals(_localizedStrings.Options.StatisticPeriod_LastQuarter))
            {
                result = eStatisticPeriod.LastQuarter;
            }
            else if (value.Equals(_localizedStrings.Options.StatisticPeriod_LastYear))
            {
                result = eStatisticPeriod.LastYear;
            }
            else if (value.Equals(_localizedStrings.Options.StatisticPeriod_AllTime))
            {
                result = eStatisticPeriod.AllTime;
            }

            return result;
        }

        #endregion
    }
}
