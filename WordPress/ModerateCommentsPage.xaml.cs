using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WordPress.Converters;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class ModerateCommentsPage : PhoneApplicationPage
    {
        #region member variables

        private ApplicationBarIconButton _deleteIconButton;
        private ApplicationBarIconButton _spamIconButton;
        private ApplicationBarIconButton _approveIconButton;
        private ApplicationBarIconButton _unapproveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public ModerateCommentsPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _deleteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _deleteIconButton.Text = _localizedStrings.ControlsText.Delete;
            _deleteIconButton.Click += OnDeleteIconButtonClick;

            _spamIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.spam.png", UriKind.Relative));
            _spamIconButton.Text = _localizedStrings.ControlsText.Spam;
            _spamIconButton.Click += OnSpamIconButtonClick;

            _approveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.approve.png", UriKind.Relative));
            _approveIconButton.Text = _localizedStrings.ControlsText.Approve;
            _approveIconButton.Click += OnApproveIconButtonClick;

            _unapproveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.unapprove.png", UriKind.Relative));
            _unapproveIconButton.Text = _localizedStrings.ControlsText.Unapprove;
            _unapproveIconButton.Click += OnUnapproveIconButtonClick;

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void OnCommentsPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = commentsPivot.SelectedIndex;
            ChangeApplicationBarAppearance(index);
        }

        private void ChangeApplicationBarAppearance(int index)
        {
            ApplicationBar.Buttons.Clear();

            ApplicationBar.Buttons.Add(_deleteIconButton);

            switch (index)
            {
                case 0:         //all
                    ApplicationBar.Buttons.Add(_spamIconButton);
                    ApplicationBar.Buttons.Add(_unapproveIconButton);
                    ApplicationBar.Buttons.Add(_approveIconButton);
                    break;
                case 1:         //approve
                    ApplicationBar.Buttons.Add(_spamIconButton);
                    ApplicationBar.Buttons.Add(_unapproveIconButton);
                    break;
                case 2:         //unapprove
                    ApplicationBar.Buttons.Add(_spamIconButton);
                    ApplicationBar.Buttons.Add(_approveIconButton);
                    break;
                case 3:         //spam
                    ApplicationBar.Buttons.Add(_approveIconButton);
                    break;
            }
        }

        private void OnDeleteIconButtonClick(object sender, EventArgs e)
        {
            ListBox activeListBox = FindActiveListBox();
            BatchDeleteComments(activeListBox.SelectedItems);
        }

        private void OnApproveIconButtonClick(object sender, EventArgs e)
        {
            ListBox activeListBox = FindActiveListBox();
            BatchApproveComments(activeListBox.SelectedItems);
        }

        private void OnUnapproveIconButtonClick(object sender, EventArgs e)
        {
            ListBox activeListBox = FindActiveListBox();
            BatchUnapproveComments(activeListBox.SelectedItems);
        }

        private void OnSpamIconButtonClick(object sender, EventArgs e)
        {
            ListBox activeListBox = FindActiveListBox();
            BatchSpamComments(activeListBox.SelectedItems);
        }

        private ListBox FindActiveListBox()
        {
            ListBox result = null;

            int index = commentsPivot.SelectedIndex;
            switch (index)
            {
                case 0:
                    result = allCommentsListBox;
                    break;
                case 1:
                    result = approvedCommentsListBox;
                    break;
                case 2:
                    result = unapprovedCommentsListBox;
                    break;
                case 3:
                    result = spamCommentsListBox;
                    break;
            }
            return result;
        }

        private void BatchDeleteComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

            DeleteCommentsRPC rpc = new DeleteCommentsRPC();
            rpc.Comments = ConvertList(comments);
            rpc.Completed += OnBatchDeleteXmlRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.DeletingComments);
        }

        private void BatchApproveComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

            EditCommentsStatusRPC rpc = new EditCommentsStatusRPC();
            rpc.CommentStatus = eCommentStatus.approve;
            rpc.Comments = ConvertList(comments);
            rpc.Completed += OnBatchEditXmlRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.ApprovingComments);
        }

        private void BatchUnapproveComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

            EditCommentsStatusRPC rpc = new EditCommentsStatusRPC();
            rpc.CommentStatus = eCommentStatus.hold;
            rpc.Comments = ConvertList(comments);
            rpc.Completed += OnBatchEditXmlRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UnapprovingComments);
        }

        private void BatchSpamComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

            EditCommentsStatusRPC rpc = new EditCommentsStatusRPC();
            rpc.CommentStatus = eCommentStatus.spam;
            rpc.Comments = ConvertList(comments);
            rpc.Completed += OnBatchEditXmlRPCCompleted;
            rpc.ExecuteAsync();

            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.MarkingCommentsAsSpam);
        }

        private IList<Comment> ConvertList(IList comments)
        {
            List<Comment> result = new List<Comment>();
            foreach (Comment c in comments)
            {
                result.Add(c);
            }
            return result;
        }

        private void OnBatchEditXmlRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            EditCommentsStatusRPC rpc = sender as EditCommentsStatusRPC;
            rpc.Completed -= OnBatchEditXmlRPCCompleted;

            App.WaitIndicationService.HideIndicator();

            UpdateDisplay();
        }

        private void OnBatchDeleteXmlRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            DeleteCommentsRPC rpc = sender as DeleteCommentsRPC;
            rpc.Completed -= OnBatchDeleteXmlRPCCompleted;

            App.WaitIndicationService.HideIndicator();

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            //TODO: figure out how to bind this in the xaml.  The lists dont refresh
            //properly when comments change their CommentStatus value, most likely due to the 
            //new collection created by the converter.  
            ObservableCollection<Comment> comments = App.MasterViewModel.CurrentBlog.Comments;
            allCommentsListBox.ItemsSource = comments;

            CommentStatusGroupingConverter approvedCommentConverter = Resources["ApprovedCommentConverter"] as CommentStatusGroupingConverter;
            approvedCommentsListBox.ItemsSource = approvedCommentConverter.Convert(comments, typeof(IEnumerable<Comment>), null, null) as IEnumerable;

            CommentStatusGroupingConverter unapprovedCommentConverter = Resources["UnapprovedCommentConverter"] as CommentStatusGroupingConverter;
            unapprovedCommentsListBox.ItemsSource = unapprovedCommentConverter.Convert(comments, typeof(IEnumerable<Comment>), null, null) as IEnumerable;

            CommentStatusGroupingConverter spamCommentConverter = Resources["SpamCommentConverter"] as CommentStatusGroupingConverter;
            spamCommentsListBox.ItemsSource = spamCommentConverter.Convert(comments, typeof(IEnumerable<Comment>), null, null) as IEnumerable;
        }

        #endregion

    }
}