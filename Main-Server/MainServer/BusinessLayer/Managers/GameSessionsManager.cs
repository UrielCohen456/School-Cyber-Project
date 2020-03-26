using DataLayer.Models;
using System.Collections.Generic;

namespace BusinessLayer.Managers
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
