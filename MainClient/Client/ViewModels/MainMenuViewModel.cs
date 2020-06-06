using Client.Utility;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Models.Networking;
using System.Windows;
using System.ServiceModel;
using Client.MainServer;

namespace Client.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
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
        private readonly GameMainViewModel gameMenuViewModel = new GameMainViewModel();
        
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

        public ICommand LogoutCommand => new RelayCommand(Logout);

        public ICommand ChangeToFriendsViewCommand => new RelayCommand(() => CurrentViewModel = friendsViewModel);
        public ICommand ChangeToProfileViewCommand => new RelayCommand(() => CurrentViewModel = new ProfileViewModel());
        public ICommand ChangeToGameViewCommand => new RelayCommand(() => CurrentViewModel = gameMenuViewModel);


        #endregion

        #region Constructors

        public MainMenuViewModel()
        { 
            CurrentViewModel = friendsViewModel;
        }

        public override void Dispose()
        {
            base.Dispose();
            friendsViewModel?.Dispose();
            gameMenuViewModel?.Dispose();
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
