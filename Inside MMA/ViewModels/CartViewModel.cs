using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Views;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inside_MMA.ViewModels
{
    public class CartViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CartItem> _orders = new ObservableCollection<CartItem>();
        public ObservableCollection<CartItem> Orders
        {
            get { return _orders; }
            set
            {
                if (Equals(value, _orders)) return;
                _orders = value;
                OnPropertyChanged();
            }
        }
        public ICommand Send { get; set; }
        public ICommand Delete { get; set; }
        public ICommand Closing { get; set; }
        public IDialogCoordinator Dialog { get; set; }

        public CartViewModel()
        {
            Send = new Command(arg => SendOrders());
            Delete = new Command(DeleteItem);
            GetCartFromFile();
            Orders.CollectionChanged += Orders_CollectionChanged;
        }

        private void DeleteItem(object obj)
        {
            Orders.Remove((CartItem)obj);
        }

        private void GetCartFromFile()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/cart";
            try
            {
                var data = File.ReadAllText(path);
                Orders = (ObservableCollection<CartItem>)
                    ((JArray)JsonConvert.DeserializeObject(data)).ToObject(typeof(ObservableCollection<CartItem>));
            }
            catch { }
            
        }

        private void Orders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveToFile();
        }

        private void SaveToFile()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/cart";
            File.WriteAllText(path, JsonConvert.SerializeObject(Orders));
            
        }

        private void SendOrders()
        {
            Task.Run(() =>
            {
                var result = string.Empty;
                foreach (var order in Orders)
                {
                    if (!order.Mkt)
                        result = TXmlConnector.ConnectorSendCommand(
                            $"<command id=\"neworder\"><security><board>{order.Board}</board><seccode>{order.Seccode}</seccode></security><client>{order.Client}</client><union>{order.Union}</union><price>{order.Price}</price><quantity>{order.Size}</quantity><buysell>{order.BuySell}</buysell></command>");
                    else
                    {
                        result = TXmlConnector.ConnectorSendCommand("<command id=\"neworder\"><security><board>" + order.Board +
                                                                        "</board><seccode>" + order.Seccode +
                                                                        "</seccode></security><client>" + order.Client +
                                                                        "</client><union>" + order.Union + "</union><quantity>" +
                                                                        order.Size + "</quantity><buysell>" + order.BuySell +
                                                                        "</buysell><bymarket/>" + "</command>");
                    }
                }
            });
            SaveToFile();
        }

        //private async void OutputResult(string result)
        //{
        //    var x = await Dialog.ShowMessageAsync(this, "Result", result);
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}