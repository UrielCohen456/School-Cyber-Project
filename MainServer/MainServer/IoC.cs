using BusinessLayer;
using DataLayer;
using Unity;

namespace MainServer
{
    public static class IoC
    {
        public static UnityContainer Container { get; private set; }

        static IoC()
        {
            Container = new UnityContainer();
            Configure();
        }

        private static void Configure()
        {
            // Setting up the container
            Container
                .RegisterType<IDb<User>, UsersDB>()
                .RegisterType<IUsersRepository, UsersRepository>()
                .RegisterType<IDb<Friend>, FriendsDB>()
                .RegisterType<IFriendsRepository, FriendsRepository>()
                .RegisterType<IDb<Message>, MessagesDB>()
                .RegisterType<IMessagesRepository, MessagesRepository>()
                .RegisterType<IServerManager, ServerManager>();
        }
    }
}
