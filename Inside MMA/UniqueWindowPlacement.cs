using System;
using System.Windows;
using System.Windows.Input;
using Inside_MMA.DataHandlers;
using Inside_MMA.Properties;
using Inside_MMA.Views;
using MahApps.Metro.Controls;
using SciChart.Core.Extensions;

namespace Inside_MMA
{
    public interface IUniqueWindowPlacement
    {
        void SubscribeToWindowEvents();
        void UnsubscribeFromWindowEvents();
        void RestoreWindowPosition(object sender, EventArgs eventArgs);
        void UpdateWindowPosition(object sender, MouseEventArgs mouseEventArgs);
    }
}