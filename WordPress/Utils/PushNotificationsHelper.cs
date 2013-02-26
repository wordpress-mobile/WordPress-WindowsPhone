using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
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
            }
        }

        private string getDeviceUUID()
        {
            string device_uuid = (string)Microsoft.Phone.Info.UserExtendedProperties.GetValue("ANID");
            if (device_uuid == null)
                return null;
            string anonymousUserId = device_uuid.Substring(2, 32);
            return anonymousUserId;
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
                rpc.Completed += OnSendBlogsListCompleted;
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
                System.Diagnostics.Debug.WriteLine(String.Format("PushNotificationsSendBlogsList  error occurred. {0}", e.Message));
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

            foreach (KeyValuePair<String, String> entry in credentials)
            {
                // do something with entry.Value or entry.Key
                RegisterPushNotificationToken rpc = new RegisterPushNotificationToken(pushNotificationURL, entry.Key, entry.Value, "1", device_uuid, channelUri);
                rpc.Completed += OnRegisterTokenCompleted;
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
               System.Diagnostics.Debug.WriteLine(String.Format("Register Token  error occurred. {0}", e.Message));
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

                // Register for this notification only if you need to receive the notifications while your application is running.
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                //Register for RAW notifications
                //pushChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(PushChannel_HttpNotificationReceived);

                pushChannel.Open();

                // Bind this new channel for toast events.
                pushChannel.BindToShellToast();

                // Bind this new channel for Tile events.
                pushChannel.BindToShellTile();
            }
            else
            {
                // The channel was already open, so just register for all the events.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                // Register for this notification only if you need to receive the notifications while your application is running.
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                //Register for RAW notifications
                //pushChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(PushChannel_HttpNotificationReceived);

                // Display the URI for testing purposes. Normally, the URI would be passed back to your web service at this point.
                System.Diagnostics.Debug.WriteLine(pushChannel.ChannelUri.ToString());
                // MessageBox.Show(String.Format("Channel Uri is {0}", pushChannel.ChannelUri.ToString()));

                this.registerDevice(pushChannel.ChannelUri.ToString());
            }
        }

        /// <summary>
        /// Event handler for when the push channel Uri is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ChannelUri.ToString());
            //MessageBox.Show(String.Format("Channel Uri is {0}", e.ChannelUri.ToString()));
            this.registerDevice(e.ChannelUri.ToString());
        }

        /// <summary>
        /// Event handler for when a push notification error occurs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            UIThread.Invoke(() =>
                MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                    e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                    );
        }

        /* RAW Notifications
        void PushChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            string message;

            using (System.IO.StreamReader reader = new System.IO.StreamReader(e.Notification.Body))
            {
                message = reader.ReadToEnd();
            }


            UIThread.Invoke(() =>
                MessageBox.Show(String.Format("Received Notification {0}:\n{1}",
                    DateTime.Now.ToShortTimeString(), message))
                    );
        }
        */

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
            StringBuilder message = new StringBuilder();
            string relativeUri = string.Empty;

            message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
            }

            // Display a dialog of all the fields in the toast.
            UIThread.Invoke(() => MessageBox.Show(message.ToString()));

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
                    System.Diagnostics.Debug.WriteLine(String.Format("sendBlogsList in PushChannel_ShellToastNotificationReceived failed. {0}", err.Message));
                }
            };
            worker.RunWorkerAsync();
        }
        #endregion
    }
}
