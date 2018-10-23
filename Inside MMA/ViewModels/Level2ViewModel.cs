using System;
using System.Collections.Concurrent;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Properties;
using Inside_MMA.Views;
using MahApps.Metro.Controls.Dialogs;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public delegate void IsUsaChanged(bool isUsa);

    public delegate void InstrumentChanged();
    public class Leve2SizeComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!MainWindowViewModel.WindowAvailabilityManager.SettingsEnabled)
                return false;
            try
            {
                return (int) values[0] > (int) values[1];
            }
            catch
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

   
    public class GradientBrushConverter : IValueConverter
    {

        public GradientBrushConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush =
                new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    Opacity = 0.6,
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop {Color = Colors.DarkSlateGray},
                        new GradientStop {Color = Colors.Transparent}
                    }
                };
            var offset = ((double) value).RoundOff(2, MidpointRounding.AwayFromZero);
            brush.GradientStops[0].Offset = offset;
            brush.GradientStops[1].Offset = offset;
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility.");

            bool? bValue = (bool?)value;

            return bValue.HasValue && bValue.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class Level2ViewModel : RememberPlacement, IAnchor
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;

        public event InstrumentChanged IntsrumentChanged;
        public List<IAnchor> AnchoredWindows => Application.Current.Dispatcher
            .Invoke(() => (MainWindowViewModel) Application.Current.MainWindow.DataContext).AnchoredWindows;

        public ObservableCollection<Level2Item> Level2Items
        {
            get { return _level2Items; }
            set
            {
                if (Equals(value, _level2Items)) return;
                _level2Items = value;
                OnPropertyChanged();
            }
        }


        private bool _isUsaChecked;
        public bool IsUsaChecked
        {
            get { return _isUsaChecked; }
            set
            {
                _isUsaChecked = value;
                OnPropertyChanged();
            }
        }

        private ComboBoxItem _selectedItem;
        private bool _isAnchorEnabled;
        public bool IsUSA { get; set; }
        public ComboBoxItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
                if (_selectedItem?.Content?.ToString() == "USA")
                    IsUsaChecked = true;
                else
                    IsUsaChecked = false;
                
                OnPropertyChanged();
            }
        }
        public Level2Item SelectedData
        {
            get { return _selectedData; }
            set
            {
                if (Equals(value, _selectedData)) return;
                _selectedData = value;
                OnPropertyChanged();
            }
        }

        public int AlertSize
        {
            get { return _alertSize; }
            set
            {
                if (value == _alertSize) return;
                _alertSize = value;
                OnPropertyChanged();
                UpdateLevel2Args(Level2ArgsType.AlertSize, _alertSize);
            }
        }
        public int AlertTwoSize
        {
            get { return _alertTwoSize; }
            set
            {
                if (value == _alertTwoSize) return;
                _alertTwoSize = value;
                OnPropertyChanged();
                UpdateLevel2Args(Level2ArgsType.AlertTwoSize, _alertTwoSize);
            }
        }

        private bool _useCredit;
        public bool UseCredit
        {
            get { return _useCredit; }
            set
            {
                if (value == _useCredit) return;
                _useCredit = value;
                Level2Settings.Default.UseCredit = _useCredit;
                Level2Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        
        public Visibility UseCreditIcon => UseCredit ? Visibility.Visible : Visibility.Collapsed;

        public int TradeSize
        {
            get { return _tradeSize; }
            set
            {
                if (value == _tradeSize) return;
                _tradeSize = value;
                Level2Settings.Default.Size = _tradeSize;
                Level2Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        private string UseCreditString => UseCredit ? "<usecredit/>" : "";
        public ICommand Closing { get; set; }
        public ICommand PlaceOrderCommand { get; set; }
        public ICommand PlaceStoporderCommand { get; set; }

        public IDialogCoordinator Dialog;
        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        private ObservableCollection<Level2Item> _level2Items;
        private Level2Item _selectedData;
        private int _alertSize = 1000;
        private int _alertTwoSize = 1100;
        private int _tradeSize;

        public Level2ViewModel(string board, string seccode, Window window, int id = 0)
        {
            Window = window;
            Closing = new Command(arg => WindowClosing());
            PlaceOrderCommand = new Command(PlaceOrder);
            PlaceStoporderCommand = new Command(PlaceStopOrder);
            Board = board;
            Seccode = seccode;
            Level2Items = Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
            if (Board == "MCT") IsUSA = true;
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
            //sub for settings changes
            UseCredit = Level2Settings.Default.UseCredit;
            TradeSize = Level2Settings.Default.Size;
            Level2Settings.Default.PropertyChanged += SettingsChanged;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UseCredit")
                UseCredit = Level2Settings.Default.UseCredit;
            if (e.PropertyName == "Size")
                TradeSize = Level2Settings.Default.Size;
        }

        public async void LeftClickMktOrder()
        {
            if (SelectedData == null)
                return;
            var buysell = SelectedData.BuySell == "buy" ? "S" : "B";
            var clientInfo = ClientSelector.GetClient(Board);
            var clientId = clientInfo == null ? "-" : clientInfo[0];
            var size = Level2Settings.Default.Size;
            var res = await Dialog.ShowMessageAsync(this, "MKT order", $"{Board} {Seccode}\r\nPrice: MKT\r\nSize: {size}\r\nType: {buysell}\r\nClient: {clientId}",
                MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Negative) return;
            TXmlConnector.ConnectorSendCommand("<command id=\"neworder\"><security><board>" + Board +
                                                            "</board><seccode>" + Seccode +
                                                            "</seccode></security><client>" + clientInfo[0] + "</client><union>" + clientInfo[1] + "</union><quantity>" +
                                                            size + "</quantity><buysell>" + buysell +
                                                            "</buysell><bymarket/>" + UseCreditString + "</command>");
        }
        public async void CtrlLeftClickLimitOrder()
        {
            if (SelectedData == null)
                return;
            var buysell = SelectedData.BuySell == "buy" ? "B" : "S";
            var price = SelectedData.Price;
            var clientInfo = ClientSelector.GetClient(Board);
            var clientId = clientInfo == null ? "-" : clientInfo[0];
            var size = Level2Settings.Default.Size;
            var res = await Dialog.ShowMessageAsync(this, "Limit order", $"{Board} {Seccode}\r\nPrice: {price}\r\nSize: {size}\r\nType: {buysell}\r\nClient: {clientId}",
                MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Negative) return;
            TXmlConnector.ConnectorSendCommand(
                $"<command id=\"neworder\"><security><board>{Board}</board><seccode>{Seccode}</seccode></security><client>{clientInfo[0]}</client><union>{clientInfo[1]}</union><price>{price}</price><quantity>{size}</quantity><buysell>{buysell}</buysell>{UseCreditString}</command>");
        }
        public async void ShiftLeftClickStopOrder()
        {
            if (SelectedData == null)
                return;
            var price = SelectedData.Price;
            var buysell = SelectedData.BuySell == "buy" ? "S" : "B";
            var clientInfo = ClientSelector.GetClient(Board);
            var clientId = clientInfo == null ? "-" : clientInfo[0];
            var size = Level2Settings.Default.Size;
            var res = await Dialog.ShowMessageAsync(this, "Stoporder", $"{Board} {Seccode}\r\nActivation price: {price}\r\nSize: {size}\r\nType: {buysell}\r\nClient: {clientId}",
                MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Negative) return;
            var str = TXmlConnector.ConnectorSendCommand($"<command id=\"newstoporder\"><security><board>{Board}</board><seccode>{Seccode}</seccode></security><client>{clientInfo[0]}</client><union>{clientInfo[1]}</union><buysell>{buysell}</buysell><stoploss><activationprice>{price}</activationprice><bymarket/><quantity>{size}</quantity>{UseCreditString}</stoploss></command>");
        }

        public void SetFastOrderManualStopPrice()
        {
            if (SelectedData == null)
                return;
            foreach (FastOrderViewModel context in Application.Current.Windows.Cast<Window>().Select(w => w.DataContext).Where(x => x is FastOrderViewModel))
            {
                if (context.Board != Board || context.Seccode != Seccode) continue;
                if (SelectedData.BuySell == "buy")
                    context.SellPrice = SelectedData.Price;
                else
                    context.BuyPrice = SelectedData.Price;
            }
        }
        ~Level2ViewModel()
        {
            Debug.WriteLine("Level2ViewModel disposed");
        }
        private void PlaceStopOrder(object obj)
        {
            if (SelectedData == null) return;
            var context = new NewStopOrderViewModel
            {
                BuySell = SelectedData.BuySell == "buy" ? "S" : "B",
                Seccode = Seccode,
                Board = Board,
                StopLossByMarket = true,
                StopLossActivationPrice = SelectedData.Price.ToString()
            };
            var window = new NewStopOrder
            {
                DataContext = context,
                StopLossExpander = {IsExpanded = true},
                BuySell = {SelectedIndex = SelectedData.BuySell == "buy" ? 1 : 0 }
            };
            window.Show();
        }

        private void PlaceOrder(object obj)
        {
            if (SelectedData == null) return;
            new NewOrder
            {
                DataContext = new NewOrderViewModel
                {
                    Board = Board,
                    Seccode = Seccode,
                    BuySell = SelectedData.BuySell == "buy" ? "B" : "S",
                    Bymarket = false,
                    Price = SelectedData.Price.ToString()
                }
            }.Show();
        }


        public void WindowClosing()
        {
            AnchoredWindows.RemoveIfContains(this);
            UnsubscribeFromWindowEvents();
            CloseWindow();
            Level2Settings.Default.PropertyChanged -= SettingsChanged;
        }


        public event IsUsaChanged IsUsaChanged;
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
            Task.Run(() => {
                Board = board;
                Seccode = seccode;
                Level2Items = Level2DataHandler.AddLevel2Subscribtion(Board, Seccode);
                _dispatcher.Invoke(() =>
                {
                    IsUsaChanged?.Invoke(Board == "MCT");
                    OnIntsrumentChanged();
                });
                UpdateWindowInstrument();
            });
        }

        protected virtual void OnIntsrumentChanged()
        {
            IntsrumentChanged?.Invoke();
        }
    }
}
