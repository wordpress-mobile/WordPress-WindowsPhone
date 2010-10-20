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

using WordPress.Model;

namespace WordPress
{
    public partial class ModerateCommentPage : PhoneApplicationPage
    {
        #region member variables

        private string COMMENTKEY_VALUE = "comment";

        #endregion

        #region constructor

        public ModerateCommentPage()
        {
            InitializeComponent();
        }

        #endregion

        #region methods

        private void replyButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void spamButton_Click(object sender, RoutedEventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.spam;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator("Marking comment as spam...");
        }

        private void unapproveButton_Click(object sender, RoutedEventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.hold;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator("Unapproving comment...");
        }

        private void approveButton_Click(object sender, RoutedEventArgs e)
        {
            Comment comment = DataContext as Comment;
            comment.CommentStatus = eCommentStatus.approve;

            EditCommentRPC rpc = new EditCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnEditCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator("Approving comment...");
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

            //change the available options based on the status of the comment
            comment = DataContext as Comment;
            switch (comment.CommentStatus)
            {
                case eCommentStatus.approve:
                    approveButton.Visibility = Visibility.Collapsed;
                    unapproveButton.Visibility = Visibility.Visible;
                    spamButton.Visibility = Visibility.Visible;
                    break;
                case eCommentStatus.hold:
                    unapproveButton.Visibility = Visibility.Collapsed;
                    approveButton.Visibility = Visibility.Visible;
                    spamButton.Visibility = Visibility.Visible;
                    break;
                case eCommentStatus.spam:
                    unapproveButton.Visibility = Visibility.Collapsed;
                    spamButton.Visibility = Visibility.Collapsed;
                    approveButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Comment comment = DataContext as Comment;
            
            DeleteCommentRPC rpc = new DeleteCommentRPC(App.MasterViewModel.CurrentBlog, comment);
            rpc.Completed += OnDeleteCommentRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator("Deleting comment...");
        }

        private void OnDeleteCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            DeleteCommentRPC rpc = sender as DeleteCommentRPC;
            rpc.Completed -= OnDeleteCommentRPCCompleted;

            if (null == args.Error)
            {
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