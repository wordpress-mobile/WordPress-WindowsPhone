using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Text;

using WordPress.Localization;
using WordPress.Model;
using WordPress.Utils;
using System.ComponentModel;
using Coding4Fun.Toolkit.Controls;

namespace WordPress
{
    public partial class BlogsPage : PhoneApplicationPage
    {
        #region member variables

        private ApplicationBarIconButton _addBlogIconButton;
        private ApplicationBarIconButton _preferencesIconButton;
        private ApplicationBarIconButton _readerIconButton;

        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public BlogsPage()
        {
            InitializeComponent();

            DataContext = App.MasterViewModel;

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _addBlogIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addBlogIconButton.Text = _localizedStrings.ControlsText.AddBlog;
            _addBlogIconButton.Click += OnAddAccountIconButtonClick;
            ApplicationBar.Buttons.Add(_addBlogIconButton);

            _preferencesIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _preferencesIconButton.Text = _localizedStrings.ControlsText.Preferences;
            _preferencesIconButton.Click += OnPreferencesIconButtonClick;
            ApplicationBar.Buttons.Add(_preferencesIconButton);

            ApplicationBarMenuItem deleteBlogMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.DeleteBlog);
            deleteBlogMenuItem.Click += OnDeleteBlogMenuItemClick;
            ApplicationBar.MenuItems.Add(deleteBlogMenuItem);

            ApplicationBarIconButton aboutIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.about.png", UriKind.Relative));
            aboutIconButton.Text = _localizedStrings.ControlsText.About;
            aboutIconButton.Click += OnAboutMenuItemClick;
            ApplicationBar.Buttons.Add(aboutIconButton);

            //check is there is a WP.COM blog
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            bool presence = false;
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.Xmlrpc.EndsWith("wordpress.com/xmlrpc.php"))
                {
                    presence = true;
                    break;
                }
            }
            if (presence)
            {
                _readerIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.reader.png", UriKind.Relative));
                _readerIconButton.Text = _localizedStrings.ControlsText.Reader;
                _readerIconButton.Click += OnReaderIconButtonClick;
                //  ApplicationBar.Buttons.Add(_readerIconButton);
            }

            CrashReporter.CheckForPreviousException();
        }

        #endregion

        #region methods

        private void OnBlogsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = blogsListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentBlog = App.MasterViewModel.Blogs[index];

            if (null != App.MasterViewModel.SharingPhotoToken)
            {
                NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/BlogPanoramaPage.xaml", UriKind.Relative));
            }

            //reset selected index so we can re-select the original list item if we want to
            blogsListBox.SelectedIndex = -1;
        }

        private void OnAddAccountIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            NavigationService.Navigate(new Uri("/LocateBlogPage.xaml", UriKind.Relative));
        }

        private void OnPreferencesIconButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PreferencesPage.xaml", UriKind.Relative));
        }

        private void OnReaderIconButtonClick(object sender, EventArgs e)
        {
            string queryStringFormat = "?{0}={1}";
            string queryString = string.Format(queryStringFormat, ReaderBrowserPage.READER, "GoMobileTeam!");
            NavigationService.Navigate(new Uri("/ReaderBrowserPage.xaml" + queryString, UriKind.Relative));
        }

        private void OnAboutMenuItemClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void OnDeleteBlogMenuItemClick(object sender, EventArgs e)
        {
            PresentSelectionOptions();
        }

        private void PresentSelectionOptions()
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.SelectBlogToDelete;
            App.PopupSelectionService.ItemsSource = DataService.Current.Blogs;
            App.PopupSelectionService.SelectionChanged += OnBlogSelectedForDelete;
            App.PopupSelectionService.ShowPopup();
        }

        private void OnBlogSelectedForDelete(object sender, SelectionChangedEventArgs e)
        {
            App.PopupSelectionService.SelectionChanged -= OnBlogSelectedForDelete;

            if (0 == e.AddedItems.Count) return;

            Blog blogToRemove = e.AddedItems[0] as Blog;
            if (null == blogToRemove) return;

            // remove this blog's tile
            ShellTile blogTile = App.MasterViewModel.FindBlogTile(blogToRemove);
            if (null != blogTile)
            {
                blogTile.Delete();
            }

            DataService.Current.Blogs.Remove(blogToRemove);
            PushNotificationsHelper.Instance.sendBlogsList();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.SelectionChanged -= OnBlogSelectedForDelete;
                e.Cancel = true;
                App.PopupSelectionService.HidePopup();
                return;
            }
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PushNotificationsHelper pHelper = PushNotificationsHelper.Instance;
         
            //there is no way to clear the query string. We must use the PhoneApplicationPage to store the ts and check it before opening the notifications screen 
            bool firstLaunch = false;
            if (!State.ContainsKey("ts"))
            {
                firstLaunch = true;
                State.Add("ts", DateTime.Now);

                pHelper.checkPushNotificationsUserPermissions();

                //Register the deviceURI and send the blogs list to the server, or disable notification and deregister the device from the server.
                pHelper.resetTileCount();
                if (pHelper.pushNotificationsEnabled())
                {
                    pHelper.enablePushNotifications();
                }
                else
                {
                    pHelper.disablePushNotifications();
                }
            }

            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;
            if (queryStrings.ContainsKey("FileId") && queryStrings.ContainsKey("Action") && queryStrings["Action"] == "ShareContent")
            {
                // sharing a photo
                App.MasterViewModel.SharingPhotoToken = queryStrings["FileId"];
            }
            else if ((true == firstLaunch) && queryStrings.ContainsKey("blog_id") && queryStrings.ContainsKey("ts")
                 && queryStrings.ContainsKey("comment_id")  && queryStrings.ContainsKey("action") && queryStrings["action"] == "OpenComment")
            {
                string blogID = queryStrings["blog_id"];
                string commentID = queryStrings["comment_id"];
                System.Diagnostics.Debug.WriteLine("IDs received from PN are - blogID:" + blogID + " commentID:" + commentID);
                pHelper.showCommentScreenFromNotification(blogID, commentID);
                pHelper.resetLastPushNotificationData();
            }
            else if (true == firstLaunch)
            {
                //App was opened by tapping on the Tile. Need to check if there are some notifications pending on the server.
                if (pHelper.pushNotificationsEnabled() && App.isNetworkAvailable())
                {
                    pHelper.loadLastPushNotificationData(OnLoadLastNotificationCompleted);
                    loadingContentProgressBar.Opacity = 1.0;
                }
            }

            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }

        public void OnLoadLastNotificationCompleted(object sender, XMLRPCCompletedEventArgs<IntResponseObject> args)
        {
            UIThread.Invoke(() =>
            {
                loadingContentProgressBar.Opacity = 0.0;
            });

            XmlRemoteProcedureCall<IntResponseObject> rpc = sender as XmlRemoteProcedureCall<IntResponseObject>;
            rpc.Completed -= OnLoadLastNotificationCompleted;

            PushNotificationsHelper.Instance.OnLoadLastNotificationCompleted(sender, args);
        }
        #endregion
    }
}