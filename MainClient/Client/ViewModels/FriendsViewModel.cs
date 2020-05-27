using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

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

        /// <summary>
        /// The status that the user selected to filter the friends list
        /// </summary>
        private string selectedStatusFilter;

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
                        var result = ExecuteFaultableMethod(() => Connection.Instance.Service.GetConversationWithUser(value.FriendUserInfo.Id));
                        CurrentConversation = new ObservableCollection<Message>(result);
                    }
                    else
                    {
                        CurrentConversation = null;
                    }
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
        /// Binding to selectedFilter element
        /// </summary>
        public string SelectedStatusFilter
        {
            get => selectedStatusFilter;
            set
            {
                selectedStatusFilter = value;
                OnPropertyChanged();
                if (selectedStatusFilter != null)
                    Dispatcher.CurrentDispatcher.InvokeAsync(GetFriendsByStatus);
            }
        }

        /// <summary>
        /// Sends the search query to look for possible friends
        /// </summary>
        public ICommand SearchForUsersByQueryCommand => new RelayCommand(async () => await SearchForUsersByQuery());

        /// <summary>
        /// Sends the current message to the current selected friend
        /// </summary>
        public ICommand SendMessageCommand => new RelayCommand(async () => await SendMessage());

        #endregion

        #region Constructors

        public FriendsViewModel()
        {
            Connection.Instance.NewMessageReceivedEvent += OnNewMessageReceived;
            Connection.Instance.FriendStatusChangedEvent += OnFriendStatusChanged;
            FriendsList = new ObservableCollection<FriendViewModel>();
            SelectedStatusFilter = "All";
            Dispatcher.CurrentDispatcher.InvokeAsync(GetFriendsByStatus);
        }

        public override void Dispose()
        {
            base.Dispose();
            Connection.Instance.NewMessageReceivedEvent -= OnNewMessageReceived;
            Connection.Instance.FriendStatusChangedEvent -= OnFriendStatusChanged;
        }

        #endregion

        #region Methods

        private void OnNewMessageReceived(object sender, NewMessageReceivedEventArgs e)
        {
            Globals.UIDispatcher.Invoke(() =>
            {
                if (e.User.Id == SelectedUser.FriendUserInfo.Id)
                    CurrentConversation.Add(e.Message);
            });
        }

        private void OnFriendStatusChanged(object sender, FriendStatusChangedEventArgs e)
        {
            Globals.UIDispatcher.Invoke(() =>
            {
                var friendVM = FriendsList.Where(vm => vm.FriendUserInfo.Id == e.FriendUser.Id).FirstOrDefault();

                if (friendVM != null)
                {
                    friendVM.FriendInfo = e.Friend;
                    return;
                }
                FriendsList.Add(new FriendViewModel(e.Friend, e.FriendUser));
            });
        }

        private async Task<IEnumerable<FriendViewModel>> GetFriendsWithSpecificStatus(FriendStatus status, int friendCount = 50)
        {
            // Checks which user id belongs to the friend and returns it
            int getFriendUserId(int fromId, int toId) => fromId == Globals.LoggedUser.Id ? toId : fromId;

            var friends = ExecuteFaultableMethod(() => Connection.Instance.Service.GetFriends(status, friendCount));
            if (friends == null) return null;

            var downloadingFriendUsers =
                    from friend in friends
                    select ExecuteFaultableMethod(() => Connection.Instance.Service.GetUserAsync(getFriendUserId(friend.UserId1, friend.UserId2)));

            var friendUsers = await Task.WhenAll(downloadingFriendUsers);

            var friendViewModelsToAdd = new List<FriendViewModel>();
            for (var i = 0; i < friends.Count; i++)
            {
                friendViewModelsToAdd.Add(new FriendViewModel(friends[i], friendUsers[i]));
            }

            return friendViewModelsToAdd;
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrEmpty(MessageText))
                return;

            var message = await ExecuteFaultableMethod(() => Connection.Instance.Service.SendMessageAsync(SelectedUser.FriendUserInfo.Id, MessageText));
            CurrentConversation.Add(message);
            MessageText = null;
        }

        private async Task GetFriendsByStatus()
        {
            List<FriendViewModel> friends;

            if (selectedStatusFilter == "Sort By")
                return;

            if (selectedStatusFilter != "All")
            {
                var status = (FriendStatus)Enum.Parse(typeof(FriendStatus), selectedStatusFilter);
                FriendsList = new ObservableCollection<FriendViewModel>(await GetFriendsWithSpecificStatus(status));
                return;
            }

            friends = (await GetFriendsWithSpecificStatus(FriendStatus.Accepted)).ToList();
            friends?.AddRange(await GetFriendsWithSpecificStatus(FriendStatus.Waiting));
            friends?.AddRange(await GetFriendsWithSpecificStatus(FriendStatus.Denied));
            friends?.AddRange(await GetFriendsWithSpecificStatus(FriendStatus.Removed));

            FriendsList = new ObservableCollection<FriendViewModel>(friends);
            selectedStatusFilter = null;
        }

        private async Task SearchForUsersByQuery()
        {
            SelectedStatusFilter = "Sort By";
            var users = await ExecuteFaultableMethod(() => Connection.Instance.Service.GetUsersAsync(SearchQuery, 50));

            IEnumerable<FriendViewModel> friendViewModelsToAdd = null;

            friendViewModelsToAdd =
                    from user in users
                    where user != null
                    select new FriendViewModel(ExecuteFaultableMethod(() => Connection.Instance.Service.GetFriendIfExists(user.Id)), user);

            FriendsList = new ObservableCollection<FriendViewModel>(friendViewModelsToAdd);
        }

        #endregion
    }
}
