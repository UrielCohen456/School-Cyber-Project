using System.Runtime.Serialization;

namespace DataLayer
{
    [DataContract]
    public class UserProfileInfo
    {
        /// <summary>
        /// The total games played by the user (Finished the game)
        /// </summary>
        [DataMember]
        public int GamesPlayed { get; set; }

        /// <summary>
        /// The total games won by the user
        /// </summary>
        [DataMember]
        public int GamesWon { get; set; }

        /// <summary>
        /// The total games lost by the user
        /// </summary>
        [DataMember]
        public int GamesLost { get; set; }

        /// <summary>
        /// The highest score the user achieved ever on a game
        /// </summary>
        [DataMember]
        public int HighestScore { get; set; }

        /// <summary>
        /// The total score of the user from all the games
        /// </summary>
        [DataMember]
        public int TotalScore { get; set; }
    }
}
