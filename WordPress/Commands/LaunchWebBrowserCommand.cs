using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace WordPress.Commands
{
    public class LaunchWebBrowserCommand: ICommand
    {
        //DEV NOTE: debugging with the emulator in VS seems to cause some wierd behavior with the browser--
        //the page we ask for gets put on a new tab, but that tab isn't shown...  If you run the app in the
        //emulator without VS running the browser task shows the expected page though.  
        //See thread @ http://social.msdn.microsoft.com/Forums/en-US/windowsphone7series/thread/d00bb0cc-bf8c-4a9e-9823-b55f589a3106

        #region events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region methods

        public bool CanExecute(object parameter)
        {
            string url = parameter as string;
            if (string.IsNullOrEmpty(url)) return false;

            Uri testUri;
            bool canExecute = Uri.TryCreate(url, UriKind.Absolute, out testUri);

            return canExecute;
        }

        public void Execute(object parameter)
        {
            string url = parameter as string;
            WebBrowserTask task = new WebBrowserTask();
            task.URL = url;
            task.Show();
        }

        #endregion
    }
}
