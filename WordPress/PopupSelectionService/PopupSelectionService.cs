using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace WordPress
{
    /// <summary>
    /// The PopupSelectionService creates a popup window with a list of options for the user to 
    /// choose from.  The window is dismissed when the user makes a selection or when the 
    /// back hardware key is pushed (note that the latter requires the page to override the
    /// OnBackKeyPress).
    /// </summary>
    public class PopupSelectionService : IPopupSelectionService
    {
        #region member variables

        Popup _popup;
        private Grid _layoutRoot;

        private const int DEFAULT_HEIGHT = 800;
        private const int DEFAULT_WIDTH = 480;
        private const int DEFAULT_VERTICAL_OFFSET = 0;
        private const int DEFAULT_HORIZONTAL_OFFSET = 0;
        private readonly TimeSpan DEFAULT_DURATION = TimeSpan.FromMilliseconds(250);

        private const double DEFAULT_FONT_SIZE = 48.0;      //48 gives us a thumb-sized font
        private const double BACKGROUND_OPACITY_MAX = 0.97;
        private const double BACKGROUND_OPACITY_MIN = 0.0;
        #endregion

        #region constructor

        public event SelectionChangedEventHandler SelectionChanged;

        #endregion

        #region events

        #endregion

        #region constructor

        public PopupSelectionService()
        {
            //set default values for portrait orientation--pages *should* set these values prior
            //to invoking the ShowPopup method
            RestoreDefaults();

            _popup = new Popup();
        }

        #endregion

        #region properties

        public int Height { get; set; }
        public int Width { get; set; }
        public int VerticalOffset { get; set; }
        public int HorizontalOffset { get; set; }
        public IEnumerable ItemsSource { get; set; }
        public TimeSpan Duration { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Allows callers to determine if the pop up window is open.  Pages should override the OnBackKeyPress
        /// method and examine this value; if a true is returned the "HidePopup" method should be called to
        /// close the popup.
        /// </summary>
        public bool IsPopupOpen
        {
            get
            {
                return _popup.IsOpen;
            }
        }

        #endregion

        #region methods

        public void ShowPopup()
        {
            ShowPopup(-1);
        }

        public void ShowPopup(int selectedIndex)
        {
            BuildLayoutRoot(selectedIndex);

            _popup.Child = _layoutRoot;
            _popup.VerticalOffset = VerticalOffset;
            _popup.HorizontalOffset = HorizontalOffset;

            _popup.IsOpen = true;

            Storyboard storyboard = CreateEaseInAnimationStoryBoard(_layoutRoot, Grid.OpacityProperty, BACKGROUND_OPACITY_MIN, BACKGROUND_OPACITY_MAX, Duration);
            storyboard.Begin();
        }

        private void BuildLayoutRoot(int selectedIndex)
        {
            if (null != _layoutRoot)
            {
                _popup.Child = null;
                _layoutRoot.Children.Clear();
                _layoutRoot = null;
            }

            Brush blackBrush = new SolidColorBrush(Colors.Black);
            Brush whiteBrush = new SolidColorBrush(Colors.White);

            _layoutRoot = new Grid();
            _layoutRoot.Height = Height;
            _layoutRoot.Width = Width;
            _layoutRoot.Background = blackBrush;

            StackPanel controlPanel = new StackPanel();
            controlPanel.Name = "controlPanel";
            controlPanel.Orientation = Orientation.Vertical;
            controlPanel.HorizontalAlignment = HorizontalAlignment.Center;
            controlPanel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Name = "titleTextBlock";
            titleTextBlock.FontSize = DEFAULT_FONT_SIZE;
            titleTextBlock.Foreground = whiteBrush;
            titleTextBlock.Margin = new Thickness(10, 0, 10, 10);
            titleTextBlock.Text = Title;
            titleTextBlock.TextWrapping = TextWrapping.Wrap;
            controlPanel.Children.Add(titleTextBlock);

            ListBox itemsListBox = new ListBox();
            itemsListBox.Name = "itemsListBox";
            itemsListBox.Background = blackBrush;
            itemsListBox.Foreground = whiteBrush;
            itemsListBox.FontSize = DEFAULT_FONT_SIZE;
            itemsListBox.Padding = new Thickness(10, 0, 10, 0);
            itemsListBox.Margin = new Thickness(20, 0, 20, 0);
            itemsListBox.MaxHeight = 300;
            itemsListBox.MaxWidth = Width;
            itemsListBox.ItemsSource = ItemsSource;

            //note that we need to set the selected index prior to wiring up the event handler--
            //we don't want to errantly trigger the event during initialization
            if (selectedIndex < itemsListBox.Items.Count)
            {
                itemsListBox.SelectedIndex = selectedIndex;
            }
            itemsListBox.SelectionChanged += OnListBoxSelectionChanged;

            controlPanel.Children.Add(itemsListBox);
            _layoutRoot.Children.Add(controlPanel);
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            NotifySelectionChanged(args);
            HidePopup();
        }

        private void NotifySelectionChanged(SelectionChangedEventArgs args)
        {
            if (null != SelectionChanged)
            {
                SelectionChanged(this, args);
            }
        }

        public void HidePopup()
        {
            Storyboard storyboard = CreateEaseOutAnimationStoryBoard(_layoutRoot, Grid.OpacityProperty, BACKGROUND_OPACITY_MAX, BACKGROUND_OPACITY_MIN, Duration);
            storyboard.Completed += OnEaseOutStoryboardCompleted;

            //delaying the begin time lets the user see what was picked prior
            //to kicking off the hide animation
            storyboard.BeginTime = DEFAULT_DURATION;
            storyboard.Begin();
        }

        private void OnEaseOutStoryboardCompleted(object sender, EventArgs e)
        {
            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnEaseOutStoryboardCompleted;

            ListBox itemsListBox = _layoutRoot.FindName("itemsListBox") as ListBox;
            if (null != itemsListBox)
            {
                itemsListBox.SelectionChanged -= OnListBoxSelectionChanged;
            }

            _popup.IsOpen = false;
        }


        private Storyboard CreateEaseInAnimationStoryBoard(DependencyObject target, DependencyProperty targetProperty, Double from, Double to, TimeSpan duration)
        {
            Storyboard animationStoryboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = duration,
                From = from,
                To = to,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            animationStoryboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(targetProperty));
            Storyboard.SetTarget(animation, target);
            return animationStoryboard;
        }

        private Storyboard CreateEaseOutAnimationStoryBoard(DependencyObject target, DependencyProperty targetProperty, Double from, Double to, TimeSpan duration)
        {
            Storyboard animationStoryboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                Duration = duration,
                From = from,
                To = to,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            animationStoryboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(targetProperty));
            Storyboard.SetTarget(animation, target);
            return animationStoryboard;
        }

        /// <summary>
        /// Restores the default property values for the service
        /// </summary>
        public void RestoreDefaults()
        {
            Width = DEFAULT_WIDTH;
            Height = DEFAULT_HEIGHT;
            VerticalOffset = DEFAULT_VERTICAL_OFFSET;
            HorizontalOffset = DEFAULT_HORIZONTAL_OFFSET;
            Duration = DEFAULT_DURATION;
        }

        #endregion
    }
}
