using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using WordPress.Model;

namespace WordPress
{
    public partial class TestPage : PhoneApplicationPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            //GetAllCommentsRPC rpc = new GetAllCommentsRPC();
            //rpc.Url = "https://isstestwp.wordpress.com/xmlrpc.php";
            //rpc.BlogId = 15979233;
            //rpc.Username = "isstestwp";
            //rpc.Password = "pass@word1";
            ////rpc.PostId = 1;
            ////rpc.CommentStatus = "approve";
            //rpc.Offset = 0;
            //rpc.Number = 20;
            //rpc.Succeeded = OnGetCommentsRPCSucceeded;
            //rpc.Failed = OnRPCFailed;
            //rpc.Execute();

            //GetRecentPostsRPC rpc = new GetRecentPostsRPC();
            //rpc.Url = "https://isstestwp.wordpress.com/xmlrpc.php";
            //rpc.BlogId = 15979233;
            //rpc.Username = "isstestwp";
            //rpc.Password = "pass@word1";
            //rpc.NumberOfPosts = 30;
            //rpc.Succeeded = OnGetRecentPostsSucceeded;
            //rpc.Failed = OnRPCFailed;
            //rpc.Execute();

            GetPageListRPC rpc = new GetPageListRPC();
            rpc.Url = "https://isstestwp.wordpress.com/xmlrpc.php";
            //rpc.BlogId = 15979233;
            //rpc.Username = "isstestwp";
            //rpc.Password = "pass@word1";
            //rpc.Failed = OnRPCFailed;
            //rpc.Succeeded = OnGetPageListSucceeded;
            //rpc.Execute();
        }

        private void OnGetPageListSucceeded(object sender, XmlRPCEventArgs<PageListItem> args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(args.Items.Count.ToString(), "Page count", MessageBoxButton.OK);
            });
        }

        private void OnGetRecentPostsSucceeded(object sender, XmlRPCEventArgs<PostListItem> args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(args.Items.Count.ToString(), "Post count", MessageBoxButton.OK);
            });
        }

        private void OnGetCommentsRPCSucceeded(object sender, XmlRPCEventArgs<Comment> args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(args.Items.Count.ToString(), "Comment Count", MessageBoxButton.OK);
            });
        }

        private void OnRPCFailed(object sender, Exception ex)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
            });
        }
    }
}