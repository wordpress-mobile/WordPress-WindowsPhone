using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordPress.Localization;

namespace WordPress
{

    public partial class ImageDetailsPage : PhoneApplicationPage
    {
        public WordPress.Model.Media TappedImage { set; get; }

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _deleteIconButton;
        private ApplicationBarIconButton _positionIconButton;
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
            ApplicationBar.Buttons.Add(_deleteIconButton);

            _positionIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.positionMedia.png", UriKind.Relative));
            _positionIconButton.Text = _localizedStrings.Prompts.MediaPlacement;
            _positionIconButton.Click += _positionIconButton_Click;
            ApplicationBar.Buttons.Add(_positionIconButton);

            // Order of the list items should match the order of WordPress.Model.eMediaPlacement
            _positionListOptions = new List<string>(2);
            _positionListOptions.Add(_localizedStrings.Options.MediaOptions_PlaceBefore);
            _positionListOptions.Add(_localizedStrings.Options.MediaOptions_PlaceAfter);

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
            BitmapImage tn = new BitmapImage();
            tn.SetSource(TappedImage.getImageStream());
            ImageObeject.Source = tn;
            ApplicationBar.IsVisible = true;
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
            base.OnNavigatedFrom(e);
        }

    }
}