using System;
using System.Windows.Data;

using WordPress.Localization;
using WordPress.Model;


namespace WordPress.Converters
{
    public class PostStatusInListConverter : IValueConverter
    {
   
        StringTable _localizedStrings;
        
        public PostStatusInListConverter()
        {
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }


        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return string.Empty;

            if (!(value is String))
            {
                throw new ArgumentException("value is not a String", "value");
            }
           
            string postStatus = value as string;
            if (postStatus.Equals("publish")) 
                return string.Empty;
            else if (postStatus.Equals("future"))
                return _localizedStrings.ControlsText.Scheduled;
            else if (postStatus.Equals("draft"))
                return _localizedStrings.ControlsText.Draft;
            else if (postStatus.Equals("pending"))
                return _localizedStrings.ControlsText.PendingReview;
            else if (postStatus.Equals("private"))
                return _localizedStrings.ControlsText.Private;
            else if (postStatus.Equals("localdraft"))
                return _localizedStrings.ControlsText.LocalDraft;
            else
                return postStatus;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
