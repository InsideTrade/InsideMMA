using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;
using Microsoft.TeamFoundation.Controls.WPF;
using Microsoft.TeamFoundation.MVVM;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core.Extensions;
using Timer = System.Threading.Timer;

namespace Inside_MMA.DataHandlers
{
    public class TickSubscription : INotifyPropertyChanged
    {
        private XyDataSeries<double, double> _horizontalVolumesBuy = new XyDataSeries<double, double>() { AcceptsUnsortedData = true };
        public XyDataSeries<double, double> HorizontalVolumesBuy
        {
            get { return _horizontalVolumesBuy; }
            set
            {
                if (Equals(value, _horizontalVolumesBuy)) return;
                _horizontalVolumesBuy = value;
                OnPropertyChanged();
            }
        }
        private XyDataSeries<double, double> _horizontalVolumesSell = new XyDataSeries<double, double>() { AcceptsUnsortedData = true };
        public XyDataSeries<double, double> HorizontalVolumesSell
        {
            get { return _horizontalVolumesSell; }
            set
            {
                if (Equals(value, _horizontalVolumesSell)) return;
                _horizontalVolumesSell = value;
                OnPropertyChanged();
            }
        }
        public List<Tick> Ticks = new List<Tick>();
        public string Board;
        public string Seccode;
        public int Tradeno;
        public bool Sorted;
        private RangeObservableCollection<TradeItem> _tradeItems = new RangeObservableCollection<TradeItem>();
        public RangeObservableCollection<TradeItem> TradeItems
        {
            get { return _tradeItems; }
            set
            {
                if (Equals(value, _tradeItems)) return;
                _tradeItems = value;
                OnPropertyChanged();
            }
        }
        private RangeObservableCollection<Tick> _tradeItemsLimit = new RangeObservableCollection<Tick>();
        public RangeObservableCollection<Tick> TradeItemsLimit
        {
            get => _tradeItemsLimit;
            set
            {
                if (Equals(value, _tradeItemsLimit)) return;
                _tradeItemsLimit = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class TickDataHandler
    {
        public delegate void SendTickData(List<Tick> data);
        private static event SendTickData SendTickDataEvent;
        private static readonly object Lock = new object();

        public static ConcurrentBag<TickSubscription> TickList =
            new ConcurrentBag<TickSubscription>();

        private static List<string> Seccodes => Application.Current.Windows.Cast<Window>()
            .Where(w => w.DataContext is IAnchor).Select(x => ((IAnchor) x.DataContext).Seccode).ToList();
        static TickDataHandler()
        {
            TXmlConnector.SendNewTicks += OnSendNewTicks;
        }

        private static Timer _timer =
            new Timer(ClearSubs, null, new TimeSpan(10, 0, 0), new TimeSpan(10, 0, 0));
        //private static Timer _timer2 = new Timer(CheckSubs, null, 1000, 1000);

        //private static void CheckSubs(object state)
        //{
        //    var brokenSubs = TickList.Where(x => x.Ticks.Count == 0).ToList();
        //    foreach (var sub in brokenSubs)
        //    {
        //        sub.Tradeno = 1;
        //    }
        //    TXmlConnector.ConnectorSendCommand(ReturnTickSubsXml());
        //    foreach (var sub in brokenSubs)
        //    {
        //        sub.Tradeno = 0;
        //    }
        //}

        private static void ClearSubs(object state)
        {
            var windows = _dispatcher.Invoke(() => Application.Current.Windows);
            List<IAnchor> list = new List<IAnchor>();
            foreach (Window window in windows)
            {
                var context = _dispatcher.Invoke(() => window.DataContext);
                if (context is IAnchor)
                    list.Add((IAnchor) context);
            }

            TickSubscription[] arr = new TickSubscription[TickList.Count];
            TickList.CopyTo(arr, 0);
            foreach (var tickSubscription in arr)
            {
                var sub = list.Count(c => c.Board == tickSubscription.Board &&
                                          c.Seccode == tickSubscription.Seccode);
                if (sub == 0)
                {
                    var subscription = tickSubscription;
                    TickList.TryTake(out subscription);
                    TXmlConnector.ConnectorSendCommand(ReturnTickSubsXml());
                }
            }
        }

        public static string ReturnTickSubsXml()
        {
            string securities = string.Empty;
            foreach (var str in TickList)
            {
                securities +=
                    $"<security><board>{str.Board}</board><seccode>{str.Seccode}</seccode><tradeno>{str.Tradeno}</tradeno></security>";
            }
            var result = $"<command id=\"subscribe_ticks\">{securities}<filter>false</filter></command>";
            return result;
        }

        public static void UnsubAll()
        {
            TickList = new ConcurrentBag<TickSubscription>();
            TXmlConnector.ConnectorSendCommand(ReturnTickSubsXml());
        }
        public static void AddTickSubscribtion(string board, string seccode, SendTickData method)
        {
            AddTick(board, seccode);
            SendTickDataEvent += method;
            var sub = TickList.First(x => x.Board == board && x.Seccode == seccode);
            if (sub.Ticks.Count != 0)
                method?.Invoke(sub.Ticks);
        }

        public static XyDataSeries<double, double>[] AddChartSubscription(string board, string seccode,
            SendTickData method)
        {
            AddTick(board, seccode);
            SendTickDataEvent += method;
            var sub = TickList.First(x => x.Board == board && x.Seccode == seccode);
            //if (sub.Ticks.Count != 0)
            //    method?.Invoke(sub.Ticks);
            return new[] {sub.HorizontalVolumesBuy, sub.HorizontalVolumesSell};
        }
        public static void AddTradesCounterSubscribtion(string board, string seccode, SendTickData method)
        {
            AddTick(board, seccode);
            SendTickDataEvent += method;
            var ticks = TickList.First(x => x.Board == board && x.Seccode == seccode).Ticks;
            method.BeginInvoke(ticks, null, null);
        }
        private static void AddTick(string board, string seccode)
        {
            var sub = TickList.FirstOrDefault(x => x.Board == board && x.Seccode == seccode);
            if (sub == null)
            {
                sub = new TickSubscription
                {
                    Board = board,
                    Seccode = seccode,
                    Tradeno = 1
                };
                TickList.Add(sub);
                TXmlConnector.ConnectorSendCommand(ReturnTickSubsXml());
                sub.Tradeno = 0;
            }
        }

        public static void SubscribeToTicksEvent(SendTickData method)
        {
            SendTickDataEvent += method;
        }
        public static void UnsubscribeFromTicksEvent(SendTickData method)
        {
            SendTickDataEvent -= method;
        }
        public static void RefreshSub(string board, string seccode)
        {
            Task.Run(() => {
                var sub = TickList.FirstOrDefault(x => x.Board == board && x.Seccode == seccode);
                _dispatcher.Invoke(() => sub.TradeItems.Clear());
                sub.Sorted = false;
                sub.Tradeno = 1;
                TXmlConnector.ConnectorSendCommand(ReturnTickSubsXml());
                sub.Tradeno = 0;
            });
        }
        private static Dispatcher _dispatcher = Application.Current.Dispatcher;

        private static XmlSerializer _tickSerializer =
            new XmlSerializer(typeof(List<Tick>), new XmlRootAttribute("ticks"));

        private static void OnSendNewTicks(string data)
        {
            List<Tick> list;
            using (var reader = new StringReader(data))
            {
                list = (List<Tick>)_tickSerializer.Deserialize(reader);
                reader.Close();
            }
            var tickList = TickList.ToArray();
            foreach (var tickSub in tickList)
            {
                var currentTicks = list.FindAll(x => x.Board == tickSub.Board && x.Seccode == tickSub.Seccode);
                if (currentTicks.Count == 0) continue;
                HandleTicks(tickSub, currentTicks);
            }
        }
        private static void HandleTicks(TickSubscription tickSub, List<Tick> currentTicks)
        {
            //add ticks to ticks collection for this instument
            tickSub.Ticks.AddRange(currentTicks);
            tickSub.TradeItemsLimit.AddRange(currentTicks);
            ////populate simple ticks observable collection
            //currentTicks.ForEach(ct => _dispatcher.BeginInvoke(new Action(() => tickSub.SimpleTicks.Insert(0, ct)), DispatcherPriority.Background));
            //send data to all windows with corresponding instrument opened
            var invocationList = SendTickDataEvent?.GetInvocationList()
                .Where(x => ((dynamic)x.Target).Seccode == tickSub.Seccode)
                .ToList();
            invocationList.ForEachDo(x => x.DynamicInvoke(currentTicks));
            //add data to volumes collections (chart)
            if (tickSub.Board != "MCT" && tickSub.Board != "INDEXR" && tickSub.Board != "INDEXM")
            {
                Task.Run(() => {
                    var points = currentTicks.Select(t => t.Price).Distinct();
                    foreach (var point in points)
                    {
                        if (!tickSub.HorizontalVolumesBuy.XValues.Contains(point))
                            tickSub.HorizontalVolumesBuy.Append(point, 0);
                        if (!tickSub.HorizontalVolumesSell.XValues.Contains(point))
                            tickSub.HorizontalVolumesSell.Append(point, 0);
                    }
                    foreach (var tick in currentTicks)
                    {
                        if (tick.Buysell == "B")
                        {
                            var index = tickSub.HorizontalVolumesBuy.FindIndex(tick.Price);
                            if (index != -1)
                                tickSub.HorizontalVolumesBuy.Update(index,
                                    tickSub.HorizontalVolumesBuy.YValues[index] + tick.Price);
                        }
                        else
                        {
                            var index = tickSub.HorizontalVolumesSell.FindIndex(tick.Price);
                            if (index != -1)
                                tickSub.HorizontalVolumesSell.Update(index,
                                    tickSub.HorizontalVolumesSell.YValues[index] + tick.Price);
                        }
                    }
                });
            }
            //process alltrades
            var currentTradeItems = (from x in currentTicks
                select
                new TradeItem(x.Seccode, x.Price, x.Quantity, DateTime.Parse(x.Tradetime).ToString("HH:mm:ss.fff"),
                    x.Buysell, x.Quantity.ToString()) { OpenInterest = x.OpenInterest, Board = x.Board}).ToList();

            //calculate interest only for FUT
            if (tickSub.Board == "FUT")
            {
                var i = 0;
                //if no trades, set first interest delta to 0 and skip first trade when counting further
                if (tickSub.TradeItems.Count == 0)
                {
                    currentTradeItems.First().InterestDelta = "0";
                    i++;
                }
                for (; i < currentTradeItems.Count; i++)
                {
                    var tradeItem = currentTradeItems[i];
                    //for first new trade, calculate delta using newest trade in alltrades
                    if (i == 0)
                    {
                        var lastTrade = tickSub.TradeItems.First().OpenInterest;
                        tradeItem.InterestDelta = (int.Parse(tradeItem.OpenInterest) -
                                                   int.Parse(lastTrade.Split(',').Last())).ToString();
                    }
                    else
                        tradeItem.InterestDelta = (int.Parse(tradeItem.OpenInterest) -
                                                   int.Parse(currentTradeItems[i - 1].OpenInterest)).ToString();
                }
                
            }
            
            var tradeItems = currentTradeItems.GroupBy(item => new { item.Seccode, item.Time, item.Buysell })
                .Select(
                    g =>
                        new TradeItem(g.Key.Seccode,
                            g.Key.Buysell == "B" ? g.Select(t => t.Price).Min() : g.Select(t => t.Price).Max(),
                            g.Sum(t => t.Quantity), g.Key.Time, g.Key.Buysell,
                            string.Join(",", g.Select(t => t.Quantity)))
                        {
                            PriceList = string.Join(";", g.Select(t => t.Price).Distinct()),
                            MiSide =
                                g.Key.Buysell == "B"
                                    ? "" +
                                        (g.Select(t => t.Price).Max() - g.Select(t => t.Price).Min()).ToString("F2")
                                    : "-" +
                                        (g.Select(t => t.Price).Max() - g.Select(t => t.Price).Min()).ToString("F2"),
                            OpenInterest = string.Join(",", g.Select(t => t.OpenInterest)),
                            InterestDelta = string.Join(",", g.Select(t => t.InterestDelta)),
                        }).ToList();
            foreach (var t in tradeItems)
            {
                if (t.Quantity.ToString() != t.Sum)
                    t.IsMul = true;
                foreach (var size in t.Sum.Split(','))
                {
                    if (int.Parse(size) > t.Quantity * 0.8)
                        t.IsEaten = true;
                }
                if (tickSub.Board == "MCT")
                {
                    var level2 = Application.Current.Dispatcher.Invoke(() => Level2DataHandler.Level2List
                        .FirstOrDefault(x => x.Board == tickSub.Board && x.Seccode == tickSub.Seccode)
                        ?.Level2Data
                        .FirstOrDefault(item => item.Price == t.Price));
                    if (level2 != null)
                    {
                        t.Buysell = level2.BuySell == "buy" ? "S" : "B";
                        var str = t.MiSide;
                        str = str.TrimStart('-');
                        var sign = level2.BuySell == "buy" ? "-" : "";
                        str = str.Insert(0, sign);
                        t.MiSide = str;
                    }
                }
            }
            tradeItems.Sort((a, b) => a.Time.CompareTo(b.Time));
            //tickSub.TradeItems.DoOperation(col => col.InsertRange(0, tradeItems));

            //if (tradeItems.Count < 5 && !tickSub.Sorted)
            //{
            //    tickSub.TradeItems.DoOperation(col => col.Sort((a, b) => b.Time.CompareTo(a.Time)));
            //    tickSub.Sorted = true;
            //}                
            //if (tradeItems.Count > 500)
            //    //_dispatcher.BeginInvoke(new Action(() => tickSub.TradeItems.InsertRange(tradeItems)), DispatcherPriority.Background);
            //    _dispatcher.Invoke(() => tickSub.TradeItems.InsertRange(tradeItems));
            //else
            foreach (var item in tradeItems)
            {
                //_dispatcher.BeginInvoke(new Action(() => tickSub.TradeItems.Insert(0, item)), DispatcherPriority.Background);
                _dispatcher.Invoke(() => tickSub.TradeItems.Insert(0, item));
            }
            //if (tradeItems.Count < 5 && !tickSub.Sorted)
            //{
            //    tickSub.TradeItems = new ObservableCollection<TradeItem>(tickSub.TradeItems.OrderByDescending(x => x.Time));
            //    tickSub.Sorted = true;
            //}
        }
        public static ObservableCollection<TradeItem> AddAllTradesSubsribtion(string board, string seccode)
        {
            AddTick(board, seccode);
            var sub = TickList.First(x => x.Board == board && x.Seccode == seccode);
            return sub.TradeItems;
        }

        public static void WatchlistSub(List<Security> list)
        {
            foreach (var sec in list)
            {
                if (!TickList.Select(t => t.Seccode).Contains(sec.Seccode))
                    TickList.Add(new TickSubscription { Board = sec.Board, Seccode = sec.Seccode, Tradeno = 1 });
            }
            TXmlConnector.ConnectorSendCommand(ReturnTickSubsXml());
            foreach (var subscription in TickList)
            {
                subscription.Tradeno = 0;
            }
        }
    }
}