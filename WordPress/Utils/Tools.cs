using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Info;

namespace WordPress.Utils
{
    public class Tools
    {

        private const bool _emulatorIsLowMemory = false;
        private static readonly Version _targetedVersion78 = new Version(7, 10, 8858);
        
        public static bool IsWindowsPhone78orHigher
        {
            get { return Environment.OSVersion.Version >= _targetedVersion78; }
        }

        public static bool IsWindowsPhone8orHigher
        {
            get { return Environment.OSVersion.Version.Major >= 8; }
        }

        //detect if user has selected a dark or light theme background in Windows Phone settings
        public bool IsLightTheme
        {
            get
            {
                var v = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"];
                return v == Visibility.Visible;
            }
        }

        /* 
         * Is low memory device? - To take advantage of devices that have more than 256 Mb of RAM available.
         * 
         * If ApplicationWorkingSetLimit is not found, then you’re not running on 7.1.1, which means there is no system paging,
         * which in turn means that the app’s working set limit is the same as its commit limit. 
         * See: http://blogs.windows.com/windows_phone/b/wpdev/archive/2012/09/12/future-proofing-your-apps.aspx
         * 
         */
        public bool IsLowMemoryDevice
        {
            get
            {

                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
                {
                    return _emulatorIsLowMemory;
                }

                long ninety = 90 * 1024 * 1024;
                long workingSetLimit;
                object ws;
                if( DeviceExtendedProperties.TryGetValue("ApplicationWorkingSetLimit", out ws) ) 
                {
                    workingSetLimit = Convert.ToInt64(ws);
                    Debug.WriteLine("Memory WorkingSetLimit " + Tools.convertMemory(workingSetLimit));
                }
                else 
                {
                     workingSetLimit = DeviceStatus.ApplicationMemoryUsageLimit; //tells you the maximum amount of memory that your application can allocate
                }

                if( workingSetLimit < ninety ) 
                {
                    return true;
                }
                
                return false;
            }
                /*
                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
                {
                    return _emulatorIsLowMemory;
                }
                else
                {
                    return (long)DeviceExtendedProperties.GetValue("DeviceTotalMemory") <= 268435456;
                }
                 * */
        }

        public static void LogException(string message, Exception exception)
        {
            //there isn't Trace fn in WP7, only Debug.Write.
            string separator = "***********************************************************";
            string titleFormat = "{0}: Exception";
            string messageFormat = "Message: {0}";
            string exceptionMessageFormat = "Exception Message: {0}";
            string keyValueFormat = "Key: {0}\t\tValue: {1}";
            string stackTraceFormat = "Stack Trace:\r\n{0}";

            Debug.WriteLine(string.Empty);
            Debug.WriteLine(separator);
            Debug.WriteLine(titleFormat, DateTime.Now);
            Debug.WriteLine(messageFormat, message);
            if (exception == null) return;

            Debug.WriteLine(exceptionMessageFormat, exception.Message);
            if (null != exception.Data && 0 < exception.Data.Count)
            {
                Debug.WriteLine("Data");
                foreach (object key in exception.Data.Keys)
                {
                    Debug.WriteLine(keyValueFormat, key, exception.Data[key]);
                }
            }
            Debug.WriteLine(stackTraceFormat, exception.StackTrace);
            Debug.WriteLine("Full exception:\n" + exception.ToString());
            Debug.WriteLine(separator);
            Debug.WriteLine(string.Empty);
        }

        public static void LogMemoryUsage()
        {
            Debug.WriteLine(" ----- ");
            Debug.WriteLine("CurrentMemoryUsage " + Tools.convertMemory(DeviceStatus.ApplicationCurrentMemoryUsage));
            Debug.WriteLine("ApplicationPeakMemoryUsage " + Tools.convertMemory(DeviceStatus.ApplicationPeakMemoryUsage));
            Debug.WriteLine("ApplicationMemoryUsageLimit " + Tools.convertMemory(DeviceStatus.ApplicationMemoryUsageLimit));
            Debug.WriteLine("DeviceTotalMemory " + Tools.convertMemory(DeviceStatus.DeviceTotalMemory));
        }

        public static String convertMemory(long size)
        {
            try
            {
                string[] unit = { "b", "kb", "mb", "gb", "tb", "pb" };
                int floor1 = (int)Math.Floor(Math.Log(size, 1024));
                return Math.Round(size / Math.Pow(1024, floor1), 2) + " " + unit[floor1];
            }
            catch (Exception e)
            {
                // Fail silently
            }
            return "n/a;";
        }
    }
}
