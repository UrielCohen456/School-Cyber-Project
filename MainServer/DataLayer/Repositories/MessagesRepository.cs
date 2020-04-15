using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer
{
    public interface IMessagesRepository
    {
        Message SaveMessage(int fromId, int toId, string text);

        IEnumerable<Message> GetConversation(int userId1, int userId2, int messagesCount = 50);
    }

    public class MessagesRepository : IMessagesRepository
    {
        private readonly IDb<Message> Db;

        public MessagesRepository(IDb<Message> db)
        {
            Db = db;
        }

        public IEnumerable<Message> GetConversation(int userId1, int userId2, int messagesCount = 50)
        {
            try
            {
                var userId1Name = "FromId";
                var userId2Name = "ToId";
                var sqlString = $"WHERE ({userId1Name} = @{userId1Name} AND {userId2Name} = @{userId2Name})" +
                    $" OR ({userId1Name} = @{userId2Name} AND {userId2Name} = @{userId1Name})";
                
                var param1 = new SqlParameter($"@{userId1Name}", userId1) { SqlDbType = System.Data.SqlDbType.Int };
                var param2 = new SqlParameter($"@{userId2Name}", userId2) { SqlDbType = System.Data.SqlDbType.Int };
                var parameters = new SqlParameter[2] { param1, param2 };

                return Db.Select(messagesCount, sqlString, parameters);
            }
            catch
            {
                return null;
            }
        }

        public Message SaveMessage(int fromId, int toId, string text)
        {
            try
            {
                var message = new Message { FromId = fromId, ToId = toId, Text = text };

                Db.Insert(message);

                return message;
            }
            catch
            {
                return null;
            }
        }
    }
}
