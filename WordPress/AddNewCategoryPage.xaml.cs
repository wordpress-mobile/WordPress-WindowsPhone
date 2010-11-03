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

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class AddNewCategoryPage : PhoneApplicationPage
    {
        #region member variables

        ApplicationBarIconButton _saveIconButton;
        StringTable _localizedStrings;

        #endregion

        #region constructors

        public AddNewCategoryPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, RoutedEventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        private void OnSaveButtonClick(object sender, EventArgs args)
        {
            SaveCategory();
        }

        private void SaveCategory()
        {
            if (string.IsNullOrEmpty(categoryNameTextBox.Text))
            {
                MessageBox.Show(_localizedStrings.Prompts.MissingCategoryName);
                categoryNameTextBox.Focus();
                return;
            }
            
            //force updates to the data context
            categoryNameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            categorySlugTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            categoryDescriptionTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            Category newCategory = DataContext as Category;
            NewCategoryRPC rpc = new NewCategoryRPC(App.MasterViewModel.CurrentBlog, newCategory);
            rpc.Completed += OnNewCategoryRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.CreatingNewCategory);
        }

        private void OnNewCategoryRPCCompleted(object sender, XMLRPCCompletedEventArgs<Category> args)
        {
            NewCategoryRPC rpc = sender as NewCategoryRPC;
            rpc.Completed -= OnNewCategoryRPCCompleted;
            
            if (null == args.Error)
            {
                DataStore.Instance.FetchCurrentBlogCategories();
                DataStore.Instance.FetchComplete += OnFetchCurrentBlogCategoriesComplete;
            }
            else
            {
                this.HandleException(args.Error);
            }
        }

        private void OnFetchCurrentBlogCategoriesComplete(object sender, EventArgs args)
        {
            DataStore.Instance.FetchComplete -= OnFetchCurrentBlogCategoriesComplete;
            App.WaitIndicationService.HideIndicator();
            NavigationService.GoBack();            
        }
        
        private void OnCategoryParentButtonClick(object sender, RoutedEventArgs e)
        {
            PresentCategories();
        }

        private void PresentCategories()
        {
            List<string> categories = new List<string>();
            int selectedIndex = -1;

            categories.Add(_localizedStrings.ControlsText.None);

            Category category = DataContext as Category;
            foreach (Category currentCategory in App.MasterViewModel.CurrentBlog.Categories)
            {
                if (category.ParentId == currentCategory.CategoryId)
                {
                    selectedIndex = App.MasterViewModel.CurrentBlog.Categories.IndexOf(currentCategory)+1;
                }
                categories.Add(currentCategory.CategoryName);
            }

            App.PopupSelectionService.Title = _localizedStrings.Prompts.SelectCategory;
            App.PopupSelectionService.ItemsSource = categories;
            App.PopupSelectionService.SelectionChanged += OnPopupSelectionServiceSelectionChanged;
            App.PopupSelectionService.ShowPopup(selectedIndex);
        }

        private void OnPopupSelectionServiceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.PopupSelectionService.SelectionChanged -= OnPopupSelectionServiceSelectionChanged;

            if (0 == e.AddedItems.Count) return;

            Category child = DataContext as Category;
            string selection = e.AddedItems[0] as string;

            if (selection != _localizedStrings.ControlsText.None)
            {
                Category parent = App.MasterViewModel.CurrentBlog.Categories.Single(category => selection == category.CategoryName);
                child.ParentId = parent.CategoryId;
            }
            else
            {
                child.ParentId = -1;    
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.SelectionChanged -= OnPopupSelectionServiceSelectionChanged;
            }
            base.OnNavigatingFrom(e);
        }

        #endregion
    }
}