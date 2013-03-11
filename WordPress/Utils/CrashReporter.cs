// See http://blogs.msdn.com/b/andypennell/archive/2010/11/01/error-reporting-on-windows-phone-7.aspx
using Microsoft.Phone.Tasks;
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using WordPress.Model;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Info;

namespace WordPress.Utils
{
    public class CrashReporter
    {
        const string filename = "CrashReporterLog.txt";

        internal static void ReportException(Exception ex, string source)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine("Please describe what you were doing when the app crashed:");
                        output.WriteLine("\n");
                        
                        output.WriteLine("\n--------------\n");

                        try
                        {
                            output.WriteLine("Extra Info:");
                            string app_version = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
                            output.WriteLine("App Version: " + app_version);
                            output.WriteLine("Device Name: " + DeviceStatus.DeviceName);
                            output.WriteLine("Device Manufacturer: " + DeviceStatus.DeviceManufacturer);
                            output.WriteLine("Device Hardware Version: " + DeviceStatus.DeviceHardwareVersion);
                            output.WriteLine("Device Firmware Version: " + DeviceStatus.DeviceFirmwareVersion);
                            string mobile_network_type = NetworkInterface.NetworkInterfaceType.ToString();
                            output.WriteLine("Network Type: " + mobile_network_type);
                            string device_language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                            output.WriteLine("Device Language: " + device_language);
                            string device_version = System.Environment.OSVersion.ToString().Replace("Microsoft Windows CE ", "");
                            output.WriteLine("OS Version: " + device_version);
                            output.WriteLine("\n--------------\n");
                        }
                        catch (Exception)
                        {
                        }

                        if (source != null)
                        {
                            output.WriteLine("Source Info:");
                            output.WriteLine(source);
                            output.WriteLine("\n--------------\n");
                        }

                        if (ex != null && ex.Message != null)
                        {
                            output.WriteLine("Message:");
                            output.WriteLine(ex.Message);
                            output.WriteLine("\n--------------\n");
                        }

                        if (ex != null && ex.StackTrace != null)
                        {
                            output.WriteLine("Stack Trace: \n");
                            output.WriteLine(ex.StackTrace);
                            output.WriteLine("\n--------------\n");
                        }

                        if (ex != null  && null != ex.Data && 0 < ex.Data.Count)
                        {
                            string keyValueFormat = "Key: {0}\t\tValue: {1}";
                            output.WriteLine("Data:");
                            foreach (object key in ex.Data.Keys)
                            {
                                output.WriteLine(keyValueFormat, key, ex.Data[key]);
                            }
                            output.WriteLine("\n--------------\n");
                        }

                        if (ex != null && null != ex.GetBaseException())
                        {
                            output.WriteLine("Base Exception ToString:\n" + ex.GetBaseException().ToString());
                            output.WriteLine("\n--------------\n");
                        }

                        output.WriteLine("Memory Usage:");
                        output.WriteLine("CurrentMemoryUsage " + Tools.convertMemory(DeviceStatus.ApplicationCurrentMemoryUsage));
                        output.WriteLine("ApplicationPeakMemoryUsage " + Tools.convertMemory(DeviceStatus.ApplicationPeakMemoryUsage));
                        output.WriteLine("ApplicationMemoryUsageLimit " + Tools.convertMemory(DeviceStatus.ApplicationMemoryUsageLimit));
                        output.WriteLine("DeviceTotalMemory " + Tools.convertMemory(DeviceStatus.DeviceTotalMemory));
                        output.WriteLine("\n--------------\n");

                        if (ex != null)
                        {
                            output.WriteLine("Exception ToString:\n" + ex.ToString());
                            output.WriteLine("\n--------------\n");
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void CheckForPreviousException()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        SafeDeleteFile(store);
                    }
                }
                if (contents != null)
                {
                    if (MessageBox.Show("It looks like WordPress crashed the last time you used it. You can help us resolve the issue by sending a crash report", "Crash Detected", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        EmailComposeTask email = new EmailComposeTask();
                        email.To = Constants.WORDPRESS_CRASHREPORT_EMAIL;
                        email.Subject = "WordPress for Windows Phone crash report";
                        email.Body = contents;
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
                        email.Show();
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception ex)
            {
            }
        }
    }

}
