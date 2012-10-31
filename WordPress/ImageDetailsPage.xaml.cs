using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using WordPress.Localization;

namespace WordPress
{
    public partial class ImageDetailsPage : PhoneApplicationPage
    {
        public WordPress.Model.Media TappedImage { set; get; }

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _deleteIconButton;
        private bool isMarkedForRemoval = false;

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
        }

        void _deleteIconButton_Click(object sender, EventArgs e)
        {
            isMarkedForRemoval = true;
            NavigationService.GoBack();
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
            if (e.Content is EditPostPage && isMarkedForRemoval)
            {
                (e.Content as EditPostPage).removeImage(TappedImage);
            }

            base.OnNavigatedFrom(e);
        }
    }
}