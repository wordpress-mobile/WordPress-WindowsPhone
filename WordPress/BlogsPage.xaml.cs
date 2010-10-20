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
    public partial class BlogsPage : PhoneApplicationPage
    {
        public BlogsPage()
        {
            InitializeComponent();
            DataContext = App.MasterViewModel;        
        }

        private void blogsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = blogsListBox.SelectedIndex;
            App.MasterViewModel.CurrentBlog = App.MasterViewModel.Blogs[index];
            NavigationService.Navigate(new Uri("/BlogPanoramaPage.xaml", UriKind.Relative));
        }

    }
}