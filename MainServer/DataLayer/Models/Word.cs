using System.Runtime.Serialization;

namespace DataLayer
{
    [DataContract]
    public class Word : BaseEntity
    {
        #region Properties

        /// <summary>
        /// The text of the word 
        /// </summary>
        [DataMember]
        public string Text { get; set; }

        #endregion

        public Word()
        {
            Id = -1;
            Text = "";
        }

        public bool IsGuessCorrect(string guess)
        {
            return Text == guess;
        }
    }

}
