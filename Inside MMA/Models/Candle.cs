using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlRoot(ElementName = "candle")]
    public class Candle
    {
        [XmlAttribute("date")]
        public string Time { get; set; }
        [XmlAttribute("open")]
        public double Open { get; set; }
        [XmlAttribute("close")]
        public double Close { get; set; }
        [XmlAttribute("high")]
        public double High { get; set; }
        [XmlAttribute("low")]
        public double Low { get; set; }
        [XmlAttribute("volume")]
        public int Volume { get; set; }

        public DateTime TradeTime => DateTime.Parse(Time);
    }
    [XmlRoot(ElementName = "candles")]
    public class Candles
    {
        [XmlElement(ElementName = "candle")]
        public List<Candle> Candle { get; set; }
        [XmlAttribute(AttributeName = "secid")]
        public string Secid { get; set; }
        [XmlAttribute(AttributeName = "board")]
        public string Board { get; set; }
        [XmlAttribute(AttributeName = "seccode")]
        public string Seccode { get; set; }
        [XmlAttribute(AttributeName = "period")]
        public string Period { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
    }
}
