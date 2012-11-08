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
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class PostFormat: INotifyPropertyChanged
    {
        #region member variables

        private string _key;
        private string _value;

        private const string KEY_VALUE = "key";
        private const string VALUE_VALUE = "value";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructor

        public PostFormat() { }

        public PostFormat(string key, string value) {
            _key = key;
            _value = value;
        }

        #endregion

        #region properties
   
        public string Key
        {
            get { return _key; }
            set 
            {
                if (value != _key)
                {
                    _key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }
        
        public string Value
        {
            get { return _value; }
            set 
            {
                if (value != _value)
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
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
