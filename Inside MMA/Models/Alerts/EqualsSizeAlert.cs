using System;
using System.Collections.Specialized;

namespace Inside_MMA.Models.Alerts
{
    public class EqualsSizeAlert : BaseAlert
    {
        private int _size = 1;
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
        public EqualsSizeAlert() { }

        protected override void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (TradeItem trade in e.NewItems)
            {
                if (DateTime.Parse(trade.Time) < Time) continue;
                if (trade.Quantity == Size)
                    ShowAlertOnSize(Board, Seccode, Size);
            }
        }
    }
}