using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.AspNet.SignalR.Client;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Properties;
using Inside_MMA.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using UnitedPortfolio = Inside_MMA.Views.UnitedPortfolio;

namespace Inside_MMA.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        //Change background of label
        private string _statusState;
        
        public string StatusState
        {
            get { return _statusState; }
            set
            {
                _statusState = value;
                OnPropertyChanged("StatusState");
            }
        }

        private string _statusStateColor;

        public string StatusStateColor
        {
            get { return _statusStateColor; }
            set
            {
                _statusStateColor = value;
                OnPropertyChanged("StatusStateColor");
            }            
        }

        public static WindowAvailabilityManager WindowAvailabilityManager { get; set; } = new WindowAvailabilityManager();
        public double LogLevel
        {
            get { return Settings.Default.LogLevel; }
            set
            {
                Settings.Default.LogLevel = value;
                Settings.Default.Save();
                TXmlConnector.SetLogLevel((int) value);
            }
        }

        public static bool IsAdmin;
        public static bool IsReconnecting;
        public static bool IsConnected; //reflects connection status
        public static SecuritiesViewModel SecVm = new SecuritiesViewModel(); //ViewModel for the "Securities" Window
        public static ClientTradesViewModel ClientTradesViewModel = new ClientTradesViewModel(); //ViewNodel for the "Client Trades" window
        public static ClientOrdersViewModel ClientOrdersViewModel = new ClientOrdersViewModel(); //ViewNodel for the "Client Orders" window
        public static ClientsViewModel ClientsViewModel = new ClientsViewModel(DialogCoordinator.Instance);
        public CartViewModel CartViewModel = new CartViewModel(); //cart
        public NewsViewModel NewsViewModel = new NewsViewModel(); //news
        public static AlertsViewModel AlertsViewModel = new AlertsViewModel(); //alerts
        public static BalanceReportViewModel BalanceReportViewModel = new BalanceReportViewModel();
        public List<Security> AllTradesSubs = new List<Security>();
        public SynchronizedCollection<Security> Level2Subs = new SynchronizedCollection<Security>();
        public List<SecurityForTicks> TicksSubs = new List<SecurityForTicks>();
        public static List<WatchlistViewModel> WatchlistVMs = new List<WatchlistViewModel>();

        public static AdminViewModel AdminViewModel;
        //to avoid errors on shutdown
        public static bool IsShuttingDown;

        public static bool IsDisconnecting;
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

        public FileSystemWatcher Watcher;
        //list of windows that need to be linked
        public List<IAnchor> AnchoredWindows = new List<IAnchor>();
        //Login credentials
        public string Login { get; set; }
        public string Password { get; set; }
        //Server settings
        public string Server { get; set; }
        public string Port { get; set; }

        //Connection status string
        ////private string _status;
        ////public string Status
        ////{
        ////    get { return _status; }
        ////    set
        ////    {
        ////        _status = value;
        ////        OnPropertyChanged();
        ////    }
        ////}

        //license exp date textblock
        private string _licenseExpDate;

        public string LicenseExpDate
        {
            get { return _licenseExpDate; }
            set
            {
                if (value == _licenseExpDate) return;
                _licenseExpDate = value;
                OnPropertyChanged();
            }
        }

        //progress ring 
        private bool _animate;
        public bool Animate
        {
            get { return _animate; }
            set
            {
                if (value == _animate) return;
                _animate = value;
                OnPropertyChanged();
            }
        }
        //Connect/Disconnect menu item text
        private string _menuItemConnectText = "Connect";
        public string MenuItemConnectText
        {
            get
            {
                return _menuItemConnectText;
            }

            set
            {
                _menuItemConnectText = value;
                OnPropertyChanged();
            }
        }

        private bool _menuItemConnectEnabled = true;
        public bool MenuItemConnectEnabled
        {
            get
            {
                return _menuItemConnectEnabled;
            }

            set
            {
                _menuItemConnectEnabled = value;
                OnPropertyChanged();
            }
        }
        private bool _infoEnabled;
        public bool InfoEnabled
        {
            get
            {
                return _infoEnabled;
            }

            set
            {
                _infoEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _tradeEnabled;
        public bool TradeEnabled
        {
            get
            {
                return _tradeEnabled;
            }

            set
            {
                _tradeEnabled = value;
                OnPropertyChanged();
            }
        }


        private bool _hideAdmin = true;
        public bool HideAdmin
        {
            get { return _hideAdmin; }
            set
            {
                if (value == _hideAdmin) return;
                _hideAdmin = value;
                OnPropertyChanged();
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value == _isActive) return;
                _isActive = value;
                OnPropertyChanged();
            }
        }

        //Menu items' Commands
        private ICommand _menuItemConnectCommand;
        public ICommand MenuItemConnectCommand
        {
            get { return _menuItemConnectCommand; }
            set
            {
                _menuItemConnectCommand = value;
                OnPropertyChanged();
            }
        }
        public ICommand SecurityChooseCommand { get; set; }
        public ICommand GraphCommand { get; set; }
        public ICommand SecuritiesCommand { get; set; }
        public ICommand ContextMenuCommand { get; set; }
        public ICommand OpenWindowCommand { get; set; }
        public ICommand SleepCommand { get; set; }
        public ICommand ChangePassCommand { get; set; }
        public ICommand SaveWorkspaceCommand { get; set; }
        public ICommand LoadWorkspaceCommand { get; set; }
        
        public HubConnection Connection;
        public static IHubProxy Hub;
        public IDialogCoordinator Dialog;
        private FlashWindowHelper _flashWindowHelper;
        private Window _thisWindow;

        public MainWindowViewModel(IDialogCoordinator dialog, IHubProxy hub, HubConnection connection, Window window)
        {
            Connection = connection;
            Connection.Reconnected += ConnectionOnReconnected;
            Connection.StateChanged += ConnectionOnStateChanged;
            Hub = hub;
            Dialog = dialog;
            AdminViewModel = new AdminViewModel(Hub, DialogCoordinator.Instance);
            _flashWindowHelper = new FlashWindowHelper(Application.Current);
            _thisWindow = window;
            MenuItemConnectCommand = new Command(arg => Show_LoginForm());
            SecurityChooseCommand = new Command(SecurityChoose);
            SecuritiesCommand = new Command(arg => GetSecurities());
            OpenWindowCommand = new Command(OpenWindow);
            ChangePassCommand = new Command(arg => ChangePass());
            SleepCommand = new Command(arg => SelfSleep());
            SaveWorkspaceCommand = new Command(arg => SaveWorkspace());
            LoadWorkspaceCommand = new Command(arg => LoadWorkspace());
            //Status = "Status: ";
            StatusState = "Offline";
            StatusStateColor = "Red";
            //get watchlists collection and initialize watcher to monitor changes in the watchlists' folder
            GetWatchlists();
            InitializeWatcher();
            SecVm.WatchListsCollection = WatchListsCollection;
            //Subscribing to events
            TXmlConnector.SendNewFormData += XmlConnector_OnSendNewFormData; //general data
            /*TXmlConnector.SendNewStatus += XmlConnector_OnSendNewStatus;*/ //status
            TXmlConnector.SendStatus += XmlConnector_OnSendNewStatus;
            TXmlConnector.SendNewSecurity += XmlConnector_OnSendNewSecurity; //securities
            TXmlConnector.ConnectorSetCallback();
            TXmlConnector.FormReady = true;
            
            //Initializing the connector dll
            var path = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\logs\\" + ClientInfo.InsideLogin).Replace("\\", "//");
            Directory.CreateDirectory(path);
            foreach (var file in new DirectoryInfo(path).GetFiles())
            {
                file.Delete();
            }
            //loading settings
            WindowDataHandler.GetWindowData();
            Settings.Default.Reload();
            Level2Settings.Default.Reload();
            //path = path.Substring(0, path.LastIndexOf('\\') + 1) + "\0";
            if (TXmlConnector.ConnectorInitialize(path, (short) Settings.Default.LogLevel))
                TXmlConnector.StatusDisconnected.Set();

            //setting the number decimal separator to a point
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture = customCulture;
            Thread.CurrentThread.CurrentCulture = customCulture;

            //subbing for singlar events
            Hub?.On("Sleep", Sleep);
            Hub?.On("Unsleep", Unsleep);
            Hub?.On("SelectWindows", WindowAvailabilityManager.SelectWindows);
            Hub?.On("Disconnect", DisconnectByServer);
            Hub?.On("DisplayMessage", DisplayMessage);
            Hub?.On("CancelOrders", ClientOrdersViewModel.CancelAllOrdersMethod);
            Hub?.On("CancelStoporders", ClientOrdersViewModel.CancelAllStopordersMethod);
            Hub?.On("CloseBalance", ClientsViewModel.CloseBalanceMethod);
            //requesting license exp date
            Hub?.On("License", GetLicense);
            Hub?.Invoke("GetLicense");
            //setting the stoporder manager
            new OrderManager(ClientOrdersViewModel.ClientStoporders, ClientOrdersViewModel.ClientOrders);
            //setting the report manager
            ReportManager.Set();
            ShowPatchnotes();
        }

        private void ConnectionOnReconnected()
        {
            Hub.Invoke("UserReconnected", ClientInfo.InsideLogin);
        }

        private void ConnectionOnStateChanged(StateChange stateChange)
        {
            if (stateChange.OldState == ConnectionState.Reconnecting && stateChange.NewState == ConnectionState.Disconnected)
                Dialog.ShowMessageAsync(this, "Error", "Connection to the Inside server has been lost\r\nTry restarting the program");
        }

        private void ShowPatchnotes()
        {
            if (!File.Exists("patchnotes.txt")) return;
            new Patchnotes(File.ReadAllText("patchnotes.txt", Encoding.UTF8)).Show();
            File.Delete("patchnotes.txt");
        }
        private void GetLicense(dynamic license)
        {
            LicenseExpDate = ((DateTime)license).ToShortDateString();
        }

        private void LoadWorkspace()
        {
            WindowDataHandler.WindowPlacements.Clear();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\settings\\workspace";
            Directory.CreateDirectory(path);
            var dialog = new OpenFileDialog { InitialDirectory = path };
            if (dialog.ShowDialog(_thisWindow) == true)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.GetType() != typeof(MainWindow) && window.GetType().Namespace.Split('.')[0] != "Microsoft")
                            {
                                window.Close();
                            }
                        }
                    });
                    WindowDataHandler.GetWindowData(dialog.FileName);
                    OpenSavedWindows();
                }
                catch 
                {
                }
            }
        }

        private void SaveWorkspace()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\settings\\workspace";
            Directory.CreateDirectory(path);
            var dialog = new SaveFileDialog {InitialDirectory = path};
            if (dialog.ShowDialog(_thisWindow) == true)
                File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(WindowDataHandler.WindowPlacements));
        }

        private void GetWatchlists()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\settings\\watchlists";
            Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path);
            if (files.Length == 0)
            {
                File.Create(path + "\\My Watchlist");
                files = new []{ path + "\\My Watchlist" };
            }
            WatchListsCollection = new ObservableCollection<MenuItem>();
            foreach (var file in files)
            {
                var name = file.Substring(file.LastIndexOf("\\")).TrimStart('\\');
                Dispatcher.Invoke(() => WatchListsCollection.Add(new MenuItem
                {
                    Header = name,
                    Command = OpenWindowCommand,
                    CommandParameter = "watchlist/" + name
                }));
            }
        }
        private void InitializeWatcher()
        {
            //initialising filesystemwatcher
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Inside MMA\\settings\\watchlists";
            Directory.CreateDirectory(path);
            Watcher = new FileSystemWatcher(path) { EnableRaisingEvents = true };
            Watcher.Created += WatchlistDirectoryChanged;
            Watcher.Deleted += WatchlistDirectoryChanged;
        }
        private void WatchlistDirectoryChanged(object sender, FileSystemEventArgs e)
        { 
            //if wathclist directory was changed get the list again
            GetWatchlists();
        }

        private async void DisconnectByServer(dynamic message)
        {
            Disconnect();
            _flashWindowHelper.FlashApplicationWindow();
            await Dialog.ShowMessageAsync(this, "Message",
                message == "" ? "You have been disconnected by a server admin" : message);
            Dispatcher.Invoke(() =>
            {
                Connection.Stop(new TimeSpan(0, 0, 1));
                Application.Current.Shutdown();
            });
            
        }
        private Dispatcher Dispatcher => Application.Current.Dispatcher;
        private void OpenSavedWindows()
        {
            
            Task.Run(() =>
            {
                //Dispatcher.Invoke(() => {
                //    if (OpenWindows.Default.Clients)
                //        OpenWindow("clients");
                //    if (OpenWindows.Default.Trades)
                //        OpenWindow("trades");
                //    if (OpenWindows.Default.Orders)
                //        OpenWindow("orders");
                //    if (OpenWindows.Default.Stoporders)
                //        OpenWindow("stoporders");
                //});
                try
                {
                    //open watchlist first
                    var watchlists = WindowDataHandler.WindowPlacements.Where(w => w.Value.WindowType == typeof(Watchlist).ToString()).ToList();
                    if (watchlists.Count != 0)
                    {
                        //if there are any watchlists open them and wait for sub to take place
                        foreach (var watchlist in watchlists)
                        {
                            Dispatcher.Invoke(

                                () =>
                                {
                                    var view = new Watchlist();
                                    view.DataContext = new WatchlistViewModel(watchlist.Value.Args, view, watchlist.Key);
                                    view.Show();
                                });
                        }
                        Thread.Sleep(1000);
                    }
                    //then open other windows
                    foreach (var window in WindowDataHandler.WindowPlacements.Where(w => w.Value.WindowType != typeof(Watchlist).ToString()))
                    {
                        //create view
                        dynamic view = null;
                        //if level2
                        if (window.Value.WindowType == typeof(Level2).ToString() && window.Value.Args != null)
                            Dispatcher.Invoke(() =>
                            {
                                view = Activator.CreateInstance(Type.GetType(window.Value.WindowType), window.Value.Args);
                            });
                        //if alltrades
                        else if (window.Value.WindowType == typeof(AllTrades).ToString() && window.Value.Args != null)
                        {
                            //if (window.Value.Args is AllTradesArgs)
                            Dispatcher.Invoke(() =>
                            {
                                view = new AllTrades();
                            });
                            //else
                            //    Dispatcher.Invoke(() =>
                            //    {
                            //        view = new AllTrades((AllTradesArgs)((JObject)window.Value.Args).ToObject(typeof(AllTradesArgs)));
                            //    });
                        }
                        //if other
                        else
                        {
                            //if special window
                            if (window.Value.Args is string && window.Value.Args  == "special_window")
                            {
                                Dispatcher.Invoke(() => OpenWindow(window.Value.ViewModelType));
                                continue;
                            }
                            Dispatcher.Invoke(() =>
                            {
                                view = Activator.CreateInstance(Type.GetType(window.Value.WindowType));
                            });
                        }
                        //create viewmodel
                        dynamic context = null;
                        //if chart
                        if (window.Value.ViewModelType == typeof(SciChartViewModel).ToString())
                        {
                            if (window.Value.Args != null /* && !(window.Value.Args is string)*/)
                            {
                                //ChartArgs args;
                                //if (window.Value.Args is ChartArgs)
                                ChartArgs args = window.Value.Args;
                                //else
                                //    args = (ChartArgs)((JObject)window.Value.Args).ToObject(typeof(ChartArgs));
                                context = new SciChartViewModel(
                                    window.Value.Board, window.Value.Seccode, view, window.Key, args.DaysBack,
                                    args.SelectedTimeframe, args.ToggleTrendlines);
                            }
                            else
                            {
//to avoid errors
                                context = Activator.CreateInstance(Type.GetType(window.Value.ViewModelType),
                                    window.Value.Board, window.Value.Seccode, view, window.Key, 0, "5 mins");
                            }
                        }
                        //if any other
                        else
                            Dispatcher.Invoke(() => {
                                context = Activator.CreateInstance(Type.GetType(window.Value.ViewModelType),
                                    window.Value.Board, window.Value.Seccode, view, window.Key);
                            });
                        //check if window is anchored
                        if (context is IAnchor)
                            context.IsAnchorEnabled = window.Value.IsAnchored;

                        Dispatcher.Invoke(() =>
                        {
                            view.DataContext = context;
                            view.Show();
                        });
                    }
                }
                catch (Exception e)
                {
                    Dialog.ShowMessageAsync(this, "Error", "There was an error opening your workspace");
                    WindowDataHandler.WindowPlacements.Clear();
                }
            });
        }
        //put yourself to sleep
        private async void SelfSleep()
        {
            var time = await Dialog.ShowInputAsync(this, "Set time", "Time:");
            if (time != null)
                await Hub.Invoke("SleepUser", ClientInfo.InsideLogin, time);
        }
        //opening windows
        public void OpenWindow(object param)
        {
            var parameter = param.ToString().Split('/');
            switch (parameter[0])
            {
                case "watchlist":
                    var view = new Watchlist();
                    view.DataContext = new WatchlistViewModel(parameter[1], view);
                    view.Show();
                    break;
                case "clients":
                    if (CheckIfWindowIsOpened(typeof(Clients))) break;
                    //_clientsViewModel.Timer.Start();
                    new Clients {DataContext = ClientsViewModel}.Show();
                    break;
                case "newOrder":
                    if (CheckIfWindowIsOpened(typeof(NewOrder))) break;
                    new NewOrder { DataContext = new NewOrderViewModel() }.Show();
                    break;
                case "newStoporder":
                    if (CheckIfWindowIsOpened(typeof(NewStopOrder))) break;
                    new NewStopOrder { DataContext = new NewStopOrderViewModel() }.Show();
                    break;
                case "trades":
                    if (CheckIfWindowIsOpened(typeof(ClientTrades))) break;
                    new ClientTrades { DataContext = ClientTradesViewModel }.Show();
                    break;
                case "orders":
                    if (CheckIfWindowIsOpened(typeof(ClientOrders))) break;
                    new ClientOrders { DataContext = ClientOrdersViewModel }.Show();
                    break;
                case "stoporders":
                    if (CheckIfWindowIsOpened(typeof(ClientStoporders))) break;
                    new ClientStoporders { DataContext = ClientOrdersViewModel }.Show();
                    break;
                case "allTradesCounterFile":
                    new AllTradesCounterWindow { DataContext = new AllTradesCounterFromFile()}.Show();
                    break;
                case "unitedPortfolio":
                    new UnitedPortfolio { DataContext = new UnitedPortfolioViewModel() }.Show();
                    break;
                case "admin":
                    if (CheckIfWindowIsOpened(typeof(AdminWindow))) break;
                    //new AdminWindow { DataContext = new AdminViewModel(Hub, DialogCoordinator.Instance) }.Show();
                    new AdminWindow { DataContext = AdminViewModel }.Show();
                    break;
                case "news":
                    if (CheckIfWindowIsOpened(typeof(Views.News))) break;
                    new Views.News { DataContext = NewsViewModel }.Show();
                    break;
                case "cart":
                    if (CheckIfWindowIsOpened(typeof(Cart))) break;
                    new Cart { DataContext = CartViewModel }.Show();
                    break;
                case "fastOrder":
                    var window = new FastOrder();
                    window.DataContext = new FastOrderViewModel(null, null, window);
                    window.Show();
                    break;
                case "alerts":
                    new Alerts(AlertsViewModel).Show();
                    break;
                case "balance":
                    new BalanceReport {DataContext = BalanceReportViewModel}.Show();
                    break;
                case "calendar":
                    new CalendarMainWindow().Show();
                    break;
            }
        }
        public static bool CheckIfWindowIsOpened(Type type)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == type)
                {
                    if (window.WindowState != WindowState.Minimized) return true;
                    window.WindowState = WindowState.Normal;
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        //Forming a list of available securities in an instance of the Securities ViewModel
        private void XmlConnector_OnSendNewSecurity(string data)
        {
            var list =
                (List<Security>)
                    new XmlSerializer(typeof(List<Security>), new XmlRootAttribute("securities")).Deserialize(
                        new StringReader(data));
            if (SecVm.SecList != null)
            {
                var temp = SecVm.SecList.ToList();
                temp.AddRange(list);
                SecVm.SecList = new ObservableCollection<Security>(temp);
            }
                
            else
                SecVm.SecList = new ObservableCollection<Security>(list.ToList());
        }
        //Open Securities window
        private void GetSecurities()
        {
            new Securities {DataContext = SecVm}.Show();
        }
        //Processing status changes
        private static XmlSerializer _statusXml = new XmlSerializer(typeof(ServerStatus));
        private void XmlConnector_OnSendNewStatus(string data)
        {
            var status = (ServerStatus) _statusXml.Deserialize(new StringReader(data));
            if (status.Recover == "true")
            {
               // Status = "Status: ";
                StatusState = "Reconnecting...";
                StatusStateColor = "GreenYellow";
                IsReconnecting = true;
                Animate = false;
                return;
            }
            if (status.Connected != "true")
            {
                if (IsShuttingDown)
                {
                    //TXmlConnector.ConnectorUnInitialize();
                    //Application.Current.Shutdown();
                    return;
                }
                MenuItemConnectCommand = new Command(arg => Show_LoginForm()); //set command to open LoginForm
                MenuItemConnectText = "Connect"; //change text
                //Status = "Status: "; //change status string
                StatusState = "Offline";
                StatusStateColor = "Red";
                
                //_dialogCoordinator.ShowMessageAsync(this, "Connection status", "Connection lost");
                //disable menu buttons
                InfoEnabled = false;
                TradeEnabled = false;
                Animate = false;
                SecVm = new SecuritiesViewModel();
                ClientTradesViewModel = new ClientTradesViewModel(); 
                ClientOrdersViewModel = new ClientOrdersViewModel();
                //close windows
                Application.Current.Dispatcher.Invoke(() => {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.GetType() != typeof(MainWindow) && window.GetType().Namespace.Split('.')[0] != "Microsoft")
                        {
                            window.Close();
                        }
                    }
                });
                //clear clients
                ClientsViewModel.Clients.Clear();
                //clear subs
                Level2DataHandler.UnsubAll();
                TickDataHandler.UnsubAll();
            }
            else
            {
                MenuItemConnectCommand = new Command(arg => Disconnect()); //set command to disconnect
               // Status = "Status: "; //change text
                StatusState = "Online";
                StatusStateColor = "Green";
                MenuItemConnectText = "Disconnect"; //change status string
                //enable menu buttons
                InfoEnabled = true;
                TradeEnabled = true;
                Animate = true;
                //Application.Current.Dispatcher.Invoke(() => {
                //    if (!CheckIfWindowIsOpened(typeof(Clients)))
                //        {
                //            _clientsViewModel.Timer.Start();
                //            new Clients { DataContext = _clientsViewModel }.Show();
                //        }
                //    });
                //if reconnecting, do not restore windows
                if (!IsReconnecting)
                    OpenSavedWindows();
                ClientsViewModel.GetInfo();
                IsReconnecting = false; //reconnected at this point
            }

            IsConnected = status.Connected == "true";
            MenuItemConnectEnabled = true; //enable it after status change
            IsActive = false;
        }
        //displaying error messages from the server
        private void XmlConnector_OnSendNewFormData(string data)
        {
            _flashWindowHelper.FlashApplicationWindow();
            Dialog.ShowMessageAsync(this, "Error", data);
        }
        //open Login window
        private void Show_LoginForm()
        {
            var form = new LoginForm();
            form.LoginFormViewModel.LoginHandlerEvent += LoginFormViewModel_LoginHandlerEvent; //sub and wait for login data
            form.ShowDialog();
            form.LoginFormViewModel.LoginHandlerEvent -= LoginFormViewModel_LoginHandlerEvent; //unsub when window is closed
        }
        //connect
        private void LoginFormViewModel_LoginHandlerEvent(string login, string password, string server, string port)
        {
            Connect(login, password, server, port);
        }
        //open Security Choose window
        private void SecurityChoose(object param)
        {
            //pass Window name
            var form = new SecurityChoose
            {
                SecurityChooseViewModel = {Window = param.ToString()},
                Owner = _thisWindow
            };
            form.ShowDialog();
            if (form.SecurityChooseViewModel.ResultOk)
            {
                var vm = form.SecurityChooseViewModel;
                ShowWindow(vm.Board, vm.Seccode, vm.Window);
            }
        }
        //open a window which requires security data
        private void ShowWindow(string board, string seccode, string window)
        {
            switch (window)
            {
                case "level2":
                {
                    var view = new Level2();
                    view.DataContext = new Level2ViewModel(board, seccode, view);
                    view.Show();
                    break;
                }
                case "logbook":
                {
                    var view = new LogBook();
                    view.DataContext = new LogBookViewModel(board, seccode, view);
                    view.Show();
                        break;
                }
                case "allTrades":
                {
                    var view = new AllTrades();
                    view.DataContext = new AllTradesViewModel(board, seccode, view);
                    view.Show();
                    break;
                }
                case "allTradesSimple":
                {
                    var view = new AllTradesSimple();
                    view.DataContext = new AllTradesSimpleViewModel(board, seccode, view);
                    view.Show();
                    break;
                }
                case "allTradesCounter":
                {
                    new AllTradesCounterWindow { DataContext = new AllTradesCounterViewModel(board, seccode) }.Show();
                        break;
                }
                case "counterLMT":
                {
                    new AllTradesCounterWindow { DataContext = new AllTradesCounterViewModel(board, seccode, true) }.Show();
                    break;
                }
                case "scichart":
                {
                    var view = new SciChartWindow();
                    view.DataContext = new SciChartViewModel(board, seccode, view);
                    view.Show();
                    break;
                }
                case "spread":
                {
                    new Spread { DataContext = new SpreadViewModel(board, seccode)}.Show();
                    break;
                }
            }
        }
        //Closing event. Uninitialize dll, disconnect, close all windows.
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            //if (Status == "Status: Disconnected")
            //{
            //    
            //    return;
            //}
            //e.Cancel = true;if (_isConnected)
            IsShuttingDown = true;
            Connection.Stop(new TimeSpan(0, 0, 0, 100));
            SecVm = null;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() != typeof(MainWindow) && window.GetType().Namespace.Split('.')[0] != "Microsoft")
                {
                    //var context = window.DataContext as IWindow;
                    //if (context != null)
                    //    context.IsManuallyClosed = false
                    if (window is IRememberPlacement)
                        ((IRememberPlacement)window).IsManuallyClosed = false;
                    window.Close();
                }
            }
            TXmlConnector.ConnectorSendCommand("<command id=\"disconnect\"/>");
            Application.Current.Shutdown();
            //TXmlConnector.ConnectorUnInitialize();
        }
        //Connect to server
        private void Connect(string login, string password, string server, string port)
        {
            IsDisconnecting = false;
            //create xml command
            var cmd = new CommandConnect
            {
                Id = "connect",
                Login = login,
                Password = password,
                Host = server,
                Port = port,
                Language = "en",
                Milliseconds = true,
                Rqdelay = 100,
                SessionTimeout = 200,
                RequestTimeout = 20
            };
            string command;
            using (var stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true
                }))
                {
                    new XmlSerializer(typeof(Models.CommandConnect)).Serialize(writer, cmd,
                        new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty}));
                    command = stream.ToString();
                }
            }
            TXmlConnector.ConnectorSendCommand(command);
            //Status = "Status: "; //set status string
            StatusState = "Connecting";
            StatusStateColor = "GreenYellow";
            MenuItemConnectEnabled = false; //disable connect/disconnect button until status is changed
            IsActive = true;
        }
        //disconnect
        private void Disconnect()
        {
            IsDisconnecting = true;
            TXmlConnector.ConnectorSendCommand("<command id=\"disconnect\"/>");
        }

        //sleep
        public void Sleep()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(NewOrder) || window.GetType() == typeof(NewStopOrder) ||
                        window.GetType() == typeof(FastOrder))
                        ((Window) window).Close();
                }
            });
            WindowAvailabilityManager.TradingEnabled = false;
            WindowAvailabilityManager.FastOrderEnabled = false;
            Dialog.ShowMessageAsync(this, "Sleep", "Trading has been disabled");
            _flashWindowHelper.FlashApplicationWindow();
        }
        private void Unsleep()
        {
            WindowAvailabilityManager.TradingEnabled = true;
            WindowAvailabilityManager.FastOrderEnabled = true;
            Dialog.ShowMessageAsync(this, "Sleep", "Trading is enabled. Good luck!");
            _flashWindowHelper.FlashApplicationWindow();
        }
        private void DisplayMessage(dynamic data)
        {
            Dialog.ShowMessageAsync(this, "Message", data);
            _flashWindowHelper.FlashApplicationWindow();
        }
        //pass change
        private void ChangePass()
        {
            var dialog = new ChangePassDialog {Owner = _thisWindow};
            dialog.ShowDialog();
        }

        //new thread creator
        //private void OpenWindowInNewThread(string window, string board, string seccode)
        //{
        //    Thread newWindowThread = new Thread(() =>
        //    {
        //        //Create our context, and install it:
        //        SynchronizationContext.SetSynchronizationContext(
        //            new DispatcherSynchronizationContext(
        //                Dispatcher.CurrentDispatcher));

        //        var tempWindow = GetWindow(window, board, seccode);
        //        // When the window closes, shut down the dispatcher
        //        tempWindow.Closed += (s, e) =>
        //            Dispatcher.CurrentDispatcher.InvokeShutdown();
        //        tempWindow.Show();
        //        // Start the Dispatcher Processing
        //        Dispatcher.Run();
        //    });
        //    // Set the apartment state
        //    newWindowThread.SetApartmentState(ApartmentState.STA);
        //    // Make the thread a background thread
        //    newWindowThread.IsBackground = true;
        //    // Start the thread
        //    newWindowThread.Start();
        //}

        //private Window GetWindow(string window, string board, string seccode)
        //{
        //    Window res = null;
        //    switch (window)
        //    {
        //        case "level2":
        //        {
        //            res = new Level2 { DataContext = new Level2ViewModel(board, seccode) };
        //            break;
        //        }
        //        case "logbook":
        //        {
        //            res = new LogBook { DataContext = new LogBookViewModel(board, seccode) };
        //            break;
        //        }
        //        case "allTrades":
        //        {
        //            res = new AllTrades { DataContext = new AllTradesViewModel(board, seccode) };
        //            break;
        //        }
        //        case "allTradesCounter":
        //        {
        //            res = new AllTradesCounterWindow { DataContext = new AllTradesCounterViewModel(board, seccode) };
        //            break;
        //        }
        //        case "scichart":
        //        {
        //            res = new SciChartWindow { DataContext = new SciChartViewModel(board, seccode) };
        //            break;
        //        }
        //    }
        //    return res;
        //}
        //INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
