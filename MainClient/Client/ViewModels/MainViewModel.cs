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
        private readonly FriendsViewModel friendsViewModel;


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
        public ICommand ChangeToProfileViewCommand => new RelayCommand(() => CurrentViewModel = new LoginViewModel());
        public ICommand ChangeToGameViewCommand => new RelayCommand(() => CurrentViewModel = new SignupViewModel());


        #endregion


        #region Constructors

        public MainViewModel()
        {
            friendsViewModel = new FriendsViewModel();
            

            CurrentViewModel = friendsViewModel;
        }

        #endregion


        #region Methods

        private void Logout()
        {
            Connection.Instance.Service.Logout();
            Globals.LoggedUser = null;
            ViewModelController.ChangeViewModel(new LoginViewModel());
        }

        #endregion
    }
}
