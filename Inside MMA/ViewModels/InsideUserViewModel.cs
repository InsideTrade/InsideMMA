using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using InsideDB;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.AspNet.SignalR.Client;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Views;
using Newtonsoft.Json.Linq;

namespace Inside_MMA.ViewModels
{
    public class InsideUserViewModel : INotifyPropertyChanged, ICloseable
    {
        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                if (value == _login) return;
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Error
        {
            get { return _error; }
            set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public string LicenseExpired
        {
            get { return _licenseExpired; }
            set
            {
                if (value == _licenseExpired) return;
                _licenseExpired = value;
                OnPropertyChanged();
            }
        }

        public bool ErrorCollapsed
        {
            get { return _errorCollapsed; }
            set
            {
                if (value == _errorCollapsed) return;
                _errorCollapsed = value;
                OnPropertyChanged();
            }
        }

        public bool LinkCollapsed
        {
            get { return _linkCollapsed; }
            set
            {
                if (value == _linkCollapsed) return;
                _linkCollapsed = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; set; }
        private PasswordBox _pass;
        private IHubProxy _hub;
        private string _error;
        private HubConnection _connection;
        private string _licenseExpired = "Your license has expired. Click here to visit inside-trade.ru.\n Ваша лицензия истекла";
        private bool _errorCollapsed = true;
        private bool _linkCollapsed = true;

        public InsideUserViewModel()
        {
            ConfirmCommand = new Command(Confirm);
        }

        public async void Connect(MetroWindow window, ProgressRing ring)
        {
            await Task.Run(() => {
                const string url = @"http://194.87.232.14:999";
                //const string url = @"http://localhost:8080";
                _connection = new HubConnection(url);
                _hub = _connection.CreateHubProxy("TransaqHub");
                try
                {
                    _connection.Start().Wait();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        window.HideOverlay();
                        ring.IsActive = false;
                    });
                    _hub.On("ServerReply", msg => CheckServerReply(msg));
                }
                catch (Exception e)
                {

                    Application.Current.Dispatcher.Invoke(async () =>
                        {
                            ring.IsActive = false;
                            await window.ShowMessageAsync("Error", "Server is unavailable.");
                            Application.Current.Shutdown();
                        });
                }
            });
            
        }
        private void Confirm(object password)
        {
            var pass = (PasswordBox) password;
            _hub.Invoke("CheckCredentials", Login, pass.Password);
            _pass = pass;
        }

        private void CheckServerReply(dynamic msg)
        {
            var user = (User) (msg as JObject)?.ToObject(typeof(User));
            if (user != null)
            {
                ClientInfo.InsideLogin = Login;
                var isAdmin = user.Role == "admin";
                SaveCredentials();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    new MainWindow(!isAdmin, _connection, _hub, user).Show();
                    CloseAction();
                });
            }
            else if (msg == "notFound")
            {
                LinkCollapsed = true;
                ErrorCollapsed = false;
                Error = "Invalid credentials";
            }
            else if (msg == "licenseExpired")
            {
                LinkCollapsed = false;
                ErrorCollapsed = true;
            }
            else if (msg == "online")
            {
                LinkCollapsed = true;
                ErrorCollapsed = false;
                Error = "Account is already logged in. Try again in 20 seconds.\n Аккаунт уже в системе. Попробуйте снова через 20 секунд";
            }
        }

        private void SaveCredentials()
        {
            var login = Encoding.UTF8.GetBytes(Login);
            var pass = Encoding.UTF8.GetBytes(_pass.Password);
            var entropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }
            login = ProtectedData.Protect(login, entropy, DataProtectionScope.CurrentUser);
            pass = ProtectedData.Protect(pass, entropy, DataProtectionScope.CurrentUser);
            var serializer = new BinaryFormatter();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings";
            Directory.CreateDirectory(path);
            using (var file = File.Open(path + "/user", FileMode.OpenOrCreate))
            {
                serializer.Serialize(file, new UserCredentials {Login = login, Password = pass, Entropy = entropy});
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Action CloseAction { get; set; }
    }
}
