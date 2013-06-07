using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public partial class GallerySettingsPage : PhoneApplicationPage
    {
        private StringTable _localizedStrings;
        private Post _post;
        private ApplicationBarIconButton _saveIconButton;

        public GallerySettingsPage()
        {
            InitializeComponent();

            _post = App.MasterViewModel.CurrentPost;
            DataContext = _post;
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            InitializeApplicationBar();
            InitializePickers();

        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            SaveGallerySettings();
            NavigationService.GoBack();
        }

        private void InitializeApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;

            ApplicationBar.Buttons.Add(_saveIconButton);            
        }

        private void InitializePickers()
        {
            SetLinkTo();
            SetColumns();
            SetType();
            SetPlacement();
        }

        private void SetLinkTo()
        {
            List<string> statusList = new List<string>() { 
                _localizedStrings.ControlsText.AttachmentPage,
                _localizedStrings.ControlsText.MediaFile
            };

            linkToPicker.ItemsSource = statusList;

            if (_post.Gallery.LinkTo == eGalleryLinkTo.AttachmentPage)
            {
                linkToPicker.SelectedIndex = 0;
            }
            else
            {
                linkToPicker.SelectedIndex = 1;
            }
        }

        private void SetColumns()
        {
            List<string> columnsList = new List<string>();
            for (int i = 1; i <= 9; i++)
            {
                columnsList.Add(i.ToString());   
            }

            columnsPicker.ItemsSource = columnsList;
            
            int selectedIndex = (int) _post.Gallery.Columns - 1;
            if (selectedIndex < 0)
                selectedIndex = 0;
            columnsPicker.SelectedIndex = selectedIndex;
        }

        private void SetType()
        {

            if (App.MasterViewModel.CurrentBlog.isWPcom())
            {
                List<string> statusList = new List<string>() { 
                _localizedStrings.ControlsText.ThumbnailGrid,
                _localizedStrings.ControlsText.Tiles,
                _localizedStrings.ControlsText.SquareTiles,
                _localizedStrings.ControlsText.Circles,
                _localizedStrings.ControlsText.Slideshow,
            };
                typePicker.ItemsSource = statusList;
                typePicker.SelectedIndex = (int)_post.Gallery.Type;
            }
            else
            {
                List<string> statusList = new List<string>() { 
                _localizedStrings.ControlsText.ThumbnailGrid,
                _localizedStrings.ControlsText.Slideshow,
            };
                typePicker.ItemsSource = statusList;
                int selected_index = ((int)_post.Gallery.Type) == 0 ? 0 : 1;
                typePicker.SelectedIndex = selected_index;
            }
        }

        private void SetPlacement()
        {
            List<string> placementList = new List<string>() { 
                _localizedStrings.Options.MediaOptions_PlaceBefore,
                _localizedStrings.Options.MediaOptions_PlaceAfter,
            };

            placementPicker.ItemsSource = placementList;

            if (!_post.Gallery.ContentBelow)
                placementPicker.SelectedIndex = 0;
            else
                placementPicker.SelectedIndex = 1;
        }

        private void SaveGallerySettings()
        {
            _post.Gallery.Enabled = true;
            _post.Gallery.LinkTo = (eGalleryLinkTo) linkToPicker.SelectedIndex;
            _post.Gallery.Columns = Convert.ToInt32(columnsPicker.Items[columnsPicker.SelectedIndex]);
            if(App.MasterViewModel.CurrentBlog.isWPcom())
                _post.Gallery.Type = (eGalleryType) typePicker.SelectedIndex;
            else
                _post.Gallery.Type = typePicker.SelectedIndex == 0 ? eGalleryType.Default : eGalleryType.Slideshow;
            _post.Gallery.RandomOrder = (bool) randomOrderCheckbox.IsChecked;
            _post.Gallery.ContentBelow = placementPicker.SelectedIndex == 1;
        }
    }
}