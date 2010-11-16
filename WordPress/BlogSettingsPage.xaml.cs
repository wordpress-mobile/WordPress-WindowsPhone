using System;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class BlogSettingsPage : PhoneApplicationPage
    {
        #region member variables

        private const string USERNAME_VALUE = "username";
        private const string PASSWORD_VALUE = "password";
        private const string APIKEY_VALUE = "apikey";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructor

        public BlogSettingsPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Blog currentBlog = App.MasterViewModel.CurrentBlog;
            currentBlog.BeginEdit();
            DataContext = currentBlog;

            if (State.ContainsKey(USERNAME_VALUE))
            {
                RestorePageState();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Blog blog = DataContext as Blog;
            if (blog.IsEditing)
            {
                blog.CancelEdit();
            }

            SavePageState();
        }

        /// <summary>
        /// Stores transient data in the page's State dictionary
        /// </summary>
        private void SavePageState()
        {
            if (State.ContainsKey(USERNAME_VALUE))
            {
                State.Remove(USERNAME_VALUE);
            }
            State.Add(USERNAME_VALUE, usernameTextBox.Text);

            if (State.ContainsKey(PASSWORD_VALUE))
            {
                State.Remove(PASSWORD_VALUE);
            }
            State.Add(PASSWORD_VALUE, passwordTextBox.Password);

            if (State.ContainsKey(APIKEY_VALUE))
            {
                State.Remove(APIKEY_VALUE);
            }
            State.Add(APIKEY_VALUE, apikeyTextBox.Password);
        }

        private void RestorePageState()
        {
            Blog blog = DataContext as Blog;

            if (State.ContainsKey(USERNAME_VALUE))
            {
                blog.Username = (string)State[USERNAME_VALUE];
            }
            
            if (State.ContainsKey(PASSWORD_VALUE))
            {
                blog.Password = (string)State[PASSWORD_VALUE];
            }

            if (State.ContainsKey(APIKEY_VALUE))
            {
                apikeyTextBox.Password = (string)State[APIKEY_VALUE];
            }
        }

        public void OnSaveButtonClick(object sender, EventArgs args)
        {
            Blog blog = DataContext as Blog;
            if (blog.IsEditing)
            {
                blog.EndEdit();
            }
            NavigationService.GoBack();
        }

        #endregion
    }
}