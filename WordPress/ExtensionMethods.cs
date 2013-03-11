using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Controls;
using WordPress.Utils;
using WordPress.Localization;

namespace WordPress
{
    public static class ExtensionMethods
    {
        public static void HandleException(this PhoneApplicationPage page, Exception exception, string title = "", string message = "")
        {
            Utils.Tools.LogException(message, exception);

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, title, MessageBoxButton.OK);
            }
            else if (exception is WordPress.Model.NoConnectionException)
            {
                StringTable _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
                MessageBox.Show(_localizedStrings.Prompts.NoConnectionErrorContent, _localizedStrings.PageTitles.Error, MessageBoxButton.OK);
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
                }
                else
                {
                    if (exception is System.Net.WebException)
                    {
                        //Error while accessing the network, or the server does reply with an HTTP error code
                        MessageBox.Show(exception.Message);
                    } else {
                        //show the nice help page.
                        UIThread.Invoke(() =>
                        {
                            ErrorPage.Exception = exception;
                            (App.Current.RootVisual as Microsoft.Phone.Controls.PhoneApplicationFrame).Source = new Uri("/ErrorPage.xaml", UriKind.Relative);
                        });
                    }
                }
            }
        }
    }
}
