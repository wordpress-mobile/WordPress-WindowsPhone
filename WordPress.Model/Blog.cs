using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class Blog: INotifyPropertyChanged, IEditableObject
    {
        #region member variables

        private string _username;
        private string _password;
        private bool _isAdmin;
        private string _url;
        private int _blogId;
        private string _blogName;
        private string _xmlrpc;

        private bool _placeImageAboveText;
        private int _thumbnailPixelWidth;
        private bool _alignThumbnailToCenter;
        private bool _createLinkToFullImage;
        private bool _geotagPosts;
        private string _apikey;

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
            _thumbnailPixelWidth = 100;

            Comments = new ObservableCollection<Comment>();
            PostListItems = new ObservableCollection<PostListItem>();
            PageListItems = new ObservableCollection<PageListItem>();
            Categories = new ObservableCollection<Category>();
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
            set 
            {
                if (value != _username)
                {
                    _username = value;
                    NotifyPropertyChanged("Username");
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set 
            {
                if (value != _password)
                {
                    _password = value;
                    NotifyPropertyChanged("Password");
                }
            }
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

        public ObservableCollection<Category> Categories { get; private set; }

        public bool PlaceImageAboveText 
        {
            get { return _placeImageAboveText; }
            set
            {
                if (value != _placeImageAboveText)
                {
                    _placeImageAboveText = value;
                    NotifyPropertyChanged("PlaceImageAboveText");
                }
            }
        }

        public int ThumbnailPixelWidth
        {
            //DEV NOTE: a value of zero should be used to indicate the image should
            //be displayed with its original dimensions
            get { return _thumbnailPixelWidth; }
            set 
            {
                if (value != _thumbnailPixelWidth)
                {
                    _thumbnailPixelWidth = value;
                    NotifyPropertyChanged("ThumbnailPixelWidth");
                }
            }
        }

        public bool AlignThumbnailToCenter 
        {
            get { return _alignThumbnailToCenter; }
            set
            {
                if (value != _alignThumbnailToCenter)
                {
                    _alignThumbnailToCenter = value;
                    NotifyPropertyChanged("AlignThumbnailToCenter");
                }
            }
        }

        public bool CreateLinkToFullImage 
        {
            get { return _createLinkToFullImage; }
            set
            {
                if (value != _createLinkToFullImage)
                {
                    _createLinkToFullImage = value;
                    NotifyPropertyChanged("CreateLinkToFullImage");
                }
            }
        }

        public bool GeotagPosts 
        {
            get { return _geotagPosts; }
            set
            {
                if (value != _geotagPosts)
                {
                    _geotagPosts = value;
                    NotifyPropertyChanged("GeotagPosts");
                }
            }
        }

        public string ApiKey
        {
            get { return _apikey; }
            set
            {
                if (value != _apikey)
                {
                    _apikey = value;
                    NotifyPropertyChanged("ApiKey");
                }
            }
        }

        private Momento<Blog> Snapshot { get; set; }

        /// <summary>
        /// Indicates that the BeginEdit method was called and a snapshot of the blog exists.
        /// </summary>
        public bool IsEditing
        {
            get { return null != Snapshot; }
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
                    value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                    if (!int.TryParse(value, out _blogId))
                    {
                        throw new ArgumentException("Unable to successfully parse Blog ID from server response");
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

        public void BeginEdit()
        {
            if (null == Snapshot)
            {
                Snapshot = new Momento<Blog>(this);
            }
        }

        public void CancelEdit()
        {
            if (null == Snapshot)
            {
                throw new InvalidOperationException("CancelEdit cannot successfully complete; BeginEdit was not invoked.");
            }
            Snapshot.RestoreState(this);
            Snapshot = null;
        }

        public void EndEdit()
        {
            if (null == Snapshot)
            {
                throw new InvalidOperationException("EndEdit cannot successfully complete; BeginEdit was not invoked.");
            }
            Snapshot = null;
        }

        #endregion

    }
}
