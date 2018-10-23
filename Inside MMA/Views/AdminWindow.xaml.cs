using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using InsideDB;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow
    {
        private CollectionViewSource _viewSource;

        public AdminWindow()
        {
            InitializeComponent();
            _viewSource = FindResource("Users") as CollectionViewSource;
            RusRadio.IsChecked = true;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            PropertyDescriptor propertyDescriptor = (PropertyDescriptor)e.PropertyDescriptor;
            e.Column.Header = propertyDescriptor.DisplayName;
            if (propertyDescriptor.DisplayName != "Login" &&
                propertyDescriptor.DisplayName != "Role" &&
                propertyDescriptor.DisplayName != "LicenseExpDate" &&
                propertyDescriptor.DisplayName != "Status" &&
                propertyDescriptor.DisplayName != "Email" &&
                propertyDescriptor.DisplayName != "ConnectionID" &&
                propertyDescriptor.DisplayName != "Sleep" &&
                propertyDescriptor.DisplayName != "TotalBalance" &&
                propertyDescriptor.DisplayName != "AutoSleep" &&
                propertyDescriptor.DisplayName != "SleepThreshold" &&
                propertyDescriptor.DisplayName != "ProfitControl" &&
                propertyDescriptor.DisplayName != "ProfitLimit" &&
                propertyDescriptor.DisplayName != "ProfitLossLimit")
            {
                e.Cancel = true;
            }
        }

        private void RusClick(object sender, RoutedEventArgs e)
        {
            _viewSource.Filter -= FilterUsa;
            _viewSource.Filter += FilterRus;
        }

        private void UsaClick(object sender, RoutedEventArgs e)
        {
            _viewSource.Filter -= FilterRus;
            _viewSource.Filter += FilterUsa;
        }

        private void FilterRus(object sender, FilterEventArgs e)
        {
            var user = e.Item as User;
            if (user == null)
                e.Accepted = false;
            else if (user.IsUsa)
                e.Accepted = false;
        }

        private void FilterUsa(object sender, FilterEventArgs e)
        {
            var user = e.Item as User;
            if (user == null)
                e.Accepted = false;
            else if (!user.IsUsa)
                e.Accepted = false;
        }
    }
}
