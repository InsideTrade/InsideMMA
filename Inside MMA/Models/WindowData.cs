using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inside_MMA.Models
{
    //[Serializable]
    //public class WindowData
    //{
    //    public string Board { get; set; }
    //    public string Seccode { get; set; }
    //    public string WindowType { get; set; }
    //    public string ViewModelType { get; set; }
    //    public string Placement { get; set; }
    //    public string IsAnchored { get; set; }
    //    public dynamic Args { get; set; }
    //    public WindowData() { }
    //    public WindowData(string board, string seccode, string windowType, string viewModelType,
    //        string placement)
    //    {
    //        Board = board;
    //        Seccode = seccode;
    //        WindowType = windowType;
    //        ViewModelType = viewModelType;
    //        Placement = placement;
    //    }
    //}

    public class WindowData
    {
        public string WindowType { get; set; }
        public string ViewModelType { get; set; }
        public string Board { get; set; }
        public string Seccode { get; set; }
        public string Placement { get; set; }
        public bool IsAnchored { get; set; }
        public dynamic Args { get; set; }

        public WindowData(string windowType, string viewModelType, string board, string seccode, string placement)
        {
            WindowType = windowType;
            ViewModelType = viewModelType;
            Board = board;
            Seccode = seccode;
            Placement = placement;
        }
    }

    public class ChartArgs
    {
        [JsonProperty]
        public string SelectedTimeframe { get; set; }
        [JsonProperty]
        public int DaysBack { get; set; }
        [JsonProperty]
        public bool ToggleTrendlines { get; set; }
        public ChartArgs(string selectedTimeFrame, int daysBack, bool toggleTrendlines)
        {
            SelectedTimeframe = selectedTimeFrame;
            DaysBack = daysBack;
            ToggleTrendlines = toggleTrendlines;
        }
    }
}
