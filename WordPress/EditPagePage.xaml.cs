using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class EditPagePage : PhoneApplicationPage
    {
        //DEV NOTE: as far as the WP data model goes, there isn't a real difference
        //between a post and a page, so we use the "post" rpcs
        #region member variables

        private const string DATACONTEXT_VALUE = "dataContext";
        private const string TITLEKEY_VALUE = "title";
        private const string CONTENTKEY_VALUE = "content";
        private const string PUBLISHKEY_VALUE = "publish";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        #endregion

        #region constructors

        public EditPagePage()
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

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            if (!State.ContainsKey(TITLEKEY_VALUE))
            {
                LoadPage();
            }
        }

        private void LoadPage()
        {
            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            if (null != App.MasterViewModel.CurrentPageListItem)
            {
                string pageId = App.MasterViewModel.CurrentPageListItem.PageId.ToString();

                GetPostRPC rpc = new GetPostRPC(currentBlog, pageId);
                rpc.Completed += OnGetPostRPCCompleted;
                rpc.ExecuteAsync();

                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPage);
            }
            else
            {
                DataContext = new Post();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            RestorePageState();
        }

        private void OnGetPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPostRPCCompleted;

            if (null == args.Error)
            {
                Post post = args.Items[0];
                DataContext = post;
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            Post post = DataContext as Post;

            if (post.IsNew)
            {
                NewPostRPC rpc = new NewPostRPC(App.MasterViewModel.CurrentBlog, post);
                rpc.Publish = publishToggleButton.IsChecked.Value;
                rpc.PostType = ePostType.page;
                rpc.Completed += OnNewPostRPCCompleted;

                rpc.ExecuteAsync();
            }
            else
            {
                EditPostRPC rpc = new EditPostRPC(App.MasterViewModel.CurrentBlog, post);
                rpc.Publish = publishToggleButton.IsChecked.Value;
                rpc.Completed += OnEditPostRPCCompleted;

                rpc.ExecuteAsync();
            }
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingChanges);
        }

        private void OnEditPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            EditPostRPC rpc = sender as EditPostRPC;
            rpc.Completed -= OnEditPostRPCCompleted;

            if (null == args.Error)
            {
                DataStore.Instance.FetchCurrentBlogPagesAsync();
                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnNewPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            NewPostRPC rpc = sender as NewPostRPC;
            rpc.Completed -= OnNewPostRPCCompleted;

            if (null == args.Error)
            {
                DataStore.Instance.FetchCurrentBlogPagesAsync();
                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }
        
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SavePageState();
        }

        private void SavePageState()
        {
            //store transient data in the State dictionary
            if (State.ContainsKey(DATACONTEXT_VALUE))
            {
                State.Remove(DATACONTEXT_VALUE);
            }
            State.Add(DATACONTEXT_VALUE, DataContext);

            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                State.Remove(TITLEKEY_VALUE);
            }
            State.Add(TITLEKEY_VALUE, titleTextBox.Text);

            if (State.ContainsKey(CONTENTKEY_VALUE))
            {
                State.Remove(CONTENTKEY_VALUE);
            }
            State.Add(CONTENTKEY_VALUE, contentTextBox.Text);

            if (State.ContainsKey(PUBLISHKEY_VALUE))
            {
                State.Remove(PUBLISHKEY_VALUE);
            }
            State.Add(PUBLISHKEY_VALUE, publishToggleButton.IsChecked);
        }

        private void RestorePageState()
        {
            //look for transient data stored in the State dictionary
            if (State.ContainsKey(DATACONTEXT_VALUE))
            {
                DataContext = State[DATACONTEXT_VALUE];
            }

            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                titleTextBox.Text = State[TITLEKEY_VALUE] as string;
            }

            if (State.ContainsKey(CONTENTKEY_VALUE))
            {
                contentTextBox.Text = State[CONTENTKEY_VALUE] as string;
            }

            if (State.ContainsKey(PUBLISHKEY_VALUE))
            {
                publishToggleButton.IsChecked = (bool)State[PUBLISHKEY_VALUE];
            }
        }

        #endregion
    }
}