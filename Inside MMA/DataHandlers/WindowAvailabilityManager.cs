using System.ComponentModel;
using System.Runtime.CompilerServices;
using InsideDB;
using Inside_MMA.Annotations;
using Newtonsoft.Json.Linq;

namespace Inside_MMA.DataHandlers
{
    //This class is responsible for setting windows' availability
    //Individual features can be disabled by setting these properties to 'false'
    public class WindowAvailabilityManager : INotifyPropertyChanged
    {
        private bool _tradesCounterEnabled;
        private bool _logbookEnabled;
        private bool _allTradesEnabled;
        private bool _allTradesProEnabled;
        private bool _level2Enabled;
        private bool _chartEnabled;
        private bool _alertsEnabled;
        private bool _tradingEnabled;
        private bool _fastOrderEnabled;
        private bool _cartEnabled;
        private bool _settingsEnabled;

        public bool TradesCounterEnabled
        {
            get { return _tradesCounterEnabled; }
            set
            {
                if (value == _tradesCounterEnabled) return;
                _tradesCounterEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool LogbookEnabled
        {
            get { return _logbookEnabled; }
            set
            {
                if (value == _logbookEnabled) return;
                _logbookEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool AllTradesEnabled
        {
            get { return _allTradesEnabled; }
            set
            {
                if (value == _allTradesEnabled) return;
                _allTradesEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool AllTradesProEnabled
        {
            get { return _allTradesProEnabled; }
            set
            {
                if (value == _allTradesProEnabled) return;
                _allTradesProEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool Level2Enabled
        {
            get { return _level2Enabled; }
            set
            {
                if (value == _level2Enabled) return;
                _level2Enabled = value;
                OnPropertyChanged();
            }
        }

        public bool ChartEnabled
        {
            get { return _chartEnabled; }
            set
            {
                if (value == _chartEnabled) return;
                _chartEnabled = value;
                OnPropertyChanged();
            }
        }
        

        public bool AlertsEnabled
        {
            get { return _alertsEnabled; }
            set
            {
                if (value == _alertsEnabled) return;
                _alertsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool TradingEnabled
        {
            get { return _tradingEnabled; }
            set
            {
                if (value == _tradingEnabled) return;
                _tradingEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool FastOrderEnabled
        {
            get { return _fastOrderEnabled; }
            set
            {
                if (value == _fastOrderEnabled) return;
                _fastOrderEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool CartEnabled
        {
            get { return _cartEnabled; }
            set
            {
                if (value == _cartEnabled) return;
                _cartEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool SettingsEnabled
        {
            get { return _settingsEnabled; }
            set
            {
                if (value == _settingsEnabled) return;
                _settingsEnabled = value;
                OnPropertyChanged();
            }
        }

        public void SelectWindows(dynamic windows)
        {
            UserWindows userWindows;
            if (windows is JObject)
                userWindows = (UserWindows)(windows as JObject).ToObject(typeof(UserWindows));
            else
                userWindows = windows;
            AlertsEnabled = userWindows.Alerts;
            AllTradesProEnabled = userWindows.AllTradesPro;
            AllTradesEnabled = userWindows.AllTrades;
            ChartEnabled = userWindows.Chart;
            TradesCounterEnabled = userWindows.Counter;
            Level2Enabled = userWindows.L2;
            LogbookEnabled = userWindows.Logbook;
            TradingEnabled = userWindows.Trading;
            FastOrderEnabled = userWindows.FastOrder;
            SettingsEnabled = true;
            CartEnabled = TradingEnabled;
        }

        public void SetFreeVersion()
        {
            AlertsEnabled = false;
            AllTradesProEnabled = false;
            AllTradesEnabled = true;
            ChartEnabled = true;
            TradesCounterEnabled = false;
            Level2Enabled = true;
            LogbookEnabled = false;
            TradingEnabled = true;
            FastOrderEnabled = false;
            CartEnabled = false;
            SettingsEnabled = false;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}