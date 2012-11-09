using System;
using System.Windows;
using Microsoft.Phone.Controls;

using WordPress.Converters;
using WordPress.Model;

namespace WordPress
{
    /// <summary>
    /// Displays all comments related to a post or page.  The "ID" value needs to be passed as
    /// a parameter in the query string.
    /// </summary>
    public partial class RelatedCommentsPage : PhoneApplicationPage
    {
        #region member variables

        public const string IDKEY_VALUE = "ID";
        
        #endregion

        #region constructors

        public RelatedCommentsPage()
        {
            InitializeComponent();

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

            string selectedId;
            if (NavigationContext.QueryString.TryGetValue("ID", out selectedId))
            {
                CommentsOnPostConverter converter = Resources["CommentsOnPostConverter"] as CommentsOnPostConverter;
                converter.Id = selectedId;
            }
        }

        private void OnCommentsListBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (null == commentsListBox.SelectedItem) return;

            App.MasterViewModel.CurrentComment = commentsListBox.SelectedItem as Comment;

            NavigationService.Navigate(new Uri("/ModerateCommentPage.xaml", UriKind.Relative));

            commentsListBox.SelectedIndex = -1;

        }

        #endregion

    }
}