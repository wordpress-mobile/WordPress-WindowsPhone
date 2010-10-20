using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WordPress.Model
{
    public class Page : INotifyPropertyChanged
    {
        #region member variables

        private int _id;
        private string _title;
        private string _content;
        private DateTime _postDate;
        private ObservableCollection<Comment> _comments = new ObservableCollection<Comment>();
        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region properties

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                NotifyPropertyChanged("Content");
            }
        }

        public DateTime PageDate
        {
            get { return _postDate; }
            set
            {
                _postDate = value;
                NotifyPropertyChanged("PageDate");
            }
        }

        public ObservableCollection<Comment> Comments
        {
            get { return _comments; }
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
