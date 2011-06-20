using System;
using System.Windows;
using Microsoft.Phone.Controls;

using WordPress.Localization;

namespace WordPress
{
    public partial class BrowserShellPage : PhoneApplicationPage
    {
        #region member variables

        public const string URIKEYNAME = "uri";

        private StringTable _localizedStrings;
        private string _uriString;

        #endregion

        #region constructors

        public BrowserShellPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (progressBar.Visibility == Visibility.Visible)
            {
                progressBar.Visibility = Visibility.Visible;
            } 

            base.OnBackKeyPress(e);            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //save the uri in member _uriString and wait for the browser element load.
            //once that happens we can tell the browser to start loading the uri
            this.NavigationContext.QueryString.TryGetValue(URIKEYNAME, out _uriString);
        }

        private void OnBrowserLoaded(object sender, RoutedEventArgs e)
        {            
            if (string.IsNullOrEmpty(_uriString)) return;

            browser.Navigate(new Uri(_uriString, UriKind.Absolute));            
        }

        private void OnBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            if (!string.IsNullOrEmpty(_uriString))
            {
                progressBar.Visibility = Visibility.Visible;
            }
        }

        private void OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            progressBar.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}