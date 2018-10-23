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
using Inside_MMA.Models;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для TradesCounterBarChart.xaml
    /// </summary>
    public partial class TradesCounterBarChart
    {
        private XyDataSeries<int, int> _buy = new XyDataSeries<int, int> { AcceptsUnsortedData = true };
        private XyDataSeries<int, int> _sell = new XyDataSeries<int, int> { AcceptsUnsortedData = true };
        private dynamic _context;
        public TradesCounterBarChart(dynamic context)
        {
            InitializeComponent();
            //tradeItems = tradeItems.OrderBy(trade => trade.Quantity).ToList();
            //for (var i = 1; i <= tradeItems.Max(item => item.Quantity); i++)
            //{
            //    _buy.Append(i, 0);
            //    _sell.Append(i, 0);
            //}
            //foreach (var item in tradeItems)
            //{
            //    _buy.Update(_buy.XValues.IndexOf(item.Quantity), item.Buy);
            //    _sell.Update(_sell.XValues.IndexOf(item.Quantity), -item.Sell);
            //}
            //BuySeries.DataSeries = _buy;
            //SellSeries.DataSeries = _sell;
            _context = context;
        }

        //public void Update(AllTradesCounter data)
        //{
        //    if (data.Quantity > _buy.XValues.Max())
        //    {
        //        for (var i = _buy.XValues.Max() + 1; i <= data.Quantity; i++)
        //        {
        //            _buy.Append(i, 0);
        //            _sell.Append(i, 0);
        //        }
        //    }

        //    _buy.Update(_buy.XValues.IndexOf(data.Quantity), data.Buy);
        //    _sell.Update(_sell.XValues.IndexOf(data.Quantity), -data.Sell);
        //}

        private void TradesCounterBarChart_OnClosed(object sender, EventArgs e)
        {
            _context.CloseChart();
        }
    }
}
