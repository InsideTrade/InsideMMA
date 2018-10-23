using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inside_MMA.Models
{
    public class Post : INotifyPropertyChanged
    {
        private string _date;
        private string _time;
        private string _name;
        private string _country;
        private string _dateSecret;
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
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public string Country
        {
            get => _country;
            set
            {
                _country = value;
                OnPropertyChanged();
            }
        }
        public string DateSecret
        {
            get => _dateSecret;
            set
            {
                _dateSecret = value;
                OnPropertyChanged();
            }
        }
        public Post()
        {
            _date = string.Empty;
            _time = string.Empty;
            _name = string.Empty;
            _country = string.Empty;
            _dateSecret = string.Empty;

        }
        public Post(string date, string time, string name, string country)
        {
            Date = date;
            Time = time;
            Name = name;
            Country = country;
        }
        public Post(string date, string time, string name, string country, string dateSec)
        {
            Date = date;
            Time = time;
            Name = name;
            Country = country;
            DateSecret = dateSec;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
