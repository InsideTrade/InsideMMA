using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using InsideDB;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Linq;
using TransaqServer;
using Timer = System.Threading.Timer;

namespace TransaqServer
{

    public class Program
    {

        static void Main(string[] args)
        {
            var url = @"http://194.87.232.14:999";
#if DEBUG
            url = @"http://localhost:8080";
#endif
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine($"Server running at {url}");
                using (var db = new InsideDBEntities())
                {
                    foreach (var user in db.Users)
                    {
                        user.Status = "offline";
                        user.ConnectionID = "-";
                        user.Sleep = "-";
                    }
                    db.SaveChanges();
                }
                while (true)
                {
                    if (Console.ReadLine() == "init")
                        CreateAdmin();
                }
            }
        }

        private static void CreateAdmin()
        {
            using (var db = new InsideDBEntities())
            {
                db.Users.Add(new User
                {
                    Login = "admin",
                    Password = PassHashing.GetPasswordHashWithSalt("admin"),
                    Role = "admin",
                    LicenseExpDate = DateTime.Parse("01.01.2050"),
                    
                });
                db.SaveChanges();
            }
        }
        
    }


    [HubName("TransaqHub")]
    public class TransaqHub : Hub
    {
        public TransaqHub()
        {
            
        }

        public static List<string> Admins = new List<string>();
        public static List<UserSleepTimer> Timers = new List<UserSleepTimer>();
        public void SelectWindows(UserWindows userWindows)
        {
            using (var db = new InsideDBEntities())
            {
                var selectedUser = db.Users.Find(userWindows.Login);
                selectedUser.Alerts = userWindows.Alerts;
                selectedUser.AllTrades = userWindows.AllTrades;
                selectedUser.AllTradesPro = userWindows.AllTradesPro;
                selectedUser.Chart = userWindows.Chart;
                selectedUser.Counter = userWindows.Counter;
                selectedUser.L2 = userWindows.L2;
                selectedUser.Logbook = userWindows.Logbook;
                selectedUser.Trading = userWindows.Trading;
                selectedUser.FastOrder = userWindows.FastOrder;
                db.SaveChanges();
                Clients.Client(selectedUser.ConnectionID).SelectWindows(userWindows);
            }
            GetUsers();
        }
        public void CheckCredentials(string login, string password)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user == null || !PassHashing.CheckPasssword(user.Password, password))
                    Clients.Caller.ServerReply("notFound");
                else if (user.LicenseExpDate < DateTime.Today)
                    Clients.Caller.ServerReply("licenseExpired");
                else if (user.Status == "online")
                    Clients.Caller.ServerReply("online");
                else if (user.LicenseExpDate > DateTime.Today || (user.Role == "demo" && user.LicenseExpDate == null))
                {
                    if (user.Role == "demo" && user.LicenseExpDate == null)
                        user.LicenseExpDate = DateTime.Today.AddDays(7);
                    Clients.Caller.ServerReply(user);
                    user.ConnectionID = Context.ConnectionId;
                    user.Status = "online";
                    db.SaveChanges();
                    if (user.Role == "admin")
                        Admins.Add(Context.ConnectionId);
                    GetUsers();
                }
            }
        }
        public void GetUsers()
        {
            using (var db = new InsideDBEntities())
            {
                var users = db.Users.ToList();
                Clients.Clients(Admins).UserList(users);
            }
        }
        public void AddUser(string login, string password, string role, string license, string email)
        {
            using (var db = new InsideDBEntities())
            {
                var user = new User
                {
                    Login = login,
                    Password = PassHashing.GetPasswordHashWithSalt(password),
                    Role = role,
                    Email = email
                };
                if (role != "demo")
                    user.LicenseExpDate = DateTime.Parse(license);
                db.Users.Add(user);
                db.SaveChanges();
            }
            GetUsers();
        }
        public void EditUser(string login, string password, string role, string license, string email)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                //keep old pass if parameter is empty
                if (password != "")
                    user.Password = PassHashing.GetPasswordHashWithSalt(password);
                user.Role = role;
                user.Email = email;
                user.LicenseExpDate = DateTime.Parse(license);
                db.SaveChanges();
            }
            GetUsers();
        }
        public void DeleteUser(string login)
        {
            using (var db = new InsideDBEntities())
            {
                db.Users.Remove(db.Users.Find(login));
                db.SaveChanges();
            }
            GetUsers();
        }
        public void SleepUser(string login, string time)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                user.Sleep = time;
                db.SaveChanges();
                Clients.Client(user.ConnectionID).Sleep();
                Timers.RemoveAll(x => x.Login == login);
                Timers.Add(new UserSleepTimer(login,
                    new Timer(TimerCallback, login, int.Parse(time) * 60000, int.MaxValue)));
            }
            GetUsers();
        }

        private void TimerCallback(object login)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                user.Sleep = "-";
                db.SaveChanges();
                Clients.Client(user.ConnectionID).Unsleep();
                Timers.Find(x => x.Login == login.ToString()).Timer.Dispose();
                Timers.RemoveAll(t => t.Login == login.ToString());
            }
            GetUsers();
        }

        public void Unsleep(string login)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                user.Sleep = "-";
                db.SaveChanges();
                Clients.Client(user.ConnectionID).Unsleep();
                Timers.Find(x => x.Login == login.ToString()).Timer.Dispose();
                Timers.RemoveAll(t => t.Login == login.ToString());
            }
            GetUsers();
        }

        public void Disconnect(string connectionId, string message)
        {
            Clients.Client(connectionId).Disconnect(message);
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.First(u => u.ConnectionID == connectionId);
                user.Status = "offline";
                user.ConnectionID = "-";
                db.SaveChanges();
            }
            Admins.RemoveAll(x => x == connectionId);
            GetUsers();
        }

        public void Broadcast(string message)
        {
            Clients.AllExcept(Context.ConnectionId).DisplayMessage(message);
        }

        public void SendMessage(string connectionId, string message)
        {
            Clients.Client(connectionId).DisplayMessage(message);
        }

        public void GetLicense()
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.First(u => u.ConnectionID == Context.ConnectionId);
                Clients.Caller.License(user.LicenseExpDate);
            }
        }

        public void NewTrades(string login, List<Trade> trades)
        {
            using (var db = new InsideDBEntities())
            {
                var tradesDb = db.Trades;
                foreach (var t in trades)
                {
                    if (tradesDb.Find(t.Tradeno) == null)
                        tradesDb.Add(t);
                }
                db.SaveChanges();
            }
        }

        public void RealtimeBalance(string login, double totalBalance)
        {
            Clients.Clients(Admins).UpdateUserBalance(new [] {login, totalBalance.ToString("F2")});
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                //risk control
                if (user.AutoSleep && totalBalance <= user.SleepThreshold)
                    ExecAutoSleep(user.Login);
                //profit control - fix profit and exec protocol
                if (user.ProfitControl && !user.ProfitFixed && totalBalance >= user.ProfitLimit)
                {
                    FixProfit(login, totalBalance);
                    ExecProfitControl(login);
                }
                //if profits keep rising - adjust profitlimit and profitloss limit
                if (user.ProfitControl && user.ProfitFixed && totalBalance >= user.ProfitLimit * 1.01)
                    AdjustProfitControl(login, totalBalance);
                //if profit control is active and losses reach a limit - exec protocol
                if (user.ProfitControl && user.ProfitFixed && user.TotalBalance < user.ProfitLossLimit)
                {
                    ExecProfitControl(login);
                    user.ProfitControl = false;
                    user.ProfitFixed = false;
                    db.SaveChanges();
                    Clients.Clients(Admins).UpdateUser(user);
                }
            }
        }

        private void ExecProfitControl(string login)
        {
            CancelOrders(login);
            CancelStoporders(login);
            CloseBalance(login);
        }

        private void AdjustProfitControl(string login, double totalBalance)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                user.ProfitLimit = totalBalance;
                user.ProfitLossLimit = totalBalance * 0.8;
                db.SaveChanges();
                Clients.Clients(Admins).UpdateUser(user);
            }
        }

        private void FixProfit(string login, double totalBalance)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                user.ProfitFixed = true;
                user.ProfitLossLimit = totalBalance * 0.8;
                db.SaveChanges();
                Clients.Clients(Admins).UpdateUser(user);
            }
        }

        public async void RequestBalance(string login, DateTime from, DateTime to)
        {
            using (var db = new InsideDBEntities())
            {
                var tradesDb = db.Trades.Where(t => t.Login == login && t.Time >= from && t.Time <= to);
                double balance = 0;
                await tradesDb.ForEachAsync(trade =>
                    balance += trade.Price * trade.Quantity * trade.Lotsize * (trade.Buysell == "S" ? 1 : -1)
                );
                Clients.Caller.Balance(balance);
            }
        }

        public void CancelOrders(string login)
        {
            using (var db = new InsideDBEntities())
            {
                Clients.Client(db.Users.Find(login)?.ConnectionID).CancelOrders();
            }
        }

        public void CancelStoporders(string login)
        {
            using (var db = new InsideDBEntities())
            {
                Clients.Client(db.Users.Find(login)?.ConnectionID).CancelStoporders();
            }
        }

        public void CloseBalance(string login)
        {
            using (var db = new InsideDBEntities())
            {
                Clients.Client(db.Users.Find(login)?.ConnectionID).CloseBalance();
            }
        }

        public void EditAutoSleep(string login, bool autoSleep)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                if (user != null) user.AutoSleep = autoSleep;
                db.SaveChanges();
            }
        }
        public void EditSleepThreshold(string login, int sleepThreshold)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                if (user != null) user.SleepThreshold = sleepThreshold;
                db.SaveChanges();
            }
            GetUsers();
        }

        public void EditProfitControl(string login, bool profitControl)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                if (user != null) user.ProfitControl = profitControl;
                db.SaveChanges();
                Clients.Clients(Admins).UpdateUser(user);
            }
        }
        public void EditProfitLimit(string login, double profitLimit)
        {
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                if (user != null) user.ProfitLimit = profitLimit;
                db.SaveChanges();
                Clients.Clients(Admins).UpdateUser(user);
            }
        }
        public void ExecAutoSleep(string login)
        {
            CancelOrders(login);
            CancelStoporders(login);
            CloseBalance(login);
            SleepUser(login, (12*60).ToString());
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.Find(login);
                user.AutoSleep = false;
                db.SaveChanges();
            }
            GetUsers();
        }
        public override Task OnConnected()
        {
            Console.WriteLine("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
            using (var db = new InsideDBEntities())
            {
                var user = db.Users.First(u => u.ConnectionID == Context.ConnectionId);
                user.Status = "offline";
                user.ConnectionID = "-";
                db.SaveChanges();
            }
            Admins.Remove(Context.ConnectionId);
            GetUsers();
            return base.OnDisconnected(true);
        }
    }
}
