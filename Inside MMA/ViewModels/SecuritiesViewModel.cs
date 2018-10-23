using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.DataHandlers;

namespace Inside_MMA.ViewModels
{
    public class SecuritiesViewModel : INotifyPropertyChanged/*, IWindow*/
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;

        private Dispatcher Dispatcher => Application.Current.Dispatcher;
        private ObservableCollection<Security> Securities
            => Application.Current.Dispatcher
                .Invoke(() => MainWindowViewModel.SecVm._secList);

        //list of watchlists
        private ObservableCollection<MenuItem> _watchListsCollection = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> WatchListsCollection
        {
            get { return _watchListsCollection; }
            set
            {
                if (Equals(value, _watchListsCollection)) return;
                _watchListsCollection = value;
                OnPropertyChanged();
            }
        }

        public string Title { get; set; } = "Securities";
        public bool HideAddMenu { get; set; }
        public ObservableCollection<Security> _secList = new ObservableCollection<Security>();

        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set
            {
                if (Equals(value, _viewSource)) return;
                _viewSource = value;
                OnPropertyChanged();
            }
        }

        //private List<IAnchor> AnchoredWindows
          //  => ((MainWindowViewModel) Application.Current.MainWindow.DataContext).AnchoredWindows;

        private Security _selectedItem;
        public Security SelectedItem {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                if (_selectedItem == null)
                    IsSelected = false;
                else
                    IsSelected = true;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Security> SecList
        {
            get { return _secList; }
            set
            {
                _secList = value;
                OnPropertyChanged();
            }
        }

        private bool _isSelected;

        private bool _hideClipboardButton = true;
        public bool HideClipboardButton
        {
            get { return _hideClipboardButton; }
            set
            {
                if (value == _hideClipboardButton) return;
                _hideClipboardButton = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        public string ClipboardText => Clipboard.GetText();
        public ICommand ContextMenuCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand Closing { get; set; }
        public ICommand InsertFromClipboardCommand { get; set; }

        public ICommand Save { get; set; }
        public ICommand Load { get; set; }

        public ICommand AddToWatchListCommand { get; set; }

        public ICommand AddNewWatchList { get; set; }
        public FileSystemWatcher Watcher;

        public IDialogCoordinator Dialog;
        public string WatchListPath;
        public SecuritiesViewModel()
        {
            ContextMenuCommand = new Command(ContextMenuAction);
            AddToWatchListCommand = new Command(AddToWatchList);
            ViewSource.Source = SecList;
            //get watchlists and sub for watcher event
            GetWatchlists();
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
                if (list.Find(x => x.Seccode == SelectedItem.Seccode) == null)
                    list.Add(SelectedItem);
                fileStream.SetLength(0);
                xml.Serialize(fileStream, list);
                fileStream.Close();
            }
        }

        public void GetWatchlists()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\settings\\watchlists";
            Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path);
            if (files.Length == 0)
            {
                File.Create(path + "\\My Watchlist");
                files = new[] { path + "\\My Watchlist" };
            }
            WatchListsCollection = new ObservableCollection<MenuItem>();
            foreach (var file in files)
            {
                var name = file.Substring(file.LastIndexOf("\\")).TrimStart('\\');
                Application.Current.Dispatcher.Invoke(() => WatchListsCollection.Add(new MenuItem
                {
                    Header = name,
                    Command = AddToWatchListCommand,
                    CommandParameter = name
                }));
            }
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            if (WatchListPath != e.FullPath) return;
            var xml = new XmlSerializer(typeof(List<Security>));
            List<Security> list = new List<Security>();
            try
            {
                using (
                    var file =
                        File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    list = (List<Security>)xml.Deserialize(file);
                    file.Close();
                }
            }
            catch
            {
            }
            SecList = new ObservableCollection<Security>(list);
        }
        private void ContextMenuAction(object param)
        {
            if (SelectedItem == null)
                return;
            var str = param.ToString().Split('/');
            switch (str[0])
            {
                case "add":
                    AddToWatchlist();
                    break;
                case "Logbook":
                {
                    var view = new LogBook();
                    view.DataContext = new LogBookViewModel(SelectedItem.Board, SelectedItem.Seccode, view);
                    view.Show();
                    break;
                    }
                case "Level2":
                {
                    var view = new Level2();
                    view.DataContext = new Level2ViewModel(SelectedItem.Board, SelectedItem.Seccode, view);
                    view.Show();
                    break;
                }
                case "AllTrades":
                {
                    var view = new AllTradesSimple();
                    view.DataContext = new AllTradesSimpleViewModel(SelectedItem.Board, SelectedItem.Seccode, view);
                    view.Show();
                    break;
                }
                case "AllTradesPro":
                {
                    var view = new AllTrades();
                    view.DataContext = new AllTradesViewModel(SelectedItem.Board, SelectedItem.Seccode, view);
                    view.Show();
                    break;
                }
                case "TradesCounter":
                    {
                        new AllTradesCounterWindow { DataContext = new AllTradesCounterViewModel(SelectedItem.Board, SelectedItem.Seccode) }.Show();
                        break;
                    }
                case "Chart":
                {
                    var view = new SciChartWindow();
                    view.DataContext = new SciChartViewModel(SelectedItem.Board, SelectedItem.Seccode, view);
                    view.Show();
                    break;
                }
                case "NewOrder":
                {
                    new NewOrder
                    {
                        DataContext = new NewOrderViewModel
                        {
                            Board = SelectedItem.Board,
                            Seccode = SelectedItem.Seccode
                        }
                    }.Show();
                    break;
                }
                case "NewStoporder":
                    {
                        new NewStopOrder
                        {
                            DataContext = new NewStopOrderViewModel
                            {
                                Board = SelectedItem.Board,
                                Seccode = SelectedItem.Seccode
                            }
                        }.Show();
                        break;
                    }
                case "Cart":
                    Application.Current.Dispatcher.Invoke(
                        () => ((MainWindowViewModel) Application.Current.MainWindow.DataContext).CartViewModel.Orders
                            .Add(new CartItem
                            {
                                Board = SelectedItem.Board,
                                Seccode = SelectedItem.Seccode, 
                                Client = SelectClient(SelectedItem.Board)[0],
                                Union = SelectClient(SelectedItem.Board)[1],
                                Mkt = true
                            }));
                    break;
                case "Spread":
                {
                    new Spread
                    {
                        DataContext = new SpreadViewModel(SelectedItem.Board, SelectedItem.Seccode)
                    }.Show();
                    break;
                }
                case "FastOrder":
                {
                    var view = new FastOrder();
                    view.DataContext = new FastOrderViewModel(SelectedItem.Board, SelectedItem.Seccode, view);
                    view.Show();
                        break;
                }
            }
            
        }
        private string[] SelectClient(string board)
        {
            var clients = MainWindowViewModel.ClientsViewModel.Clients;
            Client client = null;
            if (board == "TQBR" ||
                board == "EQOB" ||
                board == "EQRP" ||
                board == "TQIF" ||
                board == "TQDE" ||
                board == "SPFEQ")
            {

                client =
                    clients.Find(
                        cl => cl.Market == "ММВБ");
            }
            if (board == "FUT" ||
                board == "OPT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "FORTS");
            }
            if (board == "MCT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "MMA");
            }
            if (board == "CETS")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "ETS");
            }
            if (client == null)
            {
                return new[] { "-", "-" };
            }
            return new[] { client.Id, client.Union};
        }
        private void AddToWatchlist(Security instrument = null)
        {
            var xml = new XmlSerializer(typeof(List<Security>));
            using (
                var fileStream =
                    new FileStream(WatchListPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                List<Security> list = new List<Security>();
                try
                {
                    list = (List<Security>)xml.Deserialize(fileStream);
                }
                catch
                {
                }
                if (instrument == null)
                {
                    if (list.Find(x => x.Seccode == SelectedItem.Seccode) == null)
                        list.Add(SelectedItem);
                }
                else
                {
                    if (list.Find(x => x.Seccode == instrument.Seccode) == null)
                        list.Add(instrument);
                }
                fileStream.SetLength(0);
                xml.Serialize(fileStream, list);
                fileStream.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
