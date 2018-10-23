using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using Inside_MMA.Models.Alerts;
using Inside_MMA.ViewModels;
using SciChart.Core.Extensions;

namespace Inside_MMA.DataHandlers
{
    public delegate void NewBestBuySell(Level2Item bestSell, Level2Item bestBuy);
    public delegate void Level2ColChanged(List<Level2Item> list);
    public class Level2Subscription : INotifyPropertyChanged
    {
        public ObservableCollection<Level2Item> Level2Data
        {
            get { return _level2Data; }
            set
            {
                if (Equals(value, _level2Data)) return;
                _level2Data = value;
                OnPropertyChanged();
            }
        }

        //public List<Level2Item> BestBuySell = new List<Level2Item>();
        public int SubsCount { get; set; }
        public string Board;
        public string Seccode;
        public double BestBuy;
        private ObservableCollection<Level2Item> _level2Data = new ObservableCollection<Level2Item>();


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public static class Level2DataHandler
    {
        public static event Level2ColChanged Level2ColChanged;
        public static event NewBestBuySell NewBestBuySell;
        private static readonly object Lock = new object();
        public static ConcurrentBag<Level2Subscription> Level2List =
            new ConcurrentBag<Level2Subscription>();

        static Level2DataHandler()
        {
            TXmlConnector.SendNewQuotes += OnSendNewQuotes;
        }

        private static Timer _timer =
            new Timer(ClearSubs, null, new TimeSpan(10, 0, 0), new TimeSpan(10, 0, 0));

        //private static Timer _timer2 = 
        //    new Timer(CheckSubs, null, 1000, 1000);

        //private static void CheckSubs(object state)
        //{
        //    var brokenSubs = Level2List.Where(x => x.Level2Data.Count == 0).ToList();
        //    foreach (var sub in brokenSubs)
        //    {
        //        TXmlConnector.ConnectorSendCommand(
        //            ConnectorCommands.SubUnsubCommand("subscribe", "quotes", sub.Board, sub.Seccode));
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
                    list.Add((IAnchor)context);
            }

            Level2Subscription[] arr = new Level2Subscription[Level2List.Count];
            Level2List.CopyTo(arr,0);
            foreach (var level2Subscription in arr)
            {
                var sub = list.Count(c => c.Board == level2Subscription.Board &&
                                            c.Seccode == level2Subscription.Seccode);
                if (sub == 0)
                {
                    TXmlConnector.ConnectorSendCommand(
                        ConnectorCommands.SubUnsubCommand("unsubscribe", "quotes", level2Subscription.Board, level2Subscription.Seccode));
                    var subscription = level2Subscription;
                    Level2List.TryTake(out subscription);
                }
            }
        }

        public static void UnsubAll()
        {
            foreach (var subscription in Level2List)
            {
                TXmlConnector.ConnectorSendCommand(
                    ConnectorCommands.SubUnsubCommand("unsubscribe", "quotes", subscription.Board, subscription.Seccode));
                var level2Subscription = subscription;
                Level2List.TryTake(out level2Subscription);
            }
        }
        public static ObservableCollection<Level2Item> AddLevel2Subscribtion(string board, string seccode)
        {
            var subscription = Level2List.FirstOrDefault(x => x.Board == board && x.Seccode == seccode);
            if (subscription == null)
            {
                subscription = new Level2Subscription
                {
                    Board = board,
                    Seccode = seccode
                };
                Level2List.Add(subscription);
                TXmlConnector.ConnectorSendCommand(
                    ConnectorCommands.SubUnsubCommand("subscribe", "quotes", subscription.Board, subscription.Seccode));
            }
            return subscription.Level2Data;
        }

        private static Dispatcher _dispatcher = Application.Current.Dispatcher;
        private static XmlSerializer _serializer = new XmlSerializer(typeof(List<Quote>), new XmlRootAttribute("quotes"));
        private static void OnSendNewQuotes(string data)
        {
            HandleData(data);
        }

