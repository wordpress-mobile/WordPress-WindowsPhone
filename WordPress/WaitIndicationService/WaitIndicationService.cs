using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WordPress
{
    public class WaitIndicationService : IWaitIndicationService
    {
        #region member variables

        private UIElement _currentWaitElement;
        private Panel _rootVisual;
        private bool _waiting;

        #endregion

        #region constructor

        public WaitIndicationService() { }

        #endregion

        #region properties

        public bool Waiting
        {
            get { return _waiting; }
            private set
            {
                _waiting = value;
            }
        }

        public Panel RootVisualElement
        {
            get { return _rootVisual; }
            set { _rootVisual = value; }
        }

        #endregion

        #region methods

        public void ShowIndicator(string message = "Doing work...")
        {
            Waiting = true;
            var waitElement = CreateWaitElement(message);
            waitElement.Opacity = 0;

            try
            {
                _rootVisual.Children.Add(waitElement);
                
                var showSpinnerAnimation = CreateWaitAnimation(waitElement, Grid.OpacityProperty, 0.8, 0.00, 900, EasingMode.EaseOut);
                var waitStoryboard = new Storyboard();
                waitStoryboard.Children.Add(showSpinnerAnimation);
                waitStoryboard.Begin();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Exception showing indicator: {0}", ex));
            }

            _currentWaitElement = waitElement;
            _rootVisual.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            Waiting = true;
        }

        public void ShowIndicator(TimeSpan duration, string message)
        {
            ShowIndicator(message);

            ThreadPool.QueueUserWorkItem(time =>
            {
                Thread.Sleep((TimeSpan)time);
                _rootVisual.Dispatcher.BeginInvoke(() => HideIndicator());

            }, duration);
        }

        public void ShowIndicator(double milliseconds, string message)
        {
            ShowIndicator(TimeSpan.FromMilliseconds(milliseconds), message);
        }

        public void HideIndicator(double delay)
        {
            HideIndicator(TimeSpan.FromMilliseconds(delay));
        }

        public void HideIndicator(TimeSpan delay)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((arg) =>
            {
                Thread.Sleep(delay);
                Panel panel = (Panel)arg;
                panel.Dispatcher.BeginInvoke(() =>
                {
                    HideIndicator();

                });

            }), _rootVisual);
        }

        public void HideIndicator()
        {
            try
            {
                var hideSpinnerAnimation = CreateWaitAnimation(_currentWaitElement, Grid.OpacityProperty, 0.00, 0.80, 1000, EasingMode.EaseOut);
                var waitStoryboard = new Storyboard();
                waitStoryboard.Children.Add(hideSpinnerAnimation);
                waitStoryboard.Completed += OnHideSpinnerStoryBoardCompleted;
                waitStoryboard.Begin();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Exception caught hiding the indicator: {0}", ex));
            }
        }

        private void OnHideSpinnerStoryBoardCompleted(object sender, EventArgs args)
        {
            Storyboard storyboard = sender as Storyboard;
            storyboard.Completed -= OnHideSpinnerStoryBoardCompleted;

            _rootVisual.Dispatcher.BeginInvoke(() =>
            {
                _rootVisual.Children.Remove(_currentWaitElement);
                _rootVisual.SizeChanged -= OnSizeChanged;
                _currentWaitElement = null;                
            });

            Waiting = false;
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (_currentWaitElement != null)
            {
                (_currentWaitElement as Grid).Width = _rootVisual.ActualWidth;
                (_currentWaitElement as Grid).Height = _rootVisual.ActualHeight;
            }
        }

        private UIElement CreateWaitElement(string message = "Doing work...")
        {
   
            var root = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Width = _rootVisual.ActualWidth,
                Height = _rootVisual.ActualHeight
            };

            StackPanel spinnerPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = _rootVisual.Height,
                Width = _rootVisual.Width
            };

            spinnerPanel.Children.Add(new Spinner()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            spinnerPanel.Children.Add(new TextBlock()
            {
                Text = message,
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White)
            });

            root.Children.Add(spinnerPanel);

            return root;
        }

        private DoubleAnimation CreateWaitAnimation(DependencyObject obj, DependencyProperty prop, double value, double duration, EasingMode easingMode = EasingMode.EaseOut)
        {
            var from = Convert.ToDouble(obj.GetValue(prop));
            return CreateWaitAnimation(obj, prop, value, from, duration, easingMode);
        }

        private DoubleAnimation CreateWaitAnimation(DependencyObject obj, DependencyProperty prop, double value, double from, double duration, EasingMode easingMode = EasingMode.EaseOut)
        {
            CubicEase easingFunction = new CubicEase() { EasingMode = easingMode };
            DoubleAnimation animation = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                From = from,
                To = value,
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = easingFunction
            };

            Storyboard.SetTarget(animation, obj);
            Storyboard.SetTargetProperty(animation, new PropertyPath(prop));

            return animation;
        }

        public void KillSpinner()
        {
            if (null != _rootVisual && null != _currentWaitElement)
            {
                _rootVisual.Children.Remove(_currentWaitElement);
                _rootVisual.SizeChanged -= OnSizeChanged;
                _currentWaitElement = null;
            }
            Waiting = false;
        }

        #endregion
    }
}
