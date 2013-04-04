using LinqToVisualTree;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using WordPress.Localization;
using WordPress.Utils;

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

        private bool _hasChanges = false;
        private string _lastMenuConfiguration = string.Empty;

        private const string CONTENTKEY_VALUE = "content_key";
        private const string MORE_TAG_REPLACEMENT = "<div id=09-07-1979-what-a-great-date class=\"more\"></div>";
        private const string MORE_TAG_REPLACEMENT_NATIVE = "<DIV id=09-07-1979-what-a-great-date class=more></DIV>";

        public EditContent()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = false;
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _addLinkIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.link.png", UriKind.Relative));
            _addLinkIconButton.Text = _localizedStrings.ControlsText.Link;
            _addLinkIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_addLinkIconButton);

            _boldIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.bold.png", UriKind.Relative));
            _boldIconButton.Text = _localizedStrings.ControlsText.BoldAppBarItem;
            _boldIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_boldIconButton);

            _italicIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.italic.png", UriKind.Relative));
            _italicIconButton.Text = _localizedStrings.ControlsText.ItalicAppBarItem;
            _italicIconButton.Click += OnButtonOrMenuitemClicked;
            ApplicationBar.Buttons.Add(_italicIconButton);

            _quoteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.blockquote.png", UriKind.Relative));
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

            browser.Loaded += WebBrowser_OnLoaded;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_hasChanges == true)
            {
                string content = getPostContentFromVisualEditor();
                if (content != null)
                {
                    App.MasterViewModel.CurrentPost.Description = content;
                }
            }
            base.OnNavigatedFrom(e);
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
                    _hasChanges = false;
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
                content = content.Replace("\r\n", "");
                content = content.Replace("<br>", "\r");
                content = content.Replace("<br/>", "\r");
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
            App.WaitIndicationService.RootVisualElement = LayoutRoot;
         
            string htmlConcat = this.ReadFile("Resources/EditContent.html");
            if (htmlConcat == null)
            {
                MessageBox.Show("Can't load the document");
            }
            else
            {

                ApplicationBar.IsVisible = false;
                App.WaitIndicationService.ShowIndicator(_localizedStrings.Messages.Loading);

                string postContent = App.MasterViewModel.CurrentPost.Description.Replace("<!--more-->", MORE_TAG_REPLACEMENT);
                postContent = postContent.Replace("\r\n", "\n").Replace("\r", "\n");  // cross-platform newlines
                postContent = postContent.Replace("\n", "<br/>");
              
                if(!Utils.Tools.IsWindowsPhone8orHigher)
                    postContent = ConvertExtendedAscii(postContent);
                
                string content = htmlConcat.Replace("{0}", postContent);
                browser.LoadCompleted += browser_LoadCompleted;
                browser.NavigateToString(content);
            }

            var border = browser.Descendants<Border>().Last() as Border; //See: http://www.scottlogic.co.uk/blog/colin/2011/11/suppressing-zoom-and-scroll-interactions-in-the-windows-phone-7-browser-control/
            border.ManipulationDelta += Border_ManipulationDelta;
            border.ManipulationCompleted += Border_ManipulationCompleted;           
        }
        
        //Fired when the browser control has loaded the content
        void browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            browser.LoadCompleted -= browser_LoadCompleted;
            try
            {
                browser.InvokeScript("adjustImageWidth");
            }
            catch (Exception err)
            {
            }

            App.WaitIndicationService.HideIndicator();
            ApplicationBar.StateChanged += ApplicationBar_StateChanged;
            ApplicationBar.IsVisible = true;
        }

        private void OnButtonOrMenuitemClicked(object sender, EventArgs e)
        {
            if (sender == _discardChangesMenuItem)
            {
                this._hasChanges = false;
                NavigationService.GoBack();
                return;
            }

            this._hasChanges = true;

            if (sender == _addLinkIconButton)
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

            //update the menus and buttons labels
            if (e.Value != null && e.Value != "editorGotFocus" && e.Value != _lastMenuConfiguration)
            {
                System.Diagnostics.Debug.WriteLine("browser_ScriptNotify_1  ->" + e.Value);
                _lastMenuConfiguration = e.Value;
                UIThread.Invoke(() =>
               {
                    _boldIconButton.Text = e.Value.Contains("bold") ? "/" + _localizedStrings.ControlsText.BoldAppBarItem : _localizedStrings.ControlsText.BoldAppBarItem;
                    _italicIconButton.Text = e.Value.Contains("italic") ? "/" + _localizedStrings.ControlsText.ItalicAppBarItem : _localizedStrings.ControlsText.ItalicAppBarItem;
                    _quoteIconButton.Text = e.Value.Contains("indent") ? "/" + _localizedStrings.ControlsText.BlockQuoteAppBarItem : _localizedStrings.ControlsText.BlockQuoteAppBarItem;
                    _ulMenuItem.Text = e.Value.Contains("insertunorderedlist") ? "/" + _localizedStrings.ControlsText.UnorderedListMenuItem : _localizedStrings.ControlsText.UnorderedListMenuItem;
                    _olMenuItem.Text = e.Value.Contains("insertorderedlist") ? "/" + _localizedStrings.ControlsText.OrderedListMenuItem : _localizedStrings.ControlsText.OrderedListMenuItem;
                    _underlineMenuItem.Text = e.Value.Contains("underline") ? "/" + _localizedStrings.ControlsText.UnderlineMenuItem : _localizedStrings.ControlsText.UnderlineMenuItem;
                    _strikethroughMenuItem.Text = e.Value.Contains("strikethrough") ? "/" + _localizedStrings.ControlsText.StrikeThroughMenuItem : _localizedStrings.ControlsText.StrikeThroughMenuItem;
               });
            }
        }

        void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ApplicationBar_StateChanged");
            try
            {
                browser.InvokeScript("updateButtonAndMenusLabels");
            }
            catch (Exception)
            {
            }
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // suppress zoom
            if (e.FinalVelocities.ExpansionVelocity.X != 0.0 ||
                e.FinalVelocities.ExpansionVelocity.Y != 0.0)
                e.Handled = true;
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // suppress zoom
            if (e.DeltaManipulation.Scale.X != 0.0 ||
                e.DeltaManipulation.Scale.Y != 0.0)
                e.Handled = true;
        }
    }
}