using Client.MainServer;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;

namespace Client.Models.Networking
{
    /// <summary>
    /// Represents the class that talks with the server and keeps the connection object to the server
    /// </summary>
    public class Connection : IClientServiceCallback
    {
        /// <summary>
        /// Singleton class accesable by the static Instance property
        /// </summary>
        public static Connection Instance { get; private set; } = null;

        /// <summary>
        /// Initalizes the connection singleton
        /// </summary>
        public static void Initalize() 
        {
            Instance = new Connection(); 
        }

        private Connection()
        {
            try
            {
                while (Service == null || Service?.State != CommunicationState.Opened)
                {
                    Thread.Sleep(500);
                    Service = new ClientServiceClient(new InstanceContext(this));
                    Service.Open();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// The connection to the server for sending messages
        /// </summary>
        public ClientServiceClient Service { get; private set; }

        #region Duplex Events

        public event EventHandler<FriendStatusChangedEventArgs> FriendStatusChangedEvent;
        public void FriendStatusChanged(Friend friend, User friendUser)
        {
            FriendStatusChangedEvent?.Invoke(this, new FriendStatusChangedEventArgs(friend, friendUser));
        }

        public event EventHandler<NewMessageReceivedEventArgs> NewMessageReceivedEvent;
        public void NewMessageReceived(User user, Message message)
        {
            NewMessageReceivedEvent?.Invoke(this, new NewMessageReceivedEventArgs(user, message));
        }

        public event EventHandler<NewUserJoinedGameSessionEventArgs> NewUserJoinedGameSessionEvent;
        public void NewUserJoinedGameSession(User user)
        {
            NewUserJoinedGameSessionEvent?.Invoke(this, new NewUserJoinedGameSessionEventArgs(user));
        }

        public event EventHandler<RoomUpdatedEventArgs> RoomUpdatedEvent;
        public void RoomUpdated(Room updatedRoom, RoomUpdate update)
        {
            RoomUpdatedEvent?.Invoke(this, new RoomUpdatedEventArgs(updatedRoom, update));
        }

        public event EventHandler<GameStartedEventArgs> GameStartedEvent;
        public void GameStarted(int gameId)
        {
            GameStartedEvent?.Invoke(this, new GameStartedEventArgs(gameId));
        }

        public event EventHandler<PlayerLeftTheGameEventArgs> PlayerLeftTheGameEvent;
        public void PlayerLeftTheGame(User player)
        {
            PlayerLeftTheGameEvent?.Invoke(this, new PlayerLeftTheGameEventArgs(player));
        }

        public event EventHandler<BoardChangedEventArgs> BoardChangedEvent;
        public void BoardChanged(MemoryStream newBoard)
        {
            //var strokes = new StrokeCollection(newBoard);
            BoardChangedEvent?.Invoke(this, new BoardChangedEventArgs(newBoard));
        }

        public event EventHandler<PlayerSubmitedGuessEventArgs> PlayerSubmitedGuessEvent;
        public void PlayerSubmitedGuess(User player, string guess)
        {
            PlayerSubmitedGuessEvent?.Invoke(this, new PlayerSubmitedGuessEventArgs(player, guess));
        }

        public event EventHandler<PlayerAnsweredCorrectlyEventArgs> PlayerAnsweredCorrectlyEvent;
        public void PlayerAnsweredCorrectly(User player, PlayerGameData playerData)
        {
            PlayerAnsweredCorrectlyEvent?.Invoke(this, new PlayerAnsweredCorrectlyEventArgs(player, playerData));
        }

        #endregion
    }
}
