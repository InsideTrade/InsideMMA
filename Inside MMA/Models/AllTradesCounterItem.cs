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
        private int _buyLMT;
        private int _sellLMT;
        private int _deltaLMT;
        private double _balanceLMT;
        private int _countLMT;

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
        public int BuyLMT
        {
            get { return _buyLMT; }

            set
            {
                if (value == _buyLMT) return;
                _buyLMT = value;
                OnPropertyChanged();
            }
        }
        public int SellLMT
        {
            get { return _sellLMT; }
            set
            {
                if (value == _sellLMT) return;
                _sellLMT = value;
                OnPropertyChanged();
            }
        }

        public int DeltaLMT
        {
            get => _deltaLMT;
            set
            {
                if (value == _deltaLMT) return;
                _deltaLMT = value;
                OnPropertyChanged();
            }
        }

        public double BalanceLMT
        {
            get => _balanceLMT;
            set
            {
                if (value == _balanceLMT) return;
                _balanceLMT = value;
                OnPropertyChanged();
            }
        }
        
        public int CountLMT
        {
            get => _countLMT;
            set
            {
                if (value == _countLMT) return;
                _countLMT = value;
                OnPropertyChanged();
            }
        }

        public AllTradesCounterItem(int quantity, int count, int buy, int sell, int delta, double percent, int countLMT = 0, int buyLMT = 0, int sellLMT = 0, int deltaLMT = 0, double balanceLMT = 0.0)
        {
            Quantity = quantity;
            Count = count;
            Buy = buy;
            Sell = sell;
            Delta = delta;
            Percent = percent;
            BuyLMT = buyLMT;
            SellLMT = sellLMT;
            DeltaLMT = deltaLMT;
            BalanceLMT = balanceLMT;
            CountLMT = countLMT;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
