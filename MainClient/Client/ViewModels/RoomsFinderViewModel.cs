using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Client.ViewModels
{
    public class RoomsFinderViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// The controller for the rooms portion of the main view
        /// </summary>
        private readonly RoomsMainViewModel mainController;

        /// <summary>
        /// List of all the available rooms
        /// </summary>
        private List<Room> rooms;

        /// <summary>
        /// The currently selected room
        /// </summary>
        private Room selectedRoom;

        /// <summary>
        /// The password to the room if it requires it
        /// </summary>
        private string password;

        /// <summary>
        /// Indicates whether the selected room requires a password
        /// </summary>
        private bool roomRequiresPassword;

        #endregion

        #region Properties

        public RoomsMainViewModel MainController
        {
            get => mainController;
            set { }
        }

        public List<Room> Rooms
        {
            get => rooms;
            set
            {
                rooms = value;
                OnPropertyChanged();
            }
        }

        public Room SelectedRoom
        {
            get => selectedRoom;
            set
            {
                if (value == null)
                    return;

                selectedRoom = value;
                RoomRequiresPassword = selectedRoom.Data.HasPassword;
                OnPropertyChanged();
            }
        }

        public bool RoomRequiresPassword
        {
            get => roomRequiresPassword;
            set
            {
                roomRequiresPassword = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }
        

        public ICommand JoinSelectedRoomCommand => new RelayCommand(JoinSelectedRoom);

        public ICommand RefreshRoomsCommand => new RelayCommand(RefreshRooms);

        public ICommand ChangeToCreateRoomViewCommand => MainController.ChangeToCreateRoomViewCommand;


        #endregion

        #region Constructors

        public RoomsFinderViewModel(RoomsMainViewModel menuController)
        {
            mainController = menuController;
            RoomRequiresPassword = false;
            Rooms = new List<Room>();

            RefreshRooms();
            Connection.Instance.RoomUpdatedEvent += OnRoomUpdate;
        }

        public override void Dispose()
        {
            base.Dispose();
            Connection.Instance.RoomUpdatedEvent -= OnRoomUpdate;
        }

        #endregion

        #region Methods

        private void OnRoomUpdate(object sender, RoomUpdatedEventArgs e)
        {
            if (e.Update == RoomUpdate.Created)
            {
                Rooms.Add(e.UpdatedRoom);
                OnPropertyChanged(nameof(Rooms));
            }
            else if (e.Update == RoomUpdate.Closed)
            {
                Rooms.Remove(Rooms.First(r => r.Data.Id == e.UpdatedRoom.Data.Id));
                OnPropertyChanged(nameof(Rooms));
            }
            else
            {
                var dataGridRoom = Rooms.First(r => r.Data.Id == e.UpdatedRoom.Data.Id);
                dataGridRoom.Data = e.UpdatedRoom.Data;
                OnPropertyChanged(nameof(Rooms));
            }
        }

        private void JoinSelectedRoom()
        {
            if (SelectedRoom == null)
            {
                MessageBox.Show("Needs to select a room to join one...");
                return;
            }

            var room = ExecuteFaultableMethod(() => Connection.Instance.Service.JoinRoom(SelectedRoom.Data.Id, Password));
            if (room == null) 
                return;

            mainController.ChangeToRoomLobbyViewCommand.Execute(room);
        }

        private void RefreshRooms()
        {
            var rooms = ExecuteFaultableMethod(() =>
            {
                var roomsList = Connection.Instance.Service.GetAllRooms();
                return roomsList;
            });

            Rooms.Clear();
            Rooms.AddRange(rooms);
            OnPropertyChanged(nameof(Rooms));
        }

        #endregion
    }
}
