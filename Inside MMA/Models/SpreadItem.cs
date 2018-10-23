using System;

namespace Inside_MMA.Models
{
    public class SpreadItem 
    {
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int BSize { get; set; }
        public int ASize { get; set; }
        public bool CompareTo(SpreadItem obj)
        {
            return ASize == obj.ASize && BSize == obj.BSize && Bid == obj.Bid && Ask == obj.Ask;
        }
    }
}