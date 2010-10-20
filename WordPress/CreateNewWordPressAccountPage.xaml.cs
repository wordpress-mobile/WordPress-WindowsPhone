using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

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

        private void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("LoadCompleted");
        }

        private void browser_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loaded");
            browser.Navigate(new Uri(WORDPRESSSIGNUP_VALUE), null, USERAGENT_VALUE);
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("Navigated");
            Debug.WriteLine(string.Format("Uri: {0}", e.Uri));
        }

        private void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            Debug.WriteLine("Navigating");
        }

        private void browser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Debug.WriteLine("ScriptNotify");
        }

        #endregion
    }
}