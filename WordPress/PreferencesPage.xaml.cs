using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Settings;
using WordPress.Utils;

namespace WordPress
{
    public partial class PreferencesPage : PhoneApplicationPage
    {
        #region member variables

        private const string USETAGLINEFORNEWPOSTS_VALUE = "useTaglineForNewPosts";
        private const string TAGLINE_VALUE = "tagline";
        private const string ENABLE_TOAST_PUSH_NOTIFICATIONS_VALUE = "enableToastPushNotifications";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;
        private bool oldPNsValue = false;

        #endregion

        #region constructors

        public PreferencesPage()
        {
            InitializeComponent();
            UserSettings usettings = new UserSettings();
            DataContext = usettings;

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];
            
            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);

            oldPNsValue = usettings.EnableToastPushNotifications;
            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

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

            if (State.ContainsKey(ENABLE_TOAST_PUSH_NOTIFICATIONS_VALUE))
            {
                bool enabledPNs = (bool)State[ENABLE_TOAST_PUSH_NOTIFICATIONS_VALUE];
                settings.EnableToastPushNotifications = enabledPNs;
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

            if (State.ContainsKey(ENABLE_TOAST_PUSH_NOTIFICATIONS_VALUE))
            {
                State.Remove(ENABLE_TOAST_PUSH_NOTIFICATIONS_VALUE);
            }
            State.Add(ENABLE_TOAST_PUSH_NOTIFICATIONS_VALUE, enablePushNotifications.IsChecked);
        }

        /// <summary>
        /// Persists the user's preferences to isolated storage
        /// </summary>
        private void SaveSettings()
        {
            UserSettings settings = DataContext as UserSettings;
            settings.Save();
            if (settings.EnableToastPushNotifications != oldPNsValue)
            {
                if (settings.EnableToastPushNotifications == true)
                    PushNotificationsHelper.Instance.enablePushNotifications();
                else
                    PushNotificationsHelper.Instance.disablePushNotifications();
            }
        }

        #endregion
    }
}