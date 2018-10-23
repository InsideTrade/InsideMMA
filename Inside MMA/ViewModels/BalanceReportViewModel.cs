using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Views;
using Trade = InsideDB.Trade;

namespace Inside_MMA.ViewModels
{
    public class BalanceReportViewModel : INotifyPropertyChanged
    {
        private DateTime? _from = DateTime.Now.Date;
        private DateTime? _to = DateTime.Now.Date.AddDays(1);
        private string _balance;
        private bool _confirmEnabled = true;
        private string _selectedUser;
        private List<Trade> _trades;

        public DateTime? From
        {
            get { return _from; }
            set
            {
                if (value.Equals(_from)) return;
                _from = value;
                SetConfirmButtonAvailability();
                OnPropertyChanged();
            }
        }
        public DateTime? To
        {
            get { return _to; }
            set
            {
                if (value.Equals(_to)) return;
                _to = value;
                SetConfirmButtonAvailability();
                OnPropertyChanged();
            }
        }

        public string Balance
        {
            get { return _balance; }
            set
            {
                if (value == _balance) return;
                _balance = value;
                OnPropertyChanged();
            }
        }

        public bool ConfirmEnabled
        {
            get { return _confirmEnabled; }
            set
            {
                if (value == _confirmEnabled) return;
                _confirmEnabled = value;
                OnPropertyChanged();
            }
        }

        public List<string> Logins => MainWindowViewModel.AdminViewModel.UsersCollection
            .Select(u => u.Login).ToList();

        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (value == _selectedUser) return;
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public List<Trade> Trades
        {
            get { return _trades; }
            set
            {
                if (Equals(value, _trades)) return;
                _trades = value;
                OnPropertyChanged();
            }
        }

        public bool ShowUsers => MainWindowViewModel.IsAdmin;
        public ICommand ConfirmCommand { get; set; }
        public ICommand ChartCommand { get; set; }
        public BalanceReportViewModel()
        {
            ConfirmCommand = new Command(arg => Confirm());
            ChartCommand = new Command(arg =>
            {
                if (Trades != null)
                    new BalanceReportChart { DataContext = new BalanceReportChartViewModel(Trades, From.Value, To.Value) }.Show();
            });
        }

        private void SetConfirmButtonAvailability()
        {
            ConfirmEnabled = _from != null && _to != null;
        }
        private void Confirm()
        {
            if (MainWindowViewModel.IsAdmin && SelectedUser != null)
                MainWindowViewModel.Hub?.Invoke("RequestBalance", SelectedUser, From.Value, To.Value);
            else
                MainWindowViewModel.Hub?.Invoke("RequestBalance", ClientInfo.InsideLogin, From, To);
        }

        public void SetBalance(List<Trade> trades)
        {
            double balance = 0;
            trades.ForEach(trade => balance += trade.Price * trade.Quantity * trade.Lotsize * (trade.Buysell == "S" ? 1 : -1));
            Balance = balance.ToString("F2");
            Trades = trades;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
