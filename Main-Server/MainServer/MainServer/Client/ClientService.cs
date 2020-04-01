using BusinessLayer;
using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using Unity;

namespace MainServer
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class ClientService : IClientService, IDisposable
    {
        private static readonly ConcurrentDictionary<int, IClientDuplex> userCallbacks = new ConcurrentDictionary<int, IClientDuplex>();
        private static readonly UsersManager usersManager = IoC.Container.Resolve<UsersManager>();
        private static readonly MessagesManager messagesManager = IoC.Container.Resolve<MessagesManager>();

        private User LoggedUser { get; set; }
        private readonly IClientDuplex duplexChannel;

        public ClientService()
        {
            duplexChannel = OperationContext.Current.GetCallbackChannel<IClientDuplex>();
        }

        public void Dispose()
        {
            RemoveUser();
        }

        #region Private Methods

        private void AddUser(User user)
        {
            LoggedUser = user;
            userCallbacks.TryAdd(user.Id, duplexChannel);
        }
        
        private void RemoveUser()
        {
            if (LoggedUser == null)
                return;

            userCallbacks.TryRemove(LoggedUser.Id, out _);
            usersManager.Logout(LoggedUser);
            LoggedUser = null;
        }

        /// <summary>
        /// Checks wheter the user is logged in. If user is not logged in throws a FaultException of OperationFault
        /// </summary>
        /// <param name="operation">name of the operation this was called in</param>
        private void CheckIsUserAuthenticated()
        {
            if (LoggedUser == null)
                throw new Exception("You are not authenticated");
        }

        private void CheckDoesUserExist(int userId)
        {
            if (!usersManager.DoesUserExist(userId))
                throw new Exception("User requested doesn't exist");
        }

        private FaultException<OperationFault> CreateFault(string message, [CallerMemberName] string operation = null)
        {
            return new FaultException<OperationFault>(new OperationFault(message, operation));
        }

        private void NotifyUserFriendStatusChanged(int userId, Friend friend)
        {
            if (usersManager.IsUserConnected(userId))
                userCallbacks.ElementAt(userId).Value.FriendStatusChanged(friend);
        }

        #endregion

        #region Authentication Methods

        public User Login(string username, string password)
        {
            try
            {
                var user = usersManager.Login(username, password);
                AddUser(user);
                return user;
            }
            catch(Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        public User Signup(string name, string username, string password)
        {
            try
            {
                var user = usersManager.Signup(name, username, password);
                AddUser(user);
                return user;
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        public void Logout()
        {
            try
            {
                RemoveUser();
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        #endregion

        #region Friend Methods

        public List<Friend> GetFriends(int friendCount)
        {
            try
            {
                CheckIsUserAuthenticated();

                return usersManager.GetFriendsOfUser(LoggedUser.Id, friendCount).ToList();
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        public Friend AddFriend(int userId)
        {
            try
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                if (usersManager.DoesFriendExist(LoggedUser.Id, userId))
                    throw new Exception("Can't add a friend twice");

                var friend = usersManager.AddFriend(LoggedUser.Id, userId);

                // Notifying the user if he is connected someone added him
                NotifyUserFriendStatusChanged(userId, friend);

                return friend;
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        public void ChangeFriendStatus(int userId, FriendStatus status)
        {
            // TODO: Write code to test all the new functions in UsersManager and FriendsRepository
            try
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                if (!usersManager.DoesFriendExist(LoggedUser.Id, userId))
                    throw new Exception("No such friendship exists");

                var friend = usersManager.GetExistingFriend(LoggedUser.Id, userId);

                if (!friend.IsChangePossible(status, LoggedUser.Id))
                    throw new Exception("Friend status change is not possible");

                // Updating the database with the changed status
                usersManager.ChangeFriendStatus(friend, status);

                // Notifying the other user the friend status has changed
                var friendUserId = friend.UserId1 != LoggedUser.Id ? friend.UserId1 : friend.UserId2;
                NotifyUserFriendStatusChanged(friendUserId, friend);
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        public void SendMessage(int userId, string message)
        {
            try
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                //TODO: Send messages only to friends, add to db, if other user is online notify him

                //new MessagesDB().Insert(new Message() { Content = message, FromId = LoggedUser.Id, ToId = userId });
                //if (!usersManager.IsUserConnected(toUserId))
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        public List<Message> GetConversationWithUser(int userId)
        {
            try
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                //TODO: Check if friends with user and retreive the conversation if it exists
                
                return null;
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message);
            }
        }

        #endregion

        public List<GameSession> GetActiveGameSessions()
        {
            throw new NotImplementedException();
        }

    }
}
