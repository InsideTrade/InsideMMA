using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using Inside_MMA;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Models.Filters;
using Inside_MMA.ViewModels;
using Inside_MMA.Views;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public class DataGridSelectedItemsBlendBehavior : Behavior<DataGrid>
    {
        
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<AllTradesCounterItem>),
                typeof(DataGridSelectedItemsBlendBehavior),
                new FrameworkPropertyMetadata(null)
                {
                    BindsTwoWayByDefault = true
                });
        
        public ObservableCollection<AllTradesCounterItem> SelectedItems
        {
            get
            {
                return (ObservableCollection<AllTradesCounterItem>)GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject != null)
                this.AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && this.SelectedItems != null)
            {
                foreach (object obj in e.AddedItems)
                    this.SelectedItems.Add(obj as AllTradesCounterItem);
            }

            if (e.RemovedItems != null && e.RemovedItems.Count > 0 && this.SelectedItems != null)
            {
                foreach (object obj in e.RemovedItems)
                    this.SelectedItems.Remove(obj as AllTradesCounterItem);
            }
        }
    }

    public class AllTradesCounterViewModel : INotifyPropertyChanged, IAnchor
    {
        private bool _lmt;
        private bool _updating; 
        private double count = 0;
        private ObservableCollection<AllTradesCounterItem> _allTradesCounters = new ObservableCollection<AllTradesCounterItem>();
        public ObservableCollection<AllTradesCounterItem> AllTradesCounters
        {
            get { return _allTradesCounters; }
            set
            {
                _allTradesCounters = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AllTradesCounterItem> _selectedItems = new ObservableCollection<AllTradesCounterItem>();
        public ObservableCollection<AllTradesCounterItem> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView AllTradesCounterCollection { get; }


        public string MenuItemHeader => string.Join(", ", SelectedItems.Select(item => item.Quantity).OrderBy(i => i));

        private List<Tick> _ticks = new List<Tick>();
        private string _board;
        public string Board
        {
            get { return _board; }
            set
            {
                if (value == _board) return;
                _board = value;
                OnPropertyChanged();
            }
        }
        private string _seccode;
        public string Seccode
        {
            get { return _seccode; }
            set
            {
                if (value == _seccode) return;
                _seccode = value;
                OnPropertyChanged();
            }
        }
        private bool _isFlatBalanceFiltering;

        public AllTradesCounterItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _timeFilterFrom;
        public TimeSpan TimeFilterFrom
        {
            get { return _timeFilterFrom; }
            set
            {
                if (value.Equals(_timeFilterFrom)) return;
                _timeFilterFrom = value;
                Application.Current.Dispatcher.Invoke(() => AllTradesCounters.Clear());
                TicksToTrades(_ticks.FindAll(t => DateTime.Parse(t.Tradetime).TimeOfDay >= _timeFilterFrom &&
                                                  DateTime.Parse(t.Tradetime).TimeOfDay <= _timeFilterTo));
                OnPropertyChanged();
            }
        }

        private TimeSpan _timeFilterTo = new TimeSpan(23, 59, 59);
        public TimeSpan TimeFilterTo
        {
            get { return _timeFilterTo; }
            set
            {
                if (value.Equals(_timeFilterTo)) return;
                _timeFilterTo = value;
                Application.Current.Dispatcher.Invoke(() => AllTradesCounters.Clear());
                TicksToTrades(_ticks.FindAll(t => DateTime.Parse(t.Tradetime).TimeOfDay >= _timeFilterFrom &&
                                                  DateTime.Parse(t.Tradetime).TimeOfDay <= _timeFilterTo));
                OnPropertyChanged();
            }
        }

        public bool IsFiltering
        {
            get { return _isFiltering; }
            set
            {
                if (value == _isFiltering) return;
                _isFiltering = value;
                Application.Current.Dispatcher.Invoke(() => AllTradesCounters.Clear());
                if (!_isFiltering)
                {
                    TicksToTrades(_ticks);
                }
                else
                {
                    TicksToTrades(_ticks.FindAll(t => DateTime.Parse(t.Tradetime).TimeOfDay >= _timeFilterFrom &&
                                                      DateTime.Parse(t.Tradetime).TimeOfDay <= _timeFilterTo));
                }
                OnPropertyChanged();
            }
        }

        public bool IsFlatBalanceFiltering
        {
            get => _isFlatBalanceFiltering;
            set
            {
                if (value == _isFlatBalanceFiltering) return;
                _isFlatBalanceFiltering = value;
                if (_isFlatBalanceFiltering)
                {
                    AllTradesCounterCollection.Filter += FlatBalanceFilter;
                }
                else
                    AllTradesCounterCollection.Filter -= FlatBalanceFilter;
            }
        }
        private bool FlatBalanceFilter(object item)
        {
            var src = item as AllTradesCounterItem;
            return src.Delta == 0 && src.Count > 1;
        }
       
        private bool _instrumentChanged;
        public ICommand Closing
        {
            get { return _closing; }
            set
            {
                if (Equals(value, _closing)) return;
                _closing = value;                
                OnPropertyChanged();
            }
        }
        public ICommand BarChartCommand { get; set; }
        public ICommand ClearData
        {
            get { return _clearData; }
            set
            {
                if (Equals(value, _clearData)) return;
                _clearData = value;
            }
        }
        public ICommand ShowChart { get; set; }

        public bool AnchorCollapsed { get; set; } = false;
        public bool LoadCollapsed { get; set; } = true;
        public AllTradesCounterViewModel(string board, string seccode, bool lmt = false)
        {
            _lmt = lmt;
            Board = board;
            Seccode = seccode;
            Closing = new Command(arg => ClosingAction());
            ClearData = new Command(arg => Clear());
            BarChartCommand = new Command(arg => BarChart());
            ShowChart = new Command(arg => ShowCandlestick());
            TickDataHandler.AddTradesCounterSubscribtion(Board, Seccode, HandleTicks);
            SelectedItems.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(MenuItemHeader));
            AllTradesCounterCollection = CollectionViewSource.GetDefaultView(AllTradesCounters);
            SeccodeTitle = (lmt) ? $"MI {Seccode} LMT" : $"MI {Seccode}";
        }
        public string SeccodeTitle { get; set; }

        private void ShowCandlestick()
        {
            List<DataForCandlestick> dataForCandlesticks = new List<DataForCandlestick>();
            RangeObservableCollection<TradeItem> tradeItems = TickDataHandler.TickList.First(x => x.Board == Board && x.Seccode == Seccode).TradeItems;
            RangeObservableCollection<Tick> tradeItemsLimit = TickDataHandler.TickList.First(x => x.Board == Board && x.Seccode == Seccode).TradeItemsLimit;
            if (_lmt)
            {
                foreach (var item in SelectedItems)
                {
                    dataForCandlesticks.Add(new DataForCandlestick()
                    {
                        Quantity = item.Quantity,
                        DataTick = tradeItemsLimit.Where(t => t.Quantity == item.Quantity).ToList()
                    });
                }
            }
            else
            {
                foreach (var item in SelectedItems)
                {
                    dataForCandlesticks.Add(new DataForCandlestick()
                    {
                        Quantity = item.Quantity,
                        Data = tradeItems.Where(t => t.Quantity == item.Quantity).ToList()
                    });
                }
            }
            
            new AllTradesCandlestick
            {
                DataContext = new AllTradesCandlestickViewModel(Board, Seccode, dataForCandlesticks, _lmt)
            }.Show();
        }

        private static Dispatcher _dispatcher = Application.Current.Dispatcher;
        private void HandleTicks(List<Tick> data)
        {
            _ticks.AddRange(data);
            if (_isFiltering)
                TicksToTrades(data.FindAll(t => DateTime.Parse(t.Tradetime).TimeOfDay >= _timeFilterFrom &&
                                                DateTime.Parse(t.Tradetime).TimeOfDay <= _timeFilterTo));
            else TicksToTrades(data);
        }

        private void TicksToTrades(List<Tick> data)
        {
            Task.Run(() =>
            {
                SpinWait.SpinUntil(() => !_updating);
                _updating = true;
                FillCollection(data);
                _updating = false;
            });
        }

        private void FillCollection(List<Tick> data)
        {
            if (Board == "MCT")
                data.AsParallel().ForEachDo(trade => trade.Buysell = GetMctBuysell(trade.Price));
            List<TradeItem> list;
            if (_lmt)
            {
                list = data.Select(
                        item =>
                            new TradeItem
                            {
                                Seccode = item.Seccode,
                                Buysell = item.Buysell,
                                Quantity = item.Quantity
                            }).ToList();
            }
            else
            {
                list = data.GroupBy(item => new { item.Seccode, item.Tradetime, item.Buysell })
                    .Select(
                        g =>
                            new TradeItem
                            {
                                Seccode = g.Key.Seccode,
                                Buysell = g.Key.Buysell,
                                Quantity = g.Select(t => t.Quantity).Sum()
                            }).ToList();
            }
            //var tryGetBalance = list.Count < 10;           
            count += (double)list.Count / 100;
            foreach (var trade in list)
            {
                if (_instrumentChanged)
                {
                    _instrumentChanged = false;
                    if (trade.Seccode != Seccode) return;
                }
                //if entry of this size already exists we increment buy/sell
                if (AllTradesCounters.Select(c => c.Quantity).Contains(trade.Quantity))
                {
                    var val = SetValues(AllTradesCounters.First(t => t.Quantity == trade.Quantity), trade, _lmt);
                    val.Count++;                    
                    _barChartViewModel?.Update(val);
                }
                //else we create a new entry
                else
                {
                    var val = SetValues(new AllTradesCounterItem(trade.Quantity, 1, 0, 0, 0, 0), trade, _lmt);
                    _dispatcher.Invoke(() => AllTradesCounters.Add(val));
                    _barChartViewModel?.Update(val);
                }
            }
            foreach (var e in AllTradesCounters)
            {
                var val = AllTradesCounters.First(t => t.Quantity == e.Quantity);
                val.Percent = Math.Round(val.Count / count, 2);
            }
        }

        private AllTradesCounterItem SetValues(AllTradesCounterItem val, TradeItem trade, bool _lmt)
        {
            //if B or S - increment, if undefined (MCT) - do nothing
            if (_lmt)
            {
                if (trade.Buysell == "B")
                    val.Sell++;
                else if (trade.Buysell == "S")
                    val.Buy++;
            }
            else
            {
                if (trade.Buysell == "B")
                    val.Buy++;
                else if (trade.Buysell == "S")
                    val.Sell++;
            }
            val.Delta = val.Buy - val.Sell;
            val.Buysell = trade.Buysell;
            if (val.Buy == val.Sell)
                val.Balance = GetBalance(val.Quantity).Result;
            return val;
        }
        private string GetMctBuysell(double price)
        {
            var l2Row = Level2DataHandler.Level2List
                .FirstOrDefault(x => x.Board == Board && x.Seccode == Seccode)
                ?.Level2Data
                .FirstOrDefault(item => item.Price == price);
            if (l2Row != null)
                return l2Row.BuySell == "buy" ? "S" : "B";
            return null;
        }
        private Task<double> GetBalance(int size)
        {
            return Task.Run(() =>
            {
                List<TradeItem> data = null;
                while (data == null)
                {
                    try
                    {
                        data = TickDataHandler.TickList.First(x => x.Seccode == Seccode).TradeItems
                            .Where(t => t.Quantity == size).ToList();
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
                var buyBalance = data.Where(x => x.Buysell == "B").Sum(s => s.Quantity * s.Price);
                var sellBalance = data.Where(x => x.Buysell == "S").Sum(s => s.Quantity * s.Price);
                return (sellBalance - buyBalance).RoundOff(2, MidpointRounding.ToEven);
            });
        }

        public static object Lock = new object();

        ~AllTradesCounterViewModel()
        {
            Debug.WriteLine("CounterVM disposed");
        }
        private void Clear()
        {
            Application.Current.Dispatcher.Invoke(() => AllTradesCounters.Clear());
        }
       
        public void ClosingAction()
        {
            TickDataHandler.UnsubscribeFromTicksEvent(HandleTicks);
            AnchoredWindows.RemoveIfContains(this);
            AllTradesCounters = null;
            _barChart?.Close();
            _barChartViewModel = null;
            GC.Collect();
        }

        private TradesCounterBarChart _barChart;
        private TradesCounterBarChartViewModel _barChartViewModel;
        private void BarChart()
        {
            if (_barChart == null)
            {
                _barChart = new TradesCounterBarChart(this);
                _barChartViewModel = new TradesCounterBarChartViewModel(AllTradesCounters.ToList());
                _barChart.DataContext = _barChartViewModel;
                _barChart.Show();
            }
            else
            {
                _barChart.WindowState = WindowState.Normal;
                _barChart.Activate();
            }
        }
        public void CloseChart()
        {
            _barChart = null;
            _barChartViewModel = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isAnchorEnabled;
        private ICommand _closing;
        private ICommand _clearData;
        private bool _isFiltering;
        private AllTradesCounterItem _selectedItem;

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
            }
        }
        public List<IAnchor> AnchoredWindows
            => ((MainWindowViewModel)Application.Current.MainWindow.DataContext).AnchoredWindows;
        public void SetSecurity(string board, string seccode)
        {
            if (board == Board && seccode == Seccode) return;
            _instrumentChanged = true;            
            _dispatcher.Invoke(() =>
            {
                TickDataHandler.UnsubscribeFromTicksEvent(HandleTicks);
                AllTradesCounters.Clear();
                if (_barChart != null)
                {
                    _barChartViewModel.Clear();
                }
                _ticks.Clear();
                Board = board;
                Seccode = seccode;
                TickDataHandler.AddTradesCounterSubscribtion(Board, Seccode, HandleTicks);
            });
        }

    }
}
