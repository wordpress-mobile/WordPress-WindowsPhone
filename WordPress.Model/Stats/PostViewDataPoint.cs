using System.ComponentModel;

namespace WordPress.Model
{
    /// <summary>
    /// Data object returned by the GetPostViewStatsRPC
    /// </summary>
    public class PostViewDataPoint: INotifyPropertyChanged
    {
        #region member variables

        private int _id;
        private string _title;
        private string _url;
        private int _viewCount;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        #endregion

        #region properties

        public int PostId
        {
            get { return _id; }
            set 
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("PostId");
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

        public int ViewCount
        {
            get { return _viewCount; }
            set 
            {
                if (value != _viewCount)
                {
                    _viewCount = value;
                    NotifyPropertyChanged("ViewCount");
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

        #endregion
    }
}
