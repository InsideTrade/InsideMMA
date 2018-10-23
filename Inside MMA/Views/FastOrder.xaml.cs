using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для FastOrder.xaml
    /// </summary>
    public partial class FastOrder
    {
        public FastOrder()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((FastOrderViewModel) DataContext).Dialog = DialogCoordinator.Instance;
        }

        private void Expander_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Expander.IsExpanded = false;
        }

        private void StopTypeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ContentControl == null) return;
            switch (StopType.SelectedIndex)
            {
                case 0:
                    ContentControl.Content = null;
                    break;
                case 1:
                    ContentControl.Content = FindResource("StopSpread");
                    break;
                case 2:
                    ContentControl.Content = FindResource("ManualStop");
                    break;
            }
        }

        private void ResetPrices(object sender, RoutedEventArgs e)
        {
            var vm = (FastOrderViewModel) DataContext;
            vm.BuyPrice = 0;
            vm.SellPrice = 0;
        }
    }
}
