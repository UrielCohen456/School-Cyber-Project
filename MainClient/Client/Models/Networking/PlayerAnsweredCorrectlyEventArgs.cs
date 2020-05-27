using Client.MainServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Networking
{
    public class PlayerAnsweredCorrectlyEventArgs : EventArgs
    {
        public User Player { get; set; }
        
        public PlayerGameData PlayerData { get; set; }

        public PlayerAnsweredCorrectlyEventArgs(User player, PlayerGameData playerData)
        {
            Player = player;
            PlayerData = playerData;
        }
    }
}
