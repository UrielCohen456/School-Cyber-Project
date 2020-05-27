using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class NewUserJoinedGameSessionEventArgs : EventArgs
    {
        public User User { get; set; }

        public NewUserJoinedGameSessionEventArgs(User user)
        {
            User = user;
        }
    }
}
