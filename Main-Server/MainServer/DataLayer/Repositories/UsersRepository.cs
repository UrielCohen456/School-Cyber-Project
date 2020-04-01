using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer
{
    public interface IUsersRepository : IDb<User>
    {
        User SelectSpecificUser(string username);
        
        User SelectSpecificUser(int userId);
    }

    public class UsersRepository : IUsersRepository
    {
        private readonly IDb<User> Db;

        public UsersRepository(IDb<User> db)
        {
            Db = db;
        }

        public void Delete(User entity)
        {
            Db.Delete(entity);
        }

        public void Insert(User entity)
        {
            Db.Insert(entity);
        }

        public IEnumerable<User> Select(int count = 20, string condition = null, SqlParameter[] sqlParameters = null)
        {
            return Db.Select(count, condition, sqlParameters);
        }

        public void Update(User entity)
        {
            Db.Update(entity);
        }

        public User SelectSpecificUser(string username)
        {
            var sqlWhereString = $"WHERE Username = @Username";
            var sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@Username", System.Data.SqlDbType.NVarChar) { Value = username };

            var user = Db.Select(1, sqlWhereString, sqlParameters).FirstOrDefault();

            return user;
        }

        public User SelectSpecificUser(int userId)
        {
            var sqlWhereString = $"WHERE Id = @Id";
            var sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = userId };

            var user = Db.Select(1, sqlWhereString, sqlParameters).FirstOrDefault();

            return user;
        }
    }
}
