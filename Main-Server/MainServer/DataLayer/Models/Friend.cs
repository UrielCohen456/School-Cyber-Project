using DataLayer.ORM;
using System;
using System.Runtime.Serialization;

namespace DataLayer.Models
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
        /// Id of the user
        /// </summary>
        [DataMember]
        public int UserId1 { get; set; }

        /// <summary>
        /// Id of the user
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
    }
}
