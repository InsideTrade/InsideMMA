using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{

    [XmlType("security")]
    public class Security
    {
        [XmlElement("sec_tz")]
        public string SecTz { get; set; }

        [XmlElement("seccode")]
        public string Seccode { get; set; }

        [XmlElement("board")]
        public string Board { get; set; }

        [XmlElement("shortname")]
        public string Shortname { get; set; }

        [XmlElement("decimals")]
        public int Decimals { get; set; }

        [XmlElement("market")]
        public string Market { get; set; }

        [XmlElement("sectype")]
        public string Sectype { get; set; }

        [XmlElement("opmask")]
        public string Opmask { get; set; }

        [XmlElement("minstep")]
        public double Minstep { get; set; }

        [XmlElement("lotsize")]
        public int Lotsize { get; set; }

        [XmlElement("point_cost")]
        public double PointCost { get; set; }

        [XmlElement("quotestype")]
        public int Quotestype { get; set; }

        [XmlAttribute("secid")]
        public int Secid { get; set; }

        [XmlAttribute("active")]
        public string Active { get; set; }
    }
}