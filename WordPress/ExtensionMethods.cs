using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Controls;

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
                //TODO check the exception type here
                if (exception is WordPress.Model.XmlRPCParserException)
                {
                    MessageBox.Show(exception.Message);
                    /*MessageBoxResult m = MessageBox.Show(exception.Message + "\nNeed Help?", "Error", MessageBoxButton.OKCancel);
                    if(m == MessageBoxResult.OK) {
                        Debug.WriteLine("clicked ok");
                    } else {
                        Debug.WriteLine("clicked cancel");
                    }*/
                }
                else if (exception is WordPress.Model.XmlRPCException)
                {
                    MessageBox.Show(exception.Message);
                } 
                else
                {
                    MessageBox.Show(exception.Message);
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
            Debug.WriteLine(separator);
            Debug.WriteLine(string.Empty);
        }
    }
}
