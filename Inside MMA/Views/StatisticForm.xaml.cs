using Inside_MMA.Models;
using MahApps.Metro.Controls;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для StatisticForm.xaml
    /// </summary>
    public partial class StatisticForm : MetroWindow
    {
        public InfoModel INFO { set; get; }
        public StatisticForm(InfoModel _info)
        {
            INFO = _info;
            InitializeComponent();
            DataContext = new TableModelQuotations(INFO);
        }
    }
}
