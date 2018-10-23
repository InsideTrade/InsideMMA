using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models.Alerts;
using Inside_MMA.ViewModels;

namespace Inside_MMA.Models
{
    namespace Inside_MMA.Models
    {
        public class LogBookItem : INotifyPropertyChanged
        {
            private int _maxSize;
            private int _currentSize;
            private double _price;
            private string _buysell;
            private int _buy;
            private int _sell;
            private int _maxBuy;
            private int _maxSell;
            private bool _isIceBuy;
            private bool _isIceSell;
            
            [XmlIgnore]
            public LogBookViewModel Vm;

            private int _delta;
            private bool _iceEaten;
            private bool _iceEatenSell;
            private int _deltaState;

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

            public int MaxSize
            {
                get { return _maxSize; }
                set
                {
                    if (value == _maxSize) return;
                    _maxSize = value;
                    OnPropertyChanged();
                }
            }

            public int CurrentSize
            {
                get { return _currentSize; }
                set
                {
                    if (value == _currentSize) return;
                    _currentSize = value;
                    OnPropertyChanged();
                }
            }

            public string Buysell
            {
                get
                {
                    return _buysell;
                }

                set
                {
                    if (value == _buysell) return;
                    _buysell = value;
                    OnPropertyChanged();
                }
            }

            public int Buy
            {
                get { return _buy; }
                set
                {
                    if (value == _buy) return;
                    _buy = value;
                    IsIceSell = MaxSell < Buy / Vm.Coef && MaxSell != 0 && Buy >= Vm.Size;
                    Delta = Buy - Sell;
                    RecalculateIceEaten();
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
                    IsIceBuy = MaxBuy < Sell / Vm.Coef && MaxBuy != 0 && Sell >= Vm.Size;
                    Delta = Buy - Sell;
                    RecalculateIceEaten();
                    OnPropertyChanged();
                }
            }

            public int MaxBuy
            {
                get { return _maxBuy; }
                set
                {
                    if (value == _maxBuy) return;
                    _maxBuy = value;
                    IsIceBuy = MaxBuy < Sell / Vm.Coef && MaxBuy != 0 && Sell >= Vm.Size;
                    RecalculateIceEaten();
                    OnPropertyChanged();
                }
            }

            public int MaxSell
            {
                get { return _maxSell; }
                set
                {
                    if (value == _maxSell) return;
                    _maxSell = value;
                    IsIceSell = MaxSell < Buy / Vm.Coef && MaxSell != 0 && Buy >= Vm.Size;
                    RecalculateIceEaten();
                    OnPropertyChanged();
                }
            }

            public bool IsIceBuy //надо точно написать на покупку или продажу. пока это на ПОКУПКУ , если понял правильно
            {
                get { return _isIceBuy; }
                set
                {
                    if (value == _isIceBuy) return;
                    _isIceBuy = value;
                    if (_isIceBuy && Vm.IsAlerting)
                    {
                        //TODO check if this is correct
                        Vm.Alert("IceBuy", Price, MaxBuy, Sell);
                        var lbAlert = new LogBookAlert(Vm, "B", Price, Sell, Vm.TriggerDelta);
                        
                        lbAlert.Initialize();
                        Application.Current.Dispatcher.Invoke(() => Vm.Alerts.Add(lbAlert));
                    }
                    OnPropertyChanged();
                }
            }

            public bool IsIceSell // this ice na продажу
            {
                get { return _isIceSell; }
                set
                {
                    if (value == _isIceSell) return;
                    _isIceSell = value;
                    if (_isIceSell && Vm.IsAlerting)
                    {                        
                        //TODO check if this is correct
                        Vm.Alert("IceSell", Price, MaxSell, Buy);
                        var lbAlert = new LogBookAlert(Vm, "S", Price, Buy, Vm.TriggerDelta);
                        lbAlert.Initialize();
                        Application.Current.Dispatcher.Invoke(() => Vm.Alerts.Add(lbAlert));
                    }
                        
                    OnPropertyChanged();
                }
            }

            //public Color Color
            //{
            //    get { return _color; }
            //    set
            //    {
            //        if (Equals(value, _color)) return;
            //        _color = value;
            //        OnPropertyChanged();
            //    }
            //}

            public int Delta
            {
                get { return _delta; }
                set
                {
                    if (value == _delta) return;
                    _delta = value;
                    if (_delta > 0)
                        DeltaState = 1;
                    if (_delta < 0)
                        DeltaState = 2;
                    OnPropertyChanged();
                }
            }

            public bool IceEatenBuy
            {
                get { return _iceEaten; }
                set
                {
                    if (value == _iceEaten) return;
                    _iceEaten = value;
                    OnPropertyChanged();
                }
            }

            public bool IceEatenSell
            {
                get { return _iceEatenSell; }
                set
                {
                    if (value == _iceEatenSell) return;
                    _iceEatenSell = value;
                    OnPropertyChanged();
                }
            }

            //controls the background of the delta cell
            //1 - green
            //2 - red
            public int DeltaState
            {
                get { return _deltaState; }
                set
                {
                    if (value == _deltaState) return;
                    _deltaState = value;
                    OnPropertyChanged();
                }
            }

            public LogBookItem() { }
            public LogBookItem(LogBookViewModel vm)
            {
                Vm = vm;
            }

            public void RecalculateIceEaten()
            {
                IceEatenBuy = Buy != 0 && Buy >= MaxSell;
                IceEatenSell = Sell != 0 && Sell >= MaxBuy;
            }
            public void RecalculateIce()
            {
                IsIceBuy = MaxBuy < Sell / Vm.Coef && MaxBuy != 0 && Sell >= Vm.Size;
                IsIceSell = MaxSell < Buy / Vm.Coef && MaxSell != 0 && Buy >= Vm.Size;
            }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
