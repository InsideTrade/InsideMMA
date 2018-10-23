using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inside_MMA.Models
{
    public class CalendarItem : INotifyPropertyChanged
    {
        private string _date;
        private string _time;
        private string _last;
        private string _vol;
        private string _id;
        private string _oper;

        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }
        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }
        public string Last
        {
            get => _last;
            set
            {
                _last = value;
                OnPropertyChanged();
            }
        }
        public string Vol
        {
            get => _vol;
            set
            {
                _vol = value;
                OnPropertyChanged();
            }
        }
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        public string Oper
        {
            get => _oper;
            set
            {
                _oper = value;
                OnPropertyChanged();
            }
        }
        public CalendarItem(string date, string time, string last, string vol, string id, string oper)
        {
            Date = date.Insert(4, ".").Insert(7, ".");
            Time = time.Insert(2, ":").Insert(5, ":");
            Last = last;
            Vol = vol;
            Id = id;
            Oper = oper;
        }

        public CalendarItem(params string[] parameters) : 
            this(date: parameters[0], time: parameters[1], last: parameters[2], vol: parameters[3], 
                id: parameters[4], oper: parameters[5]) { }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
