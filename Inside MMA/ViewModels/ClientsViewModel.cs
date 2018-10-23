using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Views;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualStudio.Services.Client.Controls;
using SciChart.Charting.Common.Extensions;
using SciChart.Core.Extensions;
using UnitedPortfolio = Inside_MMA.Models.UnitedPortfolio;

namespace Inside_MMA.ViewModels
{
    public class SignToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (int) value >= 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ClientsViewModel : INotifyPropertyChanged
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;

        private bool _showPortfolioMct;
        public bool ShowPortfolioMCT
        {
            get { return _showPortfolioMct; }
            set
            {
                if (value == _showPortfolioMct) return;
                _showPortfolioMct = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<PortfolioMCT> _portfolioMct = new ObservableCollection<PortfolioMCT> {new PortfolioMCT()};
        public ObservableCollection<PortfolioMCT> PortfolioMct
        {
            get { return _portfolioMct; }
            set
            {
                if (Equals(value, _portfolioMct)) return;
                _portfolioMct = value;
                OnPropertyChanged();
            }
        }
        private Client _currentClient;
        public Client CurrentClient
        {
            get
            {
                return _currentClient;
            }

            set
            {
                if (Equals(value, _currentClient)) return;
                _currentClient = value;
                var cmd = $"<command id=\"get_united_portfolio\" client=\"{_currentClient.Id}\" />";
                TXmlConnector.ConnectorSendCommand(cmd);
                OnPropertyChanged();
            }
        }
        public List<Client> Clients { get; set; } = new List<Client>();
        private ObservableCollection<UnitedPortfolioDataGridRow> _unitedPortfolioDataGridRows = new ObservableCollection<UnitedPortfolioDataGridRow>();

        public ObservableCollection<UnitedPortfolioDataGridRow> UnitedPortfolioDataGridRows
        {
            get
            {
                return _unitedPortfolioDataGridRows;
            }

            set
            {
                _unitedPortfolioDataGridRows = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MoneyDataGridRow> _moneyDataGridRows = new ObservableCollection<MoneyDataGridRow>();

        public ObservableCollection<MoneyDataGridRow> MoneyDataGridRowses
        {
            get
            {
                return _moneyDataGridRows;
            }

            set
            {
                _moneyDataGridRows = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ValuePartDataGridRow> _valuePartDataGridRows = new ObservableCollection<ValuePartDataGridRow>();

        public ObservableCollection<ValuePartDataGridRow> ValuePartDataGridRowses
        {
            get
            {
                return _valuePartDataGridRows;
            }

            set
            {
                _valuePartDataGridRows = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AssetDataGridRow> _assetDataGridRows = new ObservableCollection<AssetDataGridRow>();

        public ObservableCollection<AssetDataGridRow> AssetDataGridRowses
        {
            get
            {
                return _assetDataGridRows;
            }

            set
            {
                _assetDataGridRows = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<UnitedPortfolioSecurityDataGridRow> _unitedPortfolioSecurityDataGridRows = new ObservableCollection<UnitedPortfolioSecurityDataGridRow>();

        public ObservableCollection<UnitedPortfolioSecurityDataGridRow> UnitedPortfolioSecurityDataGridRowses
        {
            get
            {
                return _unitedPortfolioSecurityDataGridRows;
            }

            set
            {
                _unitedPortfolioSecurityDataGridRows = value;
                OnPropertyChanged();
            }
        }

        public UnitedPortfolioSecurityDataGridRow SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                if (_selectedRow != null)
                    _selectedRowCache = _selectedRow;
                OnPropertyChanged();
            }
        }

        private UnitedPortfolioSecurityDataGridRow _selectedRowCache;

        public double TotalBalance
        {
            get { return _totalBalance; }
            set
            {
                if (value.Equals(_totalBalance)) return;
                _totalBalance = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClosingCommand { get; set; }
        public ICommand CloseBalance { get; set; }
        public ICommand PlaceOrder { get; set; }
        public ICommand PlaceStopOrder { get; set; }
        public Timer Timer = new Timer(1000);
        private IDialogCoordinator _dialogCoordinator;
        public ClientsViewModel(IDialogCoordinator dialogCoordinator)
        {
            TXmlConnector.SendNewClientInfo += OnSendNewClientInfo;
            TXmlConnector.SendNewUnitedPortfolio += XmlConnector_OnSendUnitesPortfolio;
            TXmlConnector.SendPortfolioMCT += OnPortfolioMCT;
            ClosingCommand = new Command(arg => Closing());
            CloseBalance = new Command(arg => CloseBalanceMethod());
            PlaceOrder = new Command(PlaceOrderMethod);
            PlaceStopOrder = new Command(PlaceStopOrderMethod);
            Timer.Elapsed += Refresh;
            Timer.AutoReset = true;
            Timer.Start();
            _dialogCoordinator = dialogCoordinator;
        }

        private void PlaceStopOrderMethod(object o)
        {
            var sec = MainWindowViewModel.SecVm.SecList.First(s => s.Secid == _selectedRowCache.Secid);
            new NewStopOrder { DataContext = new NewStopOrderViewModel
            {
                Board = sec.Board,
                Seccode = sec.Seccode
            } }.Show();
        }

        private void PlaceOrderMethod(object o)
        {
            var sec = MainWindowViewModel.SecVm.SecList.First(s => s.Secid == _selectedRowCache.Secid);
            new NewOrder { DataContext = new NewOrderViewModel
            {
                Board = sec.Board,
                Seccode = sec.Seccode
            }}.Show();
        }

        public async void CloseBalanceMethod()
        {
            MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(this, 
                "Are you sure?", "Are you sure you want to close all positions?",
                MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var row in UnitedPortfolioSecurityDataGridRowses)
                {
                    string res;
                    var security = MainWindowViewModel.SecVm.SecList
                        .First(x => x.Secid == row.Secid);
                    var client = ClientSelector.GetClient(security.Board);
                    if (row.Balance > 0)
                        res = ConnectorCommands.PlaceMktOrder(security.Board, security.Seccode, client[0], client[1],
                            row.Balance / security.Lotsize, "S", false);
                    if (row.Balance < 0)
                        res = ConnectorCommands.PlaceMktOrder(security.Board, security.Seccode, client[0], client[1],
                            Math.Abs(row.Balance / security.Lotsize), "B", false);
                }
            }
        }

        private static XmlSerializer _mctSerializer = new XmlSerializer(typeof(PortfolioMCT));
        private static readonly object Lock = new object();
        private void OnPortfolioMCT(string data)
        {
            lock (Lock)
            {
                var portfolio = (PortfolioMCT)_mctSerializer.Deserialize(new StringReader(data));
                //set properties where they don't match
                foreach (var propertyInfo in PortfolioMct[0].GetType().GetProperties())
                {
                    //skip securities
                    if (propertyInfo.Name == "Security") continue;
                    var value = propertyInfo.GetValue(portfolio);
                    if (propertyInfo.GetValue(PortfolioMct[0]) != value)
                        propertyInfo.SetValue(PortfolioMct[0], value);
                }
                foreach (var sec in portfolio.Security)
                {
                    if (PortfolioMct[0].Security.FirstOrDefault(x => x.Seccode == sec.Seccode) != null)
                        Dispatcher.Invoke(() => PortfolioMct[0]
                            .Security[
                                PortfolioMct[0].Security
                                    .IndexOf(PortfolioMct[0].Security.First(x => x.Seccode == sec.Seccode))] = sec);
                    else
                        Dispatcher.Invoke(() => PortfolioMct[0].Security.Add(sec));

                }
            }
        }

        public void GetInfo()
        {
            string cmd;
            if (!MainWindowViewModel.IsConnected || CurrentClient == null) return;
            //if mct - request mct portfolio
            if (ShowPortfolioMCT)
            {
                cmd = $"<command id=\"get_portfolio_mct\" client=\"{CurrentClient.Id}\" />";
                TXmlConnector.ConnectorSendCommand(cmd);
            }
            else
            {
                cmd = $"<command id=\"get_united_portfolio\" client=\"{CurrentClient.Id}\" />";
                var res = TXmlConnector.ConnectorSendCommand(cmd);
                var orderResult = (OrderResult)new XmlSerializer(typeof(OrderResult)).Deserialize(new StringReader(res));
                //check if getting united portfolio is possible
                if (orderResult.Success == "false")
                {
                    //if impossible - get mct portfolio for this session
                    ShowPortfolioMCT = true;
                    cmd = $"<command id=\"get_portfolio_mct\" client=\"{CurrentClient.Id}\" />";
                    TXmlConnector.ConnectorSendCommand(cmd);
                }
            }
            
        }
        private async void Refresh(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            //await Task.Run(() => { GetInfo(); });
            GetInfo();
        }

        private void Closing()
        {
            //Timer.Stop();
        }

        XmlSerializer _clientSerializer = new XmlSerializer(typeof(Client));
        private void OnSendNewClientInfo(string data)
        {
            Client client;
            using (var reader = new StringReader(data))
            {
                client = (Client)_clientSerializer.Deserialize(reader);
                reader.Close();
            }
            if (client.FortsAcc == null)
                client.FortsAcc = "-";
            client.Market = IdToName(client.Market);
            Clients.AddIfNotContains(client);
            CurrentClient = Clients[0];
        }

        private string IdToName(string id)
        {
            switch (id)
            {
                case "0":
                    return "Collateral";
                case "1":
                    return "ММВБ";
                case "4":
                    return "FORTS";
                case "7":
                    return "SPBEX";
                case "8":
                    return "INF";
                case "9":
                    return "XETRA";
                case "12":
                    return "AMERICA";
                case "14":
                    return "MMA";
                case "15":
                    return "ETS";
                default:
                    return "";
            }
        }
        private static Dispatcher Dispatcher => Application.Current.Dispatcher;

        private XmlSerializer _serializer = new XmlSerializer(typeof(UnitedPortfolio),
            new XmlRootAttribute("united_portfolio"));

        private UnitedPortfolioSecurityDataGridRow _selectedRow;
        private double _totalBalance;


        private void XmlConnector_OnSendUnitesPortfolio(string data)
        {
            //Dispatcher.Invoke(() => {
            //    AssetDataGridRowses.Clear();
            //    MoneyDataGridRowses.Clear();
            //    ValuePartDataGridRowses.Clear();
            //});
            UnitedPortfolio portfolio;
            using (var stringReader = new StringReader(data))
            {
                portfolio = (UnitedPortfolio)
                    _serializer.Deserialize(stringReader);
            }
            var upRow = new UnitedPortfolioDataGridRow
            {
                ChrgoffIr = portfolio.ChrgoffIr,
                ChrgoffMr = portfolio.ChrgoffMr,
                Client = portfolio.Client,
                Equity = portfolio.Equity,
                Finres = portfolio.Finres,
                Go = portfolio.Go,
                InitReq = portfolio.InitReq,
                MaintReq = portfolio.MaintReq,
                OpenEquity = portfolio.OpenEquity,
                RegEquity = portfolio.RegEquity,
                RegIr = portfolio.RegIr,
                RegMr = portfolio.RegMr,
                Union = portfolio.Union,
                Vm = portfolio.Vm
            };
            if (UnitedPortfolioDataGridRows.Count == 0)
                Dispatcher.Invoke(() => UnitedPortfolioDataGridRows.Add(upRow));
            else
                Dispatcher.Invoke(() => UnitedPortfolioDataGridRows[0] = upRow);
            foreach (var asset in portfolio.Assets)
            {
                //AssetDataGridRowses.Add(new AssetDataGridRow
                //{
                //    Code = asset.Code,
                //    InitReq = asset.InitReq,
                //    MaintReq = asset.MaintReq,
                //    Name = asset.Name,
                //    SetoffRate = asset.SetoffRate
                //});
                foreach (var security in asset.Securities)
                {
                    var UPSrow = new UnitedPortfolioSecurityDataGridRow
                    {
                        Balance = security.Balance,
                        Bought = security.Bought,
                        Buying = security.Bought,
                        OpenBalance = security.OpenBalance,
                        Equity = security.Equity,
                        Market = security.Market,
                        Maxbuy = security.Maxbuy,
                        Maxsell = security.Maxsell,
                        Pl = security.Pl,
                        PnlIncome = security.PnlIncome,
                        Sold = security.Sold,
                        RegEquity = security.RegEquity,
                        PnlIntraday = security.PnlIntraday,
                        Price = security.Price,
                        ReserateLong = security.ReserateLong,
                        ReserateShort = security.ReserateShort,
                        RiskrateLong = security.RiskrateLong,
                        RiskrateShort = security.RiskrateShort,
                        Seccode = security.Seccode,
                        Secid = security.Secid,
                        Selling = security.Selling
                    };
                    if (!UnitedPortfolioSecurityDataGridRowses.Select(x => x.Seccode).Contains(UPSrow.Seccode))
                        Dispatcher.Invoke(() => UnitedPortfolioSecurityDataGridRowses.Add(UPSrow));
                    else
                        Dispatcher.Invoke(() => 
                            UnitedPortfolioSecurityDataGridRowses[
                                UnitedPortfolioSecurityDataGridRowses.IndexOf(
                                    UnitedPortfolioSecurityDataGridRowses.First(x => x.Seccode == UPSrow.Seccode))] = UPSrow);
                    TotalBalance = UnitedPortfolioSecurityDataGridRowses.Sum(r => r.Pl);
                    
                    //foreach (var valuePart in security.ValueParts)
                    //{
                    //    ValuePartDataGridRowses.Add(new ValuePartDataGridRow
                    //    {
                    //        Balance = valuePart.Balance,
                    //        Bought = valuePart.Bought,
                    //        OpenBalance = valuePart.OpenBalance,
                    //        Register = valuePart.Register,
                    //        Settled = valuePart.Settled,
                    //        Sold = valuePart.Sold
                    //    });
                    //}
                }
            }
            foreach (var money in portfolio.Money)
            {
                var Mrow = new MoneyDataGridRow
                {
                    Balance = money.Balance,
                    Bought = money.Bought,
                    Name = money.Name,
                    OpenBalance = money.OpenBalance,
                    Settled = money.Settled,
                    Sold = money.Sold,
                    Tax = money.Tax
                };
                if (!MoneyDataGridRowses.Select(x => x.Name).Contains(Mrow.Name))
                    Dispatcher.Invoke(() => MoneyDataGridRowses.Add(Mrow));
                else
                    Dispatcher.Invoke(() => MoneyDataGridRowses[
                        MoneyDataGridRowses.IndexOf(
                            MoneyDataGridRowses.First(x => x.Name == Mrow.Name))] = Mrow);
                //foreach (var valuePart in money.ValueParts)
                //{
                //    ValuePartDataGridRowses.Add(new ValuePartDataGridRow
                //    {
                //        Balance = valuePart.Balance,
                //        Bought = valuePart.Bought,
                //        OpenBalance = valuePart.OpenBalance,
                //        Register = valuePart.Register,
                //        Settled = valuePart.Settled,
                //        Sold = valuePart.Sold
                //    });
                //}
            }
            //send realtime balance to the server
            SendBalanceToInsideServer();
        }

        private void SendBalanceToInsideServer()
        {
            try
            {
                MainWindowViewModel.Hub?.Invoke("RealtimeBalance", ClientInfo.InsideLogin, TotalBalance);
            }
            catch
            {
                //fail
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
