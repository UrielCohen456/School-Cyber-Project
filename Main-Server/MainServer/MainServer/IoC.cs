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
            Container
                .RegisterType<IDb<User>, UsersDB>()
                .RegisterType<IUsersRepository, UsersRepository>()
                .RegisterType<IUsersManager, UsersManager>();
        }
    }
}
