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
using Microsoft.Phone.Shell;

using WordPress.Model;
using WordPress.Localization;

namespace WordPress
{
    public partial class ModerateCommentPage : PhoneApplicationPage
    {
        #region member variables

        private string COMMENTKEY_VALUE = "comment";

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

        private void replyIconButton_Click(object sender, EventArgs e)
        {
            
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

        #endregion

    }
}