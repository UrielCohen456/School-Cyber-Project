using System.Runtime.Serialization;

namespace DataLayer
{
    [DataContract]
    public class BaseEntity
    {
        /// <summary>
        /// The id of the entity in the database
        /// </summary>
        [DataMember]
        public int Id { get; set; }
    }
}