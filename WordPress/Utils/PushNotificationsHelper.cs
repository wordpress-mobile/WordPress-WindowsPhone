using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using WordPress.Localization;
using WordPress.Model;
using WordPress.Settings;

namespace WordPress.Utils
{
    public sealed class PushNotificationsHelper
    {
        private static volatile PushNotificationsHelper instance;
        private static object syncRoot = new Object();

        private static string pushNotificationURL = "http://ercolid.wordpress.com/xmlrpc.php";

        /// Holds the push channel that is created or found.
        private HttpNotificationChannel pushChannel;
        // The name of our push channel.
        private string channelName = "WordPressDemoChannel";

        private bool processingError = false;

        #region costructors
        private PushNotificationsHelper() { }

        //Multithreaded Singleton. Ref: Multithreaded Singleton
        public static PushNotificationsHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new PushNotificationsHelper();
                    }
                }

                return instance;
            }
        }
        #endregion

        #region custom methods

        public bool pushNotificationsEnabled()
        {
            //check the push notifications user settings
            UserSettings settings = new UserSettings();
            if (!settings.AskedPermissionForToastPushNotifications || !settings.EnableToastPushNotifications)
            {
                return false;
            }

            return true;
        }

        public void checkPushNotificationsUserPermissions()
        {
            //The app must ask the user for explicit permission to receive toast notifications.
            UserSettings settings = new UserSettings();
            if (!settings.AskedPermissionForToastPushNotifications)
            {
                StringTable _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
                MessageBoxResult result = MessageBox.Show(_localizedStrings.Prompts.AllowToastPushNotification, _localizedStrings.ControlsText.PushNotifications, MessageBoxButton.OKCancel);
                if (MessageBoxResult.OK == result)
                {
                    settings.EnableToastPushNotifications = true;
                }
                else
                {
                    settings.EnableToastPushNotifications = false;
                }
                settings.AskedPermissionForToastPushNotifications = true;
                settings.Save();

                if (settings.EnableToastPushNotifications == true)
                {
                    Double fontSize = (Double) Application.Current.Resources["ControlFontSize"];
                    var messagePrompt = new MessagePrompt
                    {
                        Title = _localizedStrings.ControlsText.PushNotifications,
                        Body = new TextBlock { Text = _localizedStrings.Prompts.PinToStartPushNotification, FontSize= fontSize, TextWrapping = TextWrapping.Wrap },
                        IsCancelVisible = false
                    };
                    messagePrompt.Show();
                }          
            }
        }

        private string getDeviceUUID()
        {
            string device_uuid = null;

            try
            {
                byte[] result = null;
                object uniqueId;
                if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                {
                    result = (byte[])uniqueId;
                    device_uuid = Convert.ToBase64String(result);
                }
            }
            catch (Exception uuid_ex)
            {
                Utils.Tools.LogException("Error while retrieving DeviceUniqueId", uuid_ex);
                return null;
            }

            return device_uuid;
        }

        public void sendBlogsList()
        {
            string device_uuid = this.getDeviceUUID();
            if (device_uuid == null) return; //emulators

            Dictionary<string, string> credentials = new Dictionary<string, string>();
            Dictionary<string, Queue<int>> blogIDsDict = new Dictionary<string, Queue<int>>();

            //loop over blogs and retrieves the list of .com accounts added to the app
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    if (currentBlog.isWPcom())
                    {
                        if (!credentials.ContainsKey(currentBlog.Username))
                            credentials.Add(currentBlog.Username, currentBlog.Password);
                      
                        Queue<int> currentAccountBlogIDs;
                        blogIDsDict.TryGetValue(currentBlog.Username, out currentAccountBlogIDs);
                        if( currentAccountBlogIDs == null ) {
                            currentAccountBlogIDs = new Queue<int>();
                            currentAccountBlogIDs.Enqueue(currentBlog.BlogId);
                            blogIDsDict.Add(currentBlog.Username, currentAccountBlogIDs);
                        } else {
                            currentAccountBlogIDs.Enqueue(currentBlog.BlogId);
                        }
                    }
                    else
                    {
                        if (currentBlog.DotcomUsername != null && currentBlog.DotcomPassword != null)
                        {
                            if (!credentials.ContainsKey(currentBlog.DotcomUsername))
                                credentials.Add(currentBlog.DotcomUsername, currentBlog.DotcomPassword);

                            Queue<int> currentAccountBlogIDs;
                            blogIDsDict.TryGetValue(currentBlog.DotcomUsername, out currentAccountBlogIDs);
                            if (currentAccountBlogIDs == null)
                            {
                                currentAccountBlogIDs = new Queue<int>();
                                string currentJetpackBlogID = currentBlog.getJetpackClientID();
                                if (currentJetpackBlogID == null) 
                                    continue;
                                currentAccountBlogIDs.Enqueue(Convert.ToInt32(currentJetpackBlogID));
                                blogIDsDict.Add(currentBlog.DotcomUsername, currentAccountBlogIDs);
                            }
                            else
                            {
                                currentAccountBlogIDs.Enqueue(currentBlog.BlogId);
                            }
                        }
                    }//end else
                }
            }

            string app_version = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
            foreach (KeyValuePair<String, String> entry in credentials)
            {
                // do something with entry.Value or entry.Key
                Queue<int> currentAccountBlogIDs;
                blogIDsDict.TryGetValue(entry.Key, out currentAccountBlogIDs);
                if (currentAccountBlogIDs == null)
                    continue;
                PushNotificationsSendBlogsList rpc = new PushNotificationsSendBlogsList(pushNotificationURL, entry.Key, entry.Value, device_uuid, currentAccountBlogIDs, app_version);
               // rpc.Completed += OnSendBlogsListCompleted;
                rpc.ExecuteAsync();
            }
        }

        private void OnSendBlogsListCompleted(object sender, XMLRPCCompletedEventArgs<BooleanResponseObject> args)
        {
            PushNotificationsSendBlogsList rpc = sender as PushNotificationsSendBlogsList;
            rpc.Completed -= OnSendBlogsListCompleted;
            if (args.Cancelled)
            {
            }
            else if (null == args.Error)
            {
                return;
            }
            else
            {
                Exception e = args.Error;
                Utils.Tools.LogException(String.Format("PushNotificationsSendBlogsList  error occurred. {0}", e.Message), e);
            }
        }

        private void UnregisterDevice()
        {
            string device_uuid = this.getDeviceUUID();
            if (device_uuid == null) return; //emulators

            //loop over blogs and retrieves the list of .com accounts added to the app
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    if (currentBlog.isWPcom())
                    {
                        // do something with entry.Value or entry.Key
                        UnregisterPushNotificationToken rpc = new UnregisterPushNotificationToken(pushNotificationURL, currentBlog.Username, currentBlog.Password, device_uuid);
                        rpc.ExecuteAsync();
                        return;
                    }
                    else if (currentBlog.DotcomUsername != null && currentBlog.DotcomPassword != null)
                    {
                        // do something with entry.Value or entry.Key
                        UnregisterPushNotificationToken rpc = new UnregisterPushNotificationToken(pushNotificationURL, currentBlog.DotcomUsername, currentBlog.DotcomPassword, device_uuid);
                        rpc.ExecuteAsync();
                        return;
                    }
                }
            }
        }

        private void registerDevice(string channelUri)
        {
            string device_uuid = this.getDeviceUUID();
            if (device_uuid == null) return; //emulators

            Dictionary<string, string> credentials = new Dictionary<string,string>();
            //loop over blogs and retrieves the list of .com accounts added to the app
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    if(currentBlog.isWPcom()){
                        if( ! credentials.ContainsKey(currentBlog.Username))
                            credentials.Add(currentBlog.Username, currentBlog.Password);
                    }
                    else
                    {
                        if(currentBlog.DotcomUsername != null && currentBlog.DotcomPassword != null &&  ! credentials.ContainsKey(currentBlog.DotcomUsername))
                            credentials.Add(currentBlog.DotcomUsername, currentBlog.DotcomPassword);
                    }
                }
            }
            bool sendBlogsList = true;
            foreach (KeyValuePair<String, String> entry in credentials)
            {
                // do something with entry.Value or entry.Key
                RegisterPushNotificationToken rpc = new RegisterPushNotificationToken(pushNotificationURL, entry.Key, entry.Value, "1", device_uuid, channelUri);
                if (sendBlogsList)
                {
                    sendBlogsList = false; //Just send one blog list when thre more than one account
                    rpc.Completed += OnRegisterTokenCompleted;
                }
                rpc.ExecuteAsync();
            }
        }

        private void OnRegisterTokenCompleted(object sender, XMLRPCCompletedEventArgs<BooleanResponseObject> args)
        {
            RegisterPushNotificationToken rpc = sender as RegisterPushNotificationToken;
            rpc.Completed -= OnRegisterTokenCompleted;
            if (null == args.Error)
            {
                this.sendBlogsList(); 
                return;
            }
            else
            {
               Exception e = args.Error;
               Utils.Tools.LogException(String.Format("Register Token  error occurred. {0}", e.Message), e);
            }
        }

        public void resetTileCount()
        {
            // Reset the application Tile
            int newCount = 0;

            // Application Tile is always the first Tile, even if it is not pinned to Start.
            ShellTile TileToFind = ShellTile.ActiveTiles.First();
            
            // Application should always be found
            if (TileToFind != null)
            {
                // Set the properties to update for the Application Tile.
                // Empty strings for the text values and URIs will result in the property being cleared.
                StandardTileData NewTileData = new StandardTileData
                {
                    Count = newCount,
                };
                
                // Update the Application Tile
                TileToFind.Update(NewTileData);
            }
        }

        public void resetLastPushNotificationData()
        {
            this.loadLastPushNotificationData(null);
        }

        public void loadLastPushNotificationData(XMLRPCCompletedEventHandler<IntResponseObject> respHandler)
        {
            string device_uuid = this.getDeviceUUID();
            if (device_uuid == null) return; //emulators

            List<Blog> blogs = DataService.Current.Blogs.ToList();
            string username = string.Empty;
            string password = string.Empty;
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    if (currentBlog.isWPcom())
                    {
                        username = currentBlog.Username;
                        password = currentBlog.Password;
                    }
                    else if (currentBlog.DotcomUsername != null && currentBlog.DotcomPassword != null)
                    {
                        username = currentBlog.DotcomUsername;
                        password = currentBlog.DotcomPassword;
                    }
                }
            }

            if (username != string.Empty && password != string.Empty)
            {
                PushNotificationGetLastNotification rpc = new PushNotificationGetLastNotification(pushNotificationURL, username, password, device_uuid);
                if (respHandler!=null)
                    rpc.Completed += respHandler;
                rpc.ExecuteAsync();
            }
        }


        public void OnLoadLastNotificationCompleted(object sender, XMLRPCCompletedEventArgs<IntResponseObject> args)
        {
            XmlRemoteProcedureCall<IntResponseObject> rpc = sender as XmlRemoteProcedureCall<IntResponseObject>;
            rpc.Completed -= OnLoadLastNotificationCompleted;
            if (null == args.Error && args.Items.Count > 0)
            {
                IntResponseObject respObj = args.Items.First();
                IntResponseObject commentIDObj = args.Items.Last();
 
                if (respObj.Value == 0 || respObj.Value == -1)
                    return;

                if (commentIDObj == null || commentIDObj.Value == 0 || commentIDObj.Value == -1)
                    return;

                string blogID = string.Format("{0}", respObj.Value);
                string commentID = string.Format("{0}", commentIDObj.Value);
                showToastForNewComment(blogID, commentID);
                return;
            }
            else
            {
                Exception e = args.Error;
#if DEBUG
                MessageBoxResult result = MessageBox.Show(String.Format("Error occurred. {0}", e.Message), "Error reading the last notification", MessageBoxButton.OK);
#endif
                Utils.Tools.LogException(String.Format("Error occurred. {0}", e.Message), e);
            }
        }

        private void showToastForNewComment(string blogID, string commentID)
        {

            if (blogID == string.Empty || commentID == string.Empty || blogID == null || commentID == null)
                return;

            System.Diagnostics.Debug.WriteLine("showToastForNewComment called with - blogID:" + blogID + " commentID:" + commentID);
            string blogName = string.Empty;

            List<Blog> blogs = DataService.Current.Blogs.ToList();
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    string currentBlogID = currentBlog.isWPcom() ? Convert.ToString(currentBlog.BlogId) : currentBlog.getJetpackClientID();
                    if (currentBlogID == blogID)
                    {
                        blogName = currentBlog.BlogName;
                    }
                }
            }

            if (blogName == String.Empty)
                return;

            NestedClass nc = new NestedClass(blogID, commentID);
            UIThread.Invoke(() =>
            {
                if (App.WaitIndicationService.Waiting)
                    return;

                if (App.PopupSelectionService.IsPopupOpen)
                    return;
            
                ToastPrompt toast = new ToastPrompt();
                toast.MillisecondsUntilHidden = 6000;
                toast.Title = "New comment on " + blogName;
                toast.Tap += nc.toast_Tapped;
                toast.Show();
            });
        }

        public void showCommentScreenFromNotification(string blogID, string commentID)
        {
            System.Diagnostics.Debug.WriteLine("showCommentScreenFromNotification called with - blogID:" + blogID + " commentID:" + commentID);
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    string currentBlogID = currentBlog.isWPcom() ? Convert.ToString(currentBlog.BlogId) : currentBlog.getJetpackClientID();
                    if (currentBlogID == blogID)
                    {
                        PhoneApplicationFrame frame = (PhoneApplicationFrame)Application.Current.RootVisual;
                        string paramString = "?blog_id=" + blogID + "&comment_id=" + commentID;
                        frame.Navigate(new Uri("/PushNotificationCommentPage.xaml" + paramString, UriKind.Relative));
                        return;
                    }
                }
            }
        }

        #endregion

        #region Push Notifications Init methods

        public void disablePushNotifications()
        {
            // Try to find the push channel.
            pushChannel = HttpNotificationChannel.Find(channelName);

            if (pushChannel != null)
            {
                pushChannel.Close();
                pushChannel.Dispose();
                pushChannel = null;
            }

            this.UnregisterDevice();

            return;
        }

        public void enablePushNotifications() {

            string device_uuid = this.getDeviceUUID();
            if (device_uuid == null) return; //emulators

            //check the push notifications user settings
            UserSettings settings = new UserSettings();
            if (! this.pushNotificationsEnabled() )
            {
                this.disablePushNotifications();
                return;
            }

            //check if there is a .COM or Jetpack blog in the app. 
            List<Blog> blogs = DataService.Current.Blogs.ToList();
            bool presence = false;
            foreach (Blog currentBlog in blogs)
            {
                if (currentBlog.isWPcom() || currentBlog.hasJetpack())
                {
                    presence = true;
                    break;
                }
            }
            if (! presence)
            {
                System.Diagnostics.Debug.WriteLine("Not found a .COM or Jetpack blog");
                this.disablePushNotifications();
                return;
            }

            // Try to find the push channel.
            pushChannel = HttpNotificationChannel.Find(channelName);

            // If the channel was not found, then create a new connection to the push service.
            if (pushChannel == null)
            {
                pushChannel = new HttpNotificationChannel(channelName);

                // Register for all the events before attempting to open the channel.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                pushChannel.ConnectionStatusChanged += new EventHandler<NotificationChannelConnectionEventArgs>(PushChannel_ConnectionStatusChanged);

                // Register for this notification since we need to receive the notifications while the application is running.
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                try
                {
                    pushChannel.Open();
                }
                catch (InvalidOperationException  _pushNotificationChannelOpenFailed)
                {
                    Utils.Tools.LogException("Cannot open the channel, try it again...", _pushNotificationChannelOpenFailed);
                    try
                    {
                        pushChannel.Open();
                    }
                    catch (InvalidOperationException)
                    {
                        Utils.Tools.LogException("2nd tentative failed", _pushNotificationChannelOpenFailed);
                        return;
                    }
                }

                try
                {
                    // Bind this new channel for toast events.
                    pushChannel.BindToShellToast();
                }
                catch (InvalidOperationException _pushNotificationChannelBindFailed)
                {
                    Utils.Tools.LogException("BindToShellToast Failed", _pushNotificationChannelBindFailed);
                    try
                    {
                        pushChannel.BindToShellToast();
                    }
                    catch (InvalidOperationException) { }
                }

                try
                {
                    // Bind this new channel for Tile events.
                    pushChannel.BindToShellTile();
                }
                catch (InvalidOperationException _pushNotificationChannelBindFailed)
                {
                    Utils.Tools.LogException("BindToShellTile Failed", _pushNotificationChannelBindFailed);
                    try
                    {
                        pushChannel.BindToShellTile();
                    }
                    catch (InvalidOperationException) { }
                }

            }
            else
            {
                // The channel was already open, so just register for all the events.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                pushChannel.ConnectionStatusChanged += new EventHandler<NotificationChannelConnectionEventArgs>(PushChannel_ConnectionStatusChanged);

                // Register for this notification since we need to receive the notifications while the application is running.
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                System.Diagnostics.Debug.WriteLine(pushChannel.ChannelUri.ToString());
                this.registerDevice(pushChannel.ChannelUri.ToString());
            }
        }

       
        /// <summary>
        /// Event handler for when the connection status changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ConnectionStatusChanged(object sender, NotificationChannelConnectionEventArgs e)
        {
            Utils.Tools.LogException(String.Format("Notification Channel Connection Changed:  {0}", e.ConnectionStatus.ToString()), null);
        }


        /// <summary>
        /// Event handler for when the push channel Uri is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ChannelUri.ToString());
            this.registerDevice(e.ChannelUri.ToString());
        }

        /// <summary>
        /// Event handler for when a push notification error occurs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            Utils.Tools.LogException((String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
           e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData)), null);

            if (processingError)
                return;

            processingError = true;
            switch (e.ErrorType)
            {
                case ChannelErrorType.ChannelOpenFailed:
                case ChannelErrorType.PayloadFormatError:

                    //ChannelErrorType.ChannelOpenFailed
                    //This error is returned when the Push Client and the Push Notification Service are unable to establish a connection

                    //ChannelErrorType.PayloadFormatError:
                    //This error is returned when the XML payload format or the HTTP header of the push notification is syntactically invalid.

                    pushChannel = HttpNotificationChannel.Find(channelName);

                    if (pushChannel != null)
                    {
                        pushChannel.Close();
                        pushChannel.Dispose();
                        pushChannel = null;
                    }
                    this.enablePushNotifications();
                    break;
                case ChannelErrorType.NotificationRateTooHigh:
                    //This error is returned when the Push Client is unable to receive messages because the web service is sending too many messages at too quick a rate to a certain device.
                    //Slow down the notifications 
                    //@TODO: send the server a signal?
                    break;
                case ChannelErrorType.MessageBadContent:
                    //This error is returned when the image reference is pointing to an HTTP image, even though the notification channel is not currently bound to a list of URIs. 
                    //This should never happen to us
                    break;
                case ChannelErrorType.PowerLevelChanged:
                    //This has been deprecated because push client no longer takes any action based on any power states
                    break;
                case ChannelErrorType.Unknown:
                    //An internal error has occurred and could not be recovered. A device reboot may be necessary.
                    break;
                default:
                    break;
            }

            processingError = false;
        }

        /// <summary>
        /// Event handler for when a toast notification arrives while your application is running.  
        /// The toast will not display if your application is running so you must add this
        /// event handler if you want to do something with the toast notification.
        /// 
        /// We need to reset the badge and send to blog lists. That way the badge count is reset server side.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            this.resetTileCount();
            var worker = new BackgroundWorker();
            worker.DoWork += (workSender, e2) =>
            {
                try
                {
                    this.sendBlogsList();
                }
                catch (Exception err)
                {
                    Utils.Tools.LogException("SendBlogsList in PushChannel_ShellToastNotificationReceived failed.", err);
                }
            };
            worker.RunWorkerAsync();
            
            string relativeUri = string.Empty;
            string text2 = string.Empty;

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }

                if (string.Compare(
                    key,
                    "wp:Text2",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    text2 = e.Collection[key];
                }
            }

            if (relativeUri == string.Empty || text2 == string.Empty)
                return;

            string blog_id = string.Empty;
            string comment_id = string.Empty;
            foreach (string currentItem in relativeUri.Split('&'))
            {
                string[] keyValue = currentItem.Split('=');
                if (keyValue[0] == "?blog_id")
                {
                    blog_id = keyValue[1];
                }
                else if (keyValue[0] == "comment_id")
                {
                    comment_id = keyValue[1];
                }
            }

            if (blog_id == string.Empty || comment_id == string.Empty)
                return;

            showToastForNewComment(blog_id, comment_id);
            this.resetLastPushNotificationData();
        }

        private class NestedClass
        {
            private string _blogID = string.Empty;
            private string _commentID;

            public NestedClass(string blogID, string commentID)
            {
                this._blogID = blogID;
                this._commentID = commentID;
            }

            public void toast_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("toast_Tap");
                System.Diagnostics.Debug.WriteLine("toast_Tap -> " + this._blogID + " "+ this._commentID);
                PushNotificationsHelper.Instance.showCommentScreenFromNotification(this._blogID, this._commentID);
            }
        }
        
        #endregion
    }
}
