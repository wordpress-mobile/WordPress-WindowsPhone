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

namespace WordPress
{
    public partial class AddBlogHomePage : PhoneApplicationPage
    {
        public AddBlogHomePage()
        {
            InitializeComponent();
        }

        private void createNewBlogButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateNewWordPressAccountPage.xaml", UriKind.Relative));
        }

        private void existingWPBlogButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddExistingWordPressBlogPage.xaml", UriKind.Relative));
        }

        private void existingWPSiteButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddExistingWordPressSitePage.xaml", UriKind.Relative));            
        }
    }
}