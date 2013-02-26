using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WordPress.Model
{
    public class BooleanResponseObject : INotifyPropertyChanged
    {
  
        private bool _success = false;
        
        public event PropertyChangedEventHandler PropertyChanged;


        public BooleanResponseObject() { }

        public BooleanResponseObject(int iValue)
        {
            _success = iValue == 0 ? false : true;
        } 

        public BooleanResponseObject(bool bValue) {
            _success = bValue;
        } 

        private void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool Success
        {
            get { return _success; }
            set
            {
                if (value != _success)
                {
                    _success = value;
                    NotifyPropertyChanged("Success");
                }
            }
        }
    }
}
