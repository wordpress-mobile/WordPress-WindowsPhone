using Microsoft.Phone.Controls;
using System;
using System.Windows;
using WordPress.Commands;
using WordPress.Model;
using WordPress.Utils;

namespace WordPress
{
    public partial class LocateBlogPage : PhoneApplicationPage
    {
        #region constructors

        public LocateBlogPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnCreateNewBlogButtonClick(object sender, RoutedEventArgs e)
        {
           LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
           command.Execute(Constants.WORDPRESS_SIGNUP_URL); 
        }

        private void OnExistingWPBlogButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddExistingWordPressBlogPage.xaml", UriKind.Relative));
        }

        private void OnExistingWPSiteButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddExistingWordPressSitePage.xaml", UriKind.Relative));
        }

        private void OnPageLoaded(object sender, RoutedEventArgs args)
        {
            CrashReporter.CheckForPreviousException();
            
            /*UserSettings settings = new UserSettings();
            if (!settings.AcceptedEula)
            {
                eulaControl.Visibility = Visibility.Visible;
            }*/

            PushNotificationsHelper.Instance.checkPushNotificationsUserPermissions();
        }
        #endregion

    }
}