using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;

namespace WordPress.Model
{
    public class DataService : IApplicationService, IApplicationLifetimeAware
    {
        #region member variables

        private Blog _currentBlog;

        /// <summary>
        /// This collection is used to hold new blogs that we're downloading data for.
        /// If one of the new blogs becomes the current blog we can query this collection
        /// to avoid kicking off unnecessary downloads.  Once the download is complete
        /// the blog will be removed from this collection.
        /// </summary>
        private List<Blog> _trackedBlogs;

        private const string BLOGS_FILENAME = "blogs.dat";
        private const string STORE_FILENAME = "store.dat";

        /// <summary>
        /// The number of *items* to download per web call.
        /// </summary>
        private const int CHUNK_SIZE = 30;

        #endregion

        #region events

        public event EventHandler FetchComplete;
        public event ExceptionEventHandler ExceptionOccurred;

        #endregion

        #region constructor

        public DataService()
        {
            Blogs = new ObservableCollection<Blog>();
            _trackedBlogs = new List<Blog>();
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

            //we're already downloading data here--don't allow scenarios where we could be
            //kicking off another download
            if (_trackedBlogs.Contains(CurrentBlog)) return;

            GetAllCommentsRPC rpc = new GetAllCommentsRPC(CurrentBlog);
            rpc.Number = CHUNK_SIZE;
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

            //we're already downloading data here--don't allow scenarios where we could be
            //kicking off another download
            if (_trackedBlogs.Contains(CurrentBlog)) return;

            GetRecentPostsRPC rpc = new GetRecentPostsRPC(CurrentBlog);
            rpc.NumberOfPosts = CHUNK_SIZE;
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

            //we're already downloading data here--don't allow scenarios where we could be
            //kicking off another download
            if (_trackedBlogs.Contains(CurrentBlog)) return;

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

        public void AddBlogToStore(Blog newBlog)
        {
            if (!(Blogs.Contains(newBlog)))
            {
                Blogs.Add(newBlog);
            }

            if (string.IsNullOrEmpty(newBlog.ApiKey))
            {
                newBlog.IsLoadingContent = true;
                GetApiKeyRPC rpc = new GetApiKeyRPC(newBlog);
                rpc.Completed += OnGetApiKeyRPCCompleted;
                rpc.ExecuteAsync();
            }
        }

        private void OnGetApiKeyRPCCompleted(object sender, XMLRPCCompletedEventArgs<Blog> args)
        {
            //the blog is updated by the rpc.  all we have to do here is unbind
            GetApiKeyRPC rpc = sender as GetApiKeyRPC;
            rpc.Completed -= OnGetApiKeyRPCCompleted;

            //now that we have our new blog we need to download its contents.  
            Blog newBlog = args.Items[0];
            _trackedBlogs.Add(newBlog);

            this.DebugLog("Blog '" + newBlog.BlogName + "' is now downloading data.");

            //start with the comments
            GetAllCommentsRPC getCommentsRPC = new GetAllCommentsRPC(newBlog);
            getCommentsRPC.Number = CHUNK_SIZE;
            getCommentsRPC.Offset = 0;
            getCommentsRPC.Completed += OnGetNewBlogCommentsCompleted;
            getCommentsRPC.ProgressChanged += OnGetCommentsRPCProgressChanged;

            getCommentsRPC.ExecuteAsync();
        }

        private void OnGetCommentsRPCProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            GetAllCommentsRPC rpc = sender as GetAllCommentsRPC;
            Blog newBlog = _trackedBlogs.Where(blog => blog.BlogId == rpc.BlogId).FirstOrDefault();
            if (null != newBlog)
            {
                this.DebugLog("OnGetCommentsRPCProgressChanged-- Blog: " + newBlog.BlogName);
            }
            this.DebugLog("GetCommentsProgressChanged-- Progress: " + args.ProgressPercentage.ToString());
            this.DebugLog("GetCommentsProgressChanged-- UserState: " + args.UserState);
        }

        private void OnGetNewBlogCommentsCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            GetAllCommentsRPC rpc = sender as GetAllCommentsRPC;
            rpc.Completed -= OnGetNewBlogCommentsCompleted;
            rpc.ProgressChanged -= OnGetCommentsRPCProgressChanged;

            Blog newBlog = _trackedBlogs.Where(blog => blog.BlogId == rpc.BlogId).FirstOrDefault();
            if (null == newBlog) return;

