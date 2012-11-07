using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Media;

namespace WordPress.Model
{
    public class Media : INotifyPropertyChanged
    {
        #region member variables

        private const string JPEG_EXTENSION = ".jpg";
        private const string PNG_EXTENSION = ".png";
        private const string BMP_EXTENSION = ".bmp";

        private string _localPath; //location of the media on the device
        private DateTime _datetime;

        private string _url;       //once uploaded it contains the URL of the image on the server   
        private string _mimetype;  //mime type
        private string _fileName;  //preferred file name to set when upload the image on disk. It's the same of the filename on the device for now.

        private bool _alignThumbnailToCenter = false;
        private bool _createLinkToFullImage = false;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public Media(Blog blog, string fileName, string fileLocationInMediaLibrary, DateTime date) {
            _alignThumbnailToCenter = blog.AlignThumbnailToCenter;
            _createLinkToFullImage = blog.CreateLinkToFullImage;
            _fileName = fileName;
            _localPath = fileLocationInMediaLibrary;
            _datetime = date;
            TranslateMimeType();
        }

        #endregion


        #region properties

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    NotifyPropertyChanged("FileName");
                    TranslateMimeType();
                }
            }
        }

        public string LocalPath
        {
            get { return _localPath; }
            set
            {
                if (value != _localPath)
                {
                    _localPath = value;
                    NotifyPropertyChanged("LocalPath");
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

        public string MimeType
        {
            get { return _mimetype; }
            set
            {
                if (value != _mimetype)
                {
                    _mimetype = value;
                    NotifyPropertyChanged("MimeType");
                }
            }
        }

        #endregion


        #region methods

        public Stream getImageStream()
        {
            //load the picture: Ugly but works.
            MediaLibrary m = new MediaLibrary();
            foreach (var r in m.Pictures)
            {
                if (r.Name.Equals(LocalPath) && r.Date.Equals(_datetime))
                    return r.GetImage();
            }

            return null;
        }

        public string getHTML()
        {
           
            XElement imageNode = new XElement("img");
            imageNode.SetAttributeValue("src", this.Url);

            if (_alignThumbnailToCenter)
            {
                StringBuilder styleBuilder = new StringBuilder();
                styleBuilder.Append("display:block; margin-right:auto; margin-left:auto;");
                imageNode.SetAttributeValue("style", styleBuilder.ToString());
                imageNode.SetAttributeValue("class", "size-full;");
            }
            else
            {
                imageNode.SetAttributeValue("class", "alignnone size-full;");
            }

            if (!_createLinkToFullImage)
            {
                return "<br /><br />" + imageNode.ToString();
            }

            XElement anchorNode = new XElement("a");
            anchorNode.SetAttributeValue("href", this.Url);
            anchorNode.Add(imageNode);

            return "<br /><br />" + anchorNode.ToString();
        }


        private void TranslateMimeType()
        {
            //DEV NOTE: PhotoChooserTask only seems to allow pictures, no video.
            //capture everything else (if that is even possible) as
            //application/octet-stream
            string extension = Path.GetExtension(LocalPath); 

            if (JPEG_EXTENSION.Equals(extension))
            {
                MimeType = MimeTypes.JPEG;
            }
            else if( PNG_EXTENSION.Equals(extension) )
            {
                MimeType = MimeTypes.PNG;
            }
            else if ( BMP_EXTENSION.Equals(extension) )
            {
                MimeType = MimeTypes.BMP;
            }
            else
            {
                MimeType = MimeTypes.UNKNOWN;
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
