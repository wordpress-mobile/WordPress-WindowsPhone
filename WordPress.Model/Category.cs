using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class Category: INotifyPropertyChanged
    {
        #region member variables

        private int _categoryId;
        private int _parentId;
        private string _description;
        private string _categoryDescription;
        private string _categoryName;
        private string _htmlUrl;
        private string _rssUrl;
        private string _categorySlug;

        private const string CATEGORYID_VALUE = "categoryId";
        private const string PARENTID_VALUE = "parentId";
        private const string DESCRIPTION_VALUE = "description";
        private const string CATEGORYDESCRIPTION_VALUE = "categoryDescription";
        private const string CATEGORYNAME_VALUE = "categoryName";
        private const string HTMLURL_VALUE = "htmlUrl";
        private const string RSSURL_VALUE = "rssUrl";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public Category() 
        {
            _categoryId = -1;
            _parentId = 0;
        }

        public Category(XElement structElement)
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public int CategoryId
        {
            get { return _categoryId; }
            set 
            {
                if (value != _categoryId)
                {
                    _categoryId = value;
                    NotifyPropertyChanged("CategoryId");
                }
            }
        }

        public int ParentId
        {
            get { return _parentId; }
            set
            {
                if (value != _parentId)
                {
                    _parentId = value;
                    NotifyPropertyChanged("ParentId");
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

        public string CategoryDescription
        {
            get { return _categoryDescription; }
            set 
            {
                if (value != _categoryDescription)
                {                    
                    _categoryDescription = value;
                    NotifyPropertyChanged("CategoryDescription");
                }
            }
        }

        public string CategoryName
        {
            get { return _categoryName; }
            set 
            {
                if (value != _categoryName)
                {
                    _categoryName = value;
                    NotifyPropertyChanged("CategoryName");
                }
            }
        }

        public string HtmlUrl
        {
            get { return _htmlUrl; }
            set
            {
                if (value != _htmlUrl)
                {
                    _htmlUrl = value;
                    NotifyPropertyChanged("HtmlUrl");
                }
            }
        }

        public string RssUrl
        {
            get { return _rssUrl; }
            set
            {
                if (value != _rssUrl)
                {
                    _rssUrl = value;
                    NotifyPropertyChanged("RssUrl");
                }
            }
        }

        public string CategorySlug
        {
            get { return _categorySlug; }
            set
            {
                if (value != _categorySlug)
                {
                    _categorySlug = value;
                    NotifyPropertyChanged("CategorySlug");
                }
            }
        }
        #endregion

        #region methods

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
                if (CATEGORYID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _categoryId))
                    {
                        _categoryId = -1;
                    }
                }
                else if (PARENTID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _parentId))
                    {
                        _parentId = 0;
                    }
                }
                else if (DESCRIPTION_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _description = value.HtmlDecode();
                }
                else if (CATEGORYDESCRIPTION_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _categoryDescription = value.HtmlDecode();
                }
                else if (CATEGORYNAME_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _categoryName = value.HtmlDecode();
                }
                else if (HTMLURL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _htmlUrl = value;
                }
                else if (RSSURL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _rssUrl = value;
                }
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
