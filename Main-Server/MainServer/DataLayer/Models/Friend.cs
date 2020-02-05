using System;
using System.Runtime.Serialization;

namespace DataLayer
{
    /// <summary>
    /// Represents a friend of a user. 
    /// The Id of the user that is friends with the one inside this class is not given.
    /// </summary>
    [DataContract]
    public class Friend
    {
        #region Properties

        /// <summary>
        /// Id of the friendship inside the database
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Id of the friend user
        /// </summary>
        [DataMember]
        public int FriendId { get; set; }

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
