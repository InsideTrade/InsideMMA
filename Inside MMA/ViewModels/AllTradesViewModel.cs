using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using SciChart.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Models.Filters;
using Inside_MMA.Views;
using MahApps.Metro;

namespace Inside_MMA.ViewModels
{
    public class IsEatenSizeAvailable : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // check minEatenSize and EatenSize
            if (values[2] == DependencyProperty.UnsetValue || values[2] == null) values[2] = 1;
            if (values[3] == DependencyProperty.UnsetValue || values[3] == null) values[3] = 1;

            // background color (based on comparison with minEatenSize and EatenSize)
            if (parameter == null)
            {
                // if not eaten - default
                if (!bool.Parse(values[0].ToString()))
                    return null;

                // if size == eatenSize - yellow
                return int.Parse(values[1].ToString()) == int.Parse(values[3].ToString())
                    ? new SolidColorBrush(Color.FromRgb(255, 180, 0))
                // if size >= minEatenSize - purple
                    : int.Parse(values[1].ToString()) >= int.Parse(values[2].ToString())
                        ? ThemeManager.GetAccent("Purple").Resources["AccentColorBrush"]
                // default
                        : null;
            }

            // foreground color
            if (parameter.ToString() == "foreground")
            {               
                //// buysell is undefined
                if (values[3] == null)
                    return new SolidColorBrush(Colors.White);

                // if highlighted (size >= minEatenSize or size == eatenSize) - set to white                
                // else set to green / red                
                if ((int.Parse(values[1].ToString()) >= int.Parse(values[2].ToString()) || 
                    int.Parse(values[1].ToString()) == int.Parse(values[3].ToString()))
                    && bool.Parse(values[0].ToString())) return new SolidColorBrush(Colors.White);
                    return values[4].ToString() == "B" 
                        ? new SolidColorBrush(Colors.Green)
                        : new SolidColorBrush(Color.FromRgb(255, 82, 82));              
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeltaOIIndicator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool res;
            if (values[1] == null)
                res = false;
            else
                try
                {
                    res = Math.Abs(int.Parse(values[0].ToString().Split(',').Last())) >= int.Parse(values[1].ToString());
                }
                catch
                {
                    res = false;
                }
            if (parameter == null)
                return res
                    ? ThemeManager.GetAccent("Purple").Resources["AccentColorBrush"]
                    : null;
            return res
                ? new SolidColorBrush(Colors.White)
                : values[2].ToString() == "B"
                    ? new SolidColorBrush(Colors.Green)
                    : new SolidColorBrush(Color.FromRgb(255, 82, 82));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowOpenInterest : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() != "FUT" ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(SolidColorBrush), typeof(string))]
    public class CheckBoxColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string val)) throw new InvalidOperationException();
            var doubleValue = double.Parse(val, CultureInfo.InvariantCulture);
            if (Math.Abs(doubleValue) <= 0)
            {
                return new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AllTradesViewModel : RememberPlacement, IAnchor
    {
        private readonly Timer _timer;

        public List<IAnchor> AnchoredWindows
            => Application.Current.Dispatcher
                .Invoke(() => (MainWindowViewModel) Application.Current.MainWindow.DataContext).AnchoredWindows;

        //Collection bound to the DataGrid
        private ObservableCollection<TradeItem> _allTradesCollection;

        public ObservableCollection<TradeItem> AllTradesCollection
        {
            get
            {
                return _allTradesCollection;
            }

            set
            {
                _allTradesCollection = value;
                OnPropertyChanged();
            }
        }

        private CollectionViewSource _items;        

        private ICollectionView _itemsView;

        public ICollectionView Items
        {
            get => _itemsView;
            set
            {
                _itemsView = value;
                OnPropertyChanged();
            }
        }

        private TradeItem _selectedTrade;

        public TradeItem SelectedTrade
        {
            get { return _selectedTrade; }
            set
            {
                if (Equals(value, _selectedTrade)) return;
                _selectedTrade = value;
                OnPropertyChanged();
            }
        }
        private string _amount;
        private string _amountMI;
        public string Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public string AmountMI
        {
            get => _amountMI;
            set
            {
                _amountMI = value;
                OnPropertyChanged();
            }
        }

        public AllTradesFilter Filter { get; set; }

        public ICommand Closing { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand RefreshCommand { get; set; }

        public ICommand ShowChart { get; set; }

        public ICommand ClearFiltersCommand { get; set; }

        public AllTradesViewModel(string board, string seccode, Window window, int id = 0)
        {
            Window = window;
            Closing = new Command(arg => WindowClosing());
            SaveCommand = new Command(arg => Save());
            RefreshCommand = new Command(arg => Refresh());
            ShowChart = new Command(arg => ShowTradesChart());
            ClearFiltersCommand = new Command(arg => Clear());
            Board = board;
            Seccode = seccode;
            if (Board == "MCT")
                Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            AllTradesCollection = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
            _items = new CollectionViewSource { Source = AllTradesCollection };
            Dispatcher.Invoke(() => Items = _items.View);
            Filter = new AllTradesFilter(_items, GetWindowArgs());
            _timer = new Timer(UpdateArgs, null, 5000, 5000);
        }
        
        private void UpdateArgs(object state)
        {
            try
            {
                Amount = AllTradesCollection.Count.ToString();
                AmountMI = AllTradesCollection.Count(t => t.IsMul).ToString();
            }
            catch (Exception)
            {
                //пока так, потом исправлю и удалю весь блок этот с try/catch;
            }
            finally
            {
                UpdateWindowArgs(Filter);
            }
        }

        private void ShowTradesChart()
        {
            //ProgressRingVisible = true;
            if (SelectedTrade == null) return;
            var data = AllTradesCollection.Where(x => x.Quantity == SelectedTrade.Quantity).ToList();
            new AllTradesCandlestick
            {
                DataContext = new AllTradesCandlestickViewModel(Board, Seccode, SelectedTrade.Quantity, data)
            }.Show();           
        }

        private void Refresh()
        {
            Dispatcher.Invoke(() => AllTradesCollection.Clear());            
            TickDataHandler.RefreshSub(Board, Seccode);
        }

        private void Save()
        {
            var xml = new XmlSerializer(typeof(ObservableCollection<TradeItem>));
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA";
            Directory.CreateDirectory(path);
            using (var file = File.Create(path + $"/{ Seccode}_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.xml"))
            {
                xml.Serialize(file, AllTradesCollection);
                file.Close();
            }
        }

        private void Clear()
        {
            Filter.IsHiddenSize = false;
            Filter.IsMiOnly = false;
            Filter.IsSelectingPrice = false;
            Filter.IsSelectingSize = false;
            Filter.IsSizeFilterActive = false;
            Filter.IsTimeFilterActive = false;
            Filter.ShowAll = true;
            Filter.ShowBuy = false;
            Filter.ShowSell = false;
        }
       
        ~AllTradesViewModel()
        {
            Debug.WriteLine("AllTradesViewModel disposed");
        }

        public Dispatcher Dispatcher = Application.Current.Dispatcher;

        //Closing event
        public void WindowClosing()
        {
            _timer.Dispose();
            AnchoredWindows.RemoveIfContains(this);                 
            UnsubscribeFromWindowEvents();
            CloseWindow();
        }

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
        public void SetSecurity(string board, string seccode)
        {
            if (board == Board && seccode == Seccode) return;
            Board = board;
            Seccode = seccode;            
            AllTradesCollection = TickDataHandler.AddAllTradesSubsribtion(Board, Seccode);            
            Dispatcher.Invoke(()=> { _items = new CollectionViewSource { Source = AllTradesCollection };  Items = _items.View; Filter.Items = _items; Filter.Update(); });            
            if (Board == "MCT")
                Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            UpdateWindowInstrument();
        }
    }
}