using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для InsideUserLogin.xaml
    /// </summary>
    public partial class InsideUserLogin
    {
        public InsideUserLogin()
        {
            InitializeComponent();
            var vm = new InsideUserViewModel();
            vm.InitCloseAction(Close);
            DataContext = vm;
            Link.RequestNavigate += (sender, e) =>
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            };
            Loaded += OnLoaded;
            
            var serializer = new BinaryFormatter();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/user";
            try
            {
                using (var file = File.Open(path, FileMode.Open))
                {
                    var data = (UserCredentials) serializer.Deserialize(file);
                    Login.Text =
                        Encoding.UTF8.GetString(ProtectedData.Unprotect(data.Login, data.Entropy,
                            DataProtectionScope.CurrentUser));
                    PasswordBox.Password =
                        Encoding.UTF8.GetString(ProtectedData.Unprotect(data.Password, data.Entropy,
                            DataProtectionScope.CurrentUser));
                }
            }
            catch (Exception e)
            {
            }
            Confirm.IsEnabled = PasswordBox.SecurePassword.Length != 0;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ShowOverlay();
            ProgressRing.IsActive = true;
            Application.Current.Dispatcher.InvokeAsync(
                () => ((InsideUserViewModel) DataContext).Connect(this, ProgressRing), DispatcherPriority.Background);
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Confirm.IsEnabled = PasswordBox.SecurePassword.Length != 0;
        }

        private void ViewHelp(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://youtu.be/C1IIntww6Uk");
        }
    }
}
