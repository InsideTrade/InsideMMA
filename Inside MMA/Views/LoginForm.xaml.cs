using System;
using System.Windows;
using System.Windows.Controls;
using Inside_MMA.ViewModels;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Login_form.xaml
    /// </summary>
    public partial class LoginForm
    {
        public LoginFormViewModel LoginFormViewModel { set; get; } = new LoginFormViewModel();
        public LoginForm()
        {
            InitializeComponent();
            DataContext = LoginFormViewModel;
            LoginFormViewModel.InitCloseAction(Close);
            Connect.IsEnabled = Password.SecurePassword.Length != 0 && Login.Text.Length != 0;
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Connect.IsEnabled = Password.SecurePassword.Length != 0 && Login.Text.Length != 0;
        }

        private void Login_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Connect.IsEnabled = Password.SecurePassword.Length != 0 && Login.Text.Length != 0;
        }
    }
}
