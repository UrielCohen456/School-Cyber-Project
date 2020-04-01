using DataLayer;
using System.Collections.Concurrent;

namespace BusinessLayer
{
    public sealed class MessagesManager
    {
        private readonly IDb<Message> db;

        public MessagesManager(IDb<Message> db)
        {
            this.db = db;       
        }




    }

}
