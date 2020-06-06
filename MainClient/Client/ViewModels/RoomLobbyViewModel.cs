using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Client.ViewModels
{
    public class RoomLobbyViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// Controller
        /// </summary>
        private readonly GameMainViewModel mainController;
        
        /// <summary>
        /// The current room
        /// </summary>
        private Room room;

        private int numberOfRounds;

        #endregion

        #region Properties

        public Room Room
        {
            get => room;
            set
            {
                if (value != null)
                {
                    room = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        public bool IsAdmin
        {
            get => Room.Admin.Id == Globals.LoggedUser?.Id;
            set { }
        }

        public int NumberOfRounds
        {
            get => numberOfRounds;
            set
            {
                numberOfRounds = value;
                OnPropertyChanged();
            }
        }

        public ICommand LeaveRoomCommand => new RelayCommand(LeaveRoom);

        public ICommand StartGameCommand => new RelayCommand(StartGame);


        #endregion

        #region Constructors

        public RoomLobbyViewModel(GameMainViewModel mainController, Room room)
        {
            this.mainController = mainController;
            this.room = room;
            NumberOfRounds = 0;

            Connection.Instance.RoomUpdatedEvent += OnRoomUpdate;
            if (!IsAdmin)
                Connection.Instance.GameStartedEvent += OnGameStarted;
        }


        public override void Dispose()
        {
            base.Dispose();
            Connection.Instance.RoomUpdatedEvent -= OnRoomUpdate;
            if (!IsAdmin)
                Connection.Instance.GameStartedEvent -= OnGameStarted;
        }

        private void OnGameStarted(object sender, GameStartedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                mainController.ChangeToGameViewCommand.Execute(e.GameId);
            });
        }

        private void OnRoomUpdate(object sender, RoomUpdatedEventArgs e)
        {
            if (e.Update == RoomUpdate.Started)
            {
                MessageBox.Show("Game started");
                return;
            }

            Room = e.UpdatedRoom;
        }

        #endregion

        #region Methods

        private void LeaveRoom()
        {
            var left = ExecuteFaultableMethod(() => Connection.Instance.Service.LeaveRoom(Room.Data.Id));

            if (left)
                mainController.ChangeToRoomFinderViewCommand.Execute(null);
        }

        private void StartGame()
        {
            var gameParameters = new GameParameters { NumberOfRounds = NumberOfRounds + 1 };
            var gameId = 0;
            var success = ExecuteFaultableMethod(() => { gameId = Connection.Instance.Service.StartGame(Room.Data.Id, gameParameters); });
            if (!success)
                return;
            
            mainController.ChangeToGameViewCommand.Execute(gameId);
        }

        #endregion
    }
}
