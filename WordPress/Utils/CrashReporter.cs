// See http://blogs.msdn.com/b/andypennell/archive/2010/11/01/error-reporting-on-windows-phone-7.aspx
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using WordPress.Model;

namespace WordPress.Utils
{
    public class CrashReporter
    {
        const string filename = "CrashReporterLog.txt";

        internal static void ReportException(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine("Please describe what you were doing when the app crashed:\n");
                        output.WriteLine("\n\n");
                        if (extra != null)
                        {
                            output.WriteLine("Extra Info: \n");
                            output.WriteLine(extra);
                            output.WriteLine("--------------\n");
                        }
                        if (ex != null && ex.Message != null)
                        {
                            output.WriteLine("Message: \n");
                            output.WriteLine(ex.Message);
                            output.WriteLine("--------------\n");
                        }
                        if (ex != null && ex.StackTrace != null)
                        {
                            output.WriteLine("Stack Trace: \n");
                            output.WriteLine(ex.StackTrace);
                            output.WriteLine("--------------\n");
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
