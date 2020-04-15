using DataLayer;
using System;
using System.Runtime.Serialization;

namespace DataLayer
{
    /// <summary>
    /// Represents a friend of a user. 
    /// The Id of the user that is friends with the one inside this class is not given.
    /// </summary>
    [DataContract]
    public class Friend : BaseEntity
    {
        #region Properties
        
        /// <summary>
        /// Id of the user that requested the friendship
        /// </summary>
        [DataMember]
        public int UserId1 { get; set; }

        /// <summary>
        /// Id of the user that received the friendship
        /// </summary>
        [DataMember]
        public int UserId2 { get; set; }

        /// <summary>
        /// The current status of the friendship
        /// </summary>
        [DataMember]
        public FriendStatus Status { get; set; }

        /// <summary>
        /// The date when the friend request was made
        /// </summary>
        [DataMember]
        public DateTime DateAdded { get; set; }

        #endregion

        /// <summary>
        /// Checking if the change is logicaly possible and if it is, if t
        /// </summary>
        /// <param name="statusToChangeTo"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        public bool IsChangePossible(FriendStatus statusToChangeTo, int requestingUserId)
        {
            // Checking first if it is logicaly possible
            if (!Status.IsChangePossible(statusToChangeTo))
                return false;

            // Checking if the user that requested the change is the one that received the friend request
            // because only the user that received the request (UserId2) is the one that can choose to accept or deny it
            if ((statusToChangeTo == FriendStatus.Accepted || statusToChangeTo == FriendStatus.Denied)
                && UserId2 != requestingUserId)
                return false;


            return true;
        }
    }
}
