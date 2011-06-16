using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text;
using System.Security.Cryptography;
using System.Net;

namespace WordPress.Model
{
    public class Comment : INotifyPropertyChanged
    {
        #region member variables

        private DateTime _dateCreatedGMT;
        private int _userId;
        private int _commentId;
        private int _parent;
        private string _status;
        private string _content;
        private string _link;
        private int _postId;
        private string _postTitle;
        private string _author;
        private string _authorUrl;
        private string _authorEmail;
        private string _authorIp;
        private string _commentType;
        
        private const string DATECREATEDGMT_VALUE = "date_created_gmt";
        private const string USERID_VALUE = "user_id";
        private const string COMMENTID_VALUE = "comment_id";
        private const string PARENT_VALUE = "parent";
        private const string STATUS_VALUE = "status";
        private const string CONTENT_VALUE = "content";
        private const string LINK_VALUE = "link";
        private const string POSTID_VALUE = "post_id";
        private const string POSTTITLE_VALUE = "post_title";
        private const string AUTHOR_VALUE = "author";
        private const string AUTHORURL_VALUE = "author_url";
        private const string AUTHOREMAIL_VALUE = "author_email";
        private const string AUTHORIP_VALUE = "author_ip";
        private const string COMMENTTYPE_VALUE = "type";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public Comment() { }

        public Comment(XElement structElement)
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public DateTime DateCreatedGMT
        {
            get { return _dateCreatedGMT; }
            set
            {
                if (value != _dateCreatedGMT)
                {
                    _dateCreatedGMT = value;
                    NotifyPropertyChanged("DateCreatedGMT");
                }
            }
        }

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

        public int CommentId
        {
            get { return _commentId; }
            set
            {
                if (value != _commentId)
                {
                    _commentId = value;
                    NotifyPropertyChanged("CommentId");
                }
            }
        }

        public int Parent
        {
            get { return _parent; }
            set 
            {
                if (value != _parent)
                {                    
                    _parent = value;
                    NotifyPropertyChanged("Parent");
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

        public string Status
        {
            get { return _status; }
            set 
            {
                if (value != _status)
                {
                    _status = value;
                    NotifyPropertyChanged("Status");
                    NotifyPropertyChanged("CommentStatus");
                }
            }
        }

        public string FormattedStatus
        {
            //Format the comment status from 'approve' to 'Approved' etc
            get {
                String formattedStatus = "Approved";
                if (_status == "hold")
                    formattedStatus = "Unapproved";
                else if (_status == "spam")
                    formattedStatus = "Spam";

                return formattedStatus; 
            }
        }

        [XmlIgnore]
        public eCommentStatus CommentStatus
        {
            get
            {
                //all comments are "approve" status by default, if we can't parse the status
                //default to that value
                eCommentStatus result = eCommentStatus.approve;
                try
                {
                    if (!string.IsNullOrEmpty(_status))
                    {
                        result = (eCommentStatus)Enum.Parse(typeof(eCommentStatus), _status, true);
                    }
                }
                catch (Exception) { }

                return result;
            }
            set
            {
                if (value != CommentStatus)
                {
                    //use the Status property API to notify any interested parties of the change
                    Status = value.ToString();
                    NotifyPropertyChanged("CommentStatus");
                }
            }
        }

        public string Link
        {
            get { return _link; }
            set 
            {
                if (value != _link)
                {
                    _link = value;
                    NotifyPropertyChanged("Link");
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

        public string PostTitle
        {
            get { return _postTitle; }
            set 
            {
                if (value != _postTitle)
                {
                    _postTitle = value;
                    NotifyPropertyChanged("PostTitle");
                }
            }
        }

        public string Author
        {
            get { return _author; }
            set 
            {
                if (value != _author)
                {
                    _author = value;
                    NotifyPropertyChanged("Author");
                }
            }
        }

        public string AuthorUrl
        {
            get { return _authorUrl; }
            set 
            {
                if (value != _authorUrl)
                {
                    _authorUrl = value;
                    NotifyPropertyChanged("AuthorUrl");
                }
            }
        }

        public string AuthorEmail
        {
            get { return _authorEmail; }
            set
            {
                if (value != _authorEmail)
                {
                    _authorEmail = value;
                    NotifyPropertyChanged("AuthorEmail");
                }
            }
        }

        public string AuthorIp
        {
            get { return _authorIp; }
            set 
            {
                if (value != _authorIp)
                {
                    _authorIp = value;
                    NotifyPropertyChanged("AuthorIp");
                }
            }
        }

        public string CommentType
        {
            get { return _commentType; }
            set
            {
                if (value != _commentType)
                {
                    _commentType = value;
                    NotifyPropertyChanged("CommentType");
                }
            }
        }

        [XmlIgnore]
        public string GravatarUrl
        {
            get
            {
                string emailAddress = _authorEmail ?? String.Empty;

                Encoder encoder = Encoding.Unicode.GetEncoder();
                byte[] unicodeText = new byte[emailAddress.Length * 2];
                encoder.GetBytes(emailAddress.ToCharArray(), 0, emailAddress.Length, unicodeText, 0, true);

                string hash = MD5.GetMd5String(emailAddress.Trim());
                string uriFormat = "http://gravatar.com/avatar/{0}?s=100&d=identicon";
                string result = string.Format(uriFormat, hash);
                return result;
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
                if (DATECREATEDGMT_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.DATETIMEISO8601).First().Value;
                    if (!DateTime.TryParseExact(value, Constants.WORDPRESS_DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _dateCreatedGMT))
                    {
                        throw new FormatException("Unable to parse given date-time.");
                    }
                }
                else if (USERID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _userId))
                    {
                        _userId = -1;
                    }
                }
                else if (COMMENTID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _commentId))
                    {
                        _commentId = -1;
                    }
                }
                else if (PARENT_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _parent))
                    {
                        _parent = -1;
                    }
                }
                else if (STATUS_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _status = value;
                }
                else if (CONTENT_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _content = value.HtmlDecode();
                }
                else if (LINK_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _link = value;
                }
                else if (POSTID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _postId))
                    {
                        _postId = -1;
                    }
                }
                else if (POSTTITLE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _postTitle = value.HtmlDecode();
                }
                else if (AUTHOR_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _author = value.HtmlDecode();
                }
                else if (AUTHORURL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _authorUrl = value;
                }
                else if (AUTHOREMAIL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _authorEmail = value;
                }
                else if (AUTHORIP_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _authorIp = value;
                }
                else if (COMMENTTYPE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _commentType = value;
                }
            }
        }

        private class WPDateFormatProvider : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                DateTimeFormatInfo info = new DateTimeFormatInfo();
                info.FullDateTimePattern = "yyyyMMddTH:mm:ss";
                return info;
            }
        }
        #endregion
    }
}
