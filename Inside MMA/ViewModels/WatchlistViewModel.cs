using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Views;
using MahApps.Metro.Controls.Dialogs;
using SciChart.Core.Extensions;
using Timer = System.Timers.Timer;

namespace Inside_MMA.ViewModels
{
    public delegate void OnItemDeleted();
    public class WatchlistViewModel : RememberPlacement
    {
        public WindowAvailabilityManager WindowAvailabilityManager => MainWindowViewModel.WindowAvailabilityManager;
        public event OnItemDeleted OnItemDeleted;
        //watchlists list selected item
        private string _selectedWatchlist;
        public string SelectedWatchlist
        {
            get { return _selectedWatchlist; }
            set
            {
                if (value == _selectedWatchlist) return;
                _selectedWatchlist = value;
                OnPropertyChanged();
                if (_selectedWatchlist != null)
                {
                    InitializeWatchlist(_selectedWatchlist);
                    UpdateWindowArgs(_selectedWatchlist);
                }
            }
        }

        private ObservableCollection<Security> Securities
            => Application.Current.Dispatcher
                .Invoke(() => MainWindowViewModel.SecVm._secList);

        private ObservableCollection<ComboBoxItem> _watchlists = new ObservableCollection<ComboBoxItem>();
        public ObservableCollection<ComboBoxItem> Watchlists
        {
            get { return _watchlists; }
            set
            {
                if (Equals(value, _watchlists)) return;
                _watchlists = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MenuItem> _watchlistMenuItems;
        public ObservableCollection<MenuItem> WatchlistMenuItems
        {
            get { return _watchlistMenuItems; }
            set
            {
                if (Equals(value, _watchlistMenuItems)) return;
                _watchlistMenuItems = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Security> _secList = new ObservableCollection<Security>();

        public ObservableCollection<Security> SecList
        {
            get { return _secList; }
            set
            {
                _secList = value;
                OnPropertyChanged();
            }
        }

        private Timer _timer;
        private Security _selectedItem;

        public Security SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                if (_selectedItem == null)
                    IsSelected = false;
                else
                    IsSelected = true;
                OnPropertyChanged();

                _timer.Stop();
                _timer.Start();
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }

            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsMCT { get; set; }
        public string ClipboardText => Clipboard.GetText();

        public ICommand ContextMenuCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand Closing { get; set; }
        public ICommand InsertFromClipboardCommand { get; set; }
        public ICommand AddNewWatchList { get; set; }
        public ICommand DeleteWatchList { get; set; }
        public ICommand Rename { get; set; }
        public ICommand AddToAnotherWatchlist { get; set; }
        public FileSystemWatcher Watcher;

        public IDialogCoordinator Dialog;
        public string WatchListPath;
        private string _name;
        private Dispatcher Dispatcher => Application.Current.Dispatcher;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public WatchlistViewModel(string name, Window window, int id = 0)
        {
            Window = window;
            MainWindowViewModel.WatchlistVMs.Add(this);
            _timer = new Timer(300);
            _timer.Elapsed += SetSecurity;
            ContextMenuCommand = new Command(ContextMenuAction);
            DeleteCommand = new Command(arg => DeleteEntry());
            InsertFromClipboardCommand = new Command(InsertFromClipboard);
            AddNewWatchList = new Command(arg => AddNewList());
            DeleteWatchList = new Command(arg => DeleteList());
            Rename = new Command(arg => RenameWatchlist());
            AddToAnotherWatchlist = new Command(AddToAnotherList);
            Closing = new Command(arg => WindowClosing());
            Watcher =
                new FileSystemWatcher(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                        "\\Inside MMA\\settings\\watchlists")
                {
                    EnableRaisingEvents = true
                };
            Watcher.Changed += OnChanged;
            Watcher.Deleted += OnDeleted;
            Id = id;
            if (Id == 0)
                SaveWindow();
            SubscribeToWindowEvents();
            SelectedWatchlist = name;
        }

        private void WindowClosing()
        {
            MainWindowViewModel.WatchlistVMs.Remove(this);
            UnsubscribeFromWindowEvents();
            CloseWindow();
        }

        private void AddToAnotherList(object name)
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

        private async void RenameWatchlist()
        {
            var newName = await Dialog.ShowInputAsync(this, "Enter new name:", null);
            if (newName == null) return;
            File.Move(WatchListPath, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                     "\\Inside MMA\\settings\\watchlists\\" + newName);
            GetWatchlists();
            Name = newName;
            SelectedWatchlist = Name;
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
            for (var i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Substring(files[i].LastIndexOf("\\")).TrimStart('\\');
            }
            List<string> missingFiles = new List<string>();
            if (files.Length != Watchlists.Count)
                foreach (var item in Watchlists)
                {
                    if (!files.Contains(item.Content))
                        missingFiles.Add(item.Content.ToString());
                }
            Watchlists.RemoveWhere(x => missingFiles.Contains(x.Content));
            WatchlistMenuItems = new ObservableCollection<MenuItem>();
            foreach (var name in files)
            {
                if (!Watchlists.Select(x => x.Content).Contains(name))
                    Dispatcher.Invoke(() => Watchlists.Add(new ComboBoxItem
                    {
                        Content = name
                    }));
                WatchlistMenuItems.Add(new MenuItem
                {
                    Header = name,
                    Command = AddToAnotherWatchlist,
                    CommandParameter = name
                });
            }
           
        }
        private async void DeleteList()
        {
            var res = await Dialog.ShowMessageAsync(this, "Warning", "This watchlist will be deleted. Are you sure?",
                MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Affirmative)
            {
                File.Delete(WatchListPath);
                GetWatchlists();
                SelectedWatchlist = Watchlists.First().Content.ToString();
            }
                
        }

        private void InitializeWatchlist(string name)
        {
            SecList = new ObservableCollection<Security>();
            Name = name;
            var xml = new XmlSerializer(typeof(List<Security>));
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                       "\\Inside MMA\\settings\\watchlists\\" + name;
            WatchListPath = path;
            List<Security> list = new List<Security>();
            try
            {
                using (
                    var file =
                        File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    list = (List<Security>)xml.Deserialize(file);
                }
            }
            catch
            {
            }
            string error = string.Empty;
            foreach (var security in list)
            {
                if (Securities.FirstOrDefault(s => s.Seccode == security.Seccode) != null)
                    SecList.Add(security);
                else
                    error += security.Seccode + ",";
            }
            if (error != string.Empty)
                Task.Run(() =>
                {
                    SpinWait.SpinUntil(() => Dialog != null);
                    Dialog.ShowMessageAsync(this, "Warning",
                        "The following instruments were not found: " + error.TrimEnd(','));
                    var serializer = new XmlSerializer(typeof(List<Security>));
                    using (var file = File.Open(WatchListPath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        var data = (List<Security>)serializer.Deserialize(file);
                        foreach (var s in error.Split(','))
                        {
                            data.RemoveWhere(sec => sec.Seccode == s);
                        }
                        file.SetLength(0);
                        serializer.Serialize(file, data);
                        file.Close();
                    }
                });
            //Task.Run(() =>
            ////{
            ////    if (_firstBoot)
            ////        Thread.Sleep(3000);
            //    TickDataHandler.WatchlistSub(SecList.ToList());
            //});
            TickDataHandler.WatchlistSub(SecList.ToList());
        }

        private async void AddNewList()
        {
            var name = await Dialog.ShowInputAsync(this, "Enter watchlist name:", null);
            try
            {
                File.Create(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                            "\\Inside MMA\\settings\\watchlists\\" + name);
                GetWatchlists();
                SelectedWatchlist = name;
            }
            catch
            {
            }
        }
        private void InsertFromClipboard(object text)
        {
            var instruments = Application.Current.Dispatcher.Invoke(
                () => MainWindowViewModel.SecVm.SecList);
            var seccodes = text.ToString().Split(new[] { " ", ",", ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var seccode in seccodes)
            {
                Security instrument;
                if (IsMCT)
                    instrument = instruments?.FirstOrDefault(s => s.Seccode == seccode.ToUpper() && s.Board == "MCT");
                else
                    instrument = instruments?.FirstOrDefault(s => s.Seccode == seccode.ToUpper() && s.Board != "MCT");
                if (instrument != null)
                {
                    AddToWatchlist(instrument);
                }
            }
        }
        private void AddToWatchlist(Security instrument)
        {
            if (SecList.FirstOrDefault(x => x.Board == instrument.Board && x.Seccode == instrument.Seccode) != null) return;
            SecList.AddIfNotContains(instrument);
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
                
                if (list.Find(x => x.Board == instrument.Board && x.Seccode == instrument.Seccode) == null)
                    list.Add(instrument);
                
                fileStream.SetLength(0);
                xml.Serialize(fileStream, list);
                fileStream.Close();
            }
        }
        private void DeleteEntry()
        {
            var xml = new XmlSerializer(typeof(List<Security>));
            using (var file = File.Open(WatchListPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var list = (List<Security>)xml.Deserialize(file);
                //remove from file
                list.RemoveWhere(sec => sec.Seccode == SelectedItem.Seccode && sec.Board == SelectedItem.Board);
                //and from table in memory
                SecList.Remove(SelectedItem);
                file.SetLength(0);
                xml.Serialize(file, list);
            }
            OnItemDeleted?.Invoke();
        }
        private void ContextMenuAction(object param)
        {
            if (SelectedItem == null)
                return;
            var str = param.ToString().Split('/');
            switch (str[0])
            {
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
                        () => ((MainWindowViewModel)Application.Current.MainWindow.DataContext).CartViewModel.Orders
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
                case "Alert":
                {
                    if (MainWindowViewModel.CheckIfWindowIsOpened(typeof(Alerts)))
                        MainWindowViewModel.AlertsViewModel.AddAlert(SelectedItem.Board, SelectedItem.Seccode);
                    else
                    {
                        new Alerts(MainWindowViewModel.AlertsViewModel).Show();
                        MainWindowViewModel.AlertsViewModel.AddAlert(SelectedItem.Board, SelectedItem.Seccode);
                    }
                    break;
                }
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
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (list.Count != 0 && list.Count != SecList.Count)
                SecList = new ObservableCollection<Security>(list);
        }
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (WatchListPath != e.FullPath) return;
            Name = "";
            SelectedWatchlist = null;
        }
        private void SetSecurity(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _timer.Stop();
            if (!_isSelected) return;
            var list = Application.Current.Dispatcher
                .Invoke(() => (MainWindowViewModel)Application.Current.MainWindow.DataContext).AnchoredWindows;
            for (var i = 0; i < list.Count; i++)
            {
                if (_isSelected)
                    list[i].SetSecurity(_selectedItem.Board, _selectedItem.Seccode);
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
            return new[] { client.Id, client.Union };
        }
    }
}