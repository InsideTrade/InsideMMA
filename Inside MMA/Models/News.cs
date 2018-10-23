using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Inside_MMA.Annotations;

namespace Inside_MMA.Models
{
    [XmlType("news_header")]
    public class News : INotifyPropertyChanged
    {
        private string _title;
        private string _source;
        private string _newsBody;

        [XmlElement("id")]
        public string Id { get; set; }
        [XmlElement("timestamp")]
        public string TimeStamp { get; set; }
        [XmlElement("source")]
        public string Source
        {
            get { return _source; }
            set { _source = value.Replace("![CDATA[", "").TrimEnd(']'); }
        }
        [XmlElement("title")]
        public string Title
        {
            get
            {
                return _title;

            }
            set
            {
                _title = value.Replace("![CDATA[", "").TrimEnd(']');
            }
        }
        public string NewsBody
        {
            get { return _newsBody; }
            set
            {
                if (value == _newsBody) return;
                _newsBody = value.Replace("![CDATA[", "").TrimEnd(']');
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}