// (c) Copyright 2011 Microsoft Corporation.
// This source is subject to the Microsoft Public License (MS-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
// Author: Jason Ginchereau - jasongin@microsoft.com - http://blogs.msdn.com/jasongin/
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WordPress
{
    /// <summary>
    /// Attaches a pull-down-to-refresh mechanism to a ScrollViewer.
    /// </summary>
    /// <remarks>
    /// To use, position this element at the top of the ScrollViewer. The container of this element
    /// must also contain the ScrollViewer, though it may contain it indirectly. Example: a
    /// StackPanel contains a PullDownToRefreshPanel and a ListBox; the ListBox internally uses a
    /// ScrollViewer to scroll its items.
    /// </remarks>
    [TemplateVisualState(Name = PullDownToRefreshPanel.InactiveVisualState, GroupName = PullDownToRefreshPanel.ActivityVisualStateGroup)]
    [TemplateVisualState(Name = PullDownToRefreshPanel.PullingDownVisualState, GroupName = PullDownToRefreshPanel.ActivityVisualStateGroup)]
    [TemplateVisualState(Name = PullDownToRefreshPanel.ReadyToReleaseVisualState, GroupName = PullDownToRefreshPanel.ActivityVisualStateGroup)]
    [TemplateVisualState(Name = PullDownToRefreshPanel.RefreshingVisualState, GroupName = PullDownToRefreshPanel.ActivityVisualStateGroup)]
    public class PullDownToRefreshPanel : Control
    {
        #region Visual state name constants

        private const string ActivityVisualStateGroup = "ActivityStates";
        private const string InactiveVisualState = "Inactive";
        private const string PullingDownVisualState = "PullingDown";
        private const string ReadyToReleaseVisualState = "ReadyToRelease";
        private const string RefreshingVisualState = "Refreshing";

        #endregion

        /// <summary>
        /// The ScrollViewer that is watched for pull-down-and-release events.
        /// </summary>
        private ScrollViewer targetScrollViewer;

        /// <summary>
        /// Creates a new PullDownToRefreshPanel.
        /// </summary>
        public PullDownToRefreshPanel()
        {
            this.DefaultStyleKey = typeof(PullDownToRefreshPanel);
            this.LayoutUpdated += this.PullDownToRefreshPanel_LayoutUpdated;
        }

        /// <summary>
        /// Event raised when the target ScrollViewer is pulled down past the PullThreshold and then released.
        /// The handler of this event should set IsRefreshing to true if a refresh is actually started,
        /// and then set IsRefreshing to false when the refresh completes.
        /// </summary>
        public event EventHandler RefreshRequested;

        #region IsRefreshing DependencyProperty

        public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register(
            "IsRefreshing", typeof(bool), typeof(PullDownToRefreshPanel),
            new PropertyMetadata(false, (d, e) => ((PullDownToRefreshPanel)d).OnIsRefreshingChanged(e)));

        /// <summary>
        /// Gets or sets the refreshing state of the target. While the target is refreshing,
        /// this panel displays the refreshing template and does not allow another concurrent
        /// refresh to be triggered.
        /// </summary>
        public bool IsRefreshing
        {
            get
            {
                return (bool)this.GetValue(PullDownToRefreshPanel.IsRefreshingProperty);
            }
            set
            {
                this.SetValue(PullDownToRefreshPanel.IsRefreshingProperty, value);
            }
        }

        protected void OnIsRefreshingChanged(DependencyPropertyChangedEventArgs e)
        {
            string activityState = (bool)e.NewValue ?
                PullDownToRefreshPanel.RefreshingVisualState : PullDownToRefreshPanel.InactiveVisualState;
            VisualStateManager.GoToState(this, activityState, false);
        }

        #endregion

        #region PullThreshold DependencyProperty

        public static readonly DependencyProperty PullThresholdProperty = DependencyProperty.Register(
            "PullThreshold", typeof(double), typeof(PullDownToRefreshPanel), new PropertyMetadata(100.0));

        /// <summary>
        /// Gets or sets the minimum distance that the scroll viewer must be pulled down from the top before
        /// releasing it will trigger a refresh. The default value is 100; the maximum recommended value is 125.
        /// </summary>
        public double PullThreshold
        {
            get
            {
                return (double)this.GetValue(PullDownToRefreshPanel.PullThresholdProperty);
            }
            set
            {
                this.SetValue(PullDownToRefreshPanel.PullThresholdProperty, value);
            }
        }

        #endregion

        #region PullDistance DependencyProperty

        public static readonly DependencyProperty PullDistanceProperty = DependencyProperty.Register(
            "PullDistance", typeof(double), typeof(PullDownToRefreshPanel), new PropertyMetadata(0.0));

        /// <summary>
        /// Gets the current distance that the target ScrollViewer has been pulled down from the top.
        /// Useful for binding visuals that adjust to this distance.
        /// </summary>
        public double PullDistance
        {
            get
            {
                return (double)this.GetValue(PullDownToRefreshPanel.PullDistanceProperty);
            }
            private set
            {
                this.SetValue(PullDownToRefreshPanel.PullDistanceProperty, value);
            }
        }

        #endregion

        #region PullFraction DependencyProperty

        public static readonly DependencyProperty PullFractionProperty = DependencyProperty.Register(
            "PullFraction", typeof(double), typeof(PullDownToRefreshPanel), new PropertyMetadata(0.0));

        /// <summary>
        /// Gets the current distance that the target ScrollViewer has been pulled down from the top,
        /// expressed as a fraction (0.0 - 1.0) of the PullThreshold.
        /// </summary>
        public double PullFraction
        {
            get
            {
                return (double)this.GetValue(PullDownToRefreshPanel.PullFractionProperty);
            }
            private set
            {
                this.SetValue(PullDownToRefreshPanel.PullFractionProperty, value);
            }
        }

        #endregion

        #region PullingDownTemplate DependencyProperty

        public static readonly DependencyProperty PullingDownTemplateProperty = DependencyProperty.Register(
            "PullingDownTemplate", typeof(DataTemplate), typeof(PullDownToRefreshPanel), null);

        /// <summary>
        /// Gets or sets a template that is progressively revealed has the ScrollViewer is pulled down.
        /// </summary>
        public DataTemplate PullingDownTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(PullDownToRefreshPanel.PullingDownTemplateProperty);
            }
            set
            {
                this.SetValue(PullDownToRefreshPanel.PullingDownTemplateProperty, value);
            }
        }

        #endregion

        #region ReadyToReleaseTemplate DependencyProperty

        public static readonly DependencyProperty ReadyToReleaseTemplateProperty = DependencyProperty.Register(
            "ReadyToReleaseTemplate", typeof(DataTemplate), typeof(PullDownToRefreshPanel), null);

        /// <summary>
        /// Gets or sets the template that is displayed after the ScrollViewer is pulled down past
        /// the PullThreshold.
        /// </summary>
        public DataTemplate ReadyToReleaseTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(PullDownToRefreshPanel.ReadyToReleaseTemplateProperty);
            }
            set
            {
                this.SetValue(PullDownToRefreshPanel.ReadyToReleaseTemplateProperty, value);
            }
        }

        #endregion

        #region RefreshingTemplate DependencyProperty

        public static readonly DependencyProperty RefreshingTemplateProperty = DependencyProperty.Register(
            "RefreshingTemplate", typeof(DataTemplate), typeof(PullDownToRefreshPanel), null);

        /// <summary>
        /// Gets or sets the template that is displayed while the ScrollViewer is refreshing.
        /// </summary>
        public DataTemplate RefreshingTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(PullDownToRefreshPanel.RefreshingTemplateProperty);
            }
            set
            {
                this.SetValue(PullDownToRefreshPanel.RefreshingTemplateProperty, value);
            }
        }

        #endregion

        #region Initial setup

        /// <summary>
        /// Automatically attaches to a target ScrollViewer within the same container.
        /// </summary>
        private void PullDownToRefreshPanel_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.targetScrollViewer == null)
            {
                this.targetScrollViewer = FindVisualElement<ScrollViewer>(VisualTreeHelper.GetParent(this));
                if (this.targetScrollViewer != null)
                {
                    // The ManipulationMode property is first available with the Mango release,
                    // and with that release is required for pull-down detection.
                    // Delete the following line if targeting Windows Phone 7.0.
                    //this.targetScrollViewer.ManipulationMode = ManipulationMode.Control;

                    this.targetScrollViewer.MouseMove += this.targetScrollViewer_MouseMove;
                    this.targetScrollViewer.MouseLeftButtonUp += this.targetScrollViewer_MouseLeftButtonUp;
                }
            }
        }

        #endregion

        #region Pull-down detection

        /// <summary>
        /// When the target ScrollViewer is dragged, animates the PullDistance/PullFraction properties
        /// and updates the PullingDown/ReadyToRelease visual states as necessary.
        /// </summary>
        private void targetScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            UIElement scrollContent = (UIElement)this.targetScrollViewer.Content;
            CompositeTransform ct = scrollContent.RenderTransform as CompositeTransform;
            if (ct != null && !this.IsRefreshing)
            {
                string activityState;
                if (ct.TranslateY > this.PullThreshold)
                {
                    this.PullDistance = ct.TranslateY;
                    this.PullFraction = 1.0;
                    activityState = PullDownToRefreshPanel.ReadyToReleaseVisualState;
                }
                else if (ct.TranslateY > 0)
                {
                    this.PullDistance = ct.TranslateY;
                    double threshold = this.PullThreshold;
                    this.PullFraction = threshold == 0.0 ? 1.0 : Math.Min(1.0, ct.TranslateY / threshold);
                    activityState = PullDownToRefreshPanel.PullingDownVisualState;
                }
                else
                {
                    this.PullDistance = 0;
                    this.PullFraction = 0;
                    activityState = PullDownToRefreshPanel.InactiveVisualState;
                }
                VisualStateManager.GoToState(this, activityState, false);
            }
        }

        /// <summary>
        /// When the a drag of the target ScrollViewer is released, checks if the release
        /// is past the PullThreshold and if so raises a RefreshRequested event.
        /// </summary>
        private void targetScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement scrollContent = (UIElement)this.targetScrollViewer.Content;
            CompositeTransform ct = scrollContent.RenderTransform as CompositeTransform;
            if (ct != null && !this.IsRefreshing)
            {
                VisualStateManager.GoToState(this, PullDownToRefreshPanel.InactiveVisualState, false);
                this.PullDistance = 0;
                this.PullFraction = 0;

                if (ct.TranslateY >= this.PullThreshold)
                {
                    EventHandler handler = this.RefreshRequested;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Performs a breadth-first search of all elements in the container,
        /// and returns the first element encountered that has type T.
        /// </summary>
        /// <typeparam name="T">Type of element to search for.</typeparam>
        /// <param name="initial">The container to search</param>
        /// <returns>Element of the requested type, or null if none was found.</returns>
        private static T FindVisualElement<T>(DependencyObject container) where T : DependencyObject
        {
            Queue<DependencyObject> childQueue = new Queue<DependencyObject>();
            childQueue.Enqueue(container);

            while (childQueue.Count > 0)
            {
                DependencyObject current = childQueue.Dequeue();

                T result = current as T;
                if (result != null && result != container)
                {
                    return result;
                }

                int childCount = VisualTreeHelper.GetChildrenCount(current);
                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    childQueue.Enqueue(VisualTreeHelper.GetChild(current, childIndex));
                }
            }

            return null;
        }

        #endregion
    }
}