using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Inside_MMA
{
    public interface IRememberPlacement
    {
        int WindowId { get; set; }
        void RestorePosition(object sender, EventArgs eventArgs);
        bool IsManuallyClosed { get; set; }
        bool RestorationRequired { get; set; }
        void OnClosing(object sender, CancelEventArgs e);
    }
}