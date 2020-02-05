using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataLayer
{
    /// <summary>
    /// Represents an active game session
    /// </summary>
    [DataContract]
    public class GameSession
    {
        #region Properties

        /// <summary>
        /// Id of the game session
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// All the id's of the users inside the game session
        /// </summary>
        [DataMember]
        public List<int> Users { get; set; }

        /// <summary>
        /// The date at which the game session will not be valid anymore.
        /// This can be updated by the game server by refreshing itself
        /// </summary>
        [DataMember]
        public DateTime ActiveUntill { get; set; }

        /// <summary>
        /// The Information about the server that hosts this game session
        /// </summary>
        [DataMember]
        public GameServerData ServerData { get; set; }

        #endregion
    }
}
