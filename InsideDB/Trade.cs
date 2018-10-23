using System;

namespace InsideDB
{
    public class Trade
    {
       
        public long Tradeno { get; set; }

        public string Board { get; set; }


        public string Seccode { get; set; }

        public DateTime Time { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }


        public string Buysell { get; set; }

        public long Lotsize { get; set; }
        public long CurrentPos { get; set; }

        public string Login { get; set; }

        public virtual User User { get; set; }
    }
}