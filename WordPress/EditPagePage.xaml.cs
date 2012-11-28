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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WordPress.Localization;
using WordPress.Model;
using WordPress.Settings;

namespace WordPress
{
    public partial class EditPagePage : PhoneApplicationPage
    {
        //DEV NOTE: as far as the WP data model goes, there isn't a real difference
        //between a post and a page, so we use the "post" rpcs
        #region member variables

        private static object _syncRoot = new object();

        private const string DATACONTEXT_VALUE = "dataContext";
        private const string TITLEKEY_VALUE = "title";
        private const string CONTENTKEY_VALUE = "content";
        private const string PUBLISHKEY_VALUE = "publish";

        private ApplicationBarIconButton _saveIconButton;
        private StringTable _localizedStrings;

        private List<UploadFileRPC> _mediaUploadRPCs;
        private AbstractPostRPC currentXmlRpcConnection;
        public Media _lastTappedMedia = null; //used to pass the obj to the Media details page

        private bool _mediaDialogPresented = false;
        private bool isEditingLocalDraft = false;

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

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (Visibility.Visible == addLinkControl.Visibility)
            {
                HideAddLinkControl();
                e.Cancel = true;
            }
            else if (App.WaitIndicationService.Waiting)
            {
                App.WaitIndicationService.HideIndicator();
                ApplicationBar.IsVisible = true;
                e.Cancel = true;

                if (null != currentXmlRpcConnection)
                {
                    currentXmlRpcConnection.IsCancelled = true;
                    currentXmlRpcConnection = null;
                }

                _mediaUploadRPCs.ForEach(rpc =>
                {
                    rpc.Completed -= OnUploadMediaRPCCompleted;
                    rpc.IsCancelled = true;
                });
                _mediaUploadRPCs.Clear();
            }
            else
            {
                string prompt = string.Format(_localizedStrings.Prompts.SureCancel, _localizedStrings.Prompts.Page);
                MessageBoxResult result = MessageBox.Show(prompt, _localizedStrings.Prompts.CancelEditing, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    //remove the media
                    _mediaUploadRPCs.ForEach(rpc =>
                    {
                        rpc.Completed -= OnUploadMediaRPCCompleted;
                        rpc.IsCancelled = true;
                    });
                    _mediaUploadRPCs.Clear();
                    base.OnBackKeyPress(e);
                }
                else
                {
                    e.Cancel = true;
                }
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
                    foreach (Media currentMedia in App.MasterViewModel.CurrentPost.Media)
                    {
                        BitmapImage image = new BitmapImage();
                        image.SetSource(currentMedia.getImageStream());
                        imageWrapPanel.Children.Add(BuildTappableImageElement(image, currentMedia));
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

            RestorePageState();
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
            _mediaDialogPresented = false;
            Post post = DataContext as Post;

            //Do not publish pages with no title or content.
            if (0 >= post.Media.Count)
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
            post.Description = contentTextBox.Text;

            if (0 < post.Media.Count)
            {
                if (!post.PostStatus.Equals("localdraft"))
                {
                    foreach (Media currentMedia in post.Media)
                    {
                        UploadFileRPC rpc = new UploadFileRPC(App.MasterViewModel.CurrentBlog, currentMedia, true);
                        rpc.Completed += OnUploadMediaRPCCompleted;
                        //store this for later--we'll upload the files once the user hits save
                        _mediaUploadRPCs.Add(rpc);
                    }

                    UploadImagesAndSavePost();
                    return;
                }
            }

            SavePost();
        }

        private void UploadImagesAndSavePost()
        {
            this.Focus(); //hide the keyboard
            ApplicationBar.IsVisible = false; //hide the application bar
            App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.UploadingMedia);

            //fire off the worker rpcs
            _mediaUploadRPCs.ForEach(rpc => rpc.ExecuteAsync());
        }

        private void OnEditPostRPCCompleted(object sender, XMLRPCCompletedEventArgs<Post> args)
        {
            EditPostRPC rpc = sender as EditPostRPC;
            rpc.Completed -= OnEditPostRPCCompleted;
            ApplicationBar.IsVisible = true;

            if (args.Cancelled)
            {
                return;
            }
            else if (null == args.Error)
            {
                DataService.Current.FetchCurrentBlogPagesAsync();
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
            ApplicationBar.IsVisible = true;

            if (args.Cancelled)
            {
                return;
            }
            else if (null == args.Error)
            {
                DataService.Current.FetchCurrentBlogPagesAsync();
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
            InsertMarkupTagIntoContent(boldToggleButton, WordPressMarkupTags.BOLD_OPENING_TAG, WordPressMarkupTags.BOLD_CLOSING_TAG);
        }

        private void OnItalicToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(italicToggleButton, WordPressMarkupTags.ITALICS_OPENING_TAG, WordPressMarkupTags.ITALICS_CLOSING_TAG);
        }

        private void OnUnderlineToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(underlineToggleButton, WordPressMarkupTags.UNDERLINE_OPENING_TAG, WordPressMarkupTags.UNDERLINE_CLOSING_TAG);
        }

