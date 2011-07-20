using System;
using System.Windows;
using Microsoft.Phone.Controls;

using WordPress.Localization;
using System.Net;
using System.IO;
using WordPress.Model;
using System.Text;
using WordPress.Utils;
using System.Collections.Generic;

using System.Linq;


namespace WordPress
{
    public partial class BrowserShellPage : PhoneApplicationPage
    {
        #region member variables

        public const string URIKEYNAME = "uri";
        public const string ITEM_PERMALINK = "permaLink";

        private StringTable _localizedStrings;
        private string _uriString; //one between _uriString / _itemPermaLink must be available
        public static string _itemPermaLink;

        #endregion

        #region constructors

        public BrowserShellPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            Loaded += OnPageLoaded;
        }

        #endregion

        #region browser_methods
        //The WebBrowser class events are raised in the following order: Navigating, Navigated, and LoadCompleted.

        private void OnBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            if (!string.IsNullOrEmpty(_uriString) || !string.IsNullOrEmpty(_itemPermaLink))
            {
                progressBar.Visibility = Visibility.Visible;
            }
        }

        private void OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            progressBar.Visibility = Visibility.Collapsed;
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

            this.NavigationContext.QueryString.TryGetValue(ITEM_PERMALINK, out _itemPermaLink);
        }

        private void OnBrowserLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_uriString) && string.IsNullOrEmpty(_itemPermaLink)) return;

            if (!string.IsNullOrEmpty(_uriString))
                browser.Navigate(new Uri(_uriString, UriKind.Absolute));
            else if (!string.IsNullOrEmpty(_itemPermaLink)) //preview of posts
            {
                progressBar.Visibility = Visibility.Visible;
                this.startLoadingPostUsingLoginForm();
            }
        }
        
        private void startLoadingPostUsingLoginForm() {
            string xmlrpcURL = App.MasterViewModel.CurrentBlog.Xmlrpc.Replace("/xmlrpc.php", "/wp-login.php");
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(xmlrpcURL);
                CookieContainer container = new CookieContainer();    
                request.CookieContainer = container;
                request.AllowAutoRedirect = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = XmlRPCRequestConstants.POST;
                request.UserAgent = Constants.WORDPRESS_USERAGENT;

                request.BeginGetRequestStream(asynchronousResult =>
                {
                    HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;

                    try
                    {
                        Stream contentStream = null;
                        contentStream = webRequest.EndGetRequestStream(asynchronousResult);

                        string postData = String.Format("log={0}&pwd={1}&redirect_to={2}", HttpUtility.UrlEncode(App.MasterViewModel.CurrentBlog.Username),
                                HttpUtility.UrlEncode(App.MasterViewModel.CurrentBlog.Password), HttpUtility.UrlEncode(_itemPermaLink));

                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                        using (contentStream)
                        {
                            contentStream.Write(byteArray, 0, byteArray.Length);
                        }

                        webRequest.BeginGetResponse(OnBeginGetResponseCompleted, webRequest);
                    }
                    catch (Exception ex)
                    {
                        this.HandleException(new Exception("Something went wrong during Preview", ex));
                    }
                }, request);
    
        }


        private void OnBeginGetResponseCompleted(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = asynchronousResult.AsyncState as HttpWebRequest;
            HttpWebResponse response = null;

            try
            {
                response = request.EndGetResponse(asynchronousResult) as HttpWebResponse;
    
                Stream responseStream = response.GetResponseStream();
                string responseContent = null;
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseContent = reader.ReadToEnd();
                }

                UIThread.Invoke(() =>
                {
                    browser.NavigateToString(responseContent);
                });

                try { response.Close(); } catch (Exception ex) { /* no error here*/ }
            }
            catch (Exception ex)
            {
                this.HandleException(new Exception("Something went wrong during Preview", ex));
            }
        }
        #endregion
    }
}