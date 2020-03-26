using DataLayer.Models;
using DataLayer.Repositories;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DataLayer.Managers
{
    public sealed class UsersManager : IUsersManager
    {
        private readonly IUsersRepository repository;

        private readonly ConcurrentDictionary<int, User> LoggedUsers;

        public int UserCount => LoggedUsers.Count();

        public UsersManager(IUsersRepository repository)
        {
            this.repository = repository;
            LoggedUsers = new ConcurrentDictionary<int, User>();
        }
        public User Login(string username, string password)
        {
            if (IsUserConnected(username))
                throw new Exception("User is already logged in");

            var user = repository.SelectSpecificUser(username);

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

            var user = repository.SelectSpecificUser(username);
            if (user != null) // User exists
                throw new Exception("User already exists");

            user = new User { Name = name, Username = username };
            var hashedPassword = User.HashPassword(password);
            user.Salt = hashedPassword.Salt;
            user.Password = hashedPassword.Password;

            repository.Insert(user);

            LoggedUsers.TryAdd(user.Id, user);

            return user;
        }

        public void Logout(User user)
        {
            if (!IsUserConnected(user.Username))
                throw new Exception("User is not logged in");

            if (!LoggedUsers.TryRemove(user.Id, out _))
                throw new Exception("Couldn't remove user");

            repository.Delete(user);
        }

        public bool IsUserConnected(string username)
        {
            return !string.IsNullOrEmpty(username) && LoggedUsers.ToList().Exists(item => item.Value.Username == username);
        }
    }
}
