using DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Ink;

namespace BusinessLayer
{
    public sealed class ServerManager : IServerManager
    {
        private readonly IUsersRepository usersRepository;
        private readonly IFriendsRepository friendsRepository;
        private readonly IMessagesRepository messagesRepository;
        private readonly IWordsRepository wordsRepository;

        private readonly ConcurrentDictionary<int, User> LoggedUsers;
        private readonly ConcurrentDictionary<int, Room> ActiveRooms;
        private readonly ConcurrentDictionary<int, Game> ActiveGames;

        public int UserCount => LoggedUsers.Count();

        public List<User> Users => LoggedUsers.Values.ToList();
        public ServerManager(IUsersRepository usersRepository,
                             IFriendsRepository friendsRepository,
                             IMessagesRepository messagesRepository,
                             IWordsRepository wordsRepository)
        {
            this.usersRepository = usersRepository;
            this.friendsRepository = friendsRepository;
            this.messagesRepository = messagesRepository;
            this.wordsRepository = wordsRepository;
            LoggedUsers = new ConcurrentDictionary<int, User>();
            ActiveRooms = new ConcurrentDictionary<int, Room>();
            ActiveGames = new ConcurrentDictionary<int, Game>();
        }

        #region Authentication Methods

        public User Login(string username, string password)
        {
            if (IsUserConnected(username))
                throw new Exception("User is already logged in");

            var user = usersRepository.SelectSpecificUser(username);

            if (user == null)
                throw new Exception("User doesn't exist");

            if (!user.IsHashedPasswordCorrect(password))
                throw new Exception("Username or password is incorrect");

            LoggedUsers.TryAdd(user.Id, user);

            return user;
        }

        public User Signup(string name, string username, string password)
        {
            if (IsUserConnected(username))
                throw new Exception("User is already logged in");

            var user = usersRepository.SelectSpecificUser(username);
            if (user != null) // User exists
                throw new Exception("User already exists");

            user = usersRepository.AddUser(name, username, password);
            if (user == null) // Couldn't add him
                throw new Exception("Couldn't add user, something went wrong.");

            LoggedUsers.TryAdd(user.Id, user);

            return user;
        }

        public void Logout(User user)
        {
            if (user == null)
                throw new Exception("Can't logout without logging in");

            if (!IsUserConnected(user.Id))
                throw new Exception("User is not logged in");

            if (!LoggedUsers.TryRemove(user.Id, out _))
                throw new Exception("Couldn't remove user, something went wrong");
        }

        public Tuple<int?, int?> GetUserRoomAndGameId(User user)
        {
            int? roomId = null;
            int? gameId = null;

            foreach (var room in ActiveRooms)
            {
                if (IsUserInRoom(user.Id, room.Key))
                    roomId = room.Key;
            }

            foreach (var game in ActiveGames)
            {
                if (IsUserInGame(user, game.Key))
                    gameId = game.Key;
            }
            
            return new Tuple<int?, int?>(roomId, gameId);
        }

        #endregion

        #region User Methods

        public User GetUserById(int id)
        {
            return usersRepository.SelectSpecificUser(id);
        }

        public List<User> GetUsersByQuery(string searchQuery, int userCount = 20)
        {
            return usersRepository.GetUsersByQuery(searchQuery, userCount);
        }

        public bool IsUserConnected(string username)
        {
            return !string.IsNullOrEmpty(username) && LoggedUsers.ToList().Exists(item => item.Value.Username == username);
        }

        public bool IsUserConnected(int userId)
        {
            return LoggedUsers.ToList().Exists(item => item.Value.Id == userId);
        }

        public bool DoesUserExist(string username)
        {
            return usersRepository.SelectSpecificUser(username) != null;
        }

        public bool DoesUserExist(int userId)
        {
            return usersRepository.SelectSpecificUser(userId) != null;
        }

        #endregion

        #region Friend Methods

        public bool DoesFriendExist(int userId1, int userId2)
        {
            return friendsRepository.DoesFriendExist(userId1, userId2);
        }

        public IEnumerable<Friend> GetFriendsOfUserByStatus(int userId, FriendStatus status, int friendsCount = 20)
        {
            return friendsRepository.GetFriendsOfUserByStatus(userId, status, friendsCount);
        }

        public Friend AddFriend(int userId, int friendUserId)
        {
            return friendsRepository.CreateFriend(userId, friendUserId);
        }

        public Friend GetExistingFriend(int userId, int friendUserId)
        {
            return friendsRepository.GetSpecificFriend(userId, friendUserId);
        }

        public void ChangeFriendStatus(Friend friend, FriendStatus newStatus)
        {
            friendsRepository.ChangeFriendStatus(friend, newStatus);
        }

