using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [Serializable]
    [XmlType("candle")]
    public class CandleHistory
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
    }
}
