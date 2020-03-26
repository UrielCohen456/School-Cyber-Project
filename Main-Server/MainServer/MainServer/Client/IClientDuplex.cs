using DataLayer.Models;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MainServer
{
    /// <summary>
    /// The duplex contract for the client service
    /// </summary>
    public interface IClientDuplex
    {
        /// <summary>
        /// Tells the client that a new user has joined the session he is in
        /// </summary>
        /// <param name="user"></param>       
        [OperationContract(IsOneWay=false)]
        void NewUserJoinedGameSession(User user);

        /// <summary>
        /// Tells the client that a new message was sent to him
        /// </summary>
        /// <param name="message"></param>
        [OperationContract(IsOneWay=false)]
        void NewMessageReceived(User user, string messageContent);

        /// <summary>
        /// Tells the client that a certian friend status has changed
        /// </summary>
        /// <param name="friend">The friend object</param>
        /// <param name="status">The new status</param>
        [OperationContract(IsOneWay=false)]
        void FriendStatusChanged(Friend friend);
    }
}
