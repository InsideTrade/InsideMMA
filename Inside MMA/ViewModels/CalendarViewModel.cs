using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Inside_MMA.Models;

namespace Inside_MMA.ViewModels
{
    public class CalendarViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>();
        public CalendarViewModel()
        {
            string url = "https://www.finam.ru/analysis/macroevent/" + "/?dweek=1";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient client = new WebClient();
            var data = client.DownloadString(url);
            client.Dispose();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);
            var table = doc.QuerySelectorAll("#macroevent_main_grid tr").Skip(2)
               .Select(a => new
               {
                   Data = a.QuerySelector("td:nth-child(1)").InnerText.Replace("\t", "").Replace("\r\n", "").Trim().Replace("&nbsp;", " "),
                   Time = a.QuerySelector("td:nth-child(2)").InnerText.Replace("\t", "").Replace("\r\n", "").Trim().Replace("&nbsp;", ""),
                   Name = a.QuerySelector("td:nth-child(4)").InnerText.Replace("\t", "").Replace("\r\n", "").Trim(),
                   Сountry = a.QuerySelector("td:nth-child(3)").InnerText.Replace("\t", "").Replace("\r\n", "").Trim()
               });

            string dateSec = string.Empty;
            foreach (var row in table)
            {
                if (row.Data != " ")
                {
                    dateSec = row.Data;
                }
                Post post = new Post(dateSec, row.Time, row.Name, row.Сountry, dateSec);
                Posts.Add(post);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
