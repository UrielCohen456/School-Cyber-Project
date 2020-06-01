using System;
using System.Runtime.Serialization;
using System.Threading;

namespace DataLayer
{


    [DataContract]
    public class PlayerGameData
    {
        public bool GuessedCurrentWord;

        [DataMember]
        public int userId;

        [DataMember]
        public int score;

        public void AddScore(int addedScore)
        {
            Interlocked.Add(ref score, addedScore);
        }
    }

    [DataContract]
    public struct RevealedLetter
    {
        [DataMember]
        public char Letter { get; set; }

        [DataMember]
        public int LetterIndex { get; set; }
    }

    [DataContract]
    public enum AnswerSubmitResult
    {
        [EnumMember]
        Wrong = 1,

        [EnumMember]
        Right = 2,

        [EnumMember]
        TimesUp = 3,

        [EnumMember]
        AnsweredAlready = 4,

        [EnumMember]
        GameFinished = 5,
    }

}
