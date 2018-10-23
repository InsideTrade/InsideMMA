using System.Collections.Generic;
using InsideDB;
using Inside_MMA.ViewModels;
using Inside_MMA.Views;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;

namespace Inside_MMA.DataHandlers
{
    public static class ReportManager
    {
        public static void Set()
        {
            MainWindowViewModel.Hub?.On("Balance", GetBalance);
        }
        public static void GetBalance(dynamic trades)
        {
            var tList = (List<Trade>) ((JArray) trades).ToObject(typeof(List<Trade>));
            MainWindowViewModel.BalanceReportViewModel.SetBalance(tList);
        }
    }
}