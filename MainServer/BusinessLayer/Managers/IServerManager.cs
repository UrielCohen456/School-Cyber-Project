using DataLayer;
using System.Collections.Generic;

namespace BusinessLayer
{
    public interface IServerManager
    {
        public int UserCount { get; }

        User Login(string username, string password);

        User Signup(string name, string username, string password);

        void Logout(User user);

        User GetUserById(int id);

        List<User> GetUsersByQuery(string searchQuery, int userCount);

        bool IsUserConnected(int id);

        bool IsUserConnected(string username);

        bool DoesUserExist(string userId);

        bool DoesUserExist(int userId);

        bool DoesFriendExist(int userId1, int userId2);

        IEnumerable<Friend> GetFriendsOfUserByStatus(int userId, FriendStatus status, int friendsCount = 20);

        Friend AddFriend(int userId, int friendUserId);

        Friend GetExistingFriend(int userId, int friendUserId);

        void ChangeFriendStatus(Friend friend, FriendStatus newStatus);

        Message SaveMessage(int fromId, int toId, string text);

        IEnumerable<Message> GetConversation(int userId1, int userId2, int messagesCount = 50);

        IEnumerable<Room> GetAllRooms();

        bool DoesRoomExist(int roomId);
        bool DoesRoomExist(string roomName);

        bool IsUserInRoom(int userId, int roomId);

        Room ChangeRoomState(int roomId, RoomState newState);

        Room CreateRoom(User creator, int maxPlayerCount, string roomName, string password);

        Room AddUserToRoom(User userToAdd, int roomId, string password);

        Room RemoveUserFromRoom(User userToRemove, int roomId);
    }
}