        private static void HandleData(object data)
        {
            List<Quote> quotes;
            using (var reader = new StringReader(data.ToString()))
            {
                quotes =
                ((List<Quote>)
                    _serializer.Deserialize(reader)).ToList();
                reader.Close();
            }
            List<Level2Subscription> level2List;
            lock (Lock)
            {
                level2List = Level2List.ToList();
                foreach (var level2Sub in level2List)
                {
                    var currentQuotes =
                        quotes.Where(x => x.Board == level2Sub.Board && x.Seccode == level2Sub.Seccode).ToList();
                    if (currentQuotes.Count == 0) continue;
                    var list = currentQuotes
                        .Where(item => item.Buy == 0)
                        .Select(
                            item => new Level2Item
                            {
                                BuySell = "sell",
                                Quantity = item.Sell,
                                //Buy = item.Buy,
                                //Sell = item.Sell,
                                Price = item.Price,
                                Source = item.Source
                            })
                        .Union(currentQuotes.Where(item => item.Sell == 0)
                            .Select(
                                item =>
                                    new Level2Item
                                    {
                                        BuySell = "buy",
                                        Quantity = item.Buy,
                                        //Buy = item.Buy,
                                        //Sell = item.Sell,
                                        Price = item.Price,
                                        Source = item.Source
                                    }))
                        .ToList();
                    
                    var level2Items = level2Sub.Level2Data.ToList();
                    //level2Items.RemoveAll(x => list.Where(i => i.Quantity == -1).Contains(x));
                    //list.RemoveAll(i => i.Quantity == -1);
                    Level2Item found;
                    foreach (var item in list)
                    {
                        found = level2Items.FirstOrDefault(x => x.Price == item.Price &&
                                                       x.BuySell == item.BuySell &&
                                                       x.Source == item.Source);
                        if (found != null)
                        {
                            if (item.Quantity == -1)
                                level2Items.Remove(found);
                            else
                            {
                                //found.Source = item.Source;
                                //found.BuySell = item.BuySell;
                                found.Quantity = item.Quantity;
                            }
                        }
                        else
                            level2Items.Add(item);
                    }
                    
                    if (level2Items.Count == 0) return;
                    var max = level2Items.Select(item => item.Quantity).Max();
                    foreach (var item in level2Items)
                    {
                        item.Percentage = (double)item.Quantity / max;
                    }
                    UpdateTable(level2Sub.Level2Data, level2Items, level2Sub.Board);
                    NewBestBuySell?.GetInvocationList().ForEachDo(i =>
                    {
                        dynamic target = i.Target;
                        if (target.Board == level2Sub.Board && target.Seccode == level2Sub.Seccode)
                        {
                            if (level2Items.Count != 0)
                            {
                                var minSell = level2Items.Where(x => x.BuySell == "sell").Min(x => x.Price);
                                var maxBuy = level2Items.Where(x => x.BuySell == "buy").Max(x => x.Price);
                                i.DynamicInvoke(level2Items.Find(x => x.Price == minSell),
                                    level2Items.Find(x => x.Price == maxBuy));
                            }
                        }
                    });
                    Level2ColChanged?.GetInvocationList().ForEachDo(i =>
                    {
                        var target = (IAnchor)i.Target;
                        if (target.Board == level2Sub.Board && target.Seccode == level2Sub.Seccode)
                            i.DynamicInvoke(level2Items.ToList());
                    });
                }
            }
        }

        //private static void HandleData(object data)
        //{
        //    List<Quote> quotes;
        //    using (var reader = new StringReader(data.ToString()))
        //    {
        //        quotes =
        //        ((List<Quote>)
        //            _serializer.Deserialize(reader)).ToList();
        //        reader.Close();
        //    }
        //    List<Level2Subscription> level2List;
        //    lock (Lock)
        //    {
        //        level2List = Level2List.ToList();
        //        foreach (var level2Sub in level2List)
        //        {
        //            var currentQuotes =
        //                quotes.Where(x => x.Board == level2Sub.Board && x.Seccode == level2Sub.Seccode).ToList();
        //            if (currentQuotes.Count == 0) continue;
        //            var list = currentQuotes
        //                .Where(item => item.Buy == 0)
        //                .Select(
        //                    item => new Level2Item
        //                    {
        //                        BuySell = "sell",
        //                        Quantity = item.Sell,
        //                        Price = item.Price,
        //                        Source = item.Source
        //                    }).OrderByDescending(x => x.Price)
        //                .Union(currentQuotes.Where(item => item.Sell == 0)
        //                    .Select(
        //                        item =>
        //                            new Level2Item
        //                            {
        //                                BuySell = "buy",
        //                                Quantity = item.Buy,
        //                                Price = item.Price,
        //                                Source = item.Source
        //                            }).OrderByDescending(x => x.Price))
        //                .ToList();

