using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordPress.Localization;
using WordPress.Model;
using WordPress.Utils;
using System.Linq;

namespace WordPress
{
    public partial class EditPagePage : PhoneApplicationPage
    {
        //DEV NOTE: as far as the WP data model goes, there isn't a real difference
        //between a post and a page, so we use the "post" rpcs
        #region member variables

        private static object _syncRoot = new object();
        private bool _messageBoxIsShown = false;

        private const string TITLEKEY_VALUE = "title";
        private const string PUBLISHKEY_VALUE = "publish";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        private List<UploadFileRPC> _mediaUploadRPCs;
        private AbstractPostRPC currentXmlRpcConnection;
        public Media _lastTappedMedia = null; //used to pass the obj to the Media details page

        private bool isEditingLocalDraft = false;

        private PhotoChooserTask photoChooserTask;

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

            _mediaUploadRPCs = new List<UploadFileRPC>();

            Loaded += OnPageLoaded;

            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.ShowCamera = true;
            photoChooserTask.Completed += new EventHandler<PhotoResult>(OnChoosePhotoTaskCompleted);

            uploadImagesAsGalleryCheckbox.Visibility = Visibility.Collapsed;
            gallerySettingsButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            Loaded -= OnPageLoaded;
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
            LoadPage();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.WaitIndicationService.Waiting)
            {
                App.WaitIndicationService.HideIndicator();
                ApplicationBar.IsVisible = true;
                e.Cancel = true;

                if (null != currentXmlRpcConnection)
                {
                    currentXmlRpcConnection.IsCancelled = true;
                    currentXmlRpcConnection = null;
                }

                this.emptyImagesUploadingQueue();
            }
            else
            {
                if (_messageBoxIsShown)
                    return;
                _messageBoxIsShown = true;
                string prompt = string.Format(_localizedStrings.Prompts.SureCancel, _localizedStrings.Prompts.Page);
                MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.CancelEditing, MessageBoxButton.OKCancel);
                _messageBoxIsShown = false;
                if (result == MessageBoxResult.OK)
                {
                    this.emptyImagesUploadingQueue();
                    cleanupPostMedia();
                    base.OnBackKeyPress(e);
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void cleanupPostMedia()
        {
            Post post = App.MasterViewModel.CurrentPost;

            if (post == null || post.PostStatus == null || post.Media == null)
                return;

            if (post.IsLocalDraft())
            {
                // Don't clear images from local drafts.
                return;
            }
            foreach (Media m in post.Media)
            {
                if (m != null)
                    m.clearSavedImage();
            }
        }

        private void LoadPage()
        {
            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            BlogName.Text = currentBlog.BlogNameUpper;

            if (null != App.MasterViewModel.CurrentPageListItem)
            {
                if (App.MasterViewModel.CurrentPageListItem.DraftIndex > -1)
                {
                    // Page is a local draft
                    this.isEditingLocalDraft = true;
                    DataContext = App.MasterViewModel.CurrentBlog.LocalPageDrafts[App.MasterViewModel.CurrentPageListItem.DraftIndex];
                    App.MasterViewModel.CurrentPost = App.MasterViewModel.CurrentBlog.LocalPageDrafts[App.MasterViewModel.CurrentPageListItem.DraftIndex];
                    setStatus();

                    //update the Media UI
                    List<Media> unavaiblePictures = new List<Media>();
                    foreach (Media currentMedia in App.MasterViewModel.CurrentPost.Media)
                    {
                        using (Stream stream = currentMedia.getImageStream())
                        {
                            if (stream == null)
                            {
                                unavaiblePictures.Add(currentMedia);
                                continue;
                            }
                         
                            try
                            {
                                BitmapImage image = new BitmapImage();
                                image.SetSource(stream);
                                imageWrapPanel.Children.Add(BuildTappableImageElement(image, currentMedia));
                            }
                            catch (Exception)
                            {
                            }

                        }
                    }
                    if (unavaiblePictures.Count > 0)
                    {
                        MessageBoxResult result = MessageBox.Show("Can't read a picture attached to this draft, please try to load the draft later.", "Error", MessageBoxButton.OK);
                        foreach (Media m in unavaiblePictures)
                        {
                            App.MasterViewModel.CurrentPost.Media.Remove(m);
                        }
                    }
                }
                else
                {  
                    Post post = App.MasterViewModel.CurrentPost;
                    setStatus();
                    DataContext = post;
                }
            }
            else
            {   //New Page
                Post page = new Post();
                page.PostStatus = "publish";
                App.MasterViewModel.CurrentPost = page;
                page.DateCreated = DateTime.Now;
                page.DateCreatedGMT = DateTime.Now.ToUniversalTime();
                DataContext = page;
                setStatus();
            }

            ToggleGalleryControlsVisibility();
        }

        private void setStatus()
        {
            List<string> statusList = new List<string>() { 
                _localizedStrings.ControlsText.Publish, 
                _localizedStrings.ControlsText.Draft, 
                _localizedStrings.ControlsText.PendingReview, 
                _localizedStrings.ControlsText.Private
            };

            if (App.MasterViewModel.CurrentPost.IsNew || this.isEditingLocalDraft)
                statusList.Add(_localizedStrings.ControlsText.LocalDraft);

            this.statusPicker.ItemsSource = statusList;
            Post page = App.MasterViewModel.CurrentPost;
            if (page.PostStatus.Equals("publish"))
                statusPicker.SelectedIndex = 0;
            else if (page.PostStatus.Equals("draft"))
                statusPicker.SelectedIndex = 1;
            else if (page.PostStatus.Equals("pending"))
                statusPicker.SelectedIndex = 2;
            else if (page.PostStatus.Equals("private"))
                statusPicker.SelectedIndex = 3;
            else if (page.PostStatus.Equals("localdraft"))
                statusPicker.SelectedIndex = 4;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //look for transient data stored in the State dictionary
            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                titleTextBox.Text = State[TITLEKEY_VALUE] as string;
            }

            ToggleGalleryControlsVisibility();
        }

