using System;
using DataLayer;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public class MessagesDB : Db<Message>
    {
        public override string TableName => "MESSAGES";

        protected override Message CreateEntity(SqlDataReader reader)
        {
            var friend = new Message()
            {
                Id = Convert.ToInt32(reader["Id"]),
                FromId = Convert.ToInt32(reader["FromId"]),
                ToId = Convert.ToInt32(reader["ToId"]),
                Content = reader["Content"].ToString()
            };

            return friend;
        }

        protected override string GetSQLDeleteString(Message entity)
        {
            return $"DELETE FROM {TableName} WHERE ID = @{entity.Id}";
        }

        protected override string GetSQLInsertString()
        {
            return $"INSERT INTO {TableName} " +
                    $"(FromId, ToId, Content) " +
                    $"VALUES " +
                    $"(@FromId, @ToId, @Content)";
        }

        protected override string GetSQLUpdateString(Message entity)
        {
            return $"UPDATE {TableName} " +
                    $"SET FromId = @FromId, ToId = @ToId, Content = @Content " +
                    $"WHERE Id = {entity.Id}";
        }

        protected override void SetSQLParameters(Message entity)
        {
            sqlCommand.Parameters.Add("FromId", SqlDbType.Int).Value = entity.FromId;
            sqlCommand.Parameters.Add("ToId", SqlDbType.Int).Value = entity.ToId;
            sqlCommand.Parameters.Add("Content", SqlDbType.NVarChar).Value = entity.Content;
        }
    }
}
