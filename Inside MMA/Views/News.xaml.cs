using System.Windows.Controls;
using System.Windows.Input;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для News.xaml
    /// </summary>
    public partial class News
    {
        public News()
        {
            InitializeComponent();
        }

        private void DetailsEvent(object sender, DataGridRowDetailsEventArgs e)
        {
            var news = (Inside_MMA.Models.News)e.Row.Item;
            //if (news.NewsBody == null)
            TXmlConnector.ConnectorSendCommand(
                    $"<command id=\"get_news_body\" news_id=\"{news.Id}\"/>");
        }

    }
}
