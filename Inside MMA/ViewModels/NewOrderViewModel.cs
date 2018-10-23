using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Properties;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.ViewModels
{
    public class NewOrderViewModel : INotifyPropertyChanged
    {
        private double? _minStep;
        public string Client
        {
            get { return _client; }
            set
            {
                if (value == _client) return;
                _client = value;
                OnPropertyChanged();
            }
        }

        public string Board
        {
            get
            {
                return _board;
            }

            set
            {
                _board = value.ToUpper();
                SelectClient();
                if (_board == "MCT")
                    UseCreditVisibility = System.Windows.Visibility.Collapsed;
                else
                    UseCreditVisibility = System.Windows.Visibility.Visible;
                OnPropertyChanged();
            }
        }
        private string _board;
        public string Seccode
        {
            get
            {
                return _seccode;
            }

            set
            {
                _seccode = value;
                _minStep = MainWindowViewModel.SecVm.SecList.FirstOrDefault(s => s.Board == Board && s.Seccode == Seccode)?
                    .Minstep;
                OnPropertyChanged();
            }
        }
        private string _seccode;
        private string _buySell = "B";
        public string BuySell
        {
            get
            {
                return _buySell;
            }

            set
            {
                _buySell = value;
                OnPropertyChanged();
            }
        }
        public string Quantity
        {
            get
            {
                return _quantity;
            }

            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        private string _quantity;

        private string _price;
        public string Price
        {
            get
            {
                return _price;
            }

            set
            {
                if (_minStep >= 1)
                    _price = ((int)double.Parse(value)).ToString();
                else
                    _price = value;
                OnPropertyChanged();
            }
        }

        public bool UseCredit
        {
            get
            {
                return _useCredit; 
            }
            set
            {
                if (value == _useCredit) return;
                _useCredit = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; set; }
        private bool _bymarket = true;
        public bool Bymarket
        {
            get { return _bymarket; }
            set
            {
                _bymarket = value;
                Visibility = !Bymarket;
                OnPropertyChanged();
            }
        }

        private bool _visibility;
        private string _client = "-";
        private bool _useCredit;
        private Visibility _useCreditVisibility;
        public bool Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility UseCreditVisibility
        {
            get { return _useCreditVisibility; }
            set
            {
                if (value == _useCreditVisibility) return;
                _useCreditVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool KeepOpen
        {
            get { return Settings.Default.KeepOpenO; }
            set
            {
                if (value == Settings.Default.KeepOpenO) return;
                Settings.Default.KeepOpenO = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool O
        {
            get { return Settings.Default.OrdersO; }
            set
            {
                if (value == Settings.Default.OrdersO) return;
                Settings.Default.OrdersO = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool S
        {
            get { return Settings.Default.StopordersO; }
            set
            {
                if (value == Settings.Default.StopordersO) return;
                Settings.Default.StopordersO = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool T
        {
            get { return Settings.Default.TradesO; }
            set
            {
                if (value == Settings.Default.TradesO) return;
                Settings.Default.TradesO = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool B
        {
            get { return Settings.Default.BalanceO; }
            set
            {
                if (value == Settings.Default.BalanceO) return;
                Settings.Default.BalanceO = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public IDialogCoordinator Dialog;
        public Action Close;

        private XmlSerializer _serializer = new XmlSerializer(typeof(OrderResult));
       

        public NewOrderViewModel()
        {
            ConfirmCommand = new Command(arg => Confirm());
        }

        private async void Confirm()
        {
            var client = ClientSelector.GetClient(Board);
            string res;
            var useCredit = _useCredit ? "<usecredit/>" : "";
            if (Visibility)

                res = TXmlConnector.ConnectorSendCommand(
                    $"<command id=\"neworder\"><security><board>{_board}</board><seccode>{_seccode}</seccode></security><client>{client[0]}</client><union>{client[1]}</union><price>{ _price }</price><quantity>{ _quantity }</quantity><buysell>{ _buySell }</buysell>{useCredit}</command>");
            else
                res = TXmlConnector.ConnectorSendCommand("<command id=\"neworder\"><security><board>" + _board +
                                                   "</board><seccode>" + _seccode +
                                                   "</seccode></security><client>" + client[0] + "</client><union>" + client[1] + "</union><quantity>" +
                                                   _quantity + "</quantity><buysell>" + _buySell +
                                                   "</buysell><bymarket/>" + useCredit + "</command>");
            var orderResult = (OrderResult) _serializer.Deserialize(new StringReader(res));
            if (orderResult.Success == "true")
            {
                await Dialog.ShowMessageAsync(this, "Message", $"Success. Transaction ID - {orderResult.TransactionId}");
                OpenTradingWindows();
                if (!KeepOpen)
                    Close();
            }
            else
                await Dialog.ShowMessageAsync(this, "Warning", orderResult.Message);

        }

        private void OpenTradingWindows()
        {
            var main = (MainWindowViewModel) Application.Current.MainWindow.DataContext;
            if (O)
                main.OpenWindow("orders");
            if (S)
                main.OpenWindow("stoporders");
            if (T)
                main.OpenWindow("trades");
            if (B)
                main.OpenWindow("clients");
        }

        private void SelectClient()
        {
            var clients = MainWindowViewModel.ClientsViewModel.Clients;
            Client client = null;
            if (Board == "TQBR" ||
                Board == "EQOB" ||
                Board == "EQRP" ||
                Board == "TQIF" ||
                Board == "TQDE" ||
                Board == "SPFEQ")
            {
                
                client =
                    clients.Find(
                        cl => cl.Market == "ММВБ");
            }
            if (Board == "FUT" ||
                Board == "OPT" )
            {
                client =
                    clients.Find(
                        cl => cl.Market == "FORTS");
            }
            if (Board == "MCT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "MMA");
            }
            if (Board == "CETS")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "ETS");
            }
            if (client == null)
            {
                Client = "-";
                return;
            }
            //ClientInfo.Id = client.Id;
            //ClientInfo.Union = client.Union;
            Client = client.Id;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
