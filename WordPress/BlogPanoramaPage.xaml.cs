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
        private int _multiFetchTaskCount;
        private StringTable _localizedStrings;

        #endregion

        #region events

        #endregion

        #region constructors

        public BlogPanoramaPage()
        {
            InitializeComponent();

            DataContext = App.MasterViewModel;

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            _refreshListOptions = new List<string>();
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Comments);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Posts);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Pages);
            _refreshListOptions.Add(_localizedStrings.Options.RefreshEntity_Everything);

        }

        #endregion

        #region properties

        #endregion

        #region methods

        private void commentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = commentsListBox.SelectedIndex;
            if (-1 == index) return;


            App.MasterViewModel.CurrentComment = App.MasterViewModel.Comments[index];

            NavigationService.Navigate(new Uri("/ModerateCommentPage.xaml", UriKind.Relative));
        }

        private void postsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //NOTE: this should launch a "dialog box" that allows the user to select
            //between viewing the post in a web browser, viewing the comments related to the post, 
            //editing the post, and deleting the post.  For now, just navigate to the "edit post" page

            int index = postsListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPost = App.MasterViewModel.Posts[index];
            
            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
        }

        private void pagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //NOTE: this should launch a "dialog box" that allows the user to select
            //between viewing the page in a web browser, viewing the comments related to the page, 
            //editing the page, and deleting the page.  For now, just navigate to the "edit page" page
            
            int index = pagesListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentPage = App.MasterViewModel.Pages[index];

            NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
        }

        private void statsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature coming soon!");
        }

        private void createPostButton_Click(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPost = null;
            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
        }

        private void createPageButton_Click(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPage = null;
            NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.WaitIndicationService.RootVisualElement = LayoutRoot;
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
                App.PopupSelectionService.SelectionChanged -= OnPopupServiceSelectionChanged;

                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.RefreshEntity;
            App.PopupSelectionService.ItemsSource = _refreshListOptions;
            App.PopupSelectionService.SelectionChanged += OnPopupServiceSelectionChanged;
            App.PopupSelectionService.ShowPopup();
        }

        private void OnPopupServiceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.PopupSelectionService.SelectionChanged -= OnPopupServiceSelectionChanged;
            if (1 > e.AddedItems.Count) return;

            string selection = (string)e.AddedItems[0];
            int index = _refreshListOptions.IndexOf(selection);

            if (index > _refreshListOptions.Count || 0 > index) return;

            DataStore.Instance.ExceptionOccurred += OnDataStoreFetchExceptionOccurred;

            switch (index)
            {
                case 0:     //comments
                    DataStore.Instance.FetchComplete += OnSingleFetchComplete;
                    DataStore.Instance.FetchCurrentBlogCommentsAsync();
                    App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingComments);
                    break;
                case 1:     //posts
                    DataStore.Instance.FetchComplete += OnSingleFetchComplete;
                    DataStore.Instance.FetchCurrentBlogPostsAsync();
                    App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPosts);
                    break;
                case 2:     //pages
                    DataStore.Instance.FetchComplete += OnSingleFetchComplete;
                    DataStore.Instance.FetchCurrentBlogPagesAsync();
                    App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPages);
                    break;
                case 3:     //everything
                    _multiFetchTaskCount = 3;
                    DataStore.Instance.FetchComplete += OnMultiFetchComplete;
                    DataStore.Instance.FetchCurrentBlogCommentsAsync();
                    DataStore.Instance.FetchCurrentBlogPostsAsync();
                    DataStore.Instance.FetchCurrentBlogPagesAsync();
                    App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingEverything);
                    break;
            }
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

        private void moderateCommentsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature coming soon!");
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BlogSettingsPage.xaml", UriKind.Relative));
        }

        #endregion




    }
}