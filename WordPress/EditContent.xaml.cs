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

namespace WordPress
{
    public partial class EditContent : PhoneApplicationPage
    {
        public EditContent()
        {
            InitializeComponent();
            Post post = App.MasterViewModel.CurrentPost;
            DataContext = post;
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
                base.OnBackKeyPress(e);
            }
        }

        private void OnLinkButtonClick(object sender, RoutedEventArgs e)
        {
            ShowLinkControl();
        }

        private void ShowLinkControl()
        {
            addLinkControl.Show();

            // if content text is selected, pre-populate the dialog's fields
            if (contentTextBox.SelectionLength > 0)
            {
                addLinkControl.LinkText = contentTextBox.SelectedText;

                if (Uri.IsWellFormedUriString(contentTextBox.SelectedText, UriKind.Absolute))
                {
                    addLinkControl.Url = contentTextBox.SelectedText;
                }
            }
        }

        private void HideAddLinkControl()
        {
            addLinkControl.Hide();
        }

        private void OnLinkChosen(object sender, EventArgs e)
        {
            HideAddLinkControl();
            string linkMarkup = addLinkControl.CreateLinkMarkup();
            contentTextBox.SelectedText = linkMarkup;
            contentTextBox.Focus();
        }

        private void OnMoreButtonClick(object sender, RoutedEventArgs e)
        {
            var insertText = "<!--more-->";
            var selectionIndex = contentTextBox.SelectionStart;
            contentTextBox.Text = contentTextBox.Text.Insert(selectionIndex, insertText);
            contentTextBox.SelectionStart = selectionIndex + insertText.Length;
        }
        
        private void OnBoldToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(boldToggleButton, WordPressMarkupTags.BOLD_OPENING_TAG, WordPressMarkupTags.BOLD_CLOSING_TAG);
        }

        private void OnItalicToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(italicToggleButton, WordPressMarkupTags.ITALICS_OPENING_TAG, WordPressMarkupTags.ITALICS_CLOSING_TAG);
        }

        private void OnUnderlineToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(underlineToggleButton, WordPressMarkupTags.UNDERLINE_OPENING_TAG, WordPressMarkupTags.UNDERLINE_CLOSING_TAG);
        }

        private void OnStrikethroughToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(strikethroughToggleButton, WordPressMarkupTags.STRIKETHROUGH_OPENING_TAG, WordPressMarkupTags.STRIKETHROUGH_CLOSING_TAG);
        }

        private void OnBlockquoteToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(blockquoteToggleButton, WordPressMarkupTags.BLOCKQUOTE_OPENING_TAG, WordPressMarkupTags.BLOCKQUOTE_CLOSING_TAG);
        }

        private void OnUnorderedListToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(ulToggleButton, WordPressMarkupTags.UL_OPENING_TAG, WordPressMarkupTags.UL_CLOSING_TAG);
        }

        private void OrderedListToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(olToggleButton, WordPressMarkupTags.OL_OPENING_TAG, WordPressMarkupTags.OL_CLOSING_TAG);
        }

        private void ListItemToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(liToggleButton, WordPressMarkupTags.LI_OPENING_TAG, WordPressMarkupTags.LI_CLOSING_TAG);
        }

        private void CodeToggleButtonClick(object sender, RoutedEventArgs e)
        {
            InsertMarkupTagIntoContent(codeToggleButton, WordPressMarkupTags.CODE_OPENING_TAG, WordPressMarkupTags.CODE_CLOSING_TAG);
        }

        private void InsertMarkupTagIntoContent(ToggleButton toggleButton, string openingTag, string closingTag)
        {
            Post post = DataContext as Post;
            string description = contentTextBox.Text;

            int startIndex = contentTextBox.SelectionStart;
            if (description.Length <= startIndex)
            {
                startIndex = description.Length;
            }

            string tag;
            int selectionLength = contentTextBox.SelectionLength;
            if (selectionLength > 0)
            {
                tag = openingTag;

                description = description.Insert(startIndex, openingTag);
                description = description.Insert(startIndex + openingTag.Length + selectionLength, closingTag);

                // cancel toggle switch
                toggleButton.IsChecked = !toggleButton.IsChecked.Value;
            }
            else
            {
                if (toggleButton.IsChecked.Value)
                {
                    tag = openingTag;
                }
                else
                {
                    tag = closingTag;
                }

                description = description.Insert(startIndex, tag);
            }

            post.Description = description;

            ThreadPool.QueueUserWorkItem((state) =>
            {
                //yield long enough for the button to take focus away from the text box,
                //then reset focus to the text box
                Thread.Sleep(200);
                Dispatcher.BeginInvoke(() =>
                {
                    contentTextBox.Focus();
                    contentTextBox.SelectionStart = startIndex + tag.Length;
                    contentTextBox.SelectionLength = selectionLength;
                });
            });
        }
    }
}