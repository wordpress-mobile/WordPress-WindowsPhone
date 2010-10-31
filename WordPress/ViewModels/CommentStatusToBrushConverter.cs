using System;
using System.Windows.Data;
using System.Windows.Media;
using WordPress.Model;

namespace WordPress.Converters
{
    /// <summary>
    /// Performs the conversion from eCommentStatus to Color
    /// </summary>
    public class CommentStatusToBrushConverter: IValueConverter
    {
        #region constructor

        public CommentStatusToBrushConverter()
        {
            ApproveBrush = new SolidColorBrush(Colors.Green);
            UnapproveBrush = new SolidColorBrush(Colors.Orange);
            SpamBrush = new SolidColorBrush(Colors.Red);
        }

        #endregion

        #region properties

        public Brush ApproveBrush { get; set; }
        public Brush UnapproveBrush { get; set; }
        public Brush SpamBrush { get; set; }

        #endregion

        #region methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            eCommentStatus status = (eCommentStatus)value;

            switch (status)
            {
                case eCommentStatus.approve:
                    return ApproveBrush;                    
                case eCommentStatus.hold:
                    return UnapproveBrush;                    
                case eCommentStatus.spam:
                    return SpamBrush;                    
                default:
                    return ApproveBrush;             
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
