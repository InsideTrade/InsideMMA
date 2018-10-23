using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{

    [XmlType("tick")]
    public class Tick
    {
        [XmlElement("secid")]
        public int Secid { get; set; }

        [XmlElement("board")]
        public string Board { get; set; }

        [XmlElement("seccode")]
        public string Seccode { get; set; }

        [XmlElement("tradeno")]
        public Int64 Tradeno { get; set; }

        [XmlElement("tradetime")]
        public string Tradetime { get; set; }

        [XmlElement("price")]
        public double Price { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }

        [XmlElement("period")]
        public string Period { get; set; }

        [XmlElement("buysell")]
        public string Buysell { get; set; }
        [XmlElement("openinterest")]
        public string OpenInterest { get; set; }
    }
}
