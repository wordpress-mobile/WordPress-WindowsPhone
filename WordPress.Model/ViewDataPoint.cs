using System;
using System.ComponentModel;

namespace WordPress.Model
{
    public class ViewDataPoint: INotifyPropertyChanged
    {
        #region member variables

        private DateTime _viewDate;
        private int _viewCount;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructor

        public ViewDataPoint() { }

        #endregion

        #region properties

        public DateTime ViewDate
        {
            get { return _viewDate; }
            set 
            {
                if (value != _viewDate)
                {
                    _viewDate = value;
                    NotifyPropertyChanged("ViewDate");
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
