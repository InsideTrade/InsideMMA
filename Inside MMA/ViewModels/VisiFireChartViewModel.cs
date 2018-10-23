using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using MVVM_Solution.Annotations;
using MVVM_Solution.Models;
using MVVM_Solution.Views;
using Color = System.Drawing.Color;

namespace MVVM_Solution.ViewModels
{
    public class VisiFireChartViewModel : INotifyPropertyChanged
    {
        private static List<SecurityForTicks> _subscriptions = new List<SecurityForTicks>();
        private ObservableCollection<Candle> _candles = new ObservableCollection<Candle>();
        private List<Tick> _ticks = new List<Tick>();
        private string _board;
        private string _seccode;
        private int _timeframe = 5;
        private int _periodId = 2;
        private int _historyCount;

        //todo
        public List<int> TimeFrames { get; } = new List<int>()
        {
            1,
            5,
            15,
            60
        };

        public ObservableCollection<Candle> Candles {
            get { return _candles; }
            set
            {
                _candles = value;
                OnPropertyChanged();
            }
        }
        private int _selectedTimeframe = 5;
        public int SelectedItem
        {
            get
            {
                return _selectedTimeframe;
            }

            set
            {
                _selectedTimeframe = value;
                //todo
                switch (_selectedTimeframe)
                {
                    case 1:
                        {
                            _timeframe = 1;
                            _periodId = 1;
                            if (_daysBack != 0) GetHistory();
                            break;
                        }
                    case 5:
                        {
                            _periodId = 2;
                            _timeframe = 5;
                            if (_daysBack != 0) GetHistory();
                            break;
                        }
                    case 15:
                        {
                            _periodId = 3;
                            _timeframe = 15;
                            if (_daysBack != 0) GetHistory();
                            break;
                        }
                    case 60:
                        {
                            _periodId = 4;
                            _timeframe = 60;
                            if (_daysBack != 0) GetHistory();
                            break;
                        }

                }
                if (_daysBack != 0)
                    Refresh();
                OnPropertyChanged();
            }
        }

        private string _selectedPeriod = "Today";

        public string SelectedPeriod
        {
            get
            {
                return _selectedPeriod;
            }

            set
            {
                _selectedPeriod = value;
                switch (_selectedPeriod)
                {
                    case "Today":
                    {
                        _daysBack = 0;
                            _historyCount = 0;
                            Candles = new ObservableCollection<Candle>();
                            break;
                        }
                    case "2 days":
                    {
                            _daysBack = 1;
                            GetHistory();
                            break;
                        }
                    case "3 days":
                        {
                            _daysBack = 2;
                            GetHistory();
                            break;
                        }
                }
                OnPropertyChanged();
            }
        }

        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        public VisiFireChartViewModel() { }
        public VisiFireChartViewModel(string board, string seccode)
        {
            _board = board;
            _seccode = seccode;
            TXmlConnector.SendNewTicks += TicksToCandles;
            var sub = new SecurityForTicks {Board = board, Seccode = seccode, Tradeno = "1"};

            _subscriptions.Add(new SecurityForTicks {Board = board, Seccode = seccode, Tradeno = "1"});
            var cmd = ConnectorCommands.SubscribeTicks(SecurityForTicks.ReturnSecuritiesXml(_subscriptions));
            TXmlConnector.ConnectorSendCommand(cmd);
            _subscriptions.Last().Tradeno = "0";
        }
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            var vm = ((VisiFireChart)sender).DataContext as VisiFireChartViewModel;
            _subscriptions.RemoveAll(item => item.Board == vm._board && item.Seccode == vm._seccode);
            TXmlConnector.ConnectorSendCommand(ConnectorCommands.SubscribeTicks(SecurityForTicks.ReturnSecuritiesXml(_subscriptions)));
            TXmlConnector.SendNewTicks -= vm.TicksToCandles;
        }
        private void TicksToCandles(string data)
        {
                var ticks = ((List<Tick>)
                    new XmlSerializer(typeof(List<Tick>), new XmlRootAttribute("ticks")).Deserialize(
                        new StringReader(data))).Where(item => item.Board == _board && item.Seccode == _seccode)
                    .ToList();

                if (_ticks == null)
                    _ticks = ticks;
                else
                    _ticks.AddRange(ticks);

                var interval = new TimeSpan(0, _timeframe, 0);
                var temp = (from t in _ticks
                    group t by DateTime.Parse(t.Tradetime).Ticks/interval.Ticks
                    into g
                    select new Candle
                    {
                        High = (from t2 in g select t2.Price).Max(),
                        Low = (from t2 in g select t2.Price).Min(),
                        Open = g.First().Price,
                        Close = g.Last().Price,
                        Volume = g.Count(),
                        Time = g.First().Tradetime
                    }).ToList();

                foreach (var candle in temp)
                {
                    if (candle.Open < candle.Close)
                        candle.StickColor = System.Windows.Media.Brushes.Green;
                    else
                        candle.StickColor = System.Windows.Media.Brushes.Red;
                }

                if (Candles.Count == _historyCount)
                {
                    var realtime = Candles.ToList();
                    realtime.AddRange(temp);
                    Candles = new ObservableCollection<Candle>(realtime);
                    return;
                }

                //todo : create value if u use some math or other operatin which dublicated
            var qqqqq = temp.Count + _historyCount;
                if (qqqqq == Candles.Count)
                    _dispatcher.Invoke(() => Candles[Candles.Count - 1] = temp.Last());
                else if (qqqqq == Candles.Count + 1)
                    _dispatcher.Invoke(() => Candles.Add(temp.Last()));
                else
                    Candles = new ObservableCollection<Candle>(temp);
        }

