using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.Properties;

namespace Inside_MMA.ViewModels
{
    public delegate void LoginHandler(string login, string password, string server, string port);
    public class LoginFormViewModel : INotifyPropertyChanged, ICloseable
    {
        public Action CloseAction { get; set; }

        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        private string _login;

        public string Server
        {
            get { return _server; }
            set
            {
                switch (value)
                {
                    case "Server ru1":
                        Settings.Default.Server = "Server ru1";
                        _serverAddress = "tr1.finam.ru";
                        Port = "3900";
                        break;
                    case "Server ru2":
                        Settings.Default.Server = "Server ru2";
                        _serverAddress = "tr2.finam.ru";
                        Port = "3900";
                        break;
                    case "Server eu1":
                        Settings.Default.Server = "Server eu1";
                        _serverAddress = "mmar01-tr01.just2trade.online";
                        Port = "3900";
                        break;
                    case "Server eu2":
                        Settings.Default.Server = "Server eu2";
                        _serverAddress = "mmar01-tr02.just2trade.online";
                        Port = "3900";
                        break;
                    case "Demo server":
                        Settings.Default.Server = "Demo server";
                        _serverAddress = "tr1-demo5.finam.ru";
                        Port = "3939";
                        break;
                    case "Demo EU":
                        Settings.Default.Server = "Demo EU";
                        _serverAddress = "MMADemo.just2trade.online";
                        Port = "13900";
                        break;
                }
                _server = value;
                OnPropertyChanged(); 
                Settings.Default.Save();
                
            }
        }

        private string _server;
        private string _serverAddress;
        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged();
            }
        }

        private string _port;
        

        public event LoginHandler LoginHandlerEvent;

        public ICommand ConnectCommand { get; set; }
        public LoginFormViewModel()
        {
            Server = string.IsNullOrEmpty(Settings.Default.Server) ? "Server ru1" : Settings.Default.Server;
            ConnectCommand = new Command(Connect);
            GetLogin();
        }

        private void Connect(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;
            SaveCredentials();
            CloseAction();
            OnLoginHandlerEvent(Login, password, _serverAddress, Port);
        }

        private void SaveCredentials()
        {
            var login = Encoding.UTF8.GetBytes(Login);
            var entropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }
            login = ProtectedData.Protect(login, entropy, DataProtectionScope.CurrentUser);
            var serializer = new BinaryFormatter();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings";
            Directory.CreateDirectory(path);
            using (var file = File.Open(path + "/finam", FileMode.OpenOrCreate))
            {
                serializer.Serialize(file, new [] {login, entropy});
            }
        }

        private void GetLogin()
        {
            var serializer = new BinaryFormatter();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/finam";
            try
            {
                using (var file = File.Open(path, FileMode.Open))
                {
                    var data = (byte[][])serializer.Deserialize(file);
                    Login =
                        Encoding.UTF8.GetString(ProtectedData.Unprotect(data[0], data[1],
                            DataProtectionScope.CurrentUser));
                }
            }
            catch
            {
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnLoginHandlerEvent(string login, string password, string server, string port)
        {
            LoginHandlerEvent?.Invoke(login, password, server, port);
        }
    }
}
