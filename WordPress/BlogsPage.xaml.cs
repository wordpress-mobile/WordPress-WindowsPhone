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
using Microsoft.Phone.Shell;

namespace WordPress
{
    public partial class BlogsPage : PhoneApplicationPage
    {
        #region member variables

        private ApplicationBarIconButton _addBlogIconButton;
        private ApplicationBarIconButton _preferencesIconButton;

        #endregion

        #region constructors

        public BlogsPage()
        {
            InitializeComponent();

            DataContext = App.MasterViewModel;

            ApplicationBar = new ApplicationBar();

            _addBlogIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addBlogIconButton.Text = "add blog";
            _addBlogIconButton.Click += OnAddAccountIconButtonClick;
            ApplicationBar.Buttons.Add(_addBlogIconButton);

            _preferencesIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _preferencesIconButton.Text = "preferences";
            _preferencesIconButton.Click += OnPreferencesIconButtonClick;
            ApplicationBar.Buttons.Add(_preferencesIconButton);
        }

        #endregion

        #region methods

        private void OnBlogsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = blogsListBox.SelectedIndex;
            if (-1 == index) return;

            App.MasterViewModel.CurrentBlog = App.MasterViewModel.Blogs[index];
            NavigationService.Navigate(new Uri("/BlogPanoramaPage.xaml", UriKind.Relative));
        }

        private void OnAddAccountIconButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/LocateBlogPage.xaml", UriKind.Relative));
        }

        private void OnPreferencesIconButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PreferencesPage.xaml", UriKind.Relative));
        }

        #endregion

    }
}