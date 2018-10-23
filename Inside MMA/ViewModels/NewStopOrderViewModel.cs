using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Properties;
using MahApps.Metro.Controls.Dialogs;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public class NewStopOrderViewModel : INotifyPropertyChanged
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
                return _board.ToUpper();
            }

            set
            {
                _board = value;
                SelectClient();
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

        private string BoardString => $"<board>{_board.ToUpper()}</board>";
        private string SeccodeString => $"<seccode>{_seccode}</seccode> ";

        private string _buySell = "S";
        public string BuySell
        {
            get
            {
                return $"<buysell>{_buySell}</buysell>";
            }

            set
            {
                _buySell = value;
                OnPropertyChanged();
            }
        }
        //stop-loss---------------------------------------------------
        private string _stopLossActivationPrice;
        private string _stopLossOrderPrice;
        private string _stopLossQuantity;
        private string _stopLossGuardTime;
        private bool _stoplossByMarket;
        private bool _stopLossUseCredit;
        private string _stopLossByMarketString;
        private string _stopLossUseCreditString;
        public string StopLossActivationPrice
        {
            get { return _stopLossActivationPrice; }
            set
            {
                if (value == _stopLossActivationPrice) return;
                if (_minStep >= 1)
                    _stopLossActivationPrice = ((int)double.Parse(value)).ToString();
                else
                    _stopLossActivationPrice = value;
                OnPropertyChanged();
            }
        }

        private string StopLossActivationPriceString => _stopLossActivationPrice == ""
            ? string.Empty
            : $"<activationprice>{_stopLossActivationPrice}</activationprice>";
        public string StopLossOrderPrice
        {
            get { return _stopLossOrderPrice == "" ? string.Empty : $"<orderprice>{_stopLossOrderPrice}</orderprice>"; }
            set
            {
                if (value == _stopLossOrderPrice) return;
                _stopLossOrderPrice = value;
                OnPropertyChanged();
            }
        }
        public string StopLossQuantity
        {
            get { return _stopLossQuantity == "" ? string.Empty : $"<quantity>{_stopLossQuantity}</quantity>"; }
            set
            {
                if (value == _stopLossQuantity) return;
                _stopLossQuantity = value;
                OnPropertyChanged();
            }
        }
        public string StopLossGuardTime
        {
            get { return _stopLossGuardTime == "" ? string.Empty : $"<guardtime>{_stopLossGuardTime}</guardtime>"; }
            set
            {
                if (value == _stopLossGuardTime) return;
                _stopLossGuardTime = value;
                OnPropertyChanged();
            }
        }
        public bool StopLossByMarket
        {
            set
            {
                if (value == _stoplossByMarket) return;
                _stoplossByMarket = value;
                if (_stoplossByMarket)
                    _stopLossByMarketString = "<bymarket/>";
                else
                    _stopLossByMarketString = string.Empty;
                OnPropertyChanged();
            }
        }
        public bool StopLossUseCredit
        {
            set
            {
                if (value == _stopLossUseCredit) return;
                _stopLossUseCredit = value;
                if (_stopLossUseCredit)
                    _stopLossUseCreditString = "<usecredit/>";
                else
                    _stopLossUseCreditString = string.Empty;
                OnPropertyChanged();
            }
        }
        //------------------------------------------------------------
        //take-profit-------------------------------------------------
        private string _takeProfitActivationPrice;
        private string _takeProfitQuantity;
        private string _takeProfitGuardTime;
        private string _takeProfitCorrection;
        private string _takeProfitSpread;
        private bool _takeProfitByMarket;
        private bool _takeProfitUseCredit;
        private string _takeProfitByMarketString;
        private string _takeProfitUseCreditString;
        private string _client = "-";

        public string TakeProfitActivationPrice
        {
            get { return _takeProfitActivationPrice == "" ? string.Empty : $"<activationprice>{_takeProfitActivationPrice}</activationprice>"; }
            set
            {
                if (value == _takeProfitActivationPrice) return;
                _takeProfitActivationPrice = value;
                OnPropertyChanged();
            }
        }
        public string TakeProfitQuantity
        {
            get { return _takeProfitQuantity == "" ? string.Empty : $"<quantity>{_takeProfitQuantity}</quantity>"; }
            set
            {
                if (value == _takeProfitQuantity) return;
                _takeProfitQuantity = value;
                OnPropertyChanged();
            }
        }
        public string TakeProfitGuardTime
        {
            get { return _takeProfitGuardTime == "" ? string.Empty : $"<guardtime>{_takeProfitGuardTime}</guardtime>"; }
            set
            {
                if (value == _takeProfitGuardTime) return;
                _takeProfitGuardTime = value;
                OnPropertyChanged();
            }
        }
        public string TakeProfitCorrection
        {
            get { return _takeProfitCorrection == "" ? string.Empty : $"<correction>{_takeProfitCorrection}</correction>"; }
            set
            {
                if (value == _takeProfitCorrection) return;
                _takeProfitCorrection = value;
                OnPropertyChanged();
            }
        }
        public string TakeProfitSpread
        {
            get { return _takeProfitSpread == "" ? string.Empty : $"<spread>{_takeProfitSpread}</spread>"; }
            set
            {
                if (value == _takeProfitSpread) return;
                _takeProfitSpread = value;
                OnPropertyChanged();
            }
        }
        public bool TakeProfitByMarket
        {
            set
            {
                if (value == _takeProfitByMarket) return;
                _takeProfitByMarket = value;
                if (_takeProfitByMarket)
                    _takeProfitByMarketString = "<bymarket/>";
                else
                    _takeProfitByMarketString = string.Empty;
                OnPropertyChanged();
            }
        }
        public bool TakeProfitUseCredit
        {
            set
            {
                if (value == _takeProfitUseCredit) return;
                _takeProfitUseCredit = value;
                if (_takeProfitUseCredit)
                    _takeProfitUseCreditString = "<bymarket/>";
                else
                    _takeProfitUseCreditString = string.Empty;
                OnPropertyChanged();
            }

        }

        public string StopLossUseCreditString
        {
            get
            {
                return _stopLossUseCreditString;
                
            }
            set { _stopLossUseCreditString = value; }
        }
        //------------------------------------------------------------

        private bool _isSmartStoporder = true;
        public bool IsSmartStoporder
        {
            get { return _isSmartStoporder; }
            set
            {
                if (value == _isSmartStoporder) return;
                _isSmartStoporder = value;
                OnPropertyChanged();
            }
        }
        public bool KeepOpen
        {
            get { return Settings.Default.KeepOpenS; }
            set
            {
                if (value == Settings.Default.KeepOpenS) return;
                Settings.Default.KeepOpenS = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool O
        {
            get { return Settings.Default.OrdersS; }
            set
            {
                if (value == Settings.Default.OrdersS) return;
                Settings.Default.OrdersS = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool S
        {
            get { return Settings.Default.StopordersS; }
            set
            {
                if (value == Settings.Default.StopordersS) return;
                Settings.Default.StopordersS = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool T
        {
            get { return Settings.Default.TradesS; }
            set
            {
                if (value == Settings.Default.TradesS) return;
                Settings.Default.TradesS = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool B
        {
            get { return Settings.Default.BalanceS; }
            set
            {
                if (value == Settings.Default.BalanceS) return;
                Settings.Default.BalanceS = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public ICommand ConfirmCommand { get; set; }
        public IDialogCoordinator Dialog;
        private XmlSerializer _serializer = new XmlSerializer(typeof(OrderResult));
        public Action Close;

        public NewStopOrderViewModel()
        {
            ConfirmCommand = new Command(arg => Confirm());
            _board = "";
            _seccode = "";
        }

        private async void Confirm()
        {
            var client = ClientSelector.GetClient(Board);
            if (IsSmartStoporder && !CheckStoplossPrice()) return;
            string stoploss = $"<stoploss>{StopLossActivationPriceString}{StopLossOrderPrice}{_stopLossByMarketString}{StopLossQuantity}{_stopLossUseCreditString}{StopLossGuardTime}</stoploss>";
            string takeprofit = $"<takeprofit>{TakeProfitActivationPrice}{TakeProfitQuantity}{_takeProfitUseCreditString}{TakeProfitGuardTime}{TakeProfitCorrection}{TakeProfitSpread}<bymarket/></takeprofit>";
            
            if (stoploss == "<stoploss></stoploss>")
                stoploss = string.Empty;
            if (takeprofit == "<takeprofit><bymarket/></takeprofit>")
                takeprofit = string.Empty;
            var cmd =
                $"<command id=\"newstoporder\"><security>{BoardString}{SeccodeString}</security><client>{client[0]}</client><union>{client[1]}</union>{BuySell}{stoploss}{takeprofit}</command>";
            var orderResult = (OrderResult)_serializer.Deserialize(new StringReader(TXmlConnector.ConnectorSendCommand(cmd)));
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
            var main = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
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
            Client client = null;
            if (Board == "TQBR" ||
                Board == "EQOB" ||
                Board == "EQRP" ||
                Board == "TQIF" ||
                Board == "TQDE" ||
                Board == "SPFEQ")
            {
                client =
                    MainWindowViewModel.ClientsViewModel.Clients.Find(
                        cl => cl.Market == "ММВБ");
            }
            if (Board == "FUT" ||
                Board == "OPT")
            {
                client =
                    MainWindowViewModel.ClientsViewModel.Clients.Find(
                        cl => cl.Market == "FORTS");
            }
            if (Board == "MCT")
            {
                client =
                    MainWindowViewModel.ClientsViewModel.Clients.Find(
                        cl => cl.Market == "MMA");
            }
            if (Board == "CETS")
            {
                client =
                    MainWindowViewModel.ClientsViewModel.Clients.Find(
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

        private bool CheckStoplossPrice()
        {
            try
            {
                var lastPrice = TickDataHandler.TickList.FirstOrDefault(x => x.Board == Board && x.Seccode == Seccode).TradeItems.First().Price;
                if (lastPrice == 0) return true;
                if (_buySell == "S" && double.Parse(_stopLossActivationPrice) > lastPrice)
                {
                    DisplayWarning();
                    return false;
                }
                if (_buySell == "B" && double.Parse(_stopLossActivationPrice) < lastPrice)
                {
                    DisplayWarning();
                    return false;
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        private void DisplayWarning()
        {
            Task.Run(() =>
            {
                SpinWait.SpinUntil(() => Dialog != null);
                Dialog.ShowMessageAsync(this, "Warning",
                    "This stoporder will execute immediately! Check \"Activation price\" field");
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
