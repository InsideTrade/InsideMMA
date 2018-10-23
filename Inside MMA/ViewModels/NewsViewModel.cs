using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.Models;

namespace Inside_MMA.ViewModels
{
    public class NewsViewModel : INotifyPropertyChanged
    {
        private static XmlSerializer _xmlSerializer = new XmlSerializer(typeof(News));
        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        private ObservableCollection<News> _news = new ObservableCollection<News>();
        public ObservableCollection<News> News
        {
            get { return _news; }
            set
            {
                if (Equals(value, _news)) return;
                _news = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadOldNews { get; set; }
        public NewsViewModel()
        {
            TXmlConnector.SendNews += OnNews;
            LoadOldNews = new Command(arg => LoadNews());
        }

        private void LoadNews()
        {
            TXmlConnector.ConnectorSendCommand("<command id=\"get_old_news\" count=\"100\"/>");
        }

        private void OnNews(string data)
        {
            if (data.Contains("news_body"))
                AddBody(data);
            else
            {
                var newsHeader = (News)_xmlSerializer.Deserialize(new StringReader(data));
                if (News.FirstOrDefault(x => x.Id == newsHeader.Id) == null)
                    _dispatcher.Invoke(() => News.Insert(0, newsHeader));
            }
            
        }

        private void AddBody(string data)
        {
            var xr = XmlReader.Create(new StringReader(data));
            xr.ReadToDescendant("id");
            xr.Read();
            var news = News.First(n => n.Id == xr.Value);
            xr.ReadToNextSibling("text");
            xr.Read();
            xr.Read();
            news.NewsBody = xr.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}