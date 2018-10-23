using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Inside_MMA.Models.Alerts;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls;
using SciChart.Charting.Common.Extensions;
using SciChart.Core.Extensions;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Alerts.xaml
    /// </summary>
    public partial class Alerts
    {
        private AlertsViewModel _vm;
        public Alerts(AlertsViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            _vm.SelectionWindow = this;
            _vm.InitializeAllActive();
            Closing += (sender, args) => _vm.UninitializeAll();
        }


        //private bool FilterBoards(object obj)
        //{
        //    var text = obj.ToString();
        //    if (Boards.Text == "") return true;
        //    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(text, Boards.Text, CompareOptions.IgnoreCase) >= 0;
        //}

        //private bool FilterSeccodes(object obj)
        //{
        //    var text = obj as string;
        //    return text != null && text.Contains(Boards.Text);
        //}

        //private void TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Boards.SelectedIndex = -1;
        //    Boards.Items.Filter = null;
        //    Boards.Items.Filter += FilterBoards;
        //}

        //private void OpenAlertSelectionDialog(object sender, RoutedEventArgs e)
        //{
        //    var vm = new AlertSelectionViewModel();
        //    new AlertSelection{Owner = this, DataContext = vm}.ShowDialog();
        //    if (vm.Alert != null)
        //        _vm.AlertsCollection.Add(vm.Alert);
        //}

        //private void Edit(object sender, RoutedEventArgs e)
        //{
        //    var alert = (BaseAlert)((Button)sender).TryFindParent<DataGridRow>().Item;
        //    var vm = new AlertSelectionViewModel { Alert = alert, Name = alert.Name, Board = alert.Board, Seccode = alert.Seccode };
        //    var index = _vm.AlertsCollection.IndexOf(alert);
        //    if (alert.GetType() == typeof(EqualsSizeAlert))
        //    {
        //        vm.Size = ((EqualsSizeAlert)alert).Size;
        //        new AlertSelection { Owner = this, DataContext = vm, TypesComboBox = { SelectedIndex = 0 } }.ShowDialog();
        //    }

        //}
    }
}