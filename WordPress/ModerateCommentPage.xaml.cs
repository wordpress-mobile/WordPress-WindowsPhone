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
        private string REPLYPANELVISIBLE_VALUE = "replyPanelVisible";
        private string REPLYTEXTBOXTEXT_VALUE = "replyTextBoxText";

        private ApplicationBarIconButton _deleteIconButton;
        private ApplicationBarIconButton _replyIconButton;
        private ApplicationBarIconButton _spamIconButton;
        private ApplicationBarIconButton _approveIconButton;
        private ApplicationBarIconButton _unapproveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructor

        public ModerateCommentPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            replyPanel.Visibility = Visibility.Collapsed;

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

            authorEmailTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(authorEmailTextBlock_MouseLeftButtonDown);
            authorURLTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(authorURLTextBlock_MouseLeftButtonDown);
            Loaded += OnPageLoaded;
        }
        
        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            ChangeApplicationBarAppearance();

            //now that the application bar is in the right visual state, check for any
            //stored data for a reply
            if (State.ContainsKey(REPLYPANELVISIBLE_VALUE))
            {
                if (State.ContainsKey(REPLYTEXTBOXTEXT_VALUE))
                {
                    replyTextBox.Text = State[REPLYTEXTBOXTEXT_VALUE] as string;
                }
                ShowReplyPanel();
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Visibility.Visible == replyPanel.Visibility)
            {
                HideReplyPanel();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void OnReplyIconButtonClick(object sender, EventArgs e)
        {
            ShowReplyPanel();
        }

        private void ShowReplyPanel()
        {
            replyPanel.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = false;

            Storyboard fadeInStoryboard = AnimationHelper.CreateEaseInAnimationStoryBoard(replyPanel, Grid.OpacityProperty, 0.0, 0.97, TimeSpan.FromMilliseconds(250));
            fadeInStoryboard.Begin();
        }

        private void HideReplyPanel()
        {
            Storyboard fadeOutStoryboard = AnimationHelper.CreateEaseOutAnimationStoryBoard(replyPanel, Grid.OpacityProperty, 0.97, 0, TimeSpan.FromMilliseconds(250));
            fadeOutStoryboard.BeginTime = TimeSpan.FromMilliseconds(250);
            fadeOutStoryboard.Completed += OnFadeOutStoryboardCompleted;
            fadeOutStoryboard.Begin();
        }

        private void OnFadeOutStoryboardCompleted(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = true;
            replyPanel.Visibility = Visibility.Collapsed;
            replyTextBox.Text = string.Empty;

            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnFadeOutStoryboardCompleted;
        }

        private void OnSpamIconButtonClick(object sender, EventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.spam;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.MarkingAsSpam);
        }

        private void OnUnapproveIconButtonClick(object sender, EventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.hold;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UnapprovingComment);
        }

        private void OnApproveIconButtonClick(object sender, EventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.approve;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.ApprovingComment);
        }


        private void OnEditCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            EditCommentRPC rpc = sender as EditCommentRPC;
            rpc.Completed -= OnEditCommentRPCCompleted;

            if (null == args.Error)
            {
                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
            ChangeApplicationBarAppearance();
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
            if (Visibility.Visible == replyPanel.Visibility)
            {
                State.Add(REPLYPANELVISIBLE_VALUE, Visibility.Visible);
                State.Add(REPLYTEXTBOXTEXT_VALUE, replyTextBox.Text);
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
            Comment comment = DataContext as Comment;
            
            DeleteCommentRPC rpc = new DeleteCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnDeleteCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingComment);
        }

        private void OnDeleteCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            DeleteCommentRPC rpc = sender as DeleteCommentRPC;
            rpc.Completed -= OnDeleteCommentRPCCompleted;

            if (null == args.Error)
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
        }
        
        private void OnReplyButtonClick(object sender, RoutedEventArgs e)
        {
            ReplyToComment();
        }

        private void ReplyToComment()
        {
            if (string.IsNullOrEmpty(replyTextBox.Text))
            {
                MessageBox.Show(_localizedStrings.Messages.MissingReply);
                replyTextBox.Focus();
                return;
            }

            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            Comment comment = DataContext as Comment;

            Comment reply = new Comment()
            {
                Author = currentBlog.Username,
                Parent = comment.CommentId,                
                Content = replyTextBox.Text
            };

            NewCommentRPC rpc = new NewCommentRPC(currentBlog, comment, reply);
            rpc.Completed += new XMLRPCCompletedEventHandler<Comment>(OnNewCommentRPCCompleted);
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.ReplyingToComment);
        }

        private void OnNewCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            NewCommentRPC rpc = sender as NewCommentRPC;
            rpc.Completed -= OnNewCommentRPCCompleted;

            if (null == args.Error)
            {
                //fire off a request for the latest comment so we can get our comment updated
                //with the latest from the server.
                DataService.Current.FetchCurrentBlogCommentsAsync();

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
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = comment.AuthorEmail;
            emailcomposer.Subject = String.Format("Re: {0}", comment.PostTitle);
            emailcomposer.Body = String.Format("{0} {1},", _localizedStrings.Messages.Hello, comment.Author);
            emailcomposer.Show();
        }

        private void authorURLTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Comment comment = DataContext as Comment;
            WebBrowserTask wbTask = new WebBrowserTask();
            wbTask.URL = comment.AuthorUrl;
            wbTask.Show();
        }

        #endregion


    }
}