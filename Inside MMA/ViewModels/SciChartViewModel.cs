using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Models.Alerts;
using Inside_MMA.Views;
using Microsoft.Build.Tasks;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting.Visuals.Axes;
using SciChart.Core.Extensions;
using SciChart.Data.Model;
using Color = System.Drawing.Color;

namespace Inside_MMA.ViewModels
{
    public class SciChartViewModel : RememberPlacement, IAnchor
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;

        private static Dispatcher _dispatcher = Application.Current.Dispatcher;

        //watchlists
        public ObservableCollection<MenuItem> WatchListsCollection => _dispatcher.Invoke(
            () => MainWindowViewModel.SecVm.WatchListsCollection);

        public ObservableCollection<MenuItem> ChartWatchlistCollection
        {
            get
            {
                var col = WatchListsCollection;
                col.ForEachDo(x => x.Command = AddToWatchlistCommand);
                return col;
            }
        }
        //List of subsriptions. Transaq requires resubbing for all ticks every time we add/remove one


        //Candlestick data
        private OhlcDataSeries<DateTime, double> _ohlcDataSeries = new OhlcDataSeries<DateTime, double>();
        public OhlcDataSeries<DateTime, double> OhlcDataSeries
        {
            get
            {
                return _ohlcDataSeries;
            }

            set
            {
                _ohlcDataSeries = value;
                OnPropertyChanged();
            }
        }

        //Volume data
        private XyDataSeries<DateTime, int> _xyDataSeries = new XyDataSeries<DateTime, int>();
        public XyDataSeries<DateTime, int> XyDataSeries
        {
            get
            {
                return _xyDataSeries;
            }

            set
            {
                _xyDataSeries = value;
                OnPropertyChanged();
            }
        }
        public bool HideVolumes
        {
            get { return _hideVolumes; }
            set
            {
                if (value == _hideVolumes) return;
                _hideVolumes = value;
                OnPropertyChanged();
            }
        }

        private XyDataSeries<double, double> _horizontalVolumesBuy;
        public XyDataSeries<double, double> HorizontalVolumesBuy
        {
            get { return _horizontalVolumesBuy; }
            set
            {
                if (Equals(value, _horizontalVolumesBuy)) return;
                _horizontalVolumesBuy = value;
                OnPropertyChanged();
            }
        }
        private XyDataSeries<double, double> _horizontalVolumesSell;
        public XyDataSeries<double, double> HorizontalVolumesSell
        {
            get { return _horizontalVolumesSell; }
            set
            {
                if (Equals(value, _horizontalVolumesSell)) return;
                _horizontalVolumesSell = value;
                OnPropertyChanged();
            }
        }

        //Bubble markers data
        private XyzDataSeries<DateTime, double, double> _bubbleSeries =
            new XyzDataSeries<DateTime, double, double>(){AcceptsUnsortedData = true};
        public XyzDataSeries<DateTime, double, double> BubbleSeries
        {
            get { return _bubbleSeries; }
            set
            {
                if (Equals(value, _bubbleSeries)) return;
                _bubbleSeries = value;
                OnPropertyChanged();
            }
        }

        private Candle _lastCandle;//Current last candle


        private int _timeframe = 5; //Timeframe in minutes
        private int _periodId = 2; //Period IDs (1 min = 1, 5 min = 2, 15 min = 3, 1 hour = 4)

        //For history
        private int _daysBack;
        public int DaysBack
        {
            get { return _daysBack; }
            set
            {
                if (value == _daysBack) return;
                _daysBack = value;
                OnPropertyChanged();
            }
        }

        private int _maxPeriod;
        public int MaxPeriod
        {
            get { return _maxPeriod; }
            set
            {
                if (value == _maxPeriod) return;
                _maxPeriod = value;
                OnPropertyChanged();
            }
        }

