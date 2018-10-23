using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для ChangePassDialog.xaml
    /// </summary>
    public partial class ChangePassDialog
    {
        public bool Ok;
        public ChangePassDialog()
        {
            InitializeComponent();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Confirm.IsEnabled = OldPass.SecurePassword.Length != 0 && NewPass.SecurePassword.Length != 0 &&
                                OldPass.Password != NewPass.Password && NewPass.Password == ConfirmPass.Password;
            Error.Visibility = OldPass.Password == NewPass.Password
                ? Visibility.Visible
                : Visibility.Hidden;
            if (OldPass.Password == NewPass.Password)
            {
                Error.Text = "Enter a different password!";
                return;
            }
            if (NewPass.Password.Length != 0 && NewPass.Password != ConfirmPass.Password)
                Error.Text = "Passwords do not match!";
            Error.Visibility = ConfirmPass.Password.Length != 0 && NewPass.Password != ConfirmPass.Password
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        private async void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            var result = TXmlConnector.ConnectorSendCommand(
                $"<command id=\"change_pass\" oldpass=\"{OldPass.Password}\" newpass=\"{NewPass.Password}\"/>");
            if (result == "<result success=\"true\"/>")
            {
                await this.ShowMessageAsync("Result", "Password successfully changed");
                Close();
            }
                
            else
                await this.ShowMessageAsync("Result", result);
        }
    }
}
