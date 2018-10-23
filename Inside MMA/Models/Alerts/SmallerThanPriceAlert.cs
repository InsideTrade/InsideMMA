using System;
using System.Collections.Specialized;
using System.Linq;
using Inside_MMA.DataHandlers;

namespace Inside_MMA.Models.Alerts
{
    public class SmallerThanPriceAlert : BaseAlert, IPriceAlert
    {
        private double _price;

        public double Price
        {
            get { return _price; }
            set
            {
                if (value.Equals(_price)) return;
                _price = value;
                OnPropertyChanged();
            }
        }
        public SmallerThanPriceAlert() { }
        public SmallerThanPriceAlert(string board, string seccode)
        {
            Board = board;
            Seccode = seccode;
            try
            {
                Price = TickDataHandler.TickList.First(x => x.Board == Board && x.Seccode == Seccode).TradeItems.First()
                    .Price;
            }
            catch
            {
            }
            
        }

        protected override void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (TradeItem trade in e.NewItems)
            {
                if (DateTime.Parse(trade.Time) < Time) continue;
                if (trade.Price <= Price)
                    ShowAlertOnPrice(Board, Seccode, trade.Price);
            }
        }
    }
}