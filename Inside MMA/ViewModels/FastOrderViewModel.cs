using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Properties;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.TeamFoundation.Threading;
using SciChart.Core.Extensions;
using Timer = System.Threading.Timer;

namespace Inside_MMA.ViewModels
{
    public class FastOrderViewModel : RememberPlacement, IAnchor
    {
        private bool _isAnchorEnabled;
        private double _size = 1;
        private string _client;
        private double _spread;
        private string _buyEstimate;
        private string _sellEstimate;
        private double _stoporderSpread;
        private double _bestBuy;
        private double _bestSell;
        private double _maxSpreadForTrade;
        private bool _tradeEnabled = true;
        private bool _checkSpread;
        private string _stopType;
        private double _buyPrice;
        private double _sellPrice;
        private bool _buyEnabled = true;
        private bool _sellEnabled = true;
        private bool _useCredit = true;
        private readonly XmlSerializer _serializer = new XmlSerializer(typeof(OrderResult));
        private int _balance;
        private Timer _timer;
        private int _activeStoporders;
        private int _activeOrders;

        public double Spread
        {
            get { return _spread; }
            set
            {
                if (value == _spread) return;
                _spread = value;
                OnPropertyChanged();
                if (_checkSpread)
                    TradeEnabled = _spread <= _maxSpreadForTrade;
            }
        }

        public double Size
        {
            get { return _size; }
            set
            {
                if (value == _size) return;
                _size = value;
                Settings.Default.FastOrderSize = Size;
                Settings.Default.Save();
                OnPropertyChanged();

                CalcSpreadAndEstimates(Level2DataHandler.Level2List
                    .FirstOrDefault(x => x.Board == Board && x.Seccode == Seccode)?
                    .Level2Data.ToList());
            }
        }

        public string BuyEstimate
        {
            get { return _buyEstimate; }
            set
            {
                if (value == _buyEstimate) return;
                _buyEstimate = value;
                OnPropertyChanged();
            }
        }

        public string SellEstimate
        {
            get { return _sellEstimate; }
            set
            {
                if (value == _sellEstimate) return;
                _sellEstimate = value;
                OnPropertyChanged();
            }
        }

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

        public double StoporderSpread
        {
            get { return _stoporderSpread; }
            set
            {
                if (value.Equals(_stoporderSpread)) return;
                _stoporderSpread = value;
                OnPropertyChanged();
            }
        }

        public double MaxSpreadForTrade
        {
            get { return _maxSpreadForTrade; }
            set
            {
                if (value.Equals(_maxSpreadForTrade)) return;
                _maxSpreadForTrade = value;
                OnPropertyChanged();
                if (_checkSpread)
                    TradeEnabled = _spread <= _maxSpreadForTrade;
            }
        }

