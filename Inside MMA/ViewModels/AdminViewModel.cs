using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using InsideDB;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.AspNet.SignalR.Client;
using Inside_MMA.Annotations;
using Inside_MMA.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SciChart.Core.Extensions;


namespace Inside_MMA.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {


        public ObservableCollection<User> UsersCollection
        {
            get { return _usersCollection; }
            set
            {
                if (Equals(value, _usersCollection)) return;
                _usersCollection = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (Equals(value, _selectedUser)) return;
                _selectedUser = value;
                if (SelectedUser != null)
                    OptionEnabled = true;
                else
                    OptionEnabled = false;
                
                OnPropertyChanged();
            }
        }

        public bool OptionEnabled
        {
            get { return _optionEnabled; }
            set
            {
                if (value == _optionEnabled) return;
                _optionEnabled = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<User> _usersCollection = new ObservableCollection<User>();
        private User _selectedUser;
        private bool _optionEnabled;
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SleepCommand { get; set; }
        public ICommand UnsleepCommand { get; set; }
        public ICommand WindowsCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand BroadcastMessage { get; set; }
        public ICommand EditSleepThresholdCommand { get; set; }
        public ICommand EditAutoSleepCommand { get; set; }
        public ICommand EditProfitControlCommand { get; set; }
        public ICommand EditProfitLimitCommand { get; set; }
        public ICommand CancelOrdersCommand { get; set; }
        public ICommand CancelStopordersCommand { get; set; }
        public ICommand CloseBalanceCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        

        private IHubProxy _hub;
        private IDialogCoordinator _dialog;
        private static Dispatcher _dispatcher = Application.Current.Dispatcher;
        public AdminViewModel(IHubProxy hub, IDialogCoordinator dialog)
        {
            _dialog = dialog;
            AddCommand = new Command(arg => Add());
            EditCommand = new Command(arg => Edit());
            DeleteCommand = new Command(arg => Delete());
            SleepCommand = new Command(arg => Sleep());
            UnsleepCommand = new Command(arg => Unsleep());
            WindowsCommand = new Command(arg => SelectWindows());
            DisconnectCommand = new Command(arg => Disconnect());
            BroadcastMessage = new Command(arg => BroadcastMsg());
            EditSleepThresholdCommand = new Command(arg => EditSleepThreshold());
            EditAutoSleepCommand = new Command(arg => EditAutoSleep());
            EditProfitControlCommand = new Command(arg => EditProfitControl());
            EditProfitLimitCommand = new Command(arg => EditProfitLimit());
            CancelOrdersCommand = new Command(arg => CancelOrders());
            CancelStopordersCommand = new Command(arg => CancelStopOrders());
            CloseBalanceCommand = new Command(arg => CloseBalance());
            SendMessageCommand = new Command(arg => SendMessage());
            _hub = hub;
            _hub.On("UserList", RecieveUserList);
            _hub.Invoke("GetUsers");
            _hub.On("UpdateUserBalance", UpdateUserBalance);
            _hub.On("UpdateUser", UpdateUser);
        }

        private void UpdateUser(dynamic upd)
        {
            var user = (User) ((JObject) upd).ToObject(typeof(User));
            _dispatcher.Invoke(
                () =>
                    UsersCollection[UsersCollection.IndexOf(UsersCollection.First(u => u.Login == user.Login))] = user);
        }

        private void EditProfitLimit()
        {
            _hub.Invoke("EditProfitLimit", SelectedUser.Login, SelectedUser.ProfitLimit);
        }

        private void EditProfitControl()
        {
            _hub.Invoke("EditProfitControl", SelectedUser.Login, SelectedUser.ProfitControl);
        }

        private async void SendMessage()
        {
            var msg = await _dialog.ShowInputAsync(this, "Send message to " + SelectedUser.Login, null);
            if (msg == null) return;
            _hub.Invoke("SendMessage", SelectedUser.ConnectionID, msg);
        }

        private void CloseBalance()
        {
            _hub.Invoke("CloseBalance", SelectedUser.Login);
        }

        private void CancelStopOrders()
        {
            _hub.Invoke("CancelStoporders", SelectedUser.Login);
        }

        private void CancelOrders()
        {
            _hub.Invoke("CancelOrders", SelectedUser.Login);
        }


        private void EditAutoSleep()
        {
            _hub.Invoke("EditAutoSleep", SelectedUser.Login, SelectedUser.AutoSleep);
        }

        private void EditSleepThreshold()
        {
            _hub.Invoke("EditSleepThreshold", SelectedUser.Login, SelectedUser.SleepThreshold);
        }

        private void UpdateUserBalance(object data)
        {
            if (UsersCollection.Count == 0) return;
            var arr = (string[])((JArray)data).ToObject(typeof(string[]));
            var user = UsersCollection.First(u => u.Login == arr[0]);
            arr[1] = arr[1].Replace('.', ',');
            user.TotalBalance = double.Parse(arr[1], NumberStyles.Any,
                NumberFormatInfo.CurrentInfo);
        }

        private async void BroadcastMsg()
        {
            var result = await _dialog.ShowInputAsync(this, "Enter message", null);
            if (result == null) return;
            await _hub.Invoke("Broadcast", result);
        }
        
        private async void Disconnect()
        {
            var id = SelectedUser.ConnectionID;
            var message = await _dialog.ShowInputAsync(this, "Enter message", null);
            if (message != null)
            await _hub.Invoke("Disconnect", id, message);
        }

        private void SelectWindows()
        {
            var window = new AdminAvailableWindows
            {
                Login = {Content = SelectedUser.Login },
                Level2 = { IsChecked = SelectedUser.L2 },
                AllTrades = { IsChecked = SelectedUser.AllTrades },
                AllTradesPro = { IsChecked = SelectedUser.AllTradesPro },
                Chart = { IsChecked = SelectedUser.Chart },
                Trading = { IsChecked = SelectedUser.Trading },
                FastOrder = { IsChecked = SelectedUser.FastOrder },
                Logbook = { IsChecked = SelectedUser.Logbook },
                Counter = { IsChecked = SelectedUser.Counter },
                Alerts = { IsChecked = SelectedUser.Alerts }
            };
            window.ShowDialog();
            if (!window.Confirmed) return;
            var userWindows = new UserWindows
            {
                L2 = window.Level2.IsChecked.Value,
                AllTrades = window.AllTrades.IsChecked.Value,
                AllTradesPro = window.AllTradesPro.IsChecked.Value,
                Chart = window.Chart.IsChecked.Value,
                Trading = window.Trading.IsChecked.Value,
                FastOrder = window.FastOrder.IsChecked.Value,
                Counter = window.Counter.IsChecked.Value,
                Logbook = window.Logbook.IsChecked.Value,
                Alerts = window.Alerts.IsChecked.Value,
                Login = SelectedUser.Login
            };
            _hub.Invoke("SelectWindows", userWindows);
        }

        private void Unsleep()
        {
            _hub.Invoke("Unsleep", SelectedUser.Login);
        }

        private async void Sleep()
        {
            var time = await _dialog.ShowInputAsync(this, "Set time", "Time:");
            if (time != null && time != "")
                _hub.Invoke("SleepUser", SelectedUser.Login, time);
        }

        private void Delete()
        {
            _hub.Invoke("DeleteUser", SelectedUser.Login);
        }

        private void Edit()
        {
            var dialog = new AdminUserDialog
            {
                Login = {Text = SelectedUser.Login},
                Privileges = {Text = SelectedUser.Role},
                Year = {Text = SelectedUser.LicenseExpDate.Value.Year.ToString()},
                Month = {Text = SelectedUser.LicenseExpDate.Value.Month.ToString()},
                Day = {Text = SelectedUser.LicenseExpDate.Value.Day.ToString()},
                Email = {Text = SelectedUser.Email}
            };
            dialog.ShowDialog();
            if (dialog.Confirmed)
            {
                _hub.Invoke("EditUser", dialog.Login.Text, dialog.Password.Text, dialog.Privileges.Text,
                    $"{dialog.Year.Text}-{dialog.Month.Text}-{dialog.Day.Text}", dialog.Email.Text);
            }
        }

        private void Add()
        {
            var dialog = new AdminUserDialog();
            dialog.ShowDialog();
            if (dialog.Confirmed)
            {
                _hub.Invoke("AddUser", dialog.Login.Text, dialog.Password.Text, dialog.Privileges.Text,
                    $"{dialog.Year.Text}-{dialog.Month.Text}-{dialog.Day.Text}", dialog.Email.Text, dialog.IsUsa.IsChecked.Value);
            }
        }

        private void RecieveUserList(dynamic list)
        {
            var res = (JArray) list;
            var users = res.ToObject<List<User>>();
            if (UsersCollection.Count != users.Count)
                UsersCollection = new ObservableCollection<User>(users);
            else
            {
                for (var i = 0; i < UsersCollection.Count; i++)
                {
                    if (!UsersCollection[i].Equals(users[i]))
                        _dispatcher.Invoke(() => UsersCollection[i] = users[i]);
                }
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
