using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using WordPress.Model;
using System;
using Microsoft.Phone.Controls;

namespace WordPress
{
	public partial class BlogSelectionControl : UserControl
    {
        #region member variables

        List<Blog> _blogs;

        #endregion

        #region events

        public event RoutedEventHandler BlogsSelected;

        #endregion

        #region constructors

        public BlogSelectionControl()
		{
			// Required to initialize variables
			InitializeComponent();

            Blogs = new List<Blog>();
            DataContext = Blogs;

            selectAllButton.Click += OnSelectAllButtonClick;
            selectButton.Click += OnSelectButtonClick;

            blogListBox.SelectionChanged += new SelectionChangedEventHandler(OnBlogListBoxSelectionChanged);
		}

        #endregion

        #region properties

        public List<Blog> Blogs 
        {
            get { return _blogs; }
            set
            {
                _blogs = value;
                DataContext = value;
            }
        }

        public List<Blog> SelectedItems
        {
            get
            {
                List<Blog> result = new List<Blog>();

                foreach (Blog blog in blogListBox.SelectedItems)
                {
                    result.Add(blog);
                }

                return result;
            }
        }

        #endregion

        #region methods
        
        private void NotifyBlogsSelected(RoutedEventArgs args)
        {
            if (null != BlogsSelected)
            {
                BlogsSelected(this, args);
            }
        }

        private void OnSelectAllButtonClick(object sender, RoutedEventArgs args)
        {
            foreach (Blog blog in blogListBox.Items)
            {
                if (!(blogListBox.SelectedItems.Contains(blog)))
                {
                    blogListBox.SelectedItems.Add(blog);
                }
            }

            RoutedEventArgs newArgs = new RoutedEventArgs();
            NotifyBlogsSelected(newArgs);
        }

        private void OnSelectButtonClick(object sender, RoutedEventArgs args)
        {
            RoutedEventArgs newArgs = new RoutedEventArgs();
            NotifyBlogsSelected(newArgs);            
        }

        private void OnBlogListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // when all items are unselected the selection mode automatically turns off
            if (blogListBox.SelectedItems.Count == 0)
                blogListBox.IsSelectionEnabled = true;

            bool isEnabled = false;

            if (0 < SelectedItems.Count)
            {
                isEnabled = true;
            }

            selectButton.IsEnabled = isEnabled;
        }

        private void BlogListItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Object element = ((FrameworkElement)sender).DataContext;
            MultiselectItem container = blogListBox.ItemContainerGenerator.ContainerFromItem(element) as MultiselectItem;
            if (null != container)
            {
                container.IsSelected = !container.IsSelected;
            }
        }
        #endregion
    }
}