        //            var level2Items = level2Sub.Level2Data;
        //            foreach (var item in list)
        //            {
        //                var found = level2Items.FirstOrDefault(x => x.Price == item.Price &&
        //                                                            x.BuySell == item.BuySell &&
        //                                                            x.Source == item.Source);
        //                if (found != null)
        //                {
        //                    if (item.Quantity == -1)
        //                        _dispatcher.Invoke(() => level2Items.Remove(found));
        //                    else
        //                    {
        //                        var i = level2Items.IndexOf(found);
        //                        level2Items[i].Quantity = item.Quantity;
        //                    }
        //                }
        //                else
        //                    AddInOrder(item, level2Items);
        //            }
        //            _dispatcher.Invoke(() => level2Items.RemoveWhere(item => item.Quantity == -1));
        //            var buy = level2Items.Count(item => item.BuySell == "buy");
        //            var sell = level2Items.Count(item => item.BuySell == "sell");
        //            for (var i = 0; i < buy - 20; i++)
        //            {
        //                _dispatcher.Invoke(() => level2Items.Remove(
        //                    level2Items.Where(item => item.BuySell == "buy")
        //                        .OrderByDescending(item => item.Price)
        //                        .Last()));
        //            }
        //            for (var i = 0; i < sell - 20; i++)
        //            {
        //                _dispatcher.Invoke(() => level2Items.Remove(
        //                    level2Items.Where(item => item.BuySell == "sell")
        //                        .OrderByDescending(item => item.Price)
        //                        .First()));
        //            }
        //            //remove duplicates by price
        //            if (level2Sub.Board != "MCT" && level2Items.GroupBy(x => x.Price).Any(x => x.Count() != 1))
        //            {
        //                var dupes = level2Items.GroupBy(c => c.Price).Where(g => g.Skip(1).Any()).SelectMany(c => c);
        //                foreach (var dupe in dupes)
        //                {
        //                    _dispatcher.Invoke(() => level2Items.Remove(dupe));
        //                }
        //            }
        //            if (level2Items.Count == 0) return;
        //            var max = level2Items.Select(item => item.Quantity).Max();
        //            foreach (var item in level2Items)
        //            {
        //                item.Percentage = (double)item.Quantity / max;
        //            }
        //            NewBestBuySell?.GetInvocationList().ForEachDo(i =>
        //            {
        //                dynamic target = i.Target;
        //                if (target.Board == level2Sub.Board && target.Seccode == level2Sub.Seccode)
        //                    i.DynamicInvoke(level2Items.Last(x => x.BuySell == "sell"),
        //                        level2Items.First(x => x.BuySell == "buy"));
        //            });
        //            Level2ColChanged?.GetInvocationList().ForEachDo(i =>
        //            {
        //                var target = (IAnchor)i.Target;
        //                if (target.Board == level2Sub.Board && target.Seccode == level2Sub.Seccode)
        //                    i.DynamicInvoke(level2Items.ToList());
        //            });
        //        }
        //    }
        //}

        //private static void AddInOrder(Level2Item item, ObservableCollection<Level2Item> collection)
        //{
        //    _dispatcher.Invoke(() => collection.Add(item));
        //    var sorted = collection.OrderByDescending(x => x.Price).ToList();
        //    for (var i = 0; i < sorted.Count; i++)
        //        _dispatcher.Invoke(() => collection.Move(collection.IndexOf(sorted[i]), i));
        //}

        private static void UpdateTable(ObservableCollection<Level2Item> collection, List<Level2Item> list, string board)
        {
            //if case of having excess rows - trim the list
            list = list
                .Where(item => item.BuySell == "sell").OrderBy(x => x.Price).Take(20).Reverse()
                .Union(list.Where(item => item.BuySell == "buy").OrderByDescending(x => x.Price).Take(20))
                .ToList();
            //if empty - fill
            if (collection.Count == 0)
                foreach (var item in list)
                {
                    _dispatcher.Invoke(() => collection.Add(item));
                }
            else
            {
                if (collection.Count != list.Count)
                {
                    Debug.WriteLine($"oops {collection.Count} != {list.Count}");
                    //if does not match - equalize the two collections
                    var diff = list.Count - collection.Count;
                    //add if the new one is bigger
                    if (diff > 0)
                    {
                        for (var i = 0; i < diff; i++)
                        {
                            _dispatcher.Invoke(() =>
                            {
                                collection.Add(new Level2Item());
                            });
                        }
                    }
                    //trim if the new one is shorter
                    else
                    {
                        for (var i = 0; i < -diff; i++)
                        {
                            _dispatcher.Invoke(() =>
                            {
                                collection.RemoveAt(collection.Count - 1);
                            });
                        }
                    }    
                }
                for (var i = 0; i < collection.Count; i++)
                {
                    //no need to replace if the rows are identical
                    if (collection[i].Price != list[i].Price || collection[i].Quantity != list[i].Quantity ||
                        collection[i].Source != list[i].Source)
                        _dispatcher.Invoke(() => collection[i] = list[i]);
                }
            }
        }
    }
}