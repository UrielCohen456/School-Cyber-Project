using Client.Models.Networking;
using Client.Utility;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Client.ViewModels
{

    public class GameScoreScreenViewModel : BaseViewModel
    {
        public class GameScoreScreenParams
        {
            public int GameId { get; set; }

            public ObservableCollection<PlayerViewModel> Players { get; set; }

            public GameScoreScreenParams(int gameId, ObservableCollection<PlayerViewModel> players)
            {
                GameId = gameId;
                Players = players;
            }
        }
        private readonly RoomsMainViewModel mainController;

        private int gameId;

        private ObservableCollection<PlayerViewModel> players;

        public int GameId
        {
            get => gameId;
            set
            {
                gameId = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PlayerViewModel> Players
        {
            get => players;
            set
            {
                players = value;
                OnPropertyChanged();
            }
        }

        public ICommand LeaveGameCommand => new RelayCommand(LeaveGame);

        public GameScoreScreenViewModel(RoomsMainViewModel mainController, GameScoreScreenParams screenParams)
        {
            this.mainController = mainController;
            GameId = screenParams.GameId;
            Players = screenParams.Players;
            foreach (var playerVM in Players)
                playerVM.IsPainter = false;
        }

        private void LeaveGame()
        {
            ExecuteFaultableMethod(() => Connection.Instance.Service.LeaveGame(gameId));
            mainController.ChangeToRoomFinderViewCommand.Execute(null);
        }
    }
}
