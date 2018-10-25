using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Inside_MMA.Annotations;

namespace Inside_MMA.Models
{
    public class AllTradesCounterItem : INotifyPropertyChanged
    {
        private int _quantity;
        private int _count;
        private int _buy;
        private int _sell;
        private int _delta;
        private double _percent;
        private double _balance;
        private string _buysell;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
        public int Buy
        {
            get
            {
                return _buy;
            }

            set
            {
                if (value == _buy) return;
                _buy = value;
                OnPropertyChanged();
            }
        }
        public int Sell
        {
            get { return _sell; }
            set
            {
                if (value == _sell) return;
                _sell = value;
                OnPropertyChanged();
            }
        }
        public int Delta
        {
            get { return _delta; }
            set
            {
                if (value == _delta) return;
                _delta = value;
                OnPropertyChanged();
            }
        }
        public double Percent
        {
            get => _percent;
            set
            {
                if (value == _percent) return;
                _percent = value;
                OnPropertyChanged();
            }
        }
        public double Balance
        {
            get { return _balance; }
            set
            {
                if (value.Equals(_balance)) return;
                _balance = value;
                OnPropertyChanged();
            }
        }

        public string Buysell
        {
            get { return _buysell; }
            set
            {
                if (value == _buysell) return;
                _buysell = value;
                OnPropertyChanged();
            }
        }

        public AllTradesCounterItem(int quantity, int count, int buy, int sell, int delta, double percent)
        {
            Quantity = quantity;
            Count = count;
            Buy = buy;
            Sell = sell;
            Delta = delta;
            Percent = percent;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
