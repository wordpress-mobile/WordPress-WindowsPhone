using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone;
using Microsoft.Phone.Tasks;

using WordPress.Commands;
using WordPress.Localization;
using WordPress.Model;


namespace WordPress
{



    public partial class ErrorPage : PhoneApplicationPage
    {
        public static Exception Exception;

        #region constructors
        public ErrorPage()
        {
            InitializeComponent();
        }
        #endregion

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        { 
           // base.OnNavigatedTo(e);
            if (Exception != null)
            {
                ErrorText.Text = Exception.Message;
                FullErrorText.Text = Exception.ToString();
            }
        }

        private void FAQButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //NavigationService.GoBack();
            LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_FAQ_URL);          
        }

        private void forumButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
           // NavigationService.GoBack();
            LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_FORUMS_URL);             
        }

        private void mailButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //NavigationService.GoBack();
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "support@wordpress.com";
            emailComposeTask.Body = "Insert the URL of your blog and the error message";
            emailComposeTask.Subject = "WordPress for Windows Phone support";
            emailComposeTask.Show();
        }

    }
}