﻿using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordPress.Converters;
using WordPress.Localization;
using WordPress.Model;
using WordPress.Settings;
using WordPress.Utils;
using System.Linq;

namespace WordPress
{
    public partial class EditPostPage : PhoneApplicationPage
    {
        #region member variables

        private static object _syncRoot = new object();

        private const string PUBLISHKEY_VALUE = "publish";
        private const string TITLEKEY_VALUE = "title";
        private const string TAGSKEY_VALUE = "tags";

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _saveIconButton;
        private List<UploadFileRPC> _mediaUploadRPCs;
        private AbstractPostRPC currentXmlRpcConnection;
        private GetMediaItemRPC _mediaItemRPC;
        public Media _lastTappedMedia = null; //used to pass the obj to the Media details page

        private bool _messageBoxIsShown = false;
        private bool isEditingLocalDraft = false;

        private PhotoChooserTask photoChooserTask;

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
  
            Loaded += OnPageLoaded;

            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.ShowCamera = true;
            photoChooserTask.Completed += new EventHandler<PhotoResult>(OnChoosePhotoTaskCompleted);

            uploadImagesAsGalleryCheckbox.Visibility = Visibility.Collapsed;
            gallerySettingsButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Featured Image methods

        //called from ImageDetailsPage
        public void UpdateFeaturedImage(Media featuredImage)
        {
            if (featuredImage.IsFeatured)
            {
                foreach (Media m in App.MasterViewModel.CurrentPost.Media)
                {
                    m.IsFeatured = m.Equals(featuredImage);
                }
            }

            //update the UI
            foreach (Canvas c in imageWrapPanel.Children)
            {
                Image img = (Image)c.Children[1];
                Media current_media = (Media)c.Tag;
                if (current_media.IsFeatured)
                {
                    img.Visibility = Visibility.Visible;
                }
                else
                {
                    img.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SetupFeaturedImage()
        {
            Post post = App.MasterViewModel.CurrentPost;
            if (!App.MasterViewModel.CurrentBlog.SupportsFeaturedImage() || post.PostThumbnail.Length == 0)
            {
                featuredImagePadding.Visibility = Visibility.Collapsed;
                featuredImageHeaderBackground.Visibility = Visibility.Collapsed;
                featuredImageHeader.Visibility = Visibility.Collapsed;
                featuredImagePanel.Visibility = Visibility.Collapsed;
                return;
            }

            if (post.FeaturedImage == null)
            {
                // Get the URL for the featured image.
                _mediaItemRPC = new GetMediaItemRPC(App.MasterViewModel.CurrentBlog, post.PostThumbnail);
                _mediaItemRPC.Completed += OnGetMediaItemRPCCompleted;
                _mediaItemRPC.ExecuteAsync();
                return;
            }

            // Load the featured image.
            loadFeaturedImageFromURL(post.FeaturedImage.Thumbnail);
        }
        
        private void OnGetMediaItemRPCCompleted(object sender, XMLRPCCompletedEventArgs<MediaItem> args)
        {
            _mediaItemRPC.Completed -= OnGetMediaItemRPCCompleted;
            _mediaItemRPC = null;

            if (args.Cancelled)
            {
                return;
            }

            if (null == args.Error)
            {
                if (args.Items.Count > 0)
                {
                    MediaItem m = args.Items[0] as MediaItem;
                    App.MasterViewModel.CurrentPost.FeaturedImage = m;
                    SetupFeaturedImage();
                }
            }
            else
            {
                //Error!
                showFeaturedImageLoadingError(args.Error);
            }
        }

        //WP.COM private blog. 
        private void loadFeaturedImageFromURL(string url)
        {
            HttpWebRequest webRequest = null;
            if (App.MasterViewModel.CurrentBlog.isPrivate())
            {
                string loginURL = App.MasterViewModel.CurrentBlog.loginURL();
                webRequest = (HttpWebRequest)WebRequest.Create(loginURL);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Accept = "*/*";
                webRequest.UserAgent = Constants.WORDPRESS_USERAGENT;
                webRequest.AllowAutoRedirect = true;

                try
                {
                    webRequest.BeginGetRequestStream(ar =>
                        {
                            try
                            {
                                var requestStream = webRequest.EndGetRequestStream(ar);
                                // Add the post data to the web request
                                string postData = "log=" + HttpUtility.UrlEncode(App.MasterViewModel.CurrentBlog.Username)
                                    + "&pwd=" + HttpUtility.UrlEncode(App.MasterViewModel.CurrentBlog.Password)
                                    + "&redirect_to=" + HttpUtility.UrlEncode(url);
                                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                                requestStream.Write(byteArray, 0, byteArray.Length);
                                requestStream.Close();
                                requestStream.Dispose();
                                webRequest.BeginGetResponse(new AsyncCallback(loadFeaturedImageFromURLRequestCallback), webRequest);
                            }
                            catch (Exception ex)
                            {
                                showFeaturedImageLoadingError(ex);
                            }
                        }, null);
                }
                catch (Exception ex)
                {
                    showFeaturedImageLoadingError(ex);
                }
            }
            else
            { //Not a .com private blog
                try
                {
                    System.Uri targetUri = new System.Uri(url);
                    webRequest = (HttpWebRequest)HttpWebRequest.Create(targetUri);
                    webRequest.BeginGetResponse(new AsyncCallback(loadFeaturedImageFromURLRequestCallback), webRequest);
                }
                catch (Exception ex)
                {
                    showFeaturedImageLoadingError(ex);
                }
            }
        }

        private void loadFeaturedImageFromURLRequestCallback(IAsyncResult callbackResult)
        {
            HttpWebRequest myRequest = (HttpWebRequest)callbackResult.AsyncState;
            HttpWebResponse myResponse = null;
            try
            {
                myResponse = (HttpWebResponse)myRequest.EndGetResponse(callbackResult);
            }
            catch (WebException ex)
            {
                showFeaturedImageLoadingError(ex);
                return;
            }

            UIThread.Invoke(() =>
            {
                using (Stream pictureStream = myResponse.GetResponseStream())
                {
                    if (null == pictureStream)
                        return;

                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(pictureStream);
                        featuredImage.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        showFeaturedImageLoadingError(ex);
                        return;
                    }
                    
                }
            });
        }

        private void showFeaturedImageLoadingError(Exception ex)
        {
            UIThread.Invoke(() =>
            {
                featuredImage.Visibility = Visibility.Collapsed;
                featuredImageError.Visibility = Visibility.Visible;
                if (ex != null && ex.Message != null)
                    featuredImageError.Text = "Sorry, something went wrong while loading the image.\n"+ex.Message;
                else
                    featuredImageError.Text = "Sorry, something went wrong while loading the image.";
            });
        }

        private void OnRemoveFeaturedImageButtonClicked(object sender, EventArgs args)
        {
            Post post = App.MasterViewModel.CurrentPost;
            post.FeaturedImage = null;
            post.PostThumbnail = "";

            SetupFeaturedImage();
        }

        #endregion

        #region methods

        private void OnPageLoaded(object sender, EventArgs args)
        {
            Loaded -= OnPageLoaded;
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
            LoadBlog();
            SetupFeaturedImage();
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

                if (null != _mediaItemRPC)
                {
                    _mediaItemRPC.IsCancelled = true;
                    _mediaItemRPC = null;
                }

                this.emptyImagesUploadingQueue();
            }
            else
            {
                if (_messageBoxIsShown)
                    return;
                _messageBoxIsShown = true;
                string prompt = string.Format(_localizedStrings.Prompts.SureCancel, _localizedStrings.Prompts.Post);
                MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.CancelEditing, MessageBoxButton.OKCancel);
                _messageBoxIsShown = false;
                if (result == MessageBoxResult.OK)
                {
                    if (_mediaItemRPC != null)
                    {
                        _mediaItemRPC.Completed -= OnGetMediaItemRPCCompleted;
                        _mediaItemRPC.IsCancelled = true;
                    }

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
                if( m != null )
                    m.clearSavedImage();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //check for transient data stored in State dictionary
            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                if (State.ContainsKey(TITLEKEY_VALUE))
                {
                    titleTextBox.Text = State[TITLEKEY_VALUE] as string;
                }

                if (State.ContainsKey(TAGSKEY_VALUE))
                {
                    tagsTextBox.Text = State[TAGSKEY_VALUE] as string;
                }

                CategoryContentConverter converter = Resources["CategoryContentConverter"] as CategoryContentConverter;
                if (null == converter) return;

                Post post = App.MasterViewModel.CurrentPost;
                if (post != null)
                {
                    categoriesTextBlock.Text = converter.Convert(App.MasterViewModel.CurrentPost.Categories, typeof(string), null, null) as string;
                    if (post.Gallery.Enabled)
                        uploadImagesAsGalleryCheckbox.IsChecked = true;
                }
            }

            this.ToggleGalleryControlsVisibility();

            App.WaitIndicationService.RootVisualElement = LayoutRoot;
        }

        /// <summary>
        /// Locates a Post object and specifies the result as the page's DataContext
        /// </summary>
        private void LoadBlog()
        {
            Blog currentBlog = App.MasterViewModel.CurrentBlog;

            BlogName.Text = string.IsNullOrEmpty(currentBlog.BlogNameUpper) ? "" : currentBlog.BlogNameUpper;

            bool isSharingPhoto = (App.MasterViewModel.SharingPhotoToken != null);
            
            if (null != App.MasterViewModel.CurrentPostListItem && !isSharingPhoto)
            {
                string postId = App.MasterViewModel.CurrentPostListItem.PostId;

                if (App.MasterViewModel.CurrentPostListItem.DraftIndex > -1)
                {
                    // Post is a local draft
                    this.isEditingLocalDraft = true;
                    DataContext = App.MasterViewModel.CurrentBlog.LocalPostDrafts[App.MasterViewModel.CurrentPostListItem.DraftIndex];
                    App.MasterViewModel.CurrentPost = App.MasterViewModel.CurrentBlog.LocalPostDrafts[App.MasterViewModel.CurrentPostListItem.DraftIndex];
                    setStatus();
                    initPostFormatUI(App.MasterViewModel.CurrentPost.PostFormat);

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
                    if (unavaiblePictures.Count > 0 )
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
                    DataContext = post;
                    setStatus();
                    initPostFormatUI(post.PostFormat);
                    if (!string.IsNullOrWhiteSpace(post.MtKeyWords))
                    {
                        tagsTextBox.Text = post.MtKeyWords;
                    }
                }
            }
            else
            {   //New post
                Post post = new Post();
                App.MasterViewModel.CurrentPost = post;
                post.DateCreated = DateTime.Now;
                post.DateCreatedGMT = DateTime.Now.ToUniversalTime();
                DataContext = post;
                initPostFormatUI("standard");
                post.PostStatus = "publish";
                setStatus();
                /*postTimePicker.Value = post.DateCreated;
                postDatePicker.Value = post.DateCreated;*/
                if (isSharingPhoto)
                {
                    MediaLibrary library = new MediaLibrary();
                    Picture picture = library.GetPictureFromToken(App.MasterViewModel.SharingPhotoToken);
                    using (Stream pictureStream = picture.GetImage())
                        AddNewMediaStream(pictureStream, picture.Name);;

                    // clear the photo token so we don't try to add it to another post
                    App.MasterViewModel.SharingPhotoToken = null;

                    // blog selection page will be in the backstack, but if the user hits Back they should leave the app
                    // and return to the photo that they were sharing (e.g., so they can share it on another service)
                    NavigationService.RemoveBackEntry();
                }
            }

            this.ToggleGalleryControlsVisibility();
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
            
            Post post = App.MasterViewModel.CurrentPost;
            if (post.PostStatus.Equals("publish"))
                statusPicker.SelectedIndex = 0;
            else if (post.PostStatus.Equals("draft"))
                statusPicker.SelectedIndex = 1;
            else if (post.PostStatus.Equals("pending"))
                statusPicker.SelectedIndex = 2;
            else if (post.PostStatus.Equals("private"))
                statusPicker.SelectedIndex = 3;
            else if (post.PostStatus.Equals("localdraft"))
                statusPicker.SelectedIndex = 4;
        }

        private void initPostFormatUI(string currentPostFormatKey)
        {
            this.postFormatsPicker.ItemsSource = App.MasterViewModel.CurrentBlog.PostFormats;
            int i = 0;
            if (string.IsNullOrEmpty(currentPostFormatKey))
                currentPostFormatKey = "standard";
            foreach (PostFormat pf in App.MasterViewModel.CurrentBlog.PostFormats)
            {
                if (currentPostFormatKey.Equals(pf.Key))
                {
                    try
                    {
                        postFormatsPicker.SelectedIndex = i;
                    }
                    catch (Exception pfException)
                    {
                        Tools.LogException("PostFormats has probably changed while running the For cycle to select the item??", pfException);
                    }
                    postFormatsPicker.SelectionChanged += new SelectionChangedEventHandler(listPicker_SelectionChanged);
                    return;
                }
                i++;
            }
            postFormatsPicker.SelectionChanged += new SelectionChangedEventHandler(listPicker_SelectionChanged);
        }

        private void OnContentTextBoxTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Focus();
            NavigationService.Navigate(new Uri("/EditContentTextMode.xaml", UriKind.Relative));
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
            Post post = App.MasterViewModel.CurrentPost;

            //Do not publish posts with no title or content.
            if (!post.HasMedia())
            {
                //check the content
                if (titleTextBox.Text.Trim() == "" && post.Description.Trim() == "")
                {
                    MessageBox.Show(
                        string.Format(_localizedStrings.Messages.TitleAndContentEmpty, _localizedStrings.Prompts.Post),
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
       
        //Title text field KeyUp event handler: Dismiss the keyboard by focusing the main control if the Key pressed is the Enter key
        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void SavePost()
        {
            //Post post = DataContext as Post;
            //changed to CurrentPost so categories would save
            Post post = App.MasterViewModel.CurrentPost;
            Blog blog = App.MasterViewModel.CurrentBlog;

            if (post.HasMedia() && !post.IsLocalDraft())
            {
                bool galleryEnabled = uploadImagesAsGalleryCheckbox.Visibility == Visibility.Visible &&
                      uploadImagesAsGalleryCheckbox.IsChecked.GetValueOrDefault();
                post.GenerateImageMarkup(blog.PlaceImageAboveText, galleryEnabled);
            }

            //make sure the post has the latest UI data--the Save button is a ToolbarButton
            //which doesn't force focus to change
            post.Title = titleTextBox.Text;
            post.MtKeyWords = tagsTextBox.Text;

            if (post.IsNew)
            {
                if (!post.IsLocalDraft())
                {
                    // Anything but local draft status gets uploaded
                    UserSettings settings = new UserSettings();
                    if (settings.UseTaglineForNewPosts)
                    {
                        post.AddTagline(settings.Tagline);
                    }
                    NewPostRPC rpc = new NewPostRPC(App.MasterViewModel.CurrentBlog, post);
                    rpc.PostType = ePostType.post;
                    rpc.Completed += OnNewPostRPCCompleted;
                    rpc.ExecuteAsync();

                    currentXmlRpcConnection = rpc;
                    this.Focus();
                    ApplicationBar.IsVisible = false;
                    App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingChanges);

                } else {
                    // Create or update a local draft
                    if (App.MasterViewModel.CurrentPostListItem != null)
                    {
                        if (App.MasterViewModel.CurrentPostListItem.DraftIndex > -1)
                            App.MasterViewModel.CurrentBlog.LocalPostDrafts[App.MasterViewModel.CurrentPostListItem.DraftIndex] = post;
                    }
                    else
                    {
                        blog.LocalPostDrafts.Add(post);
                    }
                    // Exit post editor if the app was not lauched from the sharing feature.
                    if(NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    else
                        throw new ApplicationShouldEndException();
                }
            }
            else
            {
                EditPostRPC rpc = new EditPostRPC(App.MasterViewModel.CurrentBlog, post);
                rpc.Completed += OnEditPostRPCCompleted;
                rpc.ExecuteAsync();

                currentXmlRpcConnection = rpc;
                this.Focus();
                ApplicationBar.IsVisible = false;
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingChanges);
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

            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.HideIndicator();

            if (null == args.Error)
            {
                cleanupPostMedia();
                if (NavigationService.CanGoBack)
                {
                    DataService.Current.FetchCurrentBlogPostsAsync(false);
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
                //do not set the connection to null here
                return;
            } 

            if (this.isEditingLocalDraft)
            {
                // Local Draft was published
                App.MasterViewModel.CurrentBlog.LocalPostDrafts.Remove(App.MasterViewModel.CurrentPost);
            }
            ApplicationBar.IsVisible = true;
            App.WaitIndicationService.HideIndicator();

            if (null == args.Error)
            {
                cleanupPostMedia();
                DataService.Current.FetchCurrentBlogPostsAsync(false);
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
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
            else if (e.Content is BlogPanoramaPage)
            {
                //remove the listers!
                postFormatsPicker.SelectionChanged -= listPicker_SelectionChanged;
                postFormatsPicker.ItemsSource = null;
            }

            if (State.ContainsKey(TITLEKEY_VALUE))
            {
                State.Remove(TITLEKEY_VALUE);
            }
            State.Add(TITLEKEY_VALUE, titleTextBox.Text);

            if (State.ContainsKey(TAGSKEY_VALUE))
            {
                State.Remove(TAGSKEY_VALUE);
            }
            State.Add(TAGSKEY_VALUE, tagsTextBox.Text);

            base.OnNavigatedFrom(e);
        }

        private void OnSelectCategoriesButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SelectCategoriesPage.xaml", UriKind.Relative));
        }

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

            using (Stream pictureStream = e.ChosenPhoto)
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

            // Save to isolated storage in the draft_media directory 
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
            
            Post post = App.MasterViewModel.CurrentPost;
            post.Media.Add(currentMedia);

            //update the UI
            imageWrapPanel.Children.Add(BuildTappableImageElement(image, currentMedia));
        }

        private void ToggleGalleryControlsVisibility()
        {

            if (App.MasterViewModel.CurrentPost == null)
                return;

            //Gallery only on wpcom, or WP > 3.5
            if (App.MasterViewModel.CurrentBlog.isWPcom() == false && App.MasterViewModel.CurrentBlog.SupportsGallery() == false)
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

        private Canvas BuildTappableImageElement(BitmapImage image, Media currentMedia)
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

            Image img = new Image();
            img.Source = new BitmapImage(new Uri("/Images/star.png", UriKind.Relative));
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            img.Width = 30;
            img.Height = 30;
            if (!currentMedia.IsFeatured)
            {
                img.Visibility = Visibility.Collapsed;
            }
            
            Canvas canvas = new Canvas();
            canvas.Width = width;
            canvas.Height = height;
            canvas.Children.Add(imageOuterButton);
            canvas.Children.Add(img);
            canvas.Tag = currentMedia;
            
            Canvas.SetTop(img, (height - 45));
            Canvas.SetLeft(img, (width - 45));

            return canvas;
        }

        private void sp_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Control tappedObj = (Control)sender;
            Media currentMedia = (Media) tappedObj.Tag;
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
            Post post = App.MasterViewModel.CurrentPost;
            post.Media.Clear();
            ToggleGalleryControlsVisibility();
            
            // This forces the media section to layout properly after clicking the
            // clear media button.
            uploadImagesAsGalleryCheckbox.InvalidateMeasure();
            imageWrapPanel.InvalidateMeasure();
        }

        public void removeImage(Media imageToRemove)
        {
            Post post = App.MasterViewModel.CurrentPost;
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

        private void OnDatePickerChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            Post post = (Post) App.MasterViewModel.CurrentPost;
            if (post != null && e.NewDateTime != null)
            {
                if (sender == postDatePicker)
                {
                    postTimePicker.Value = e.NewDateTime;
                }
                else if (sender == postTimePicker)
                {
                    postDatePicker.Value = e.NewDateTime;
                }
                post.DateCreated = (DateTime)e.NewDateTime;
                post.DateCreatedGMT = ((DateTime)e.NewDateTime).ToUniversalTime();
            }
        }

        private void listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Post post = (Post)App.MasterViewModel.CurrentPost;
            if (sender == postFormatsPicker)
            {
                int newIndex = postFormatsPicker.SelectedIndex;
                if ( !(newIndex >= 0) || newIndex >= App.MasterViewModel.CurrentBlog.PostFormats.Count)
                    return;
                PostFormat newPostFormat = App.MasterViewModel.CurrentBlog.PostFormats[newIndex];
                post.PostFormat = newPostFormat.Key;
            }
        }

        private void OnVisualEditorButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditContent.xaml", UriKind.Relative));
        }

        private void OnTextEditorButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditContentTextMode.xaml", UriKind.Relative));
        }

        #endregion
    }
}