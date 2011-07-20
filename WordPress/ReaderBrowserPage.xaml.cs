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
using System.Windows.Media;
using Microsoft.Phone.Shell;


namespace WordPress
{
    public partial class ReaderBrowserPage : PhoneApplicationPage
    {
        #region member variables

        public const string READER = "reader";

        private StringTable _localizedStrings;
        public static string _reader;

        private ApplicationBarIconButton _backButtonIconButton;
        private ApplicationBarIconButton _likeButtonIconButton;
        private ApplicationBarIconButton _reblogButtonIconButton;

        Stack<Uri> history = new Stack<Uri>();
        Uri current = null; 
 
        #endregion

        #region constructors

        public ReaderBrowserPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            Loaded += OnPageLoaded;
        }

        #endregion

        #region browser_methods
        //The WebBrowser class events are raised in the following order: Navigating, Navigated, and LoadCompleted.

        private void OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            progressBar.Visibility = Visibility.Collapsed;
            //add the like and re-nlog btn
        }

        //The WebBrowser class events are raised in the following order: Navigating, Navigated, and LoadCompleted.
        private void OnBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            if (!string.IsNullOrEmpty(_reader))
            {
                progressBar.Visibility = Visibility.Visible;
            }
        }


        //The WebBrowser class events are raised in the following order: Navigating, Navigated, and LoadCompleted.
        private void OnBrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Uri previous = null;
            if (history.Count > 0) 
                previous = history.Peek();

            // This assumption is NOT always right. 
            // if the page had a forward reference that creates a loop (e.g. A->B->A ), 
            // we would not detect it, we assume it is an A -> B -> back () 

            if (previous != null && e.Uri == previous)
            {
                history.Pop();
            }
            else
            {
                if (current != null)
                    history.Push(current);
            }
            current = e.Uri;

            if (history.Count > 0)
            {
                ApplicationBar.IsVisible = true;
                ApplicationBar.Opacity = 100.0;
            }
            else
            {
                ApplicationBar.Opacity = 0.5;
                ApplicationBar.IsVisible = false;
            }
        }

        private void OnBackButtonIconButtonClick(object sender, EventArgs e)
        {
            if (history.Count > 0)
            {
                Uri destination = history.Peek();
                browser.Navigate(destination);
            }
        }

        private void OnLikeButtonIconButtonClick(object sender, EventArgs e)
        {
            string content = browser.SaveToString();
            string urlLink = null;
            //<a href="<permalink>?like=1&amp;_wpnonce=c93d60f9a3" title="I like this post" class="like sd-button" rel="nofollow"><span>Like</span></a>
            String[] results = content.Split('"');
            foreach (String current in results)
            {
                if (current.Contains("?like=1&") && current.Contains("_wpnonce"))
                {
                    urlLink = current;
                    break;
                }
            }

            if (urlLink != null)
            {
                this.DebugLog(urlLink);
                browser.Navigate(new Uri(urlLink, UriKind.Absolute));
            }
        }

        private void OnReblogButtonIconButtonClick(object sender, EventArgs e)
        {
 

        }
        #endregion

        #region methods

        private void OnBrowserLoaded(object sender, RoutedEventArgs e)
        {
            //init the bottom bar after the webbrowser control otherwise the bottombar space will not be used by the webcontrol
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _backButtonIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.back.png", UriKind.Relative));
            _backButtonIconButton.Text = _localizedStrings.ControlsText.Back;
            _backButtonIconButton.Click += OnBackButtonIconButtonClick;
            ApplicationBar.Buttons.Add(_backButtonIconButton);

            if (!string.IsNullOrEmpty(_reader)) //loading the reader
            {
                progressBar.Visibility = Visibility.Visible;
                this.startLoadingReaderUsingLoginForm();
            }
        }

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
            this.NavigationContext.QueryString.TryGetValue(READER, out _reader);
        }


        private void startLoadingReaderUsingLoginForm()
        {

            //check is there is a WP.COM blog
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            string username = null;
            string pass = null;
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.Xmlrpc.EndsWith("wordpress.com/xmlrpc.php"))
                {
                    username = currentBlog.Username;
                    pass = currentBlog.Password;
                    break;
                }
            }

            string responseContent = "<head>"
                +"<script type=\"text/javascript\">"
                +"function submitform(){document.loginform.submit();} </script>"
                +"</head>"
                +"<body onload=\"submitform()\">"
                + "<form style=\"visibility:hidden;\" name=\"loginform\" id=\"loginform\" action=\"" + Constants.WORDPRESS_LOGIN_URL + "\" method=\"post\">"
                + "<input type=\"text\" name=\"log\" id=\"user_login\" value=\""+username+"\"/></label>"
                + "<input type=\"password\" name=\"pwd\" id=\"user_pass\" value=\""+pass+"\" /></label>"
                + "<input type=\"submit\" name=\"wp-submit\" id=\"wp-submit\" value=\"Log In\" />"
                + "<input type=\"hidden\" name=\"redirect_to\" value=\""+Constants.WORDPRESS_READER_URL+"\" />"
                + "</form>"
                +"</body>";

            UIThread.Invoke(() =>
            {
                browser.NavigateToString(responseContent);
            });
       
        }
        #endregion
    }
}