        private void Refresh()
        {
            Task.Run(() =>
            {
                var interval = new TimeSpan(0, _timeframe, 0);
                var temp = (from t in _ticks
                    group t by DateTime.Parse(t.Tradetime).Ticks/interval.Ticks
                    into g
                    select new Candle
                    {
                        High = (from t2 in g select t2.Price).Max(),
                        Low = (from t2 in g select t2.Price).Min(),
                        Open = g.First().Price,
                        Close = g.Last().Price,
                        Volume = g.Count(),
                        Time = g.First().Tradetime
                    }).ToList();

                foreach (var candle in temp)
                {
                    if (candle.Open < candle.Close)
                        candle.StickColor = System.Windows.Media.Brushes.Green;
                    else
                        candle.StickColor = System.Windows.Media.Brushes.Red;
                }

                Candles = new ObservableCollection<Candle>(temp);
            });
        }
        
        private void GetHistory()
        {
            int count = 0;
            switch (_periodId)
            {
                case 1:
                    count = 600;
                    break;
                case 2:
                    count = 120;
                    break;
                case 3:
                    count = 40;
                    break;
                case 4:
                    count = 10;
                    break;
            }
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"gethistorydata\"><security><board>{_board}</board><seccode>{_seccode}</seccode></security><period>{_periodId}</period><count>{count + _daysBack * count}</count><reset>true</reset></command>");

            TXmlConnector.SendNewCandles += ProcessCandles;
        }

        private int _daysBack;
        private void ProcessCandles(string data)
        {
                var list =
                    (List<Candle>)
                        new XmlSerializer(typeof(List<Candle>), new XmlRootAttribute("candles")).Deserialize(
                            new StringReader(data));

                var dates =
                    list.GroupBy(item => DateTime.Parse(item.Time).Date)
                        .Select(g => DateTime.Parse(g.First().Time).Date)
                        .ToList();
                list =
                    list.Where(
                        item =>
                            DateTime.Parse(item.Time).Date < dates.Last() &&
                            DateTime.Parse(item.Time).Date >= dates[dates.Count - 1 - _daysBack]).ToList();

                foreach (var candle in list)
                {
                    if (candle.Open < candle.Close)
                        candle.StickColor = System.Windows.Media.Brushes.Green;
                    else
                        candle.StickColor = System.Windows.Media.Brushes.Red;
                }
                Candles = new ObservableCollection<Candle>(list);
                _historyCount = list.Count;
                TXmlConnector.SendNewCandles -= ProcessCandles;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
