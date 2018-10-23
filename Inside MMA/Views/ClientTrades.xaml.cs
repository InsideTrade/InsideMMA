using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Inside_MMA.Properties;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.TeamFoundation.MVVM;
using SciChart.Core.Extensions;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для ClientTrades.xaml
    /// </summary>
    public partial class ClientTrades
    {
        private int _id;
        private bool _rmbClicked;
        public ClientTrades()
        {
            InitializeComponent();
            _id = DataHandlers.WindowDataHandler.OpenSpecialWindow(GetType().ToString(), "trades");
            Closing += (sender, args) => {
                DataHandlers.WindowDataHandler.CloseWindow(_id);
            };
        }
        

        private void RightMouseOnRow(object sender, MouseButtonEventArgs e)
        {
            _rmbClicked = true;
        }

        private void MouseEnterRow(object sender, MouseEventArgs e)
        {
            ((DataGridRow)sender).IsSelected = true;
            ((DataGridRow) sender).Focus();
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
