using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;

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

            StringTable localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _addBlogIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addBlogIconButton.Text = localizedStrings.ControlsText.AddBlog;
            _addBlogIconButton.Click += OnAddAccountIconButtonClick;
            ApplicationBar.Buttons.Add(_addBlogIconButton);

            _preferencesIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _preferencesIconButton.Text = localizedStrings.ControlsText.Preferences;
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

        #endregion

    }
}