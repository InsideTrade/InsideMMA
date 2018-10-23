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
using MahApps.Metro.Controls;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для LogBook.xaml
    /// </summary>
    public partial class LogBook
    {
        public LogBook()
        {
            InitializeComponent();
        }

        private void Expander_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Expander.IsExpanded = false;
        }
    }
}
