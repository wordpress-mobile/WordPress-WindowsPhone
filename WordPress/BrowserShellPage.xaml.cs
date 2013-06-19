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

        public const string TARGET_URL = "targetURL";
        public const string REQUIRE_LOGIN = "loginReq";

        private StringTable _localizedStrings;
        public string _targetURL;
        public string _requireLogin;

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

        private void OnBrowserLoaded(object sender, RoutedEventArgs e)
        {
            this.DebugLog("OnBrowserLoaded");

            if (string.IsNullOrEmpty(_targetURL)) //no target URL defined
            {
                MessageBox.Show("Can't open the page.\nNo target URL defined.");
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                return;
            }

            //Check that _targetURL is a valid URI
            Uri testUri;
            bool canExecute = Uri.TryCreate(_targetURL, UriKind.Absolute, out testUri);
            if(!canExecute)
            {
                MessageBox.Show("Can't open the page.\nInvalid address: "+_targetURL);
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                return;
            }

            if (string.IsNullOrEmpty(_requireLogin) || _requireLogin == "0")
            {
                browser.Navigate(testUri);
            }
            else
            {
                progressBar.Visibility = Visibility.Visible;
                this.startLoadingPostUsingLoginForm();
            }
        }
                
        //The WebBrowser class events are raised in the following order: Navigating, Navigated, and LoadCompleted.
        private void OnBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            this.DebugLog("OnBrowserNavigating");
            if ( !string.IsNullOrEmpty(_targetURL) )
            {
                progressBar.Visibility = Visibility.Visible;
            }
        }

        private void OnBrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DebugLog("OnBrowserNavigated");
        }

        private void OnBrowserLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DebugLog("OnBrowserLoadCompleted");
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
                progressBar.Visibility = Visibility.Collapsed;
            } 

            base.OnBackKeyPress(e);            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //save the uri in member _uriString and wait for the browser element load.
            //once that happens we can tell the browser to start loading the uri
            this.NavigationContext.QueryString.TryGetValue(REQUIRE_LOGIN, out _requireLogin);
            if (this.NavigationContext.QueryString.TryGetValue(TARGET_URL, out _targetURL))
            {
            }
            else
            {
                this._targetURL = null;
            }
        }

        private void startLoadingPostUsingLoginForm() {


            if (string.IsNullOrEmpty(_targetURL) || string.IsNullOrEmpty(App.MasterViewModel.CurrentBlog.Password)
                || string.IsNullOrEmpty(App.MasterViewModel.CurrentBlog.Username) || string.IsNullOrEmpty(App.MasterViewModel.CurrentBlog.loginURL()))
            {
                MessageBox.Show("Can't open the page!");
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                return;
            }

            string responseContent = "<head>"
             + "<script type=\"text/javascript\">"
             + "function submitform(){document.loginform.submit();} </script>"
             + "</head>"
             + "<body onload=\"submitform()\">"
             + "<form style=\"visibility:hidden;\" name=\"loginform\" id=\"loginform\" action=\"" + App.MasterViewModel.CurrentBlog.loginURL() + "\" method=\"post\">"
             + "<input type=\"text\" name=\"log\" id=\"user_login\" value=\"" + App.MasterViewModel.CurrentBlog.Username + "\"/></label>"
             + "<input type=\"password\" name=\"pwd\" id=\"user_pass\" value=\"" + App.MasterViewModel.CurrentBlog.Password + "\" /></label>"
             + "<input type=\"submit\" name=\"wp-submit\" id=\"wp-submit\" value=\"Log In\" />"
             + "<input type=\"hidden\" name=\"redirect_to\" value=\"" + System.Uri.EscapeUriString(_targetURL) + "\" />"
             + "</form>"
             + "</body>";

            UIThread.Invoke(() =>
            {
                browser.NavigateToString(responseContent);
            });
       
        }
                
        /*
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

                String testUrlDecoded = HttpUtility.UrlDecode(_itemPermaLink);
                String testUrlEncoded = HttpUtility.UrlEncode(testUrlDecoded); //sould be used only on get request? 
                String testSystemUriEscapeDataString = System.Uri.EscapeDataString(testUrlDecoded);
                String testSystemUriEscapeUriString = System.Uri.EscapeUriString(testUrlDecoded); //Good for URL encoding
                this.DebugLog(_itemPermaLink);
                this.DebugLog(testUrlDecoded);
                this.DebugLog(testUrlEncoded);
                this.DebugLog(testSystemUriEscapeDataString);
                this.DebugLog(testSystemUriEscapeUriString);
                if (_itemPermaLink.Equals(testSystemUriEscapeDataString))
                {
                    this.DebugLog("_itemPermaLink.Equals(testSystemUriEscapeDataString)");
                }
                if (_itemPermaLink.Equals(testSystemUriEscapeUriString))
                {
                    this.DebugLog("_itemPermaLink.Equals(testSystemUriEscapeUriString)");
                }

                try
                {
                    Stream contentStream = null;
                    contentStream = webRequest.EndGetRequestStream(asynchronousResult);
                       
                    string postData = String.Format("log={0}&pwd={1}&redirect_to={2}", HttpUtility.UrlEncode(App.MasterViewModel.CurrentBlog.Username),
                            HttpUtility.UrlEncode(App.MasterViewModel.CurrentBlog.Password),  HttpUtility.UrlEncode(_itemPermaLink));
                    
                    byte[] byteArray =Encoding.UTF8.GetBytes(postData);
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

                try { response.Close(); } catch (Exception ex) {  }
            }
            catch (Exception ex)
            {
                this.HandleException(new Exception("Something went wrong during Preview", ex));
            }
        }

        */


        #endregion
    }
}