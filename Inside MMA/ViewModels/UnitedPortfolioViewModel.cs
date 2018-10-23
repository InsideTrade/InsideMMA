using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Views;
using UnitedPortfolio = Inside_MMA.Models.UnitedPortfolio;

namespace Inside_MMA.ViewModels
{
    public class UnitedPortfolioViewModel : INotifyPropertyChanged
    {
        private List<Client> Clients => MainWindowViewModel.ClientsViewModel.Clients ; 

        public event PropertyChangedEventHandler PropertyChanged;

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
        public ICommand ConfirmCommand { get; set; }
        public UnitedPortfolioViewModel()
        {
            ConfirmCommand = new Command(arg => Refresh());
            var cmd = $"<command id=\"get_united_portfolio\" client=\"{ClientInfo.Id}\" />";
            TXmlConnector.SendNewUnitedPortfolio += XmlConnector_OnSendUnitesPortfolio;
            TXmlConnector.ConnectorSendCommand(cmd);
        }

        private void Refresh()
        {
            //var cmd = IsClientChecked
            //    ? $"<command id=\"get_united_portfolio\" client=\"{UnionOrClient}\" />"
            //    : $"<command id=\"get_united_portfolio\" union=\"{UnionOrClient}\" />";

            UnitedPortfolioDataGridRows.Clear();
            UnitedPortfolioSecurityDataGridRowses.Clear();
            AssetDataGridRowses.Clear();
            MoneyDataGridRowses.Clear();
            ValuePartDataGridRowses.Clear();
            
            TXmlConnector.SendNewUnitedPortfolio += XmlConnector_OnSendUnitesPortfolio;
            TXmlConnector.ConnectorSendCommand($"<command id=\"get_united_portfolio\" client=\"{ClientInfo.Id}\" />");
        }

        private static Dispatcher Dispatcher => Application.Current.Dispatcher;
        private void XmlConnector_OnSendUnitesPortfolio(string data)
        {
            UnitedPortfolio portfolio = null;
            Dispatcher.Invoke(() =>
            {
                portfolio =
                    (UnitedPortfolio)
                        new XmlSerializer(typeof (UnitedPortfolio), new XmlRootAttribute("united_portfolio"))
                            .Deserialize(
                                new StringReader(data));
            });

            Dispatcher.Invoke(() =>
            {
                UnitedPortfolioDataGridRows.Add(new UnitedPortfolioDataGridRow()
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
                });
            });
            Dispatcher.Invoke(() =>
            {
                foreach (var asset in portfolio.Assets)
                {
                    AssetDataGridRowses.Add(new AssetDataGridRow()
                    {
                        Code = asset.Code,
                        InitReq = asset.InitReq,
                        MaintReq = asset.MaintReq,
                        Name = asset.Name,
                        SetoffRate = asset.SetoffRate
                    });
                    foreach (var security in asset.Securities)
                    {
                        UnitedPortfolioSecurityDataGridRowses.Add(new UnitedPortfolioSecurityDataGridRow()
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
                        });
                        foreach (var valuePart in security.ValueParts)
                        {
                            ValuePartDataGridRowses.Add(new ValuePartDataGridRow()
                            {
                                Balance = valuePart.Balance,
                                Bought = valuePart.Bought,
                                OpenBalance = valuePart.OpenBalance,
                                Register = valuePart.Register,
                                Settled = valuePart.Settled,
                                Sold = valuePart.Sold
                            });
                        }
                    }
                }
            });
            Dispatcher.Invoke(() =>
            {
                foreach (var money in portfolio.Money)
                {
                    MoneyDataGridRowses.Add(new MoneyDataGridRow()
                    {
                        Balance = money.Balance,
                        Bought = money.Bought,
                        Name = money.Name,
                        OpenBalance = money.OpenBalance,
                        Settled = money.Settled,
                        Sold = money.Sold,
                        Tax = money.Tax
                    });
                    foreach (var valuePart in money.ValueParts)
                    {
                        ValuePartDataGridRowses.Add(new ValuePartDataGridRow()
                        {
                            Balance = valuePart.Balance,
                            Bought = valuePart.Bought,
                            OpenBalance = valuePart.OpenBalance,
                            Register = valuePart.Register,
                            Settled = valuePart.Settled,
                            Sold = valuePart.Sold
                        });
                    }
                }
            });
            TXmlConnector.SendNewUnitedPortfolio -= XmlConnector_OnSendUnitesPortfolio;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
