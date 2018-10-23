using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inside_MMA
{
    public class SecurityForTicks
    {
        public string Board { get; set; }
        public string Seccode { get; set; }
        public string Tradeno { get; set; }
        public int SubsCount { get; set; }
        public static string ReturnSecuritiesXml(List<SecurityForTicks> securities)
        {
            string result = string.Empty;
            foreach (var str in securities)
            {
                result += $"<security><board>{str.Board}</board><seccode>{str.Seccode}</seccode><tradeno>{str.Tradeno}</tradeno></security>";
            }
            return result;
        }
    }
    public static class ConnectorCommands
    {

        public static string SubUnsubCommand(string id, string to, string board, string seccode)
        {
            return $"<command id=\"{id}\"><{to}><security><board>{board}</board><seccode>{seccode}</seccode></security></{to}></command>";
        }

        public static string SubUnbubTics(string id, string board, string seccode, string tradeno, string filter)
        {
            return
                $"<command id =\"{id}\"><security><board>{board}</board><seccode>{seccode}</seccode><tradeno>{tradeno}</tradeno></security><filter>{filter}</filter></command>";
        }

        public static string SubscribeTicks(string securities)
        {
            return
                $"<command id=\"subscribe_ticks\">{securities}<filter>true</filter></command>";
        }

        public static string NewStopLoss(string board, string seccode, string client, string union, string buysell, string aPrice, string oPrice, string quantity, bool byMarket, bool useCredit, string guardTime = null)
        {
            var mktString = byMarket ? "<bymarket/>" : "";
            var creditString = useCredit ? "<usecredit/>" : "";
            var gtString = guardTime == null ? "" : $"<guardtime>{guardTime}</guardtime>";
            return
                $"<command id=\"newstoporder\"><security><board>{board}</board><seccode>{seccode}</seccode></security><client>{client}</client><union>{union}</union><buysell>{buysell}</buysell><stoploss><activationprice>{aPrice}</activationprice><orderprice>{oPrice}</orderprice>{mktString}<quantity>{quantity}</quantity>{creditString}{gtString}</stoploss></command>";

        }

        public static string PlaceMktOrder(string board, string seccode, string client, string union, int size, string buysell, bool usecredit)
        {
            var ucstring = usecredit ? "<usecredit/>" : "";
            return TXmlConnector.ConnectorSendCommand("<command id=\"neworder\"><security><board>" + board +
                                                      "</board><seccode>" + seccode +
                                                      "</seccode></security><client>" + client + "</client>" +
                                                      "<union>" + union + "</union>" +
                                                      "<quantity>" + size + "</quantity>" +
                                                      "<buysell>" + buysell + "</buysell>" +
                                                      "<bymarket/>" + ucstring + "</command>");
        }
    }
}
