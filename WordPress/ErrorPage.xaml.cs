using System;
using System.Diagnostics;
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
        
        private StringTable _localizedStrings;

        private const string ERROR_DESC_VALUE = "errDesc";
        private const string FULL_STACK_VALUE = "fullStackDesc";
        private string fullStackTrace = ""; //used to store the page state
        
        #region constructors
        public ErrorPage()
        {
            InitializeComponent();
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
            this.DebugLog("end constructor");
        }
        #endregion

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        { 
           // base.OnNavigatedTo(e);
            this.DebugLog("OnNavigatedTo");
            if (Exception != null)
            {
                ErrorText.Text = Exception.Message;
                fullStackTrace = Exception.ToString();
            }
            else //user re-activaed the app
            {
                //look for transient data stored in the State dictionary
                if (State.ContainsKey(ERROR_DESC_VALUE))
                {
                    ErrorText.Text = State[ERROR_DESC_VALUE] as string;
                }

                if (State.ContainsKey(FULL_STACK_VALUE))
                {
                    fullStackTrace = State[FULL_STACK_VALUE] as string;
                }
            }
        }
         
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.DebugLog("OnNavigatedFrom");
            base.OnNavigatedFrom(e);
            //store transient data in the State dictionary
            if (State.ContainsKey(ERROR_DESC_VALUE))
            {
                State.Remove(ERROR_DESC_VALUE);
            }
            State.Add(ERROR_DESC_VALUE, ErrorText.Text);

            if (State.ContainsKey(FULL_STACK_VALUE))
            {
                State.Remove(FULL_STACK_VALUE);
            }
            State.Add(FULL_STACK_VALUE, fullStackTrace);
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
            emailComposeTask.To = Constants.WORDPRESS_SUPPORT_EMAIL;
            emailComposeTask.Body = _localizedStrings.Prompts.SupportEmailBody + "\n\n\n\n" + fullStackTrace; 
            emailComposeTask.Subject = _localizedStrings.Prompts.SupportEmailSubject;
            emailComposeTask.Show();
        }

    }
}