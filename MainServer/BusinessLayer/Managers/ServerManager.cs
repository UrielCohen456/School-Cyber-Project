﻿using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BusinessLayer
{
    public sealed class ServerManager : IServerManager
    {
        private readonly IUsersRepository usersRepository;
        private readonly IFriendsRepository friendsRepository;
        private readonly IMessagesRepository messagesRepository;

        private readonly ConcurrentDictionary<int, User> LoggedUsers;
        private readonly ConcurrentDictionary<int, Room> ActiveRooms;

        public int UserCount => LoggedUsers.Count();

        public ServerManager(IUsersRepository usersRepository, IFriendsRepository friendsRepository, IMessagesRepository messagesRepository)
        {
            this.usersRepository = usersRepository;
            this.friendsRepository = friendsRepository;
            this.messagesRepository = messagesRepository;
            LoggedUsers = new ConcurrentDictionary<int, User>();
            ActiveRooms = new ConcurrentDictionary<int, Room>();
        }
        
        public User Login(string username, string password)
        {
            if (IsUserConnected(username))
                throw new Exception("User is already logged in");

            var user = usersRepository.SelectSpecificUser(username);

            if (user == null)
                throw new Exception("User doesn't exist");

            if (!user.IsHashedPasswordCorrect(password))
                throw new Exception("Username or password is incorrect");

            LoggedUsers.TryAdd(user.Id, user);

            return user;
        }

        public User Signup(string name, string username, string password)
        {
            if (IsUserConnected(username))
                throw new Exception("User is already logged in");

            var user = usersRepository.SelectSpecificUser(username);
            if (user != null) // User exists
                throw new Exception("User already exists");

            user = usersRepository.AddUser(name, username, password);
            if (user == null) // Couldn't add him
                throw new Exception("Couldn't add user, something went wrong.");

            LoggedUsers.TryAdd(user.Id, user);

            return user;
        }

        public void Logout(User user)
        {
            if (user == null)
                throw new Exception("Can't logout without logging in");

            if (!IsUserConnected(user.Id))
                throw new Exception("User is not logged in");

            if (!LoggedUsers.TryRemove(user.Id, out _))
                throw new Exception("Couldn't remove user, something went wrong");
        }

        public User GetUserById(int id)
        {
            return usersRepository.SelectSpecificUser(id);
        }

        public List<User> GetUsersByQuery(string searchQuery, int userCount = 20)
        {
            return usersRepository.GetUsersByQuery(searchQuery, userCount);
        }

        public bool IsUserConnected(string username)
        {
            return !string.IsNullOrEmpty(username) && LoggedUsers.ToList().Exists(item => item.Value.Username == username);
        }

        public bool IsUserConnected(int userId)
        {
            return LoggedUsers.ToList().Exists(item => item.Value.Id == userId);
        }

        public bool DoesUserExist(string username)
        {
            return usersRepository.SelectSpecificUser(username) != null;
        }

        public bool DoesUserExist(int userId)
        {
            return usersRepository.SelectSpecificUser(userId) != null;
        }

        public bool DoesFriendExist(int userId1, int userId2)
        {
            return friendsRepository.DoesFriendExist(userId1, userId2);
        }

        public IEnumerable<Friend> GetFriendsOfUserByStatus(int userId, FriendStatus status, int friendsCount = 20)
        {
            return friendsRepository.GetFriendsOfUserByStatus(userId, status, friendsCount);
        }

        public Friend AddFriend(int userId, int friendUserId)
        {
            return friendsRepository.CreateFriend(userId, friendUserId);
        }

        public Friend GetExistingFriend(int userId, int friendUserId)
        {
            return friendsRepository.GetSpecificFriend(userId, friendUserId);
        }

        public void ChangeFriendStatus(Friend friend, FriendStatus newStatus)
        {
            friendsRepository.ChangeFriendStatus(friend, newStatus);
        }

        public Message SaveMessage(int fromId, int toId, string text)
        {
            return messagesRepository.SaveMessage(fromId, toId, text);
        }

        public IEnumerable<Message> GetConversation(int userId1, int userId2, int messagesCount = 50)
        {
            return messagesRepository.GetConversation(userId1, userId2, messagesCount);
        }

        public IEnumerable<Room> GetAllRooms()
        {
            return ActiveRooms.Values;
        }

        public bool DoesRoomExist(int roomId)
        {
            return ActiveRooms.ToList().Exists(item => item.Value.Data.Id == roomId);
        }

        public bool DoesRoomExist(string roomName)
        {
            return ActiveRooms.ToList().Exists(item => item.Value.Data.Name == roomName);
        }

        public bool IsUserInRoom(int userId, int roomId)
        {
            return ActiveRooms.ToList().Exists(
                roomIdPair => roomIdPair.Value.Data.Id == roomId &&
                roomIdPair.Value.Users.ToList().Exists(user => user.Value.Id == userId));
        }

        public Room ChangeRoomState(int roomId, RoomState newState)
        {
            if (!DoesRoomExist(roomId))
                throw new Exception("Room doesn't exist");

            ActiveRooms.TryGetValue(roomId, out var room);

            room.Data.State = newState;

            return room;
        }

        public Room CreateRoom(User creator, int maxPlayerCount, string roomName, string password)
        {
            if (DoesRoomExist(roomName))
                throw new Exception("Room with the same name already exists");
            
            var roomId = ActiveRooms.IsEmpty ? 1 : ActiveRooms.Count + 1;
            var roomData = new RoomData {
                Id = roomId, Name = roomName, MaxPlayersCount = maxPlayerCount,
                Password = password, State = RoomState.Open };

            var room = new Room(creator, roomData, password);

            ActiveRooms.TryAdd(roomId, room);

            return room;
        }

        public Room AddUserToRoom(User userToAdd, int roomId, string password)
        {
            if (!DoesRoomExist(roomId))
                throw new Exception("Room doesn't exist");

            ActiveRooms.TryGetValue(roomId, out var room);

            room.AddUser(userToAdd, password);

            return room;
        }

        public Room RemoveUserFromRoom(User userToRemove, int roomId)
        {
            if (!DoesRoomExist(roomId))
                throw new Exception("Room doesn't exist");

            ActiveRooms.TryGetValue(roomId, out var room);

            var shouldDeleteRoom = room.RemoveUser(userToRemove);
            if (!shouldDeleteRoom)
                return room;
        
            ActiveRooms.TryRemove(roomId, out var _);
            return null;
        }
    }
}
