using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.ViewModels;
using Inside_MMA.Views;
using SciChart.Core.Extensions;

namespace Inside_MMA.Models.Alerts
{
    //for XmlSerializer to serialize child types, include them here
    [XmlInclude(typeof(EqualsSizeAlert))]
    [XmlInclude(typeof(GreaterThanPriceAlert))]
    [XmlInclude(typeof(GreaterThanSizeAlert))]
    [XmlInclude(typeof(SmallerThanPriceAlert))]
    [XmlInclude(typeof(TrueAlert))]
    [XmlInclude(typeof(GreaterThanEatenSize))]
    [XmlInclude(typeof(GreaterThanDeltaOIAlert))]

    public class BaseAlert : INotifyPropertyChanged
    {
        //common fields for all types of alerts
        private string _name;
        private bool _initialized;
        protected DateTime Time;
        private string _board;
        private string _seccode;
        private bool _active;
        private string _type;

        public bool Active
        {
            get { return _active; }
            set
            {
                if (value == _active) return;
                _active = value;
                if (_active)
                {
                    if (MainWindowViewModel.IsConnected)
                        Initialize();
                }
                else
                    Uninitialize();
                OnPropertyChanged();
                MainWindowViewModel.AlertsViewModel?.SaveAlerts();
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }
        public string Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged();
            }
        }
        public string Board
        {
            get { return _board; }
            set
            {
                if (value == _board) return;
                _board = value;
                OnPropertyChanged();
            }
        }
        public string Seccode
        {
            get { return _seccode; }
            set
            {
                if (value == _seccode) return;
                _seccode = value;
                OnPropertyChanged();

                if (this is IPriceAlert)
                {
                    try
                    {
                        ((IPriceAlert)this).Price = TickDataHandler.TickList.First(x => x.Board == Board && x.Seccode == Seccode).TradeItems.First()
                            .Price;
                    }
                    catch
                    {
                    }
                }
            }
        }
        public BaseAlert()
        {
            //Task.Run(() =>
            //{
            //    Thread.Sleep(5000);
            ////    wait until properties are set and connection is established
            //    SpinWait.SpinUntil(() => Board != null && Seccode != null && MainWindowViewModel.IsConnected);
            ////    wait until initial subscribtions are made


            //    Initialize();
            //});
        }

