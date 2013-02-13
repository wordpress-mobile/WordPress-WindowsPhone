using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using WordPress.Commands;
using WordPress.Converters;
using WordPress.Localization;
using WordPress.Model;
using WordPress.Utils;

namespace WordPress
{
    enum CommentsListFilter {
        All,
        Approved,
        Unapproved,
        Spam
    };

    public partial class BlogPanoramaPage : PhoneApplicationPage
    {

        #region member variables

        private List<string> _refreshListOptions;
        private List<string> _postListOptions;
        private List<string> _pageListOptions;
        private List<string> _draftListOptions;

        private object currentXMLRPCConnection = null; //keeps a reference to the latest 'blocking' XML-RPC connection

        private bool _blogIsPinned = false;
        private System.Windows.Data.CollectionViewSource commentsCollectionViewSource;
        private bool _isModeratingComments;
        private CommentsListFilter _currentCommentFilter;
        private StringTable _localizedStrings;
        private SelectionChangedEventHandler _popupServiceSelectionChangedHandler;
        

        private ApplicationBarIconButton _pinBlogIconButton;
        private ApplicationBarIconButton _unpinBlogIconButton;
        private ApplicationBarIconButton _addIconButton;
        private ApplicationBarIconButton _refreshIconButton;

        private ApplicationBarIconButton _moderateIconButton;
        private ApplicationBarIconButton _spamIconButton;
        private ApplicationBarIconButton _approveIconButton;
        private ApplicationBarIconButton _unapproveIconButton;

        private ApplicationBarMenuItem _delMenuItem;
        private ApplicationBarMenuItem _filterAllMenuItem;
        private ApplicationBarMenuItem _filterApprovedMenuItem;
        private ApplicationBarMenuItem _filterUnapprovedMenuItem;
        private ApplicationBarMenuItem _filterSpamMenuItem;

        #endregion


        #region events

        #endregion


        #region constructors

