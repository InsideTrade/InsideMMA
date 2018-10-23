using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public class SpreadViewModel : INotifyPropertyChanged, IAnchor
    {
        private static Dispatcher Dispatcher => Application.Current.Dispatcher;
        private ObservableCollection<SpreadItem> _spreadItems = new ObservableCollection<SpreadItem>();
        private bool _isAnchorEnabled;
        private string _seccode;

        public ObservableCollection<SpreadItem> SpreadItems
        {
            get { return _spreadItems; }
            set
            {
                if (Equals(value, _spreadItems)) return;
                _spreadItems = value;
                OnPropertyChanged();
            }
        }
        public string Board { get; set; }

        public string Seccode
        {
            get { return _seccode; }
            set
            {
                if (value == _seccode) return;
                _seccode = value;
                OnPropertyChanged();
            }
        }

        

        public SpreadViewModel(string board, string seccode)
        {
            Board = board;
            Seccode = seccode;
            Level2DataHandler.AddLevel2Subscribtion(board, seccode);
            Level2DataHandler.NewBestBuySell += OnNewBestBuySell;
        }

        private void OnNewBestBuySell(Level2Item bestsell, Level2Item bestbuy)
        {
            if (SpreadItems.Count == 0)
                Dispatcher.Invoke(() => SpreadItems.Insert(0,
                    new SpreadItem
                    {
                        Bid = bestsell.Price,
                        Ask = bestbuy.Price,
                        BSize = bestsell.Quantity,
                        ASize = bestbuy.Quantity
                    }));
            else
            {
                var item = new SpreadItem
                {
                    Bid = bestsell.Price,
                    Ask = bestbuy.Price,
                    BSize = bestsell.Quantity,
                    ASize = bestbuy.Quantity
                };
                if (item.CompareTo(SpreadItems.First())) return;
                Dispatcher.Invoke(() => SpreadItems.Insert(0, item));
            }
        }
        public bool IsAnchorEnabled
        {
            get { return _isAnchorEnabled; }
            set
            {
                if (value == _isAnchorEnabled) return;
                _isAnchorEnabled = value;
                if (_isAnchorEnabled)
                    AnchoredWindows.Add(this);
                else
                    AnchoredWindows.RemoveIfContains(this);
                OnPropertyChanged();
            }
        }

        public List<IAnchor> AnchoredWindows => Dispatcher
            .Invoke(() => (MainWindowViewModel) Application.Current.MainWindow.DataContext).AnchoredWindows;
        public void SetSecurity(string board, string seccode)
        {
            Level2DataHandler.NewBestBuySell -= OnNewBestBuySell;
            Dispatcher.Invoke(() => SpreadItems.Clear());
            Board = board;
            Seccode = seccode;
            Level2DataHandler.NewBestBuySell += OnNewBestBuySell;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}