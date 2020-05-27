using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class RoomUpdatedEventArgs : EventArgs
    {
        public Room UpdatedRoom { get; set; }
        
        public RoomUpdate Update { get; set; }

        public RoomUpdatedEventArgs(Room updatedRoom, RoomUpdate update)
        {
            UpdatedRoom = updatedRoom;
            Update = update;
        }
    }

}
