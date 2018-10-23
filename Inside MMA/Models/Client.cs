using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlRoot(ElementName = "client")]
    public class Client
    {
        [XmlElement(ElementName = "market")]
        public string Market { get; set; }
        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "union")]
        public string Union { get; set; }
        [XmlElement(ElementName = "forts_acc")]
        public string FortsAcc { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "remove")]
        public string Remove { get; set; }
    }

    public static class ClientInfo
    {
        public static string Id;
        public static string Union;
        public static string InsideLogin;
    }

    [Serializable]
    public class UserCredentials
    {
        public byte[] Login { get; set; }
        public byte[] Password { get; set; }
        public byte[] Entropy { get; set; }
    }
}
