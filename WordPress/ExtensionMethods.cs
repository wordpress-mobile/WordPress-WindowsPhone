using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Controls;
using WordPress.Utils;

namespace WordPress
{
    public static class ExtensionMethods
    {
        public static void HandleException(this PhoneApplicationPage page, Exception exception, string title = "", string message = "")
        {
            LogException(page, message, exception);

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, title, MessageBoxButton.OK);
            }
            else
            {
                if (exception is WordPress.Model.XmlRPCParserException) //cannot parse the XML-RPC response document
                {
                    UIThread.Invoke(() =>
                    {
                        ErrorPage.Exception = exception;
                        (App.Current.RootVisual as Microsoft.Phone.Controls.PhoneApplicationFrame).Source = new Uri("/ErrorPage.xaml", UriKind.Relative);
                    });
                }
                else if (exception is WordPress.Model.XmlRPCException) //the XML-RPC document contains a fault error
                {                    
                    MessageBox.Show(exception.Message);
                   // ErrorPage.Exception = exception;
                   // (App.Current.RootVisual as Microsoft.Phone.Controls.PhoneApplicationFrame).Source = new Uri("/ErrorPage.xaml", UriKind.Relative);
                } 
                else
                {
                    //MessageBox.Show(exception.Message);
                    UIThread.Invoke(() =>
                    {
                        ErrorPage.Exception = exception;
                        (App.Current.RootVisual as Microsoft.Phone.Controls.PhoneApplicationFrame).Source = new Uri("/ErrorPage.xaml", UriKind.Relative);
                    });
                }
            }
        }

        public static void LogException(this PhoneApplicationPage page, string message, Exception exception)
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
            Debug.WriteLine("Full exception:\n"+exception.ToString());
            Debug.WriteLine(separator);
            Debug.WriteLine(string.Empty);
        }
    }
}
