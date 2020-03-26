using DataLayer;
using System.Collections.Generic;

namespace BusinessLayer
{
    public sealed class GameSessionsManager
    {
        public readonly List<GameSession> GameSessions;

        public GameSessionsManager()
        {
            GameSessions = new List<GameSession>();
        }
    }

}
