using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Views;
using Microsoft.Win32;

namespace Inside_MMA.ViewModels
{
    public class AllTradesCounterFromFile : INotifyPropertyChanged
    {
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

        private string _seccode;
        private TradesCounterBarChart _barChart;

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
        
        public bool AnchorCollapsed { get; set; } = true;
        public bool LoadCollapsed { get; set; } = false;

        public ICommand LoadCommand { get; set; }
        public ICommand ClearData { get; set; }
        public ICommand BarChartCommand { get; set; }
        public ICommand Closing { get; set; }
        public AllTradesCounterFromFile()
        {
            LoadCommand = new Command(arg => Load());
            ClearData = new Command(arg => Clear());
            BarChartCommand = new Command(arg => BarChart());
            Closing = new Command(arg => ClosingCommand());
            _allTradesCounterCollection = CollectionViewSource.GetDefaultView(AllTradesCounters);
        }

        private void ClosingCommand()
        {
            _barChart?.Close();
        }

        private void BarChart()
        {
            if (_barChart == null)
            {
                _barChart = new TradesCounterBarChart(this) {DataContext = new TradesCounterBarChartViewModel(AllTradesCounters.ToList())};
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
        }
        private void Clear()
        {
            AllTradesCounters = new ObservableCollection<AllTradesCounterItem>();
        }

        private void Load()
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA",
                Multiselect = true
            };
            if (dialog.ShowDialog() != true) return;
            foreach (var fileName in dialog.FileNames)
            {
                var file = File.Open(fileName, FileMode.Open);
                var list = (List<TradeItem>)new XmlSerializer(typeof(List<TradeItem>)).Deserialize(file);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in list)
                    {
                        if (AllTradesCounters.Select(c => c.Quantity).Contains(item.Quantity))
                        {
                            var val = AllTradesCounters.First(t => t.Quantity == item.Quantity);
                            val.Count++;
                            if (item.Buysell == "B")
                                val.Buy++;
                            else
                                val.Sell++;
                            val.Delta = val.Buy - val.Sell;
                        }
                        else
                        {
                            var val = new AllTradesCounterItem(item.Quantity, 1, 0, 0, 0, 0);
                            if (item.Buysell == "B")
                                val.Buy++;
                            else
                                val.Sell++;
                            val.Delta = val.Buy - val.Sell;
                            AllTradesCounters.Add(val);
                        }
                    }
                });
                var temp = AllTradesCounters.Sum(x => x.Count);
                foreach(var item in AllTradesCounters)
                {
                    item.Percent = Math.Round((double)item.Count / temp, 4) * 100.00;
                }
                Seccode += fileName.Split('\\').Last().Replace(".xml", "") + " ";
                file.Close();
            }
        }
        
        private ICollectionView _allTradesCounterCollection;
        
        public ICollectionView AllTradesCounterCollection
        {
            get => _allTradesCounterCollection;
        }

        private bool _isFlatBalanceFiltering;
        
        public bool IsFlatBalanceFiltering
        {
            get => _isFlatBalanceFiltering;
            set
            {
                if (value == _isFlatBalanceFiltering) return;
                _isFlatBalanceFiltering = value;
                OnPropertyChanged();
                if (_isFlatBalanceFiltering)
                    _allTradesCounterCollection.Filter += FlatBalanceFilter;
                else
                    _allTradesCounterCollection.Filter -= FlatBalanceFilter;
            }
        }
        private bool FlatBalanceFilter(object item)
        {
            var src = item as AllTradesCounterItem;
            return src.Delta == 0 && src.Count > 1;
        }

        //var itemsToDelete = AllTradesCounters.Where(item => item.Delta == 0 || item.Count == 1).ToList();
        //if (!_isFlatBalanceFiltering)
        //{
        //    Application.Current.Dispatcher.Invoke(() => AllTradesCounters.Clear());
        //    AllTradesCounters = AllTradesCountersCopy;
        //}

        //else
        //{
        //    foreach (var itemToDelete in itemsToDelete)
        //    {
        //        Application.Current.Dispatcher.Invoke(() => AllTradesCounters.Remove(itemToDelete));
        //    }
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
