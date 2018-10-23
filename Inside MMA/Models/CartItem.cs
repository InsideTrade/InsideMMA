using System.ComponentModel;
using System.Runtime.CompilerServices;
using Inside_MMA.Annotations;

namespace Inside_MMA.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private string _savedPrice;
        private bool _mkt;
        private string _price;
        private string _buySell;
        public string Board { get; set; }
        public string Seccode { get; set; }
        public string Client { get; set; }
        public string Size { get; set; }

        public bool Mkt
        {
            get { return _mkt; }
            set
            {
                if (value == _mkt) return;
                _mkt = value;
                if (_mkt)
                {
                    _savedPrice = Price;
                    Price = "-";
                }
                else
                    Price = _savedPrice;
                OnPropertyChanged();
            }
        }

        public string Price
        {
            get { return _price; }
            set
            {
                if (value == _price) return;
                _price = value;
                OnPropertyChanged();
            }
        }

        public string Union { get; set; }

        public string BuySell
        {
            get { return _buySell; }
            set
            {
                _buySell = value;
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
}