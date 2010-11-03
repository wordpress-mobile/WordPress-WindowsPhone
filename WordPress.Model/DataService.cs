using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;

namespace WordPress.Model
{
    public class DataService: IApplicationService, IApplicationLifetimeAware
    {
        #region member variables

        private Blog _currentBlog;

        private const string BLOGS_FILENAME = "blogs.dat";
        private const string STORE_FILENAME = "store.dat";

        #endregion

        #region events

        public event EventHandler FetchComplete;
        public event ExceptionEventHandler ExceptionOccurred;

        #endregion

        #region constructor

        public DataService()
        {
            Blogs = new ObservableCollection<Blog>();
        }

        #endregion

        #region properties

        public static DataService Current { get; private set; }

        public Blog CurrentBlog
        {
            get
            {
                return _currentBlog;
            }
            set
            {
                _currentBlog = value;
            }
        }

        public ObservableCollection<Blog> Blogs { get; private set; }

        public Comment CurrentComment { get; set; }

        public PostListItem CurrentPostListItem { get; set; }

        public PageListItem CurrentPageListItem { get; set; }

        public Post CurrentPost { get; set; }

        #endregion

        #region methods

        public void ClearStorage()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(BLOGS_FILENAME))
                {
                    isoStore.DeleteFile(BLOGS_FILENAME);
                } 
            }

            CurrentBlog = null;
            CurrentComment = null;
            CurrentPageListItem = null;
            CurrentPostListItem = null;
        }

        public void Serialize()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                SerializeBlogs(isoStore);
                SerializeStoreData(isoStore);
            }
        }

        private void SerializeBlogs(IsolatedStorageFile isoStore)
        {
            if (isoStore.FileExists(BLOGS_FILENAME))
            {
                isoStore.DeleteFile(BLOGS_FILENAME);
            }

            using (IsolatedStorageFileStream isoStream = isoStore.CreateFile(BLOGS_FILENAME))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Blog>));
                serializer.Serialize(isoStream, Blogs);
            }
        }

        private void SerializeStoreData(IsolatedStorageFile isoStore)
        {
            if (isoStore.FileExists(STORE_FILENAME))
            {
                isoStore.DeleteFile(STORE_FILENAME);
            }

            using (IsolatedStorageFileStream isoStream = isoStore.CreateFile(STORE_FILENAME))
            {
                StoreData data = new StoreData();
                if (null != CurrentBlog)
                {
                    data.BlogId = CurrentBlog.BlogId;
                }
                else
                {
                    data.BlogId = -1;
                }
                data.Comment = CurrentComment;
                data.PostListItem = CurrentPostListItem;
                data.PageListItem = CurrentPageListItem;
                data.Post = CurrentPost;

                XmlSerializer serializer = new XmlSerializer(typeof(StoreData));
                serializer.Serialize(isoStream, data);
            }
        }

        public void Initialize()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                DeserializeBlogs(isoStore);
                DeserializeStoreData(isoStore);
            }
        }

        private void DeserializeBlogs(IsolatedStorageFile isoStore)
        {
            if (!isoStore.FileExists(BLOGS_FILENAME)) return;

            using (IsolatedStorageFileStream isoStream = isoStore.OpenFile(BLOGS_FILENAME, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Blog>));
                var result = serializer.Deserialize(isoStream);
                if (null != result && result is ObservableCollection<Blog>)
                {
                    Blogs = result as ObservableCollection<Blog>;
                }
            }
        }

        private void DeserializeStoreData(IsolatedStorageFile isoStore)
        {
            if (!isoStore.FileExists(STORE_FILENAME)) return;

            using (IsolatedStorageFileStream isoStream = isoStore.OpenFile(STORE_FILENAME, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StoreData));
                StoreData result = serializer.Deserialize(isoStream) as StoreData;

                if (-1 != result.BlogId)
                {
                    if (null != Blogs && 0 < Blogs.Count)
                    {
                        CurrentBlog = Blogs.Single(b => b.BlogId == result.BlogId);
                    }
                }
                CurrentComment = result.Comment;
                CurrentPostListItem = result.PostListItem;
                CurrentPageListItem = result.PageListItem;
                CurrentPost = result.Post;
            }
        }

        private void NotifyFetchComplete()
        {
            if (null != FetchComplete)
            {
                FetchComplete(this, EventArgs.Empty);
            }
        }

        private void NotifyExceptionOccurred(ExceptionEventArgs args)
        {
            if (null != ExceptionOccurred)
            {
                ExceptionOccurred(this, args);
            }
        }

        public void FetchCurrentBlogCommentsAsync()
        {
            if (null == CurrentBlog)
            {
                throw new ArgumentException("CurrentBlog may not be null", "CurrentBlog");
            }

            GetAllCommentsRPC rpc = new GetAllCommentsRPC(CurrentBlog);
            rpc.Number = 30;
            rpc.Offset = 0;
            rpc.Completed += OnFetchCurrentBlogCommentsCompleted;

            rpc.ExecuteAsync();
        }

        private void OnFetchCurrentBlogCommentsCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            GetAllCommentsRPC rpc = sender as GetAllCommentsRPC;
            rpc.Completed -= OnFetchCurrentBlogCommentsCompleted;

            CurrentBlog.Comments.Clear();

            if (null == args.Error)
            {
                foreach (Comment comment in args.Items)
                {
                    CurrentBlog.Comments.Add(comment);
                }
                NotifyFetchComplete();
            }
            else
            {
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
        }

        public void FetchCurrentBlogPostsAsync()
        {
            if (null == CurrentBlog)
            {
                throw new ArgumentException("CurrentBlog may not be null", "CurrentBlog");
            }

            GetRecentPostsRPC rpc = new GetRecentPostsRPC(CurrentBlog);
            rpc.NumberOfPosts = 30;
            rpc.Completed += OnFetchCurrentBlogPostsCompleted;

            rpc.ExecuteAsync();
        }

        private void OnFetchCurrentBlogPostsCompleted(object sender, XMLRPCCompletedEventArgs<PostListItem> args)
        {
            GetRecentPostsRPC rpc = sender as GetRecentPostsRPC;
            rpc.Completed -= OnFetchCurrentBlogPostsCompleted;

            CurrentBlog.PostListItems.Clear();

            if (null == args.Error)
            {
                foreach (PostListItem item in args.Items)
                {
                    CurrentBlog.PostListItems.Add(item);
                }

                NotifyFetchComplete();
            }
            else
            {
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
        }

        public void FetchCurrentBlogPagesAsync()
        {
            if (null == CurrentBlog)
            {
                throw new ArgumentException("CurrentBlog may not be null", "CurrentBlog");
            }

            GetPageListRPC rpc = new GetPageListRPC(CurrentBlog);
            rpc.Completed += OnFetchCurrentBlogPagesCompleted;

            rpc.ExecuteAsync();
        }

        private void OnFetchCurrentBlogPagesCompleted(object sender, XMLRPCCompletedEventArgs<PageListItem> args)
        {
            GetPageListRPC rpc = sender as GetPageListRPC;
            rpc.Completed -= OnFetchCurrentBlogPagesCompleted;

            CurrentBlog.PageListItems.Clear();

            if (null == args.Error)
            {
                foreach (PageListItem item in args.Items)
                {
                    CurrentBlog.PageListItems.Add(item);
                }
                NotifyFetchComplete();
            }
            else
            {
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
        }

        public void FetchCurrentBlogCategories()
        {
            if (null == CurrentBlog)
            {
                throw new ArgumentException("CurrentBlog may not be null", "CurrentBlog");
            }

            GetCategoriesRPC rpc = new GetCategoriesRPC(CurrentBlog);
            rpc.Completed += OnGetCategoriesRPCCompleted;
            rpc.ExecuteAsync();
        }

        private void OnGetCategoriesRPCCompleted(object sender, XMLRPCCompletedEventArgs<Category> args)
        {
            GetCategoriesRPC rpc = sender as GetCategoriesRPC;
            rpc.Completed -= OnGetCategoriesRPCCompleted;

            if (null == args.Error)
            {
                CurrentBlog.Categories.Clear();
                args.Items.ForEach(category =>
                {
                    CurrentBlog.Categories.Add(category);
                });
                NotifyFetchComplete();
            }
            else
            {
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
        }

        public void StartService(ApplicationServiceContext context)
        {
            Current = this;
        }

        public void StopService()
        {
            Current = null;
        }

        public void Exited()
        {

        }

        public void Exiting()
        {
            Serialize();
        }

        public void Started()
        {

        }

        public void Starting()
        {
            Initialize();
        }

        #endregion

        #region StoreData class definition

        public class StoreData
        {
            //DEV NOTE: using a simple momento pattern here to capture the DataStore's state--
            //serializing a singleton won't work with the reflection constraints
            public int BlogId { get; set; }
            public Comment Comment { get; set; }
            public PostListItem PostListItem { get; set; }
            public PageListItem PageListItem { get; set; }
            public Post Post { get; set; }
        }

        #endregion
    }
}
