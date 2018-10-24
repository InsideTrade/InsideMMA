using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inside_MMA.Models
{
    public class DataForCandlestick
    {
        public int Quantity { get; set; }
        public List<TradeItem> Data { get; set; }
        public List<Tick> DataTick { get; set; }
    }
}
