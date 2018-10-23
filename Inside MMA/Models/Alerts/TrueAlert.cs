using System.Collections.Specialized;
using System.Linq;
using Inside_MMA.DataHandlers;
using Inside_MMA.Views;
using SciChart.Core.Extensions;

namespace Inside_MMA.Models.Alerts
{
    public class TrueAlert : BaseAlert
    {
        public int Size { get; set; }
        public int Percentage { get; set; } = 10;
        public string BuySell { get; set; }
        public int InitialSize;
        public int LastSize;
        private bool _triggered;
        public double Price;
        //set to true to start counting trades
        public bool Triggered
        {
            get { return _triggered; }
            set
            {
                _triggered = value;
                if (!_triggered)
                    LastSize = InitialSize;
            }
        }
        public TrueAlert()
        {
        }
        public TrueAlert(string board, string seccode, string name)
        {
            Board = board;
            Seccode = seccode;
            Name = name;
        }
        private void OnNewBestBuySell(Level2Item bestsell, Level2Item bestbuy)
        {
            //if (Triggered) return;
            switch (BuySell)
            {
                case null:
                    return;
                case "Buy":
                    if (Price != bestbuy.Price)
                    {
                        Price = bestbuy.Price;
                        Triggered = false;
                    }
                    //else if (!Triggered)
                    Triggered = bestbuy.Quantity > Size;
                    if (Triggered)
                        InitialSize = LastSize = bestbuy.Quantity;
                    break;
                case "Sell":
                    if (Price != bestsell.Price)
                    {
                        Price = bestsell.Price;
                        Triggered = false;
                    }
                    //else if (!Triggered)
                    Triggered = bestsell.Quantity > Size;
                    if (Triggered)
                        InitialSize = LastSize = bestsell.Quantity;
                    break;
            }
            
        }
        protected override void OnInitialize()
        {
            Level2DataHandler.NewBestBuySell += OnNewBestBuySell;
        }
        protected override void OnUninitialize()
        {
            Level2DataHandler.NewBestBuySell -= OnNewBestBuySell;
        }
        protected override void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Triggered) return;
            var trades = e.NewItems.Cast<TradeItem>();
            trades = BuySell == "Buy"
                ? trades.Where(t => t.Buysell == "S" && t.Price == Price)
                : trades.Where(t => t.Buysell == "B" && t.Price == Price);
            trades.ForEachDo(t => LastSize -= t.Quantity);
            if (LastSize < (1 - (float)Percentage / 100) * InitialSize)
            {
                ShowTrueAlert(Board, Seccode, LastSize, InitialSize, Price, BuySell == "Buy" ? "bought" : "sold");
                Triggered = false;
                InitialSize = LastSize;
            }
        }
        protected void ShowTrueAlert(string board, string seccode, int lastSize, int initialSize, double price, string direction)
        {
            new AlertMessage(board, seccode, $"{direction} at {price}\r\n{initialSize} -> {lastSize}", 1000, "True\r\n") { ShowActivated = false }.Show();
        }

    }
}