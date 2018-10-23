

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using finam.ru_economic_calendar;
using Inside_MMA.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class CalendarMainWindow : MetroWindow
    {
        public List<UniqTimer> listTimers = new List<UniqTimer>();
        static double width = SystemParameters.FullPrimaryScreenWidth;
        static double height = SystemParameters.FullPrimaryScreenHeight;
        public int timerMin = 15;
        WindowMessage WM;

        public static string UserIdTelegram = string.Empty;
        static MyTelegram myTel = new MyTelegram();
        public CalendarMainWindow()
        {
            InitializeComponent();
        }

        async void OnChecked(object sender, RoutedEventArgs e)
        {
            DataGridCell test = (DataGridCell)sender;
            Post post = (Post)test.DataContext;

            TimeSpan ts = TimeSpan.FromMilliseconds(0);

            string[] strs = post.DateSecret.Split(' ');
            string date = string.Format("{0}/{1}/{2} {3}", strs[0], DateConvert(strs[1]), DateTime.Now.Year, post.Time);
            DateTime dt = Convert.ToDateTime(date);

            if (dt.Ticks < DateTime.Now.Ticks)
            {
                await this.ShowMessageAsync("Внимание!!!", "Новость которую выбрали уже прошла...");
                test.IsEnabled = false;
            }
            else
            {
                ts = dt - DateTime.Now.AddMinutes(timerMin);
                UniqTimer uniqTimer = new UniqTimer()
                {
                    post = (Post)test.DataContext,
                    Id = post.Name + post.Time,
                    Timer = new Timer((ts.TotalMilliseconds < 0) ? 1 : ts.TotalMilliseconds)
                };

                uniqTimer.Timer.Elapsed += Timer_Elapsed;
                uniqTimer.Timer.AutoReset = false;
                uniqTimer.Timer.Start();

                listTimers.Add(uniqTimer);
            }

        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;
            Post post = listTimers.FirstOrDefault(a => a.Timer == timer)?.post;

            Dispatcher.Invoke(() => { new WindowMessage(height, width, post, UserIdTelegram).Show(); });
        }
        void Unchecked(object sender, RoutedEventArgs e)
        {
            DataGridCell test = (DataGridCell)sender;
            Post post = (Post)test.DataContext;
            string tmp = post.Name + post.Time;
            var item = listTimers.FirstOrDefault(p => p.Id == tmp);
            if (item != null)
            {
                item.Timer.Stop();
                listTimers.Remove(item);
            }
        }
        private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Post post = postsGrid.SelectedItem as Post;
            string[] test = post.DateSecret.Split(' ');
            //"01/08/2008 14:50:50.42";
            string date = $"{test[0]}/{DateConvert(test[1])}/{DateTime.Now.Year} {post.Time}";
            DateTime dt = Convert.ToDateTime(date);
            if (DateTime.Compare(dt, DateTime.Now) < 0 || DateTime.Compare(dt, DateTime.Now) == 0)
            {
                DownloadFileQuotations DFQ = new DownloadFileQuotations {post = post};
                DFQ.Show();
            }
            else
            {
                MessageBox.Show("Этой новости ещё не было");
            }

        }
        public string DateConvert(string date)
        {
            string month = string.Empty;
            switch (date)
            {
                case "Янв": { month = "01"; break; }
                case "Фев": { month = "02"; break; }
                case "Мар": { month = "03"; break; }
                case "Апр": { month = "04"; break; }
                case "Май": { month = "05"; break; }
                case "Июн": { month = "06"; break; }
                case "Июл": { month = "07"; break; }
                case "Авг": { month = "08"; break; }
                case "Сен": { month = "09"; break; }
                case "Окт": { month = "10"; break; }
                case "Ноя": { month = "11"; break; }
                case "Дек": { month = "12"; break; }
            }
            return month;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbItem = (ComboBoxItem)cb_Timer.SelectedItem;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TelegramSettings ts = new TelegramSettings();
            ts.Show();
            ts.Closed += Ts_Closed;
        }

        private void Ts_Closed(object sender, EventArgs e)
        {
            UserIdTelegram = ((TelegramSettings)sender).result;
            if (UserIdTelegram != null)
            {
                lb_Telegram.Content = "Hello, " + UserIdTelegram;
                myTel.userID = UserIdTelegram;
                myTel.SendMessage("Вы успешно подписались на нашего бота!!!");
            }
        }

        private void DataGridRowPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var row = GetVisualParentByType((FrameworkElement)e.OriginalSource, typeof(DataGridRow)) as DataGridRow;
                if (row != null)
                {
                    row.IsSelected = !row.IsSelected;
                    e.Handled = true;
                }
            }
        }

        public static DependencyObject GetVisualParentByType(DependencyObject startObject, Type type)
        {
            DependencyObject parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }
    }


}