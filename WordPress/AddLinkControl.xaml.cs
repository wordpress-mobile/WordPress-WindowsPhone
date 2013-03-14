using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WordPress
{
	public partial class AddLinkControl : UserControl
    {
        #region member variables

        private string _url;
        private string _linkText;

        private const double BACKGROUND_OPACITY_MAX = 0.97;
        private const double BACKGROUND_OPACITY_MIN = 0.0;
        private readonly TimeSpan DEFAULT_DURATION = TimeSpan.FromMilliseconds(250);

        #endregion

        #region events

        public event EventHandler LinkChosen;

        #endregion

        #region constructor

        public AddLinkControl()
		{
			// Required to initialize variables
			InitializeComponent();
        }

        #endregion

        #region properties

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url",
                typeof(string),
                typeof(AddLinkControl),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnUrlPropertyChanged)));

        private static void OnUrlPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AddLinkControl control = sender as AddLinkControl;
            control.Url = args.NewValue as string;
            control.ValidateUrl();
        }

        public string LinkText
        {
            get { return (string)GetValue(LinkTextProperty); }
            set { SetValue(LinkTextProperty, value); }
        }

        public static readonly DependencyProperty LinkTextProperty =
            DependencyProperty.Register("LinkText",
                typeof(string),
                typeof(AddLinkControl),
                new PropertyMetadata(string.Empty));

        private bool FireLinkChosenEvent { get; set; }

        #endregion

        #region methods

        private void ValidateUrl()
        {
            if (Uri.IsWellFormedUriString(Url, UriKind.RelativeOrAbsolute))
            {
                insertLinkButton.IsEnabled = true;
                return;
            }
        }

        public void Show()
        {
            Url = "http://";
            LinkText = string.Empty;
            insertLinkButton.IsEnabled = false;

            Storyboard storyboard = AnimationHelper.CreateEaseInAnimationStoryBoard(LayoutRoot, Grid.OpacityProperty, BACKGROUND_OPACITY_MIN, BACKGROUND_OPACITY_MAX, DEFAULT_DURATION);
            Visibility = Visibility.Visible;
            storyboard.Begin();
        }
        
        public void Hide()
        {
            Storyboard storyboard = AnimationHelper.CreateEaseOutAnimationStoryBoard(LayoutRoot, Grid.OpacityProperty, BACKGROUND_OPACITY_MAX, BACKGROUND_OPACITY_MIN, DEFAULT_DURATION);
            storyboard.Completed += OnEaseOutStoryboardCompleted;
            storyboard.Begin();
        }

        private void OnEaseOutStoryboardCompleted(object sender, EventArgs e)
        {
            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnEaseOutStoryboardCompleted;

            Visibility = Visibility.Collapsed;
        }

        private void NotifyLinkChosen()
        {
            if (null != LinkChosen)
            {
                LinkChosen(this, EventArgs.Empty);
            }
        }

        private void OnInsertLinkButtonClick(object sender, RoutedEventArgs e)
        {
            NotifyLinkChosen();
        }

        public string CreateLinkMarkup()
        {
            string linkFormat = "<a href=\"{0}\">{1}</a>";
            string linkMarkup = string.Format(linkFormat, Url, LinkText);
            return linkMarkup;
        }

        #endregion

    }
}