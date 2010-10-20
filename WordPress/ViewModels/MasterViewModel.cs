using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using WordPress.Model;
using System.ComponentModel;

namespace WordPress
{
    public class MasterViewModel: INotifyPropertyChanged
    {
        #region member variables

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constructors

        public MasterViewModel()
        {
            Blogs = DataStore.Instance.Blogs;
        }

        #endregion


        #region properties

        public ObservableCollection<Blog> Blogs { get; private set; }

        public Blog CurrentBlog
        {
            get { return DataStore.Instance.CurrentBlog; }
            set 
            {
                if (value != DataStore.Instance.CurrentBlog)
                {
                    DataStore.Instance.CurrentBlog = value;
                    NotifyPropertyChanged("CurrentBlog");

                    Comments.Clear();
                    Posts.Clear();
                    Pages.Clear();
                }
            }
        }

        public ObservableCollection<Comment> Comments
        {
            get
            {
                if (null == CurrentBlog)
                {
                    return null;
                }
                return CurrentBlog.Comments;
            }
        }

        public Comment CurrentComment
        {
            get { return DataStore.Instance.CurrentComment; }
            set 
            {
                if (value != DataStore.Instance.CurrentComment)
                {
                    DataStore.Instance.CurrentComment = value;
                    NotifyPropertyChanged("CurrentComment");
                }
            }
        }

        public ObservableCollection<PostListItem> Posts
        {
            get
            {
                if (null == CurrentBlog) 
                {
                    return null; 
                }
                return CurrentBlog.PostListItems;
            }
        }

        public PostListItem CurrentPost
        {
            get { return DataStore.Instance.CurrentPost; }
            set
            {
                if (value != DataStore.Instance.CurrentPost)
                {
                    DataStore.Instance.CurrentPost = value;
                    NotifyPropertyChanged("CurrentPost");
                }
            }
        }

        public ObservableCollection<PageListItem> Pages 
        {
            get
            {
                if (null == CurrentBlog)
                {
                    return null;
                }
                return CurrentBlog.PageListItems;
            }
        }

        public PageListItem CurrentPage
        {
            get { return DataStore.Instance.CurrentPage; }
            set
            {
                if (value != DataStore.Instance.CurrentPage)
                {
                    DataStore.Instance.CurrentPage = value;
                    NotifyPropertyChanged("CurrentPage");
                }
            }
        }

        #endregion

        #region methods

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
