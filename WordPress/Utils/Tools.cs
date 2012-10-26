﻿using System;
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
        

        public static void LogMemoryUsage()
        {
            Debug.WriteLine(" ----- ");
            Debug.WriteLine("CurrentMemoryUsage " + Tools.convertMemory(DeviceStatus.ApplicationCurrentMemoryUsage));
            Debug.WriteLine("ApplicationPeakMemoryUsage " + Tools.convertMemory(DeviceStatus.ApplicationPeakMemoryUsage));
            Debug.WriteLine("ApplicationMemoryUsageLimit " + Tools.convertMemory(DeviceStatus.ApplicationMemoryUsageLimit));
            Debug.WriteLine("DeviceTotalMemory " + Tools.convertMemory(DeviceStatus.DeviceTotalMemory));
        }

        private static String convertMemory ( long size ) {
            string[] unit = { "b", "kb", "mb", "gb", "tb", "pb" };
            int floor1 = (int) Math.Floor(  Math.Log ( size, 1024) );
            return Math.Round( size / Math.Pow( 1024, floor1 ), 2 ) + " " + unit[floor1];
        }

    }

}