        public BlogPanoramaPage()
        {
            InitializeComponent();

#if WINPHONE8
            panoramaBlogTitle.Margin = new Thickness(0, 12, 0, 0);
            commentsTitle.Margin = new Thickness(0, -36, 0, 25);
            postsTitle.Margin = new Thickness(0, -36, 0, 25);
            pagesTitle.Margin = new Thickness(0, -36, 0, 25);
#endif
            DataContext = App.MasterViewModel;

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            _refreshListOptions = new List<string>(3);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Comments);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Posts);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Pages);
            
            _postListOptions = new List<string>(4);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_ViewPost);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_ViewComments);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_EditPost);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_DeletePost);

            _draftListOptions = new List<string>(2);
            _draftListOptions.Add(_localizedStrings.Options.PostOptions_EditDraft);
            _draftListOptions.Add(_localizedStrings.Options.PostOptions_DeleteDraft);

            _pageListOptions = new List<string>(4);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_ViewPage);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_ViewComments);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_EditPage);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_DeletePage);

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _pinBlogIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.pin.png", UriKind.Relative)); // todo: icon
            _pinBlogIconButton.Text =_localizedStrings.ControlsText.Pin;
            _pinBlogIconButton.Click += OnPinIconButtonClick;

            _unpinBlogIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.unpin.png", UriKind.Relative)); // todo: icon
            _unpinBlogIconButton.Text = _localizedStrings.ControlsText.Unpin;
            _unpinBlogIconButton.Click += OnUnpinIconButtonClick;

            _addIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addIconButton.Text = _localizedStrings.ControlsText.Add;
            _addIconButton.Click += OnAddIconButtonClick;

            _refreshIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.refresh.png", UriKind.Relative));
            _refreshIconButton.Text = _localizedStrings.ControlsText.Refresh;
            _refreshIconButton.Click += OnRefreshIconButtonClick;

            // comment moderation
            _moderateIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.moderate.list.png", UriKind.Relative));
            _moderateIconButton.Text = _localizedStrings.ControlsText.Moderate;
            _moderateIconButton.Click += OnModerateIconButtonClick;

            _spamIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.spam.png", UriKind.Relative));
            _spamIconButton.Text = _localizedStrings.ControlsText.Spam;
            _spamIconButton.Click += OnSpamIconButtonClick;

            _approveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.approve.png", UriKind.Relative));
            _approveIconButton.Text = _localizedStrings.ControlsText.Approve;
            _approveIconButton.Click += OnApproveIconButtonClick;

            _unapproveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.unapprove.png", UriKind.Relative));
            _unapproveIconButton.Text = _localizedStrings.ControlsText.Unapprove;
            _unapproveIconButton.Click += OnUnapproveIconButtonClick;

            _delMenuItem = new ApplicationBarMenuItem();
            _delMenuItem.Text = _localizedStrings.ControlsText.DeleteSelected;
            _delMenuItem.Click += OnDeleteMenuItemClick;

            _filterAllMenuItem = new ApplicationBarMenuItem();
            _filterAllMenuItem.Text = _localizedStrings.ControlsText.FilterAll;
            _filterAllMenuItem.Click += OnFilterAllMenuItemClick;

            _filterApprovedMenuItem = new ApplicationBarMenuItem();
            _filterApprovedMenuItem.Text = _localizedStrings.ControlsText.FilterApproved;
            _filterApprovedMenuItem.Click += OnFilterApprovedMenuItemClick;

            _filterUnapprovedMenuItem = new ApplicationBarMenuItem();
            _filterUnapprovedMenuItem.Text = _localizedStrings.ControlsText.FilterUnapproved;
            _filterUnapprovedMenuItem.Click += OnFilterUnapprovedMenuItemClick;

            _filterSpamMenuItem = new ApplicationBarMenuItem();
            _filterSpamMenuItem.Text = _localizedStrings.ControlsText.FilterSpam;
            _filterSpamMenuItem.Click += OnFilterSpamMenuItemClick;

            blogPanorama.SelectionChanged += OnBlogPanoramaSelectionChanged;

            Loaded += OnPageLoaded;
            postsScrollerView.Loaded += enableInfiniteScrolling;
            pagesScrollerView.Loaded += enableInfiniteScrolling;
            commentsScrollerView.Loaded += enableInfiniteScrolling;
        }

        #endregion


        #region methods

        private void enableInfiniteScrolling(object sender, RoutedEventArgs args)
        {
            if (sender is ScrollViewer == false) return;
            ScrollViewer sv = (ScrollViewer)sender;
            if (sv != null)
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(group_CurrentStateChanging);
                    }
                }
            }
        }

        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            Control ctrl = e.Control;
            if (ctrl is ScrollViewer)
            {
                if (e.NewState.Name == "NotScrolling")
                {
                    //check the position 
                    ScrollViewer currScroller = (ScrollViewer)ctrl;
                    this.DebugLog(ctrl.Name + "->VerticalOffset: " + currScroller.VerticalOffset);
                    this.DebugLog(ctrl.Name + "->ScrollableHeight: " + currScroller.ScrollableHeight);

                    // The vertical offset of the multiselectlist mightnot match the scrollableheight. Give it a little extra room.
                    if (currScroller.ScrollableHeight > 0 && (currScroller.VerticalOffset == currScroller.ScrollableHeight ))
                        loadMoreItems(currScroller);
                }
            }
        }

        private void loadMoreItems(ScrollViewer currScroller)
        {
            if (currScroller.Name == "postsScrollerView")
            {
                this.DebugLog("LoadingMorePosts");

                if (App.MasterViewModel.CurrentBlog.IsLoadingPosts == true) return;

                if (DataService.Current.FetchCurrentBlogPostsAsync(true))
                    DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;
            }
            else if (currScroller.Name == "pagesScrollerView")
            {
                this.DebugLog("LoadingMorePages");

                if (App.MasterViewModel.CurrentBlog.IsLoadingPages == true) return;

                if (DataService.Current.FetchCurrentBlogPagesAsync(true))
                    DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;
            }
            else if (currScroller.Name == "commentsScrollerView")
            {
                this.DebugLog("LoadingMoreComments");
                FetchComments(true);
            }
        }

        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            // check if blog is pinned (on background thread to prevent blocking the UI)
            var worker = new BackgroundWorker();
            worker.DoWork += (workSender, e) =>
            {
                _blogIsPinned = (App.MasterViewModel.FindBlogTile() != null);
            }; 
            worker.RunWorkerCompleted += (completeSender, e) => RefreshAppBar();
            worker.RunWorkerAsync();

            // we don't want the data-binding to delay load of the overall panorama,
            // so put the bindings at the end of the UI thread's task queue instead
            // of putting them in the XAML (where they get evalutead at XAML instantiation)
            Application.Current.RootVisual.Dispatcher.BeginInvoke(SetPanoramaListDataBindings);
        }

        private void SetPanoramaListDataBindings()
        {
            commentsCollectionViewSource = new System.Windows.Data.CollectionViewSource();
            commentsCollectionViewSource.Source = App.MasterViewModel.CurrentBlog.Comments;
            commentsListBox.ItemsSource = commentsCollectionViewSource.View;
            //commentsListBox.SetBinding(MultiselectList.ItemsSourceProperty, new System.Windows.Data.Binding("Comments"));
            postsListBox.SetBinding(ListBox.ItemsSourceProperty, new System.Windows.Data.Binding("Posts"));
            pagesListBox.SetBinding(ListBox.ItemsSourceProperty, new System.Windows.Data.Binding("Pages"));
        }

        private void RefreshAppBar()
        {
            Tools.LogMemoryUsage();
            // Set the app bar based on which pivot item is visible
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();

            if (blogPanorama.SelectedItem == pagesPanoramaItem || blogPanorama.SelectedItem == postsPanoramaItem)
            {
                ApplicationBar.Buttons.Add(_addIconButton);
                ApplicationBar.Buttons.Add(_refreshIconButton);
            }
            else if (blogPanorama.SelectedItem == commentsPanoramaItem)
            {
                if (_isModeratingComments)
                {
                    ApplicationBar.Buttons.Add(_moderateIconButton);
                    ApplicationBar.Buttons.Add(_approveIconButton);
                    ApplicationBar.Buttons.Add(_unapproveIconButton);
                    ApplicationBar.Buttons.Add(_spamIconButton);

                    ApplicationBar.MenuItems.Add(_delMenuItem);
                }
                else
                {
                    ApplicationBar.Buttons.Add(_moderateIconButton);
                    ApplicationBar.Buttons.Add(_refreshIconButton);
                }

                // Don't show the current filter to conserve space.
                if (_currentCommentFilter != CommentsListFilter.All)
                    ApplicationBar.MenuItems.Add(_filterAllMenuItem);

                if (_currentCommentFilter != CommentsListFilter.Approved)
                    ApplicationBar.MenuItems.Add(_filterApprovedMenuItem);

                if (_currentCommentFilter != CommentsListFilter.Unapproved)
                    ApplicationBar.MenuItems.Add(_filterUnapprovedMenuItem);

                if (_currentCommentFilter != CommentsListFilter.Spam)
                    ApplicationBar.MenuItems.Add(_filterSpamMenuItem);

            }
            else if (blogPanorama.SelectedItem == actionsPanoramaItem)
            {
                if (_blogIsPinned)
                {
                    ApplicationBar.Buttons.Add(_unpinBlogIconButton);
                }
                else
                {
                    ApplicationBar.Buttons.Add(_pinBlogIconButton);
                }
            }

            ApplicationBar.IsVisible = ApplicationBar.Buttons.Count > 0;
        }

        private void OnBlogPanoramaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAppBar();
        }

        private void OnPinIconButtonClick(object sender, EventArgs e)
        {
            // check to see if tile already exists
            ShellTile existingTile = App.MasterViewModel.FindBlogTile();

            if (null == existingTile)
            {
                StandardTileData BlogTile = new StandardTileData
                {
                    Title = App.MasterViewModel.CurrentBlog.BlogName,
                    BackgroundImage = new Uri("wp-tile.png", UriKind.Relative)
                };

                _blogIsPinned = true;
                ShellTile.Create(App.MasterViewModel.BuildBlogTileUrl(App.MasterViewModel.CurrentBlog), BlogTile);
                RefreshAppBar();
            }
        }

        private void OnUnpinIconButtonClick(object sender, EventArgs e)
        {
            ShellTile existingTile = App.MasterViewModel.FindBlogTile();

            if (null != existingTile)
            {
                existingTile.Delete();
                _blogIsPinned = false;
                RefreshAppBar();
            }
        }

        private void OnAddIconButtonClick(object sender, EventArgs e)
        {
            if (blogPanorama.SelectedItem == postsPanoramaItem)
            {
                OnCreatePostButtonClick(sender, null);
            }
            else if (blogPanorama.SelectedItem == pagesPanoramaItem)
            {
                OnCreatePageButtonClick(sender, null);
            }
        }

        private void OnRefreshIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }

            if (blogPanorama.SelectedItem == commentsPanoramaItem)
            {
                FetchComments(false);
            }
            else if (blogPanorama.SelectedItem == postsPanoramaItem)
            {
                //The blog is already loading posts.
                if (App.MasterViewModel.CurrentBlog.IsLoadingPosts == true) return;

                //syncs postFormats, options (and maybe other stuff in the future) after the refresh of the posts list
                DataService.Current.FetchComplete += OnFetchCurrentBlogPostsComplete;
                DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;
                DataService.Current.FetchCurrentBlogPostsAsync(false);
            }
            else if (blogPanorama.SelectedItem == pagesPanoramaItem)
            {
                //The blog is already loading pages.
                if (App.MasterViewModel.CurrentBlog.IsLoadingPages == true) return;

                if (DataService.Current.FetchCurrentBlogPagesAsync(false))
                    DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;
            }
        }

        private void OnDataStoreFetchExceptionOccurred(object sender, ExceptionEventArgs args)
        {
            App.WaitIndicationService.HideIndicator();
            ApplicationBar.IsVisible = true;
            currentXMLRPCConnection = null;
            DataService.Current.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;

            this.HandleException(args.Exception);
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BlogSettingsPage.xaml", UriKind.Relative));
        }

        private void OnStatsButtonClick(object sender, RoutedEventArgs e)
        {

            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            else
            {
                if (App.MasterViewModel.CurrentBlog.isWPcom() || App.MasterViewModel.CurrentBlog.hasJetpack())
                {
                    NavigationService.Navigate(new Uri("/ViewStatsPage.xaml", UriKind.Relative));
                    return;
                }

                //Not a WPCOM blog and JetPack 1.8.2 or higher is installed on the site. Show the error message.
                if (!App.MasterViewModel.CurrentBlog.hasJetpack())
                {
                    MessageBoxResult result = MessageBox.Show(_localizedStrings.Messages.JetpackNotAvailable, _localizedStrings.PageTitles.Error, MessageBoxButton.OKCancel);
                    if (MessageBoxResult.OK == result) //start the browser
                    {
                        LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
                        command.Execute(Constants.JETPACK_SITE_URL);
                    }
                }
            }
        }

        #endregion


        #region Posts methods

        private void OnFetchCurrentBlogPostsComplete(object sender, EventArgs args)
        {
            DataService.Current.FetchComplete -= OnFetchCurrentBlogPostsComplete;
            DataService.Current.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred; //do now show exceptions for these XML-RPCS
            DataService.Current.FetchCurrentBlogAdditionalDataAsync();
        }

        private void OnPostsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == postsListBox.SelectedItem) return;
            ListBox postListBox = (ListBox)sender;
            PostListItem postListItem = (PostListItem)postListBox.SelectedItem;
            if (postListItem.PostId.Equals("-1", StringComparison.Ordinal))
                PresentPostOptions(true);
            else
                PresentPostOptions(false);
        }

        private void PresentPostOptions(bool isLocalDraft)
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.PostActions;
            if (isLocalDraft)
            {
                App.PopupSelectionService.ItemsSource = _draftListOptions;
                _popupServiceSelectionChangedHandler = OnDraftOptionSelected;
                App.PopupSelectionService.SelectionChanged += OnDraftOptionSelected;
            }
            else
            {
                //No local draft, check the connection status first
                if (!App.isNetworkAvailable())
                {
                    Exception connErr = new NoConnectionException();
                    this.HandleException(connErr);
                    return;
                }

                App.PopupSelectionService.ItemsSource = _postListOptions;
                _popupServiceSelectionChangedHandler = OnPostOptionSelected;
                App.PopupSelectionService.SelectionChanged += OnPostOptionSelected;
            }
            
            App.PopupSelectionService.ShowPopup(); 
        }

        private void OnPostOptionSelected(object sender, SelectionChangedEventArgs args)
        {
                    
            App.PopupSelectionService.SelectionChanged -= OnPostOptionSelected;
            _popupServiceSelectionChangedHandler = null;

            //recheck the connection status
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }

            int index = _postListOptions.IndexOf(args.AddedItems[0] as string);

            switch (index)
            {
                case 0:         //view post
                    ViewPost();
                    break;
                case 1:         //view post comments
                    ViewPostComments();
                    break;
                case 2:         //edit post
                    EditPost();
                    break;
                case 3:         //delete post
                    DeletePost();
                    break;
            }

            //reset selected index so we can re-select the original list item if we want to
            postsListBox.SelectedIndex = -1;
        }

        private void OnDraftOptionSelected(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnDraftOptionSelected;
            _popupServiceSelectionChangedHandler = null;

            int index = _draftListOptions.IndexOf(args.AddedItems[0] as string);

            switch (index)
            {
                case 0:         //edit draft
                    EditPost();
                    break;
                case 1:         //delete draft
                    DeleteDraft();
                    break;
            }

            //reset selected index so we can re-select the original list item if we want to
            postsListBox.SelectedIndex = -1;
        }

        private void DeleteDraft()
        {
            PostListItem postItem = postsListBox.SelectedItem as PostListItem;
            if (null == postItem) return;

            string prompt = string.Format(_localizedStrings.Prompts.ConfirmDeletePostFormat, postItem.Title);
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.PageTitles.ConfirmDelete, MessageBoxButton.OKCancel);
            if (MessageBoxResult.Cancel == result) return;

            //Make sure the post is available in the LocalPostDrafts list.
            if (App.MasterViewModel.CurrentBlog.LocalPostDrafts.Count > 0 &&
                App.MasterViewModel.CurrentBlog.LocalPostDrafts.Count > postsListBox.SelectedIndex)
            {
                App.MasterViewModel.CurrentBlog.LocalPostDrafts.RemoveAt(postsListBox.SelectedIndex);
            }
            
            App.MasterViewModel.CurrentBlog.PostListItems.RemoveAt(postsListBox.SelectedIndex);
        }

        private void ViewPost()
        {
            //use the GetPostRPC to get the Post.PermaLink value, then transmit that Uri to the shell
            PostListItem postListItem = postsListBox.SelectedItem as PostListItem;
            if (null == postListItem) return;
        
            GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, postListItem.PostId);
            rpc.Completed += OnViewPostRPCCompleted;
            rpc.ExecuteAsync();

            currentXMLRPCConnection = rpc;
            ApplicationBar.IsVisible = false; //hide the application bar 
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.AcquiringPermalink);
        }

        private void OnViewPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnViewPostRPCCompleted;
            
            currentXMLRPCConnection = null;
            App.WaitIndicationService.KillSpinner();
            ApplicationBar.IsVisible = true;

            if (args.Cancelled)
            { 
            } 
            else if (null == args.Error)
            {
                //DEV NOTE: We could fire off a WebBrowserTask here but in testing with the emulator
                //the browser acts a bit odd if there are already tabs open.  The WebBrowserTask 
                //creates a new tab for the web content, but doesn't automatically
                //open your new tab if other tabs already exist.
                Post post = args.Items[0];
                string queryStringFormat = "?{0}={1}&{2}={3}";
                string queryString = string.Format(queryStringFormat, BrowserShellPage.TARGET_URL, post.PermaLink, BrowserShellPage.REQUIRE_LOGIN, "1");
                NavigationService.Navigate(new Uri("/BrowserShellPage.xaml" + queryString, UriKind.Relative));
            }
            else
            {                
                this.HandleException(args.Error);
            }
        }
         
        private void EditPost()
        {
            int index = postsListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPostListItem = App.MasterViewModel.Posts[index];

            if (App.MasterViewModel.CurrentPostListItem.PostId.Equals("-1", StringComparison.Ordinal))
            {
                // Local draft, set current post so post editor knows what to load

                //Make sure the post is available in the LocalPostDrafts list.
                if (App.MasterViewModel.CurrentBlog.LocalPostDrafts.Count <= 0 ||
                    App.MasterViewModel.CurrentBlog.LocalPostDrafts.Count <= index)
                {
                    MessageBox.Show("Sorry, local draft could not be loaded!");
                    return;
                }
             
                App.MasterViewModel.CurrentPostListItem.DraftIndex = index;
                NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
            }
            else
            {
                //not a local draft, download the content from the server
                PostListItem postListItem = postsListBox.SelectedItem as PostListItem;
                if (null == postListItem) return;
                
                ApplicationBar.IsVisible = false; //hide the application bar 
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPost);
                
                GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, postListItem.PostId);
                rpc.Completed += OnGetPostRPCCompleted;
                rpc.ExecuteAsync();

                currentXMLRPCConnection = rpc;
            }
        }

        private void OnGetPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPostRPCCompleted;
            App.WaitIndicationService.KillSpinner();
            ApplicationBar.IsVisible = true;
            currentXMLRPCConnection = null;
            
            if (args.Cancelled)
            { 
            } 
            else if (null == args.Error)
            {
                Post post = args.Items[0];
                App.MasterViewModel.CurrentPost = post;
                NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
            }
            else
            {
                this.HandleException(args.Error);
            }
        }

        private void DeletePost()
        {           
            PostListItem postItem = postsListBox.SelectedItem as PostListItem;
            if (null == postItem) return;

            string prompt = string.Format(_localizedStrings.Prompts.ConfirmDeletePostFormat, postItem.Title);
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.PageTitles.ConfirmDelete, MessageBoxButton.OKCancel);
            if (MessageBoxResult.Cancel == result) return;

            Post post = new Post();
            post.PostId = postItem.PostId;

            DeletePostRPC rpc = new DeletePostRPC(App.MasterViewModel.CurrentBlog, post);
            rpc.Completed += OnDeletePostRPCCompleted;
            rpc.ExecuteAsync();

            ApplicationBar.IsVisible = false; //hide the application bar 
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingPost);
            currentXMLRPCConnection = rpc;
        }

        private void OnDeletePostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.HideIndicator();
            currentXMLRPCConnection = null;

            DeletePostRPC rpc = sender as DeletePostRPC;
            rpc.Completed -= OnDeletePostRPCCompleted;

            if (args.Cancelled)
            {
            }
            else if (null == args.Error)
            {
                string postId = args.Items[0].PostId;
                var postListItem = App.MasterViewModel.CurrentBlog.PostListItems.Single(item => postId.Equals(item.PostId));
                App.MasterViewModel.CurrentBlog.PostListItems.Remove(postListItem);
            }
            else
            {
                this.HandleException(args.Error);
            }
        }

        private void OnCreatePostButtonClick(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPostListItem = null;
            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
        }

        #endregion


        #region Comments methods

        private void FetchComments(bool more)
        {
            //The blog is already loading comments.
            if (App.MasterViewModel.CurrentBlog.IsLoadingComments == true) return;

            if(DataService.Current.FetchCurrentBlogCommentsAsync(more))
                DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;
        }

        private void ViewPostComments()
        {
            int index = postsListBox.SelectedIndex;
            if (-1 == index) return;

            PostListItem postListItem = postsListBox.SelectedItem as PostListItem;
            string queryStringFormat = "?{0}={1}";
            string queryString = string.Format(queryStringFormat, RelatedCommentsPage.IDKEY_VALUE, postListItem.PostId);
            NavigationService.Navigate(new Uri("/RelatedCommentsPage.xaml" + queryString, UriKind.Relative));
        }

        private void OnModerateCommentsButtonClick(object sender, RoutedEventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            NavigationService.Navigate(new Uri("/ModerateCommentsPage.xaml", UriKind.Relative));
        }

        private void CommentListItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            if (commentsListBox.IsSelectionEnabled)
            {

                MultiselectItem container = commentsListBox.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext) as MultiselectItem;
                if (null != container)
                {
                    container.IsSelected = !container.IsSelected;
                }
                // When the last item is unchecked the list disables its selection state automatically. :/
                commentsListBox.IsSelectionEnabled = _isModeratingComments;
            }
            else
            {
                App.MasterViewModel.CurrentComment = (Comment)((FrameworkElement)sender).DataContext;
                NavigationService.Navigate(new Uri("/ModerateCommentPage.xaml", UriKind.Relative));

            }
            return;
        }

        private void multiselectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isModeratingComments && commentsListBox.SelectedItems.Count > 0)
            {
                // Treat this like any other tap.
                App.MasterViewModel.CurrentComment = (Comment)commentsListBox.SelectedItems[0];
                NavigationService.Navigate(new Uri("/ModerateCommentPage.xaml", UriKind.Relative));
            }

            // when all items are unselected the selection mode automatically turns off
            commentsListBox.IsSelectionEnabled = _isModeratingComments;
        }

        private void OnModerateIconButtonClick(object sender, EventArgs e)
        {
            _isModeratingComments = !_isModeratingComments;
            commentsListBox.IsSelectionEnabled = _isModeratingComments;

            RefreshAppBar();
        }

        private void OnFilterAllMenuItemClick(object sender, EventArgs e)
        {
            _currentCommentFilter = CommentsListFilter.All;
            removeAllcommentsFilters();
            RefreshAppBar();
        }
        
        private void OnFilterApprovedMenuItemClick(object sender, EventArgs e)
        {
            _currentCommentFilter = CommentsListFilter.Approved;
            removeAllcommentsFilters();
            commentsCollectionViewSource.Filter += new System.Windows.Data.FilterEventHandler(FilterApproved);
            RefreshAppBar();
        }

        private void OnFilterUnapprovedMenuItemClick(object sender, EventArgs e)
        {
            _currentCommentFilter = CommentsListFilter.Unapproved;
            removeAllcommentsFilters();
            commentsCollectionViewSource.Filter += new System.Windows.Data.FilterEventHandler(FilterUnapproved);
            RefreshAppBar();
        }

        private void OnFilterSpamMenuItemClick(object sender, EventArgs e)
        {
            _currentCommentFilter = CommentsListFilter.Spam;
            removeAllcommentsFilters();
            commentsCollectionViewSource.Filter += new System.Windows.Data.FilterEventHandler(FilterSpam);
            RefreshAppBar();
        }

        // Comments Filters
        private void FilterApproved(object sender, FilterEventArgs e)
        {
            Comment comment = e.Item as Comment;
            if (comment.Status != "approve")
            {
                e.Accepted = false;
            }
        }

        private void FilterUnapproved(object sender, FilterEventArgs e)
        {
            Comment comment = e.Item as Comment;
            if (comment.Status != "hold")
            {
                e.Accepted = false;
            }
        }

        private void FilterSpam(object sender, FilterEventArgs e)
        {
            Comment comment = e.Item as Comment;
            if (comment.Status != "spam")
            {
                e.Accepted = false;
            }
        }

        private void removeAllcommentsFilters()
        {
            commentsCollectionViewSource.Filter -= new System.Windows.Data.FilterEventHandler(FilterApproved);
            commentsCollectionViewSource.Filter -= new System.Windows.Data.FilterEventHandler(FilterUnapproved);
            commentsCollectionViewSource.Filter -= new System.Windows.Data.FilterEventHandler(FilterSpam);
        }


        // Moderation Actions
        private void OnApproveIconButtonClick(object sender, EventArgs e)
        {
            BatchApproveComments(commentsListBox.SelectedItems);
        }

        private void OnUnapproveIconButtonClick(object sender, EventArgs e)
        {
            BatchUnapproveComments(commentsListBox.SelectedItems);
        }

        private void OnSpamIconButtonClick(object sender, EventArgs e)
        {
            BatchSpamComments(commentsListBox.SelectedItems);
        }

        private void OnDeleteMenuItemClick(object sender, EventArgs e)
        {
            BatchDeleteComments(commentsListBox.SelectedItems);
        }

        // XMLRPC Actions
        private void BatchApproveComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;
            IList<Comment> list = ConvertList(comments, eCommentStatus.approve);
            if (0 == list.Count) return;

            EditCommentsStatusRPC rpc = new EditCommentsStatusRPC();
            rpc.CommentStatus = eCommentStatus.approve;
            rpc.Comments = list;
            rpc.Completed += OnBatchEditXmlRPCCompleted;
            rpc.ExecuteAsync();

            currentXMLRPCConnection = rpc;
            ApplicationBar.IsVisible = false; //hide the application bar 
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.ApprovingComments);
        }

        private void BatchUnapproveComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;
            IList<Comment> list = ConvertList(comments, eCommentStatus.hold);
            if (0 == list.Count) return;

            EditCommentsStatusRPC rpc = new EditCommentsStatusRPC();
            rpc.CommentStatus = eCommentStatus.hold;
            rpc.Comments = list;
            rpc.Completed += OnBatchEditXmlRPCCompleted;
            rpc.ExecuteAsync();

            currentXMLRPCConnection = rpc;
            ApplicationBar.IsVisible = false; //hide the application bar 
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UnapprovingComments);
        }

        private void BatchSpamComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;
            IList<Comment> list = ConvertList(comments, eCommentStatus.spam);
            if (0 == list.Count) return;

            string comment_label = (1 == comments.Count) ? _localizedStrings.Prompts.Comment : _localizedStrings.Prompts.Comments;
            string prompt = string.Format(_localizedStrings.Prompts.ConfirmMarkSpamCommentsFormat, comments.Count, comment_label);
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.Confirm, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                EditCommentsStatusRPC rpc = new EditCommentsStatusRPC();
                rpc.CommentStatus = eCommentStatus.spam;
                rpc.Comments = list;
                rpc.Completed += OnBatchEditXmlRPCCompleted;
                rpc.ExecuteAsync();

                currentXMLRPCConnection = rpc;
                ApplicationBar.IsVisible = false; //hide the application bar 
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.MarkingCommentsAsSpam);
            }
        }

        private void BatchDeleteComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

            string comment_label = (1 == comments.Count) ? _localizedStrings.Prompts.Comment : _localizedStrings.Prompts.Comments;
            string prompt = string.Format(_localizedStrings.Prompts.ConfirmDeleteCommentsFormat, comments.Count, comment_label);
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.Confirm, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                DeleteCommentsRPC rpc = new DeleteCommentsRPC();
                rpc.Comments = ConvertList(comments);
                rpc.Completed += OnBatchDeleteXmlRPCCompleted;
                rpc.ExecuteAsync();

                currentXMLRPCConnection = rpc;
                ApplicationBar.IsVisible = false; //hide the application bar 
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingComments);
            }
        }

        private IList<Comment> ConvertList(IList comments)
        {
            List<Comment> result = new List<Comment>();
            foreach (Comment c in comments)
            {
                result.Add(c);
            }
            return result;
        }

        private IList<Comment> ConvertList(IList comments, eCommentStatus statusToExclude)
        {
            List<Comment> result = new List<Comment>();
            foreach (Comment c in comments)
            {
                if (statusToExclude != c.CommentStatus)
                {
                    result.Add(c);
                }
            }
            return result;
        }

        private void OnBatchEditXmlRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            EditCommentsStatusRPC rpc = sender as EditCommentsStatusRPC;
            rpc.Completed -= OnBatchEditXmlRPCCompleted;
            currentXMLRPCConnection = null;
            App.WaitIndicationService.HideIndicator();
            ApplicationBar.IsVisible = true;

            //switch back to 'single mode'. 
            _isModeratingComments = false;
            commentsListBox.IsSelectionEnabled = _isModeratingComments;
            RefreshAppBar();
        }

        private void OnBatchDeleteXmlRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            DeleteCommentsRPC rpc = sender as DeleteCommentsRPC;
            rpc.Completed -= OnBatchDeleteXmlRPCCompleted;

            currentXMLRPCConnection = null;
            App.WaitIndicationService.HideIndicator();
            ApplicationBar.IsVisible = true;

            //switch back to 'single mode'. 
            _isModeratingComments = false;
            commentsListBox.IsSelectionEnabled = _isModeratingComments;
            RefreshAppBar();
        }

        #endregion


        #region Pages methods

        private void OnPagesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == pagesListBox.SelectedItem) return;

            ListBox pageListBox = (ListBox)sender;
            PageListItem pageListItem = (PageListItem)pageListBox.SelectedItem;
            if (pageListItem.PageId.Equals("-1", StringComparison.Ordinal))
                PresentPageOptions(true);
            else
                PresentPageOptions(false);
        }

        private void PresentPageOptions(bool isLocalDraft)
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.PostActions;
            if (isLocalDraft)
            {
                App.PopupSelectionService.ItemsSource = _draftListOptions;
                _popupServiceSelectionChangedHandler = OnPageDraftOptionSelected;
                App.PopupSelectionService.SelectionChanged += OnPageDraftOptionSelected;
            }
            else
            {
                if (!App.isNetworkAvailable())
                {
                    Exception connErr = new NoConnectionException();
                    this.HandleException(connErr);
                    return;
                }
                App.PopupSelectionService.ItemsSource = _pageListOptions;
                _popupServiceSelectionChangedHandler = OnPageOptionSelected;
                App.PopupSelectionService.SelectionChanged += OnPageOptionSelected;
            }

            App.PopupSelectionService.ShowPopup();
        }

        private void OnPageOptionSelected(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnPageOptionSelected;
            _popupServiceSelectionChangedHandler = null;

            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }

            int index = _pageListOptions.IndexOf(args.AddedItems[0] as string);

            switch (index)
            {
                case 0:         //view page
                    ViewPage();
                    break;
                case 1:         //view page comments
                    ViewPageComments();
                    break;
                case 2:         //edit page
                    EditPage();
                    break;
                case 3:         //delete page
                    DeletePage();
                    break;
            }

            //reset selected index so we can re-select the original list item if we want to
            pagesListBox.SelectedIndex = -1;
        }

        private void OnPageDraftOptionSelected(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnPageDraftOptionSelected;
            _popupServiceSelectionChangedHandler = null;

            int index = _draftListOptions.IndexOf(args.AddedItems[0] as string);

            switch (index)
            {
                case 0:         //edit draft
                    EditPage();
                    break;
                case 1:         //delete draft
                    DeletePageDraft();
                    break;
            }

            //reset selected index so we can re-select the original list item if we want to
            postsListBox.SelectedIndex = -1;
        }

        private void DeletePageDraft()
        {
            PageListItem pageItem = pagesListBox.SelectedItem as PageListItem;
            if (null == pageItem) return;

            string prompt = string.Format(_localizedStrings.Prompts.ConfirmDeletePostFormat, pageItem.PageTitle);
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.PageTitles.ConfirmDelete, MessageBoxButton.OKCancel);
            if (MessageBoxResult.Cancel == result) return;

            //Make sure the page is available in the LocalPageDrafts list.
            if (App.MasterViewModel.CurrentBlog.LocalPageDrafts.Count > 0 &&
                App.MasterViewModel.CurrentBlog.LocalPageDrafts.Count > pagesListBox.SelectedIndex)
            {
                App.MasterViewModel.CurrentBlog.LocalPageDrafts.RemoveAt(pagesListBox.SelectedIndex);
            }

            App.MasterViewModel.CurrentBlog.PageListItems.RemoveAt(pagesListBox.SelectedIndex);
        }

        private void EditPage()
        {
            int index = pagesListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPageListItem = App.MasterViewModel.Pages[index];

            if (App.MasterViewModel.CurrentPageListItem.PageId.Equals("-1", StringComparison.Ordinal))
            {
                // Local draft, set current page DraftIndex so editor knows what to load

                //Make sure the page is available in the LocalPostDrafts list.
                if (App.MasterViewModel.CurrentBlog.LocalPageDrafts.Count <= 0 ||
                    App.MasterViewModel.CurrentBlog.LocalPageDrafts.Count <= index)
                {
                    MessageBox.Show("Sorry, local draft could not be loaded!");
                    return;
                }

                App.MasterViewModel.CurrentPageListItem.DraftIndex = index;
                NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
            }
            else
            {
                //not a local draft, download the content from the server
                PageListItem pageListItem = pagesListBox.SelectedItem as PageListItem;
                if (null == pageListItem) return;

                GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, pageListItem.PageId.ToString());
                rpc.Completed += OnGetPageRPCCompleted;
                rpc.ExecuteAsync();

                currentXMLRPCConnection = rpc;
                ApplicationBar.IsVisible = false;
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPage);
            }
        }

        private void OnGetPageRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPageRPCCompleted;

            App.WaitIndicationService.KillSpinner();

            if (args.Cancelled)
            {
            }
            else if (null == args.Error)
            {
                Post post = args.Items[0];
                App.MasterViewModel.CurrentPost = post;
                NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
            }
            else
            {
                this.HandleException(args.Error);
            }
        }


        private void ViewPage()
        {
            //use the GetPostRPC to get the Post.PermaLink value, then transmit that Uri to the shell

            PageListItem pageListItem = pagesListBox.SelectedItem as PageListItem;
            if (null == pageListItem) return;

            GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, pageListItem.PageId.ToString());
            rpc.Completed += OnViewPageRPCCompleted;
            rpc.ExecuteAsync();

            currentXMLRPCConnection = rpc;
            ApplicationBar.IsVisible = false;
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.AcquiringPermalink);
        }

        private void OnViewPageRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnViewPageRPCCompleted;

            App.WaitIndicationService.KillSpinner();
            ApplicationBar.IsVisible = true;
            currentXMLRPCConnection = null;

            if (args.Cancelled)
            {
            }
            else if (null == args.Error)
            {
                //DEV NOTE: We could fire off a WebBrowserTask here but in testing with the emulator
                //the browser acts a bit odd if there are already tabs open.  The WebBrowserTask 
                //creates a new tab for the web content, but doesn't automatically
                //open your new tab if other tabs already exist.

                Post post = args.Items[0];
                Uri permaLinkUri = new Uri(post.PermaLink, UriKind.Absolute);
                string queryStringFormat = "?{0}={1}&{2}={3}";
                string paramString = string.Format(queryStringFormat, BrowserShellPage.TARGET_URL, permaLinkUri.ToString(), BrowserShellPage.REQUIRE_LOGIN, "1");
                NavigationService.Navigate(new Uri("/BrowserShellPage.xaml" + paramString, UriKind.Relative));
            }
            else
            {
                this.HandleException(args.Error);
            }            
        }

        private void ViewPageComments()
        {
            int index = pagesListBox.SelectedIndex;
            if (-1 == index) return;

            PageListItem pageListItem = pagesListBox.SelectedItem as PageListItem;
            string queryStringFormat = "?{0}={1}";
            string queryString = string.Format(queryStringFormat, RelatedCommentsPage.IDKEY_VALUE, pageListItem.PageId);
            NavigationService.Navigate(new Uri("/RelatedCommentsPage.xaml" + queryString, UriKind.Relative));
        }

        private void DeletePage()
        {
            PageListItem pageListItem = pagesListBox.SelectedItem as PageListItem;
            if (null == pageListItem) return;

            string prompt = string.Format(_localizedStrings.Prompts.ConfirmDeletePageFormat, pageListItem.PageTitle);
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.PageTitles.ConfirmDelete, MessageBoxButton.OKCancel);
            if (MessageBoxResult.Cancel == result) return;

            Post post = new Post();
            post.PostId = pageListItem.PageId;

            DeletePageRPC rpc = new DeletePageRPC(App.MasterViewModel.CurrentBlog, post);
            rpc.Completed += OnDeletePageRPCCompleted;
            rpc.ExecuteAsync();

            currentXMLRPCConnection = rpc;
            ApplicationBar.IsVisible = false; //hide the application bar 
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingPage);
        }

        private void OnDeletePageRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            DeletePageRPC rpc = sender as DeletePageRPC;
            rpc.Completed -= OnDeletePageRPCCompleted;

            if (args.Cancelled)
            {
            }
            else if (null == args.Error)
            {              
                string pageId = args.Items[0].PostId;
                var pageListItem = App.MasterViewModel.CurrentBlog.PageListItems.Single(item => pageId.Equals(item.PageId));
                App.MasterViewModel.CurrentBlog.PageListItems.Remove(pageListItem);
            }
            else
            {
                this.HandleException(args.Error);
            }

            currentXMLRPCConnection = null;
            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.HideIndicator();
        }

        private void OnCreatePageButtonClick(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPageListItem = null;
            NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
        }

        #endregion

        
        #region Navigation methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("Blog"))
            {
                // navigated from secondary tile
                string blogXmlrpc = this.NavigationContext.QueryString["Blog"];
                Blog blog = App.MasterViewModel.Blogs.FirstOrDefault(b => b.Xmlrpc == blogXmlrpc);

                if (null != blog)
                {
                    App.MasterViewModel.CurrentBlog = blog;
                }
                else
                {
                    // hm... blog index is no longer valid. delete the tile and quit the app
                    ShellTile OldTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("Blog=" + blogXmlrpc));
                    OldTile.Delete();
                    NavigationService.GoBack();
                }
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            DataService.Current.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.WaitIndicationService.Waiting)
            {
                App.WaitIndicationService.HideIndicator();
                ApplicationBar.IsVisible = true;
                e.Cancel = true;

                if (currentXMLRPCConnection != null) //Really ugly, I know. But don't want change the XML-RPC class in this release.
                {
                    if (currentXMLRPCConnection is GetPostRPC)
                    {
                        (currentXMLRPCConnection as GetPostRPC).IsCancelled = true;
                /*        (currentXMLRPCConnection as GetPostRPC).Completed -= OnViewPostRPCCompleted;
                        (currentXMLRPCConnection as GetPostRPC).Completed -= OnViewPageRPCCompleted;
                        (currentXMLRPCConnection as GetPostRPC).Completed -= OnGetPostRPCCompleted;*/
                    }
                    else if (currentXMLRPCConnection is DeletePostRPC)
                    {
                        (currentXMLRPCConnection as DeletePostRPC).IsCancelled = true;
                    }
                    else if (currentXMLRPCConnection is DeletePageRPC)
                    {
                        (currentXMLRPCConnection as DeletePageRPC).IsCancelled = true;
                    }
                    else if (currentXMLRPCConnection is EditCommentsStatusRPC)
                    {
                        (currentXMLRPCConnection as EditCommentsStatusRPC).Completed -= OnBatchEditXmlRPCCompleted;
                    }
                    else if (currentXMLRPCConnection is DeleteCommentsRPC)
                    {
                        (currentXMLRPCConnection as DeleteCommentsRPC).Completed -= OnBatchDeleteXmlRPCCompleted;
                    }

                    currentXMLRPCConnection = null;
                }       
            } else if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.HidePopup();
                App.PopupSelectionService.SelectionChanged -= _popupServiceSelectionChangedHandler;
                ApplicationBar.IsVisible = true;
                //make sure none of the list items are selected, allowing the user to re-select
                //an item in the list.  This will trigger the SelectionChanged event
                //commentsListBox.SelectedIndex = -1;
                // TODO: Deselect the comment list box
                postsListBox.SelectedIndex = -1;
                pagesListBox.SelectedIndex = -1;

                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
        
        #endregion

    }
}