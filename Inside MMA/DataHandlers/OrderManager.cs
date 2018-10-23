using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;

namespace Inside_MMA.DataHandlers
{
    public delegate void NotifyOrderAdded(string board, string seccode, string id, double price, string buySell);
    public delegate void NotifyOrderDeleted(string board, string seccode, string id);
    public class OrderManager
    {
        public static event NotifyOrderAdded NotifyStoporderAdded;
        public static event NotifyOrderDeleted NotifyStoporderDeleted;
        public static event NotifyOrderAdded NotifyOrderAdded;
        public static event NotifyOrderDeleted NotifyOrderDeleted;
        private static ObservableCollection<Stoporder> _stoporders;
        private static ObservableCollection<Order> _orders;
        public OrderManager(ObservableCollection<Stoporder> stops, ObservableCollection<Order> orders)
        {
            _stoporders = stops;
            _stoporders.CollectionChanged += StopsCollectionChanged;
            _orders = orders;
            _orders.CollectionChanged += OrdersCollectionChanged;
        }

        private void OrdersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Order order in e.NewItems)
                    {
                        if (order.Status == "active")
                            NotifyOrderAdded?.Invoke(order.Board, order.Seccode, order.Transactionid,
                                Convert.ToDouble(order.Price), order.Buysell);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Order order in e.NewItems)
                    {
                        NotifyOrderDeleted?.Invoke(order.Board, order.Seccode, order.Transactionid);
                    }
                    break;
            }
        }

        private void StopsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Stoporder stop in e.NewItems)
                    {
                        if (stop.Status == "watching")
                            NotifyStoporderAdded?.Invoke(stop.Board, stop.Seccode, stop.Transactionid,
                                Convert.ToDouble(stop.Stoploss[0].Activationprice), stop.Buysell);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Stoporder stop in e.NewItems)
                    {
                        NotifyStoporderDeleted?.Invoke(stop.Board, stop.Seccode, stop.Transactionid);
                    }
                    break;
            }
        }

        public static IEnumerable<Stoporder> GetActiveStoporders(string board, string seccode)
        {
            return _stoporders.Where(s => s.Status == "watching" && s.Board == board && s.Seccode == seccode);
        }
        public static IEnumerable<Order> GetActiveOrders(string board, string seccode)
        {
            return _orders.Where(s => s.Status == "active" && s.Board == board && s.Seccode == seccode);
        }

        public static void EditStoporder(string id, double price)
        {
            var editedStoporder = _stoporders.First(s => s.Transactionid == id);
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"cancelstoporder\"><transactionid>{editedStoporder.Transactionid}</transactionid></command>");
            var res = TXmlConnector.ConnectorSendCommand(ConnectorCommands.NewStopLoss(editedStoporder.Board,
                editedStoporder.Seccode, editedStoporder.Client,
                ClientSelector.GetClient(editedStoporder.Board)[1], editedStoporder.Buysell,
                price.ToString(), editedStoporder.Stoploss[0].Orderprice ?? "",
                editedStoporder.Stoploss[0].Quantity, editedStoporder.Stoploss[0].IsByMarket,
                editedStoporder.Stoploss[0].Usecredit != "no"));
        }

        public static void EditOrder(string id, double price)
        {
            var editedOrder = _orders.First(o => o.Transactionid == id);
            var useCredit = editedOrder.Board != "FUT" ? "<usecredit/>" : "";
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"cancelorder\"><transactionid>{id}</transactionid></command>");
            var res = TXmlConnector.ConnectorSendCommand("<command id=\"neworder\"><security><board>" + editedOrder.Board +
                                                     "</board><seccode>" + editedOrder.Seccode +
                                                     "</seccode></security><client>" + editedOrder.Client + "</client><union>" + editedOrder.Union + "</union><price>" + price + "</price><quantity>" +
                                                     editedOrder.Quantity + "</quantity><buysell>" + editedOrder.Buysell +
                                                     "</buysell>" + useCredit + "</command>");
        }
    }
}
