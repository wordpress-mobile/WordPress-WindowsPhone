using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class Option : INotifyPropertyChanged
    {
        #region member variables

        private string _name;
        private string _value;
        private bool _readOnly;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructor

        public Option() {}

        public Option(XElement structElement)
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
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

        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                if (value != _readOnly)
                {
                    _readOnly = value;
                    NotifyPropertyChanged("ReadOnly");
                }
            }
        }

        #endregion

        #region methods

        private void ParseElement(XElement element)
        {
            if (!element.HasElements)
            {
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            string name = element.Descendants(XmlRPCResponseConstants.NAME).First().Value; //name of the option
            string value = "";
            bool readOnly = false;

            XElement innerStruct = element.Descendants(XmlRPCResponseConstants.STRUCT).First(); //inner struct with values fot this option
            foreach (XElement innerMember in innerStruct.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string tmp = innerMember.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if (tmp.Equals("value"))
                {
                    value = innerMember.Descendants(XmlRPCResponseConstants.VALUE).First().Value;
                }
                else if (tmp.Equals("readonly"))
                {
                    string tmpReadOnlyString = innerMember.Descendants(XmlRPCResponseConstants.VALUE).First().Value.ToLower();

                    if (tmpReadOnlyString == "true" || tmpReadOnlyString == "yes" || tmpReadOnlyString == "1")
                        readOnly = true;
                    else
                        readOnly = false;
                }
            }

            _name = name;
            _value = value;
            _readOnly = readOnly;
        }

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
