using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Serialization;
using SciChart.Core.Extensions;
using Inside_MMA.Models;
using Inside_MMA.Models.Filters;
using Inside_MMA.ViewModels;
using Inside_MMA.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SciChart.Charting.Common.Extensions;

namespace Inside_MMA.DataHandlers
{
    public static class WindowDataHandler
    {
        //public static List<WindowPosition> Placements = new List<WindowPosition>();

        public static Dictionary<int, WindowData> WindowPlacements = new Dictionary<int, WindowData>();
        private static Timer _timer = new Timer(SaveWindowData, null, 2000, 2000);
        public static int GenerateWindowId()
        {
            var rng = new Random();
            int id = 1;
            while (WindowPlacements.ContainsKey(id))
            {
                id = rng.Next(1, 1000000);
            }
            return id;
        }
        public static void RemoveFromOpenedWindows(int windowId)
        {
            if (WindowPlacements.ContainsKey(windowId))
                WindowPlacements.Remove(windowId);
        }
        public static void UpdateWindowPlacement(int windowId, string placement)
        {
            if (WindowPlacements.ContainsKey(windowId))
                WindowPlacements[windowId].Placement = placement;
        }
        public static void UpdateWindowInstrument(int windowId, string board, string seccode)
        {
            WindowPlacements[windowId].Board = board;
            WindowPlacements[windowId].Seccode = seccode;
        }

        public static void UpdateWindowBinding(int windowId, bool isanchored)
        {
            if (WindowPlacements.ContainsKey(windowId))
                WindowPlacements[windowId].IsAnchored = isanchored;
        }

        public static void UpdateWindowArgs(int windowId, dynamic args)
        {
            if (WindowPlacements.ContainsKey(windowId))
                WindowPlacements[windowId].Args = args;
        }

        public static dynamic GetWindowArgs(int windowId)
        {
            try
            {
                return WindowPlacements[windowId].Args;
            }
            catch
            {
                return null;
            }
        }
        public static void CloseWindow(int id)
        {
            if (!(MainWindowViewModel.IsShuttingDown || MainWindowViewModel.IsDisconnecting))
                WindowPlacements.Remove(id);
        }

        public static int OpenSpecialWindow(string type, string vmtype)
        {
            var w = WindowPlacements.FirstOrDefault(x => x.Value.ViewModelType == vmtype);
            if (w.Value != null)
                return w.Key;
            var id = GenerateWindowId();
            WindowPlacements.Add(id, new WindowData(type, vmtype, null, null, null) {Args = "special_window"});
            return id;
        }
        public static void CloseSpecialWindow(int id)
        {
            WindowPlacements.Remove(id);
        }
        public static string GetSavedPlacement(int id)
        {
            return WindowPlacements[id].Placement;
        }
        public static int SaveWindow(string windowType, string viewModelType, string board, string seccode)
        {
            var windowData = new WindowData(windowType, viewModelType, board, seccode, "");
            var id = GenerateWindowId();
            WindowPlacements.Add(id, windowData);
            return id;
        }

        public static void SaveWindowData(object state)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/windows1";
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(WindowPlacements));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }
        public static void GetWindowData(string path = null)
        {
            if (path == null)
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/windows1";
            try
            {
                var data = File.ReadAllText(path);
                WindowPlacements = (Dictionary<int, WindowData>)
                    ((JObject) JsonConvert.DeserializeObject(data)).ToObject(typeof(Dictionary<int, WindowData>));

                //convert args to objects
                foreach (var windowData in WindowPlacements)
                {
                    if (windowData.Value.Args == null) continue;
                    if (windowData.Value.WindowType == typeof(Level2).ToString())
                        windowData.Value.Args =
                            (Level2Args) ((JObject) windowData.Value.Args).ToObject(typeof(Level2Args));
                    if (windowData.Value.WindowType == typeof(AllTrades).ToString())
                        windowData.Value.Args =
                            (AllTradesFilter)((JObject)windowData.Value.Args).ToObject(typeof(AllTradesFilter));
                    if (windowData.Value.WindowType == typeof(SciChartWindow).ToString())
                        windowData.Value.Args =
                            (ChartArgs)((JObject)windowData.Value.Args).ToObject(typeof(ChartArgs));
                    if (windowData.Value.WindowType == typeof(LogBook).ToString())
                        windowData.Value.Args =
                            (LogBookArgs)((JObject)windowData.Value.Args).ToObject(typeof(LogBookArgs));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}