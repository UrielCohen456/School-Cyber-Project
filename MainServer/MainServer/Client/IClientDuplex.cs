using DataLayer;
using System.Runtime.Serialization;
using System.ServiceModel;

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
        [OperationContract(IsOneWay = true)]
        void NewUserJoinedGameSession(User user);

        /// <summary>
        /// Tells the client that a new message was sent to him
        /// </summary>
        /// <param name="message"></param>
        [OperationContract(IsOneWay = true)]
        void NewMessageReceived(User user, Message message);

        /// <summary>
        /// Tells the client that a certian friend status has changed
        /// </summary>
        /// <param name="friend">The friend object</param>
        /// <param name="friendUser">The user object of the friend that sent the request</param>
        /// <param name="status">The new status</param>
        [OperationContract(IsOneWay = true)]
        void FriendStatusChanged(Friend friend, User friendUser);

        /// <summary>
        /// Updates the client that their room has updated(Only applies if the user is in the room)
        /// </summary>
        /// <param name="updatedRoom"></param>
        [OperationContract(IsOneWay = true)]
        void RoomUpdated(Room updatedRoom, RoomUpdate update);
    }

    [DataContract]
    public enum RoomUpdate : byte
    {
        [EnumMember]
        StateChanged,

        [EnumMember]
        UserLeft,

        [EnumMember]
        UserJoined
    }
}
