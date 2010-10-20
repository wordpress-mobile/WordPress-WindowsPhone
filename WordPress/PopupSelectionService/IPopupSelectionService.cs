using System;
using System.Collections;
using System.Windows.Controls;

namespace WordPress
{
    public interface IPopupSelectionService
    {
        #region events

        /// <summary>
        /// Notifies subscribes that an item has been selected from the list of options
        /// </summary>
        event SelectionChangedEventHandler SelectionChanged;

        #endregion

        #region properties

        /// <summary>
        /// Specifies the duration of the show/hide animations
        /// </summary>
        TimeSpan Duration { get; set; }

        /// <summary>
        /// Specifies the height of the control; this should typically be the same value as the
        /// screen's visible area
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Specifies the width of the control; this should typically be the same value as the
        /// screen's visible area
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Specifies the offset on the X axis for the popup window.  This value will typically be
        /// zero.
        /// </summary>
        int HorizontalOffset { get; set; }

        /// <summary>
        /// Specifies the offset on the Y axis for the popup window.  This value will typically be
        /// zero.
        /// </summary>
        int VerticalOffset { get; set; }
        
        /// <summary>
        /// The list of items that should be presented to the user when the ShowPopup is invoked
        /// </summary>
        IEnumerable ItemsSource { get; set; }
        
        /// <summary>
        /// The string that should be displayed as the header above the list of choices to choose from
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Indicates that the popup window is open.  Pages should override the OnBackKeyPress method and
        /// examine this value; if it is true the HidePopup method should be called and the backward
        /// navigation should be canceled.
        /// </summary>
        bool IsPopupOpen { get; }

        #endregion

        #region methods

        /// <summary>
        /// Restores the default property values to the service.
        /// </summary>
        void RestoreDefaults();

        /// <summary>
        /// Shows the popup window to the user.
        /// </summary>
        void ShowPopup();

        /// <summary>
        /// Shows the popup window to the user, specifying which item should be selected in the list
        /// </summary>
        /// <param name="selectedIndex"></param>
        void ShowPopup(int selectedIndex);

        /// <summary>
        /// Closes the popup window
        /// </summary>
        void HidePopup();

        #endregion
    }
}