            //report the error, but keep trying to get data
            if (null != args.Error)
            {
                this.DebugLog("OnFetchNewBlogCommentsCompleted: Exception occurred (" + newBlog.BlogName + ")");
                this.DebugLog(args.Error.ToString());
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
            else
            {
                foreach (Comment comment in args.Items)
                {
                    newBlog.Comments.Add(comment);
                }
                this.DebugLog("Blog '" + newBlog.BlogName + "' has finished downloading comments.");
            }

            this.DebugLog("Blog '" + newBlog.BlogName + "' has finished downloading comments.");

            if (newBlog == CurrentBlog)
            {
                NotifyFetchComplete();
            }

            //get the posts for the new blog
            GetRecentPostsRPC recentPostsRPC = new GetRecentPostsRPC(newBlog);
            recentPostsRPC.NumberOfPosts = CHUNK_SIZE;
            recentPostsRPC.Completed += OnGetNewBlogRecentPostsCompleted;
            recentPostsRPC.ProgressChanged += OnGetNewBlogRecentPostsRPCProgressChanged;
            recentPostsRPC.ExecuteAsync();
        }

        private void OnGetNewBlogRecentPostsRPCProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            GetRecentPostsRPC rpc = sender as GetRecentPostsRPC;
            Blog newBlog = _trackedBlogs.Where(blog => blog.BlogId == rpc.BlogId).FirstOrDefault();
            if (null != newBlog)
            {
                this.DebugLog("OnGetNewBlogRecentPostsRPCProgressChanged-- Blog: " + newBlog.BlogName);
            }
            this.DebugLog("OnGetNewBlogRecentPostsRPCProgressChanged-- Progress: " + args.ProgressPercentage.ToString());
            this.DebugLog("OnGetNewBlogRecentPostsRPCProgressChanged-- UserState: " + args.UserState);
        }

        private void OnGetNewBlogRecentPostsCompleted(object sender, XMLRPCCompletedEventArgs<PostListItem> args)
        {
            GetRecentPostsRPC rpc = sender as GetRecentPostsRPC;
            rpc.Completed -= OnGetNewBlogRecentPostsCompleted;
            rpc.ProgressChanged -= OnGetNewBlogRecentPostsRPCProgressChanged;

            Blog newBlog = _trackedBlogs.Where(blog => blog.BlogId == rpc.BlogId).FirstOrDefault();
            if (null == newBlog) return;

            //report the error, but keep trying to get data
            if (null != args.Error)
            {
                this.DebugLog("OnGetNewBlogRecentPostsCompleted: Exception occurred (" + newBlog.BlogName + ")");
                this.DebugLog(args.Error.ToString());
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
            else
            {
                foreach (PostListItem item in args.Items)
                {
                    newBlog.PostListItems.Add(item);
                }
            }

            this.DebugLog("Blog '" + newBlog.BlogName + "' has finished downloading posts.");

            if (newBlog == CurrentBlog)
            {
                NotifyFetchComplete();
            }

            //get the pages for the new blog
            GetPageListRPC pageListRPC = new GetPageListRPC(newBlog);
            pageListRPC.Completed += OnGetNewBlogPagesCompleted;
            pageListRPC.ProgressChanged += OnGetNewBlogPagesProgressChanged;
            pageListRPC.ExecuteAsync();
        }

        private void OnGetNewBlogPagesProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            GetPageListRPC rpc = sender as GetPageListRPC;
            Blog newBlog = _trackedBlogs.Where(blog => blog.BlogId == rpc.BlogId).FirstOrDefault();
            if (null != newBlog)
            {
                this.DebugLog("OnGetNewBlogPagesProgressChanged-- Blog: " + newBlog.BlogName);
            }
            this.DebugLog("OnGetNewBlogPagesProgressChanged-- Progress: " + args.ProgressPercentage.ToString());
            this.DebugLog("OnGetNewBlogPagesProgressChanged-- UserState: " + args.UserState);
        }

        private void OnGetNewBlogPagesCompleted(object sender, XMLRPCCompletedEventArgs<PageListItem> args)
        {
            GetPageListRPC rpc = sender as GetPageListRPC;
            rpc.Completed -= OnGetNewBlogPagesCompleted;
            rpc.ProgressChanged -= OnGetNewBlogPagesProgressChanged;

            Blog newBlog = _trackedBlogs.Where(blog => blog.BlogId == rpc.BlogId).FirstOrDefault();
            if (null == newBlog) return;

            if (null != args.Error)
            {
                this.DebugLog("OnGetNewBlogPagesCompleted: Exception occurred (" + newBlog.BlogName + ")");
                this.DebugLog(args.Error.ToString());
                NotifyExceptionOccurred(new ExceptionEventArgs(args.Error));
            }
            else
            {
                foreach (PageListItem item in args.Items)
                {
                    newBlog.PageListItems.Add(item);
                }
            }

            _trackedBlogs.Remove(newBlog);

            newBlog.IsLoadingContent = false;

            if (newBlog == CurrentBlog)
            {
                NotifyFetchComplete();
            }
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
