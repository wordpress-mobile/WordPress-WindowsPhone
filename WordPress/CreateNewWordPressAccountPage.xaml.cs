using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Controls;

namespace WordPress
{
    public partial class CreateNewWordPressAccountPage : PhoneApplicationPage
    {
        #region member variables

        private const string WORDPRESSSIGNUP_VALUE = "http://wordpress.com/signup";
        private const string USERAGENT_VALUE = "User-Agent: wp-android";

        #endregion

        #region constructors

        public CreateNewWordPressAccountPage()
        {
            InitializeComponent();            
        }

        #endregion

        #region properties

        #endregion

        #region methods

        private void OnBrowserLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("LoadCompleted");
        }

        private void OnBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loaded");
            browser.Navigate(new Uri(WORDPRESSSIGNUP_VALUE), null, USERAGENT_VALUE);
        }

        private void OnBrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("Navigated");
            Debug.WriteLine(string.Format("Uri: {0}", e.Uri));
        }

        private void OnBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            Debug.WriteLine("Navigating");
        }

        private void OnBrowserScriptNotify(object sender, NotifyEventArgs e)
        {
            Debug.WriteLine("ScriptNotify");
        }

        #endregion
    }
}