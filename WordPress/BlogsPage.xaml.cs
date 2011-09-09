using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;

using WordPress.Localization;
using WordPress.Model;

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
            List<string> blogTitles = new List<string>();
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            blogs.ForEach(blog => blogTitles.Add(blog.BlogName));

            App.PopupSelectionService.Title = _localizedStrings.Prompts.SelectBlogToDelete;
            App.PopupSelectionService.ItemsSource = blogTitles;
            App.PopupSelectionService.SelectionChanged += OnBlogSelectedForDelete;
            App.PopupSelectionService.ShowPopup();
        }

        private void OnBlogSelectedForDelete(object sender, SelectionChangedEventArgs e)
        {
            App.PopupSelectionService.SelectionChanged -= OnBlogSelectedForDelete;

            if (0 == e.AddedItems.Count) return;

            string blogName = e.AddedItems[0] as string;
            if (string.IsNullOrEmpty(blogName)) return;

            Blog blogToRemove = null;
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            if (blogs.Any(blog => blog.BlogName.Equals(blogName)))
            {
                blogToRemove = blogs.Single(blog => blog.BlogName.Equals(blogName));
            }
            if (null == blogToRemove) return;

            int blogIndex = DataService.Current.Blogs.IndexOf(blogToRemove);
            DataService.Current.Blogs.Remove(blogToRemove);

            // remove this blog's tile
            ShellTile blogTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("Blog=" + blogIndex.ToString()));
            if (null != blogTile)
            {
                blogTile.Delete();
            }
            // todo: since tiles are index-based, any blogs with a higher index than the blog that was just deleted now have invalid
            // NavigationUri for their tiles. to solve this, we probably need some other way of uniquely address blogs.
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.SelectionChanged -= OnBlogSelectedForDelete;
                e.Cancel = true;
                return;
            }
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;

            if (queryStrings.ContainsKey("FileId") && queryStrings.ContainsKey("Action") && queryStrings["Action"] == "ShareContent")
            {
                // sharing a photo
                App.MasterViewModel.SharingPhotoToken = queryStrings["FileId"];
            }
        }
        #endregion

    }
}