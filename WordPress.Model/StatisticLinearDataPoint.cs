using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace WordPress.Model
{
    public class StatisticLinearDataPoint: INotifyPropertyChanged
    {
        #region member variables

        private string _dependentAxisValue;
        private string _independentAxisValue;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public StatisticLinearDataPoint() { }

        #endregion

        #region properties

        public string DependentAxisValue
        {
            get { return _dependentAxisValue; }
            set
            {
                if (value != _dependentAxisValue)
                {
                    _dependentAxisValue = value;
                    NotifyPropertyChanged("DependentAxisValue");
                }
            }
        }

        public string IndependentAxisValue
        {
            get { return _independentAxisValue; }
            set
            {
                if (value != _independentAxisValue)
                {
                    _independentAxisValue = value;
                    NotifyPropertyChanged("IndependentAxisValue");
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
