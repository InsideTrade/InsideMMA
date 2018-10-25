using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Inside_MMA.Models;
using Inside_MMA.Views;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;

namespace Inside_MMA.ViewModels
{
    public delegate void AllTradesCandlestickDataLoaded();
    public class AllTradesCandlestickViewModel : INotifyPropertyChanged
    {

        public event AllTradesCandlestickDataLoaded AllTradesCandlestickDataLoaded;
        private static Dispatcher Dispatcher => Dispatcher.CurrentDispatcher;
        //private AnnotationCollection _annotations = new AnnotationCollection();
        //public AnnotationCollection Annotations
        //{
        //    get { return _annotations; }
        //    set
        //    {
        //        if (Equals(value, _annotations)) return;
        //        _annotations = value;
        //        OnPropertyChanged();
        //    }
        //}

        public OhlcDataSeries<DateTime, double> OhlcDataSeries
        {
            get { return _ohlcDataSeries; }
            set
            {
                if (Equals(value, _ohlcDataSeries)) return;
                _ohlcDataSeries = value;
                OnPropertyChanged();
            }
        }

        public XyDataSeries<DateTime, double> BuySeries
        {
            get { return _buySeries; }
            set
            {
                if (Equals(value, _buySeries)) return;
                _buySeries = value;
                OnPropertyChanged();
            }
        }

        public XyDataSeries<DateTime, double> SellSeries
        {
            get { return _sellSeries; }
            set
            {
                if (Equals(value, _sellSeries)) return;
                _sellSeries = value;
                OnPropertyChanged();
            }
        }

        public string Seccode { get; set; }
        public string Title { get; set; }
        private OhlcDataSeries<DateTime, double> _ohlcDataSeries = new OhlcDataSeries<DateTime, double>();
        private XyDataSeries<DateTime, double> _buySeries = new XyDataSeries<DateTime, double> {AcceptsUnsortedData = true};
        private XyDataSeries<DateTime, double> _sellSeries = new XyDataSeries<DateTime, double> { AcceptsUnsortedData = true };
        public ICommand Closing { get; set; }
        public AllTradesCandlestickViewModel(string board, string seccode, int size, List<TradeItem> data)
        {
            Closing = new Command(arg => WindowClosing());
            Title = $"Size selector {seccode}: {size}";
            Seccode = seccode;
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"gethistorydata\"><security><board>{board}</board><seccode>{seccode}</seccode></security><period>2</period><count>120</count><reset>true</reset></command>");

            TXmlConnector.SendNewCandles += ProcessCandles;

            foreach (var tradeItem in data)
            {
                if (tradeItem.Buysell == "B")
                    BuySeries.Append(DateTime.Parse(tradeItem.Time), tradeItem.Price);
                else
                    SellSeries.Append(DateTime.Parse(tradeItem.Time), tradeItem.Price);
            }
            
        }
        
        public AllTradesCandlestickViewModel(string board, string seccode, List<DataForCandlestick> data, bool lmt = false)
        {
            Closing = new Command(arg => WindowClosing());
            Title = $"Size selector {seccode}: {string.Join(", ", data.Select(item => item.Quantity))}";
            Seccode = seccode;
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"gethistorydata\"><security><board>{board}</board><seccode>{seccode}</seccode></security><period>2</period><count>120</count><reset>true</reset></command>");

            TXmlConnector.SendNewCandles += ProcessCandles;

            foreach (var dataForCandlestick in data)
            {
                if (lmt)
                {
                    
                    foreach (var tickItem in dataForCandlestick.DataTick)
                    {
                        if (tickItem.Buysell == "B")
                            BuySeries.Append(DateTime.Parse(tickItem.Tradetime), tickItem.Price);
                        else
                            SellSeries.Append(DateTime.Parse(tickItem.Tradetime), tickItem.Price);
                    }
                }
                else
                {
                    foreach (var tradeItem in dataForCandlestick.Data)
                    {
                        if (tradeItem.Buysell == "B")
                            BuySeries.Append(DateTime.Parse(tradeItem.Time), tradeItem.Price);
                        else
                            SellSeries.Append(DateTime.Parse(tradeItem.Time), tradeItem.Price);
                    }
                }
            }
            
        }

        private void WindowClosing()
        {
            OhlcDataSeries.Clear();
            BuySeries.Clear();
            SellSeries.Clear();
        }
        
        private void ProcessCandles(string data)
        {
            List<Candle> list;
            Candles candles;
            using (var reader = new StringReader(data))
            {
                list = new List<Candle>();
                var candleSerializer = new XmlSerializer(typeof(Candles));
                candles =
                    (Candles)
                    candleSerializer.Deserialize(reader);
                reader.Close();
            }
            //check if seccode matches 
            if (candles.Seccode != Seccode)
                return;
            list = candles.Candle.Where(
                    item =>
                        item.TradeTime.Date >= DateTime.Today).ToList();

            if (list.Count == 0) return;
            OhlcDataSeries.Append(GetDateTime(list), GetValues(list));
            TXmlConnector.SendNewCandles -= ProcessCandles;
        }
        private IEnumerable<DateTime> GetDateTime(List<Candle> list)
        {
            return list.Select(item => item.TradeTime);
        }

        private IEnumerable<double>[] GetValues(List<Candle> list)
        {
            var array = new[]
            {
                list.Select(item => item.Open), list.Select(item => item.High), list.Select(item => item.Low),
                list.Select(item => item.Close)
            };
            return array;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}