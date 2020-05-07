using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DataLayer
{
    /// <summary>
    /// Represents a room
    /// </summary>
    [DataContract]
    public class Room
    {
        #region Properties

        /// <summary>
        /// All the id's of the users inside the game session
        /// </summary>
        [DataMember]
        public ConcurrentDictionary<int, User> Users { get; set; }

        /// <summary>
        /// The room's admin
        /// </summary>
        [DataMember]
        public User Admin { get; set; }

        /// <summary>
        /// The Information about the server that hosts this game session
        /// </summary>
        [DataMember]
        public RoomData Data { get; set; }

        #endregion
        
        #region Constructors

        public Room()
        {
            Users = new ConcurrentDictionary<int, User>();
        }

        public Room(User creator, RoomData data, string password)
        {
            Data = data;
            Admin = creator;
            Users = new ConcurrentDictionary<int, User>();

            AddUser(creator, password);
        }

        #endregion

        #region Methods

        public void AddUser(User userToAdd, string password)
        {
            if (Data.State == RoomState.Closed)
                throw new Exception("Admin closed the room");
            
            if (Users.ToList().Exists(pair => pair.Key == userToAdd.Id))
                throw new Exception("User already in the room");

            if (Data.HasPassword && Data.Password != password)
                throw new Exception("Incorrect password");

            Users.TryAdd(userToAdd.Id, userToAdd);
        }

        public bool RemoveUser(User userToRemove)
        {
            if (!Users.ToList().Exists(pair => pair.Key == userToRemove.Id))
                throw new Exception("User isn't in the room");

            Users.TryRemove(userToRemove.Id, out var _);

            if (IsRoomEmpty())
                return true;

            if (Admin.Id == userToRemove.Id)
                Admin = Users.ToList().ElementAt(0).Value;

            return false;
        }

        public bool IsRoomEmpty()
        {
            return Users.Count == 0;
        }

        #endregion
    }
}
