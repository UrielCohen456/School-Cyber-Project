using System.Runtime.Serialization;

namespace DataLayer
{
    /// <summary>
    /// Message that is sent from one user to another
    /// </summary>
    [DataContract]
    public class Message
    {
        #region Properties

        /// <summary>
        /// The message's id
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// The id of the user that sent the message
        /// </summary>
        [DataMember]
        public int FromId { get; set; }

        /// <summary>
        /// The id of the user that the message is sent to
        /// </summary>
        [DataMember]
        public int ToId { get; set; }

        /// <summary>
        /// The Status of the message (True - message was accepted, False - message was stored) 
        /// </summary>
        [DataMember]
        public bool Status { get; set; }

        /// <summary>
        /// The text of the message 
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        #endregion

    }
}
