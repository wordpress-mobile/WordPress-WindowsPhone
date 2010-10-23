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
    public class CustomField: INotifyPropertyChanged
    {
        #region member variables

        private string _id;
        private string _key;
        private string _value;

        private const string ID_VALUE = "id";
        private const string KEY_VALUE = "key";
        private const string VALUE_VALUE = "value";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructor

        public CustomField() { }

        public CustomField(XElement structElement)
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public string Id
        {
            get { return _id; }
            set 
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }
        
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

        private void ParseElement(XElement element)
        {
            if (!element.HasElements)
            {
                throw new ArgumentException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            string value = string.Empty;
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string memberName = member.Element(XmlRPCResponseConstants.NAME).Value;
                if (ID_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _id = value;
                }
                else if (KEY_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _key = value.HtmlDecode();
                }
                else if (VALUE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _value = value.HtmlDecode();
                }
            }        
        }

        #endregion

    }
}
