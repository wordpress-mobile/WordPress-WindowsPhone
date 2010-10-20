using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WordPress.Model
{
    public class Account: INotifyPropertyChanged
    {
        #region member variables

        private int _id;
        private string _url;
        private string _blogName;
        private string _userName;
        private string _password;
        //private ObservableCollection<Post> _posts = new ObservableCollection<Post>();
        //private ObservableCollection<Comment> _comments = new ObservableCollection<Comment>();
        //private ObservableCollection<Page> _pages = new ObservableCollection<Page>();

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region properties

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        public string Url
        {
            get { return _url; }
            set 
            {
                if (_url != value)
                {
                    _url = value;
                    NotifyPropertyChanged("Url");
                }
            }
        }

        public string BlogName
        {
            get { return _blogName; }
            set 
            {
                if (_blogName != value)
                {
                    _blogName = value;
                    NotifyPropertyChanged("BlogName");
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set 
            {
                if (_userName != value)
                {
                    _userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    NotifyPropertyChanged("Password");
                }
            }
        }

        //public ObservableCollection<Post> Posts
        //{
        //    get { return _posts; }
        //}

        //public ObservableCollection<Comment> Comments
        //{
        //    get { return _comments; }
        //}

        //public ObservableCollection<Page> Pages
        //{
        //    get { return _pages; }
        //}

        #endregion

        #region methods

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));                
            }
        }

        #endregion

    }
}
