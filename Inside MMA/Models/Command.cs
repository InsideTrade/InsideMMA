using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlRoot("command")]
    public class CommandConnect
    {
        [XmlElement("login")]
        public string Login { get; set; }
        
        [XmlElement("password")]
        public string Password { get; set; }
        [XmlElement("host")]
        public string Host { get; set; }
        [XmlElement("milliseconds")]
        public bool Milliseconds { get; set; }
        [XmlElement("language")]
        public string Language { get; set; }
        [XmlElement("port")]
        public string Port { get; set; }

        [XmlElement("rqdelay")]
        public int Rqdelay { get; set; }

        [XmlElement("session_timeout")]
        public int SessionTimeout { get; set; }

        [XmlElement("request_timeout")]
        public int RequestTimeout { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }
        
    }


}
