using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BusinessLayer
{
    public sealed class UsersManager : IUsersManager
    {
        private readonly IUsersRepository usersRepository;
        private readonly IFriendsRepository friendsRepository;

        private readonly ConcurrentDictionary<int, User> LoggedUsers;

        public int UserCount => LoggedUsers.Count();

        public UsersManager(IUsersRepository usersRepository, IFriendsRepository friendsRepository)
        {
            this.usersRepository = usersRepository;
            this.friendsRepository = friendsRepository;
            LoggedUsers = new ConcurrentDictionary<int, User>();
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

            user = new User { Name = name, Username = username };
            var hashedPassword = User.HashPassword(password);
            user.Salt = hashedPassword.Salt;
            user.Password = hashedPassword.Password;

            usersRepository.Insert(user);

            LoggedUsers.TryAdd(user.Id, user);

            return user;
        }

        public void Logout(User user)
        {
            if (user == null)
                throw new Exception("Can't logout without logging in");

            if (!IsUserConnected(user.Username))
                throw new Exception("User is not logged in");

            if (!LoggedUsers.TryRemove(user.Id, out _))
                throw new Exception("Couldn't remove user");

            usersRepository.Delete(user);
        }

        public bool IsUserConnected(string username)
        {
            return !string.IsNullOrEmpty(username) && LoggedUsers.ToList().Exists(item => item.Value.Username == username);
        }

        public bool IsUserConnected(int userId)
        {
            return LoggedUsers.ToList().Exists(item => item.Value.Id == userId);
        }

        public bool DoesUserExist(string userId)
        {
            return usersRepository.SelectSpecificUser(userId) != null;
        }

        public bool DoesFriendExist(int userId1, int userId2)
        {
            return friendsRepository.DoesFriendExist(userId1, userId2);
        }

        public IEnumerable<Friend> GetFriendsOfUser(int userId, int friendsCount = 20)
        {
            return friendsRepository.Select(friendsCount, 
                "WHERE userId1 = @userId OR userId2 = @userId", 
                new SqlParameter[1] { new SqlParameter("@userId", userId) { SqlDbType = System.Data.SqlDbType.Int } });
        }

        public Friend AddFriend(int userId, int friendUserId)
        {
            var friend = new Friend() { DateAdded = DateTime.Now, UserId1 = userId, UserId2 = friendUserId, Status = FriendStatus.Waiting };

            friendsRepository.Insert(friend);

            return friend;
        }

        public Friend GetExistingFriend(int userId, int friendUserId)
        {
            return friendsRepository.GetSpecificFriend(userId, friendUserId);
        }

        public void ChangeFriendStatus(Friend friend, FriendStatus newStatus)
        {
            friend.Status = newStatus;
            friendsRepository.Update(friend);
        }

        public bool DoesUserExist(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
