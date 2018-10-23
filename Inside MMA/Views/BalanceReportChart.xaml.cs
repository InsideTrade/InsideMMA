using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Inside_MMA.ViewModels;
using SciChart.Charting.Model.DataSeries;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для BalanceReportChart.xaml
    /// </summary>
    public partial class BalanceReportChart
    {
        public BalanceReportChart()
        {
            InitializeComponent();
        }
        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(() => ((BalanceReportChartViewModel.TabItem) TabControl.SelectedItem).Candlesticks
                    .InvalidateParentSurface(RangeMode.ZoomToFit));
            });
        }
    }
}
