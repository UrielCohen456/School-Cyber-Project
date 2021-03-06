﻿using System;
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
        #region Fields

        #endregion

        #region Properties

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
            if (Users.Any(pair => pair.Key == userToAdd.Id))
                throw new Exception("User already in the room");

            if (Data.HasPassword && Data.Password != password)
                throw new Exception("Incorrect password");

            if (Users.Count >= Data.MaxPlayersCount)
                throw new Exception("Max user count reached");

            Users.TryAdd(userToAdd.Id, userToAdd);
        }

        public bool RemoveUser(User userToRemove)
        {
            if (!Users.Any(pair => pair.Key == userToRemove.Id))
                throw new Exception("User isn't in the room");

            Users.TryRemove(userToRemove.Id, out var _);

            if (IsRoomEmpty())
                return true;

            if (Admin.Id == userToRemove.Id)
                Admin = Users.ElementAt(0).Value;

            return false;
        }

        public bool IsRoomEmpty()
        {
            return Users.Count == 0;
        }

        #endregion
    }

    [DataContract]
    public struct RoomParameters
    {
        [DataMember]
        public int MaxPlayersCount { get; set; }

        [DataMember]
        public string RoomName { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}