        private void OnStrikethroughToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(strikethroughToggleButton, WordPressMarkupTags.STRIKETHROUGH_OPENING_TAG, WordPressMarkupTags.STRIKETHROUGH_CLOSING_TAG);
        }

        private void OnBlockquoteToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(blockquoteToggleButton, WordPressMarkupTags.BLOCKQUOTE_OPENING_TAG, WordPressMarkupTags.BLOCKQUOTE_CLOSING_TAG);
        }

        private void InsertMarkupTagIntoContent(ToggleButton toggleButton, string openingTag, string closingTag)
        {
            Post post = DataContext as Post;
            string description = contentTextBox.Text;

            int startIndex = contentTextBox.SelectionStart;
            if (description.Length <= startIndex)
            {
                startIndex = description.Length;
            }

            string tag;
            int selectionLength = contentTextBox.SelectionLength;
            if (selectionLength > 0)
            {
                tag = openingTag;

                description = description.Insert(startIndex, openingTag);
                description = description.Insert(startIndex + openingTag.Length + selectionLength, closingTag);

                // cancel toggle switch
                toggleButton.IsChecked = !toggleButton.IsChecked;
            }
            else
            {
                if (toggleButton.IsChecked.Value)
                {
                    tag = openingTag;
                }
                else
                {
                    tag = closingTag;
                }

                description = description.Insert(startIndex, tag);
            }

            post.Description = description;

            ThreadPool.QueueUserWorkItem((state) =>
            {
                //yield long enough for the button to take focus away from the text box,
                //then reset focus to the text box
                Thread.Sleep(200);
                Dispatcher.BeginInvoke(() =>
                {
                    contentTextBox.Focus();
                    contentTextBox.SelectionStart = startIndex + tag.Length;
                    contentTextBox.SelectionLength = selectionLength;
                });
            });
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            if (e.Content is ImageDetailsPage)
            {
                (e.Content as ImageDetailsPage).TappedImage = _lastTappedMedia;
                _lastTappedMedia = null;
            }

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
        }

        private void OnLinkButtonClick(object sender, RoutedEventArgs e)
        {
            ShowLinkControl();
        }

        private void ShowLinkControl()
        {
            ApplicationBar.IsVisible = false;
            addLinkControl.Show();

            // if content text is selected, pre-populate the dialog's fields
            if (contentTextBox.SelectionLength > 0)
            {
                addLinkControl.LinkText = contentTextBox.SelectedText;

                if (Uri.IsWellFormedUriString(contentTextBox.SelectedText, UriKind.Absolute))
                {
                    addLinkControl.Url = contentTextBox.SelectedText;
                }
            }
        }

        private void HideAddLinkControl()
        {
            addLinkControl.Hide();
            ApplicationBar.IsVisible = true;
        }

        private void OnLinkChosen(object sender, EventArgs e)
        {
            HideAddLinkControl();
            string linkMarkup = addLinkControl.CreateLinkMarkup();
            contentTextBox.SelectedText = linkMarkup;
            contentTextBox.Focus();
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
            AddNewMediaStream(chosenPhoto, e.OriginalFileName);
        }

        private void AddNewMediaStream(Stream bitmapStream, string originalFilePath)
        {
            BitmapImage image = new BitmapImage();
            image.SetSource(bitmapStream);

            // 1.Resize the picture and save the output to the isolated storage if 'PreserveBandwidth' is enabled and dimensions are > 800 px.
            if (App.MasterViewModel.CurrentBlog.PreserveBandwidth && (image.PixelWidth > 800 || image.PixelHeight > 800))
            {
                // Create a file name for the JPEG file in isolated storage.
                String tempJPEG = "TempJPEG";

                // Create a virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(tempJPEG))
                {
                    myStore.DeleteFile(tempJPEG);
                }

                IsolatedStorageFileStream myFileStream = myStore.CreateFile(tempJPEG);
                WriteableBitmap wb = new WriteableBitmap(image);

                float wRatio = image.PixelWidth / 800F;
                float hRatio = image.PixelHeight / 800F;
                float currentRatio = Math.Max(wRatio, hRatio);
                int resizedWidth = (int)(image.PixelWidth / currentRatio);
                int resizedHeight = (int)(image.PixelHeight / currentRatio);

                // Encode the WriteableBitmap object to a JPEG stream.
                wb.SaveJpeg(myFileStream, resizedWidth, resizedHeight, 0, 85);
                myFileStream.Close();

                //The file is now encoded in the IsolatedStorage
                // Create a new stream from isolated storage
                bitmapStream = myStore.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);//change the stream reference for uploading
            }
            else
            {
                bitmapStream.Seek(0, 0); // necessary to initiate the stream correctly before save
            }

            //Save the picture to the picture library if it's a new picture           
            DateTime capture = DateTime.Now;
            string fileNameFormat = "SavedPicture-{0}{1}{2}{3}{4}{5}{6}"; //year, month, day, hours, min, sec, file extension
            string savedFileName = string.Format(fileNameFormat,
                capture.Year,
                capture.Month,
                capture.Day,
                capture.Hour,
                capture.Minute,
                capture.Second,
                Path.GetExtension(originalFilePath));

            // Save the image to the camera roll or saved pictures album.
            MediaLibrary library = new MediaLibrary();
            // Save the image to the saved pictures album.
            Picture pic = library.SavePicture(savedFileName, bitmapStream);
            string sanitizedFileName = this.translateFileName(originalFilePath); //Read and sanitize the file name from the original path for now. In the next release we can give the possibility to set an arbitraty file name                                                        
            DateTime pictureDateTime = pic.Date;
            bitmapStream.Close();

            Media currentMedia = new Media(App.MasterViewModel.CurrentBlog, sanitizedFileName, savedFileName, pictureDateTime);
            Post post = DataContext as Post;
            post.Media.Add(currentMedia);

            //update the UI
            imageWrapPanel.Children.Add(BuildTappableImageElement(image, currentMedia));
        }

        private string translateFileName(string originalFileName)
        {
            //DEV NOTE: the original file name from the PhotoChooserTask is pretty gross.
            //The plan is to nab the extension and use a timestamp for the file name so
            //there's something that doesn't seem crazy when the user checks what media
            //has been uploaded.
            const string PHOTOCHOOSER_VALUE = "PhotoChooser";

            if (originalFileName.Contains(PHOTOCHOOSER_VALUE))
            {
                DateTime capture = DateTime.Now;
                string fileNameFormat = "{0}{1}{2}{3}{4}{5}{6}"; //year, month, day, hours, min, sec, file extension
                string fileName = string.Format(fileNameFormat,
                    capture.Year,
                    capture.Month,
                    capture.Day,
                    capture.Hour,
                    capture.Minute,
                    capture.Second,
                    Path.GetExtension(originalFileName));
                return fileName;
            }

            //if we're at this point, the file name should be reasonably readable so we'll
            //leave it alone
            return Path.GetFileName(originalFileName);
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
            _mediaUploadRPCs.ForEach(rpc => rpc.Completed -= OnUploadMediaRPCCompleted);
            _mediaUploadRPCs.Clear();
            Post post = DataContext as Post;
            post.Media.Clear();
        }


        public void removeImage(Media imageToRemove)
        {
            Post post = DataContext as Post;
            post.Media.Remove(imageToRemove);
            foreach (var el in imageWrapPanel.Children)
            {
                if ((el as Control).Tag == imageToRemove)
                {
                    imageWrapPanel.Children.Remove(el);
                    break;
                }
            }
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
                if (null == args.Error)
                {
                    //Image uploaded correctly
                }
                
                if (args.Items.Count > 0)
                {
                    // _infoToRpcMap.Add(args.Items[0], rpc);
                }
                else
                {
                    //uh oh, media upload problem
                    App.WaitIndicationService.KillSpinner();
                    ApplicationBar.IsVisible = true;

                    if (!_mediaDialogPresented)
                    {
                        _mediaDialogPresented = true;
                        MessageBoxResult result = MessageBox.Show(_localizedStrings.Prompts.MediaErrorContent, _localizedStrings.Prompts.MediaError, MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            UpdatePostContent();
                            SavePost();
                            return;
                        }
                        else
                        {
                            //add the object back since the user wants to have another go at uploading
                            rpc.Completed += OnUploadMediaRPCCompleted;
                            _mediaUploadRPCs.Add(rpc);
                            return;
                        }
                    }
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
            StringBuilder prependBuilder = new StringBuilder();
            StringBuilder appendBuilder = new StringBuilder();

            Blog currentBlog = App.MasterViewModel.CurrentBlog;
            Post post = DataContext as Post;
            foreach (Media currentMedia in post.Media)
            {
                if (currentMedia.placement != eMediaPlacement.BlogPreference)
                {
                    if (currentMedia.placement == WordPress.Model.eMediaPlacement.Before)
                    {
                        prependBuilder.Append(currentMedia.getHTML());
                    }
                    else
                    {
                        appendBuilder.Append(currentMedia.getHTML());
                    }
                }
                else
                {
                    if (currentBlog.PlaceImageAboveText)
                    {
                        prependBuilder.Append(currentMedia.getHTML());
                    }
                    else
                    {
                        appendBuilder.Append(currentMedia.getHTML());
                    }
                }
            }

            contentTextBox.Text = prependBuilder.ToString() + contentTextBox.Text + appendBuilder.ToString();
            contentTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void SavePost()
        {
            Post post = App.MasterViewModel.CurrentPost;
            Blog blog = App.MasterViewModel.CurrentBlog;
            //make sure the post has the latest UI data--the Save button is a ToolbarButton
            //which doesn't force focus to change
            post.Title = titleTextBox.Text;
            post.Description = contentTextBox.Text;
            if (post.IsNew)
            {
                if (!post.PostStatus.Equals("localdraft"))
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
                    NavigationService.GoBack();
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

    }
}