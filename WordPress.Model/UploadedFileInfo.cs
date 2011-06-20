using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class UploadedFileInfo: INotifyPropertyChanged
    {
        #region member variables

        private const string FILE_VALUE = "file";
        private const string URL_VALUE = "url";
        private const string TYPE_VALUE = "type";

        private string _file;
        private string _url;
        private string _type;

        #endregion
        
        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public UploadedFileInfo() { }

        public UploadedFileInfo(XElement structElement)
        {
            ParseElement(structElement);
        }

        #endregion

        #region properties

        public string File
        {
            get { return _file; }
            set
            {
                if (value != _file)
                {
                    _file = value;
                    NotifyPropertyChanged("File");
                }
            }
        }

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

        public string Type
        {
            get { return _type; }
            set
            {
                if (value != _type)
                {
                    _type = value;
                    NotifyPropertyChanged("Type");
                }
            }
        }

        #endregion

        #region methods

        private void ParseElement(XElement element)
        {
            if (!element.HasElements)
            {
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            string value = null;
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if (FILE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _file = value;
                }
                else if (URL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _url = value;
                }
                else if (TYPE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    _type = value;
                }
            }  
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
