using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class Blog: INotifyPropertyChanged
    {
        #region member variables

        private string _username;
        private string _password;
        private bool _isAdmin;
        private string _url;
        private int _blogId;
        private string _blogName;
        private string _xmlrpc;

        private const string ISADMIN_VALUE = "isAdmin";
        private const string URL_VALUE = "url";
        private const string BLOGID_VALUE = "blogid";
        private const string BLOGNAME_VALUE = "blogName";
        private const string XMLRPC_VALUE = "xmlrpc";
        
        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public Blog() 
        {
            Comments = new ObservableCollection<Comment>();
            PostListItems = new ObservableCollection<PostListItem>();
            PageListItems = new ObservableCollection<PageListItem>();
        }

        public Blog(XElement structElement) 
            : this()
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                if (value != _isAdmin)
                {
                    _isAdmin = value;
                    NotifyPropertyChanged("IsAdmin");
                }
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (value != _url)
                {
                    _url = value;
                    NotifyPropertyChanged("Url");
                }
            }
        }

        public int BlogId
        {
            get { return _blogId; }
            set
            {
                if (value != _blogId)
                {
                    _blogId = value;
                    NotifyPropertyChanged("BlogId");
                }
            }
        }

        public string BlogName
        {
            get { return _blogName; }
            set
            {
                if (value != _blogName)
                {
                    _blogName = value;
                    NotifyPropertyChanged("BlogName");
                }
            }
        }

        public string Xmlrpc
        {
            get { return _xmlrpc; }
            set
            {
                if (value != _xmlrpc)
                {
                    _xmlrpc = value;
                    NotifyPropertyChanged("Xmlrpc");
                }
            }
        }

        public ObservableCollection<Comment> Comments { get; private set; }

        public ObservableCollection<PostListItem> PostListItems { get; private set; }

        public ObservableCollection<PageListItem> PageListItems { get; private set; }

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

            //TODO: research performance on this, seems a bit wordy...
            string value = null;
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if (ISADMIN_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.BOOLEAN).First().Value;
                    _isAdmin = Convert.ToBoolean(Convert.ToInt16(value));
                }
                else if (URL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _url = value;
                }
                else if (BLOGID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    if (!int.TryParse(value, out _blogId))
                    {
                        //TODO: throw exception here?  failure to parse this is a major problem
                    }
                }
                else if (BLOGNAME_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _blogName = value.HtmlDecode();
                }
                else if (XMLRPC_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _xmlrpc = value;
                }
            }            
        }

        #endregion
    }
}
