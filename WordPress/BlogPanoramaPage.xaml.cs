using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class BlogPanoramaPage : PhoneApplicationPage
    {
        #region member variables

        private List<string> _refreshListOptions;
        private List<string> _postListOptions;
        private List<string> _pageListOptions;
        private int _multiFetchTaskCount;
        private StringTable _localizedStrings;
        private SelectionChangedEventHandler _popupServiceSelectionChangedHandler;

        #endregion

        #region events

        #endregion

        #region constructors

        public BlogPanoramaPage()
        {
            InitializeComponent();

            DataContext = App.MasterViewModel;

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            _refreshListOptions = new List<string>(4);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Comments);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Posts);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Pages);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Everything);

            _postListOptions = new List<string>(4);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_ViewPost);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_ViewComments);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_EditPost);
            _postListOptions.Add(_localizedStrings.Options.PostOptions_DeletePost);

            _pageListOptions = new List<string>(4);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_ViewPage);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_ViewComments);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_EditPage);
            _pageListOptions.Add(_localizedStrings.Options.PageOptions_DeletePage);

            Loaded += OnPageLoaded;
        }

        #endregion

        #region properties

        #endregion

        #region methods

        private void OnPageLoaded(object sender, RoutedEventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        private void OnCommentsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = commentsListBox.SelectedIndex;
            if (-1 == index) return;


            App.MasterViewModel.CurrentComment = App.MasterViewModel.Comments[index];

            NavigationService.Navigate(new Uri("/ModerateCommentPage.xaml", UriKind.Relative));
        }

        private void OnPostsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == postsListBox.SelectedItem) return;
            PresentPostOptions();
        }

        private void PresentPostOptions()
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.PostActions;
            App.PopupSelectionService.ItemsSource = _postListOptions;
            App.PopupSelectionService.SelectionChanged += OnPostOptionSelected;
            App.PopupSelectionService.ShowPopup();

            _popupServiceSelectionChangedHandler = OnPostOptionSelected;
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
        }

        private void ViewPost()
        {
            //use the GetPostRPC to get the Post.PermaLink value, then transmit that Uri to the shell

            PostListItem postListItem = postsListBox.SelectedItem as PostListItem;
            if (null == postListItem) return;

            GetPostRPC rpc = new GetPostRPC(App.MasterViewModel.CurrentBlog, postListItem.PostId.ToString());
            rpc.Completed += OnGetPostRPCCompleted;
            rpc.ExecuteAsync();

            //TODO: showing the spinner here causes the screen to be non-responsive when we return to it after navigation to the BrowserShellPage...
            //doesn't seem to cause any issue when the spinner is shown when we stay on the page though.  Spinner needs to be presented in a Popup?
            //App.WaitIndicationService.ShowIndicator("acquiring permalink...");
        }

        private void OnGetPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPostRPCCompleted;

            //App.WaitIndicationService.HideIndicator();
            
            if (null == args.Error)
            {
                Post post = args.Items[0];
                Uri permaLinkUri = new Uri(post.PermaLink, UriKind.Absolute);
                string uriFormatString = "?{0}={1}";
                string paramString = string.Format(uriFormatString, BrowserShellPage.URIKEYNAME, permaLinkUri.ToString());
                NavigationService.Navigate(new Uri("/BrowserShellPage.xaml" + paramString, UriKind.Relative));
            }
            else
            {
                HandleError(args.Error);
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

            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
        }

        private void DeletePost()
        {
            //TODO: prompt the user to confirm the delete

            PostListItem postItem = postsListBox.SelectedItem as PostListItem;
            if (null == postItem) return;

            //TODO: rework the RPC--inefficient to create a new post just to call the delete rpc
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
                PostListItem selectedItem = postsListBox.SelectedItem as PostListItem;
                postsListBox.SelectedItem = null;

                App.MasterViewModel.CurrentBlog.PostListItems.Remove(selectedItem);
            }
            else
            {
                HandleError(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnPagesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == pagesListBox.SelectedItem) return;

            PresentPageOptions();
        }

        private void PresentPageOptions()
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.PageActions;
            App.PopupSelectionService.ItemsSource = _pageListOptions;
            App.PopupSelectionService.SelectionChanged += OnPageOptionSelected;
            App.PopupSelectionService.ShowPopup();

            _popupServiceSelectionChangedHandler = OnPageOptionSelected;
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
        }

        private void EditPage()
        {
            int index = pagesListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPageListItem = App.MasterViewModel.Pages[index];

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

            //TODO: showing the spinner here causes the screen to be non-responsive when we return to it after navigation to the BrowserShellPage...
            //doesn't seem to cause any issue when the spinner is shown when we stay on the page though.  Spinner needs to be presented in a Popup?
            //App.WaitIndicationService.ShowIndicator("acquiring permalink...");
        }

        private void OnGetPageRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPageRPCCompleted;

            //App.WaitIndicationService.HideIndicator();

            if (null == args.Error)
            {
                Post post = args.Items[0];
                Uri permaLinkUri = new Uri(post.PermaLink, UriKind.Absolute);
                string uriFormatString = "?{0}={1}";
                string paramString = string.Format(uriFormatString, BrowserShellPage.URIKEYNAME, permaLinkUri.ToString());
                NavigationService.Navigate(new Uri("/BrowserShellPage.xaml" + paramString, UriKind.Relative));
            }
            else
            {
                HandleError(args.Error);
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
            //TODO: prompt the user to confirm the delete

            PageListItem pageListItem = postsListBox.SelectedItem as PageListItem;
            if (null == pageListItem) return;

            //TODO: rework the RPC--inefficient to create a new post just to call the delete rpc
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
                PageListItem pageListItem = pagesListBox.SelectedItem as PageListItem;
                pagesListBox.SelectedItem = null;
                App.MasterViewModel.CurrentBlog.PageListItems.Remove(pageListItem);
            }
            else
            {
                HandleError(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnStatsButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature coming soon!");
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            DataStore.Instance.FetchComplete -= OnSingleFetchComplete;
            DataStore.Instance.FetchComplete -= OnMultiFetchComplete;
            DataStore.Instance.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.HidePopup();
                App.PopupSelectionService.SelectionChanged -= _popupServiceSelectionChangedHandler;

                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void OnRefreshButtonClick(object sender, RoutedEventArgs e)
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.RefreshEntity;
            App.PopupSelectionService.ItemsSource = _refreshListOptions;
            App.PopupSelectionService.SelectionChanged += OnRefreshOptionSelected;
            App.PopupSelectionService.ShowPopup();

            _popupServiceSelectionChangedHandler = OnRefreshOptionSelected;
        }

        private void OnRefreshOptionSelected(object sender, SelectionChangedEventArgs e)
        {
            App.PopupSelectionService.SelectionChanged -= OnRefreshOptionSelected;
            _popupServiceSelectionChangedHandler = null;

            if (1 > e.AddedItems.Count) return;

            string selection = (string)e.AddedItems[0];
            int index = _refreshListOptions.IndexOf(selection);

            if (index > _refreshListOptions.Count || 0 > index) return;

            DataStore.Instance.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;

            switch (index)
            {
                case 0:     //comments
                    FetchComments();
                    break;
                case 1:     //posts
                    FetchPosts();
                    break;
                case 2:     //pages
                    FetchPages();
                    break;
                case 3:     //everything
                    FetchEverything();
                    break;
            }
        }

        private void FetchEverything()
        {
            _multiFetchTaskCount = 3;
            DataStore.Instance.FetchComplete += OnMultiFetchComplete;
            DataStore.Instance.FetchCurrentBlogCommentsAsync();
            DataStore.Instance.FetchCurrentBlogPostsAsync();
            DataStore.Instance.FetchCurrentBlogPagesAsync();
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingEverything);
        }

        private void FetchPages()
        {
            DataStore.Instance.FetchComplete += OnSingleFetchComplete;
            DataStore.Instance.FetchCurrentBlogPagesAsync();
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPages);
        }

        private void FetchPosts()
        {
            DataStore.Instance.FetchComplete += OnSingleFetchComplete;
            DataStore.Instance.FetchCurrentBlogPostsAsync();
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPosts);
        }

        private void FetchComments()
        {
            DataStore.Instance.FetchComplete += OnSingleFetchComplete;
            DataStore.Instance.FetchCurrentBlogCommentsAsync();
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingComments);
        }

        private void OnDataStoreFetchExceptionOccurred(object sender, ExceptionEventArgs args)
        {
            App.WaitIndicationService.HideIndicator();
            DataStore.Instance.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;
            DataStore.Instance.FetchComplete -= OnSingleFetchComplete;
            DataStore.Instance.FetchComplete -= OnMultiFetchComplete;

            HandleError(args.Exception);
        }

        private void HandleError(Exception exception)
        {
            //TODO: clean this up...
            MessageBox.Show(exception.Message);
        }

        private void OnSingleFetchComplete(object sender, EventArgs e)
        {
            DataStore.Instance.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;
            App.WaitIndicationService.HideIndicator();
        }

        private void OnMultiFetchComplete(object sender, EventArgs e)
        {            
            _multiFetchTaskCount--;
            
            if (0 == _multiFetchTaskCount)
            {
                DataStore.Instance.ExceptionOccurred -= OnDataStoreFetchExceptionOccurred;
                App.WaitIndicationService.HideIndicator();
            }
        }

        private void OnModerateCommentsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ModerateCommentsPage.xaml", UriKind.Relative));
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BlogSettingsPage.xaml", UriKind.Relative));
        }

        private void OnPanoramaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FetchInitialDataForBlog();
        }

        private void FetchInitialDataForBlog()
        {
            PanoramaItem selectedItem = blogPanorama.SelectedItem as PanoramaItem;
            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            //TODO: determine how to resolve scenarios where a user actually doesn't have any of the following
            if (selectedItem == commentsPanoramaItem)
            {
                if (0 == currentBlog.Comments.Count)
                {
                    FetchComments();
                }
            }
            else if (selectedItem == postsPanoramaItem)
            {
                if (0 == currentBlog.PostListItems.Count)
                {
                    FetchPosts();
                }
            }
            else if (selectedItem == pagesPanoramaItem)
            {
                if (0 == currentBlog.PageListItems.Count)
                {
                    FetchPages();
                }
            }
        }

        #endregion





    }
}