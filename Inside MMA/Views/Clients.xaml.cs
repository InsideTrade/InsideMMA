using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Clients.xaml
    /// </summary>
    public partial class Clients : MetroWindow
    {
        private int _id;
        
        public Clients()
        {
            InitializeComponent();
            _id = DataHandlers.WindowDataHandler.OpenSpecialWindow(GetType().ToString(), "clients");
            Closing += (sender, args) => {
                DataHandlers.WindowDataHandler.CloseWindow(_id);
            };
        }

        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName != "Balance") return;
            e.Column.CellStyle = (Style)FindResource("BalanceCellStyle");
        }
    }
}
