using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class CreateRoomViewModel : BaseViewModel
    {
        #region Fields

        private readonly GameMainViewModel mainController;

        private RoomParameters roomParameters;

        #endregion

        #region Properties

        public GameMainViewModel MainController
        {
            get => mainController;
            set { }
        }

        public ICommand CreateRoomCommand => new RelayCommand(CreateRoom);

        public string RoomName
        {
            get { return roomParameters.RoomName; }
            set
            {
                if (roomParameters.RoomName != value)
                {
                    roomParameters.RoomName = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MaxPlayersCount
        {
            get { return roomParameters.MaxPlayersCount; }
            set
            {
                if (roomParameters.MaxPlayersCount != value)
                {
                    roomParameters.MaxPlayersCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get { return roomParameters.Password; }
            set
            {
                if (roomParameters.Password != value)
                {
                    roomParameters.Password = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public CreateRoomViewModel(GameMainViewModel mainController)
        {
            this.mainController = mainController;
            roomParameters = new RoomParameters();
            RoomName = Globals.LoggedUser.Name + "'s Room";
            MaxPlayersCount = 3;
        }

        #endregion

        #region Methods

        private void CreateRoom()
        {
            if (string.IsNullOrEmpty(roomParameters.RoomName) || roomParameters.MaxPlayersCount < 3)
            {
                MessageBox.Show("Invalid parameters, please fill again");
                return;
            }

            var room = ExecuteFaultableMethod(() => Connection.Instance.Service.CreateRoom(roomParameters));
            if (room == null) return;

            mainController.ChangeToRoomLobbyViewCommand.Execute(room);
        }

        #endregion

    }
}
