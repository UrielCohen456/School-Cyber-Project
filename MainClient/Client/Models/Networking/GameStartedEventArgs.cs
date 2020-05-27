using System;

namespace Client.Models.Networking
{
    public class GameStartedEventArgs : EventArgs
    {
        public int GameId { get; set; }

        public GameStartedEventArgs(int gameId)
        {
            GameId = gameId;
        }

    }
}
