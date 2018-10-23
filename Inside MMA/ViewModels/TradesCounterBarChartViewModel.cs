using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using SciChart.Charting.Model.DataSeries;

namespace Inside_MMA.ViewModels
{
    public class TradesCounterBarChartViewModel : INotifyPropertyChanged
    {
        private XyDataSeries<int, int> _buy = new XyDataSeries<int, int> { AcceptsUnsortedData = true };
        private XyDataSeries<int, int> _sell = new XyDataSeries<int, int> { AcceptsUnsortedData = true };

        public XyDataSeries<int, int> Buy
        {
            get { return _buy; }
            set
            {
                if (Equals(value, _buy)) return;
                _buy = value;
                OnPropertyChanged();
            }
        }

        public XyDataSeries<int, int> Sell
        {
            get { return _sell; }
            set
            {
                if (Equals(value, _sell)) return;
                _sell = value;
                OnPropertyChanged();
            }
        }

        public TradesCounterBarChartViewModel(List<AllTradesCounterItem> tradeItems)
        {
            tradeItems = tradeItems.OrderBy(trade => trade.Quantity).ToList();
            for (var i = 1; i <= tradeItems.Max(item => item.Quantity); i++)
            {
                Buy.Append(i, 0);
                Sell.Append(i, 0);
            }
            foreach (var item in tradeItems)
            {
                Buy.Update(Buy.XValues.IndexOf(item.Quantity), item.Buy);
                Sell.Update(Sell.XValues.IndexOf(item.Quantity), -item.Sell);
            }
        }

        public void Update(AllTradesCounterItem data)
        {
            if (data.Quantity > Buy.XValues.Max())
            {
                for (var i = Buy.XValues.Max() + 1; i <= data.Quantity; i++)
                {
                    Buy.Append(i, 0);
                    Sell.Append(i, 0);
                }
            }

            Buy.Update(Buy.XValues.IndexOf(data.Quantity), data.Buy);
            Sell.Update(Sell.XValues.IndexOf(data.Quantity), -data.Sell);
        }

        public void Clear()
        {
            for (var index = 0; index < Buy.YValues.Count; index++)
            {
                Buy.YValues[index] = 0;
            }
            for (var index = 0; index < Sell.YValues.Count; index++)
            {
                Sell.YValues[index] = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}