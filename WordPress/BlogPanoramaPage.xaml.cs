using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using WordPress.Model;
using System.Collections.ObjectModel;

namespace WordPress
{
    public partial class BlogPanoramaPage : PhoneApplicationPage
    {
        #region member variables

        #endregion

        #region events

        #endregion

        #region constructors

        public BlogPanoramaPage()
        {
            InitializeComponent();

            DataContext = App.MasterViewModel;        
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

        private void refreshCommentsButton_Click(object sender, RoutedEventArgs e)
        {            
            DataStore.Instance.FetchCurrentBlogCommentsAsync();

            App.WaitIndicationService.ShowIndicator("Retrieving comments...");
        }

        private void refreshPostsButton_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.FetchCurrentBlogPostsAsync();

            App.WaitIndicationService.ShowIndicator("Retrieving posts...");
        }
        
        private void refreshPagesButton_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.FetchCurrentBlogPagesAsync();

            App.WaitIndicationService.ShowIndicator("Retrieving pages...");
        }

        private void newPostButton_Click(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPost = null;
            NavigationService.Navigate(new Uri("/EditPostPage.xaml", UriKind.Relative));
        }

        private void newPageButton_Click(object sender, RoutedEventArgs e)
        {
            App.MasterViewModel.CurrentPage = null;
            NavigationService.Navigate(new Uri("/EditPagePage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataStore.Instance.FetchComplete += DataStore_FetchComplete;
            DataStore.Instance.ExceptionOccurred += DataStore_ExceptionOccurred;

            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            DataStore.Instance.FetchComplete -= DataStore_FetchComplete;
            DataStore.Instance.ExceptionOccurred -= DataStore_ExceptionOccurred;
        }

        private void DataStore_FetchComplete(object sender, EventArgs e)
        {
            App.WaitIndicationService.HideIndicator();
        }

        private void DataStore_ExceptionOccurred(object sender, ExceptionEventArgs args)
        {
            App.WaitIndicationService.HideIndicator();
        }

        #endregion




    }
}