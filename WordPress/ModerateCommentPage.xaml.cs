using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WordPress.Localization;
using WordPress.Model;
using Microsoft.Phone.Tasks;

namespace WordPress
{
    public partial class ModerateCommentPage : PhoneApplicationPage
    {
        #region member variables

        private string COMMENTKEY_VALUE = "comment";
        private string REPLYEDITPANELVISIBLE_VALUE = "replyEditPanelVisible";
        private string REPLYEDITTEXTBOXTEXT_VALUE = "replyEditTextBoxText";
        private string REPLYEDITPANELMODE_VALUE = "replyEditPanelMode";

        private ApplicationBarIconButton _deleteIconButton;
        private ApplicationBarIconButton _replyIconButton;
        private ApplicationBarIconButton _spamIconButton;
        private ApplicationBarIconButton _approveIconButton;
        private ApplicationBarIconButton _unapproveIconButton;
        private StringTable _localizedStrings;
        private bool _messageBoxIsShown = false;
        IXmlRemoteProcedureCall _currentConnection = null;

        private bool _isEditing = false;

        #endregion

        #region constructor

        public ModerateCommentPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            replyEditPanel.Visibility = Visibility.Collapsed;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _deleteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _deleteIconButton.Text = _localizedStrings.ControlsText.Delete;
            _deleteIconButton.Click += OnDeleteIconButtonClick;

            _replyIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.edit.png", UriKind.Relative));
            _replyIconButton.Text = _localizedStrings.ControlsText.Reply;
            _replyIconButton.Click += OnReplyIconButtonClick;

            _spamIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.spam.png", UriKind.Relative));
            _spamIconButton.Text = _localizedStrings.ControlsText.Spam;
            _spamIconButton.Click += OnSpamIconButtonClick;

            _approveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.approve.png", UriKind.Relative));
            _approveIconButton.Text = _localizedStrings.ControlsText.Approve;
            _approveIconButton.Click += OnApproveIconButtonClick;

            _unapproveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.unapprove.png", UriKind.Relative));
            _unapproveIconButton.Text = _localizedStrings.ControlsText.Unapprove;
            _unapproveIconButton.Click += OnUnapproveIconButtonClick;

            ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem();
            menuItem.Text = _localizedStrings.ControlsText.EditComment;
            menuItem.Click += OnEditCommentMenuItemClick;
            ApplicationBar.MenuItems.Add(menuItem);

            authorEmailTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(authorEmailTextBlock_MouseLeftButtonDown);
            authorURLTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(authorURLTextBlock_MouseLeftButtonDown);
            Loaded += OnPageLoaded;
        }
        
        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            BlogName.Text = App.MasterViewModel.CurrentBlog.BlogNameUpper;

            ChangeApplicationBarAppearance();

            //now that the application bar is in the right visual state, check for any
            //stored data for a reply
            if (State.ContainsKey(REPLYEDITPANELVISIBLE_VALUE))
            {
                if (State.ContainsKey(REPLYEDITTEXTBOXTEXT_VALUE))
                {
                    replyEditTextBox.Text = State[REPLYEDITTEXTBOXTEXT_VALUE] as string;
                    _isEditing =  Convert.ToBoolean(State[REPLYEDITPANELMODE_VALUE] as string);
                }
                ShowReplyEditPanel(_isEditing);
            }

        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {

            if (App.WaitIndicationService.Waiting)
            {
                if (null != _currentConnection)
                {
                    _currentConnection.IsCancelled = true;
                    _currentConnection = null;
                }
                App.WaitIndicationService.HideIndicator();
                ApplicationBar.IsVisible = true;
                ChangeApplicationBarAppearance();
                e.Cancel = true;
            }
            else if (Visibility.Visible == replyEditPanel.Visibility)
            {
                HideReplyEditPanel();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void OnEditCommentMenuItemClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }

            // Set the textbox text here and not in ShowReplyEditPanel so 
            // changes are not overwritten when resuming from the background.
            Comment comment = DataContext as Comment;
            replyEditTextBox.Text = comment.Content;
            ShowReplyEditPanel(true);
        }