        //TImeframes bindings
        public List<string> TimeFrames { get; } = new List<string>()
        {
            "1 min",
            "5 mins",
            "15 mins",
            "Hour",
            "Day"
        };
        private string _selectedTimeframe = "5 mins";
        public string SelectedTimeframe
        {
            get
            {
                return _selectedTimeframe;
            }

            set
            {
                _selectedTimeframe = value;
                switch (_selectedTimeframe)
                {
                    case "1 min":
                        {
                            _timeframe = 1;
                            _periodId = 1;
                            GetHistory();
                            break;
                        }
                    case "5 mins":
                        {
                            _periodId = 2;
                            _timeframe = 5;
                            GetHistory();
                            break;
                        }
                    case "15 mins":
                        {
                            _periodId = 3;
                            _timeframe = 15;
                            GetHistory();
                            break;
                        }
                    case "Hour":
                        {
                            _periodId = 4;
                            _timeframe = 60;
                            GetHistory();
                            break;
                        }
                    case "Day":
                    {
                        _periodId = 5;
                        _timeframe = 1440;
                        GetHistory();
                        break;
                    }
                }
                if (_selectedTimeframe == "Day")
                {
                    HideVolumes = true;
                    MaxPeriod = 300;
                }
                else
                {
                    HideVolumes = false;
                    MaxPeriod = 31;
                }
                    
                OnPropertyChanged();
            }
        }

        //Properties for range syncing
        private IRange _sharedXRange;
        public IRange SharedXRange
        {
            get { return _sharedXRange; }
            set
            {
                _sharedXRange = value;
                OnPropertyChanged();
            }
        }

        private IRange _yRange;
        public IRange YRange
        {
            get { return _yRange; }
            set
            {
                _yRange = value;
                OnPropertyChanged();
            }
        }
        //Annotations (trendlines for now)
        public AnnotationCollection StockChartAnnotations
        {
            get
            {
                return _stockChartAnnotations;
            }

            set
            {
                _stockChartAnnotations = value;
                OnPropertyChanged();
            }
        }
        private AnnotationCollection _stockChartAnnotations = new AnnotationCollection();

        private double _width = 1000;
        public double Width
        {
            get { return _width; }
            set
            {
                if (value.Equals(_width)) return;
                _width = value;
                VolumesWidth = _width / 2;
                OnPropertyChanged();
            }
        }

        private double _volumesWidth;
        public double VolumesWidth
        {
            get { return _volumesWidth; } 
            set
            {
                if (value.Equals(_volumesWidth)) return;
                _volumesWidth = value;
                OnPropertyChanged();
            }
        }

        public double CurrentPrice;

        //trade size (bubbles)
        private int _tradeSize = 0;
        public int TradeSize
        {
            get { return _tradeSize; }
            set
            {
                if (value == _tradeSize) return;
                _tradeSize = value;
                BubbleSeries.Clear();
                GetBubbleData();
                OnPropertyChanged();
            }
        }

        //history lines visibility.

        private bool _toggleTrendlines;
        public bool ToggleTrendlines
        {
            get { return _toggleTrendlines; }
            set
            {
                if (value == _toggleTrendlines) return;
                _toggleTrendlines = value;
                OnPropertyChanged();

                UpdateWindowArgs(new ChartArgs(SelectedTimeframe, DaysBack, ToggleTrendlines));
                ToggleHistoryLines();
            }
        }

