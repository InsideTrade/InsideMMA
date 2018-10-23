using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.ViewModels;
using Inside_MMA.Views;

namespace Inside_MMA.Models
{
    public class Alert : INotifyPropertyChanged
    {
        private bool _isEdited;
        private ObservableCollection<string> _seccodes = new ObservableCollection<string>();
        private string _board;
        private int _size;
        private bool _isSizeValid;
        private bool _isBoardValid;
        private bool _isSeccodeValid;
        private string _seccode;
        private DateTime _time;

        public string Board
        {
            get { return _board; }
            set
            {
                if (value == _board) return;
                _board = value;
                Seccodes.Clear();

                foreach (var trade in MainWindowViewModel.SecVm.SecList
                    .Where(i => i.Board == _board)
                    .Select(x => x.Seccode).Distinct().ToList())
                {
                    Seccodes.Add(trade);
                }

                OnPropertyChanged();
                IsBoardValid = !string.IsNullOrEmpty(_board);
            }
        }

        public string Seccode
        {
            get { return _seccode; }
            set
            {
                _seccode = value;
                IsSeccodeValid = !string.IsNullOrEmpty(_seccode);
            }
        }

        public int Size
        {
            get { return _size; }
            set
            {
                if (value == _size) return;
                _size = value;
                OnPropertyChanged();

                IsSizeValid = _size != 0;
                //IsSizeValid = int.TryParse(_size, out res);
            }
        }

        public ObservableCollection<string> Seccodes
        {
            get { return _seccodes; }
            set
            {
                _seccodes = value;
                OnPropertyChanged();
            }
        }

        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                _isEdited = value;
                OnPropertyChanged();
            }
        }

        public bool IsSizeValid
        {
            get { return _isSizeValid; }
            set
            {
                if (value == _isSizeValid) return;
                _isSizeValid = value;
                OnPropertyChanged();
            }
        }

        public bool IsBoardValid
        {
            get { return _isBoardValid; }
            set
            {
                if (value == _isBoardValid) return;
                _isBoardValid = value;
                OnPropertyChanged();
            }
        }

        public bool IsSeccodeValid
        {
            get { return _isSeccodeValid; }
            set
            {
                if (value == _isSeccodeValid) return;
                _isSeccodeValid = value;
                OnPropertyChanged();
            }
        }

        public ICommand Save { get; set; }
        public bool Initialized { get; set; }

        public Alert()
        {
            IsEdited = true;
            _time = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
        }

        public void Initialize()
        {
            var tradeItems = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode); 
            tradeItems.CollectionChanged += TradeItemsOnCollectionChanged;
            Initialized = true;
        }

        public void Uninitialize()
        {
            if (!Initialized) return;
            var tradeItems = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);
            tradeItems.CollectionChanged -= TradeItemsOnCollectionChanged;
        }

        private void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var size = /*int.Parse(Size);*/Size;
            foreach (TradeItem trade in e.NewItems)
            {
                if (DateTime.Parse(trade.Time) < _time) continue;
                if (trade.Quantity == size)
                    ShowAlertOnSize(Board, Seccode, size);
            }
        }
        private void ShowAlertOnSize(string board, string seccode, int size)
        {
            new AlertMessage(board, seccode, size.ToString()){ShowActivated = false}.Show();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}