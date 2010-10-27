using System.ComponentModel;
using System.IO.IsolatedStorage;

namespace WordPress.Model
{
    /// <summary>
    /// Provides access to strongly-typed user settings.  This class is a wrapper
    /// around the IsolatedStorageSettings object; this can be changed later if 
    /// a different storage mechanism is desired.
    /// </summary>
    public class UserSettings: INotifyPropertyChanged
    {
        #region Member variables
                
        private const string USETAGLINEFORNEWPOSTS_VALUE = "useTaglineForNewPosts";
        private const string TAGLINE_VALUE = "tagline";

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public UserSettings()
        {
            Settings = IsolatedStorageSettings.ApplicationSettings;
        }

        #endregion

        #region properties

        private IsolatedStorageSettings Settings { get; set; }

        public bool UseTaglineForNewPosts
        {
            get
            {
                bool result = true;

                if (Settings.Contains(USETAGLINEFORNEWPOSTS_VALUE))
                {
                    result = (bool)Settings[USETAGLINEFORNEWPOSTS_VALUE];
                }
                return result;
            }

            set
            {
                bool oldValue = UseTaglineForNewPosts;
                if (oldValue != value)
                {
                    Settings[USETAGLINEFORNEWPOSTS_VALUE] = value;
                    NotifyPropertyChanged("UseTaglineForNewPosts");
                }
            }
        }

        public string Tagline
        {
            get
            {
                string result = string.Empty;

                if (Settings.Contains(TAGLINE_VALUE))
                {
                    result = (string)Settings[TAGLINE_VALUE];
                }

                return result;
            }

            set
            {
                string oldValue = Tagline;
                if (oldValue != value)
                {
                    Settings[TAGLINE_VALUE] = value;
                    NotifyPropertyChanged("Tagline");
                }
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Persists changes to storage
        /// </summary>
        public void Save()
        {
            Settings.Save();
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
