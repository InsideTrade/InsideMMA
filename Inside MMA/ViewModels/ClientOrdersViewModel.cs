using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Views;

namespace Inside_MMA.ViewModels
{
    class ClientOrdersViewModel : INotifyPropertyChanged
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;
        //Binding properties
        private ObservableCollection<Order> _clientOrders = new ObservableCollection<Order>();
        private ObservableCollection<Stoporder> _clientStoporders = new ObservableCollection<Stoporder>();
        public ObservableCollection<Order> ClientOrders
        {
            get
            {
                return _clientOrders;
            }

            set
            {
                _clientOrders = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Stoporder> ClientStoporders
        {
            get
            {
                return _clientStoporders;
            }

            set
            {
                _clientStoporders = value;
                OnPropertyChanged();
            }
        }

        public Order SelectedOrder
        {
            get
            {
                return _selectedOrder;
            }

            set
            {
                _selectedOrder = value;
                IsSelected = _selectedOrder != null;
                OnPropertyChanged();
            }
        }

        public Stoporder SelectedStoporder
        {
            get { return _selectedStoporder; }
            set
            {
                if (Equals(value, _selectedStoporder)) return;
                _selectedStoporder = value;
                IsSelected = _selectedStoporder != null;
                OnPropertyChanged();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        private int _activeOrders;
        private int _activeStoporders;
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

        public ICommand CancelOrder { get; set; }
        public ICommand CancelStopOrderCommand { get; set; }
        public ICommand CancelAllOrders { get; set; }
        public ICommand CancelAllStoporders { get; set; }

        public ICommand OpenWindowCommand { get; set; }
        private Order _selectedOrder;

        public ClientOrdersViewModel()
        {
            CancelOrder = new Command(arg => CancelOrderAction());
            CancelStopOrderCommand = new Command(arg => CancelStopOrder());
            OpenWindowCommand = new Command(OpenWindow);
            CancelAllOrders = new Command(arg => CancelAllOrdersMethod());
            CancelAllStoporders = new Command(arg => CancelAllStopordersMethod());
            TXmlConnector.SendNewOrders += XmlConnector_OnSendNewOrders;
        }

        public void CancelAllOrdersMethod()
        {
            Task.Run(() =>
            {
                var orders = ClientOrders.ToArray();
                foreach (var order in orders)
                {
                    if (order.Status != "matched")
                        TXmlConnector.ConnectorSendCommand(
                            $"<command id=\"cancelorder\"><transactionid>{order.Transactionid}</transactionid></command>");
                }
                Thread.Sleep(250);
            });
            
        }

        public void CancelAllStopordersMethod()
        {
            Task.Run(() => {
                var orders = ClientStoporders.ToArray();
                foreach (var order in orders)
                {
                    if (order.Status != "matched")
                        TXmlConnector.ConnectorSendCommand(
                            $"<command id=\"cancelstoporder\"><transactionid>{order.Transactionid}</transactionid></command>");
                    Thread.Sleep(250);
                }
            });
            
        }
        private void OpenWindow(object obj)
        {
            dynamic item;
            if (SelectedOrder != null)
                item = SelectedOrder;
            else
                item = SelectedStoporder;
            switch (obj.ToString())
            {
                case "level2":
                {
                    var view = new Level2();
                    view.DataContext = new Level2ViewModel(item.Board, item.Seccode, view);
                    view.Show();
                    break;
                }
                case "alltrades":
                {
                    var view = new AllTrades();
                    view.DataContext = new AllTradesViewModel(item.Board, item.Seccode, view);
                    view.Show();
                    break;
                }
                case "chart":
                {
                    var view = new SciChartWindow();
                    view.DataContext = new SciChartViewModel(item.Board, item.Seccode, view);
                    view.Show();
                    break;
                }
                case "orders":
                    if (MainWindowViewModel.CheckIfWindowIsOpened(typeof(ClientOrders))) break;
                    new ClientOrders { DataContext = MainWindowViewModel.ClientOrdersViewModel }.Show();
                    break;
                case "stoporders":
                    if (MainWindowViewModel.CheckIfWindowIsOpened(typeof(ClientStoporders))) break;
                    new ClientStoporders { DataContext = MainWindowViewModel.ClientOrdersViewModel }.Show();
                    break;
            }
        }


        private void CancelOrderAction()
        {
            var cmd =
                $"<command id=\"cancelorder\"><transactionid>{SelectedOrder.Transactionid}</transactionid></command>";
            TXmlConnector.ConnectorSendCommand($"<command id=\"cancelorder\"><transactionid>{SelectedOrder.Transactionid}</transactionid></command>");
        }

        private void CancelStopOrder()
        {
            TXmlConnector.ConnectorSendCommand($"<command id=\"cancelstoporder\"><transactionid>{SelectedStoporder.Transactionid}</transactionid></command>");
        }

        private Dispatcher _dispatcher => Application.Current.Dispatcher;
        private Stoporder _selectedStoporder;
       

        private void XmlConnector_OnSendNewOrders(string data)
        {
            var orders =
               (Orders)
                   new XmlSerializer(typeof(Orders)).Deserialize(
                       new StringReader(data));

            try
            {
                orders.Order = orders.Order.OrderBy(item => DateTime.Parse(item.Time)).ToList();
            }
            catch
            {
               
            }
            _dispatcher.Invoke(() => {
                foreach (var order in orders.Order)
                {
                    var found = ClientOrders.FirstOrDefault(item => item.Transactionid == order.Transactionid);

                    if (found == null)
                        ClientOrders.Insert(0, order);
                    else
                        ClientOrders[ClientOrders.IndexOf(found)] = order;
                }
                foreach (var stoporder in orders.Stoporder)
                {
                    var found = ClientStoporders.FirstOrDefault(item => item.Transactionid == stoporder.Transactionid);
                    if (found == null)
                        ClientStoporders.Insert(0, stoporder);
                    else
                        ClientStoporders[ClientStoporders.IndexOf(found)] = stoporder;
                }
            });

            ActiveOrders = ClientOrders.Count(o => o.Status == "active");
            ActiveStoporders = ClientStoporders.Count(o => o.Status == "watching");
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
