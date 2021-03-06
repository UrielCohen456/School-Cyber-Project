﻿using Client.MainServer;
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
        private readonly GameMainViewModel mainController;

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

        public GameMainViewModel MainController
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
                selectedRoom = value;
                RoomRequiresPassword = selectedRoom != null ? selectedRoom.Data.HasPassword : false;
                OnPropertyChanged();
                Password = null;
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

        public RoomsFinderViewModel(GameMainViewModel menuController)
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
            else if (e.Update == RoomUpdate.Closed || e.Update == RoomUpdate.Started)
            {
                
                var room = Rooms.FirstOrDefault(r => r.Data.Id == e.UpdatedRoom.Data.Id);
                if (room == null)
                    return;

                Rooms.Remove(room);
                OnPropertyChanged(nameof(Rooms));
                if (SelectedRoom != null && SelectedRoom.Data.Id == e.UpdatedRoom.Data.Id)
                    SelectedRoom = null;
            }
            else if (e.Update == RoomUpdate.UserJoined || e.Update == RoomUpdate.UserLeft)
            {
                var room = Rooms.FirstOrDefault(r => r.Data.Id == e.UpdatedRoom.Data.Id);
                if (room == null)
                    return;

                room.Users = e.UpdatedRoom.Users;
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
            if (rooms != null)
            {
                Rooms.AddRange(rooms);
                OnPropertyChanged(nameof(Rooms));
            }
        }

        #endregion
    }
}
