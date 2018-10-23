using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для AllTradesCounterWindow.xaml
    /// </summary>
    public partial class AllTradesCounterWindow
    {
        public AllTradesCounterWindow()
        {
            InitializeComponent();
            Filter.Culture = CultureInfo.CurrentCulture;
            Filter2.Culture = CultureInfo.CurrentCulture;
        }

        private void Expander_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Expander.IsExpanded = false;
        }

        ~AllTradesCounterWindow()
        {
            Debug.WriteLine("Counter disposed");
        }
    }
}
