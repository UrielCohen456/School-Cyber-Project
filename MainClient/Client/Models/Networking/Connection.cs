using Client.MainServer;
using System;
using System.ServiceModel;
using System.Threading;

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
        public static void Initalize() { Instance = new Connection(); }

        private Connection()
        {
            while (Service == null || Service.State != CommunicationState.Created)
            {
                Thread.Sleep(100);
                try { Service = new ClientServiceClient(new InstanceContext(this)); Thread.Sleep(100); }
                catch { }
            }

            Service.Open();
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

        // TODO: Implement this when starting to work on the game
        public void NewUserJoinedGameSession(User user)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
