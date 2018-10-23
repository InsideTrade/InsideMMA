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

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для ChartStopAlert.xaml
    /// </summary>
    public partial class ChartStopAlert
    {
        private double _oTop;
        private double _oLeft;
        public bool? ExecuteStop;
        public ChartStopAlert(Window owner, string text)
        {
            InitializeComponent();
            TextBlock.Text = $"This {text} will execute immediately.\r\nAre you sure?";
            Owner = owner;
            _oTop = owner.Top;
            _oLeft = owner.Left;
            Owner.LocationChanged += (sender, args) =>
            {
                Left += owner.Left - _oLeft;
                Top += owner.Top - _oTop;
                _oTop = owner.Top;
                _oLeft = owner.Left;
            };
            //Owner.SizeChanged += (sender, args) =>
            //{
            //    Left += args.NewSize.Width - args.PreviousSize.Width;
            //    Top += args.NewSize.Height - args.PreviousSize.Height;
            //};
        }
        private void YesClick(object sender, RoutedEventArgs e)
        {
            ExecuteStop = true;
            Close();
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            ExecuteStop = false;
            Close();
        }
    }
}
