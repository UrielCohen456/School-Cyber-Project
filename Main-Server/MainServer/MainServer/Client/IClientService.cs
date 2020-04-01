﻿using DataLayer;
using System.Collections.Generic;
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;

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
        User Signup(string username, string password, string name);

        /// <summary>
        /// Logs out the connected user. If no one is connected throws an exception.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void Logout();

        #endregion

        #region Friend Methods

        /// <summary>
        /// Gets all friends of the currently logged in user
        /// </summary>
        /// <returns>A list of all the friends of the currently logged user</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<Friend> GetFriends(int friendCount);

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
        [OperationContract(IsOneWay = true)]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void ChangeFriendStatus(int userId, FriendStatus status);

        /// <summary>
        /// Sends a message to a requested user
        /// </summary>
        /// <param name="toUserId">The id of the user to send the message to</param>
        /// <param name="message">The message</param>
        [OperationContract(IsOneWay = true)]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void SendMessage(int userId, string message);

        /// <summary>
        /// Retrieves the full conversation with the requested friend userId 
        /// </summary>
        /// <param name="userId">The friend's userId</param>
        /// <returns></returns>
        [OperationContract(IsOneWay = true)]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<Message> GetConversationWithUser(int userId);

        #endregion 

        /// <summary>
        /// Returns all the active game sessions
        /// </summary>
        /// <returns>List of game sessions</returns>
        [OperationContract]
        [FaultContract(typeof(OperationFault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        List<GameSession> GetActiveGameSessions();
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
