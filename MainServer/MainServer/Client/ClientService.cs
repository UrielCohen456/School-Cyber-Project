using BusinessLayer;
using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Windows.Ink;
using Unity;

namespace MainServer
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
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

            var roomIdAndGameId = serverManager.GetUserRoomAndGameId(LoggedUser);
            
            // User is in a room
            if (roomIdAndGameId.Item1.HasValue)
                LeaveRoom(roomIdAndGameId.Item1.Value);
            // User is in a game
            else if (roomIdAndGameId.Item2.HasValue)
                LeaveGame(roomIdAndGameId.Item2.Value);

            serverManager.Logout(LoggedUser);
            userCallbacks.TryRemove(LoggedUser.Id, out _);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Removed user: {LoggedUser.Name}");
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

        private void NotifyUserFriendStatusChanged(int userId, Friend friend, User friendUser)
        {
            if (serverManager.IsUserConnected(userId))
            {
                var pair = userCallbacks.Where(p => p.Key == userId).First();
                var callback = pair.Value;
                callback.FriendStatusChanged(friend, friendUser);
            }
        }

        private void NotifyUsersOfRoomChange(Room room, RoomUpdate update)
        {
            // Getting all other users in the room beside the logged user
            var users = serverManager.Users.Where(user => user.Id != LoggedUser.Id);
            foreach (var user in users)
            {
                var callback = userCallbacks.First(pair => pair.Key == user.Id).Value;
                callback.RoomUpdated(room, update);
            }
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

                var users = serverManager.GetUsersByQuery(searchQuery, userCount).ToList();
                users.RemoveAll(u => u.Id == LoggedUser.Id);

                return users;
            });
        }

        #endregion

        #region Friend Methods

        public List<Friend> GetFriends(FriendStatus status, int friendCount)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                return serverManager.GetFriendsOfUserByStatus(LoggedUser.Id, status, friendCount).ToList();
            });
        }

        public Friend GetFriendIfExists(int userId)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                return serverManager.GetExistingFriend(LoggedUser.Id, userId);
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
                NotifyUserFriendStatusChanged(userId, friend, LoggedUser);

                return friend;
            });
        }

        public Friend ChangeFriendStatus(int userId, FriendStatus status)
        {
            // TODO: Write code to test all the new functions in UsersManager and FriendsRepository
            return Operation(() =>
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
                NotifyUserFriendStatusChanged(friendUserId, friend, LoggedUser);

                return friend;
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

        #region Room Methods

        public List<Room> GetAllRooms()
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                var rooms = serverManager.GetAllRooms().ToList();
                rooms.RemoveAll(r =>
                {
                    if (!r.IsRoomEmpty()) return false;

                    NotifyUsersOfRoomChange(r, RoomUpdate.Closed);

                    return true;
                });

                return rooms;
            });
        }

        public Room CreateRoom(RoomParameters roomParams)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                var room = serverManager.CreateRoom(LoggedUser, roomParams);

                NotifyUsersOfRoomChange(room, RoomUpdate.Created);

                return room;
            });
        }

        public Room JoinRoom(int roomId, string password)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();

                var room = serverManager.AddUserToRoom(LoggedUser, roomId, password);

                NotifyUsersOfRoomChange(room, RoomUpdate.UserJoined);

                return room;
            });
        }

        public void LeaveRoom(int roomId)
        {
            Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInRoom(LoggedUser.Id, roomId))
                    throw new Exception("Can't leave a room you are not in");

                var room = serverManager.RemoveUserFromRoom(LoggedUser, roomId);

                // Room has been deleted
                if (room == null)
                {
                    NotifyUsersOfRoomChange(room, RoomUpdate.Closed);
                    return;
                }

                NotifyUsersOfRoomChange(room, RoomUpdate.UserLeft);
            });
        }

        public void ChangeRoomState(int roomId, RoomState newState)
        {
            Operation(() =>
            {
                CheckIsUserAuthenticated();

                var room = serverManager.ChangeRoomState(roomId, newState);

                NotifyUsersOfRoomChange(room, RoomUpdate.StateChanged);
            });
        }

        #endregion

        #region Game Methods

        public int StartGame(int roomId, GameParameters parameters)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInRoom(LoggedUser.Id, roomId))
                    throw new Exception("Can't start a game from a room you are not in");

                var gameId = serverManager.StartGame(LoggedUser, roomId, parameters);
                var gameUsers = serverManager.GetGameUsers(gameId);

                foreach (var user in gameUsers)
                {
                    if (user.Id == LoggedUser.Id)
                        continue;
                    var callback = userCallbacks.First(pair => pair.Key == user.Id).Value;
                    callback.GameStarted(gameId);
                }

                return gameId;
            });
        }

        public void LeaveGame(int gameId)
        {
            Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInGame(LoggedUser, gameId))
                    throw new Exception("Can't leave a game you are not in");

                var gameFinished = serverManager.RemovePlayerFromGame(LoggedUser, gameId);
                if (gameFinished)
                    return;

                // Update other users if the game isnt finished yet
                var users = serverManager.GetGameUsers(gameId);
                foreach (var user in users)
                {
                    if (user.Id == LoggedUser.Id)
                        continue;

                    var callback = userCallbacks.First(pair => pair.Key == user.Id).Value;
                    callback.PlayerLeftTheGame(LoggedUser);
                }
            });
        }

        public List<User> GetAllPlayers(int gameId)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInGame(LoggedUser, gameId))
                    throw new Exception("Can't get information about a game you are not in");

                var players = serverManager.GetGameUsers(gameId);

                return players;
            });

        }

        public AnswerSubmitResult SubmitGuess(int gameId, string guess)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInGame(LoggedUser, gameId))
                    throw new Exception("Can't submit a guess to a game you are not in");

                var result = serverManager.SubmitGuess(LoggedUser, gameId, guess);
                var gameUsers = serverManager.GetGameUsers(gameId);

                foreach (var user in gameUsers)
                {
                    if (user.Id == LoggedUser.Id)
                        continue;

                    var callback = userCallbacks.First(pair => pair.Key == user.Id).Value;
                    if (result == AnswerSubmitResult.Wrong)
                        callback.PlayerSubmitedGuess(LoggedUser, guess);
                    else if (result == AnswerSubmitResult.Right)
                        callback.PlayerAnsweredCorrectly(LoggedUser, serverManager.GetPlayerGameData(LoggedUser, gameId));
                }

                return result;
            });
        }

        public void SubmitDraw(int gameId, MemoryStream strokes)
        {
            Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInGame(LoggedUser, gameId))
                    throw new Exception("Can't draw in a game you are not in");

                var strokeCollection = new StrokeCollection(strokes);
                strokes.Position = 0;

                serverManager.SubmitDraw(LoggedUser, gameId, strokeCollection);
                var gameUsers = serverManager.GetGameUsers(gameId);

                foreach (var user in gameUsers)
                {
                    if (user.Id == LoggedUser.Id)
                        continue;

                    var callback = userCallbacks.First(pair => pair.Key == user.Id).Value;
                    callback.BoardChanged(strokes);
                }
            });
        }

        public GameInformation GetGameInformation(int gameId)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInGame(LoggedUser, gameId))
                    throw new Exception("Can't get information about a game you are not in");

                var info = serverManager.GetGameInformation(LoggedUser, gameId);

                return info;
            });
        }
        
        public List<PlayerGameData> GetScores(int gameId)
        {
            return Operation(() =>
            {
                CheckIsUserAuthenticated();
                if (!serverManager.IsUserInGame(LoggedUser, gameId))
                    throw new Exception("Can't get scores of a game you are not in");

                var scores = new List<PlayerGameData>();

                var users = serverManager.GetGameUsers(gameId);
                foreach (var user in users)
                {
                    scores.Add(serverManager.GetPlayerGameData(user, gameId));
                }

                return scores;
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
    }
}
