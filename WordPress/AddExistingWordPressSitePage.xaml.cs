using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

using WordPress.Model;

namespace WordPress
{
    public partial class AddExistingWordPressSitePage : PhoneApplicationPage
    {
        #region member variables

        private const string URLKEY_VALUE = "url";
        private const string USERNAMEKEY_VALUE = "username";
        private const string PASSWORDKEY_VALUE = "password";

        #endregion

        #region constructor

        public AddExistingWordPressSitePage()
        {
            InitializeComponent();
        }

        #endregion

        #region properties

        #endregion

        #region methods

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptToLoginAsync();
        }

        private void AttemptToLoginAsync()
        {
            if (!ValidateUserName())
            {
                PromptUserForInput("Please enter a user name", usernameTextBox);
                return;
            }

            if (!ValidatePassword())
            {
                PromptUserForInput("Please enter a password", passwordPasswordBox);
                return;
            }

            if (!ValidateUrl())
            {
                PromptUserForInput("Please enter a url", urlTextBox);
            }

            string username = usernameTextBox.Text;
            string password = passwordPasswordBox.Password;
            string url = urlTextBox.Text;

            GetUsersBlogsRPC rpc = new GetUsersBlogsRPC(url, username, password);
            rpc.Completed += OnGetUsersBlogsCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator("Attempting to log-in...");
        }

        /// <summary>
        /// If true, the user input for "user name" is valid.
        /// </summary>
        /// <returns></returns>
        private bool ValidateUserName()
        {
            bool result = !string.IsNullOrEmpty(usernameTextBox.Text);
            return result;
        }

        /// <summary>
        /// If true, the user input for "password" is valid.
        /// </summary>
        /// <returns></returns>
        private bool ValidatePassword()
        {
            bool result = !string.IsNullOrEmpty(passwordPasswordBox.Password);
            return result;
        }

        private bool ValidateUrl()
        {
            bool result = !string.IsNullOrEmpty(urlTextBox.Text);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="textbox"></param>
        private void PromptUserForInput(string message, Control control)
        {
            MessageBox.Show(message);
            control.Focus();
        }

        private void OnGetUsersBlogsCompleted(object sender, XMLRPCCompletedEventArgs<Blog> args)
        {
            GetUsersBlogsRPC rpc = sender as GetUsersBlogsRPC;
            rpc.Completed -= OnGetUsersBlogsCompleted;

            if (null == args.Error)
            {
                Blog blog = args.Items[0];
                DataStore.Instance.Blogs.Add(blog);
                NavigationService.Navigate(new Uri("/BlogsPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show(args.Error.Message);
            }

            App.WaitIndicationService.HideIndicator();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            //retrieve transient data from State dictionary
            if (State.ContainsKey(URLKEY_VALUE))
            {
                urlTextBox.Text = (string)State[URLKEY_VALUE];
            }

            if (State.ContainsKey(USERNAMEKEY_VALUE))
            {
                usernameTextBox.Text = (string)State[USERNAMEKEY_VALUE];
            }

            if (State.ContainsKey(PASSWORDKEY_VALUE))
            {
                passwordPasswordBox.Password = (string)State[PASSWORDKEY_VALUE];
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //store transient data in State dictionary
            if (State.ContainsKey(URLKEY_VALUE))
            {
                State.Remove(URLKEY_VALUE);
            }
            State.Add(URLKEY_VALUE, urlTextBox.Text);
            
            if (State.ContainsKey(USERNAMEKEY_VALUE))
            {
                State.Remove(USERNAMEKEY_VALUE);
            }
            State.Add(USERNAMEKEY_VALUE, usernameTextBox.Text);

            if (State.ContainsKey(PASSWORDKEY_VALUE))
            {
                State.Remove(PASSWORDKEY_VALUE);
            }
            State.Add(PASSWORDKEY_VALUE, passwordPasswordBox.Password);
        }

        #endregion
    }
}