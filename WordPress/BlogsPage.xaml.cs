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

            //send PNs info in background
            PushNotificationsHelper pHelper = PushNotificationsHelper.Instance;
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

            //there is no way to clear the query string. We must use the PhoneApplicationPage to store the ts and check it before opening the notifications screen 
            bool firstLaunch = false;
            if (! State.ContainsKey("ts"))
            {
                firstLaunch = true;
                State.Add("ts", DateTime.Now);
            }

            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;
            if (queryStrings.ContainsKey("FileId") && queryStrings.ContainsKey("Action") && queryStrings["Action"] == "ShareContent")
            {
                // sharing a photo
                App.MasterViewModel.SharingPhotoToken = queryStrings["FileId"];
            }
            else if ((true == firstLaunch) && queryStrings.ContainsKey("blog_id") && queryStrings.ContainsKey("ts") 
                && queryStrings.ContainsKey("action") && queryStrings["action"] == "OpenComment")
            {
                string blogID = queryStrings["blog_id"];
                System.Diagnostics.Debug.WriteLine("The blogID received from PN is: " + blogID);
                this.openCommentsScreenForBlog(blogID);
            }
            else if (true == firstLaunch)
            {
                //App was opened by tapping on the Tile. Need to check if there are some notifications pending on the server.
                PushNotificationsHelper pHelper = PushNotificationsHelper.Instance;
                if (pHelper.pushNotificationsEnabled())
                {
                    pHelper.loadLastPushNotification(this.OnLoadLastNotificationCompleted);
                }
            }
            
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }

        private void openCommentsScreenForBlog(string blogID)
        {
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    string currentBlogID = currentBlog.isWPcom() ? Convert.ToString(currentBlog.BlogId) : currentBlog.getJetpackClientID();
                    if (currentBlogID == blogID)
                    {
                        App.MasterViewModel.CurrentBlog = currentBlog;
                        App.MasterViewModel.ShowCommentsPageAndRefresh = true;
                        NavigationService.Navigate(new Uri("/BlogPanoramaPage.xaml", UriKind.Relative));
                        return;
                    }
                }
            }
        }

        private void OnLoadLastNotificationCompleted(object sender, XMLRPCCompletedEventArgs<IntResponseObject> args)
        {
            XmlRemoteProcedureCall<IntResponseObject> rpc = sender as XmlRemoteProcedureCall<IntResponseObject>;
            rpc.Completed -= OnLoadLastNotificationCompleted;
            if (null == args.Error && args.Items.Count > 0)
            {
                IntResponseObject respObj = args.Items.First();
                
                if (respObj.Value == 0)
                    return;

                string blogID = string.Format("{0}", respObj.Value);
                string blogName = string.Empty;

                List<Blog> blogs = DataService.Current.Blogs.ToList();
                foreach (Blog currentBlog in blogs)
                {
                    if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                    {
                        string currentBlogID = currentBlog.isWPcom() ? Convert.ToString(currentBlog.BlogId) : currentBlog.getJetpackClientID();
                        if (currentBlogID == blogID)
                        {
                            blogName = currentBlog.BlogName;
                        }
                    }
                }
                
                NestedClass nc = new NestedClass(this, blogID);

                UIThread.Invoke(() =>
                {
                    ToastPrompt toast = new ToastPrompt();
                    toast.Title = "New comment on " + blogName;
                    //toast.Message = "Some message";
                    //toast.ImageSource = new BitmapImage(new Uri("ApplicationIcon.png", UriKind.RelativeOrAbsolute));
                    toast.Tap += nc.toast_Completed;
                    toast.Show();
                });
                return;
            }
            else
            {
                Exception e = args.Error;
                Utils.Tools.LogException(String.Format("Error occurred. {0}", e.Message), e);
            }
        }

        
        private class NestedClass
        {
            string _blogID = string.Empty;
            BlogsPage outerClass = null;

            public NestedClass(BlogsPage outerClass, string blogID)
            {
                this._blogID = blogID;
                this.outerClass = outerClass;
            }

            public void toast_Completed(object sender, System.Windows.Input.GestureEventArgs e)
            {
                this.outerClass.openCommentsScreenForBlog(this._blogID);
            }
        }
        

        #endregion
    }
}