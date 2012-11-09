using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using System.IO;
using System.Text;
using System.Globalization;
using WordPress.Model;
using WordPress.Settings;

namespace WordPress
{
    public partial class App : Application
    {
        private static MasterViewModel _masterViewModel = null;

        public static MasterViewModel MasterViewModel
        {
            get
            {
                if (null == _masterViewModel)
                {
                    _masterViewModel = new MasterViewModel();
                }
                return _masterViewModel;
            }
        }


        private static IWaitIndicationService _waitIndicationService = null;

        public static IWaitIndicationService WaitIndicationService
        {
            get
            {
                if (null == _waitIndicationService)
                {
                    _waitIndicationService = new WaitIndicationService();
                }
                return _waitIndicationService;
            }
        }

        private static IPopupSelectionService _popupSelectionService = null;

        public static IPopupSelectionService PopupSelectionService
        {
            get
            {
                if (null == _popupSelectionService)
                {
                    _popupSelectionService = new PopupSelectionService();
                }
                return _popupSelectionService;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

        }

        private void InitializeUriMapper()
        {
            //DEV NOTE: application launching and application activated happen on two different
            //execution paths--if we want the UriMapper resource to contain the bindings below
            //we have to ensure that the method is called in both places.
            UriMapper mapper = Resources["UriMapper"] as UriMapper;
            RootFrame.UriMapper = mapper;

            if (null == DataService.Current.Blogs || 0 == DataService.Current.Blogs.Count)
            {
                mapper.UriMappings[0].MappedUri = new Uri("/LocateBlogPage.xaml", UriKind.Relative);
            }
            else
            {
                mapper.UriMappings[0].MappedUri = new Uri("/BlogsPage.xaml", UriKind.Relative);
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            InitializeUriMapper();
            checkStats();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            InitializeUriMapper();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            DataService.Current.Serialize();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();
            RootFrame.Background = new SolidColorBrush(Colors.White);
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        void checkStats()
        {
            UserSettings settings = new UserSettings();

            DateTime lastRefresh = settings.LastStatsUpload;

            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now.Subtract(lastRefresh);
            if (timeDifference.Days > 7)
            {
                settings.LastStatsUpload = now;
                settings.Save();
                //upload stats
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.wordpress.org/windowsphoneapp/update-check/1.0/");
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
            }

        }

        private static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);
            
            //gather stats data
            string device_uuid = "";
            try
            {
                // In some cases the ANID property might be unavailable and will throw an exception.
                device_uuid = (string)Microsoft.Phone.Info.UserExtendedProperties.GetValue("ANID");
            } 
            catch(Exception e) 
            {
                // Fail silently
            }
            string app_version = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
            string device_language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            string mobile_network_type = NetworkInterface.NetworkInterfaceType.ToString();
            string device_version = System.Environment.OSVersion.ToString().Replace("Microsoft Windows CE ", "");
            string postData = String.Format("device_uuid={0}&app_version={1}&device_language={2}&mobile_network_type={3}&device_version={4}", HttpUtility.UrlEncode(device_uuid), HttpUtility.UrlEncode(app_version), HttpUtility.UrlEncode(device_language), HttpUtility.UrlEncode(mobile_network_type), HttpUtility.UrlEncode(device_version));
            
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();
            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
        }

        private static void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                response.Close();
            }
            catch (WebException e)
            {
                //moving on
            }
        }
    }


}