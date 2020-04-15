using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class NewMessageReceivedEventArgs : EventArgs
    {
        public User User { get; set; }
        public Message Message { get; set; }

        public NewMessageReceivedEventArgs(User user, Message message)
        {
            User = user;
            Message = message;
        }
    }
}