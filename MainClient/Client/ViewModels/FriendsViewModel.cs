using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class FriendsViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// The current selected friend to chat with
        /// </summary>
        private FriendViewModel selectedUser;

        /// <summary>
        /// The current conversation with the friend
        /// </summary>
        private ObservableCollection<Message> currentConversation;

        /// <summary>
        /// The text that the logged user wants to send
        /// </summary>
        private string messageText;

        /// <summary>
        /// The search query to search for friends
        /// </summary>
        private string searchQuery;

        /// <summary>
        /// Dictates wheter the user can send a message
        /// </summary>
        private bool canSendMessage;
        
        /// <summary>
        /// The friends list with the selected status
        /// </summary>
        private ObservableCollection<FriendViewModel> friendsList;

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

        public FriendViewModel SelectedUser
        {
            get => selectedUser;
            set
            {
                if (value == null)
                {
                    CanSendMessage = false;
                    CurrentConversation = null;
                    return;
                }
                if (selectedUser != value)
                {
                    selectedUser = value;
                    CanSendMessage = selectedUser.FriendExists;
                    OnPropertyChanged();

                    if (selectedUser.FriendExists)
                    {
                        var result = Connection.Instance.Service.GetConversationWithUser(value.FriendUserInfo.Id);
                        CurrentConversation = new ObservableCollection<Message>(result);
                    }
                    else
                        CurrentConversation = null;
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
        /// Binding to the search query
        /// </summary>
        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                searchQuery = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates wheter the user can send a message to the selected friend
        /// </summary>
        public bool CanSendMessage
        {
            get => canSendMessage;
            private set
            {
                canSendMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All the friends of the logged in user
        /// </summary>
        public ObservableCollection<FriendViewModel> FriendsList
        {
            get => friendsList;
            set 
            { 
                friendsList = value; 
                OnPropertyChanged();
                SelectedUser = null;
            }
        }

        /// <summary>
        /// Sends a message to the current conversation
        /// </summary>
        public ICommand SendMessageCommand => new RelayCommand(SendMessage);

        /// <summary>
        /// Retrieves all friends that have the requested status
        /// </summary>
        public ICommand GetFriendsByStatusCommand => new RelayCommand<string>(async (str) => await GetFriendsByStatus(str));

        /// <summary>
        /// Sends the search query to look for possible friends
        /// </summary>
        public ICommand SearchForUsersByQueryCommand => new RelayCommand(async () => await SearchForUsersByQuery());

        #endregion

        #region Constructors

        public FriendsViewModel()
        {
            Connection.Instance.NewMessageReceivedEvent += OnNewMessageReceived;
        }

        public override void Dispose()
        {
            base.Dispose();
            Connection.Instance.NewMessageReceivedEvent -= OnNewMessageReceived;
        }

        #endregion

        #region Methods

        private void OnNewMessageReceived(object sender, NewMessageReceivedEventArgs e)
        {
            if (e.User.Id == SelectedUser.FriendUserInfo.Id)
                CurrentConversation.Add(e.Message);
        }

        private async Task<IEnumerable<FriendViewModel>> GetFriendsWithSpecificStatus(FriendStatus status, int friendCount = 50)
        {
            // Checks which user id belongs to the friend and returns it
            int getFriendUserId(int fromId, int toId) => fromId == Globals.LoggedUser.Id ? toId : fromId;

            var friends = Connection.Instance.Service.GetFriends(status, friendCount);
            if (friends == null) return null;

            var downloadingFriendUsers =
                 from friend in friends
                 select Connection.Instance.Service.GetUserAsync(getFriendUserId(friend.UserId1, friend.UserId2));

            var friendUsers = await Task.WhenAll(downloadingFriendUsers);
            var friendViewModelsToAdd =
                 from friend in friends
                 from user in friendUsers
                 where friend != null && user != null
                 select new FriendViewModel(friend, user);

            return friendViewModelsToAdd;
        }

        private async void SendMessage()
        {
            if (string.IsNullOrEmpty(MessageText))
                return;
            var message = await Connection.Instance.Service.SendMessageAsync(SelectedUser.FriendUserInfo.Id, MessageText);
            CurrentConversation.Add(message);
            MessageText = null;
        }

        private async Task GetFriendsByStatus(string statusString)
        {
            var status = (FriendStatus)Enum.Parse(typeof(FriendStatus), statusString);
            var friends = await GetFriendsWithSpecificStatus(status);
            FriendsList = new ObservableCollection<FriendViewModel>(friends);
        }

        private async Task SearchForUsersByQuery()
        {
            var users = await Connection.Instance.Service.GetUsersAsync(SearchQuery, 50);

            var friendViewModelsToAdd =
                 from user in users
                 where user != null
                 select new FriendViewModel(Connection.Instance.Service.GetFriendIfExists(user.Id), user);
            
            FriendsList = new ObservableCollection<FriendViewModel>(friendViewModelsToAdd);
        }

        #endregion
    }
}
