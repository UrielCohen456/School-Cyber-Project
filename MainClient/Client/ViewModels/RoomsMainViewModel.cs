using Client.MainServer;
using Client.Utility;
using System.Windows.Input;
using static Client.ViewModels.GameScoreScreenViewModel;

namespace Client.ViewModels
{
    public class RoomsMainViewModel : BaseViewModel
    {
        #region Fields

        private BaseViewModel displayedViewModel;

        #endregion

        #region Properties

        public BaseViewModel DisplayedViewModel
        {
            get => displayedViewModel;
            set
            {
                displayedViewModel = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public RoomsMainViewModel()
        {
            DisplayedViewModel = new RoomsFinderViewModel(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            DisplayedViewModel?.Dispose();
        }

        #endregion

        #region Methods

        public ICommand ChangeToRoomFinderViewCommand => new RelayCommand(() =>
        {
            ChangeViewModel(new RoomsFinderViewModel(this));
        });

        public ICommand ChangeToCreateRoomViewCommand => new RelayCommand(() =>
        {
            ChangeViewModel(new CreateRoomViewModel(this));
        });

        public ICommand ChangeToRoomLobbyViewCommand => new RelayCommand<Room>(room =>
        {
            ChangeViewModel(new RoomLobbyViewModel(this, room));
        });

        public ICommand ChangeToGameViewCommand => new RelayCommand<int>(gameId =>
        {
            ChangeViewModel(new GameViewModel(this, gameId));
        });

        public ICommand ChangeToGameScoreScreenCommand => new RelayCommand<GameScoreScreenParams>(scoreParams =>
        {
            ChangeViewModel(new GameScoreScreenViewModel(this, scoreParams));
        });

        private void ChangeViewModel(BaseViewModel viewModel)
        {
            DisplayedViewModel?.Dispose();
            DisplayedViewModel = viewModel;
        }

        #endregion
    }
}
