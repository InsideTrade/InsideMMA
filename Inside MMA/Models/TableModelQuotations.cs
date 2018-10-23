using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Inside_MMA.Models
{
    class TableModelQuotations : INotifyPropertyChanged
    {
        private TaskScheduler _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private bool _isReading;
        private string _title;
        private string _min;
        private string _price;
        private string _max;
        private bool _isUpdating;
        private List<CalendarItem> _buffer;
        public static ObservableCollection<CalendarItem> Items { get; set; } = new ObservableCollection<CalendarItem>();
        public string TITLE
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        public string MIN
        {
            get => _min;
            set
            {
                _min = value;
                OnPropertyChanged();
            }
        }
        public string MAX
        {
            get => _max;
            set
            {
                _max = value;
                OnPropertyChanged();
            }
        }
        public string PRICE
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }
        public InfoModel Info { set; get; }
        public bool IsReading
        {
            get => _isReading;
            set
            {
                _isReading = value;
                OnPropertyChanged();
            }
        }
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }
        private void ReadTask()
        {
            IsReading = true;
            _buffer = new List<CalendarItem>();
            string pathTest = "Files/" + Info.NameFile;
            using (StreamReader fs = new StreamReader(pathTest))
            {
                const char delimiter = ';';
                while (true)
                {
                    string value = fs.ReadLine();
                    if (value == "<DATE>;<TIME>;<LAST>;<VOL>;<ID>;<OPER>")
                    { }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        _buffer.Add(new CalendarItem(value.Split(delimiter)));
                    }
                    else if (string.IsNullOrEmpty(value))
                    {
                        break;
                    }
                }
            }
            IsReading = false;
        }
        private void LoadItemsToPanel()
        {
            Items.Clear();
            DateTime time1 = DateTime.Parse(Info.TimeSearch);
            double tik = Convert.ToDouble(Info.Tiker);
            DateTime time2 = time1.AddMinutes(Convert.ToDouble(tik));
            
            IsUpdating = true;
            foreach (CalendarItem item in _buffer)
            {
                if (Convert.ToDateTime(item.Time) >= Convert.ToDateTime(Info.TimeSearch) && Convert.ToDateTime(item.Time) <= time2)
                {
                    PRICE = item.Last;
                }
            }
            double min = 0;
            double max = 0;
            if (PRICE != null)
            {
                min = Convert.ToDouble(PRICE.Replace(".", ","));
                max = Convert.ToDouble(PRICE.Replace(".", ","));
            }
            foreach (CalendarItem item in _buffer)
            {
                if (DateTime.Parse(item.Time) >= time1 && DateTime.Parse(item.Time) <= time2)
                {
                    if (double.Parse(item.Last.Replace(".", ",").Trim()) < min)
                    {
                        min = double.Parse(item.Last.Replace(".", ",").Trim());
                    }
                    else if (double.Parse(item.Last.Replace(".", ",").Trim()) > max)
                    {
                        max = double.Parse(item.Last.Replace(".", ",").Trim());
                    }
                    Items.Add(item);
                }
            }
            if (Items.Count == 0)
            {
                TITLE = "Статистики за данный период нет!!!";
            }
            else
            {
                MIN = "MIN: " + min.ToString(CultureInfo.InvariantCulture);
                MAX = "MAX: " + max.ToString(CultureInfo.InvariantCulture);
            }
            IsUpdating = false;
        }

        public TableModelQuotations(InfoModel _info)
        {
            Info = _info;
            Task.Factory.StartNew(ReadTask).ContinueWith(t => LoadItemsToPanel(), _uiScheduler);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