        //metadata for bubbles
        public class MyMetadata : IPointMetadata
        {
            public MyMetadata(string buysell)
            {
               Buysell = buysell;
            }
            public bool IsSelected { get; set; }
            public string Buysell { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //Commands
        public ICommand ChangePeriod { get; set; }
        public ICommand Closing { get; set; }
        public ICommand PlaceOrderCommand { get; set; }
        public ICommand PlaceStopOrderCommand { get; set; }
        public ICommand CopyCommand { get; set; }
        public ICommand AddToWatchlistCommand { get; set; }
        public ICommand PriceAlertCommand { get; set; }
        private Timer _timer;
        private double _stopPos;
        private double _orderPos;
        private double DragStep
        {
            get { return _dragStep; }
            set
            {
                _dragStep = value;
                StockChartAnnotations.Where(a => a is HorizontalLineAnnotation &&
                                                 ((HorizontalLineAnnotation) a).Name.Contains("stop"))
                    .ForEachDo(a => ((HorizontalLineAnnotation) a).YDragStep = _dragStep);
            }
        }

        public SciChartViewModel(string board, string seccode, Window window, int id = 0, int daysBack = 0, string selectedTimeFrame = "5 mins", bool toggleTrendlines = true)
        {
            Board = board;
            Seccode = seccode;
            Window = window;
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
            VolumesWidth = Width / 2;
            Closing = new Command(arg => WindowClosing());
            ChangePeriod = new Command(arg => GetHistory());
            PlaceOrderCommand = new Command(arg => PlaceOrder());
            PlaceStopOrderCommand = new Command(arg => PlaceStopOrder());
            CopyCommand = new Command(arg => Copy());
            AddToWatchlistCommand = new Command(AddToWatchList);
            PriceAlertCommand = new Command(arg => AddPriceAlert());
            _timer = new Timer(GetBubbleData, null, 1000, 2000);
            var volumes = TickDataHandler.AddChartSubscription(Board, Seccode, TicksToCandles);
            HorizontalVolumesBuy = volumes[0];
            HorizontalVolumesSell = volumes[1];
            DaysBack = daysBack;
            SelectedTimeframe = selectedTimeFrame;
            ToggleTrendlines = toggleTrendlines;
            //_stockChartAnnotations = new AnnotationCollection();
            //sub to stoporders manager
            OrderManager.NotifyStoporderAdded += NewStoporder;
            OrderManager.NotifyStoporderDeleted += DeleteStoporder;
            OrderManager.NotifyOrderAdded += NewOrder;
            OrderManager.NotifyOrderDeleted += DeleteOrder;
            //set dragstep
            DragStep = Application.Current.Dispatcher
                .Invoke(() => MainWindowViewModel.SecVm._secList
                .First(s => s.Board == Board && s.Seccode == Seccode).Minstep);
            GetOrders();
        }

        private void AddPriceAlert()
        {
            if (CurrentPrice >= _lastCandle.Close)
                MainWindowViewModel.AlertsViewModel.AlertsCollection.Add(
                    new GreaterThanPriceAlert(Board, Seccode)
                    {
                        Name = $"{Board} {Seccode} GP {CurrentPrice}",
                        Price = CurrentPrice,
                        Type = "Trade price >=",
                        Active = true
                    });
            else
            {
                MainWindowViewModel.AlertsViewModel.AlertsCollection.Add(
                    new SmallerThanPriceAlert(Board, Seccode)
                    {
                        Name = $"{Board} {Seccode} SP {CurrentPrice}",
                        Price = CurrentPrice,
                        Type = "Trade price <=",
                        Active = true
                    });
            }
        }

        private void GetOrders()
        {
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                var activeStoporders = OrderManager.GetActiveStoporders(Board, Seccode);
                foreach (var stop in activeStoporders)
                {
                    NewStoporder(Board, Seccode, stop.Transactionid,
                        double.Parse(stop.Stoploss[0].Activationprice, NumberStyles.AllowDecimalPoint,
                            NumberFormatInfo.InvariantInfo), stop.Buysell);
                }
                foreach (var order in OrderManager.GetActiveOrders(Board, Seccode))
                {
                    NewOrder(Board, Seccode, order.Transactionid,
                        double.Parse(order.Price, NumberStyles.AllowDecimalPoint,
                            NumberFormatInfo.InvariantInfo), order.Buysell);
                }
            });
        }

        private void DeleteOrder(string board, string seccode, string id)
        {
            if (board != Board && seccode != Seccode) return;
            try
            {
                StockChartAnnotations?.RemoveWhere(a => a is HorizontalLineAnnotation &&
                                                        ((HorizontalLineAnnotation)a).Name.Contains("order") &&
                                                        ((HorizontalLineAnnotation)a).AnnotationLabels[1].Text == id);
            }
            catch { }
        }

