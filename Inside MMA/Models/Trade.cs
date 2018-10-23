using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlType("trade")]
    public class ClientTrade
    {
        [XmlElement("seccode")]
        public string Seccode { get; set; }
        [XmlElement("price")]
        public double Price { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }
        [XmlElement("comission")]
        public double Comission { get; set; }
        [XmlElement("currentpos")]
        public Int64 Currentpos { get; set; }
        [XmlElement("time")]
        public string Time { get; set; }
        

        [XmlElement("tradeno")]
        public Int64 Tradeno { get; set; }

        [XmlElement("orderno")]
        public Int64 Orderno { get; set; }

        [XmlElement("board")]
        public string Board { get; set; }

        

        [XmlElement("client")]
        public string Client { get; set; }
        [XmlElement("buysell")]
        public string Buysell { get; set; }

        [XmlElement("union")]
        public string Union { get; set; }

       

        [XmlElement("brokerref")]
        public string Brokerref { get; set; }

        [XmlElement("value")]
        public double Value { get; set; }

        

        

        [XmlElement("items")]
        public Int64 Items { get; set; }

        [XmlElement("yield")]
        public double Yield { get; set; }

        
        [XmlElement("accruedint")]
        public double Accruedint { get; set; }
        [XmlElement("tradetype")]
        public string Tradetype { get; set; }
        [XmlElement("settlecode")]
        public string Settlecode { get; set; }
        [XmlElement("secid")]
        public int SecId { get; set; }
    }
}
