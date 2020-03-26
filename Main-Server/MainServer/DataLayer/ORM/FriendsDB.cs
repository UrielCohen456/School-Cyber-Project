using DataLayer;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public class FriendsDB : Db<Friend>
    {
        public override string TableName => "FRIENDS";

        protected override Friend CreateEntity(SqlDataReader reader)
        {
            var friend = new Friend()
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId1 = Convert.ToInt32(reader["UserId1"]),
                UserId2 = Convert.ToInt32(reader["UserId2"]),
                Status = (FriendStatus)Enum.Parse(typeof(FriendStatus), reader["Status"].ToString()),
                DateAdded = Convert.ToDateTime(reader["DateAdded"])
            };

            return friend;
        }

        protected override string GetSQLDeleteString(Friend entity)
        {
            return $"DELETE FROM {TableName} WHERE ID = @{entity.Id}";
        }

        protected override string GetSQLInsertString()
        {
            return $"INSERT INTO {TableName} " +
                    $"(UserId1, UserId2, Status, DateAdded) " +
                    $"VALUES " +
                    $"(@UserId1, @UserId2, @Status, @DateAdded)";
        }

        protected override string GetSQLUpdateString(Friend entity)
        {
            return $"UPDATE {TableName} " +
                    $"SET UserId1 = @UserId1, UserId2 = @UserId2, Status = @Status, DateAdded = @DateAdded " +
                    $"WHERE Id = {entity.Id}";
        }

        protected override void SetSQLParameters(Friend entity)
        {
            sqlCommand.Parameters.Add("UserId1", SqlDbType.Int).Value = entity.UserId1;
            sqlCommand.Parameters.Add("UserId2", SqlDbType.Int).Value = entity.UserId2;
            sqlCommand.Parameters.Add("Status",  SqlDbType.Char).Value = (byte)entity.Status;
            sqlCommand.Parameters.Add("DateAdded", SqlDbType.DateTime).Value = entity.DateAdded;
        }
    }
}
