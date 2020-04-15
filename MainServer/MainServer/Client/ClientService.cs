using BusinessLayer;
using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static readonly IServerManager serverManager = IoC.Container.Resolve<ServerManager>();

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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Added user: {user.Name}");
        }

        private void RemoveUser()
        {
            if (LoggedUser == null)
                throw new Exception("User is not logged in");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Removed user: {LoggedUser.Name}");

            userCallbacks.TryRemove(LoggedUser.Id, out _);
            serverManager.Logout(LoggedUser);
            LoggedUser = null;
        }

        /// <summary>
        /// Checks wheter the user is logged in. If user is not logged in throws a FaultException of OperationFault
        /// </summary>
        private void CheckIsUserAuthenticated()
        {
            if (LoggedUser == null)
                throw new Exception("You are not authenticated");
        }

        private void CheckDoesUserExist(int userId)
        {
            if (!serverManager.DoesUserExist(userId))
                throw new Exception("User requested doesn't exist");
        }

        private FaultException<OperationFault> CreateFault(string message, [CallerMemberName] string operation = null)
        {
            return new FaultException<OperationFault>(new OperationFault(message, operation));
        }

        private void NotifyUserFriendStatusChanged(int userId, Friend friend)
        {
            if (serverManager.IsUserConnected(userId))
                userCallbacks.ElementAt(userId).Value.FriendStatusChanged(friend);
        }

        #endregion

        #region Authentication Methods

        public User Login(string username, string password)
        {
            return Operation(() =>
            {
                var user = serverManager.Login(username, password);
                AddUser(user);
                return user;
            });
        }

        public User Signup(string name, string username, string password)
        {
            return Operation(() =>
            {
                var user = serverManager.Signup(name, username, password);
                AddUser(user);
                return user;
            });
        }

        public void Logout()
        {
            Operation(() =>
            {
                RemoveUser();
            });
        }

        #endregion

        #region Users Methods

        public User GetUser(int id)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                return serverManager.GetUserById(id);
            });
        }

        public List<User> GetUsers(string searchQuery, int userCount)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                return serverManager.GetUsersByQuery(searchQuery, userCount).ToList();
            });
        }

        #endregion

        #region Friend Methods


        public List<Friend> GetFriends(int friendCount)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                return serverManager.GetFriendsOfUser(LoggedUser.Id, friendCount).ToList();
            });

        }

        public Friend AddFriend(int userId)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                if (serverManager.DoesFriendExist(LoggedUser.Id, userId))
                    throw new Exception("Can't add a friend twice");

                var friend = serverManager.AddFriend(LoggedUser.Id, userId);

                // Notifying the user if he is connected someone added him
                NotifyUserFriendStatusChanged(userId, friend);

                return friend;
            });
        }

        public void ChangeFriendStatus(int userId, FriendStatus status)
        {
            // TODO: Write code to test all the new functions in UsersManager and FriendsRepository
            Operation(() =>
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                if (!serverManager.DoesFriendExist(LoggedUser.Id, userId))
                    throw new Exception("No such friendship exists");

                var friend = serverManager.GetExistingFriend(LoggedUser.Id, userId);

                if (!friend.IsChangePossible(status, LoggedUser.Id))
                    throw new Exception("Friend status change is not possible");

                // Updating the database with the changed status
                serverManager.ChangeFriendStatus(friend, status);

                // Notifying the other user the friend status has changed
                var friendUserId = friend.UserId1 != LoggedUser.Id ? friend.UserId1 : friend.UserId2;
                NotifyUserFriendStatusChanged(friendUserId, friend);
            });
        }

        public Message SendMessage(int userId, string messageText)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                // Check if we are even friends
                if (!serverManager.DoesFriendExist(LoggedUser.Id, userId))
                    throw new Exception("Can only send messages to friends");

                var message = serverManager.SaveMessage(LoggedUser.Id, userId, messageText);

                if (serverManager.IsUserConnected(userId))
                {
                    var messagedUserCallback = userCallbacks.First(val => val.Key == userId).Value;
                    messagedUserCallback.NewMessageReceived(LoggedUser, message);
                }

                return message;
            });
        }

        public List<Message> GetConversationWithUser(int userId)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                CheckDoesUserExist(userId);

                if (serverManager.DoesFriendExist(LoggedUser.Id, userId))
                {
                    return serverManager.GetConversation(LoggedUser.Id, userId).ToList();
                }

                return null;
            });
        }

        #endregion

        private void Operation(Action func, [CallerMemberName] string operation = null)
        {
            try
            {
                func();
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message, operation);
            }
        }
        private T Operation<T>(Func<T> func, [CallerMemberName] string operation = null)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                throw CreateFault(e.Message, operation);
            }
        }

        public List<GameSession> GetActiveGameSessions()
        {
            throw new NotImplementedException();
        }
    }
}
