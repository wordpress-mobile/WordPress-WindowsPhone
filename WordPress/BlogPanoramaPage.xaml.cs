using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using Microsoft.Phone.Controls;

using WordPress.Localization;
using WordPress.Model;
using WordPress.Utils;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace WordPress
{
    public partial class BlogPanoramaPage : PhoneApplicationPage
    {
        #region member variables

        private List<string> _refreshListOptions;
        private List<string> _postListOptions;
        private List<string> _pageListOptions;
        private List<string> _draftListOptions;

        private int _multiFetchTaskCount;
        private bool _blogIsPinned = false;
        private StringTable _localizedStrings;
        private SelectionChangedEventHandler _popupServiceSelectionChangedHandler;

        private ApplicationBarIconButton _pinBlogIconButton;
        private ApplicationBarIconButton _unpinBlogIconButton;
        private ApplicationBarIconButton _addIconButton;
        private ApplicationBarIconButton _refreshIconButton;

        #endregion

        #region events

        #endregion

        #region constructors

        public BlogPanoramaPage()
        {
            InitializeComponent();

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
            _draftListOptions.Add("Edit Draft");
            _draftListOptions.Add("Delete Draft");

            _pageListOptions = new List<string>(4);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_ViewPage);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_ViewComments);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_EditPage);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_DeletePage);

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];
            ApplicationBar.Opacity = 0.5;

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

            blogPanorama.SelectionChanged += OnBlogPanoramaSelectionChanged;

            Loaded += OnPageLoaded;
            postsScrollerView.Loaded += enableInfiniteScrolling;
            //we don't need infinite scroll on page, since we are using wp.getPageList and not getPages
            //pagesScrollerView.Loaded += enableInfiniteScrolling;
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
                    if (currScroller.ScrollableHeight > 0 && currScroller.ScrollableHeight == currScroller.VerticalOffset)
                        loadMoreItems(currScroller);
                }
            }
        }

        private void loadMoreItems(ScrollViewer currScroller)
        {
            if (currScroller.Name == "postsScrollerView")
            {
                this.DebugLog("LoadingMorePosts");
                FetchPosts(true);
            }
            else if (currScroller.Name == "pagesScrollerView")
            {
                this.DebugLog("LoadingMorePages");
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
            commentsListBox.SetBinding(ListBox.ItemsSourceProperty, new System.Windows.Data.Binding("Comments"));
            postsListBox.SetBinding(ListBox.ItemsSourceProperty, new System.Windows.Data.Binding("Posts"));
            pagesListBox.SetBinding(ListBox.ItemsSourceProperty, new System.Windows.Data.Binding("Pages"));
        }

        private void RefreshAppBar()
        {
            Tools.LogMemoryUsage();
            // Set the app bar based on which pivot item is visible
            ApplicationBar.Buttons.Clear();

            if (blogPanorama.SelectedItem == pagesPanoramaItem || blogPanorama.SelectedItem == postsPanoramaItem)
            {
                ApplicationBar.Buttons.Add(_addIconButton);
                ApplicationBar.Buttons.Add(_refreshIconButton);
            }
            else if (blogPanorama.SelectedItem == commentsPanoramaItem)
            {
                ApplicationBar.Buttons.Add(_refreshIconButton);
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
            if (blogPanorama.SelectedItem == commentsPanoramaItem)
            {
                FetchComments(false);
            }
            else if (blogPanorama.SelectedItem == postsPanoramaItem)
            {
                //syncs postFormats (and maybe other stuff in the future) after the refresh of the posts list
                DataService.Current.FetchComplete += OnFetchCurrentBlogPostsComplete;
                FetchPosts(false);
            }
            else if (blogPanorama.SelectedItem == pagesPanoramaItem)
            {
                FetchPages();
            }
        }

        private void OnFetchCurrentBlogPostsComplete(object sender, EventArgs args)
        {
            DataService.Current.FetchComplete -= OnFetchCurrentBlogPostsComplete;
            DataService.Current.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred; //do now show exceptions for this kind of syncs
            DataService.Current.FetchCurrentBlogPostFormatsAsync();    
        }

        private void OnCommentsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = commentsListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentComment = App.MasterViewModel.Comments[index];

            NavigationService.Navigate(new Uri("/ModerateCommentPage.xaml", UriKind.Relative));

            //reset selected index so we can re-select the original list item if we want to
            commentsListBox.SelectedIndex = -1;
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

            App.MasterViewModel.CurrentBlog.LocalPostDrafts.RemoveAt(postsListBox.SelectedIndex);
            App.MasterViewModel.CurrentBlog.PostListItems.RemoveAt(postsListBox.SelectedIndex);
        }

        private void ViewPost()
        {
            //use the GetPostRPC to get the Post.PermaLink value, then transmit that Uri to the shell

            PostListItem postListItem = postsListBox.SelectedItem as PostListItem;
            if (null == postListItem) return;
        
            GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, postListItem.PostId);
            rpc.Completed += OnGetPostRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.AcquiringPermalink);
        }

        private void OnGetPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPostRPCCompleted;

            App.WaitIndicationService.KillSpinner();

            if (null == args.Error)
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
         
        private void ViewPostComments()
        {
            int index = postsListBox.SelectedIndex;
            if (-1 == index) return;

            PostListItem postListItem = postsListBox.SelectedItem as PostListItem;
            string queryStringFormat = "?{0}={1}";
            string queryString = string.Format(queryStringFormat, RelatedCommentsPage.IDKEY_VALUE, postListItem.PostId);
            NavigationService.Navigate(new Uri("/RelatedCommentsPage.xaml" + queryString, UriKind.Relative));
        }

        private void EditPost()
        {
            int index = postsListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPostListItem = App.MasterViewModel.Posts[index];

            if (App.MasterViewModel.CurrentPostListItem.PostId.Equals("-1", StringComparison.Ordinal))
            {
                // Local draft, set current post so post editor knows what to load
                App.MasterViewModel.CurrentPostListItem.DraftIndex = index;
            }

            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
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

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingPost);
        }

        private void OnDeletePostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            DeletePostRPC rpc = sender as DeletePostRPC;
            rpc.Completed -= OnDeletePostRPCCompleted;

            if (null == args.Error)
            {
                string postId = args.Items[0].PostId;
                var postListItem = App.MasterViewModel.CurrentBlog.PostListItems.Single(item => postId.Equals(item.PostId));
                App.MasterViewModel.CurrentBlog.PostListItems.Remove(postListItem);
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

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
                App.PopupSelectionService.ItemsSource = _pageListOptions;
                _popupServiceSelectionChangedHandler = OnPageOptionSelected;
                App.PopupSelectionService.SelectionChanged += OnPageOptionSelected;
            }

            App.PopupSelectionService.ShowPopup();
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

            App.MasterViewModel.CurrentBlog.LocalPageDrafts.RemoveAt(pagesListBox.SelectedIndex);
            App.MasterViewModel.CurrentBlog.PageListItems.RemoveAt(pagesListBox.SelectedIndex);
        }

        private void OnPageOptionSelected(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnPageOptionSelected;
            _popupServiceSelectionChangedHandler = null;

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

        private void EditPage()
        {
            int index = pagesListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPageListItem = App.MasterViewModel.Pages[index];

            if (App.MasterViewModel.CurrentPageListItem.PageId.Equals("-1", StringComparison.Ordinal))
            {
                // Local draft, set current page DraftIndex so editor knows what to load
                App.MasterViewModel.CurrentPageListItem.DraftIndex = index;
            }

            NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
        }

        private void ViewPage()
        {
            //use the GetPostRPC to get the Post.PermaLink value, then transmit that Uri to the shell

            PageListItem pageListItem = pagesListBox.SelectedItem as PageListItem;
            if (null == pageListItem) return;

            GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, pageListItem.PageId.ToString());
            rpc.Completed += OnGetPageRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.AcquiringPermalink);
        }

        private void OnGetPageRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPageRPCCompleted;

            App.WaitIndicationService.KillSpinner();

            if (null == args.Error)
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

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingPage);
        }

        private void OnDeletePageRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            DeletePageRPC rpc = sender as DeletePageRPC;
            rpc.Completed -= OnDeletePageRPCCompleted;

            if (null == args.Error)
            {                
                string pageId = args.Items[0].PostId;
                var pageListItem = App.MasterViewModel.CurrentBlog.PageListItems.Single(item => pageId.Equals(item.PageId));
                App.MasterViewModel.CurrentBlog.PageListItems.Remove(pageListItem);
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnCreatePostButtonClick(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPostListItem = null;
            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
        }

        private void OnCreatePageButtonClick(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPageListItem = null;
            NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
        }

        private void OnStatsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewStatsPage.xaml", UriKind.Relative));
        }

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
            if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.HidePopup();
                App.PopupSelectionService.SelectionChanged -= _popupServiceSelectionChangedHandler;

                //make sure none of the list items are selected, allowing the user to re-select
                //an item in the list.  This will trigger the SelectionChanged event
                commentsListBox.SelectedIndex = -1;
                postsListBox.SelectedIndex = -1;
                pagesListBox.SelectedIndex = -1;

                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void FetchPages()
        {
            DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred; 
            DataService.Current.FetchCurrentBlogPagesAsync();
        }

        private void FetchPosts(bool more)
        {
            DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred; 
            DataService.Current.FetchCurrentBlogPostsAsync(more);            
        }

        private void FetchComments(bool more)
        {
            DataService.Current.ExceptionOccurred += OnDataStoreFetchExceptionOccurred; 
            DataService.Current.FetchCurrentBlogCommentsAsync(more);            
        }

        private void OnDataStoreFetchExceptionOccurred(object sender, ExceptionEventArgs args)
        {
            App.WaitIndicationService.HideIndicator();
            DataService.Current.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;
            
            this.HandleException(args.Exception);
        }

        private void OnModerateCommentsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ModerateCommentsPage.xaml", UriKind.Relative));
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BlogSettingsPage.xaml", UriKind.Relative));
        }

        #endregion
    }
}