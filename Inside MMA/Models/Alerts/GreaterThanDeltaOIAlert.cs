using System;
using System.Collections.Specialized;
using System.Linq;
using Inside_MMA.Views;

namespace Inside_MMA.Models.Alerts
{
    public class GreaterThanDeltaOIAlert : BaseAlert
    {
        private int _size;
        private bool _absolute;
        private double _min = double.MinValue;

        public bool Absolute
        {
            get { return _absolute; }
            set
            {
                if (value == _absolute) return;
                _absolute = value;
                Min = _absolute ? 0 : double.MinValue;
                OnPropertyChanged();
            }
        }

        public GreaterThanDeltaOIAlert() { }

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

        public double Min
        {
            get { return _min; }
            set
            {
                if (value.Equals(_min)) return;
                _min = value;
                OnPropertyChanged();
            }
        }

        protected override void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int delta;
            foreach (TradeItem trade in e.NewItems)
            {
                if (DateTime.Parse(trade.Time) < Time) continue;
                delta = int.Parse(trade.InterestDelta.Split(',').Last());
                if (Absolute)
                {
                    if (delta >= Math.Abs(Size))
                        new AlertMessage(Board, Seccode, $"Delta OI {delta}") {ShowActivated = false}.Show();
                }
                else
                {
                    if (delta >= Size)
                        new AlertMessage(Board, Seccode, $"Delta OI {delta}") { ShowActivated = false }.Show();
                }
            }
        }
    }
}