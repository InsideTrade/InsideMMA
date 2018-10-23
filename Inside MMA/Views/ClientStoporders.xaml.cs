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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Inside_MMA.Properties;
using Inside_MMA.ViewModels;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для ClientStoporders.xaml
    /// </summary>
    public partial class ClientStoporders
    {
        private int _id;

        private bool _rmbClicked;

        public ClientStoporders()
        {
            InitializeComponent();
            Stoporders.SelectedIndex = -1;
            _id = DataHandlers.WindowDataHandler.OpenSpecialWindow(GetType().ToString(), "stoporders");
            Closing += (sender, args) => {
                DataHandlers.WindowDataHandler.CloseWindow(_id);
            };
        }

        //private void RowClick(object sender, MouseButtonEventArgs e)
        //{
        //    var row = (DataGridRow)sender;
        //    if (!row.IsSelected) Stoporders.SelectedIndex = -1;
        //    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        //}

        private void Stoporders_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Stoploss" || e.PropertyName == "Takeprofit" || e.PropertyName == "SlCollapsed" || e.PropertyName == "TpCollapsed")
            {
                e.Column = null;
            }
        }

        //private void Details_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var row = (DataGridRow) Stoporders.ItemContainerGenerator.ContainerFromIndex(Stoporders.SelectedIndex);
        //    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible
        //        ? Visibility.Collapsed
        //        : Visibility.Visible;
        //}


        //private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        //{
        //    var menuItem = (MenuItem) ((ContextMenu) sender).Items[0];
        //    var row = (DataGridRow)Stoporders.ItemContainerGenerator.ContainerFromIndex(Stoporders.SelectedIndex);
        //    menuItem.Header = row.DetailsVisibility == Visibility.Visible
        //        ? "Hide details"
        //        : "Details";
        //}

        private void RightMouseOnRow(object sender, MouseButtonEventArgs e)
        {
            _rmbClicked = true;
        }

        private void MouseEnterRow(object sender, MouseEventArgs e)
        {
            ((DataGridRow)sender).IsSelected = true;
        }

        private void MouseLeaveRow(object sender, MouseEventArgs e)
        {
            if (_rmbClicked)
                _rmbClicked = false;
            else
            {
                var row = (DataGridRow) sender;
                row.IsSelected = false;
            }
        }

        private void ToggleDetails(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            if (row.DetailsVisibility == Visibility.Visible)
                row.DetailsVisibility = Visibility.Collapsed;
            else
                row.DetailsVisibility = Visibility.Visible;
        }

        private void MouseOverCancel(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        
    }
}
