using Client.MainServer;
using System;

namespace Client.Models.Networking
{
    public class PlayerSubmitedGuessEventArgs : EventArgs
    {
        public User Player { get; set; }

        public string Guess { get; set; }

        public PlayerSubmitedGuessEventArgs(User player, string guess)
        {
            Player = player;
            Guess = guess;
        }
    }
}
