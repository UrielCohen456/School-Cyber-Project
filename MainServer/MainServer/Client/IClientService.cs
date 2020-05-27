using DataLayer;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Ink;

namespace MainServer
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IClientDuplex))]
    public interface IClientService
    {
        #region Authentication Methods

        /// <summary>
        /// Attempts to login.
        /// </summary>
        /// <param name="username">The username of the user (Not the public Name of the user)</param>
        /// <param name="password">The password of the user (Not hashed)</param>
        /// <returns>User object if succesfull, otherwise null</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        User Login(string username, string password);

        /// <summary>
        /// Attempts to Signup. If succesfull the user is logged in.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="password">The password of the user (Not hashed)</param>
        /// <param name="name">The public name of the user (Other users see this)</param>
        /// <returns>User object if succesfull, otherwise null</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        User Signup(string name, string username, string password);

        /// <summary>
        /// Logs out the connected user. If no one is connected throws an exception.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void Logout();

        #endregion

        #region Users Methods

        /// <summary>
        /// Gets a specific user based on his id
        /// </summary>
        /// <returns>The selected user or null if not found</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        User GetUser(int id);


        /// <summary>
        /// Gets all users based on a query (or just first users if it is empty)
        /// </summary>
        /// <returns>A list of all the friends of the currently logged user</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<User> GetUsers(string searchQuery, int userCount);

        #endregion

        #region Friend Methods

        /// <summary>
        /// Gets all friends of the currently logged in user
        /// </summary>
        /// <returns>A list of all the friends of the currently logged user</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<Friend> GetFriends(FriendStatus status, int friendCount);

        /// <summary>
        /// Gets the friend for the requested user if it exists.
        /// If not returns null
        /// </summary>
        /// <param name="userId">The requested user's id</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Friend GetFriendIfExists(int userId);

        /// <summary>
        /// Sends a request to add the friend.
        /// </summary>
        /// <param name="userId">Id of the user to add as a friend</param>
        /// <returns>Friend object of the friendship and its details</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Friend AddFriend(int userId);

        /// <summary>
        /// Sends a request to send a friend status. If the dest status is not a valid option to change to,
        /// Throws an exception
        /// </summary>
        /// <param name="friendId">The id of the friendship (not the friend's userId)</param>
        /// <param name="status">The requested status</param>
        /// <returns>The updated friend</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Friend ChangeFriendStatus(int userId, FriendStatus status);

        /// <summary>
        /// Sends a message to a requested user
        /// </summary>
        /// <param name="toUserId">The id of the user to send the message to</param>
        /// <param name="message">The message</param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Message SendMessage(int userId, string messageText);

        /// <summary>
        /// Retrieves the full conversation with the requested friend userId 
        /// </summary>
        /// <param name="userId">The friend's userId</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<Message> GetConversationWithUser(int userId);

        #endregion

        #region Room Methods

        /// <summary>
        /// Gets all active rooms
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<Room> GetAllRooms();

        /// <summary>
        /// Creates a room and returns it
        /// </summary>
        /// <param name="maxPlayerCount"></param>
        /// <param name="roomName"></param>
        /// <param name="password">Will be null or empty if the room doesn't need a password</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Room CreateRoom(RoomParameters roomParams);

        /// <summary>
        /// Attempts to join a room
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="password">Will be null or empty if room doesn't have a password</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Room JoinRoom(int roomId, string password);

        /// <summary>
        /// Attempts to leave the current room
        /// </summary>
        /// <param name="roomId"></param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void LeaveRoom(int roomId);

        /// <summary>
        /// Changes the room state 
        /// </summary>
        /// <param name="newState"></param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void ChangeRoomState(int roomId, RoomState newState);

        ///// <summary>
        ///// Sends an invite request to the friend and he can then send a join request to the server to join the room
        ///// </summary>
        ///// <param name="friendUserId"></param>
        ///// <param name="roomId"></param>
        //[OperationContract]
        //[FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        //void InviteFriendToRoom(int friendUserId, int roomId);

        #endregion

        #region Game Methods

        /// <summary>
        /// Starts a game from the room screen (only works from the admin) and returns the game's id
        /// </summary>
        /// <param name="roomId"></param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        int StartGame(int roomId, GameParameters parameters);

        /// <summary>
        /// Removes the user from the current game
        /// </summary>
        /// <param name="gameId"></param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void LeaveGame(int gameId);

        /// <summary>
        /// Retrieves all the players in the game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<User> GetAllPlayers(int gameId);

        /// <summary>
        /// Gets the remaining time for the current round
        /// </summary>
        /// <param name="gameId"></param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        GameInformation GetGameInformation(int gameId);

        /// <summary>
        /// Submits a guess (Called by the guessers)
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="guess"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        AnswerSubmitResult SubmitGuess(int gameId, string guess);

        /// <summary>
        /// Submit the board when it is changed (Called by the painter of that round)
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="strokes"></param>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void SubmitDraw(int gameId, MemoryStream strokes);

        /// <summary>
        /// Returns the scores of the match
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<PlayerGameData> GetScores(int gameId);

        #endregion
    }

    /// <summary>
    /// Basic operation fault class
    /// </summary>
    [DataContract]
    public class OperationFault
    {
        /// <summary>
        /// The error that occured
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The operation that caused this exception
        /// </summary>
        [DataMember]
        public string Operation { get; set; }

        public OperationFault(string errorMessage, string operation)
        {
            ErrorMessage = errorMessage;
            Operation = operation;
        }

    }
}
