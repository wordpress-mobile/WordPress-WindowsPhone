using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WordPress
{
    public partial class Spinner : UserControl
    {
        #region member variables

        private Storyboard _storyboard;

        #endregion

        #region constructor

        public Spinner()
        {
            InitializeComponent();
            BeginAnimation();
        }

        #endregion

        #region properties

        #endregion

        #region methods

        private void BeginAnimation()
        {
            _storyboard = new Storyboard();
            _storyboard.RepeatBehavior = RepeatBehavior.Forever;

            int delay = 0;

            DoubleAnimationUsingKeyFrames animation;
            foreach (UIElement childElement in spinner.Children)
            {
                animation = new DoubleAnimationUsingKeyFrames();

                //start out invisible
                animation.KeyFrames.Add(new SplineDoubleKeyFrame()
                {
                    KeyTime = new TimeSpan(0),
                    Value = 1
                });

                //at 1 second 70% visible
                animation.KeyFrames.Add(new SplineDoubleKeyFrame()
                {
                    KeyTime = new TimeSpan(0, 0, 0, 1, 0),
                    Value = 0.3
                });

                //wait .7 seconds at 70% visible, then back to invisible
                animation.KeyFrames.Add(new SplineDoubleKeyFrame()
                {
                    KeyTime = new TimeSpan(0, 0, 0, 1, 700),
                    Value = 0.3
                });

                animation.RepeatBehavior = RepeatBehavior.Forever;

                //delay the start to get the twirly animation--see below
                animation.BeginTime = new TimeSpan(0, 0, 0, 0, delay);

                //bind the opacity property to the animation
                Storyboard.SetTarget(animation, childElement);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Path.OpacityProperty));

                _storyboard.Children.Add(animation);

                //increment the delay so the next iteration has a different delay value, giving
                //us the twirly effect
                delay += 100;
            }

            _storyboard.Begin();
        }

        #endregion
    }
}
