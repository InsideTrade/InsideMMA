
using System.Threading;

namespace TransaqServer
{
    public class UserSleepTimer
    {
        public string Login;
        public Timer Timer;

        public UserSleepTimer(string login, Timer timer)
        {
            Login = login;
            Timer = timer;
        }
    }
}