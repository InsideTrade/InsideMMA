using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlType("result")]
    public class OrderResult
    {
        [XmlAttribute("success")]
        public string Success { get; set; }
        [XmlAttribute("transactionid")]
        public string TransactionId { get; set; }
        [XmlElement("message")]
        public string Message { get; set; }
    }
}