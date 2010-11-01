using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace WordPress
{
    public partial class BrowserShellPage : PhoneApplicationPage
    {
        #region member variables

        public const string URIKEYNAME = "uri";
        private string _uriString;

        #endregion

        #region constructors

        public BrowserShellPage()
        {
            InitializeComponent();

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.WaitIndicationService.Waiting)
            {
                App.WaitIndicationService.HideIndicator();
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
                //App.WaitIndicationService.ShowIndicator("loading uri...");
            }
        }

        private void OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //the content isn't presented in the UI just yet, so we'll delay hiding
            //the spinner for a bit
            //App.WaitIndicationService.HideIndicator();
        }

        #endregion



        

    }
}