using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Inside_MMA.Annotations;

namespace Inside_MMA.Models
{
    [XmlRoot(ElementName = "security")]
    public class SecurityMCT
    {
        [XmlElement(ElementName = "market")]
        public string Market { get; set; }
        [XmlElement(ElementName = "seccode")]
        public string Seccode { get; set; }
        [XmlElement(ElementName = "security_currency")]
        public string SecurityCurrency { get; set; }
        [XmlElement(ElementName = "go_rate")]
        public string GoRate { get; set; }
        [XmlElement(ElementName = "go_rate_long")]
        public string GoRateLong { get; set; }
        [XmlElement(ElementName = "go_rate_short")]
        public string GoRateShort { get; set; }
        [XmlElement(ElementName = "price")]
        public string Price { get; set; }
        [XmlElement(ElementName = "init_rate")]
        public string InitRate { get; set; }
        [XmlElement(ElementName = "cross_rate")]
        public string CrossRate { get; set; }
        [XmlElement(ElementName = "init_cross_rate")]
        public string InitCrossRate { get; set; }
        [XmlElement(ElementName = "open_balance")]
        public string OpenBalance { get; set; }
        [XmlElement(ElementName = "bought")]
        public string Bought { get; set; }
        [XmlElement(ElementName = "sold")]
        public string Sold { get; set; }
        [XmlElement(ElementName = "balance")]
        public int Balance { get; set; }
        [XmlElement(ElementName = "buying")]
        public string Buying { get; set; }
        [XmlElement(ElementName = "selling")]
        public string Selling { get; set; }
        [XmlElement(ElementName = "pos_cost")]
        public string PosCost { get; set; }
        [XmlElement(ElementName = "go_pos_fact")]
        public string GoPosFact { get; set; }
        [XmlElement(ElementName = "go_pos_plan")]
        public string GoPosPlan { get; set; }
        [XmlElement(ElementName = "tax")]
        public string Tax { get; set; }
        [XmlElement(ElementName = "pnl_income")]
        public string PnlIncome { get; set; }
        [XmlElement(ElementName = "pnl_intraday")]
        public string PnlIntraday { get; set; }
        [XmlElement(ElementName = "maxbuy")]
        public string Maxbuy { get; set; }
        [XmlElement(ElementName = "maxsell")]
        public string Maxsell { get; set; }
        [XmlElement(ElementName = "bought_average")]
        public string BoughtAverage { get; set; }
        [XmlElement(ElementName = "sold_average")]
        public string SoldAverage { get; set; }
        [XmlAttribute(AttributeName = "secid")]
        public string Secid { get; set; }
    }

    [XmlRoot(ElementName = "portfolio_mct")]
    public class PortfolioMCT : INotifyPropertyChanged
    {
        private string _portfolioCurrency;
        private string _capital;
        private string _utilizationFact;
        private string _utilizationPlan;
        private string _coverageFact;
        private string _coveragePlan;
        private string _openBalance;
        private string _tax;
        private string _pnlIncome;
        private string _pnlIntraday;
        private ObservableCollection<SecurityMCT> _security = new ObservableCollection<SecurityMCT>();
        private string _client;

        [XmlElement(ElementName = "portfolio_currency")]
        public string PortfolioCurrency
        {
            get { return _portfolioCurrency; }
            set
            {
                if (value == _portfolioCurrency) return;
                _portfolioCurrency = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "capital")]
        public string Capital
        {
            get { return _capital; }
            set
            {
                if (value == _capital) return;
                _capital = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "utilization_fact")]
        public string UtilizationFact
        {
            get { return _utilizationFact; }
            set
            {
                if (value == _utilizationFact) return;
                _utilizationFact = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "utilization_plan")]
        public string UtilizationPlan
        {
            get { return _utilizationPlan; }
            set
            {
                if (value == _utilizationPlan) return;
                _utilizationPlan = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "coverage_fact")]
        public string CoverageFact
        {
            get { return _coverageFact; }
            set
            {
                if (value == _coverageFact) return;
                _coverageFact = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "coverage_plan")]
        public string CoveragePlan
        {
            get { return _coveragePlan; }
            set
            {
                if (value == _coveragePlan) return;
                _coveragePlan = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "open_balance")]
        public string OpenBalance
        {
            get { return _openBalance; }
            set
            {
                if (value == _openBalance) return;
                _openBalance = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "tax")]
        public string Tax
        {
            get { return _tax; }
            set
            {
                if (value == _tax) return;
                _tax = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "pnl_income")]
        public string PnlIncome
        {
            get { return _pnlIncome; }
            set
            {
                if (value == _pnlIncome) return;
                _pnlIncome = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "pnl_intraday")]
        public string PnlIntraday
        {
            get { return _pnlIntraday; }
            set
            {
                if (value == _pnlIntraday) return;
                _pnlIntraday = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "security")]
        public ObservableCollection<SecurityMCT> Security
        {
            get { return _security; }
            set
            {
                if (Equals(value, _security)) return;
                _security = value;
                OnPropertyChanged();
            }
        }

        [XmlAttribute(AttributeName = "client")]
        public string Client
        {
            get { return _client; }
            set
            {
                if (value == _client) return;
                _client = value;
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