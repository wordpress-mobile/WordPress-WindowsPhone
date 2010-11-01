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
                
                    NotifyPropertyChanged("Comments");
                    NotifyPropertyChanged("Posts");
                    NotifyPropertyChanged("Pages");
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

        public PostListItem CurrentPostListItem
        {
            get { return DataStore.Instance.CurrentPostListItem; }
            set
            {
                if (value != DataStore.Instance.CurrentPostListItem)
                {
                    DataStore.Instance.CurrentPostListItem = value;
                    NotifyPropertyChanged("CurrentPostListItem");
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

        public PageListItem CurrentPageListItem
        {
            get { return DataStore.Instance.CurrentPageListItem; }
            set
            {
                if (value != DataStore.Instance.CurrentPageListItem)
                {
                    DataStore.Instance.CurrentPageListItem = value;
                    NotifyPropertyChanged("CurrentPageListItem");
                }
            }
        }

        public Post CurrentPost
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
