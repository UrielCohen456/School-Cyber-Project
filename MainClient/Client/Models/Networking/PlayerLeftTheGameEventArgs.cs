using Client.MainServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Networking
{
    public class PlayerLeftTheGameEventArgs : EventArgs
    {
        public User Player { get; set; }

        public PlayerLeftTheGameEventArgs(User player)
        {
            Player = player;
        }
    }
}
