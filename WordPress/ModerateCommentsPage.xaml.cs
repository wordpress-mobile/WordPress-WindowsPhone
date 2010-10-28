using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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
            ApplicationBar.IsVisible = true;

            _deleteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _deleteIconButton.Text = _localizedStrings.ControlsText.Delete;
            _deleteIconButton.Click += OnDeleteIconButtonClick;

            //TODO: need to acquire images for spam, approve, and unapprove
            //NOTE: not providing an image uri seems to cause an exception when the buttons
            //are added to the ApplicationBar
            _spamIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _spamIconButton.Text = _localizedStrings.ControlsText.Spam;
            _spamIconButton.Click += OnSpamIconButtonClick;

            _approveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _approveIconButton.Text = _localizedStrings.ControlsText.Approve;
            _approveIconButton.Click += OnApproveIconButtonClick;

            _unapproveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            _unapproveIconButton.Text = _localizedStrings.ControlsText.Unapprove;
            _unapproveIconButton.Click += OnUnapproveIconButtonClick;
        }

        #endregion

        #region methods

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
        }

        private void BatchApproveComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

        }

        private void BatchUnapproveComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;

        }

        private void BatchSpamComments(IList comments)
        {
            if (null == comments || 0 == comments.Count) return;            
        }

        #endregion

    }
}