using DataLayer;
using System.ServiceModel;

namespace MainServer
{
    /// <summary>
    /// The duplex contract for the client service
    /// </summary>
    public interface IClientDuplex
    {
        /// <summary>
        /// Tells the client that the message was accepted by the user
        /// </summary>
        /// <param name="messageId"></param>
        [OperationContract(IsOneWay=false)]
        void MessageAccepted(int messageId);

        /// <summary>
        /// Tells the client that a new message was sent to him
        /// </summary>
        /// <param name="message"></param>
        [OperationContract(IsOneWay=false)]
        void NewMessage(Message message);

        /// <summary>
        /// Tells the client that a certian friend status has changed
        /// </summary>
        /// <param name="friend">The friend object</param>
        /// <param name="status">The new status</param>
        [OperationContract(IsOneWay=false)]
        void FriendStatusChanged(Friend friend, FriendStatus status);
    }
}
