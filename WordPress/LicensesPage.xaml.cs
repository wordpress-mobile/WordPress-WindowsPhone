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
using System.IO;
using WordPress.Commands;
using WordPress.Model;
using LinqToVisualTree;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace WordPress
{
    public partial class LicensesPage : PhoneApplicationPage
    {
        #region member variables
        private StringTable _localizedStrings;
        #endregion

        #region constructors

        public LicensesPage()
        {
            InitializeComponent();
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            var rs = Application.GetResourceStream(new Uri("/WordPress;component/Resources/licenses.html", UriKind.Relative));
            using (StreamReader sr = new StreamReader(rs.Stream))
            {
                webBrowser.NavigateToString(sr.ReadToEnd());
            }

            Loaded += OnPageLoaded;

            webBrowser.Loaded += WebBrowser_OnLoaded;
        }
        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        private void WebBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            var border = webBrowser.Descendants<Border>().Last() as Border; //See: http://www.scottlogic.co.uk/blog/colin/2011/11/suppressing-zoom-and-scroll-interactions-in-the-windows-phone-7-browser-control/
            border.ManipulationDelta += Border_ManipulationDelta;
            border.ManipulationCompleted += Border_ManipulationCompleted;
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // suppress zoom
            if (e.FinalVelocities.ExpansionVelocity.X != 0.0 ||
                e.FinalVelocities.ExpansionVelocity.Y != 0.0)
                e.Handled = true;
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // suppress zoom
            if (e.DeltaManipulation.Scale.X != 0.0 ||
                e.DeltaManipulation.Scale.Y != 0.0)
                e.Handled = true;
        }

        #endregion
    }
}