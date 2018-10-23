using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;
using Inside_MMA.ViewModels;
using Inside_MMA.Views;

namespace Inside_MMA.Models.Alerts
{
    public class LogBookAlert : BaseAlert
    {
        public LogBookViewModel Vm;
        public double Price { get; set; }
        public double Delta { get; set; } = 0.05;
        public string Buysell { get; set; }
        public string Size;
        public LogBookAlert(LogBookViewModel vm, string buysell, double price, int size, double delta)
        {
            Vm = vm;
            Buysell = buysell;
            Price = price;
            Board = vm.Board;
            Seccode = vm.Seccode;
            Delta = delta;
            Size = size.ToString();
        }
        protected override void TradeItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (Buysell)
            {
                case "B":
                    foreach (TradeItem trade in e.NewItems)
                    {
                        if (trade.Price < Price)
                        {
                            UninitializeLogBookAlert(this,
                                $"Ice Buy on {Price} of size {Size} has lost ");
                            ShowLogBookAlertResult("Buy", Price, Size, "lost");
                        }
                        if (trade.Price >= Price + Delta)
                        {
                            UninitializeLogBookAlert(this,
                                $"Ice Buy on {Price} of size {Size} has won ");
                            ShowLogBookAlertResult("Buy", Price, Size, "won");
                        }
                    }
                    break;
                case "S":
                    foreach (TradeItem trade in e.NewItems)
                    {
                        if (trade.Price > Price)
                        {
                            UninitializeLogBookAlert(this,
                                $"Ice Sell on {Price} of size {Size} has lost ");
                            ShowLogBookAlertResult("Sell", Price, Size, "lost");
                        }
                        if (trade.Price <= Price - Delta)
                        {
                            UninitializeLogBookAlert(this,
                                $"Ice Sell on {Price} of size {Size} has won ");
                            ShowLogBookAlertResult("Sell", Price, Size, "won");
                        }
                    }
                    break;
            }
            
        }
        protected void UninitializeLogBookAlert(LogBookAlert thisAlert, string info)
        {
            thisAlert.Vm.Save(info.Replace('.', '_'));
            thisAlert.Vm.Reset();
            Application.Current.Dispatcher.Invoke(() => thisAlert.Vm.Alerts.Remove(thisAlert));
            Uninitialize();
        }
        protected void ShowLogBookAlertResult(string buysell, double price, string size, string result)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => new AlertMessage(Board, Seccode, $"Ice {buysell} on {price} of size {size}\r\nhas {result} ", 20) { ShowActivated = false }
                    .Show()), DispatcherPriority.Background);
        }
    }
}