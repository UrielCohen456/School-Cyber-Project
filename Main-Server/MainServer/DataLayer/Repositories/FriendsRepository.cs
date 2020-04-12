using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer
{
    public interface IFriendsRepository 
    {
        bool DoesFriendExist(int userId1, int userId2);

        Friend GetSpecificFriend(int userId, int friendUserId);

        Friend CreateFriend(int userId1, int userId2);

        IEnumerable<Friend> GetFriendsOfUser(int userId, int friendsCount = 20);

        void ChangeFriendStatus(Friend friend, FriendStatus status);
    }

    public class FriendsRepository : IFriendsRepository
    {
        private readonly IDb<Friend> Db;

        public FriendsRepository(IDb<Friend> db)
        {
            Db = db;
        }

        public Friend CreateFriend(int userId, int friendUserId)
        {
            var friend = new Friend() { DateAdded = DateTime.Now, UserId1 = userId, UserId2 = friendUserId, Status = FriendStatus.Waiting };

            Db.Insert(friend);

            return friend;
        }

        public IEnumerable<Friend> GetFriendsOfUser(int userId, int friendsCount = 20)
        {
            return Db.Select(friendsCount,
               "WHERE userId1 = @userId OR userId2 = @userId",
               new SqlParameter[1] { new SqlParameter("@userId", userId) { SqlDbType = System.Data.SqlDbType.Int } });
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
        
        public void ChangeFriendStatus(Friend friend, FriendStatus status)
        {
            friend.Status = status;
            Db.Update(friend);
        }
    }
}
