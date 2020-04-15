using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class FriendStatusChangedEventArgs : EventArgs
    {
        public Friend Friend { get; set; }

        public FriendStatusChangedEventArgs(Friend friend)
        {
            Friend = friend;
        }
    }
}
