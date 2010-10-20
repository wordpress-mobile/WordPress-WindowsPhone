using System;
using System.Windows.Controls;

namespace WordPress
{
    public interface IWaitIndicationService
    {
        bool Waiting { get; }

        Panel RootVisualElement { get; set; }

        void ShowIndicator(string message);
        void ShowIndicator(TimeSpan duration, string message);
        void ShowIndicator(double milliseconds, string message);

        void HideIndicator(double delay);
        void HideIndicator(TimeSpan delay);
        void HideIndicator();
    }
}
