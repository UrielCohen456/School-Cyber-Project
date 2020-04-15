using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class NewMessageReceivedEventArgs : EventArgs
    {
        public User User { get; set; }
        public string Message { get; set; }

        public NewMessageReceivedEventArgs(User user, string message)
        {
            User = user;
            Message = message;
        }
    }
}