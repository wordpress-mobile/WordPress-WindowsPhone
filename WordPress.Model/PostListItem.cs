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
        private DateTime _dateCreatedGMT;
        private string _title;
        private string _postId;
        private string _status;

        private const string USERID_VALUE = "userid";
        private const string DATECREATED_VALUE = "dateCreated";
        private const string DATECREATEDGMT_VALUE = "date_created_gmt";
        private const string TITLE_VALUE = "title";
        private const string POSTID_VALUE = "postid";
        private const string POST_STATUS__VALUE = "post_status";


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

        public String Status
        {
            get { return _status; }
            set
            {
                if (value != _status)
                {
                    _status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }
 
        public string PostId
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
                if (string.IsNullOrEmpty(_title))
                {
                    return string.Empty;
                }
                else
                {
                    return _title;
                }
            }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");      
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
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            string value = null;
            DateTime _tmpDateCreated = DateTime.Now;
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                value = null;
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
                        _tmpDateCreated = tempDate;
                    }
                    else
                    {
                        //Pending posts don't have a valid date set.
                        if (_status != null && _status.Equals("pending"))
                        {
                            _dateCreated = DateTime.Now;
                            _dateCreatedGMT = _dateCreated.ToUniversalTime();
                        }
                        else
                            throw new XmlRPCParserException(XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE, XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_MESSAGE, new FormatException("Unable to parse given date-time"));
                    }
                }
                else if (DATECREATEDGMT_VALUE.Equals(memberName)) 
                {
                    value = member.Descendants(XmlRPCResponseConstants.DATETIMEISO8601).First().Value;
                    DateTime tempDate;
                    if (DateTime.TryParseExact(value, Constants.WORDPRESS_DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                    {
                        _dateCreatedGMT = tempDate;
                        _dateCreated = tempDate.ToLocalTime();
                    }
                    else
                    {
                        //Pending posts don't have a valid date set.
                        if (_status != null && _status.Equals("pending"))
                        {
                            _dateCreated = DateTime.Now;
                            _dateCreatedGMT = _dateCreated.ToUniversalTime();
                        } 
                        else
                            throw new XmlRPCParserException(XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE, XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_MESSAGE, new FormatException("Unable to parse given date-time-gmt"));
                    }
                }
                else if (TITLE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _title = value.HtmlDecode();
                }
                else if (POST_STATUS__VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _status = value.HtmlDecode();
                }
                else if (POSTID_VALUE.Equals(memberName))
                {
                    try
                    {
                        value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                        }
                        catch (Exception)
                        {

                        }
                    }
                    
                    if (value == null)
                        throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);

                    _postId = value;
                    
                }
            } //End for-each

            if (_dateCreated == null && _tmpDateCreated != null)
            {
                _dateCreated = _tmpDateCreated; //make sure dateCreated is initializated
            }
        }

        #endregion

    }
}
