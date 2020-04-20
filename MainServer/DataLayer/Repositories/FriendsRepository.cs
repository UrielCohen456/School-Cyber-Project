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

        IEnumerable<Friend> GetFriendsOfUserByStatus(int userId, FriendStatus status, int friendsCount = 20);

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
            try
            {
                var friend = new Friend() { DateAdded = DateTime.Now, UserId1 = userId, UserId2 = friendUserId, Status = FriendStatus.Waiting };
                Db.Insert(friend);

                return friend;  
            }
            catch
            { return null; }
        }

        public IEnumerable<Friend> GetFriendsOfUserByStatus(int userId, FriendStatus status, int friendsCount = 20)
        {
            try
            {
                return Db.Select(friendsCount,
                   "WHERE ((UserId1 = @UserId) OR (UserId2 = @UserId)) AND Status = @Status",
                   new SqlParameter[2] 
                   { 
                       new SqlParameter("@UserId", userId) { SqlDbType = System.Data.SqlDbType.Int },
                       new SqlParameter("@Status", (byte)status) { SqlDbType = System.Data.SqlDbType.TinyInt }
                   });
            }
            catch { return null; }
        }

        public Friend GetSpecificFriend(int userId1, int userId2)
        {
            try
            {
                var userIdName1 = "UserId1";
                var userIdName2 = "UserId2";
                var sqlWhereString = $"WHERE ({userIdName1} = @{userIdName1} AND {userIdName2} = @{userIdName2}) " +
                    $"OR ({userIdName1} = @{userIdName2} AND {userIdName2} = @{userIdName1})";
                var sqlParameters = new SqlParameter[2];
                sqlParameters[0] = new SqlParameter($"@{userIdName1}", System.Data.SqlDbType.Int) { Value = userId1 };
                sqlParameters[1] = new SqlParameter($"@{userIdName2}", System.Data.SqlDbType.Int) { Value = userId2 };

                var user = Db.Select(1, sqlWhereString, sqlParameters).FirstOrDefault();

                return user;
            }
            catch { return null; }
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
