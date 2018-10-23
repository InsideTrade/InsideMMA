using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using SciChart.Charting.Model.DataSeries;
using Trade = InsideDB.Trade;

namespace Inside_MMA.ViewModels
{
    public class BalanceReportChartViewModel : INotifyPropertyChanged
    {
        private static Dispatcher Dispatcher => Application.Current.Dispatcher;
        private ObservableCollection<TabItem> _tabItems = new ObservableCollection<TabItem>();
        private DateTime _from;
        private DateTime _to;
        public class TabItem
        {
            public string Board { get; set; }
            public string Seccode { get; set; }
            public string Header { get; set; }

            public OhlcDataSeries<DateTime, double> Candlesticks { get; set; } =
                new OhlcDataSeries<DateTime, double> {AcceptsUnsortedData = true};

            public XyDataSeries<DateTime, double> BuySeries { get; set; } =
                new XyDataSeries<DateTime, double> { AcceptsUnsortedData = true };

            public XyDataSeries<DateTime, double> SellSeries { get; set; } =
                new XyDataSeries<DateTime, double> { AcceptsUnsortedData = true };

            public TabItem(string board, string seccode)
            {
                Board = board;
                Seccode = seccode;
                Header = $"{Board} {Seccode}";
            }
        }

        public ObservableCollection<TabItem> TabItems
        {
            get { return _tabItems; }
            set
            {
                if (Equals(value, _tabItems)) return;
                _tabItems = value;
                OnPropertyChanged();
            }
        }

        public BalanceReportChartViewModel(List<Trade> trades, DateTime fromValue, DateTime toValue)
        {
            TXmlConnector.SendNewCandles += ProcessCandles;
            _from = fromValue;
            _to = toValue;
            var instruments = trades.Select(t => new { t.Board, t.Seccode }).Distinct().ToArray();
            foreach (var i in instruments)
            {
                var tabItem = new TabItem(i.Board, i.Seccode);
                var buys = trades.Where(tr => tr.Board == i.Board && tr.Seccode == i.Seccode).Where(ct => ct.Buysell == "B").ToArray();
                var sells = trades.Where(tr => tr.Board == i.Board && tr.Seccode == i.Seccode).Where(ct => ct.Buysell == "S").ToArray();
                tabItem.BuySeries.Append(buys.Select(ct => ct.Time), buys.Select(ct => ct.Price));
                tabItem.SellSeries.Append(sells.Select(ct => ct.Time), sells.Select(ct => ct.Price));
                Dispatcher.Invoke(() => TabItems.Add(tabItem));
                TXmlConnector.ConnectorSendCommand(
                    $"<command id=\"gethistorydata\"><security><board>{i.Board}</board><seccode>{i.Seccode}</seccode></security><period>2</period><count>{120 + (DateTime.Now - _from).Days * 120}</count><reset>true</reset></command>");
            }
            Task.Run(() =>
            { 
                Thread.Sleep(2000);
                TXmlConnector.SendNewCandles -= ProcessCandles;
            });
        }

        private void ProcessCandles(string data)
        {
            Candles candles;
            using (var reader = new StringReader(data))
            {
                var candleSerializer = new XmlSerializer(typeof(Candles));
                candles =
                    (Candles)
                    candleSerializer.Deserialize(reader);
                reader.Close();
            }
            var list = candles.Candle.Where(
                item =>
                    item.TradeTime.Date >= _from && item.TradeTime.Date <= _to).ToList();

            if (list.Count == 0) return;
            var dataSeries = TabItems.First(t => t.Board == candles.Board && t.Seccode == candles.Seccode).Candlesticks;
            dataSeries.Append(GetDateTime(list), GetValues(list));
            Thread.Sleep(100);
            Dispatcher.Invoke(() => dataSeries.InvalidateParentSurface(RangeMode.ZoomToFit));
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