        private void NewOrder(string board, string seccode, string id, double price, string buysell)
        {
            if (board != Board && seccode != Seccode) return;
            _dispatcher.Invoke(() => {
                var annotation = new HorizontalLineAnnotation
                {
                    Name = "order" + buysell,
                    Stroke = buysell == "B" ?
                        Brushes.Green : Brushes.OrangeRed,
                    StrokeThickness = 2,
                    Y1 = price,
                    IsEditable = true,
                    StrokeDashArray = new DoubleCollection { 1 , 1 },
                    Visibility = Visibility.Visible,
                    YDragStep = _dragStep
                };
                annotation.DragStarted += OrderDragged;
                annotation.DragEnded += DeleteOrderOnDragOverAsync;
                annotation.AnnotationLabels.Add(new AnnotationLabel
                {
                    LabelPlacement = LabelPlacement.Axis,
                    Foreground = Brushes.White,
                    Background = buysell == "B" ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.OrangeRed)
                });
                annotation.AnnotationLabels.Add(new AnnotationLabel
                {
                    LabelPlacement = LabelPlacement.TopRight,
                    Foreground = Brushes.White,
                    Text = id,
                    Margin = new Thickness(0, 0, 20, 0)
                });
                if (StockChartAnnotations == null)
                    StockChartAnnotations = new AnnotationCollection();
                StockChartAnnotations.Add(annotation);
            });
        }

        private async void DeleteOrderOnDragOverAsync(object sender, EventArgs e)
        {
            var annotation = (HorizontalLineAnnotation)sender;
            var price = annotation.Y1.ToDouble().RoundOff(2, MidpointRounding.ToEven);
            switch (annotation.Name)
            {
                case "orderB":
                    if (price >= _lastCandle.Close)
                    {
                        var popup = CreatePopup(annotation, "limit order");
                        await Task.Run(() => 
                        {
                            SpinWait.SpinUntil(() => popup.ExecuteStop != null);
                        });
                        if (popup.ExecuteStop == true) break;
                        annotation.Y1 = _orderPos;
                        return;
                    }
                    break;
                case "orderS":
                    if (price <= _lastCandle.Close)
                    {
                        var popup = CreatePopup(annotation, "limit order");
                        await Task.Run(() =>
                        {
                            SpinWait.SpinUntil(() => popup.ExecuteStop != null);
                        });
                        if (popup.ExecuteStop == true) break;
                        annotation.Y1 = _orderPos;
                        return;
                    }
                    break;
            }
            OrderManager.EditOrder(annotation.AnnotationLabels[1].Text, price);
        }

        private void OrderDragged(object sender, EventArgs e)
        {
            _orderPos = ((HorizontalLineAnnotation)sender).Y1.ToDouble().RoundOff(2, MidpointRounding.ToEven);
        }

        private void DeleteStoporder(string board, string seccode, string id)
        {
            if (board != Board && seccode != Seccode) return;
            try
            {
                StockChartAnnotations?.RemoveWhere(a => a is HorizontalLineAnnotation &&
                                                       ((HorizontalLineAnnotation) a).Name.Contains("stop") &&
                                                       ((HorizontalLineAnnotation) a).AnnotationLabels[1].Text == id);
            }
            catch { }
        }

        private void NewStoporder(string board, string seccode, string id, double price, string buySell)
        {
            if (board != Board && seccode != Seccode) return;
             _dispatcher.Invoke(() => {
                var annotation = new HorizontalLineAnnotation
                {
                    Name = "stop" + buySell,
                    Stroke = buySell == "B" ? 
                    new LinearGradientBrush(Colors.Green, Colors.White, new Point(0,0), new Point(0.025, 0)) {SpreadMethod = GradientSpreadMethod.Repeat} : 
                    new LinearGradientBrush(Colors.OrangeRed, Colors.White, new Point(0, 0), new Point(0.025, 0)) {SpreadMethod = GradientSpreadMethod.Repeat},
                    StrokeThickness = 2,
                    Y1 = price,
                    IsEditable = true,
                    StrokeDashArray = new DoubleCollection { 10, 5 },
                    Visibility = Visibility.Visible,
                    YDragStep = _dragStep
                };
                annotation.DragStarted += StopDragged;
                annotation.DragEnded += DeleteStopOnDragOverAsync;
                annotation.AnnotationLabels.Add(new AnnotationLabel
                {
                    LabelPlacement = LabelPlacement.Axis,
                    Foreground = Brushes.White,
                    Background = buySell == "B" ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.OrangeRed)
                });
                annotation.AnnotationLabels.Add(new AnnotationLabel
                {
                    LabelPlacement = LabelPlacement.TopRight,
                    Foreground = Brushes.White,
                    Text = id,
                    Margin = new Thickness(0, 0, 20, 0)
                });
                 if (StockChartAnnotations == null)
                     StockChartAnnotations = new AnnotationCollection();
                StockChartAnnotations.Add(annotation);
            });
        }

        private void StopDragged(object sender, EventArgs e)
        {
            _stopPos = ((HorizontalLineAnnotation)sender).Y1.ToDouble().RoundOff(2, MidpointRounding.ToEven);
        }

        private async void DeleteStopOnDragOverAsync(object sender, EventArgs eventArgs)
        {
            var annotation = (HorizontalLineAnnotation)sender;
            var price = annotation.Y1.ToDouble().RoundOff(2, MidpointRounding.ToEven);
            switch (annotation.Name)
            {
                case "stopB":
                    if (price <= _lastCandle.Close)
                    {
                        var popup = CreatePopup(annotation, "stoporder");
                        await Task.Run(() =>
                        {
                            SpinWait.SpinUntil(() => popup.ExecuteStop != null);
                        });
                        if (popup.ExecuteStop == true) break;
                        annotation.Y1 = _stopPos;
                        return;
                    }
                    break;
                case "stopS":
                    if (price >= _lastCandle.Close)
                    {
                        var popup = CreatePopup(annotation, "stoporder");
                        await Task.Run(() =>
                        {
                            SpinWait.SpinUntil(() => popup.ExecuteStop != null);
                        });
                        if (popup.ExecuteStop == true) break;
                        annotation.Y1 = _stopPos;
                        return;
                    }
                    break;
            }
            OrderManager.EditStoporder(annotation.AnnotationLabels[1].Text, price);
        }

        private ChartStopAlert CreatePopup(HorizontalLineAnnotation annotation, string text)
        {
            var location = annotation.PointToScreen(new Point(0, 0));
            var popup = new ChartStopAlert (Window, text)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                //Left = Window.Left + Window.ActualWidth / 2
            };
            popup.Show();
            popup.Top = location.Y;
            return popup;
        }
        private void ToggleHistoryLines()
        {
            if (ToggleTrendlines)
                StockChartAnnotations.Where(x => x.GetType() == typeof(HorizontalLineAnnotation) &&
                                       ((HorizontalLineAnnotation)x).Name == "history").ForEachDo(annotation => annotation.Show());
            else
                StockChartAnnotations.Where(x => x.GetType() == typeof(HorizontalLineAnnotation) &&
                                       ((HorizontalLineAnnotation)x).Name == "history").ForEachDo(annotation => annotation.Hide());
        }

        private void AddToWatchList(object name)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                       "\\Inside MMA\\settings\\watchlists\\" + name;
            var xml = new XmlSerializer(typeof(List<Security>));
            using (
                var fileStream =
                    new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var list = new List<Security>();
                try
                {
                    list = (List<Security>)xml.Deserialize(fileStream);
                }
                catch
                {
                }
                if (list.Find(x => x.Seccode == Seccode) == null)
                    list.Add(_dispatcher
                        .Invoke(() => MainWindowViewModel.SecVm.SecList
                        .First(s => s.Board == Board && s.Seccode == Seccode)));
                fileStream.SetLength(0);
                xml.Serialize(fileStream, list);
                fileStream.Close();
            }
        }

        private void Copy()
        {
            var view = new SciChartWindow();
            view.DataContext = new SciChartViewModel(Board, Seccode, view, 0, DaysBack, SelectedTimeframe);
            view.Show();
        }
        private void GetBubbleData(object state = null)
        {
            if (TradeSize == 0) return;
            var data = TickDataHandler.TickList.First(x => x.Seccode == Seccode).TradeItems.ToList();
            data = data.FindAll(t => t.Quantity > TradeSize);
            data.Sort((a, b) => a.Time.CompareTo(b.Time));
            if (BubbleSeries.Count != 0)
            {
                var time = BubbleSeries.XValues.Last();
                data = data.FindAll(x => DateTime.Parse(x.Time) > time);
            }
            foreach (var item in data)
            {
                BubbleSeries.Append(DateTime.Parse(item.Time), item.Price, item.Quantity/300, new MyMetadata(item.Buysell));
            }
        }

        private void PlaceStopOrder()
        {
            var context = new NewStopOrderViewModel
            {
                Board = Board,
                Seccode = Seccode,
                StopLossByMarket = true,
                StopLossActivationPrice = CurrentPrice.ToString("F2")
            };
            var window = new NewStopOrder
            {
                DataContext = context,
                StopLossExpander = { IsExpanded = true },
                //StopLossActivationPrice = { Text = CurrentPrice.ToString("F2") },
                BuySell = {SelectedIndex = CurrentPrice < _lastCandle.Close ? 1 : 0 }
            };
            window.Show();
        }

        private void PlaceOrder()
        {
            new NewOrder
            {
                DataContext = new NewOrderViewModel
                {
                    Board = Board,
                    Seccode = Seccode,
                    Bymarket = false,
                    Price = CurrentPrice.ToString("F2"),
                    BuySell = CurrentPrice < _lastCandle.Close ? "B" : "S"
                }
            }.Show();
        }

        ~SciChartViewModel()
        {
            Debug.WriteLine("SciChartViewModel disposed");
        }

        //History request
        private void GetHistory()
        {
            UpdateWindowArgs(new ChartArgs(SelectedTimeframe, DaysBack, ToggleTrendlines));
            int count = 0;
            //Max candles per day for each period
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
                case 5:
                    count = 1;
                    break;
            }
            
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"gethistorydata\"><security><board>{Board}</board><seccode>{Seccode}</seccode></security><period>{_periodId}</period><count>{count + _daysBack * count}</count><reset>true</reset></command>");

            TXmlConnector.SendNewCandles += ProcessCandles;
        }

        private XmlSerializer _candleSerializer = new XmlSerializer(typeof(Candles));
        //Get candles and add them to the data series
        private static readonly object Lock = new object();
        private void ProcessCandles(string data)
        {
            InitHistory(data);
        }
        private void InitHistory(string data)
        {
            List<Candle> list;
            Candles candles;
            using (var reader = new StringReader(data))
            {
                list = new List<Candle>();
                if (_candleSerializer == null)
                    _candleSerializer = new XmlSerializer(typeof(Candles));
                candles =
                    (Candles)
                    _candleSerializer.Deserialize(reader);
                reader.Close();
            }
            //check if seccode matches 
            if (candles.Seccode != Seccode)
                return;
            //check if timeframe matches
            if (candles.Candle.Count > 1 && candles.Period != _periodId.ToString())
                return;
            
            list = candles.Candle;
            list =
                list.Where(
                    item =>
                        item.TradeTime.Date >= DateTime.Today.AddDays(-_daysBack)).ToList();

            if (list.Count == 0) return;
            _dispatcher.Invoke(() => 
            {
                
                try
                {
                    StockChartAnnotations.RemoveWhere(
                        a => a is HorizontalLineAnnotation && ((HorizontalLineAnnotation) a).Name == "history");
                }
                catch
                {
                }
            }
            )
            ;
            OhlcDataSeries = new OhlcDataSeries<DateTime, double> {AcceptsUnsortedData = true};
            OhlcDataSeries.Append(GetDateTime(list), GetValues(list));

            XyDataSeries = new XyDataSeries<DateTime, int>();
            XyDataSeries.Append(GetDateTime(list), GetVolumes(list));
            //Add trendlines
            
            if (OhlcDataSeries.Count == 0) return;
            var dates =
                OhlcDataSeries.XValues.GroupBy(date => date.Date).Select(group => group.First().Date).ToList();

            _lastCandle = list.Last();
            if (_selectedTimeframe != "Day")
                for (var i = 0; i < dates.Count; i++)
                {
                    var date = dates[i];
                    if (i == dates.Count - 1) //for today (only open)
                    {
                        AddAnnotation("history",
                            list.Where(item => Equals(item.TradeTime.Date, date))
                                .Select(item => item.Open)
                                .First(), "Open");
                    }
                    else //for other days
                    {
                        AddAnnotation("history",
                            list.Where(item => Equals(item.TradeTime.Date, date))
                                .Select(item => item.Open)
                                .First(), "Open");
                        AddAnnotation("history",
                            list.Where(item => Equals(item.TradeTime.Date, date))
                                .Select(item => item.Close)
                                .Last(), "Close");
                        AddAnnotation("history",
                            list.Where(item => Equals(item.TradeTime.Date, date)).Select(item => item.High).Max(),
                            "High");
                        AddAnnotation("history",
                            list.Where(item => Equals(item.TradeTime.Date, date)).Select(item => item.Low).Min(),
                            "Low");
                    }
                    _dispatcher.Invoke(() =>
                    {
                        //Vertical date annotations
                        try
                        {
                            StockChartAnnotations.Add(new VerticalLineAnnotation
                            {
                                Name = "history",
                                Stroke = new SolidColorBrush(Colors.DarkGray),
                                X1 = OhlcDataSeries.FindIndex(OhlcDataSeries.XValues.First(x => x.Date == date)),
                                IsEditable = false,
                                Opacity = 0.5,
                                AnnotationLabels =
                                    new ObservableCollection<AnnotationLabel>
                                    {
                                        new AnnotationLabel
                                        {
                                            LabelPlacement = LabelPlacement.Axis,
                                            Foreground = Brushes.White,
                                            Text = date.ToShortDateString()
                                        }
                                    }
                            });
                        }
                        catch
                        {

                        }
                    });
                }
            try
            {
                SharedXRange = new IndexRange(0, OhlcDataSeries.Count + 3);
                YRange = new DoubleRange(OhlcDataSeries.LowValues.Min(), OhlcDataSeries.HighValues.Max());
            }
            catch { }
            TXmlConnector.SendNewCandles -= ProcessCandles;
        }
        //On closing
        public void WindowClosing()
        {
            AnchoredWindows.RemoveIfContains(this);
            OhlcDataSeries = null;
            SharedXRange = null;
            StockChartAnnotations = null;
            XyDataSeries = null;
            YRange = null;
            _candleSerializer = null;
            UnsubscribeFromWindowEvents();
            CloseWindow();
        }
        
        //Getting realtime ticks
        private void TicksToCandles(List<Tick> list)
        {
            if (!list.Exists(x => x.Board == Board && x.Seccode == Seccode)) return;
            if (_lastCandle == null)
                Task.Run(() =>
                {
                    SpinWait.SpinUntil(() => _lastCandle != null);
                    HandleTicks(list);
                });
            else
                HandleTicks(list);
        }

        private void HandleTicks(List<Tick> list)
        {
            List<Tick> ticks = new List<Tick>();
            try
            {
                ticks = list.FindAll(i => i.Board == Board && i.Seccode == Seccode &&
                                            DateTime.Parse(i.Tradetime) > _lastCandle.TradeTime);
            }
            catch
            {
                if (_lastCandle == null) return;
            }
                
            if (ticks.Count == 0) return;

            foreach (var tick in ticks)
            {
                var tickTime = DateTime.Parse(tick.Tradetime);
                try
                {
                    if ((tickTime - _lastCandle.TradeTime).TotalMinutes >= _timeframe)
                    {
                        if (OhlcDataSeries == null) OhlcDataSeries = new OhlcDataSeries<DateTime, double>();
                        if (XyDataSeries == null) XyDataSeries = new XyDataSeries<DateTime, int>();
                        OhlcDataSeries.Append(tickTime, tick.Price, tick.Price, tick.Price, tick.Price);
                        XyDataSeries.Append(tickTime, (int) tick.Price);
                        _lastCandle = new Candle //init last candle
                        {
                            Time = tick.Tradetime,
                            Open = tick.Price,
                            High = tick.Price,
                            Low = tick.Price,
                            Close = tick.Price
                        };
                    }
                    else //else we update the last one
                    {
                        //if (_lastCandle.Close == tick.Price)
                        //{
                        //    continue;
                        //}
                        _lastCandle.Close = tick.Price;
                        _lastCandle.High = _lastCandle.High < tick.Price ? tick.Price : _lastCandle.High;
                        _lastCandle.Low = _lastCandle.Low > tick.Price ? tick.Price : _lastCandle.Low;
                        if (Board == "FUT")
                            _lastCandle.Volume++;
                        else
                            _lastCandle.Volume += (int) tick.Price;
                        OhlcDataSeries?.Update(OhlcDataSeries.Count - 1, _lastCandle.Open, _lastCandle.High,
                            _lastCandle.Low,
                            _lastCandle.Close);
                        XyDataSeries?.Update(XyDataSeries.Count - 1, _lastCandle.Volume);
                    }
                }
                catch (Exception)
                {
                    if (_lastCandle == null) return;
                    throw;
                }
            }
        }
        //Adding annotations. All annotations added here have a name "history" so they are different from user
        //annotations and thus are unaffected by "Remove trendlines" button
        private void AddAnnotation(string name, double y1, string text)
        {
            _dispatcher.Invoke(() =>
            {
                StockChartAnnotations.Add(new HorizontalLineAnnotation
                {
                    Name = name,
                    Stroke = new SolidColorBrush(Colors.DeepSkyBlue),
                    StrokeThickness = 1,
                    Y1 = y1,
                    AnnotationLabels = new ObservableCollection<AnnotationLabel>
                    {
                        new AnnotationLabel
                        {
                            LabelPlacement = LabelPlacement.TopLeft,
                            Foreground = Brushes.White,
                            Text = $"{text} {y1}"
                        }
                    },
                    IsHidden = !ToggleTrendlines
                });
            });
        }

        //Use these to feed data to SciChart's data series
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

        private IEnumerable<int> GetVolumes(List<Candle> list)
        {
            return list.Select(item => item.Volume);
        }

        //IAnchor members
        public List<IAnchor> AnchoredWindows
            => Application.Current.Dispatcher
                .Invoke(() => (MainWindowViewModel) Application.Current.MainWindow.DataContext).AnchoredWindows;
        private bool _isAnchorEnabled;
        public bool IsAnchorEnabled
        {
            get { return _isAnchorEnabled; }

            set
            {
                if (value == _isAnchorEnabled) return;
                _isAnchorEnabled = value;
                if (_isAnchorEnabled)
                    AnchoredWindows.Add(this);
                else
                    AnchoredWindows.RemoveIfContains(this);
                OnPropertyChanged();
                UpdateWindowBinding(IsAnchorEnabled);
            }
        }
        private bool _hideVolumes;
        private double _dragStep;

        public void SetSecurity(string board, string seccode)
        {
            if (Seccode == seccode) return;
            TickDataHandler.UnsubscribeFromTicksEvent(TicksToCandles);
            TXmlConnector.SendNewCandles -= ProcessCandles;
            OhlcDataSeries.Clear();
            XyDataSeries.Clear();
            StockChartAnnotations = new AnnotationCollection();
            Board = board;
            Seccode = seccode;
            _lastCandle = null;
            Task.Run(() => GetHistory());
            BubbleSeries.Clear();
            GetBubbleData();
            var volumes = TickDataHandler.AddChartSubscription(Board, Seccode, TicksToCandles);
            HorizontalVolumesBuy = volumes[0];
            HorizontalVolumesSell = volumes[1];
            UpdateWindowInstrument();
            DragStep = Application.Current.Dispatcher
                .Invoke(() => MainWindowViewModel.SecVm._secList
                .First(s => s.Board == Board && s.Seccode == Seccode).Minstep);
            GetOrders();
        }
    }
}