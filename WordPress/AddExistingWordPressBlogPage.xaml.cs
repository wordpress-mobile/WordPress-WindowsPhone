using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class AddExistingWordPressBlogPage : PhoneApplicationPage
    {
        #region member variables

        private const string USERNAMEKEY_VALUE = "username";
        private const string PASSWORDKEY_VALUE = "password";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public AddExistingWordPressBlogPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);
        }

        #endregion

        #region properties

        #endregion

        #region methods

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            AttemptToLoginAsync();
        }

        private void AttemptToLoginAsync()
        {
            if (!ValidateUserName())
            {
                PromptUserForInput(_localizedStrings.Prompts.MissingUserName, usernameTextBox);
                return;
            }

            if (!ValidatePassword())
            {
                PromptUserForInput(_localizedStrings.Prompts.MissingPassword, passwordPasswordBox);
                return;
            }
            string username = usernameTextBox.Text;
            string password = passwordPasswordBox.Password;
            string url = Constants.WORDPRESS_XMLRPC_URL;

            GetUsersBlogsRPC rpc = new GetUsersBlogsRPC(url, username, password);
            rpc.Completed += OnGetUsersBlogsCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.LoggingIn);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="control"></param>
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
                //TODO: if multiple blogs are returned, ask the user to select which blogs 
                //to add to the data store
                foreach (Blog blog in args.Items)
                {                    
                    DataStore.Instance.Blogs.Add(blog);
                }
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

            //retreive transient data from State dictionary
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

        private void OnCreateNewBlogButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BrowserShellPage.xaml?uri=http://wordpress.com/signup", UriKind.Relative));
        }

        #endregion
        
    }
}