using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class FriendsViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// The current selected friend to chat with
        /// </summary>
        private User selectedUser;

        /// <summary>
        /// The current conversation with the friend
        /// </summary>
        private ObservableCollection<Message> currentConversation;

        /// <summary>
        /// The text that the logged user wants to send
        /// </summary>
        private string messageText;

        #endregion

        #region Properties

        /// <summary>
        /// The current conversation between the logged user and the 
        /// </summary>
        public ObservableCollection<Message> CurrentConversation
        {
            get => currentConversation;
            private set
            {
                currentConversation = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get => selectedUser;
            set
            {
                if (selectedUser != value)
                {
                    selectedUser = value;
                    OnPropertyChanged();

                    var result = Connection.Instance.Service.GetConversationWithUser(value.Id);
                    CurrentConversation = new ObservableCollection<Message>(result);
                }
            }
        }

        /// <summary>
        /// Binding to the message text
        /// </summary>
        public string MessageText
        {
            get => messageText;
            set
            {
                messageText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All the friends of the logged in user
        /// </summary>
        public ObservableCollection<User> FriendsList { get; private set; }


        public ICommand SendMessageCommand => new RelayCommand(SendMessage);

        #endregion

        #region Constructors

        public FriendsViewModel()
        {
            GetFriends();
            Connection.Instance.NewMessageReceivedEvent += OnNewMessageReceived;
        }

        public override void Dispose()
        {
            base.Dispose();
            Connection.Instance.NewMessageReceivedEvent -= OnNewMessageReceived;
        }

        private void OnNewMessageReceived(object sender, NewMessageReceivedEventArgs e)
        {
            if (e.User.Id == SelectedUser.Id)
                CurrentConversation.Add(e.Message); 
        }

        private void GetFriends()
        {
            int returnFriendUserId(int fromId, int toId) => fromId == Globals.LoggedUser.Id ? toId : fromId;

            var friends = Connection.Instance.Service.GetFriends(50);
            
            if (friends != null)
            {
                var friendsList =
                    (from friend in friends
                     select Connection.Instance.Service.GetUser(returnFriendUserId(friend.UserId1, friend.UserId2))
                    ).ToList();
                FriendsList = new ObservableCollection<User>(friendsList);
            }
            else FriendsList = new ObservableCollection<User>();
        }

        #endregion

        #region Methods

        public async void SendMessage()
        {
            var message = await Connection.Instance.Service.SendMessageAsync(SelectedUser.Id, MessageText);
            CurrentConversation.Add(message);
            MessageText = null;
        }

        #endregion
    }
}
