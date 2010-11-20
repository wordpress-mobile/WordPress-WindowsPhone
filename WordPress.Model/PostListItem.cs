using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;

namespace WordPress.Model
{
    public class PostListItem : INotifyPropertyChanged
    {
        #region member variables

        private int _userId;
        private DateTime _dateCreated;
        private string _content;
        private int _postId;

        private const string USERID_VALUE = "userid";
        private const string DATECREATED_VALUE = "dateCreated";
        private const string CONTENT_VALUE = "content";
        private const string POSTID_VALUE = "postid";
        private const string TITLE_OPENING_TAG = "<title>";
        private const string TITLE_CLOSING_TAG = "</title>";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public PostListItem() { }

        public PostListItem(XElement structElement)
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public int UserId
        {
            get { return _userId; }
            set
            {
                if (value != _userId)
                {
                    _userId = value;
                    NotifyPropertyChanged("UserId");
                }
            }
        }

        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set
            {
                if (value != _dateCreated)
                {
                    _dateCreated = value;
                    NotifyPropertyChanged("DateCreated");
                }
            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                if (value != _content)
                {
                    _content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }

        public int PostId
        {
            get { return _postId; }
            set
            {
                if (value != _postId)
                {
                    _postId = value;
                    NotifyPropertyChanged("PostId");
                }
            }
        }

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_content))
                {
                    return string.Empty;
                }
                else
                {

                    int startIndex = _content.IndexOf(TITLE_OPENING_TAG) + TITLE_OPENING_TAG.Length;
                    int endIndex = _content.IndexOf(TITLE_CLOSING_TAG);
                    return _content.Substring(startIndex, endIndex - startIndex);
                }
            }
        }
        #endregion

        #region methods

        private void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ParseElement(XElement element)
        {
            if (!element.HasElements)
            {
                throw new ArgumentException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            string value = null;
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if (USERID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _userId))
                    {
                        _userId = -1;
                    }
                }
                else if (DATECREATED_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.DATETIMEISO8601).First().Value;
                    DateTime tempDate;
                    if (DateTime.TryParseExact(value, Constants.WORDPRESS_DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                    {
                        _dateCreated = tempDate.ToLocalTime();
                    }
                    else
                    {
                        throw new FormatException("Unable to parse given date-time");
                    }
                }
                else if (CONTENT_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _content = value.HtmlDecode();
                }
                else if (POSTID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                    if (!int.TryParse(value, out _postId))
                    {
                        _postId = -1;
                    }
                }
            }
        }

        #endregion

    }
}
