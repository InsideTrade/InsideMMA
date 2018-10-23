using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Views;
using Trade = InsideDB.Trade;

namespace Inside_MMA.ViewModels
{
    public class ClientTradesViewModel : INotifyPropertyChanged
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;

        private ObservableCollection<ClientTrade> _clientTrades = new ObservableCollection<ClientTrade>();

        public ObservableCollection<ClientTrade> ClientTrades
        {
            get
            {
                return _clientTrades;
            }

            set
            {
                _clientTrades = value;
                OnPropertyChanged();
            }
        }

        public ClientTrade SelectedTrade
        {
            get { return _selectedTrade; }
            set
            {
                if (Equals(value, _selectedTrade)) return;
                _selectedTrade = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClosingCommand { get; set; }
        public ICommand OpenWindowCommand { get; set; }
        public ClientTradesViewModel()
        {
            TXmlConnector.SendNewTrades += XmlConnector_OnSendNewTrades;
            OpenWindowCommand = new Command(OpenWindow);
        }
        private void OpenWindow(object obj)
        {
            switch (obj.ToString())
            {
                case "level2":
                {
                    var view = new Level2();
                    view.DataContext = new Level2ViewModel(SelectedTrade.Board, SelectedTrade.Seccode, view);
                    view.Show();
                    break;
                }
                case "alltrades":
                {
                    var view = new AllTrades();
                    view.DataContext = new AllTradesViewModel(SelectedTrade.Board, SelectedTrade.Seccode, view);
                    view.Show();
                    break;
                }
                case "chart":
                {
                    var view = new SciChartWindow();
                    view.DataContext = new SciChartViewModel(SelectedTrade.Board, SelectedTrade.Seccode, view);
                    view.Show();
                    break;
                }
                case "orders":
                    if (MainWindowViewModel.CheckIfWindowIsOpened(typeof(ClientOrders))) break;
                    new ClientOrders {DataContext = MainWindowViewModel.ClientOrdersViewModel}.Show();
                    break;
                case "stoporders":
                    if (MainWindowViewModel.CheckIfWindowIsOpened(typeof(ClientStoporders))) break;
                    new ClientStoporders { DataContext = MainWindowViewModel.ClientOrdersViewModel }.Show();
                    break;
            }
        }

        private XmlSerializer _serializer = new XmlSerializer(typeof(List<ClientTrade>), new XmlRootAttribute("trades"));
        private ClientTrade _selectedTrade;

        private void XmlConnector_OnSendNewTrades(string data)
        {
            var list =
                (List<ClientTrade>)_serializer.Deserialize(
                        new StringReader(data));
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var trade in list)
                {
                    ClientTrades.Insert(0, trade);
                }
            });
            SendTradesToInsideServer(list);
        }

        private void SendTradesToInsideServer(List<ClientTrade> trades)
        {
            var newTrades = trades.Select(trade => new Trade
                {
                    Login = ClientInfo.InsideLogin,
                    Tradeno = trade.Tradeno,
                    Seccode = trade.Seccode,
                    Price = trade.Price,
                    Quantity = trade.Quantity,
                    Time = DateTime.Parse(trade.Time),
                    Board = trade.Board,
                    Buysell = trade.Buysell,
                    Lotsize = trade.Items,
                    CurrentPos = trade.Currentpos
                })
                .ToList();
            MainWindowViewModel.Hub?.Invoke("NewTrades", ClientInfo.InsideLogin, newTrades);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
