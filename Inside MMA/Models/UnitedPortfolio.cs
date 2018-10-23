using System.Collections.Generic;
using System.Xml.Serialization;

namespace Inside_MMA.Models
{
    [XmlRoot(ElementName = "united_portfolio")]
    public class UnitedPortfolio
    {
        [XmlAttribute("union")]
        public string Union;

        [XmlAttribute("client")]
        public string Client;

        [XmlElement(ElementName = "open_equity")]
        public double OpenEquity;

        [XmlElement(ElementName = "equity")]
        public double Equity;

        [XmlElement(ElementName = "chrgoff_ir")]
        public double ChrgoffIr;

        [XmlElement(ElementName = "init_req")]
        public double InitReq;

        [XmlElement(ElementName = "chrgoff_mr")]
        public double ChrgoffMr;

        [XmlElement(ElementName = "maint_req")]
        public double MaintReq;

        [XmlElement(ElementName = "reg_equity")]
        public double RegEquity;

        [XmlElement(ElementName = "reg_ir")]
        public double RegIr;

        [XmlElement(ElementName = "reg_mr")]
        public double RegMr;

        [XmlElement(ElementName = "vm")]
        public double Vm;

        [XmlElement(ElementName = "finres")]
        public double Finres;

        [XmlElement(ElementName = "go")]
        public double Go;

        [XmlElement(ElementName = "money")]
        public List<Money> Money;

        [XmlElement(ElementName = "asset")]
        public List<Asset> Assets;
    }

    [XmlRoot(ElementName = "money")]
    public class Money
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "open_balance")]
        public double OpenBalance;

        [XmlElement(ElementName = "bought")]
        public double Bought;

        [XmlElement(ElementName = "sold")]
        public double Sold;

        [XmlElement(ElementName = "settled")]
        public double Settled;

        [XmlElement(ElementName = "balance")]
        public double Balance;

        [XmlElement(ElementName = "tax")]
        public double Tax;

        [XmlElement(ElementName = "value_part")]
        public List<ValuePart> ValueParts;
    }

    [XmlRoot(ElementName = "value_part")]
    public class ValuePart
    {
        [XmlAttribute("register")]
        public string Register;

        [XmlElement(ElementName = "open_balance")]
        public double OpenBalance;

        [XmlElement(ElementName = "bought")]
        public double Bought;

        [XmlElement(ElementName = "sold")]
        public double Sold;

        [XmlElement(ElementName = "settled")]
        public double Settled;

        [XmlElement(ElementName = "balance")]
        public double Balance;
    }

    [XmlRoot(ElementName = "asset")]
    public class Asset
    {
        [XmlAttribute("code")]
        public string Code;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "setoff_rate")]
        public double SetoffRate;

        [XmlElement(ElementName = "init_req")]
        public double InitReq;

        [XmlElement(ElementName = "maint_req")]
        public double MaintReq;

        [XmlElement(ElementName = "security")]
        public List<UnitedPortfolioSecurity> Securities;
    }

    [XmlRoot(ElementName = "security")]
    public class UnitedPortfolioSecurity
    {
        [XmlAttribute("secid")]
        public int Secid;

        [XmlElement(ElementName = "market")]
        public int Market;

        [XmlElement(ElementName = "seccode")]
        public string Seccode;

        [XmlElement(ElementName = "price")]
        public double Price;

        [XmlElement(ElementName = "open_balance")]
        public int OpenBalance;

        [XmlElement(ElementName = "bought")]
        public int Bought;

        [XmlElement(ElementName = "sold")]
        public int Sold;

        [XmlElement(ElementName = "balance")]
        public int Balance;

        [XmlElement(ElementName = "buying")]
        public int Buying;

        [XmlElement(ElementName = "selling")]
        public int Selling;

        [XmlElement(ElementName = "equity")]
        public double Equity;

        [XmlElement(ElementName = "reg_equity")]
        public double RegEquity;

        [XmlElement(ElementName = "riskrate_long")]
        public double RiskrateLong;

        [XmlElement(ElementName = "riskrate_short")]
        public double RiskrateShort;

        [XmlElement(ElementName = "reserate_long")]
        public double ReserateLong;

        [XmlElement(ElementName = "reserate_short")]
        public double ReserateShort;

        [XmlElement(ElementName = "pl")]
        public double Pl;

        [XmlElement(ElementName = "pnl_income")]
        public double PnlIncome;

        [XmlElement(ElementName = "pnl_intraday")]
        public double PnlIntraday;

        [XmlElement(ElementName = "maxbuy")]
        public int Maxbuy;

        [XmlElement(ElementName = "maxsell")]
        public int Maxsell;

        [XmlElement(ElementName = "value_part")]
        public List<ValuePart> ValueParts;
    }
}
