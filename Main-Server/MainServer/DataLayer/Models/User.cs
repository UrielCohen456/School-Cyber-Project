using System.Runtime.Serialization;

namespace DataLayer
{
    /// <summary>
    /// Represents a user
    /// </summary>
    [DataContract]
    public class User
    {
        #region Properties

        /// <summary>
        /// Id of the user
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Public name of the user that people see
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// If null then user is not inside a session, if not null then it is the id of the active session
        /// </summary>
        [DataMember]
        public int? ActiveSession { get; set; }

        #endregion
    }
}
