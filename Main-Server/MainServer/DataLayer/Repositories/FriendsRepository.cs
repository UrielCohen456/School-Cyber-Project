using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer
{
    public interface IFriendsRepository : IDb<Friend>
    {
        bool DoesFriendExist(int userId1, int userId2);

        Friend GetSpecificFriend(int userId1, int userId2);
    }

    public class FriendsRepository : IFriendsRepository
    {
        private readonly IDb<Friend> Db;

        public FriendsRepository(IDb<Friend> db)
        {
            Db = db;
        }

        public void Delete(Friend entity)
        {
            Db.Delete(entity);
        }

        public void Insert(Friend entity)
        {
            Db.Insert(entity);
        }

        public IEnumerable<Friend> Select(int count = 20, string condition = null, SqlParameter[] sqlParameters = null)
        {
            return Db.Select(count, condition, sqlParameters);
        }

        public void Update(Friend entity)
        {
            Db.Update(entity);
        }

        public Friend GetSpecificFriend(int userId1, int userId2)
        {
            var sqlWhereString = $"WHERE (userId1 = @userId1 AND userId2 = @userId2) OR (userId1 = @userId2 AND userId2 = @userId1)";
            var sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@userId1", System.Data.SqlDbType.Int) { Value = userId1 };
            sqlParameters[1] = new SqlParameter("@userId2", System.Data.SqlDbType.Int) { Value = userId2 };

            var user = Db.Select(1, sqlWhereString, sqlParameters).FirstOrDefault();

            return user;
        }
        
        public bool DoesFriendExist(int userId1, int userId2)
        {
            return GetSpecificFriend(userId1, userId2) != null;
        }



    }
}
