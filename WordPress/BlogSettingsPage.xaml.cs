using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class BlogSettingsPage : PhoneApplicationPage
    {
        #region member variables

        private const string USERNAME_VALUE = "username";
        private const string PASSWORD_VALUE = "password";
        private const string MEDIAABOVETEXT_VALUE = "mediaAboveText";
        private const string THUMBNAILPIXELWIDTH_VALUE = "thumbnailPixelWidth";
        private const string ALIGNTHUMBNAILTOCENTER_VALUE = "alignThumbnailToCenter";
        private const string UPLOADANDLINKTOFULLIMAGE_VALUE = "uploadAndLinkToFullImage";
        private const string GEOTAGPOSTS_VALUE = "geotag";

        private List<int> _thumbnailSizes;
        private ApplicationBarIconButton _cancelIconButton;
        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructor

        public BlogSettingsPage()
        {
            InitializeComponent();
                        
            _thumbnailSizes = new List<int>(new int[] { 100, 200, 300 });

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _cancelIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.cancel.png", UriKind.Relative));
            _cancelIconButton.Text = _localizedStrings.ControlsText.Cancel;
            _cancelIconButton.Click += OnCancelButtonClick;
            ApplicationBar.Buttons.Add(_cancelIconButton);

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);
        }

        #endregion

        #region methods

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.PopupSelectionService.IsPopupOpen)
            {
                HidePopupSelectionService();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Blog currentBlog = App.MasterViewModel.CurrentBlog;
            currentBlog.BeginEdit();
            DataContext = currentBlog;

            if (State.ContainsKey(USERNAME_VALUE))
            {
                RestorePageState();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Blog blog = DataContext as Blog;
            if (blog.IsEditing)
            {
                blog.CancelEdit();
            }

            SavePageState();
        }

        /// <summary>
        /// Stores transient data in the page's State dictionary
        /// </summary>
        private void SavePageState()
        {
            if (State.ContainsKey(USERNAME_VALUE))
            {
                State.Remove(USERNAME_VALUE);
            }
            State.Add(USERNAME_VALUE, usernameTextBox.Text);

            if (State.ContainsKey(PASSWORD_VALUE))
            {
                State.Remove(PASSWORD_VALUE);
            }
            State.Add(PASSWORD_VALUE, passwordTextBox.Password);

            if (State.ContainsKey(MEDIAABOVETEXT_VALUE))
            {
                State.Remove(MEDIAABOVETEXT_VALUE);
            }
            State.Add(MEDIAABOVETEXT_VALUE, aboveTextRadioButton.IsChecked);

            if (State.ContainsKey(THUMBNAILPIXELWIDTH_VALUE))
            {
                State.Remove(THUMBNAILPIXELWIDTH_VALUE);
            }
            State.Add(THUMBNAILPIXELWIDTH_VALUE, (int)thumbnailPixelWidthButton.Content);

            if (State.ContainsKey(ALIGNTHUMBNAILTOCENTER_VALUE))
            {
                State.Remove(ALIGNTHUMBNAILTOCENTER_VALUE);
            }
            State.Add(ALIGNTHUMBNAILTOCENTER_VALUE, alignThumbnailCheckBox.IsChecked);

            if (State.ContainsKey(UPLOADANDLINKTOFULLIMAGE_VALUE))
            {
                State.Remove(UPLOADANDLINKTOFULLIMAGE_VALUE);
            }
            State.Add(UPLOADANDLINKTOFULLIMAGE_VALUE, uploadCheckBox.IsChecked);

            if (State.ContainsKey(GEOTAGPOSTS_VALUE))
            {
                State.Remove(GEOTAGPOSTS_VALUE);
            }
            State.Add(GEOTAGPOSTS_VALUE, geotagCheckBox.IsChecked);
        }

        private void RestorePageState()
        {
            Blog blog = DataContext as Blog;

            if (State.ContainsKey(USERNAME_VALUE))
            {
                blog.Username = (string)State[USERNAME_VALUE];
            }
            
            if (State.ContainsKey(PASSWORD_VALUE))
            {
                blog.Password = (string)State[PASSWORD_VALUE];
            }
            
            if (State.ContainsKey(MEDIAABOVETEXT_VALUE))
            {
                blog.PlaceImageAboveText = (bool)State[MEDIAABOVETEXT_VALUE];
            }

            if (State.ContainsKey(THUMBNAILPIXELWIDTH_VALUE))
            {
                blog.ThumbnailPixelWidth = (int)State[THUMBNAILPIXELWIDTH_VALUE];
            }

            if (State.ContainsKey(ALIGNTHUMBNAILTOCENTER_VALUE))
            {
                blog.AlignThumbnailToCenter = (bool)State[ALIGNTHUMBNAILTOCENTER_VALUE];
            }

            if (State.ContainsKey(UPLOADANDLINKTOFULLIMAGE_VALUE))
            {
                blog.UploadAndLinkToFullImage = (bool)State[UPLOADANDLINKTOFULLIMAGE_VALUE]; 
            }
            
            if (State.ContainsKey(GEOTAGPOSTS_VALUE))
            {
                blog.GeotagPosts = (bool)State[GEOTAGPOSTS_VALUE];
            }
        }

        private void OnThumbnailPixelWidthButtonClick(object sender, RoutedEventArgs args)
        {            
            ShowThumbnailSizeSelections();
        }

        private void ShowThumbnailSizeSelections()
        {
            App.PopupSelectionService.Title = "Please select a thumbnail size.";
            App.PopupSelectionService.ItemsSource = _thumbnailSizes;
            App.PopupSelectionService.SelectionChanged += new SelectionChangedEventHandler(OnPopupSelectionServiceSelectionChanged);
            App.PopupSelectionService.ShowPopup();
        }

        private void OnPopupSelectionServiceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 == e.AddedItems.Count) return;
            if (!(e.AddedItems[0] is int)) return;

            int selection = (int)e.AddedItems[0];
            if (_thumbnailSizes.Contains(selection))
            {
                int index = _thumbnailSizes.IndexOf(selection);
                Blog currentBlog = DataContext as Blog;
                currentBlog.ThumbnailPixelWidth = _thumbnailSizes[index];
            }

            HidePopupSelectionService();
        }

        private void HidePopupSelectionService()
        {
            App.PopupSelectionService.SelectionChanged -= OnPopupSelectionServiceSelectionChanged;
            App.PopupSelectionService.HidePopup();
        }

        public void OnSaveButtonClick(object sender, EventArgs args)
        {
            Blog blog = DataContext as Blog;
            if (blog.IsEditing)
            {
                blog.EndEdit();
            }
            NavigationService.GoBack();
        }

        public void OnCancelButtonClick(object sender, EventArgs args)
        {
            NavigationService.GoBack();
        }

        #endregion
    }
}