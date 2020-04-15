using System.Runtime.Serialization;

namespace DataLayer
{
    [DataContract]
    public class GameSessionData
    {
        #region Properties

        /// <summary>
        /// The displayed name of the server (given by the user that created the server)
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The password of the game server. Will be null or empty if no password is provided
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates whetere there is a password
        /// </summary>
        [DataMember]
        public bool HasPassword { get => !string.IsNullOrEmpty(Password); }
 
        [DataMember]
        public int MaxPlayersCount { get; set; }
        #endregion
    }
}