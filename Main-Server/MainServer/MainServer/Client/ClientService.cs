using BusinessLayer;
using DataLayer;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Unity;

namespace MainServer
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class ClientService : IClientService
    {
        private static readonly UsersManager UsersManager = IoC.Container.Resolve<UsersManager>();
        private User LoggedUser { get; set; }

        private readonly IClientDuplex duplexChannel;

        public ClientService()
        {
            duplexChannel = OperationContext.Current.GetCallbackChannel<IClientDuplex>();
        }

        #region Authentication Methods
        public User Login(string username, string password)
        {
            try
            {
                LoggedUser = UsersManager.Login(username, password);
                return LoggedUser;
            }
            catch(Exception e)
            {
                throw new FaultException<OperationFault>(new OperationFault(e.Message, nameof(Login)));
            }
        }

        public User Signup(string name, string username, string password)
        {
            try
            {
                LoggedUser = UsersManager.Signup(name, username, password);
                return LoggedUser;
            }
            catch (Exception e)
            {
                throw new FaultException<OperationFault>(new OperationFault(e.Message, nameof(Signup)));
            }
        }

        public void Logout()
        {
            try
            {
                LoggedUser = null;
                UsersManager.Logout(LoggedUser);
            }
            catch (Exception e)
            {
                throw new FaultException<OperationFault>(new OperationFault(e.Message, nameof(Logout)));
            }
        }

        #endregion

        #region Friend Methods
        public List<Friend> GetFriends()
        {
            throw new NotImplementedException();
        }

        public Friend AddFriend(int userId)
        {
            throw new NotImplementedException();
        }

        public void ChangeFriendStatus(int friendId, FriendStatus status)
        {
            throw new NotImplementedException();
        }
        public bool SendMessage(int toUserId, string message)
        {
            throw new NotImplementedException();
        }

        #endregion

        public List<GameSession> GetActiveGameSessions()
        {
            throw new NotImplementedException();
        }

        #region Private Methods

        public void Gal()
        {
            Console.WriteLine("Gal is gay");

            Console.WriteLine();




        }

        #endregion
    }
}
