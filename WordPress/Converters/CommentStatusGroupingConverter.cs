using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

using WordPress.Model;

namespace WordPress.Converters
{
    /// <summary>
    /// Groups a list of Comment objects by their CommentStatus value.
    /// </summary>
    public class CommentStatusGroupingConverter: IValueConverter
    {
        public CommentStatusGroupingConverter() { }

        [DefaultValue(eCommentStatus.approve)]
        public eCommentStatus Status { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(IEnumerable<Comment>)))
            {
                throw new ArgumentException("Value cannot be assigned to IEnumerable<Comment>", "value");
            }

            IEnumerable<Comment> comments = value as IEnumerable<Comment>;
            IEnumerable<Comment> result = comments.Where(comment => Status == comment.CommentStatus).OrderByDescending(comment => comment.DateCreatedGMT);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
