using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Threading;
using WordPress.Model;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WordPress.Localization;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace WordPress
{
    public partial class EditContent : PhoneApplicationPage
    {

        private StringTable _localizedStrings;
        private ApplicationBarIconButton _addLinkIconButton;
        private ApplicationBarIconButton _boldIconButton;
        private ApplicationBarIconButton _italicIconButton;
        private ApplicationBarIconButton _quoteIconButton;
        private ApplicationBarMenuItem _moreMenuItem;
        private ApplicationBarMenuItem _ulMenuItem;
        private ApplicationBarMenuItem _olMenuItem;
        private ApplicationBarMenuItem _underlineMenuItem;
        private ApplicationBarMenuItem _strikethroughMenuItem;
        private ApplicationBarMenuItem _discardChangesMenuItem;
        private ApplicationBarMenuItem _switchToTextModeMenuItem;

        private bool _showTextEditor = false; //true when the user taps the ShowTextMode menuitem
        private bool _hasChanges = false;

        private const string CONTENTKEY_VALUE = "content_key";
        private const string MORE_TAG_REPLACEMENT = "<div id=09-07-1979-what-a-great-date class=\"more\"></div>";
        private const string MORE_TAG_REPLACEMENT_NATIVE = "<DIV id=09-07-1979-what-a-great-date class=more></DIV>";

        public EditContent()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _addLinkIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addLinkIconButton.Text = _localizedStrings.ControlsText.Link;
            _addLinkIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_addLinkIconButton);

            _boldIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _boldIconButton.Text = _localizedStrings.ControlsText.BoldAppBarItem;
            _boldIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_boldIconButton);

            _italicIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _italicIconButton.Text = _localizedStrings.ControlsText.ItalicAppBarItem;
            _italicIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_italicIconButton);

            _quoteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _quoteIconButton.Text = _localizedStrings.ControlsText.BlockQuoteAppBarItem;
            _quoteIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_quoteIconButton);

            //Menu items
            _moreMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.MoreTag);
            _moreMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_moreMenuItem);

            _ulMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.UnorderedListMenuItem);
            _ulMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_ulMenuItem);

            _olMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.OrderedListMenuItem);
            _olMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_olMenuItem);

            _underlineMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.UnderlineMenuItem);
            _underlineMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_underlineMenuItem);

            _strikethroughMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.StrikeThroughMenuItem);
            _strikethroughMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_strikethroughMenuItem);

            _discardChangesMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.DiscardChangesMenuItem);
            _discardChangesMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_discardChangesMenuItem);

            _switchToTextModeMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.SwitchToTextModeMenuItem);
            _switchToTextModeMenuItem.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.MenuItems.Add(_switchToTextModeMenuItem);
       
            browser.Loaded += WebBrowser_OnLoaded;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_showTextEditor) //Switch to Text Mode was pressed
            {
                if (e.Content is EditPostPage)
                {
                    (e.Content as EditPostPage).showTextEditor();
                }
                else if (e.Content is EditPagePage)
                {
                    (e.Content as EditPagePage).showTextEditor();
                }
            }

            //store transient data in the State dictionary
            if (State.ContainsKey(CONTENTKEY_VALUE))
            {
                State.Remove(CONTENTKEY_VALUE);
            }
            State.Add(CONTENTKEY_VALUE, browser.SaveToString());

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //look for transient data stored in the State dictionary
            if (State.ContainsKey(CONTENTKEY_VALUE))
            {
                if (!Utils.Tools.IsWindowsPhone8orHigher)
                    browser.NavigateToString(ConvertExtendedAscii(State[CONTENTKEY_VALUE] as string));
                else
                    browser.NavigateToString(State[CONTENTKEY_VALUE] as string);
            }
        }


        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (Visibility.Visible == addLinkControl.Visibility)
            {
                HideAddLinkControl();
                e.Cancel = true;
            }
            else if (_hasChanges == false)
            {
                base.OnBackKeyPress(e);
            }
            else
            {
                string content = getPostContentFromVisualEditor();

                if (content != null)
                {
                    App.MasterViewModel.CurrentPost.Description = content;
                    base.OnBackKeyPress(e);
                }
                else
                {
                    MessageBox.Show("Sorry, can't get the content from the editor.");
                }
            }
        }

        private string getPostContentFromVisualEditor()
        {
            //rebuild the post content here
            string content = null;
            try
            {
                object result = browser.InvokeScript("getContent");
                content = result.ToString().Replace(MORE_TAG_REPLACEMENT, "<!--more-->");
                content = content.Replace("<br>", "\n");
            }
            catch (Exception err1)
            {
                Utils.Tools.LogException("Error while calling the JS function getContent", err1);
                try
                {
                    object result = browser.InvokeScript("getContentSafe");
                    content = result.ToString().Replace(MORE_TAG_REPLACEMENT_NATIVE, "<!--more-->");
                    content = content.Replace("<BR>", "\n");
                }
                catch (Exception err2)
                {
                    Utils.Tools.LogException("Error while calling the JS function getContentSafe! Really?", err2);
                }
            }
            return content;
        }


        private static string ConvertExtendedAscii(string html)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var c in html)
            {
                int charInt = Convert.ToInt32(c);
                if (charInt > 127)
                    sb.AppendFormat("&#{0};", charInt);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private void WebBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            string htmlConcat = this.ReadFile("Resources/EditContent.html");
            if (htmlConcat == null)
            {
                MessageBox.Show("Can't load the document");
            }
            else
            {

                string postContent = App.MasterViewModel.CurrentPost.Description.Replace("<!--more-->", MORE_TAG_REPLACEMENT);
                postContent = postContent.Replace("\n", "<br/>");
                
                if(!Utils.Tools.IsWindowsPhone8orHigher)
                    postContent = ConvertExtendedAscii(postContent);
                
                string content = htmlConcat.Replace("{0}", postContent);
                browser.NavigateToString(content);
            }
        }

        private void OnButtonOrMenuitemClicked(object sender, EventArgs e)
        {
            if (sender == _discardChangesMenuItem)
            {
                NavigationService.GoBack();
                return;
            }
            else if (sender == _addLinkIconButton)
            {
                try
                {
                    object selectedTextObj = browser.InvokeScript("getSelectedText");
                    string selectedTextString = selectedTextObj as string;
                    if ("" == selectedTextString)
                    {
                        MessageBox.Show(_localizedStrings.Prompts.PleaseSelectSomeText); 
                    }
                    else
                    {
                        this.Focus();
                        ApplicationBar.IsVisible = false;
                        addLinkControl.Show();
                        
                        addLinkControl.LinkText = selectedTextString;

                        if (Uri.IsWellFormedUriString(selectedTextString, UriKind.Absolute))
                        {
                            addLinkControl.Url = selectedTextString;
                        }
                    }
                }
                catch (Exception err)
                {
                    showJavaScriptError(err);
                }

                return;
            }
            else if (sender == _switchToTextModeMenuItem)
            {
                this._showTextEditor = true;

                if (_hasChanges)
                {
                    string content = getPostContentFromVisualEditor();
                    if (content != null)
                    {
                        App.MasterViewModel.CurrentPost.Description = content;
                        NavigationService.GoBack();
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Sorry, can't retrieve the content from the editor.");
                        return;
                    }
                }
                else
                {
                    NavigationService.GoBack();
                    return;
                }
            }

            _hasChanges = true;
            object[] parameters = new object[] { };
            if (sender == _boldIconButton)
            {
                parameters = new object[] { "bold" };
            }
            else if (sender == _italicIconButton)
            {
                parameters = new object[] { "italic" };
            }
            else if (sender == _quoteIconButton)
            {
                string command = _quoteIconButton.Text.StartsWith("/") ? "outdent" : "indent";
                _quoteIconButton.Text = _quoteIconButton.Text.StartsWith("/") ? _quoteIconButton.Text.TrimStart(new char[] { '/' }) : "/" + _quoteIconButton.Text;
                parameters = new object[] { command };
            }
            else if (sender == _moreMenuItem)
            {
                parameters = new object[] { "more" };
            }
            else if (sender == _ulMenuItem)
            {
                parameters = new object[] { "insertunorderedlist" };
            }
            else if (sender == _olMenuItem)
            {
                parameters = new object[] { "insertorderedlist" };
            }
            else if (sender == _underlineMenuItem)
            {
                parameters = new object[] { "underline" };
            }
            else if (sender == _strikethroughMenuItem)
            {
                parameters = new object[] { "strikethrough" };
            }


            try
            {
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void HideAddLinkControl()
        {
            addLinkControl.Hide();
            ApplicationBar.IsVisible = true;
        }

        private void OnLinkChosen(object sender, EventArgs e)
        {
            HideAddLinkControl();
            string linkMarkup = addLinkControl.CreateLinkMarkup();
            try
            {
                var parameters = new object[] { "createlink", linkMarkup};
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
            ApplicationBar.IsVisible = true;
        }

        private void showJavaScriptError(Exception error)
        {
            MessageBox.Show("Error while excuting the command.");
            Utils.Tools.LogException("Error while excuting the command.", error);
        }

        private string ReadFile(string filePath)
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri(filePath, UriKind.Relative));
            if (streamResourceInfo != null)
            {
                Stream myFileStream = streamResourceInfo.Stream;
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);

                    //read the content here
                    return myStreamReader.ReadToEnd();
                }
            }
            return null;
        }

        private void browser_ScriptNotify_1(object sender, NotifyEventArgs e)
        {
            this._hasChanges = true;
        }
    }
}