        public bool TradeEnabled
        {
            get { return _tradeEnabled; }
            set
            {
                if (value == _tradeEnabled) return;
                _tradeEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool CheckSpread
        {
            get { return _checkSpread; }
            set
            {
                if (value == _checkSpread) return;
                _checkSpread = value;
                if (!_checkSpread)
                    TradeEnabled = true;
                else
                    TradeEnabled = _spread <= _maxSpreadForTrade;
                OnPropertyChanged();
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

                UpdateWindowBinding(IsAnchorEnabled);
            }
        }

        public new string Board
        {
            get { return _board; }
            set
            {
                if (value == _board) return;
                _board = value;
                OnPropertyChanged();

                Client = ClientSelector.SelectClient(Board);
            }
        }

        public string StopType
        {
            get { return _stopType; }
            set
            {
                if (value == _stopType) return;
                _stopType = value;
                OnPropertyChanged();
            }
        }

        public double BuyPrice
        {
            get { return _buyPrice; }
            set
            {
                if (value.Equals(_buyPrice)) return;
                _buyPrice = value;
                OnPropertyChanged();
                SellEnabled = _buyPrice == 0;
            }
        }

        public double SellPrice
        {
            get { return _sellPrice; }
            set
            {
                if (value.Equals(_sellPrice)) return;
                _sellPrice = value;
                OnPropertyChanged();
                BuyEnabled = _sellPrice == 0;
            }
        }

        public bool BuyEnabled
        {
            get { return _buyEnabled; }
            set
            {
                if (value == _buyEnabled) return;
                _buyEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool SellEnabled
        {
            get { return _sellEnabled; }
            set
            {
                if (value == _sellEnabled) return;
                _sellEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool UseCredit
        {
            get { return _useCredit; }
            set
            {
                if (value == _useCredit) return;
                _useCredit = value;
                OnPropertyChanged();
            }
        }

        public bool O
        {
            get { return Settings.Default.FastOrderO; }
            set
            {
                if (value == Settings.Default.FastOrderO) return;
                Settings.Default.FastOrderO = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool S
        {
            get { return Settings.Default.FastOrderS; }
            set
            {
                if (value == Settings.Default.FastOrderS) return;
                Settings.Default.FastOrderS = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool T
        {
            get { return Settings.Default.FastOrderT; }
            set
            {
                if (value == Settings.Default.FastOrderT) return;
                Settings.Default.FastOrderT = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool B
        {
            get { return Settings.Default.FastOrderC; }
            set
            {
                if (value == Settings.Default.FastOrderC) return;
                Settings.Default.FastOrderC = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public List<IAnchor> AnchoredWindows => Application.Current.Dispatcher
            .Invoke(() => (MainWindowViewModel)Application.Current.MainWindow.DataContext).AnchoredWindows;

        public int Balance
        {
            get { return _balance; }
            set
            {
                if (value == _balance) return;
                _balance = value;
                OnPropertyChanged();
            }
        }


        public int ActiveOrders
        {
            get { return _activeOrders; }
            set
            {
                if (value == _activeOrders) return;
                _activeOrders = value;
                OnPropertyChanged();
            }
        }

        public int ActiveStoporders
        {
            get { return _activeStoporders; }
            set
            {
                if (value == _activeStoporders) return;
                _activeStoporders = value;
                OnPropertyChanged();
            }
        }

        public ICommand CancelAllOrders => MainWindowViewModel.ClientOrdersViewModel.CancelAllOrders;
        public ICommand CancelAllStoporders => MainWindowViewModel.ClientOrdersViewModel.CancelAllStoporders;

        public ICommand SendOrder { get; set; }
        public ICommand Closing { get; set; }
        public IDialogCoordinator Dialog { get; set; }

        //public FastOrderViewModel()
        //{
        //    Size = Settings.Default.FastOrderSize;
        //    SendOrder = new Command(PlaceOrder);
        //    IsAnchorEnabled = true;
        //    _timer = new Timer(GetBalance, null, 0, 500);
        //}

        private void GetBalance(object state)
        {
            try
            {
                var clientsVm = MainWindowViewModel.ClientsViewModel;
                if (clientsVm.ShowPortfolioMCT)
                {
                    var row = clientsVm
                        .PortfolioMct[0].Security.FirstOrDefault(s => s.Seccode == Seccode);
                    Balance = row?.Balance ?? 0;
                }
                else
                {
                    var row = clientsVm
                        .UnitedPortfolioSecurityDataGridRowses.FirstOrDefault(r => r.Seccode == Seccode);
                    Balance = row?.Balance ?? 0;
                }
            }
            catch 
            {
            }
            
        }

        public FastOrderViewModel(string board, string seccode, Window window, int id = 0)
        {
            Window = window;
            Board = board;
            Seccode = seccode;
            Size = Settings.Default.FastOrderSize;
            SendOrder = new Command(PlaceOrder);
            Closing = new Command(arg => WindowClosing());
            Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            Level2DataHandler.Level2ColChanged += OnLevel2ColChanged;
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
            IsAnchorEnabled = true;
            _timer = new Timer(GetBalance, null, 0, 500);
            ActiveOrders = MainWindowViewModel.ClientOrdersViewModel.ActiveOrders;
            ActiveStoporders = MainWindowViewModel.ClientOrdersViewModel.ActiveStoporders;
            SubscriveToOrderCountUpdates();
        }

        private void SubscriveToOrderCountUpdates()
        {
            MainWindowViewModel.ClientOrdersViewModel.PropertyChanged += 
                UpdateOrderCount;
        }
        private void UnsubscribeFromOrderCountUpdates()
        {
            MainWindowViewModel.ClientOrdersViewModel.PropertyChanged -=
                UpdateOrderCount;
        }
        private void UpdateOrderCount(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "ActiveOrders")
                ActiveOrders = MainWindowViewModel.ClientOrdersViewModel.ActiveOrders;
            if (args.PropertyName == "ActiveStoporders")
                ActiveStoporders = MainWindowViewModel.ClientOrdersViewModel.ActiveStoporders;
        }
        private void WindowClosing()
        {
            AnchoredWindows.RemoveIfContains(this);
            UnsubscribeFromWindowEvents();
            UnsubscribeFromOrderCountUpdates();
            CloseWindow();
        }

        public void SetSecurity(string board, string seccode)
        {
            Level2DataHandler.Level2ColChanged -= OnLevel2ColChanged;
            Board = board;
            Seccode = seccode;
            Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            Level2DataHandler.Level2ColChanged += OnLevel2ColChanged;
        }

        private void OnLevel2ColChanged(List<Level2Item> list)
        {
            CalcSpreadAndEstimates(list);
        }

        private void CalcSpreadAndEstimates(List<Level2Item> list)
        {
            if (list == null) return;
            
            var bestsell = list.LastOrDefault(x => x.BuySell == "sell");
            var bestbuy = list.FirstOrDefault(x => x.BuySell == "buy");
            if (bestbuy == null || bestsell == null) return;
            _bestBuy = bestbuy.Price;
            _bestSell = bestsell.Price;
            Spread = (bestsell.Price - bestbuy.Price).RoundOff(2, MidpointRounding.AwayFromZero);
            //calc sell estimate
            double sum = 0;
            var size = Size;
            var sellList = list.Where(x => x.BuySell == "sell").OrderBy(s => s.Price);
            foreach (var item in sellList)
            {
                if (size - item.Quantity > 0)
                {
                    // if you can't  - subtract available quantity from required size, add sum, move to next sell
                    sum += item.Price * item.Quantity;
                    size -= item.Quantity;
                }
                else
                {
                    //if you can, add sum and break
                    sum += item.Price * size;
                    break;
                }
            }
            SellEstimate = (sum / Size).ToString("F2").Replace(',', '.');
            //calc buy estimate
            sum = 0;
            size = Size;
            var buyList = list.Where(x => x.BuySell == "buy").OrderByDescending(s => s.Price);
            foreach (var item in buyList)
            {
                if (size - item.Quantity > 0)
                {
                    // if you can't  - subtract available quantity from required size, add sum, move to next sell
                    sum += item.Price * item.Quantity;
                    size -= item.Quantity;
                }
                else
                {
                    //if you can, add sum and break
                    sum += item.Price * size;
                    break;
                }
            }
            BuyEstimate = (sum / Size).ToString("F2").Replace(',', '.');
        }

        private async void PlaceOrder(object param)
        {
            var client = ClientSelector.GetClient(Board);
            var usecredit = UseCredit ? "<usecredit/>" : "";
            var res1 = TXmlConnector.ConnectorSendCommand("<command id=\"neworder\"><security><board>" + Board +
                                                     "</board><seccode>" + Seccode +
                                                     "</seccode></security><client>" + client[0] + "</client>" +
                                                     "<union>" + client[1] + "</union>" +
                                                     "<quantity>" + Size + "</quantity>" +
                                                     "<buysell>" + param + "</buysell>" +
                                                     "<bymarket/>" + usecredit + "</command>");
            OpenTradingWindows();
            var res2 = string.Empty;
            var orderResult1 = (OrderResult)_serializer.Deserialize(new StringReader(res1));
            if (StopType == "Stop spread")
            {
                var direction = param.ToString() == "B" ? "S" : "B";
                var activationPrice = direction == "B" ? _bestBuy + StoporderSpread : _bestSell - StoporderSpread;
                res2 = TXmlConnector.ConnectorSendCommand(
                    $"<command id=\"newstoporder\"><security><board>{Board}</board><seccode>{Seccode}</seccode></security><client>{client[0]}</client><union>{client[1]}</union><buysell>{direction}</buysell><stoploss><activationprice>{activationPrice}</activationprice><bymarket/><quantity>{Size}</quantity>{usecredit}</stoploss></command>");
            }
            if (StopType == "Manual stop")
            {
                var direction = param.ToString() == "B" ? "S" : "B";
                var activationPrice = direction == "B" ? BuyPrice : SellPrice;
                res2 = TXmlConnector.ConnectorSendCommand(
                    $"<command id=\"newstoporder\"><security><board>{Board}</board><seccode>{Seccode}</seccode></security><client>{client[0]}</client><union>{client[1]}</union><buysell>{direction}</buysell><stoploss><activationprice>{activationPrice}</activationprice><bymarket/><quantity>{Size}</quantity>{usecredit}</stoploss></command>");
            }
            if (orderResult1.Success == "true")
            {
                await Dialog.ShowMessageAsync(this, "Message", $"Success. Transaction ID - {orderResult1.TransactionId}");
            }
            else
                await Dialog.ShowMessageAsync(this, "Warning", orderResult1.Message);
            if (res2.IsNullOrEmpty()) return;
            var orderResult2 = (OrderResult)_serializer.Deserialize(new StringReader(res2));
            if (orderResult2.Success == "true")
            {
                await Dialog.ShowMessageAsync(this, "Message", $"Success. Transaction ID - {orderResult2.TransactionId}");
            }
            else
                await Dialog.ShowMessageAsync(this, "Warning", orderResult2.Message);
            
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
    }
}