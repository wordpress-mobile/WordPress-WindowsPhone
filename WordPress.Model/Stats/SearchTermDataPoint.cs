using System.ComponentModel;

namespace WordPress.Model
{
    /// <summary>
    /// Data object returned by the GetSearchTermStatsRPC
    /// </summary>
    public class SearchTermDataPoint: INotifyPropertyChanged
    {
        #region member variables

        private string _searchTerm;
        private int _count;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public SearchTermDataPoint() { }

        #endregion

        #region properties

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                if (value != _searchTerm)
                {
                    _searchTerm = value;
                    NotifyPropertyChanged("SearchTerm");
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