        private void OnReplyIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }

            replyEditTextBox.Text = "";
            ShowReplyEditPanel(false);
        }

        private void ShowReplyEditPanel(bool forEditing)
        {
            _isEditing = forEditing;
            if (_isEditing)
            {
                replyEditLabel.Text = _localizedStrings.ControlsText.EditComment;
            }
            else
            {
                replyEditLabel.Text = _localizedStrings.ControlsText.ReplyToComment;
            }

            replyEditPanel.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = false;

            Storyboard fadeInStoryboard = AnimationHelper.CreateEaseInAnimationStoryBoard(replyEditPanel, Grid.OpacityProperty, 0.0, 0.97, TimeSpan.FromMilliseconds(250));
            fadeInStoryboard.Completed += OnFadeInCommentReplyStoryboardCompleted;
            fadeInStoryboard.Begin();
        }

        private void OnFadeInCommentReplyStoryboardCompleted(object sender, EventArgs e)
        {
            replyEditTextBox.Focus();
            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnFadeInCommentReplyStoryboardCompleted;
        }

        private void HideReplyEditPanel()
        {
            Storyboard fadeOutStoryboard = AnimationHelper.CreateEaseOutAnimationStoryBoard(replyEditPanel, Grid.OpacityProperty, 0.97, 0, TimeSpan.FromMilliseconds(250));
            fadeOutStoryboard.BeginTime = TimeSpan.FromMilliseconds(250);
            fadeOutStoryboard.Completed += OnFadeOutStoryboardCompleted;
            fadeOutStoryboard.Begin();
        }

        private void OnFadeOutStoryboardCompleted(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = true;
            replyEditPanel.Visibility = Visibility.Collapsed;
            replyEditTextBox.Text = string.Empty;

            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnFadeOutStoryboardCompleted;
        }

        private void OnSpamIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            string prompt = _localizedStrings.Prompts.ConfirmMarkSpamComment;
            if (_messageBoxIsShown)
                return;
            _messageBoxIsShown = true;
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.Confirm, MessageBoxButton.OKCancel);
            _messageBoxIsShown = false;
            if (result == MessageBoxResult.OK)
            {
                Comment comment = DataContext as Comment;
                comment.CommentStatus = eCommentStatus.spam;

                EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
                rpc.Completed += OnEditCommentRPCCompleted;
                rpc.ExecuteAsync();
                _currentConnection = rpc;

                ApplicationBar.IsVisible = false;
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.MarkingAsSpam);
            }
            else
            {
                return;
            }
        }

        private void OnUnapproveIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.hold;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();
            _currentConnection = rpc;

            ApplicationBar.IsVisible = false;
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UnapprovingComment);
        }

        private void OnApproveIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.approve;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();
            _currentConnection = rpc;

            ApplicationBar.IsVisible = false;
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.ApprovingComment);
        }


        private void OnEditCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            EditCommentRPC rpc = sender as EditCommentRPC;
            rpc.Completed -= OnEditCommentRPCCompleted;

            if (args.Cancelled)
            {
                return;
            }

            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.HideIndicator();
            if (null == args.Error)
            {
                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
                ChangeApplicationBarAppearance();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //store transient data in the State dictionary
            if (State.ContainsKey(COMMENTKEY_VALUE))
            {
                State.Remove(COMMENTKEY_VALUE);
            }
            Comment comment = DataContext as Comment;
            State.Add(COMMENTKEY_VALUE, comment);

            //save reply data if it is active
            if (Visibility.Visible == replyEditPanel.Visibility)
            {
                State.Add(REPLYEDITPANELVISIBLE_VALUE, Visibility.Visible);
                State.Add(REPLYEDITTEXTBOXTEXT_VALUE, replyEditTextBox.Text);
                State.Add(REPLYEDITPANELMODE_VALUE, Convert.ToString(_isEditing));
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Comment comment = null;

            //check for transient data in the State dictionary
            if (State.ContainsKey(COMMENTKEY_VALUE))
            {
                comment = (Comment)State[COMMENTKEY_VALUE];
                DataContext = comment;
            }
            else
            {
                DataContext = App.MasterViewModel.CurrentComment;
            }

            State.Clear();
        }

        private void ChangeApplicationBarAppearance()
        {
            //change the available options based on the status of the comment
            Comment comment = DataContext as Comment;

            ApplicationBar.Buttons.Clear();

            ApplicationBar.Buttons.Add(_deleteIconButton);
            ApplicationBar.Buttons.Add(_replyIconButton);

            switch (comment.CommentStatus)
            {
                case eCommentStatus.approve:
                    ApplicationBar.Buttons.Add(_spamIconButton);
                    ApplicationBar.Buttons.Add(_unapproveIconButton);
                    break;
                case eCommentStatus.hold:
                    ApplicationBar.Buttons.Add(_spamIconButton);                    
                    ApplicationBar.Buttons.Add(_approveIconButton);
                    break;
                case eCommentStatus.spam:
                    ApplicationBar.Buttons.Add(_approveIconButton);
                    break;
            }
        }

        private void OnDeleteIconButtonClick(object sender, EventArgs e)
        {
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            string prompt = _localizedStrings.Prompts.ConfirmDeleteComment;
            if (_messageBoxIsShown)
                return;
            _messageBoxIsShown = true;
            MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.Confirm, MessageBoxButton.OKCancel);
            _messageBoxIsShown = false;
            if (result == MessageBoxResult.OK)
            {
                Comment comment = DataContext as Comment;

                DeleteCommentRPC rpc = new DeleteCommentRPC(App.MasterViewModel.CurrentBlog, comment);
                rpc.Completed += OnDeleteCommentRPCCompleted;
                rpc.ExecuteAsync();
                _currentConnection = rpc;

                ApplicationBar.IsVisible = false;
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingComment);
            }
            else
            {
                return;
            }
        }

        private void OnDeleteCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            DeleteCommentRPC rpc = sender as DeleteCommentRPC;
            rpc.Completed -= OnDeleteCommentRPCCompleted;

            if (args.Cancelled)
            {
                return;
            }
            else if (null == args.Error)
            {
                //remove the comment from the store--saves us a web call
                App.MasterViewModel.Comments.Remove(args.Items[0]);

                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
            ApplicationBar.IsVisible = true;
        }
        
        private void OnReplyEditButtonClick(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                EditComment();
            }
            else
            {
                ReplyToComment();
            }
        }

        private void EditComment()
        {
            if (string.IsNullOrEmpty(replyEditTextBox.Text))
            {
                if (_messageBoxIsShown)
                    return;
                _messageBoxIsShown = true;
                MessageBox.Show(_localizedStrings.Messages.MissingFields);
                _messageBoxIsShown = false;
                replyEditTextBox.Focus();
                return;
            }
            
            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }

            Comment comment = DataContext as Comment;
            comment.Content = replyEditTextBox.Text;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();
            _currentConnection = rpc;

            ApplicationBar.IsVisible = false;
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.EditingComment);
        }

        private void ReplyToComment()
        {
            if (string.IsNullOrEmpty(replyEditTextBox.Text))
            {
                if (_messageBoxIsShown)
                    return;
                _messageBoxIsShown = true;
                MessageBox.Show(_localizedStrings.Messages.MissingReply);
                _messageBoxIsShown = false;
                replyEditTextBox.Focus();
                return;
            }

            if (!App.isNetworkAvailable())
            {
                Exception connErr = new NoConnectionException();
                this.HandleException(connErr);
                return;
            }
            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            Comment comment = DataContext as Comment;

            Comment reply = new Comment()
            {
                Author = currentBlog.Username,
                Parent = comment.CommentId,                
                Content = replyEditTextBox.Text
            };

            NewCommentRPC rpc = new NewCommentRPC(currentBlog, comment, reply);
            rpc.Completed += new XMLRPCCompletedEventHandler<Comment>(OnNewCommentRPCCompleted);
            rpc.ExecuteAsync();
            _currentConnection = rpc;

            ApplicationBar.IsVisible = false;
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.ReplyingToComment);
        }

        private void OnNewCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            NewCommentRPC rpc = sender as NewCommentRPC;
            rpc.Completed -= OnNewCommentRPCCompleted;

            if (args.Cancelled)
            {
                return;
            }
            else if (null == args.Error)
            {
                //fire off a request for the latest comment so we can get our comment updated
                //with the latest from the server.
                DataService.Current.FetchCurrentBlogCommentsAsync(false);

                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
            }
        }

        private void authorEmailTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Comment comment = DataContext as Comment;
            if (comment.AuthorEmail == null || comment.AuthorEmail.Equals(""))
            {
                return; // No email address so don't show the compose task.
            }
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = comment.AuthorEmail;
            emailcomposer.Subject = String.Format("Re: {0}", comment.PostTitle);
            emailcomposer.Body = String.Format("{0} {1},", _localizedStrings.Messages.Hello, comment.Author);
            emailcomposer.Show();
        }

        private void authorURLTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Comment comment = DataContext as Comment;

            if (comment.AuthorUrl == null || comment.AuthorUrl.Equals(""))
            {
                return; // do nothing.
            }

            WebBrowserTask wbTask = new WebBrowserTask();
            try
            {
                // A url that is not a good URI will throw an exception.
                wbTask.Uri = new Uri(comment.AuthorUrl);
                wbTask.Show();
            }
            catch (Exception ex)
            {
                // Fail silently
            }
        }

        #endregion


    }
}