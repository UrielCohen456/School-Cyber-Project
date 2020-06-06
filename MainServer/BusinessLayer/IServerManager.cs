using DataLayer;
using System;
using System.Collections.Generic;
using System.Windows.Ink;

namespace BusinessLayer
{
    public interface IServerManager
    {
        public int UserCount { get; }

        #region Authentication Methods

        public List<User> Users { get; }

        User Login(string username, string password);

        User Signup(string name, string username, string password);

        void Logout(User user);

        #endregion

        #region User Methods

        User GetUserById(int id);

        List<User> GetUsersByQuery(string searchQuery, int userCount);

        bool IsUserConnected(int id);

        bool IsUserConnected(string username);

        bool DoesUserExist(string userId);

        bool DoesUserExist(int userId);

        Tuple<int?, int?> GetUserRoomAndGameId(User user);

        void SaveUser(User user);

        #endregion

        #region Friend Methods

        bool DoesFriendExist(int userId1, int userId2);

        IEnumerable<Friend> GetFriendsOfUserByStatus(int userId, FriendStatus status, int friendsCount = 20);

        Friend AddFriend(int userId, int friendUserId);

        Friend GetExistingFriend(int userId, int friendUserId);

        void ChangeFriendStatus(Friend friend, FriendStatus newStatus);


        Message SaveMessage(int fromId, int toId, string text);

        IEnumerable<Message> GetConversation(int userId1, int userId2, int messagesCount = 50);

        #endregion

        #region Room Methods

        IEnumerable<Room> GetAllRooms();

        bool DoesRoomExist(int roomId);

        bool IsUserInRoom(int userId, int roomId);

        Room CreateRoom(User creator, RoomParameters roomParams);

        Room AddUserToRoom(User userToAdd, int roomId, string password);

        Room RemoveUserFromRoom(User userToRemove, int roomId);

        #endregion

        #region Game Methods

        bool IsUserInGame(User user, int gameId);

        int StartGame(User user, int roomId, GameParameters parameters);

        bool RemovePlayerFromGame(User player, int gameId);

        GameInformation GetGameInformation(User player, int gameId);

        AnswerSubmitResult SubmitGuess(User player, int gameId,  string guess);

        void SubmitDraw(User painter, int gameId, StrokeCollection strokes);

        List<User> GetGameUsers(int gameId);

        PlayerGameData GetPlayerGameData(User player, int gameId);

        List<RevealedLetter> GetGameRevealedLetters(User player, int gameId);

        #endregion
    }
}