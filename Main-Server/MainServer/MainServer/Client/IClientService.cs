using DataLayer;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace MainServer
{
    [ServiceContract(SessionMode= SessionMode.Required, CallbackContract=typeof(IClientDuplex))]
    public interface IClientService
    {
        User LoggedUser { get; }

        /// <summary>
        /// Attempts to login.
        /// </summary>
        /// <param name="username">The username of the user (Not the public Name of the user)</param>
        /// <param name="password">The password of the user (Not hashed)</param>
        /// <returns>User object if succesfull, otherwise null</returns>
        [OperationContract]
        User Login(string username, string password);
        
        /// <summary>
        /// Attempts to Signup. If succesfull the user is logged in.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="password">The password of the user (Not hashed)</param>
        /// <param name="name">The public name of the user (Other users see this)</param>
        /// <returns>User object if succesfull, otherwise null</returns>
        [OperationContract]
        User Signup(string username, string password, string name);

        /// <summary>
        /// Logs out the connected user. If no one is connected throws an exception.
        /// </summary>
        [OperationContract(IsOneWay=true)]
        void Logout();

        /// <summary>
        /// Gets all friends of the currently logged in user
        /// </summary>
        /// <returns>A list of all the friends of the currently logged user</returns>
        [OperationContract]
        List<Friend> GetFriends();

        /// <summary>
        /// Sends a request to add the friend.
        /// </summary>
        /// <param name="userId">Id of the user to add as a friend</param>
        /// <returns>Friend object of the friendship and its details</returns>
        [OperationContract]
        Friend AddFriend(int userId);

        /// <summary>
        /// Sends a request to send a friend status. If the dest status is not a valid option to change to,
        /// Throws an exception
        /// </summary>
        /// <param name="friendId">The id of the friendship (not the friend's userId)</param>
        /// <param name="status">The requested status</param>
        [OperationContract(IsOneWay=true)]
        void ChangeFriendStatus(int friendId, FriendStatus status);

        /// <summary>
        /// Sends a message to a requested user
        /// </summary>
        /// <param name="toUserId">The id of the user to send the message to</param>
        /// <param name="message">The message</param>
        /// <returns>True if the message was accepted, false if it is stored</returns>
        [OperationContract]
        bool SendMessage(int toUserId, string message);

        /// <summary>
        /// Returns all the active game sessions
        /// </summary>
        /// <returns>List of game sessions</returns>
        [OperationContract]
        List<GameSession> GetActiveGameSessions();

        /// <summary>
        /// Sends a request to get a game session identifier to be able to join it
        /// </summary>
        /// <param name="sessionId">The game session id</param>
        /// <param name="password">The password of the session (If the session doesnt have a password this is null)</param>
        /// <returns>The identifier of the game session</returns>
        [OperationContract]
        string GetSessionIdentifier(int sessionId, string password);
    }
}
