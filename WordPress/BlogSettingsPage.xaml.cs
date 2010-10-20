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
    public partial class BlogSettingsPage : PhoneApplicationPage
    {
        public BlogSettingsPage()
        {
            InitializeComponent();
            thumbnailWidthComboBox.ItemsSource = new int[] {100, 200, 300};
        }
    }
}