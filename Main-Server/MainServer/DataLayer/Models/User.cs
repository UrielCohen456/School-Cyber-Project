using DataLayer.ORM;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace DataLayer.Models
{
    /// <summary>
    /// Represents a user
    /// </summary>
    [DataContract]
    public class User : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Public name of the user that people see
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Username of the user that is used for logging in
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The first 20 bytes of the hashed password
        /// </summary>
        public byte[] Password { get; set; }

        /// <summary>
        /// The bytes of the salt used to hash the password
        /// </summary>
        public byte[] Salt { get; set; }

        #endregion

        public static HashedPassword HashPassword(string password)
        {
            // specify that we want to randomly generate a 20-byte salt
            using var deriveBytes = new Rfc2898DeriveBytes(password, 20);

            var salt = deriveBytes.Salt;
            var passwordBytes = deriveBytes.GetBytes(20);  // derive a 20-byte key
            
            return new HashedPassword { Salt = salt, Password = passwordBytes };
        }

        public bool IsHashedPasswordCorrect(string password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, Salt);
            var newKey = deriveBytes.GetBytes(20);  // derive a 20-byte key

            return newKey.SequenceEqual(Password);
        }
    }

    public class HashedPassword
    {
        public byte[] Salt { get; set; }

        public byte[] Password { get; set; }
    }
}
