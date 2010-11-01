using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class SelectCategoriesPage : PhoneApplicationPage
    {
        #region member variables

        private ApplicationBarIconButton _cancelIconButton;
        private ApplicationBarIconButton _addIconButton;
        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public SelectCategoriesPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _cancelIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.cancel.png", UriKind.Relative));
            _cancelIconButton.Text = _localizedStrings.ControlsText.Cancel;
            _cancelIconButton.Click += OnCancelButtonClick;
            ApplicationBar.Buttons.Add(_cancelIconButton);

            _addIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addIconButton.Text = _localizedStrings.ControlsText.Add;
            _addIconButton.Click += OnAddButtonClick;
            ApplicationBar.Buttons.Add(_addIconButton);

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            LoadCategories();
        }

        private void LoadCategories()
        {
            if (null == App.MasterViewModel.CurrentBlog) return;

            if (0 == DataStore.Instance.CurrentBlog.Categories.Count)
            {
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingCategories);
                DataStore.Instance.FetchComplete += OnFetchCurrentBlogCategoriesComplete;
                DataStore.Instance.FetchCurrentBlogCategories();
            }
            else
            {
                foreach (string categoryString in App.MasterViewModel.CurrentPost.Categories)
                {
                    foreach (Category category in App.MasterViewModel.CurrentBlog.Categories)
                    {
                        if (categoryString.Equals(category.Description))
                        {
                            categoriesListBox.SelectedItems.Add(category);
                        }
                    }
                }
            }
        }

        private void OnFetchCurrentBlogCategoriesComplete(object sender, EventArgs e)
        {
            DataStore.Instance.FetchComplete -= OnFetchCurrentBlogCategoriesComplete;
            App.WaitIndicationService.HideIndicator();
        }

        private void OnCancelButtonClick(object sender, EventArgs args)
        {
            NavigationService.GoBack();
        }

        private void OnAddButtonClick(object sender, EventArgs args)
        {
            
        }

        private void OnSaveButtonClick(object sender, EventArgs args)
        {
            Post currentPost = App.MasterViewModel.CurrentPost;
            currentPost.Categories.Clear();

            foreach (Category category in categoriesListBox.SelectedItems)
            {
                currentPost.Categories.Add(category.Description);
            }
            NavigationService.GoBack();
        }

        #endregion
    }
}