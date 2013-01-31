using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class MediaItem : INotifyPropertyChanged
    {
        #region member variables

        private DateTime _dateCreatedGMT;
        private string _parent;
        private string _link;
        private string _thumbnail;
        private string _title;
        private string _caption;
        private string _description;

        private const string DATECREATEDGMT_VALUE = "date_created_gmt";
        private const string PARENT_VALUE = "parent";
        private const string LINK_VALUE = "link";
        private const string THUMBNAIL_VALUE = "thumbnail";
        private const string DESCRIPTION_VALUE = "description";
        private const string CAPTION_VALUE = "caption";
        private const string TITLE_VALUE = "title";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public MediaItem() { }

        public MediaItem(XElement structElement)
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

        public string Parent
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

        public string Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                if (value != _thumbnail)
                {
                    _thumbnail = value;
                    NotifyPropertyChanged("Thumbnail");
                }
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                if (value != _caption)
                {
                    _caption = value;
                    NotifyPropertyChanged("Caption");
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
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string value = string.Empty;
                string memberName = member.Element(XmlRPCResponseConstants.NAME).Value;
               
                if (DATECREATEDGMT_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.DATETIMEISO8601).First().Value;
                    DateTime tempDate;
                    if (DateTime.TryParseExact(value, Constants.WORDPRESS_DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                    {
                        _dateCreatedGMT = tempDate;
                    }
                    else
                    {
                        Exception detailedError = new FormatException("Unable to parse GMT date-time: " + value);
                        throw new XmlRPCParserException(XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE, XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_MESSAGE, detailedError);
                    }
                }
                else if (PARENT_VALUE.Equals(memberName))
                {
                    _parent = member.GetValueAsString(true);
                }
                else if (LINK_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _link = value;
                }
                else if (THUMBNAIL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _thumbnail = value;
                }
                else if (DESCRIPTION_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _description = value.HtmlDecode();
                }
                else if (TITLE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _title = value.HtmlDecode();
                }
                else if (CAPTION_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _caption = value.HtmlDecode();
                }

            } // end for-each

        }
        #endregion

    }
}