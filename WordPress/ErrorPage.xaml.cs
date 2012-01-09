using System;
using System.Diagnostics;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone;
using Microsoft.Phone.Tasks;

using WordPress.Commands;
using WordPress.Localization;
using WordPress.Model;
using System.Windows;


namespace WordPress
{

    public partial class ErrorPage : PhoneApplicationPage
    {
        public static Exception Exception;

        private StringTable _localizedStrings;

        private const string ERROR_DESC_VALUE = "errDesc";
        private const string FULL_STACK_VALUE = "fullStackDesc";
        private string fullStackTrace = ""; //used to store the page state

        #region constructors
        public ErrorPage()
        {
            InitializeComponent();
            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
            this.DebugLog("end constructor");
        }
        #endregion

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // base.OnNavigatedTo(e);
            this.DebugLog("OnNavigatedTo");
            if (Exception != null)
            {
                if (Exception is WordPress.Model.XmlRPCParserException)
                {
                    //translate the exception message here
                    WordPress.Model.XmlRPCParserException exp = Exception as WordPress.Model.XmlRPCParserException;
                   
                    if (exp.FaultCode == XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE)
                        ErrorText.Text = _localizedStrings.Prompts.ServerReturnedInvalidXmlRpcMessage;
                    else if (exp.FaultCode == XmlRPCResponseConstants.XML_RPC_OPERATION_FAILED_CODE)
                        ErrorText.Text = _localizedStrings.Prompts.XmlRpcOperationFailed;
                    else if (exp.FaultCode == XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE)
                        ErrorText.Text = _localizedStrings.Prompts.XeElementMissing;
                    else
                        ErrorText.Text = Exception.Message;
                }
                else
                    ErrorText.Text = Exception.Message;

                fullStackTrace = Exception.ToString();
            }
            else //user re-activaed the app
            {
                //look for transient data stored in the State dictionary
                if (State.ContainsKey(ERROR_DESC_VALUE))
                {
                    ErrorText.Text = State[ERROR_DESC_VALUE] as string;
                }

                if (State.ContainsKey(FULL_STACK_VALUE))
                {
                    fullStackTrace = State[FULL_STACK_VALUE] as string;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.DebugLog("OnNavigatedFrom");
            base.OnNavigatedFrom(e);
            //store transient data in the State dictionary
            if (State.ContainsKey(ERROR_DESC_VALUE))
            {
                State.Remove(ERROR_DESC_VALUE);
            }
            State.Add(ERROR_DESC_VALUE, ErrorText.Text);

            if (State.ContainsKey(FULL_STACK_VALUE))
            {
                State.Remove(FULL_STACK_VALUE);
            }
            State.Add(FULL_STACK_VALUE, fullStackTrace);
        }

        private void FAQButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_FAQ_URL);
        }

        private void forumButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LaunchWebBrowserCommand command = new LaunchWebBrowserCommand();
            command.Execute(Constants.WORDPRESS_FORUMS_URL);
        }

        private void copyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(fullStackTrace);
            MessageBoxResult result = MessageBox.Show(_localizedStrings.Messages.ContentCopied, _localizedStrings.Messages.Info , MessageBoxButton.OK);
        }
    }
}