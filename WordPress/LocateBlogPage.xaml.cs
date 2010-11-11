using System;
using System.Windows;
using Microsoft.Phone.Controls;

using WordPress.Model;
using WordPress.Settings;

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
            string uriString = string.Format("/BrowserShellPage.xaml?uri={0}", Constants.WORDPRESS_SIGNUP_URL);
            NavigationService.Navigate(new Uri(uriString, UriKind.Relative));
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
            UserSettings settings = new UserSettings();
            if (!settings.AcceptedEula)
            {
                eulaControl.Visibility = Visibility.Visible;
            }
        }
        #endregion

    }
}