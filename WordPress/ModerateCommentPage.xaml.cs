using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

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
            ApplicationBar.IsVisible = true;

            _deleteIconButton = new ApplicationBarIconButton(new Uri("/Images/icon_delete.png", UriKind.Relative));
            _deleteIconButton.Text = _localizedStrings.ControlsText.Delete;
            _deleteIconButton.Click += deleteIconButton_Click;

            _replyIconButton = new ApplicationBarIconButton(new Uri("/Images/icon_edit.png", UriKind.Relative));
            _replyIconButton.Text = _localizedStrings.ControlsText.Reply;
            _replyIconButton.Click += replyIconButton_Click;

            //TODO: need to acquire images for spam, approve, and unapprove
            //NOTE: not providing an image uri seems to cause an exception when the buttons
            //are added to the ApplicationBar
            _spamIconButton = new ApplicationBarIconButton(new Uri("/Images/icon_delete.png", UriKind.Relative));
            _spamIconButton.Text = _localizedStrings.ControlsText.Spam;
            _spamIconButton.Click += spamIconButton_Click;

            _approveIconButton = new ApplicationBarIconButton(new Uri("/Images/icon_delete.png", UriKind.Relative));
            _approveIconButton.Text = _localizedStrings.ControlsText.Approve;
            _approveIconButton.Click += approveIconButton_Click;

            _unapproveIconButton = new ApplicationBarIconButton(new Uri("/Images/icon_delete.png", UriKind.Relative));
            _unapproveIconButton.Text = _localizedStrings.ControlsText.Unapprove;
            _unapproveIconButton.Click += unapproveIconButton_Click;
        }
        
        #endregion

        #region methods

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

        private void replyIconButton_Click(object sender, EventArgs e)
        {
            ShowReplyPanel();
        }

        private void ShowReplyPanel()
        {
            replyPanel.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = false;

            Storyboard fadeInStoryboard = CreateEaseInAnimationStoryBoard(replyPanel, Grid.OpacityProperty, 0.0, 0.97, TimeSpan.FromMilliseconds(250));
            fadeInStoryboard.Begin();
        }

        private Storyboard CreateEaseInAnimationStoryBoard(DependencyObject target, DependencyProperty targetProperty, Double from, Double to, TimeSpan duration)
        {
            Storyboard animationStoryboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = duration,
                From = from,
                To = to,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            animationStoryboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(targetProperty));
            Storyboard.SetTarget(animation, target);
            return animationStoryboard;
        }

        private Storyboard CreateEaseOutAnimationStoryBoard(DependencyObject target, DependencyProperty targetProperty, Double from, Double to, TimeSpan duration)
        {
            Storyboard animationStoryboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = duration,
                From = from,
                To = to,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            animationStoryboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(targetProperty));
            Storyboard.SetTarget(animation, target);
            return animationStoryboard;
        }

        private void HideReplyPanel()
        {
            Storyboard fadeOutStoryboard = CreateEaseOutAnimationStoryBoard(replyPanel, Grid.OpacityProperty, 0.97, 0, TimeSpan.FromMilliseconds(250));
            fadeOutStoryboard.BeginTime = TimeSpan.FromMilliseconds(250);
            fadeOutStoryboard.Completed += OnFadeOutStoryboardCompleted;
            fadeOutStoryboard.Begin();
        }

        void OnFadeOutStoryboardCompleted(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = true;
            replyPanel.Visibility = Visibility.Collapsed;
            replyTextBox.Text = string.Empty;

            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnFadeOutStoryboardCompleted;
        }

        private void spamIconButton_Click(object sender, EventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.spam;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.MarkingAsSpam);
        }

        private void unapproveIconButton_Click(object sender, EventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.hold;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UnapprovingComment);
        }

        private void approveIconButton_Click(object sender, EventArgs e)
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
                HandleError(args.Error);
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
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

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

        private void deleteIconButton_Click(object sender, EventArgs e)
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
                HandleError(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void HandleError(Exception exception)
        {
            //TODO: clean this up...
            MessageBox.Show(exception.Message);
        }

        private void replyButton_Click(object sender, RoutedEventArgs e)
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
                DataStore.Instance.FetchCurrentBlogCommentsAsync();

                NavigationService.GoBack();
            }
            else
            {
                HandleError(args.Error);
            }
        }

        #endregion


    }
}