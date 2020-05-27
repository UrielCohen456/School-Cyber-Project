using System.Runtime.Serialization;

namespace DataLayer
{
    [DataContract]
    public class RoomData
    {
        #region Properties

        [DataMember]
        public int Id { get; set; }

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
        public bool HasPassword { get => !string.IsNullOrEmpty(Password); set { } }

        /// <summary>
        /// The max number of users that can join this room
        /// </summary>
        [DataMember]
        public int MaxPlayersCount { get; set; }

        [DataMember]
        public RoomState State { get; set; }

        #endregion

        #region Constructors
        public RoomData()
        {
        }

        public RoomData(int id, string name, int maxPlayersCount, string password)
        {
            Id = id;
            Name = name;
            MaxPlayersCount = maxPlayersCount;
            Password = password;
            State = RoomState.Open;
        }

        #endregion

    }

    [DataContract]
    public enum RoomState : byte
    {
        [EnumMember]
        Open,

        [EnumMember]
        Closed,

        [EnumMember]
        GameBegun,

    }
}