using Client.MainServer;
using System.Windows.Threading;

namespace Client
{
    public static class Globals
    {
        public static User LoggedUser { get; set; }

        public static Dispatcher UIDispatcher { get; set; }
    }
}
