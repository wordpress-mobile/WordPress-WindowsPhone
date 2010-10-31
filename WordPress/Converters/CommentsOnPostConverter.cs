using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

using WordPress.Model;

namespace WordPress.Converters
{
    /// <summary>
    /// This class is used to group lists of Comment objects related to a post or a page
    /// </summary>
    public class CommentsOnPostConverter: IValueConverter
    {
        public CommentsOnPostConverter() { }

        public int? Id { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(IEnumerable<Comment>)))
            {
                throw new ArgumentException("Value cannot be assigned to IEnumerable<Comment>", "value");
            }

            IEnumerable<Comment> comments = value as IEnumerable<Comment>;
            IEnumerable<Comment> result;
            if (Id.HasValue)
            {
                result = comments.Where(comment => Id.Value == comment.PostId).OrderByDescending(comment => comment.DateCreatedGMT);
            }
            else
            {
                result = comments;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
