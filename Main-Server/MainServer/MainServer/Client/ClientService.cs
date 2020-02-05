using System;
using System.Collections.Generic;
using DataLayer;
using System.ServiceModel;

namespace MainServer
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession, 
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class ClientService : IClientService
    {
        #region Fields

        private IClientDuplex DuplexChannel;

        #endregion

        #region Properties

        public User LoggedUser { get; private set; }

        #endregion

        #region Constructors

        public ClientService()
        {
            DuplexChannel = OperationContext.Current.GetCallbackChannel<IClientDuplex>();
        }

        #endregion

        #region Service Methods

        public User Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public User Signup(string username, string password, string name)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

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

        public List<GameSession> GetActiveGameSessions()
        {
            throw new NotImplementedException();
        }

        public string GetSessionIdentifier(int sessionId, string password)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