        //~BaseAlert()
        //{
        //    Debug.WriteLine(GetType() + " disposed");
        //}
        //initializing enables an alert (subs to events)
        public void Initialize()
        {
            if (Board == "MCT")
                Time = DateTime.UtcNow;
            else
                Time = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

            var tradeItems = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);
            tradeItems.CollectionChanged += TradeItemsOnCollectionChanged;
            OnInitialize();
            _initialized = true;
        }
        //extra initializition work (override in child class)
        protected virtual void OnInitialize() { }
        //disable an alert (unsub from events)
        public void Uninitialize()
        {
            if (!_initialized) return;
            var tradeItems = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);
            tradeItems.CollectionChanged -= TradeItemsOnCollectionChanged;
            OnUninitialize();
        }
        //extra unitialization work (override in child class)
        protected virtual void OnUninitialize() { }
        //this method handles new data coming from the alltrades table
        //override in child class
        protected virtual void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
        //displays a message box showing board, seccode, size
        protected void ShowAlertOnSize(string board, string seccode, int size)
        {
            new AlertMessage(board, seccode, size.ToString()) { ShowActivated = false }.Show();
        }
        //displays a message box showing board, seccode, price
        protected void ShowAlertOnPrice(string board, string seccode, double price)
        {
            new AlertMessage(board, seccode, price.ToString("F2")) { ShowActivated = false }.Show();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //protected void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    var type = GetType();
        //    if (type == typeof(EqualsSizeAlert))
        //        foreach (TradeItem trade in e.NewItems)
        //        {
        //            if (DateTime.Parse(trade.Time) < _time) continue;
        //            if (trade.Quantity == ((EqualsSizeAlert)this).Size)
        //                ShowAlertOnSize(Board, Seccode, ((EqualsSizeAlert)this).Size);
        //        }
        //    if (type == typeof(GreaterThanSizeAlert))
        //        foreach (TradeItem trade in e.NewItems)
        //        {
        //            if (DateTime.Parse(trade.Time) < _time) continue;
        //            if (trade.Quantity > ((GreaterThanSizeAlert)this).Size)
        //                ShowAlertOnSize(Board, Seccode, trade.Quantity);
        //        }
        //    if (type == typeof(GreaterThanPriceAlert))
        //        foreach (TradeItem trade in e.NewItems)
        //        {
        //            if (DateTime.Parse(trade.Time) < _time) continue;
        //            if (trade.Price >= ((GreaterThanPriceAlert)this).Price)
        //                ShowAlertOnPrice(Board, Seccode, trade.Price);
        //        }
        //    if (type == typeof(SmallerThanPriceAlert))
        //        foreach (TradeItem trade in e.NewItems)
        //        {
        //            if (DateTime.Parse(trade.Time) < _time) continue;
        //            if (trade.Price <= ((SmallerThanPriceAlert)this).Price)
        //                ShowAlertOnPrice(Board, Seccode, trade.Price);
        //        }
        //    if (type == typeof(TrueAlert))
        //    {
        //        var thisAlert = (TrueAlert)this;
        //        if (!thisAlert.Triggered) return;
        //        var trades = e.NewItems.Cast<TradeItem>();
        //        trades = thisAlert.BuySell == "Buy"
        //            ? trades.Where(t => t.Buysell == "S" && t.Price == thisAlert.Price)
        //            : trades.Where(t => t.Buysell == "B" && t.Price == thisAlert.Price);
        //        trades.ForEachDo(t => thisAlert.LastSize -= t.Quantity);
        //        if (thisAlert.LastSize < (1 - (float)thisAlert.Percentage / 100) * thisAlert.InitialSize)
        //        {
        //            ShowTrueAlert(Board, Seccode, thisAlert.LastSize, thisAlert.InitialSize, thisAlert.Price, thisAlert.BuySell == "Buy" ? "bought" : "sold");
        //            thisAlert.Triggered = false;
        //            thisAlert.InitialSize = thisAlert.LastSize;
        //        }
        //    }
        //    if (type == typeof(LogBookAlert))
        //    {
        //        var thisAlert = (LogBookAlert)this;
        //        switch (thisAlert.Buysell)
        //        {
        //            case "B":
        //                foreach (TradeItem trade in e.NewItems)
        //                {
        //                    if (trade.Price < thisAlert.Price)
        //                    {
        //                        UninitializeLogBookAlert(thisAlert,
        //                            $"Ice Buy on {thisAlert.Price} of size {thisAlert.Size} has lost ");
        //                        ShowLogBookAlertResult("Buy", thisAlert.Price, thisAlert.Size, "lost");
        //                    }
        //                    if (trade.Price >= thisAlert.Price + thisAlert.Delta)
        //                    {
        //                        UninitializeLogBookAlert(thisAlert,
        //                            $"Ice Buy on {thisAlert.Price} of size {thisAlert.Size} has won ");
        //                        ShowLogBookAlertResult("Buy", thisAlert.Price, thisAlert.Size, "won");
        //                    }
        //                }
        //                break;
        //            case "S":
        //                foreach (TradeItem trade in e.NewItems)
        //                {
        //                    if (trade.Price > thisAlert.Price)
        //                    {
        //                        UninitializeLogBookAlert(thisAlert,
        //                            $"Ice Sell on {thisAlert.Price} of size {thisAlert.Size} has lost ");
        //                        ShowLogBookAlertResult("Sell", thisAlert.Price, thisAlert.Size, "lost");
        //                    }
        //                    if (trade.Price <= thisAlert.Price - thisAlert.Delta)
        //                    {
        //                        UninitializeLogBookAlert(thisAlert,
        //                            $"Ice Sell on {thisAlert.Price} of size {thisAlert.Size} has won ");
        //                        ShowLogBookAlertResult("Sell", thisAlert.Price, thisAlert.Size, "won");
        //                    }
        //                }
        //                break;
        //        }
        //    }
        //}

    }
}