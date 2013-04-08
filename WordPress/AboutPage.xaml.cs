using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Settings;
using WordPress.Utils;
using Microsoft.Phone.Tasks;
using System.Reflection;
using WordPress.Commands;
using WordPress.Model;

namespace WordPress
{
    public partial class AboutPage : PhoneApplicationPage
    {
        #region member variables
        private StringTable _localizedStrings;
        #endregion

        #region constructors

        public AboutPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            string version = Constants.WORDPRESS_USERAGENT.Split('/')[1];
            AppVersion.Text = string.Format("{0} {1}", _localizedStrings.ControlsText.Version, version);
            AppCopyright.Text = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute), false)).Copyright;
            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        private void OnOpenSourceButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LicensesPage.xaml", UriKind.Relative));
        }

        private void OnPrivacyPolicyButtonClick(object sender, RoutedEventArgs e)
        {
            var command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_PRIVACY_URL);
        }

        private void OnTermsOfServiceButtonClick(object sender, RoutedEventArgs e)
        {
            var command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_TOS_URL);
        }

        #endregion
    }
}