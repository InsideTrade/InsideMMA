using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlType("server_status")]
    public class ServerStatus
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("connected")]
        public string Connected { get; set; }
        [XmlAttribute("recover")]
        public string Recover { get; set; }
        [XmlAttribute("server tz")]
        public string TimeZone { get; set; }
    } 
}