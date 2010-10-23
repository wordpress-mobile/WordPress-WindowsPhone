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

using WordPress.Model;

namespace WordPress
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
                    break;
                case eCommentStatus.hold:
                    return UnapproveBrush;
                    break;
                case eCommentStatus.spam:
                    return SpamBrush;
                    break;
                default:
                    return ApproveBrush;
                    break;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
