using System.Windows;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;

namespace Inside_MMA
{
    public static class ClientSelector
    {
        public static string SelectClient(string board)
        {
            var clients = MainWindowViewModel.ClientsViewModel
                .Clients;
            Client client = null;
            if (board == "TQBR" ||
                board == "EQOB" ||
                board == "EQRP" ||
                board == "TQIF" ||
                board == "TQDE" ||
                board == "SPFEQ")
            {

                client =
                    clients.Find(
                        cl => cl.Market == "ММВБ");
            }
            if (board == "FUT" ||
                board == "OPT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "FORTS");
            }
            if (board == "MCT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "MMA");
            }
            if (board == "CETS")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "ETS");
            }
            if (client == null)
            {
                return "-";
            }
            ClientInfo.Id = client.Id;
            ClientInfo.Union = client.Union;
            return client.Id;
        }

        public static string[] GetClient(string board)
        {
            var clients = MainWindowViewModel.ClientsViewModel
                .Clients;
            Client client = null;
            if (board == "TQBR" ||
                board == "EQOB" ||
                board == "EQRP" ||
                board == "TQIF" ||
                board == "TQDE" ||
                board == "SPFEQ")
            {

                client =
                    clients.Find(
                        cl => cl.Market == "ММВБ");
            }
            if (board == "FUT" ||
                board == "OPT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "FORTS");
            }
            if (board == "MCT")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "MMA");
            }
            if (board == "CETS")
            {
                client =
                    clients.Find(
                        cl => cl.Market == "ETS");
            }
            if (client == null)
            {
                return null;
            }
            return new []{client.Id, client.Union};
        }
    }
}