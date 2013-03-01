using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordPress.Localization;
using System.Windows;

namespace WordPress
{

    public partial class ImageDetailsPage : PhoneApplicationPage
    {
        public WordPress.Model.Media TappedImage { set; get; }

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _deleteIconButton;
        private ApplicationBarIconButton _positionIconButton;
        private ApplicationBarIconButton _featureIconButton;
        private ApplicationBarIconButton _unfeatureIconButton;
        private List<string> _positionListOptions;
        private bool isMarkedForRemoval = false;
        private SelectionChangedEventHandler _popupServiceSelectionChangedHandler;

        public ImageDetailsPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _deleteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _deleteIconButton.Text = _localizedStrings.ControlsText.Remove;
            _deleteIconButton.Click += _deleteIconButton_Click;

            _positionIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.positionMedia.png", UriKind.Relative));
            _positionIconButton.Text = _localizedStrings.Prompts.MediaPlacement;
            _positionIconButton.Click += _positionIconButton_Click;

            _featureIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.feature.png", UriKind.Relative));
            _featureIconButton.Text = _localizedStrings.ControlsText.FeatureImage;
            _featureIconButton.Click += featureIconButton_Click;

            _unfeatureIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.unfeature.png", UriKind.Relative));
            _unfeatureIconButton.Text = _localizedStrings.ControlsText.UnfeatureImage;
            _unfeatureIconButton.Click += unfeatureIconButton_Click;

            // Order of the list items should match the order of WordPress.Model.eMediaPlacement
            _positionListOptions = new List<string>(2);
            _positionListOptions.Add(_localizedStrings.Options.MediaOptions_PlaceBefore);
            _positionListOptions.Add(_localizedStrings.Options.MediaOptions_PlaceAfter);
        }

        void setupApplicationBar()
        {
            ApplicationBar.Buttons.Clear();

            ApplicationBar.Buttons.Add(_deleteIconButton);
            ApplicationBar.Buttons.Add(_positionIconButton);
            if (App.MasterViewModel.CurrentBlog.SupportsFeaturedImage() && TappedImage.CanBeFeatured)
            {
                if (TappedImage.IsFeatured)
                {
                    ApplicationBar.Buttons.Add(_unfeatureIconButton);
                }
                else
                {
                    ApplicationBar.Buttons.Add(_featureIconButton);
                }
            }

        }

        void _deleteIconButton_Click(object sender, EventArgs e)
        {
            isMarkedForRemoval = true;
            NavigationService.GoBack();
        }

        void _positionIconButton_Click(object sender, EventArgs e)
        {
            // Show popup to choose where to position the media - at the beginning or end of the content. 
            App.PopupSelectionService.Title = _localizedStrings.Prompts.MediaPlacement;
            App.PopupSelectionService.ItemsSource = _positionListOptions;
            App.PopupSelectionService.SelectionChanged += OnPositionOptionSelected;
            _popupServiceSelectionChangedHandler = OnPositionOptionSelected; //Keep a reference to the changes handler. It's only one for now, but preparing the code for future upgrade.

            App.PopupSelectionService.ShowPopup();
        }

        void featureIconButton_Click(object sender, EventArgs e)
        {
            TappedImage.IsFeatured = true;
            setupApplicationBar();
        }

        void unfeatureIconButton_Click(object sender, EventArgs e)
        {
            TappedImage.IsFeatured = false;
            setupApplicationBar();
        }


        private void OnPositionOptionSelected(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnPositionOptionSelected;
            App.PopupSelectionService.HidePopup();

            int index = 1 + _positionListOptions.IndexOf(args.AddedItems[0] as string);

            TappedImage.placement = (WordPress.Model.eMediaPlacement)index;

            NavigationService.GoBack();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.PopupSelectionService.IsPopupOpen)
            {
                App.PopupSelectionService.SelectionChanged -= _popupServiceSelectionChangedHandler;
                e.Cancel = true;
                App.PopupSelectionService.HidePopup();
                return;
            }
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            setupApplicationBar();
            ApplicationBar.IsVisible = true;
            using (Stream stream = TappedImage.getImageStream())
            {
               
                if (stream == null)
                {
                    MessageBoxResult result = MessageBox.Show("Can't read the picture, please try again later.", "Error", MessageBoxButton.OK);
                    return;
                }

                try
                {
                    BitmapImage tn = new BitmapImage();
                    tn.SetSource(stream);
                    ImageObeject.Source = tn;
                }
                catch (Exception)
                {
                    MessageBoxResult result = MessageBox.Show("Can't read the picture, please try again later.", "Error", MessageBoxButton.OK);
                }
                
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (isMarkedForRemoval)
            {
                if (e.Content is EditPostPage)
                {
                    (e.Content as EditPostPage).removeImage(TappedImage);
                }
                else if (e.Content is EditPagePage)
                {
                    (e.Content as EditPagePage).removeImage(TappedImage);
                }
            }
            else
            {
                if (e.Content is EditPostPage)
                {
                    (e.Content as EditPostPage).UpdateFeaturedImage(TappedImage);
                }
            }
           
            base.OnNavigatedFrom(e);
        }

    }
}