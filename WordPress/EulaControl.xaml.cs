using System.Windows;
using System.Windows.Controls;
using System.IO;

using WordPress.Localization;
using WordPress.Settings;

namespace WordPress
{
    public partial class EulaControl : UserControl
    {
        #region constructors

        public EulaControl()
        {
            // Required to initialize variables
            InitializeComponent();
            acceptButton.Click += OnAcceptButtonClick;
            declineButton.Click += OnDeclineButtonClick;

            Loaded += OnControlLoaded;
        }

        #endregion

        #region methods

        private void OnAcceptButtonClick(object sender, RoutedEventArgs args)
        {
            UserSettings settings = new UserSettings();
            settings.AcceptedEula = true;
            settings.Save();

            Visibility = Visibility.Collapsed;
        }

        private void OnDeclineButtonClick(object sender, RoutedEventArgs args)
        {
            throw new ApplicationShouldEndException();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs args)
        {
            browser.NavigateToString(LocalizedResources.Eula);
        }

        #endregion
    }
}