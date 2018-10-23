
using System.Windows.Controls;
using System.Windows.Input;
using Inside_MMA.Properties;
using Inside_MMA.ViewModels;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для ClientOrders.xaml
    /// </summary>
    public partial class ClientOrders
    {
        private int _id;

        private bool _rmbClicked;
        public ClientOrders()
        {
            InitializeComponent();
            _id = DataHandlers.WindowDataHandler.OpenSpecialWindow(GetType().ToString(), "orders");
            Closing += (sender, args) => {
                DataHandlers.WindowDataHandler.CloseWindow(_id);
            };
        }

        private void MouseEnterRow(object sender, MouseEventArgs e)
        {
            ((DataGridRow)sender).IsSelected = true;
        }

        private void RightMouseOnRow(object sender, MouseButtonEventArgs e)
        {
            _rmbClicked = true;
        }

        private void MouseLeaveRow(object sender, MouseEventArgs e)
        {
            if (_rmbClicked)
                _rmbClicked = false;
            else
                ((DataGridRow)sender).IsSelected = false;
        }
    }
}
