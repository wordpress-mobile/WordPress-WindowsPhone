using System.ComponentModel;

namespace WordPress.Model
{
    /// <summary>
    /// Data object returned by the GetClickStatsRPC
    /// </summary>
    public class ClickDataPoint: INotifyPropertyChanged
    {
        #region member variables

        private string _url;
        private int _count;

        #endregion

        #region events
        
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public ClickDataPoint() { }

        #endregion

        #region properties

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

        public int Count
        {
            get { return _count; }
            set
            {
                if (value != _count)
                {
                    _count = value;
                    NotifyPropertyChanged("Count");
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
