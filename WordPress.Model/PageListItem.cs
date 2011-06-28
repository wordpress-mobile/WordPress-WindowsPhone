using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ComponentModel;
using System.Globalization;

namespace WordPress.Model
{
    public class PageListItem : INotifyPropertyChanged
    {
        #region member variables

        private string _pageId;
        private string _pageTitle;
        private string _pageParentId;
        private DateTime _dateCreated;
        private DateTime _dateCreatedGMT;

        private const string PAGEID_VALUE = "page_id";
        private const string PAGETITLE_VALUE = "page_title";
        private const string PAGEPARENTID_VALUE = "page_parent_id";
        private const string DATECREATED_VALUE = "dateCreated";
        private const string DATECREATEDGMT_VALUE = "date_created_gmt";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public PageListItem() { }

        public PageListItem(XElement element)
        {
            ParseElement(element);
        }

        #endregion

        #region properties

        public string PageId
        {
            get { return _pageId; }
            set
            {
                if (value != _pageId)
                {
                    _pageId = value;
                    NotifyPropertyChanged("PageId");
                }
            }
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                if (value != _pageTitle)
                {
                    _pageTitle = value;
                    NotifyPropertyChanged("PageTitle");
                }
            }
        }

        public string PageParentId
        {
            get { return _pageParentId; }
            set
            {
                if (value != _pageParentId)
                {
                    _pageParentId = value;
                    NotifyPropertyChanged("PageParentId");
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
                string value = null;
                string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if (PAGEID_VALUE.Equals(memberName))
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

                    _pageId = value;
                }
                else if (PAGETITLE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _pageTitle = value.HtmlDecode();
                }
                else if (PAGEPARENTID_VALUE.Equals(memberName))
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
                    
                    _pageParentId = value;
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
                else if (DATECREATEDGMT_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.DATETIMEISO8601).First().Value;
                    if (!DateTime.TryParseExact(value, Constants.WORDPRESS_DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _dateCreatedGMT))
                    {
                        throw new FormatException("Unable to parse given date-time");                        
                    }
                }
            }
        }

        #endregion
    }
}