        private void OnContentTextBoxTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditContentTextMode.xaml", UriKind.Relative));
        }

        //Title text field KeyUp event handler: Dismiss the keyboard by focusing the main control if the Key pressed is the Enter key
        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void OnPageContentLostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void OnPageContentGotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false; //hide the application bar
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            _messageBoxIsShown = false;
            Post post = DataContext as Post;

            //Do not publish pages with no title or content.
            if (!post.HasMedia())
            {
                //check the content
                if (titleTextBox.Text.Trim() == "" && contentTextBox.Text.Trim() == "")
                {
                    MessageBox.Show(
                        string.Format(_localizedStrings.Messages.TitleAndContentEmpty, _localizedStrings.Prompts.Page),
                        _localizedStrings.PageTitles.Error,
                        MessageBoxButton.OK);
                    return;
                }
            }

            switch (this.statusPicker.SelectedIndex)
            {
                case 0:
                    post.PostStatus = "publish";
                    break;
                case 1:
                    post.PostStatus = "draft";
                    break;
                case 2:
                    post.PostStatus = "pending";
                    break;
                case 3:
                    post.PostStatus = "private";
                    break;
                case 4:
                    post.PostStatus = "localdraft";
                    break;
            }

            //make sure the post has the latest UI data--the Save button is a ToolbarButton
            //which doesn't force focus to change
            post.Title = titleTextBox.Text;

            if (post.HasMedia())
            {
                if (!post.IsLocalDraft())
                {
                    foreach (Media currentMedia in post.Media)
                    {
                        // If there is an error posting the user may try again.
                        // Media that has already been uploaded will have a good URL. 
                        // Check for this and don't upload media a second time. 
                        if (currentMedia.Url != null && currentMedia.Url.Length > 0)
                            continue;

                        UploadFileRPC rpc = new UploadFileRPC(App.MasterViewModel.CurrentBlog, currentMedia, true);
                        rpc.Completed += OnUploadMediaRPCCompleted;
                        //store this for later--we'll upload the files once the user hits save
                        _mediaUploadRPCs.Add(rpc);
                    }

                    if (_mediaUploadRPCs.Count > 0)
                    {
                        UploadImagesAndSavePost();
                        return;
                    }
                }
            }

            SavePost();
        }

        private void emptyImagesUploadingQueue()
        {
            if (_mediaUploadRPCs == null || (_mediaUploadRPCs.Count == 0)) return;
            _mediaUploadRPCs.ForEach(rpc =>
            {
                rpc.Completed -= OnUploadMediaRPCCompleted;
                rpc.IsCancelled = true;
            });
            _mediaUploadRPCs.Clear();
        }

        private void UploadImagesAndSavePost()
        {
            this.Focus(); //hide the keyboard
            ApplicationBar.IsVisible = false; //hide the application bar
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingMedia);

            //fire off the worker rpcs
            if (_mediaUploadRPCs.Count > 0)
            {
                UploadFileRPC item = _mediaUploadRPCs.First() as UploadFileRPC;
                item.ExecuteAsync();
            }
        }

        private void OnEditPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            EditPostRPC rpc = sender as EditPostRPC;
            rpc.Completed -= OnEditPostRPCCompleted;
         
            if (args.Cancelled)
            {
                return;
            }

            //Do not change the UI if the connection is cancelled. The user could have started another connection...
            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.HideIndicator();
            
            if (null == args.Error)
            {
                cleanupPostMedia();
                if (NavigationService.CanGoBack)
                {
                    DataService.Current.FetchCurrentBlogPagesAsync(false);
                    NavigationService.GoBack();
                }
                else
                    throw new ApplicationShouldEndException();
            }
            else
            {
                this.HandleException(args.Error);
            }
        }

        private void OnNewPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            NewPostRPC rpc = sender as NewPostRPC;
            rpc.Completed -= OnNewPostRPCCompleted;

            if (args.Cancelled)
            {
                return;
            }

            if (this.isEditingLocalDraft)
            {
                // Local Draft was published
                App.MasterViewModel.CurrentBlog.LocalPageDrafts.Remove(App.MasterViewModel.CurrentPost);
            }

            App.WaitIndicationService.HideIndicator();
            ApplicationBar.IsVisible = true;

            if (null == args.Error)
            {
                cleanupPostMedia();
                if (NavigationService.CanGoBack)
                {
                    DataService.Current.FetchCurrentBlogPagesAsync(false);
                    NavigationService.GoBack();
                }
                else
                    throw new ApplicationShouldEndException();
            }
            else
            {
                this.HandleException(args.Error);
            }
        }
        
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            if (e.Content is ImageDetailsPage)
            {
                (e.Content as ImageDetailsPage).TappedImage = _lastTappedMedia;
                _lastTappedMedia = null;
            }

            base.OnNavigatedFrom(e);

            //store transient data in the State dictionary
            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                State.Remove(TITLEKEY_VALUE);
            }
            State.Add(TITLEKEY_VALUE, titleTextBox.Text);

            Post post = App.MasterViewModel.CurrentPost; 
            if (post != null && post.Gallery.Enabled)
            {
                uploadImagesAsGalleryCheckbox.IsChecked = true;
           }
        }

        private void OnDatePickerChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            Post page = DataContext as Post;

            if (page != null && e.NewDateTime != null)
            {
                if (sender == postDatePicker)
                {
                    postTimePicker.Value = e.NewDateTime;
                }
                else if (sender == postTimePicker)
                {
                    postDatePicker.Value = e.NewDateTime;
                }
                page.DateCreated = (DateTime)e.NewDateTime;
                page.DateCreatedGMT = ((DateTime)e.NewDateTime).ToUniversalTime();
            }
        }

        #endregion


        #region media_methods
        /* media methods copied from the Post Class. We need to find a way to abstract them in a common class.*/
        private void OnAddNewMediaButtonClick(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();
        }
    
        private void OnEditGallerySettingsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GallerySettingsPage.xaml", UriKind.Relative));
        }

        private void OnChoosePhotoTaskCompleted(object sender, PhotoResult e)
        {
            if (TaskResult.OK != e.TaskResult) return;
            using(Stream pictureStream = e.ChosenPhoto)
                AddNewMediaStream(pictureStream, e.OriginalFileName);
        }

        private void AddNewMediaStream(Stream bitmapStream, string originalFilePath)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                image.SetSource(bitmapStream);
            }
            catch (Exception)
            {
                MessageBoxResult result = MessageBox.Show(_localizedStrings.Prompts.MediaErrorPictureCorruptedOrUnreadable, _localizedStrings.PageTitles.Error, MessageBoxButton.OK);
                return;
            }

            // Save to isolated storage. 
            // The OriginalFilename is a GUID.  Since we're saving to IsolatedStorage, use this as a filename to avoid collisions.
            string localfilename = Path.Combine(Media.MEDIA_IMAGE_DIRECTORY, Path.GetFileName(originalFilePath));
            DateTime capture = DateTime.Now;
            string fileNameFormat = "SavedPicture-{0}{1}{2}{3}{4}{5}{6}"; //year, month, day, hours, min, sec, file extension
            string filename = string.Format(fileNameFormat,
                capture.Year,
                capture.Month,
                capture.Day,
                capture.Hour,
                capture.Minute,
                capture.Second,
                Path.GetExtension(originalFilePath));

            Media currentMedia = null;
            try
            {
                currentMedia = new Media(App.MasterViewModel.CurrentBlog, filename, localfilename, bitmapStream, App.MasterViewModel.CurrentBlog.PreserveBandwidth);
            }
            catch (Exception)
            {
                MessageBoxResult result = MessageBox.Show(_localizedStrings.Prompts.MediaErrorCannotWritePicture, _localizedStrings.PageTitles.Error, MessageBoxButton.OK);
                return;
            }
            currentMedia.CanBeFeatured = false;
            Post post = App.MasterViewModel.CurrentPost;
            post.Media.Add(currentMedia);

            //update the UI
            imageWrapPanel.Children.Add(BuildTappableImageElement(image, currentMedia));
        }

        private void ToggleGalleryControlsVisibility()
        {

            if (App.MasterViewModel.CurrentPost == null)
                return;
            
            //Gallery only on wpcom.
            if (App.MasterViewModel.CurrentBlog.isWPcom() == false)
                return; 

            Post post = App.MasterViewModel.CurrentPost;
            if (post.isGalleryAvailable())
            {
                uploadImagesAsGalleryCheckbox.Visibility = Visibility.Visible;
                gallerySettingsButton.Visibility = Visibility.Visible;
            }
            else
            {
                uploadImagesAsGalleryCheckbox.Visibility = Visibility.Collapsed;
                gallerySettingsButton.Visibility = Visibility.Collapsed;
            }
        }

        private Button BuildTappableImageElement(BitmapImage image, Media currentMedia)
        {
            Button imageOuterButton = new Button();
            imageOuterButton.Tag = currentMedia;
            imageOuterButton.Tap += sp_Tap;
            float width = 180F;
            int height = (int)(width / image.PixelWidth * image.PixelHeight);
            imageOuterButton.Width = width;
            imageOuterButton.Height = height;
            Style btnStyle = App.Current.Resources["BasicButtonStyle"] as Style;
            imageOuterButton.Style = btnStyle;
            imageOuterButton.Background = new ImageBrush { ImageSource = image };
            return imageOuterButton;
        }

        private void sp_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Control tappedObj = (Control)sender;
            Media currentMedia = (Media)tappedObj.Tag;
            _lastTappedMedia = currentMedia;
            NavigationService.Navigate(new Uri("/ImageDetailsPage.xaml", UriKind.Relative));
        }

        private void OnClearMediaButtonClick(object sender, RoutedEventArgs e)
        {
            ClearMedia();
        }

        private void ClearMedia()
        {
            imageWrapPanel.Children.Clear();
            this.emptyImagesUploadingQueue();
            Post post = DataContext as Post;
            post.Media.Clear();

            ToggleGalleryControlsVisibility();
            // This forces the media section to layout properly after clicking the
            // clear media button.
            uploadImagesAsGalleryCheckbox.InvalidateMeasure();
            imageWrapPanel.InvalidateMeasure();
        }


        public void removeImage(Media imageToRemove)
        {
            Post post = DataContext as Post;
            post.Media.Remove(imageToRemove);
            foreach (var el in imageWrapPanel.Children)
            {
                if ((el as FrameworkElement).Tag == imageToRemove)
                {
                    imageWrapPanel.Children.Remove(el);
                    break;
                }
            }
            ToggleGalleryControlsVisibility();
        }

        private void OnUploadMediaRPCCompleted(object sender, XMLRPCCompletedEventArgs<Media> args)
        {
            UploadFileRPC rpc = sender as UploadFileRPC;
            rpc.Completed -= OnUploadMediaRPCCompleted;

            lock (_syncRoot)
            {
                _mediaUploadRPCs.Remove(rpc);
                if (args.Cancelled)
                {
                    return;
                }

                if (args.Items.Count == 0 || args.Error != null)
                {
                    //uh oh, media upload problem
                    App.WaitIndicationService.KillSpinner();
                    //Move 
                    UIThread.Invoke(() =>
                    {
                        ApplicationBar.IsVisible = true;
                        if (!_messageBoxIsShown)
                        {
                            _messageBoxIsShown = true;
                            String msg = args.Error != null ? args.Error.Message : _localizedStrings.Prompts.MediaError;
                            MessageBoxResult result = MessageBox.Show(msg, _localizedStrings.Prompts.MediaError, MessageBoxButton.OK);
                            _messageBoxIsShown = false;
                        }
                    });
                    this.emptyImagesUploadingQueue();
                    return;
                }
                else
                {
                    //Image uploaded correctly. Upload the next picture in the list
                    if (_mediaUploadRPCs.Count > 0)
                    {
                        UploadFileRPC item = _mediaUploadRPCs.First() as UploadFileRPC;
                        item.ExecuteAsync();
                        return;
                    }

                    App.WaitIndicationService.KillSpinner();
                    SavePost();
                }
            }//end lock
        }

        private void SavePost()
        {
            Post post = App.MasterViewModel.CurrentPost;
            Blog blog = App.MasterViewModel.CurrentBlog;

            if (post.HasMedia() && !post.IsLocalDraft())
            {
                StringBuilder prependBuilder = new StringBuilder();
                StringBuilder appendBuilder = new StringBuilder();
                 List<string> mediaIds = new List<string>();
                bool galleryEnabled = uploadImagesAsGalleryCheckbox.Visibility == Visibility.Visible &&
                                      uploadImagesAsGalleryCheckbox.IsChecked.GetValueOrDefault();
                post.GenerateImageMarkup(blog.PlaceImageAboveText, galleryEnabled);
            }

            //make sure the post has the latest UI data--the Save button is a ToolbarButton
            //which doesn't force focus to change
            post.Title = titleTextBox.Text;
            if (post.IsNew)
            {
                if (!post.IsLocalDraft())
                {
                    NewPostRPC rpc = new NewPostRPC(App.MasterViewModel.CurrentBlog, post);
                    rpc.PostType = ePostType.page;
                    rpc.Completed += OnNewPostRPCCompleted;
                    rpc.ExecuteAsync();
                    currentXmlRpcConnection = rpc;
                }
                else
                {
                    // Create or update a local draft
                    if (App.MasterViewModel.CurrentPageListItem != null)
                    {
                        if (App.MasterViewModel.CurrentPageListItem.DraftIndex > -1)
                            App.MasterViewModel.CurrentBlog.LocalPageDrafts[App.MasterViewModel.CurrentPageListItem.DraftIndex] = post;
                    }
                    else
                    {
                        blog.LocalPageDrafts.Add(post);
                    }
                    // Exit post editor
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    else
                        throw new ApplicationShouldEndException();
                }
            }
            else
            {
                EditPostRPC rpc = new EditPostRPC(App.MasterViewModel.CurrentBlog, post);
                if (post.PostStatus == "publish")
                    rpc.Publish = true;
                else
                    rpc.Publish = false;
                rpc.PostType = ePostType.page;
                rpc.Completed += OnEditPostRPCCompleted;

                currentXmlRpcConnection = rpc;
                rpc.ExecuteAsync();
            }
            this.Focus();
            ApplicationBar.IsVisible = false;
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingChanges);
        }

        #endregion media_methods

        private void OnVisualEditorButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditContent.xaml", UriKind.Relative));
        }
         
        private void OnTextEditorButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditContentTextMode.xaml", UriKind.Relative));
        }
    }
}