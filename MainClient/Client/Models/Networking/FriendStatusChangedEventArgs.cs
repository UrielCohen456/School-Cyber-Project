using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class FriendStatusChangedEventArgs : EventArgs
    {
        public Friend Friend { get; set; }

        public User FriendUser { get; set; }

        public FriendStatusChangedEventArgs(Friend friend, User friendUser)
        {
            Friend = friend;
            FriendUser = friendUser;
        }
    }
}
