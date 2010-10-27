using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Controls;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class PreferencesPage : PhoneApplicationPage
    {
        #region member variables

        private const string USETAGLINEFORNEWPOSTS_VALUE = "useTaglineForNewPosts";
        private const string TAGLINE_VALUE = "tagline";

        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public PreferencesPage()
        {
            InitializeComponent();

            DataContext = new UserSettings();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
        }

        #endregion

        #region methods

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //check for transient data stored in page state
            if (State.ContainsKey(USETAGLINEFORNEWPOSTS_VALUE))
            {
                RestorePageState();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SavePageState();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            NavigationService.GoBack();
        }

        /// <summary>
        /// Retrieves transient data from the page's State dictionary
        /// </summary>
        private void RestorePageState()
        {
            UserSettings settings = new UserSettings();
            if (State.ContainsKey(USETAGLINEFORNEWPOSTS_VALUE))
            {
                bool useTagline = (bool)State[USETAGLINEFORNEWPOSTS_VALUE];
                settings.UseTaglineForNewPosts = useTagline;
            }

            if (State.ContainsKey(TAGLINE_VALUE))
            {
                string tagline = (string)State[TAGLINE_VALUE];
                if (string.IsNullOrEmpty(tagline))
                {
                    tagline = _localizedStrings.ControlsText.DefaultTagline;
                    settings.UseTaglineForNewPosts = false;
                }
                settings.Tagline = tagline;
            }

            DataContext = settings;
        }

        /// <summary>
        /// Stores transient data in the page's State dictionary
        /// </summary>
        private void SavePageState()
        {
            if (State.ContainsKey(USETAGLINEFORNEWPOSTS_VALUE))
            {
                State.Remove(USETAGLINEFORNEWPOSTS_VALUE);
            }
            State.Add(USETAGLINEFORNEWPOSTS_VALUE, useTaglineCheckbox.IsChecked);

            if (State.ContainsKey(TAGLINE_VALUE))
            {
                State.Remove(TAGLINE_VALUE);
            }
            State.Add(TAGLINE_VALUE, taglineTextBox.Text);
        }

        /// <summary>
        /// Persists the user's preferences to isolated storage
        /// </summary>
        private void SaveSettings()
        {
            UserSettings settings = DataContext as UserSettings;

            if (string.IsNullOrEmpty(settings.Tagline))
            {
                settings.Tagline = _localizedStrings.ControlsText.DefaultTagline;
                settings.UseTaglineForNewPosts = false;
            }

            settings.Save();
        }

        #endregion
    }
}