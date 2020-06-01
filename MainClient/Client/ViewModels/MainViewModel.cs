using Client.Utility;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Models.Networking;
using System.Windows;
using System.ServiceModel;
using Client.MainServer;

namespace Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// holds a reference to the active view model
        /// </summary>
        private BaseViewModel currentViewModel;

        /// <summary>
        /// The friends view model
        /// </summary>
        private readonly FriendsViewModel friendsViewModel = new FriendsViewModel();

        /// <summary>
        /// Controls what to show - create room screen, in a room screen, room finder
        /// </summary>
        private readonly RoomsMainViewModel roomsMenuViewModel = new RoomsMainViewModel();
        
        #endregion

        #region Properties

        /// <summary>
        /// Binding to the active view model
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get => currentViewModel;
            private set
            {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The logged user's name for display
        /// </summary>
        public string LoggedUserName => Globals.LoggedUser.Name;

        public ICommand LogoutCommand => new RelayCommand(Logout);

        public ICommand ChangeToFriendsViewCommand => new RelayCommand(() => CurrentViewModel = friendsViewModel);
        public ICommand ChangeToProfileViewCommand => new RelayCommand(() => CurrentViewModel = new ProfileViewModel());
        public ICommand ChangeToRoomsViewCommand => new RelayCommand(() => CurrentViewModel = roomsMenuViewModel);


        #endregion

        #region Constructors

        public MainViewModel()
        { 
            CurrentViewModel = roomsMenuViewModel;
        }

        public override void Dispose()
        {
            base.Dispose();
            friendsViewModel?.Dispose();
            roomsMenuViewModel?.Dispose();
        }

        #endregion

        #region Methods

        private void Logout()
        {
            ExecuteFaultableMethod(() => Connection.Instance.Service.Logout());
            Globals.LoggedUser = null;
            ViewModelController.ChangeViewModel(new LoginViewModel());
        }

        #endregion
    }
}