        public Message SaveMessage(int fromId, int toId, string text)
        {
            return messagesRepository.SaveMessage(fromId, toId, text);
        }

        public IEnumerable<Message> GetConversation(int userId1, int userId2, int messagesCount = 50)
        {
            return messagesRepository.GetConversation(userId1, userId2, messagesCount);
        }

        #endregion

        #region Room Methods

        public IEnumerable<Room> GetAllRooms()
        {
            return ActiveRooms.Values;
        }

        public bool DoesRoomExist(int roomId)
        {
            return ActiveRooms.Any(pair => pair.Key == roomId);
        }

        public bool IsUserInRoom(int userId, int roomId)
        {
            var roomExists = ActiveRooms.TryGetValue(roomId, out var room);
            if (!roomExists) return false;

            return room.Users.Any(pair => pair.Key == userId);
        }

        public Room ChangeRoomState(int roomId, RoomState newState)
        {
            var roomExists = ActiveRooms.TryGetValue(roomId, out var room);

            if (!roomExists)
                throw new Exception("Room doesn't exist");
                
            room.Data.State = newState;

            return room;
        }

        public Room CreateRoom(User creator, RoomParameters roomParams)
        {
            if (ActiveRooms.Any(pair => pair.Value.Data.Name == roomParams.RoomName))
                throw new Exception("Room with the same name already exists");
            
            var roomId = ActiveRooms.IsEmpty ? 1 : ActiveRooms.Last().Key + 1;
            var roomData = new RoomData {
                Id = roomId, Name = roomParams.RoomName, MaxPlayersCount = roomParams.MaxPlayersCount,
                Password = roomParams.Password, State = RoomState.Open };

            var room = new Room(creator, roomData, roomParams.Password);

            ActiveRooms.TryAdd(roomId, room);

            return room;
        }

        public Room AddUserToRoom(User userToAdd, int roomId, string password)
        {
            if (!DoesRoomExist(roomId))
                throw new Exception("Room doesn't exist");

            ActiveRooms.TryGetValue(roomId, out var room);

            room.AddUser(userToAdd, password);

            return room;
        }

        public Room RemoveUserFromRoom(User userToRemove, int roomId)
        {
            if (!DoesRoomExist(roomId))
                throw new Exception("Room doesn't exist");

            ActiveRooms.TryGetValue(roomId, out var room);

            var shouldDeleteRoom = room.RemoveUser(userToRemove);
            if (!shouldDeleteRoom)
                return room;
        
            ActiveRooms.TryRemove(roomId, out var _);
            return null;
        }

        #endregion

        #region Game Methods

        public bool IsUserInGame(User user, int gameId)
        {
             var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists) return false;

            return game.Users.Any(u => u.Id == user.Id);
        }

        public int StartGame(User user, int roomId, GameParameters parameters)
        {
            var roomExists = ActiveRooms.TryGetValue(roomId, out var room);
            if (!roomExists)
                throw new Exception("Room doesn't exist");

            if (room.Users.Count < 2)
                throw new Exception("Can only start when there are 2 or more players in a room");

            ActiveRooms.TryRemove(roomId, out var _);

            room.Data.State = RoomState.GameBegun;
            var gameId = ActiveGames.IsEmpty ? 1 : ActiveGames.Last().Key + 1;
            var numberOfWords = parameters.NumberOfRounds * room.Users.Count;
            var words = wordsRepository.GetRandomWords(numberOfWords).ToList();

            var game = new Game(gameId, room, words, parameters.NumberOfRounds);
            
            ActiveGames.TryAdd(gameId, game);

            return gameId;
        }

        public bool RemovePlayerFromGame(User player, int gameId)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            var isGameFinished = game.RemovePlayer(player);
            if (isGameFinished)
                ActiveGames.TryRemove(gameId, out var _);
            
            return isGameFinished;
        }

        public GameInformation GetGameInformation(User player, int gameId)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            return game.GetCurrentRoundInformation(player);
        }

        public AnswerSubmitResult SubmitGuess(User player, int gameId, string guess)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            return game.SubmitGuess(player, guess);
        }

        public void SubmitDraw(User painter, int gameId, StrokeCollection strokes)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            game.SubmitDraw(painter, strokes);
        }

        public List<User> GetGameUsers(int gameId)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            return game.Users;
        }

        public PlayerGameData GetPlayerGameData(User player, int gameId)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            return game.GetPlayersScores().First(score => score.userId == player.Id);
        }

        public List<RevealedLetter> GetGameRevealedLetters(User player, int gameId)
        {
            var gameExists = ActiveGames.TryGetValue(gameId, out var game);
            if (!gameExists)
                throw new Exception("Game doesn't exist");

            return game.RevealedLetters;
        }

        #endregion
    }
}
