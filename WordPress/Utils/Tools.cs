using System;
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

        //Is low memory device? - To take advantage of devices that have more than 256 Mb of RAM available.
        public bool IsLowMemoryDevice
        {
            get
            {
                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
                {
                    return _emulatorIsLowMemory;
                }
                else
                {
                    return (long)DeviceExtendedProperties.GetValue("DeviceTotalMemory") <= 268435456;
                }
            }
        }
    }
}
