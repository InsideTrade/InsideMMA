using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Models.Alerts;
using Inside_MMA.Models.Inside_MMA.Models;
using Inside_MMA.Views;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public class LogbookColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            return (double) values[0] <= (double) values[1]
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Color.FromRgb(255, 82, 82));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EatenSizeInRangeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (int)values[0] >= (int)values[1];
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //public class IsIceConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {

    //        return (double)values[0] <= (double)values[1]
    //            ? new SolidColorBrush(Colors.Green)
    //            : new SolidColorBrush(Color.FromRgb(255, 82, 82));
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class LogBookViewModel : RememberPlacement
    {
        private static XmlSerializer _logbookSerializer = new XmlSerializer(typeof(ObservableCollection<LogBookItem>));
        private ObservableCollection<Level2Item> _level2Items;

        public SynchronizedCollection<Security> Level2Subs
            => ((MainWindowViewModel) Application.Current.MainWindow.DataContext).Level2Subs;

        private ObservableCollection<LogBookAlert> _alerts = new ObservableCollection<LogBookAlert>();
        public ObservableCollection<LogBookAlert> Alerts
        {
            get { return _alerts; }
            set
            {
                _alerts = value;
                OnPropertyChanged();
            }
        }

        private List<Security> AllTradesSubs => ((MainWindowViewModel) Application.Current.MainWindow.DataContext)
            .AllTradesSubs;

        private ObservableCollection<LogBookItem> _logBookCollection = new ObservableCollection<LogBookItem>();

        
        

        public ObservableCollection<LogBookItem> LogBookCollection
        {
            get { return _logBookCollection; }
            set
            {
                if (Equals(value, _logBookCollection)) return;
                _logBookCollection = value;
                OnPropertyChanged();
            }
        }

        public double BestBuy
        {
            get { return _bestBuy; }
            set
            {
                if (value.Equals(_bestBuy)) return;
                _bestBuy = value;
                //foreach (var item in LogBookCollection)
                //{
                //    item.Color = item.Price <= _bestBuy
                //        ? Colors.Green
                //        : Color.FromRgb(255, 82, 82);
                //}
                OnPropertyChanged();
            }
        }

        private double _bestBuy;
        private int _coef = 2;

        public int Coef
        {
            get { return _coef; }
            set
            {
                if (value == _coef) return;
                _coef = value;
                //foreach (var item in LogBookCollection)
                //{
                //    item.Coef = _coef;
                //}
                LogBookCollection.ForEachDo(i => i.RecalculateIce());
                OnPropertyChanged();
                var args = GetWindowArgs() as LogBookArgs;
                args.Ratio = _coef;
                UpdateWindowArgs(args);
            }
        }
        private int _size = 1;
        public int Size
        {
            get { return _size; }
            set
            {
                if (value == _size) return;
                _size = value;
                OnPropertyChanged();
                //foreach (var item in LogBookCollection)
                //{
                //    item.Size = _size;
                //}
                LogBookCollection.ForEachDo(i => i.RecalculateIce());
                var args = (LogBookArgs)GetWindowArgs();
                args.Size = _size;
                UpdateWindowArgs(args);
            }
        }
        private double _triggerDelta = 0.5;
        public double TriggerDelta
        {
            get { return _triggerDelta; }
            set
            {
                if (value.Equals(_triggerDelta)) return;
                _triggerDelta = value;
                OnPropertyChanged();
                var args = (LogBookArgs)GetWindowArgs();
                args.TriggerDelta = _triggerDelta;
                UpdateWindowArgs(args);
            }
        }

        public int EatenSize
        {
            get { return _eatenSize; }
            set
            {
                if (value == _eatenSize) return;
                _eatenSize = value;
                OnPropertyChanged();
            }
        }

        public bool IsAlerting
        {
            get { return _isAlerting; }
            set
            {
                if (value == _isAlerting) return;
                _isAlerting = value;
                OnPropertyChanged();
                var args = (LogBookArgs)GetWindowArgs();
                args.Alert = _isAlerting;
                UpdateWindowArgs(args);
            }
        }
        public ICommand ClosingCommand { get; set; }
        public ICommand ResetCommand { get; set; }

        public LogBookViewModel(string board, string seccode, Window window, int id = 0)
        {
            Window = window;
            ClosingCommand = new Command(arg => Closing());
            ResetCommand = new Command(arg => Reset());
            Board = board;
            Seccode = seccode;
            _level2Items = Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            TXmlConnector.SendNewQuotes += XmlConnector_OnSendNewQuotes;
            Level2Subs.Add(new Security {Board = Board, Seccode = Seccode});
            TXmlConnector.ConnectorSendCommand(
                ConnectorCommands.SubUnsubCommand("subscribe", "alltrades", Board, Seccode));
            TXmlConnector.SendNewAllTrades += XmlConnector_OnSendNewTrades;
            AllTradesSubs.Add(new Security {Board = Board, Seccode = Seccode});
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
            var args = GetWindowArgs() as LogBookArgs;
            if (args == null)
            {
                UpdateWindowArgs(new LogBookArgs {Alert = false, Ratio = 2, Size = 1, TriggerDelta = 0.5});
                return;
            }
            Coef = args.Ratio;
            Size = args.Size;
            IsAlerting = args.Alert;
            TriggerDelta = args.TriggerDelta;
        }

        public void Alert(string text, double price, int size1, int size2)
        {
            if (!IsAlerting) return;
            Dispatcher.Invoke(
                () => new AlertMessage(Board, Seccode, $"{text} {price}\r\n{size1} {size2}", 20) {ShowActivated = false}
                    .Show());

        }

        private XmlSerializer _tradeSerializer =
            new XmlSerializer(typeof(List<Trade>), new XmlRootAttribute("alltrades"));

        private void XmlConnector_OnSendNewTrades(string data)
        {
            if (LogBookCollection.Count == 0) return;
            List<TradeItem> list;
            using (var stringReader = new StringReader(data))
            {
                list = (from x in (List<Trade>) _tradeSerializer.Deserialize(stringReader)
                    where x.Board == Board && x.Seccode == Seccode
                    select
                    new TradeItem(x.Seccode, x.Price, x.Quantity, DateTime.Parse(x.Time).ToString("hh:mm:ss.fff"),
                        x.Buysell, x.Quantity.ToString())).ToList();
                stringReader.Close();
            }
            foreach (var tradeItem in list)
            {
                var log = LogBookCollection.FirstOrDefault(l => l.Price == tradeItem.Price);
                if (log == null)
                {
                    var val = new LogBookItem(this)
                    {
                        Price = tradeItem.Price,
                        Buy = 0,
                        Sell = 0,
                        MaxSize = 0,
                        CurrentSize = 0,
                        MaxBuy = 0,
                        MaxSell = 0,
                        //Color = tradeItem.Buysell == "B" ? Colors.Green : Color.FromRgb(255, 82, 82),
                        //Size = Size,
                        //Coef = Coef
                    };
                    if (tradeItem.Buysell == "B")
                    {
                        val.Buy = tradeItem.Quantity;
                    }

                    else
                    {
                        val.Sell = tradeItem.Quantity;
                    }
                }
                else if (tradeItem.Buysell == "B")
                {
                    log.Buy += tradeItem.Quantity;
                }
                else
                {
                    log.Sell += tradeItem.Quantity;
                }
            }
        }

        public void Reset()
        {
            LogBookCollection.Clear();
        }

        public void Save(string info)
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\logbooks");
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                       "\\Inside MMA\\logbooks\\" + Board + " " + Seccode + " " + info + DateTime.Now.ToString().Replace(":", "");
            using (
                var fileStream =
                    new FileStream(path, FileMode.Create))

            {
                _logbookSerializer.Serialize(fileStream, LogBookCollection);
                fileStream.Close();
            }
        }
        private void Closing()
        {
            TXmlConnector.SendNewQuotes -= XmlConnector_OnSendNewQuotes;
            TXmlConnector.SendNewAllTrades -= XmlConnector_OnSendNewTrades;
            var atsub = new Security {Board = Board, Seccode = Seccode};
            AllTradesSubs.Remove(atsub);
            if (!AllTradesSubs.Contains(atsub))
                ConnectorCommands.SubUnsubCommand("unsubscribe", "alltrades", Board, Seccode);
            UnsubscribeFromWindowEvents();
            CloseWindow();
            GC.Collect();
        }

        private Dispatcher Dispatcher = Application.Current.Dispatcher;
        private XmlSerializer _serializer = new XmlSerializer(typeof(List<Quote>), new XmlRootAttribute("quotes"));
        private bool _isAlerting;
        private int _eatenSize = 1;


        private static readonly object Lock = new object();

        private void XmlConnector_OnSendNewQuotes(string data)
        {
            if (_level2Items.Count == 0) return;
            List<Level2Item> list;
            using (var reader = new StringReader(data))
            {
                var quotes =
                    ((List<Quote>)
                        _serializer.Deserialize(reader)).Where(item => item.Board == Board && item.Seccode == Seccode)
                    .ToList();
                if (quotes.Count == 0) return;
                list =
                    quotes.Where(item => item.Buy == 0)
                        .Select(
                            item => new Level2Item {BuySell = "sell", Quantity = item.Sell, Price = item.Price})
                        .OrderByDescending(item => item.Price)
                        .Take(20)
                        .Union(quotes.Where(item => item.Sell == 0)
                            .Select(
                                item =>
                                    new Level2Item {BuySell = "buy", Quantity = item.Buy, Price = item.Price})
                            .OrderByDescending(item => item.Price)
                            .Take(20))
                        .ToList();
            }
            double bestBuy = 0;
            try
            {
                bestBuy = _level2Items.Where(l2 => l2.BuySell == "buy").Max(l2 => l2.Price);
            }
            catch
            { 
            }
            Dispatcher.Invoke(() =>
            {
                if (_logBookCollection.Count == 0)
                {
                    foreach (var item in _level2Items)
                    {
                        LogBookCollection.Add(new LogBookItem(this)
                        {
                            Price = item.Price,
                            CurrentSize = item.Quantity,
                            MaxSize = item.Quantity,
                            //Buysell = item.BuySell,
                            Buy = 0,
                            Sell = 0,
                            MaxBuy = item.BuySell == "buy" ? item.Quantity : 0,
                            MaxSell = item.BuySell == "sell" ? item.Quantity : 0,
                            //Color = item.BuySell == "buy" ? Colors.Green : Color.FromRgb(255, 82, 82),
                            //Size = Size,
                            //Coef = Coef
                        });

                        BestBuy = bestBuy;
                    }
                }

                else
                {
                    foreach (var item in list)
                    {
                        var found = _logBookCollection.FirstOrDefault(x => x.Price == item.Price);
                        if (found != null)
                        {
                            found.CurrentSize = item.Quantity;
                            if (found.MaxSize < item.Quantity)
                                found.MaxSize = item.Quantity;
                            if (item.BuySell == "buy")
                            {
                                if (item.Quantity > found.MaxBuy)
                                    found.MaxBuy = item.Quantity;
                            }
                            else
                            {
                                if (item.Quantity > found.MaxSell)
                                    found.MaxSell = item.Quantity;
                            }
                            found.CurrentSize = item.Quantity;
                            if (found.CurrentSize > found.MaxSize)
                                found.MaxSize = found.CurrentSize;
                        }
                        else
                        {
                            LogBookCollection.Add(new LogBookItem(this)
                            {
                                Price = item.Price,
                                CurrentSize = item.Quantity,
                                MaxSize = item.Quantity,
                                //Buysell = item.BuySell,
                                Buy = 0,
                                Sell = 0,
                                MaxBuy = item.BuySell == "buy" ? item.Quantity : 0,
                                MaxSell = item.BuySell == "sell" ? item.Quantity : 0,
                                //Color = item.BuySell == "buy" ? Colors.Green : Color.FromRgb(255, 82, 82)
                            });
                        }
                        BestBuy = bestBuy;
                    }
                }
            });
        }

    }
}
