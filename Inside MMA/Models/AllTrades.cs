using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]

    //класс, описывающий XML-структуру сделок
    public class TradeItem
    {
        public string Seccode { get; set; }
        public string Board { get; set; }

        public double Price { get; set; }
        public int Quantity { get; set; }
        public string Time { get; set; }
        public string Buysell { get; set; }
        public string Sum { get; set; }
        public bool IsMul { get; set; }
        public string PriceList { get; set; }
        public string MiSide { get; set; }
        public string OpenInterest { get; set; }
        public string InterestDelta { get; set; }
        public bool IsEaten { get; set; }
        public TradeItem(string seccode, double price, int quantity, string time, string buysell, string sum)
        {
            Seccode = seccode;
            Price = price;
            Quantity = quantity;
            Time = time;
            Buysell = buysell;
            Sum = sum;
            IsMul = false;
        }
        public TradeItem () { }
    }

    [XmlType("trade")]
    public partial class Trade
    {
        [XmlElement("tradeno")]
        public Int64 Tradeno { get; set; }

        [XmlElement("board")]
        public string Board { get; set; }

        [XmlElement("time")]
        public string Time { get; set; }

        [XmlElement("price")]
        public double Price { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }

        [XmlElement("buysell")]
        public string Buysell { get; set; }

        [XmlElement("seccode")]
        public string Seccode { get; set; }

        [XmlElement("period")]
        public string Period { get; set; }

        [XmlElement("openinterest")]
        public int Openinterest { get; set; }

        [XmlIgnore]
        public bool OpeninterestSpecified { get; set; }
        
        [XmlAttribute("secid")]
        public int Secid { get; set; }
    }
}
