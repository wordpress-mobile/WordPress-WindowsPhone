using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using WordPress.Model;

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

        #region Custom methods
        public static bool isNetworkAvailable()
        {
            bool hasNetworkConnection = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            return false;
           // return hasNetworkConnection;
        }
        #endregion

    }


}