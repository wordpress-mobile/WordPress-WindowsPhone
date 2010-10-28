using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Settings;

namespace WordPress
{
    public partial class PreferencesPage : PhoneApplicationPage
    {
        #region member variables

        private const string USETAGLINEFORNEWPOSTS_VALUE = "useTaglineForNewPosts";
        private const string TAGLINE_VALUE = "tagline";

        private ApplicationBarIconButton _cancelIconButton;
        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public PreferencesPage()
        {
            InitializeComponent();

            DataContext = new UserSettings();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _cancelIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.cancel.png", UriKind.Relative));
            _cancelIconButton.Text = _localizedStrings.ControlsText.Cancel;
            _cancelIconButton.Click += OnCancelButtonClick;
            ApplicationBar.Buttons.Add(_cancelIconButton);

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);
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

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
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

            settings.Save();
        }

        #endregion
    }
}