using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

using WordPress.Converters;
using WordPress.Localization;
using WordPress.Model;
using WordPress.Settings;
using System.Text;

namespace WordPress
{
    public partial class EditPostPage : PhoneApplicationPage
    {
        #region member variables

        private static object _syncRoot = new object();

        private const string DATACONTEXT_VALUE = "dataContext";
        private const string PUBLISHKEY_VALUE = "publish";
        private const string TITLEKEY_VALUE = "title";
        private const string CONTENTKEY_VALUE = "content";
        private const string TAGSKEY_VALUE = "tags";

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _saveIconButton;
        private List<UploadFileRPC> _mediaUploadRPCs;
        private List<UploadedFileInfo> _uploadInfo;

        #endregion

        #region constructors

        public EditPostPage()
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

            _mediaUploadRPCs = new List<UploadFileRPC>();
            _uploadInfo = new List<UploadedFileInfo>();

            Loaded += OnPageLoaded;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            App.WaitIndicationService.RootVisualElement = LayoutRoot;

            if (!(State.ContainsKey(TITLEKEY_VALUE)))
            {
                LoadBlog();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //check for transient data stored in State dictionary
            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                RestorePageState();
            }
        }

        /// <summary>
        /// Retrieves transient data from the page's State dictionary
        /// </summary>
        private void RestorePageState()
        {
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

            if (State.ContainsKey(TAGSKEY_VALUE))
            {
                tagsTextBox.Text = State[TAGSKEY_VALUE] as string;
            }

            CategoryContentConverter converter = Resources["CategoryContentConverter"] as CategoryContentConverter;
            if (null == converter) return;

            categoriesTextBlock.Text = converter.Convert(App.MasterViewModel.CurrentPost.Categories, typeof(string), null, null) as string;
        }

        /// <summary>
        /// Locates a Post object and specifies the result as the page's DataContext
        /// </summary>
        private void LoadBlog()
        {
            Blog currentBlog = App.MasterViewModel.CurrentBlog;
            
            if (null != App.MasterViewModel.CurrentPostListItem)
            {
                string postId = App.MasterViewModel.CurrentPostListItem.PostId.ToString();

                GetPostRPC rpc = new GetPostRPC(currentBlog, postId);
                rpc.Completed += OnGetPostRPCCompleted;
                rpc.ExecuteAsync();

                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.RetrievingPost);
            }
            else
            {
                Post post = new Post();
                DataContext = post;
                UserSettings settings = new UserSettings();
                if (settings.UseTaglineForNewPosts)
                {
                    contentTextBox.Text = settings.Tagline;
                }
                App.MasterViewModel.CurrentPost = post;
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
                App.MasterViewModel.CurrentPost = post;
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            if (0 < _mediaUploadRPCs.Count)
            {
                UploadImagesAndSavePost();
                return;
            }

            SavePost();
        }

        private void UploadImagesAndSavePost()
        {
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingMedia);
            
            //make sure nothing is in our results collection
            _uploadInfo.Clear();

            //fire off the worker rpcs
            _mediaUploadRPCs.ForEach(rpc => rpc.ExecuteAsync());
        }

        private void SavePost()
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
                DataService.Current.FetchCurrentBlogPostsAsync();
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
                DataService.Current.FetchCurrentBlogPostsAsync();
                NavigationService.GoBack();
            }
            else
            {
                this.HandleException(args.Error);
            }

            App.WaitIndicationService.HideIndicator();
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

            if (State.ContainsKey(TAGSKEY_VALUE))
            {
                State.Remove(TAGSKEY_VALUE);
            }
            State.Add(TAGSKEY_VALUE, tagsTextBox.Text);
        }

        private void OnSelectCategoriesButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SelectCategoriesPage.xaml", UriKind.Relative));
        }

        private void OnAddNewMediaButtonClick(object sender, RoutedEventArgs e)
        {
            AddNewMedia();
        }

        private void AddNewMedia()
        {
            PhotoChooserTask task = new PhotoChooserTask();            
            task.Completed += new EventHandler<PhotoResult>(OnChoosePhotoTaskCompleted);
            task.ShowCamera = true;
            task.Show();
        }

        private void OnChoosePhotoTaskCompleted(object sender, PhotoResult e)
        {
            PhotoChooserTask task = sender as PhotoChooserTask;
            task.Completed -= OnChoosePhotoTaskCompleted;

            if (TaskResult.OK != e.TaskResult) return;

            Stream chosenPhoto = e.ChosenPhoto;

            //build out ui updates
            BitmapImage image = BuildBitmap(chosenPhoto);            
            Image imageElement = BuildImageElement(image);
            imageWrapPanel.Children.Add(imageElement);

            //build out upload rpcs
            int length = (int)chosenPhoto.Length;
            chosenPhoto.Position = 0;
            byte[] payload = new byte[length];
            chosenPhoto.Read(payload, 0, length);

            UploadFileRPC rpc = new UploadFileRPC(App.MasterViewModel.CurrentBlog, e.OriginalFileName, payload, true);
            rpc.Completed += OnUploadMediaRPCCompleted;

            //store this for later--we'll upload the files once the user hits save
            _mediaUploadRPCs.Add(rpc);
        }

        private BitmapImage BuildBitmap(Stream bitmapStream)
        {
            BitmapImage image = new BitmapImage();
            image.SetSource(bitmapStream);
            return image;
        }

        private Image BuildImageElement(BitmapImage image)
        {
            Image imageElement = new Image();
            imageElement.Source = image;

            //TODO: integrate with settings
            imageElement.Height = 100;
            imageElement.Width = 100;
            imageElement.Margin = new Thickness(10);
            return imageElement;
        }

        private void OnClearMediaButtonClick(object sender, RoutedEventArgs e)
        {
            ClearMedia();
        }

        private void ClearMedia()
        {
            imageWrapPanel.Children.Clear();
            _mediaUploadRPCs.ForEach(rpc => rpc.Completed -= OnUploadMediaRPCCompleted);
            _mediaUploadRPCs.Clear();
        }

        private void OnUploadMediaRPCCompleted(object sender, XMLRPCCompletedEventArgs<UploadedFileInfo> args)
        {
            UploadFileRPC rpc = sender as UploadFileRPC;
            rpc.Completed -= OnUploadMediaRPCCompleted;

            lock (_syncRoot)
            {
                _mediaUploadRPCs.Remove(rpc);
                if (null == args.Error)
                {
                    _uploadInfo.Add(args.Items[0]);
                }
            }

            //if we're not done, bail
            if (0 < _mediaUploadRPCs.Count) return;

            UpdatePostContent();
            App.WaitIndicationService.KillSpinner();
            SavePost();
        }

        private void UpdatePostContent()
        {
            StringBuilder builder = new StringBuilder();

            _uploadInfo.ForEach(info =>
            {
                builder.Append(BuildImgMarkup(info));
            });

            contentTextBox.Text += builder.ToString();
            contentTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private string BuildImgMarkup(UploadedFileInfo info)
        {
            //TODO: integrate with settings
            string imgFormat = "<img src='{0}'/>";
            string img = string.Format(imgFormat, info.Url);
            return img;
        }

        #endregion
    }
}