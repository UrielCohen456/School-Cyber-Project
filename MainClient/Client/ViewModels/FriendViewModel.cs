using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Client.ViewModels
{
    public class FriendViewModel : BaseViewModel
    {

        #region Fields

        /// <summary>
        /// The friend object representing the friendship
        /// </summary>
        private Friend friendInfo;

        /// <summary>
        /// The user object of the user that is friends with the logged user
        /// </summary>
        private User friendUserInfo;

        #endregion

        #region Properties
        
        public Friend FriendInfo
        {
            get => friendInfo;
            set
            {
                friendInfo = value;
                OnPropertyChanged(string.Empty);
            }
        }

        public User FriendUserInfo
        {
            get => friendUserInfo;
            set
            { 
                friendUserInfo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Public binding that indicates wheter this FriendViewModel is for an existing friend or a possible friend
        /// (To allow requesting to add friend)
        /// </summary>
        public bool FriendExists => FriendInfo != null;

        /// <summary>
        /// Whether the logged user was the one that sent the friend request
        /// </summary>
        public bool DidFriendSendTheRequest => FriendInfo.UserId1 == FriendUserInfo.Id;

        /// <summary>
        /// The command that the button can activate to change status
        /// </summary>
        public ICommand ChangeFriendStatusCommand => new RelayCommand<string>(async (s) => await ChangeFriendStatus(s));

        public ICommand SendFriendRequestCommand => new RelayCommand(async () => await SendFriendRequest());

        #endregion

        #region Constructors

        public FriendViewModel(Friend friendInfo, User friendUserInfo)
        {
            this.friendUserInfo = friendUserInfo;
            this.friendInfo = friendInfo;
        }

        #endregion

        #region Methods

        private async Task ChangeFriendStatus(string statusStr)
        {
            try
            {
                var status = (FriendStatus)Enum.Parse(typeof(FriendStatus), statusStr);
                FriendInfo = await Connection.Instance.Service.ChangeFriendStatusAsync(FriendUserInfo.Id, status);
            }
            catch (FaultException<OperationFault> of)
            {
                ShowFault(of);
            }
        }

        private async Task SendFriendRequest()
        {
            try
            {
                FriendInfo = await Connection.Instance.Service.AddFriendAsync(FriendUserInfo.Id);
            }
            catch (FaultException<OperationFault> of)
            {
                ShowFault(of);
            }
        }

        #endregion
    }
}
