using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using InsideDB.Annotations;

namespace InsideDB
{
    public class User : INotifyPropertyChanged
    {
        private double _totalBalance;
        public string Login { get; set; }
        
        public string Password { get; set; }
        public string Role { get; set; }

        public DateTime? LicenseExpDate { get; set; }
        
        public string Status { get; set; }
        
        public string ConnectionID { get; set; }
        
        public string Sleep { get; set; }

        public bool Alerts { get; set; }

        public bool AllTrades { get; set; }

        public bool AllTradesPro { get; set; }

        public bool Chart { get; set; }

        public bool Counter { get; set; }

        public bool L2 { get; set; }

        public bool Logbook { get; set; }
       
        public string Email { get; set; }

        public bool Trading { get; set; }

        public bool FastOrder { get; set; }

        public int? SleepThreshold { get; set; }

        public bool AutoSleep { get; set; }

        public bool ProfitControl { get; set; }
        public bool ProfitFixed { get; set; }
        public double? ProfitLimit { get; set; }
        public double? ProfitLossLimit { get; set; }

        public double TotalBalance
        {
            get { return _totalBalance; }
            set
            {
                if (value.Equals(_totalBalance)) return;
                _totalBalance = value;
                OnPropertyChanged();
            }
        }

        public bool IsUsa { get; set; }
        public bool Equals(User other)
        {
            return _totalBalance.Equals(other._totalBalance) && AutoSleep == other.AutoSleep &&
                   SleepThreshold == other.SleepThreshold && string.Equals(Login, other.Login) &&
                   string.Equals(Password, other.Password) && string.Equals(Role, other.Role) &&
                   LicenseExpDate.Equals(other.LicenseExpDate) && string.Equals(Status, other.Status) &&
                   string.Equals(ConnectionID, other.ConnectionID) && string.Equals(Sleep, other.Sleep) &&
                   Alerts == other.Alerts && AllTrades == other.AllTrades && AllTradesPro == other.AllTradesPro &&
                   Chart == other.Chart && Counter == other.Counter && L2 == other.L2 && Logbook == other.Logbook &&
                   Trading == other.Trading && FastOrder == other.FastOrder && string.Equals(Email, other.Email) &&
                   SleepThreshold == other.SleepThreshold &&
                   ProfitControl == other.ProfitControl && ProfitLimit.Equals(other.ProfitLimit) &&
                   ProfitLossLimit.Equals(other.ProfitLossLimit) && ProfitFixed == other.ProfitFixed &&
                   IsUsa == other.IsUsa;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _totalBalance.GetHashCode();
                hashCode = (hashCode * 397) ^ AutoSleep.GetHashCode();
                hashCode = (hashCode * 397) ^ SleepThreshold.GetHashCode();
                hashCode = (hashCode * 397) ^ (Login != null ? Login.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Role != null ? Role.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LicenseExpDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConnectionID != null ? ConnectionID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Sleep != null ? Sleep.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Alerts.GetHashCode();
                hashCode = (hashCode * 397) ^ AllTrades.GetHashCode();
                hashCode = (hashCode * 397) ^ AllTradesPro.GetHashCode();
                hashCode = (hashCode * 397) ^ Chart.GetHashCode();
                hashCode = (hashCode * 397) ^ Counter.GetHashCode();
                hashCode = (hashCode * 397) ^ L2.GetHashCode();
                hashCode = (hashCode * 397) ^ Logbook.GetHashCode();
                hashCode = (hashCode * 397) ^ Trading.GetHashCode();
                hashCode = (hashCode * 397) ^ FastOrder.GetHashCode();
                hashCode = (hashCode * 397) ^ (Email != null ? Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SleepThreshold.GetHashCode();
                hashCode = (hashCode * 397) ^ ProfitControl.GetHashCode();
                hashCode = (hashCode * 397) ^ ProfitLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ ProfitLossLimit.GetHashCode();
                return hashCode;
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