using Client.MainServer;
using Client.Utility;

namespace Client.ViewModels
{
    public class FriendViewModel : BaseViewModel
    {
        /// <summary>
        /// The friend object representing the friendship
        /// </summary>
        public Friend FriendInfo { get; set; }

        /// <summary>
        /// The user object of the user that is friends with the logged user
        /// </summary>
        public User FriendUserInfo { get; set; }

        /// <summary>
        /// Public binding that indicates wheter this FriendViewModel is for an existing friend or a possible friend
        /// (To allow requesting to add friend)
        /// </summary>
        public bool FriendExists => FriendInfo != null;

        public FriendViewModel(Friend friendInfo, User friendUserInfo)
        {
            FriendInfo = friendInfo;
            FriendUserInfo = friendUserInfo;
        }
    }
}
