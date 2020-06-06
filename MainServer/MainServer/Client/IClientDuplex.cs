using DataLayer;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Ink;

namespace MainServer
{
    /// <summary>
    /// The duplex contract for the client service
    /// </summary>
    public interface IClientDuplex
    {
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
        /// Updates the client that their room has updated (Only applies if the user is in the room)
        /// </summary>
        /// <param name="updatedRoom"></param>
        [OperationContract(IsOneWay = true)]
        void RoomUpdated(Room updatedRoom, RoomUpdate update);

        /// <summary>
        /// Says that a game has started
        /// </summary>
        /// <param name="gameId"></param>
        [OperationContract(IsOneWay = true)]
        void GameStarted(int gameId);

        /// <summary>
        /// Notifys when a player has left the game
        /// </summary>
        /// <param name="player"></param>
        [OperationContract(IsOneWay = true)]
        void PlayerLeftTheGame(User player);

        /// <summary>
        /// Notifys when the painting player changes the board
        /// </summary>
        /// <param name="newBoard"></param>
        [OperationContract(IsOneWay = true)]
        void BoardChanged(MemoryStream newBoard);

        /// <summary>
        /// Notifys when a player submits a guess only if he didnt get it right
        /// </summary>
        /// <param name="player"></param>
        /// <param name="guess"></param>
        [OperationContract(IsOneWay = true)]
        void PlayerSubmitedGuess(User player, string guess);

        /// <summary>
        /// Notifys when a player answers currectly
        /// </summary>
        /// <param name="player"></param>
        /// <param name="playerData"></param>
        [OperationContract(IsOneWay = true)]
        void PlayerAnsweredCorrectly(User player, PlayerGameData playerData);
    }

    [DataContract]
    public enum RoomUpdate : byte
    {
        [EnumMember]
        Created,

        [EnumMember]
        Closed,

        [EnumMember]
        StateChanged,

        [EnumMember]
        Started,

        [EnumMember]
        UserLeft,

        [EnumMember]
        UserJoined,

    }
}
