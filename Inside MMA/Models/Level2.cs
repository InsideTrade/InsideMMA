using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Inside_MMA.Annotations;

namespace Inside_MMA.Models
{
    public class Level2Item : INotifyPropertyChanged
    {
        private int _quantity;
        private double _price;
        private double _percentage;
        private string _source;
        private string _buySell;

        public string BuySell
        {
            get { return _buySell; }
            set
            {
                if (value == _buySell) return;
                _buySell = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (value == _quantity) return;
                _quantity = value;
                OnPropertyChanged();
            }
        }

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

        public double Percentage
        {
            get { return _percentage; }
            set
            {
                if (value.Equals(_percentage)) return;
                _percentage = value;
                OnPropertyChanged();
            }
        }

        public string Source
        {
            get { return _source; }
            set
            {
                if (value == _source) return;
                _source = value;
                OnPropertyChanged();
            }
        }

        public int Buy { get; set; }
        public int Sell { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
    [XmlType("quote")]
    public class Quote
    {
        [XmlElement("board")]
        public string Board { get; set; }

        [XmlElement("seccode")]
        public string Seccode { get; set; }

        [XmlElement("price")]
        public double Price { get; set; }

        [XmlElement("yield")]
        public int Yield { get; set; }

        [XmlElement("sell")]
        public int Sell { get; set; }

        [XmlElement("buy")]
        public int Buy { get; set; }

        [XmlAttribute("secid")]
        public int Secid { get; set; }

        [XmlElement("source")]
        public string Source { get; set; }
    }
}
