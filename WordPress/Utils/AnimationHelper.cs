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

namespace WordPress
{
    public class AnimationHelper
    {
        public static Storyboard CreateEaseInAnimationStoryBoard(DependencyObject target, DependencyProperty targetProperty, Double from, Double to, TimeSpan duration)
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

        public static Storyboard CreateEaseOutAnimationStoryBoard(DependencyObject target, DependencyProperty targetProperty, Double from, Double to, TimeSpan duration)
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
    }
}
