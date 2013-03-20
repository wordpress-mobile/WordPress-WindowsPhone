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

        public EditContent()
        {
            InitializeComponent();

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)App.Current.Resources["AppbarBackgroundColor"];
            ApplicationBar.ForegroundColor = (Color)App.Current.Resources["WordPressGrey"];

            _addLinkIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            _addLinkIconButton.Text = _localizedStrings.ControlsText.Link;
            _addLinkIconButton.Click += OnLinkButtonClick;
            ApplicationBar.Buttons.Add(_addLinkIconButton);

            _boldIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _boldIconButton.Text = "Bold";
            _boldIconButton.Click += OnBoldClicked;
            ApplicationBar.Buttons.Add(_boldIconButton);

            _italicIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _italicIconButton.Text = "Italic";
            _italicIconButton.Click += OnItalicClicked;
            ApplicationBar.Buttons.Add(_italicIconButton);

            _quoteIconButton = new ApplicationBarIconButton(new Uri("/Images/appbar.settings.png", UriKind.Relative));
            _quoteIconButton.Text = "BlockQuote";
            _quoteIconButton.Click += OnQuoteClicked;
            ApplicationBar.Buttons.Add(_quoteIconButton);

            //Menu items
            _moreMenuItem = new ApplicationBarMenuItem(_localizedStrings.ControlsText.MoreTag);
            _moreMenuItem.Click += MoreMenuItem_Click;
            ApplicationBar.MenuItems.Add(_moreMenuItem);

            _ulMenuItem = new ApplicationBarMenuItem("Unordered List");
            _ulMenuItem.Click += UnorderedListMenuItem_Click;
            ApplicationBar.MenuItems.Add(_ulMenuItem);

            _olMenuItem = new ApplicationBarMenuItem("Ordered List");
            _olMenuItem.Click += OrderedListMenuItem_Click;
            ApplicationBar.MenuItems.Add(_olMenuItem);

            _underlineMenuItem = new ApplicationBarMenuItem("Underline");
            _underlineMenuItem.Click += UnderlineMenuItem_Click;
            ApplicationBar.MenuItems.Add(_underlineMenuItem);
            
            _strikethroughMenuItem = new ApplicationBarMenuItem("Strike Through");
            _strikethroughMenuItem.Click += StrikeThroughMenuItem_Click;
            ApplicationBar.MenuItems.Add(_strikethroughMenuItem);

            _discardChangesMenuItem = new ApplicationBarMenuItem("Discard Changes");
            _discardChangesMenuItem.Click += DiscardChangesMenuItem_Click;
            ApplicationBar.MenuItems.Add(_discardChangesMenuItem);

            browser.Loaded += WebBrowser_OnLoaded;
        }

        private void DiscardChangesMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (Visibility.Visible == addLinkControl.Visibility)
            {
                HideAddLinkControl();
                e.Cancel = true;
            }
            else
            {
                //rebuild the post content here
                try
                {
                    object result = browser.InvokeScript("getContent");
                    string content = result.ToString().Replace("<div class=\"more\"></div><br>", "");
                    App.MasterViewModel.CurrentPost.Description = content;
                    base.OnBackKeyPress(e);
                }
                catch (Exception)
                {
                    MessageBox.Show("Sorry, can't retrieve the content from the editor.");
                }
            }
        }

        private string FetchBackgroundColor()
        {
            string color;
            Color mc = (Color)Application.Current.Resources["TextBoxBackgroundColor"];
            color = mc.ToString();
            return color;
        }

        private string FetchFontColor()
        {
            string color;
            Color mc = (Color)Application.Current.Resources["TextBoxForegroundColor"];
            color = mc.ToString();
            return color;
        }

        private void SetBackground()
        {
            Color mc = (Color)Application.Current.Resources["TextBoxBackgroundColor"];
            browser.Background = new SolidColorBrush(mc);
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
                string postContent = App.MasterViewModel.CurrentPost.Description.Replace("<!--more-->\n","<!--more--><div class=\"more\"></div><br/>");
                postContent = postContent.Replace("\n", "<br/>");
                string content = htmlConcat.Replace("{0}", postContent);
                browser.NavigateToString(content);
            }
        }

        private void OnBoldClicked(object sender, EventArgs e)
        {
            try
            {
                _boldIconButton.Text = _boldIconButton.Text.StartsWith("/") ? _boldIconButton.Text.TrimStart(new char[] { '/' }) : "/" + _boldIconButton.Text;
                var parameters = new object[] { "bold" };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void OnItalicClicked(object sender, EventArgs e)
        {
            try
            {
                _italicIconButton.Text = _italicIconButton.Text.StartsWith("/") ? _italicIconButton.Text.TrimStart(new char[] { '/' }) : "/" + _italicIconButton.Text;
                var parameters = new object[] { "italic" };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void OnQuoteClicked(object sender, EventArgs e)
        {
            try
            {
                string command = _quoteIconButton.Text.StartsWith("/") ? "outdent" : "indent";
                _quoteIconButton.Text = _quoteIconButton.Text.StartsWith("/") ? _quoteIconButton.Text.TrimStart(new char[] { '/' }) : "/" + _quoteIconButton.Text;
                var parameters = new object[] { command };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void MoreMenuItem_Click(object sender, EventArgs e)
        {
            var parameters = new object[] { "more" };
            browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
        }

        private void UnorderedListMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _ulMenuItem.Text = _ulMenuItem.Text.StartsWith("/") ? _ulMenuItem.Text.TrimStart(new char[] { '/' }) : "/" + _ulMenuItem.Text;
                var parameters = new object[] { "insertunorderedlist" };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void OrderedListMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _olMenuItem.Text = _olMenuItem.Text.StartsWith("/") ? _olMenuItem.Text.TrimStart(new char[] { '/' }) : "/" + _olMenuItem.Text;
                var parameters = new object[] { "insertorderedlist" };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void UnderlineMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _underlineMenuItem.Text = _underlineMenuItem.Text.StartsWith("/") ? _underlineMenuItem.Text.TrimStart(new char[] { '/' }) : "/" + _underlineMenuItem.Text;
                var parameters = new object[] { "underline" };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void CodeMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void StrikeThroughMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _strikethroughMenuItem.Text = _strikethroughMenuItem.Text.StartsWith("/") ? _strikethroughMenuItem.Text.TrimStart(new char[] { '/' }) : "/" + _strikethroughMenuItem.Text;
                var parameters = new object[] { "strikethrough" };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void OnLinkButtonClick(object sender, EventArgs e)
        {
            this.Focus();
            ApplicationBar.IsVisible = false;
            addLinkControl.Show();
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
                var parameters = new object[] { "createlink", linkMarkup };
                browser.InvokeScript("formatBtnClick", parameters.Select(c => c.ToString()).ToArray());
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
            ApplicationBar.IsVisible = true;
        }

        private void browser_GotFocus_1(object sender, RoutedEventArgs e)
        {
            try
            {
                browser.InvokeScript("onGotFocus");
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
        }

        private void browser_LostFocus_1(object sender, RoutedEventArgs e)
        {
            try
            {
                browser.InvokeScript("onLostFocus");
            }
            catch (Exception err)
            {
                showJavaScriptError(err);
            }
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

        }
    }
}