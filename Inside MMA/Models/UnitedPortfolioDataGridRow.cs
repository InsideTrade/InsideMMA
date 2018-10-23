namespace Inside_MMA.Models
{
    public class UnitedPortfolioDataGridRow
    {
        public string Union { get; set; }
        public string Client { get; set; }
        public double OpenEquity { get; set; }
        public double Equity { get; set; }
        public double ChrgoffIr { get; set; }
        public double InitReq { get; set; }
        public double ChrgoffMr { get; set; }
        public double MaintReq { get; set; }
        public double RegEquity { get; set; }
        public double RegIr { get; set; }
        public double RegMr { get; set; }
        public double Vm { get; set; }
        public double Finres { get; set; }
        public double Go { get; set; }
    }

    public class MoneyDataGridRow
    {
        public string Name { get; set; }
        public double OpenBalance { get; set; }
        public double Bought { get; set; }
        public double Sold { get; set; }
        public double Settled { get; set; }
        public double Balance { get; set; }
        public double Tax { get; set; }
    }

    public class ValuePartDataGridRow
    {
        public string Register { get; set; }
        public double OpenBalance { get; set; }
        public double Bought { get; set; }
        public double Sold { get; set; }
        public double Settled { get; set; }
        public double Balance { get; set; }
    }

    public class AssetDataGridRow
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double SetoffRate { get; set; }
        public double InitReq { get; set; }
        public double MaintReq { get; set; }
    }

    public class UnitedPortfolioSecurityDataGridRow
    {
        public string Seccode { get; set; }
        public int OpenBalance { get; set; }
        public int Balance { get; set; }
        public int Market { get; set; }
        public double Price { get; set; }
        public int Bought { get; set; }
        public int Sold { get; set; }
        public int Buying { get; set; }
        public int Selling { get; set; }
        public double Equity { get; set; }
        public double RegEquity { get; set; }
        public double RiskrateLong { get; set; }
        public double RiskrateShort { get; set; }
        public double ReserateLong { get; set; }
        public double ReserateShort { get; set; }
        public double Pl { get; set; }
        public double PnlIncome { get; set; }
        public double PnlIntraday { get; set; }
        public int Maxbuy { get; set; }
        public int Maxsell { get; set; }
        public int Secid { get; set; }
    }
}
