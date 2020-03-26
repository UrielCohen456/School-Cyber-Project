using DataLayer.ORM;
using System.Runtime.Serialization;

namespace DataLayer.Models
{
    /// <summary>
    /// Message that is sent from one user to another
    /// </summary>
    [DataContract]
    public class Message : BaseEntity
    {
        #region Properties

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
        /// The text of the message 
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        #endregion

    }
}
