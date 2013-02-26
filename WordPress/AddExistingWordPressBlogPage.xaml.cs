using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

using WordPress.Commands;
using WordPress.Localization;
using WordPress.Model;
using System.Windows.Input;
using WordPress.Utils;

namespace WordPress
{
    public partial class AddExistingWordPressBlogPage : PhoneApplicationPage
    {
        #region member variables

        private const string USERNAMEKEY_VALUE = "username";
        private const string PASSWORDKEY_VALUE = "password";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;
        private GetUsersBlogsRPC rpc;

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

        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == usernameTextBox)
                    passwordPasswordBox.Focus();
                else if (sender == passwordPasswordBox)
                {
                    AttemptToLoginAsync();
                }
            }
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            AttemptToLoginAsync();
        }

        private void AttemptToLoginAsync()
        {
            this.Focus();

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

            rpc = new GetUsersBlogsRPC(url, username, password);
            rpc.Completed += OnGetUsersBlogsCompleted;
            rpc.ExecuteAsync();

            ApplicationBar.IsVisible = false;
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

        private void PromptUserForInput(string message, Control control)
        {
            MessageBox.Show(message);
            control.Focus();
        }

        private void OnGetUsersBlogsCompleted(object sender, XMLRPCCompletedEventArgs<Blog> args)
        {
            GetUsersBlogsRPC rpc = sender as GetUsersBlogsRPC;
            rpc.Completed -= OnGetUsersBlogsCompleted;
            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.KillSpinner();

            if (args.Cancelled)
            {
            } 
            else if (null == args.Error)
            {
                if (1 == args.Items.Count)
                {
                    if (!(DataService.Current.Blogs.Any(b => b.Xmlrpc == args.Items[0].Xmlrpc)))
                    {
                        DataService.Current.AddBlogToStore(args.Items[0]);
                    }
                    NavigationService.Navigate(new Uri("/BlogsPage.xaml", UriKind.Relative));
                }
                else
                {
                    ShowBlogSelectionControl(args.Items);
                }
            }
            else
            {
                Exception currentException = args.Error;
                if (currentException is XmlRPCException && (currentException as XmlRPCException).FaultCode == 403) //username or password error
                {
                    UIThread.Invoke(() =>
                    {
                        MessageBox.Show(_localizedStrings.Prompts.UsernameOrPasswordError);
                    });
                }
                else
                    this.HandleException(args.Error);
            }
        }

        private void ShowBlogSelectionControl(List<Blog> items)
        {
            ApplicationBar.IsVisible = false;
            ContentPanel.Visibility = Visibility.Collapsed;
            blogSelectionControl.Visibility = Visibility.Visible;
            blogSelectionControl.Blogs = items;
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

            HideBlogSelectionControl();
        }

        private void HideBlogSelectionControl()
        {
            blogSelectionControl.Visibility = Visibility.Collapsed;
            blogSelectionControl.Blogs = null;             
            ContentPanel.Visibility = Visibility.Visible;            
            ApplicationBar.IsVisible = true;
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
            LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_SIGNUP_URL);
        }
        
        private void OnBlogsSelected(object sender, RoutedEventArgs e)
        {
            blogSelectionControl.SelectedItems.ForEach(blog =>
            {
                if (!(DataService.Current.Blogs.Any(b => b.Xmlrpc == blog.Xmlrpc)))
                {
                    DataService.Current.AddBlogToStore(blog);
                }
            });

            NavigationService.Navigate(new Uri("/BlogsPage.xaml", UriKind.Relative));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (App.WaitIndicationService.Waiting)
            {
                if (rpc != null)
                {
                    rpc.Completed -= OnGetUsersBlogsCompleted;
                }
                App.WaitIndicationService.KillSpinner();
                HideBlogSelectionControl();
                e.Cancel = true;
            }
            
            if (Visibility.Visible == blogSelectionControl.Visibility)
            {
                HideBlogSelectionControl();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        #endregion

        
    }
}