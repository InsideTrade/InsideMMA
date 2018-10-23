using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public class AllTradesSimpleViewModel: RememberPlacement, IAnchor
    {
        private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;
        private static readonly object Lock = new object();
        private ObservableCollection<TradeItem> _allTrades;
        private bool _isAnchorEnabled;

        public ObservableCollection<TradeItem> AllTrades
        {
            get { return _allTrades; }
            set
            {
                if (Equals(value, _allTrades)) return;
                _allTrades = value;
                OnPropertyChanged();
            }
        }
        public ICommand Closing { get; set; }
        public AllTradesSimpleViewModel(string board, string seccode, Window window, int id = 0)
        {
            Window = window;
            Closing = new Command(arg => WindowClosing());
            Board = board;
            Seccode = seccode;
            if (Board == "MCT")
                Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            AllTrades = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
        }

        private void WindowClosing()
        {
            AnchoredWindows.RemoveIfContains(this);
            UnsubscribeFromWindowEvents();
            CloseWindow();
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
                UpdateWindowBinding(IsAnchorEnabled);
                OnPropertyChanged();
            }
        }

        public List<IAnchor> AnchoredWindows => Application.Current.Dispatcher
            .Invoke(() => (MainWindowViewModel)Application.Current.MainWindow.DataContext).AnchoredWindows;
        public void SetSecurity(string board, string seccode)
        {
            if (board == Board && seccode == Seccode) return;
            Board = board;
            Seccode = seccode;
            if (Board == "MCT")
                Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            AllTrades = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);
            UpdateWindowInstrument();
        }
    }
}