using System;
using System.Collections.Specialized;
using Inside_MMA.Views;

namespace Inside_MMA.Models.Alerts
{
    public class GreaterThanEatenSize : BaseAlert
    {
        private int _size;

        public int Size
        {
            get { return _size; }
            set
            {
                if (value == _size) return;
                _size = value;
                OnPropertyChanged();
            }
        }

        public GreaterThanEatenSize() { }

        protected override void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (TradeItem trade in e.NewItems)
            {
                if (DateTime.Parse(trade.Time) < Time) continue;
                if (trade.Quantity >= Size && trade.IsEaten)
                    new AlertMessage(Board, Seccode, $"Size {trade.Quantity} is eaten") { ShowActivated = false }.Show();
            }
        }
    }
}