using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WordPress.Model
{
    public class IntResponseObject : INotifyPropertyChanged
    {
  
        private int _value = 0;
        
        public event PropertyChangedEventHandler PropertyChanged;


        public IntResponseObject() { }

        public IntResponseObject(int iValue)
        {
            _value = iValue;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int Value
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
    }
}

