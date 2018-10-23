using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inside_MMA
{
    public interface IAnchor
    {
        string Board { get; set; }
        string Seccode { get; set; }
        bool IsAnchorEnabled { get; set; }
        List<IAnchor> AnchoredWindows { get; }
        void SetSecurity(string board, string seccode);
    }
}
