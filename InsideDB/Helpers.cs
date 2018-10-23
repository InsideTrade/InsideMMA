namespace InsideDB
{
    public static class Helpers
    {
        public static UserWindows GetUserWindows(User user)
        {
            return new UserWindows {
                Login = user.Login,
                Alerts = user.Alerts,
                AllTrades = user.AllTrades,
                AllTradesPro = user.AllTradesPro,
                Chart = user.Chart,
                Counter = user.Counter,
                L2 = user.L2,
                Logbook = user.Logbook,
                Trading = user.Trading,
                FastOrder = user.FastOrder
            };
        }
    }
}