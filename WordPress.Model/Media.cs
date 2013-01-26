using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;

namespace WordPress.Model
{

    public enum eMediaPlacement
    {
        BlogPreference, // Default (0) should use the blog level preference. 
        Before,
        After
    }

    public class Media : INotifyPropertyChanged
    {
        #region class constants
        static public string MEDIA_IMAGE_DIRECTORY = "draft_media";
        #endregion

        #region member variables

        private const string JPEG_EXTENSION = ".jpg";
        private const string PNG_EXTENSION = ".png";
        private const string BMP_EXTENSION = ".bmp";

        private string _localPath; //location of the media on the device
        private DateTime _datetime;
        private string _id;
        private string _url;       //once uploaded it contains the URL of the image on the server   
        private string _mimetype;  //mime type
        private string _fileName;  //preferred file name to set when upload the image on disk. It's the same of the filename on the device for now.
        private eMediaPlacement _placement; // User specified placement of the image before or after the post/page content.

        private bool _alignThumbnailToCenter = false;
        private bool _createLinkToFullImage = false;
        private bool _isfeatured = false; // If the media should be the featured image when uploaded to a post.
        private bool _canBeFeatured = true; // Whether its possible to feature this media file. When adding to a page this should be set to false. 

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region constructors
        
        public Media() { } //used to de-serialize the Media

        public Media(Blog blog, string fileName, string fileLocationInMediaLibrary, DateTime date) {
            _alignThumbnailToCenter = blog.AlignThumbnailToCenter;
            _createLinkToFullImage = blog.CreateLinkToFullImage;
            _fileName = fileName;
            _localPath = fileLocationInMediaLibrary;
            _datetime = date;
            TranslateMimeType();
        }

        public Media(Blog blog, string filename, string localfilename, Stream stream, bool preserveBandwidth)
        {
            _alignThumbnailToCenter = blog.AlignThumbnailToCenter;
            _createLinkToFullImage = blog.CreateLinkToFullImage;
            _fileName = filename;
            _localPath = localfilename;
            SaveImageStream(stream, preserveBandwidth);
            TranslateMimeType();
        }

        #endregion


        #region properties

        public eMediaPlacement placement
        {

            get { return _placement; }
            set
            {
                if (value != _placement)
                {
                    _placement = value;
                    NotifyPropertyChanged("Placement");
                }
            }
        }

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

        public bool AlignThumbnailToCenter
        {
            get { return _alignThumbnailToCenter; }
            set
            {
                if (value != _alignThumbnailToCenter)
                {
                    _alignThumbnailToCenter = value;
                    NotifyPropertyChanged("AlignThumbnailToCenter");
                }
            }
        }

        public bool CreateLinkToFullImage
        {
            get { return _createLinkToFullImage; }
            set
            {
                if (value != _createLinkToFullImage)
                {
                    _createLinkToFullImage = value;
                    NotifyPropertyChanged("CreateLinkToFullImage");
                }
            }
        }

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

        public DateTime DateTime
        {
            get { return _datetime; }
            set
            {
                if (value != _datetime)
                {
                    _datetime = value;
                    NotifyPropertyChanged("DateTime");
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

        public bool IsFeatured
        {
            get { return _isfeatured; }
            set
            {
                if (value != _isfeatured)
                {
                    _isfeatured = value;
                    NotifyPropertyChanged("IsFeatured");
                }
            }
        }

        public bool CanBeFeatured
        {
            get { return _canBeFeatured; }
            set
            {
                if (value != _canBeFeatured)
                {
                    _canBeFeatured = value;
                    NotifyPropertyChanged("CanBeFeatured");
                }
            }
        }

        #endregion


        #region methods

        static public void CleanupOrphanedImages()
        {
            // Get a list of all the draft media images in IsolatedStorage.
            string mediaDir = Media.MEDIA_IMAGE_DIRECTORY;
            string[] mediaFiles = null;
            var isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.DirectoryExists(mediaDir))
            {
                string searchPath = Path.Combine(mediaDir, "*");
                mediaFiles = isoStore.GetFileNames(searchPath);
            }

            if (mediaFiles == null || mediaFiles.Length == 0)
                return;

            // Remove from the list any media file that belongs to a draft post.
            List<string> orphanedImages = new List<string>(mediaFiles);
            foreach (Blog b in DataService.Current.Blogs)
            {
                List<Post> posts = new List<Post>(b.LocalPostDrafts);
                posts.AddRange(b.LocalPageDrafts);

                foreach (Post p in posts)
                {
                    foreach (Media m in p.Media)
                    {
                        string localPath = Path.GetFileName(m.LocalPath);
                        if (orphanedImages.Contains(localPath))
                        {
                            // This image is not an orphan so remove it from our list.
                            orphanedImages.Remove(localPath);
                        }
                    }
                }
            }

            // Delete the remaining orphans.
            foreach (string image in orphanedImages)
            {
                try
                {
                    isoStore.DeleteFile(Path.Combine(mediaDir, image));
                }
                catch (Exception)
                {
                }
            }

        }

        public void clearSavedImage()
        {
            try
            {
                var isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (isoStore.FileExists(_localPath))
                {
                    isoStore.DeleteFile(_localPath);
                }
            }
            catch (Exception){}
        }

        private void SaveImageStream(Stream stream, bool preserveBandwidth)
        {
            BitmapImage image = new BitmapImage();
            image.SetSource(stream);

            int pixelWidth = image.PixelWidth;
            int pixelHeight = image.PixelHeight;
             
            // Resize the image if we need to preserve bandwidth
            if (preserveBandwidth && (pixelWidth > 800 || pixelHeight > 800))
            {
                float wRatio = image.PixelWidth / 800F;
                float hRatio = image.PixelHeight / 800F;
                float currentRatio = Math.Max(wRatio, hRatio);
                pixelWidth = (int)(image.PixelWidth / currentRatio);
                pixelHeight = (int)(image.PixelHeight / currentRatio);
            }

            // Save to isolated storage.
            var isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists(_localPath))
            {
                // Just in case of collisions.
                isoStore.DeleteFile(_localPath);
            }

            string dirname = Path.GetDirectoryName(_localPath);
            if (!isoStore.DirectoryExists(dirname))
            {
                isoStore.CreateDirectory(dirname);
            }
            IsolatedStorageFileStream filestream = isoStore.CreateFile(_localPath);
            WriteableBitmap wb = new WriteableBitmap(image);
            wb.SaveJpeg(filestream, pixelWidth, pixelHeight, 0, 85);
            filestream.Close();
        }

        public Stream getImageStream()
        {
            // Check if the image was saved in IsolatedStorage or if we're retriving 
            // an Picture from the MediaLibrary.
            var isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists(_localPath))
            {
                IsolatedStorageFileStream filestream = isoStore.OpenFile(this.LocalPath, FileMode.Open);
                return filestream;
            }            
            else
            {
                // Load the picture: Ugly but works.
                MediaLibrary m = new MediaLibrary();

                int len = m.Pictures.Count;
                for (int i = 0; i < len; i++)
                {
                    try
                    {
                        var r = m.Pictures[i];
                        if (r.Name.Equals(LocalPath) && r.Date.Equals(_datetime))
                            return r.GetImage();
                    }
                    catch (Exception e)
                    {
                        // See trac #170. 
                        // If the enumerator throws an error getting the image catch it and continue through the list. 
                        // This assumes that a corrupted file could cause the enumerator to throw an invalid operation exception.
                        continue;
                    }
                }
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
