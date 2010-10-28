﻿using System;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class EditPostPage : PhoneApplicationPage
    {
        #region member variables

        private const string POSTKEY_VALUE = "post";
        private const string PUBLISHKEY_VALUE = "publish";

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _cancelIconButton;
        private ApplicationBarIconButton _saveIconButton;

        #endregion

        #region constructors

        public EditPostPage()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _cancelIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.cancel.png", UriKind.Relative));
            _cancelIconButton.Text = _localizedStrings.ControlsText.Cancel;
            _cancelIconButton.Click += OnCancelButtonClick;
            ApplicationBar.Buttons.Add(_cancelIconButton);

            _saveIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.save.png", UriKind.Relative));
            _saveIconButton.Text = _localizedStrings.ControlsText.Save;
            _saveIconButton.Click += OnSaveButtonClick;
            ApplicationBar.Buttons.Add(_saveIconButton);
        }

        #endregion

        #region methods

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //check for transient data stored in State dictionary
            if (State.ContainsKey(POSTKEY_VALUE))
            {
                RestorePageState();
            }
            else
            {
                LoadBlog();
            }
        }

        /// <summary>
        /// Retrieves transient data from the page's State dictionary
        /// </summary>
        private void RestorePageState()
        {
            if (State.ContainsKey(POSTKEY_VALUE))
            {
                Post post = State[POSTKEY_VALUE] as Post;
                DataContext = post;
            }

            if (State.ContainsKey(PUBLISHKEY_VALUE))
            {
                publishToggleButton.IsChecked = (bool)State[PUBLISHKEY_VALUE];
            }
        }

        /// <summary>
        /// Locates a Post object and specifies the result as the page's DataContext
        /// </summary>
        private void LoadBlog()
        {
            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            if (null != App.MasterViewModel.CurrentPost)
            {
                string postId = App.MasterViewModel.CurrentPost.PostId.ToString();

                GetPostRPC rpc = new GetPostRPC(currentBlog, postId);
                rpc.Completed += OnGetPostRPCCompleted;
                rpc.ExecuteAsync();

                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPost);
            }
            else
            {
                DataContext = new Post();
            }
        }

        private void OnGetPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {            
            GetPostRPC rpc = sender as GetPostRPC;
            rpc.Completed -= OnGetPostRPCCompleted;

            if (null == args.Error)
            {
                Post post = args.Items[0];
                DataContext = post;
                StringBuilder tagBuilder = new StringBuilder();
                foreach (string tag in post.Categories)
                {
                    tagBuilder.Append(tag);
                }
                tagsTextBox.Text = tagBuilder.ToString();
            }
            else
            {
                HandleError(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            Post post = DataContext as Post;

            if (post.IsNew)
            {
                NewPostRPC rpc = new NewPostRPC(App.MasterViewModel.CurrentBlog, post);
                rpc.PostType = ePostType.post;
                rpc.Publish = publishToggleButton.IsChecked.Value;                
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
                DataStore.Instance.FetchCurrentBlogPostsAsync();
                NavigationService.GoBack();
            }
            else
            {
                HandleError(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnNewPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            NewPostRPC rpc = sender as NewPostRPC;
            rpc.Completed -= OnNewPostRPCCompleted;

            if (null == args.Error)
            {
                DataStore.Instance.FetchCurrentBlogPostsAsync();
                NavigationService.GoBack();
            }
            else
            {
                HandleError(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            //TODO: ask the user to confirm
            NavigationService.GoBack();
        }

        private void OnBoldToggleButtonClick(object sender, RoutedEventArgs e)
        {
            Post post = DataContext as Post;
            string description = post.Description;

            int startIndex = contentTextBox.SelectionStart;
            if (description.Length <= startIndex)
            {
                startIndex = description.Length;
            }

            if (boldToggleButton.IsChecked.Value)
            {
                post.Description = description.Insert(startIndex, WordPressMarkupTags.BOLD_OPENING_TAG);
            }
            else
            {
                post.Description = description.Insert(startIndex, WordPressMarkupTags.BOLD_CLOSING_TAG);
            }
        }

        private void HandleError(Exception exception)
        {
            //TODO: clean this up...
            MessageBox.Show(exception.Message);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //store transient data in the State dictionary
            SavePageState();
        }

        /// <summary>
        /// Stores transient data in the page's State dictionary
        /// </summary>
        private void SavePageState()
        {
            if (State.ContainsKey(POSTKEY_VALUE))
            {
                State.Remove(POSTKEY_VALUE);
            }

            Post post = DataContext as Post;

            //make sure that the post contains the latest title and content
            post.Title = titleTextBox.Text;
            post.Description = contentTextBox.Text;

            State.Add(POSTKEY_VALUE, post);

            if (State.ContainsKey(PUBLISHKEY_VALUE))
            {
                State.Remove(PUBLISHKEY_VALUE);
            }
            State.Add(PUBLISHKEY_VALUE, publishToggleButton.IsChecked);
        }

        #endregion

    }
}