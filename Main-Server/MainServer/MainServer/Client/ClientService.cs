using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DataLayer;
using System.ServiceModel.Channels;

namespace MainServer
{
    public class ClientService : IClientService
    {
        public User LoggedUser => throw new NotImplementedException();


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

    }
}
