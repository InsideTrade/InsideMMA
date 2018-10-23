using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для AlertMessage.xaml
    /// </summary>
    public partial class AlertMessage
    {
        private Timer _timer;
        public AlertMessage(string board, string seccode, string text, int time = 120, string type = null)
        {
            InitializeComponent();
            Text.Text =  $"{type}{board} {seccode}\r\n{text}";
            SystemSounds.Asterisk.Play();
            _timer = new Timer(Close, null, time * 1000, 0);
        }

        private void Close(object state)
        {
            _timer.Dispose();
            Dispatcher.Invoke(Close);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
