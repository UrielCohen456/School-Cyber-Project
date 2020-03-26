using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataLayer.Models
{
    /// <summary>
    /// Represents an active game session
    /// </summary>
    [DataContract]
    public class GameSession 
    {
        #region Properties

        /// <summary>
        /// All the id's of the users inside the game session
        /// </summary>
        [DataMember]
        public List<User> Users { get; set; }

        /// <summary>
        /// The Information about the server that hosts this game session
        /// </summary>
        [DataMember]
        public GameSessionData GameSessionData { get; set; }

        #endregion
    }
}
