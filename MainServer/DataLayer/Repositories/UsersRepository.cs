using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer
{
    public interface IUsersRepository
    {
        /// <summary>
        /// Attempts to find a specific user based on his username
        /// </summary>
        /// <param name="username">Username to search upon</param>
        /// <returns>The user if found else null</returns>
        User SelectSpecificUser(string username);

        /// <summary>
        /// Attempts to find a specific user based on his id
        /// </summary>
        /// <param name="userId">Id to search upon</param>
        /// <returns>The user if found else null</returns>
        User SelectSpecificUser(int userId);

        /// <summary>
        /// Attempts to remove a user from the database
        /// </summary>
        /// <param name="user">User that is already in the database</param>
        /// <returns>Whether the remove was a success</returns>
        bool RemoveUser(User user);

        /// <summary>
        /// Attempts to add a user the database and returns it with its Id
        /// </summary>
        /// <param name="name">public name</param>
        /// <param name="username">username for logging in</param>
        /// <param name="password">password for logging in</param>
        /// <returns>The user that was created</returns>
        User AddUser(string name, string username, string password);

        /// <summary>
        /// Attempts to search users based on a query
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="userCount"></param>
        /// <returns></returns>
        List<User> GetUsersByQuery(string searchQuery = "", int userCount = 20);
    }

    public class UsersRepository : IUsersRepository
    {
        private readonly IDb<User> Db;

        public UsersRepository(IDb<User> db)
        {
            Db = db;
        }

        public bool RemoveUser(User user)
        {
            try
            {
                Db.Delete(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public User AddUser(string name, string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    throw new Exception("Some fields were missing or invalid");
                var user = new User { Name = name, Username = username };
                var hashedPassword = User.HashPassword(password);
                user.Salt = hashedPassword.Salt;
                user.Password = hashedPassword.Password;
                Db.Insert(user);

                return user;
            }
            catch
            {
                return null;
            }
        }

        public User SelectSpecificUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return SelectSpecificUser(username, "Username", System.Data.SqlDbType.NVarChar);
        }

        public User SelectSpecificUser(int userId)
        {
            if (userId < 0)
                return null;

            return SelectSpecificUser(userId, "Id", System.Data.SqlDbType.Int);
        }

        public List<User> GetUsersByQuery(string searchQuery = "", int userCount = 20)
        {
            try
            {
                searchQuery = $"%{searchQuery?.Trim()}%";
                var sqlWhereString = $"WHERE Name LIKE @SearchQuery";
                var sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter($"@SearchQuery", System.Data.SqlDbType.NVarChar, 100) { Value = searchQuery };

                var users = Db.Select(userCount, sqlWhereString, sqlParameters);

                return users.ToList();
            }
            catch { return null; }
        }

        private User SelectSpecificUser(object value, string name, System.Data.SqlDbType dbType)
        {
            try
            {
                var sqlWhereString = $"WHERE {name} = @{name}";
                var sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter($"@{name}", dbType) { Value = value };

                var user = Db.Select(1, sqlWhereString, sqlParameters)?.FirstOrDefault();

                return user;
            }
            catch { return null; }
        }
    }
}
