using System.ComponentModel;

namespace WordPress.Model
{
    /// <summary>
    /// Data object returned by the GetReferrerStatsRPC
    /// </summary>
    public class ReferrerDataPoint: INotifyPropertyChanged
    {
        #region member variables

        private string _url;
        private int _referrals;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructor

        public ReferrerDataPoint() { }

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

        public int Referrals
        {
            get { return _referrals; }
            set
            {
                if (value != _referrals)
                {
                    _referrals = value;
                    NotifyPropertyChanged("Referrals");
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
