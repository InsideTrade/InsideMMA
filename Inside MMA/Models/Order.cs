using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlRoot(ElementName = "order")]
    public class Order
    {
        [XmlElement(ElementName = "seccode")]
        public string Seccode { get; set; }
        [XmlElement(ElementName = "price")]
        public string Price { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
        [XmlElement(ElementName = "buysell")]
        public string Buysell { get; set; }
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }
        [XmlElement(ElementName = "time")]
        public string Time { get; set; }
        [XmlElement(ElementName = "withdrawtime")]
        public string Withdrawtime { get; set; }
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
        [XmlElement(ElementName = "board")]
        public string Board { get; set; }
        [XmlElement(ElementName = "union")]
        public string Union { get; set; }
        [XmlElement(ElementName = "orderno")]
        public string Orderno { get; set; }
        [XmlElement(ElementName = "secid")]
        public string Secid { get; set; }
        [XmlElement(ElementName = "client")]
        public string Client { get; set; }
        [XmlElement(ElementName = "expdate")]
        public string Expdate { get; set; }
        [XmlElement(ElementName = "origin_orderno")]
        public string OriginOrderno { get; set; }
        [XmlElement(ElementName = "accepttime")]
        public string Accepttime { get; set; }
        [XmlElement(ElementName = "brokerref")]
        public string Brokerref { get; set; }
        
        [XmlElement(ElementName = "accruedint")]
        public string Accruedint { get; set; }
        [XmlElement(ElementName = "settlecode")]
        public string Settlecode { get; set; }
        [XmlElement(ElementName = "balance")]
        public string Balance { get; set; }
        
        [XmlElement(ElementName = "quantity")]
        public string Quantity { get; set; }
        [XmlElement(ElementName = "hidden")]
        public string Hidden { get; set; }
        [XmlElement(ElementName = "yield")]
        public string Yield { get; set; }
        
        [XmlElement(ElementName = "condition")]
        public string Condition { get; set; }
        [XmlElement(ElementName = "conditionvalue")]
        public string Conditionvalue { get; set; }
        [XmlElement(ElementName = "validafter")]
        public string Validafter { get; set; }
        [XmlElement(ElementName = "validbefore")]
        public string Validbefore { get; set; }
        [XmlElement(ElementName = "maxcomission")]
        public string Maxcomission { get; set; }
        
        [XmlAttribute(AttributeName = "transactionid")]
        public string Transactionid { get; set; }
    }

    [XmlRoot(ElementName = "stoploss")]
    public class Stoploss
    {
        [XmlElement(ElementName = "activationprice")]
        public string Activationprice { get; set; }
        [XmlElement(ElementName = "guardtime")]
        public string Guardtime { get; set; }
        [XmlElement(ElementName = "brokerref")]
        public string Brokerref { get; set; }
        [XmlElement(ElementName = "quantity")]
        public string Quantity { get; set; }
        [XmlElement(ElementName = "bymarket")]
        public string Bymarket { get; set; }
        [XmlElement(ElementName = "orderprice")]
        public string Orderprice { get; set; }
        [XmlAttribute(AttributeName = "usecredit")]
        public string Usecredit { get; set; }
        public bool IsByMarket => Bymarket != null;
    }

    [XmlRoot(ElementName = "takeprofit")]
    public class Takeprofit
    {
        [XmlElement(ElementName = "activationprice")]
        public string Activationprice { get; set; }
        [XmlElement(ElementName = "guardtime")]
        public string Guardtime { get; set; }
        [XmlElement(ElementName = "brokerref")]
        public string Brokerref { get; set; }
        [XmlElement(ElementName = "quantity")]
        public string Quantity { get; set; }
        [XmlElement(ElementName = "extremum")]
        public string Extremum { get; set; }
        [XmlElement(ElementName = "level")]
        public string Level { get; set; }
        [XmlElement(ElementName = "correction")]
        public string Correction { get; set; }
        [XmlElement(ElementName = "guardspread")]
        public string Guardspread { get; set; }
    }

    [XmlRoot(ElementName = "stoporder")]
    public class Stoporder
    {
        [XmlElement(ElementName = "seccode")]
        public string Seccode { get; set; }
        [XmlElement(ElementName = "buysell")]
        public string Buysell { get; set; }
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
        [XmlElement(ElementName = "accepttime")]
        public string Accepttime { get; set; }
        [XmlElement(ElementName = "withdrawtime")]
        public string Withdrawtime { get; set; }
        [XmlElement(ElementName = "client")]
        public string Client { get; set; }
        [XmlElement(ElementName = "activeorderno")]
        public string Activeorderno { get; set; }
        [XmlElement(ElementName = "secid")]
        public string Secid { get; set; }
        [XmlElement(ElementName = "board")]
        public string Board { get; set; }
        [XmlElement(ElementName = "canceller")]
        public string Canceller { get; set; }
        [XmlElement(ElementName = "alltradeno")]
        public string Alltradeno { get; set; }
        [XmlElement(ElementName = "validbefore")]
        public string Validbefore { get; set; }
        [XmlElement(ElementName = "author")]
        public string Author { get; set; }
        
        [XmlElement(ElementName = "linkedorderno")]
        public string Linkedorderno { get; set; }
        [XmlElement(ElementName = "expdate")]
        public string Expdate { get; set; }
        
        [XmlElement(ElementName = "stoploss")]
        public List<Stoploss> Stoploss { get; set; }
        [XmlElement(ElementName = "takeprofit")]
        public List<Takeprofit> Takeprofit { get; set; }
        
        
        [XmlAttribute(AttributeName = "transactionid")]
        public string Transactionid { get; set; }

        public bool SlCollapsed => Stoploss.Count == 0;
        public bool TpCollapsed => Takeprofit.Count == 0;
    }

    [XmlRoot(ElementName = "orders")]
    public class Orders
    {
        [XmlElement(ElementName = "order")]
        public List<Order> Order { get; set; }
        [XmlElement(ElementName = "stoporder")]
        public List<Stoporder> Stoporder { get; set; }
    }

}

