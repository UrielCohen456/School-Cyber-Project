using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Ink;

namespace DataLayer
{
    [DataContract]
    public class GameInformation
    {
        /// <summary>
        /// Remaining time for this turn
        /// </summary>
        [DataMember]
        public int RemainingTime { get; set; }

        /// <summary>
        /// The current round's number
        /// </summary>
        [DataMember]
        public int CurrentRoundNumber { get; set; }

        /// <summary>
        /// Number of rounds for this game
        /// </summary>
        [DataMember]
        public int NumberOfRounds { get; set; }

        /// <summary>
        /// The current turn's painter
        /// </summary>
        [DataMember]
        public User Painter { get; set; }

        /// <summary>
        /// The current turn's length
        /// </summary>
        [DataMember]
        public int CurrentWordLength { get; set; }

        /// <summary>
        /// Indicates that a turn has been reset (other painter chosen)
        /// </summary>
        [DataMember]
        public bool TurnReset { get; set; }

        /// <summary>
        /// The currently revealed letters (Or the whole word if the player guess the word / is the painter)
        /// </summary>
        [DataMember]
        public List<RevealedLetter> RevealedLetters { get; set; }

        /// <summary>
        /// Indicates that the game is finished
        /// </summary>
        [DataMember]
        public bool IsGameFinished { get; set; }
    }

    [DataContract]
    public class GameParameters
    {
        [DataMember]
        public int NumberOfRounds { get; set; }
    }

    public class Game
    {
        #region Fields

        /// <summary>
        /// How much time there is to guess the word
        /// </summary>
        private const int GuessTime = 91;

        /// <summary>
        /// The time at which to reset the round (needs to be below 0 to give time to show score and such)
        /// </summary>
        private const int ResetTime = -5;

        /// <summary>
        /// When to reveal the first letter
        /// </summary>
        private const int RevealFirstLetterTime = 60;

        /// <summary>
        /// When to reveal the second letter
        /// </summary>
        private const int RevealSecondLetterTime = 30;

        /// <summary>
        /// The room that the game was created from (Holds data about the room)
        /// </summary>
        private readonly Room gameRoom;

        /// <summary>
        /// The number of rounds for this game
        /// </summary>
        private readonly int numberOfRounds;

        /// <summary>
        /// The dictionary of the players id to its score
        /// </summary>
        private readonly ConcurrentDictionary<int, PlayerGameData> playersData;

        /// <summary>
        /// Lock for synchronizing access to the players dictionary (Because we change the value of the objects so its not thread safe without the lock)
        /// </summary>
        private static readonly object playersLock = new object();

        /// <summary>
        /// The words that players have to guess (The first is always the current word)
        /// </summary>
        private readonly ConcurrentQueue<Word> words;

        /// <summary>
        /// The thread that runs the game loop that changes the current word when time is up
        /// </summary>
        private readonly Timer mainGameTimer;

        /// <summary>
        /// The index of the current user that is painting the current word
        /// </summary>
        private int currentPainterId;

        /// <summary>
        /// Holds the current word
        /// </summary>
        private Word currentWord;

        /// <summary>
        /// To make sure the game doesnt think the round is over while the other thread checks if it is
        /// </summary>
        private object roundFinishedLock = new object();

        /// <summary>
        /// True when all players answered currectly so need to move to other round
        /// </summary>
        private bool isTurnFinished;

        /// <summary>
        /// The id's of the players who painted this round
        /// </summary>
        private List<int> currentRoundPainters;

        /// <summary>
        /// The current round
        /// </summary>
        private int currentRoundNumber;

        private User winner = null;

        #endregion

        #region Properties

        /// <summary>
        /// The game's id to identify the game
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The amount of time left for the word
        /// </summary>
        public int RemainingTime { get; private set; }

        /// <summary>
        /// Indicates whether the game has finished or not
        /// </summary>
        public bool IsGameFinished { get; private set; }

        /// <summary>
        /// Access to the current painter User class
        /// </summary>
        public User CurrentPainter
        {
            get
            {
                lock (playersLock)
                {
                    if (!gameRoom.Users.Any(pair => pair.Key == currentPainterId))
                        return null;

                    try
                    {
                        return gameRoom.Users[currentPainterId]; 
                    }
                    catch { return null; }
                }
            }
        }

        /// <summary>
        /// The currently revealed letters (One letter revealed at a set time)
        /// </summary>
        public List<RevealedLetter> RevealedLetters { get; private set; }

        /// <summary>
        ///  The strokes that the main player draws, will update each time he draws
        /// </summary>
        public StrokeCollection DrawingBoard { get; private set; }

        public List<User> Users 
        {
            get
            {
                lock (playersLock)
                {
                    return gameRoom.Users.Values.ToList();
                }
            }
        }

        #endregion

        #region Constructors

        public Game(int id, Room gameRoom, List<Word> words, int numberOfRounds)
        {
            Id = id;
            this.gameRoom = gameRoom;
            this.numberOfRounds = numberOfRounds;
            this.currentRoundNumber = 1;
            this.words = new ConcurrentQueue<Word>(words);
            currentRoundPainters = new List<int>();

            playersData = new ConcurrentDictionary<int, PlayerGameData>();
            foreach (var pair in gameRoom.Users)
            {
                playersData.TryAdd(pair.Key, new PlayerGameData { GuessedCurrentWord = false, score = 0, userId = pair.Key });
            }
            currentPainterId = gameRoom.Admin.Id;
            currentRoundPainters.Add(CurrentPainter.Id);

            IsGameFinished = false;
            RemainingTime = GuessTime;
            RevealedLetters = new List<RevealedLetter>();
            this.words.TryDequeue(out var word);
            currentWord = word;

            mainGameTimer = new Timer(GameTickCallback, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the information about the current round as it is
        /// </summary>
        /// <returns></returns>
        public GameInformation GetCurrentRoundInformation(User player)
        {
            lock (playersLock)
            {
                var guessedWord = playersData.First(pair => pair.Key == player.Id).Value.GuessedCurrentWord;
                var revealAllLetters = guessedWord || CurrentPainter.Id == player.Id;

                var info = new GameInformation
                {
                    Painter = CurrentPainter,
                    CurrentRoundNumber = currentRoundNumber,
                    NumberOfRounds = numberOfRounds,
                    RemainingTime = RemainingTime,
                    CurrentWordLength = currentWord != null ? currentWord.Text.Length : 0,
                    RevealedLetters = revealAllLetters ? GetAllCurrentWordLetters() : RevealedLetters,
                    TurnReset = RemainingTime == GuessTime - 1,
                    IsGameFinished = IsGameFinished,
                };

                return info;
            }
        }

        /// <summary>
        /// Tells whether the game is empty
        /// </summary>
        /// <returns></returns>
        public bool IsGameEmpty()
        {
            return gameRoom.Users.IsEmpty;
        }

        /// <summary>
        /// Removes a player from the game
        /// </summary>
        /// <param name="playerId"></param>
        public bool RemovePlayer(User player)
        {
            lock (playersLock)
            {
                if (player.Id == CurrentPainter.Id)
                    isTurnFinished = true;

                if (currentRoundPainters.Any(id => id == player.Id))
                    currentRoundPainters.Remove(player.Id);

                while (!gameRoom.Users.TryRemove(player.Id, out var _)) ;

                PlayerGameData gameData;
                while (!playersData.TryRemove(player.Id, out gameData)) ;

                if (gameRoom.Users.Count < 2)
                    IsGameFinished = true;

                player.TotalScore += gameData.score;
                if (player.HighestScore < gameData.score) player.HighestScore = gameData.score;
                if (IsGameFinished) player.GamesPlayed += 1;

                if (winner != null)
                {
                    if (winner.Id == player.Id)
                        player.GamesWon += 1;
                    else
                        player.GamesLost += 1;
                }

                return gameRoom.Users.Count == 0;
            }
        }

        /// <summary>
        /// Gets the scores at this point of the game
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PlayerGameData> GetPlayersScores()
        {
            lock (playersLock)
            {
                return playersData.Values.AsEnumerable();
            }
        }
        
        /// <summary>
        /// Updates the drawing board (Only possible if the user is the painter)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newBoard"></param>
        public void SubmitDraw(User user, StrokeCollection newBoard)
        {
            if (user.Id != CurrentPainter.Id)
                throw new Exception("Only the current painter can paint");

            DrawingBoard = newBoard;
        }

        /// <summary>
        /// Submits a guess from a player
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="guess"></param>
        /// <returns></returns>
        public AnswerSubmitResult SubmitGuess(User player, string guess)
        {
            if (IsGameFinished)
                return AnswerSubmitResult.GameFinished;

            if (RemainingTime <= 0)
                return AnswerSubmitResult.TimesUp;

            lock (playersLock)
            {
                if (!playersData.TryGetValue(player.Id, out var userData))
                    throw new Exception("User is not in the game");

                if (userData.userId == CurrentPainter.Id)
                    throw new Exception("Painter can't guess the word");

                if (userData.GuessedCurrentWord)
                    return AnswerSubmitResult.AnsweredAlready; 

                if (currentWord.IsGuessCorrect(guess))
                {
                    userData.GuessedCurrentWord = true;

                    PlayerGameData painterData = null;
                    // Calculate the ratio between the remaining players and amount of players
                    var remainingPlayersRatio = 0.0f;
                    foreach (var pair in playersData)
                    {
                        if (pair.Key == currentPainterId)
                            painterData = pair.Value;
                        else
                            remainingPlayersRatio += pair.Value.GuessedCurrentWord ? 1 : 0;
                    }
                    remainingPlayersRatio /= playersData.Count;
                    var addedScore = RemainingTime * (int)Math.Ceiling(remainingPlayersRatio);

                    userData.AddScore(addedScore);
                    painterData?.AddScore(addedScore / 4);

                    return AnswerSubmitResult.Right;
                }

                return AnswerSubmitResult.Wrong;
            }
        }

        /// <summary>
        /// The main game thread, ticks approximately each second
        /// </summary>
        /// <param name="state"></param>
        private void GameTickCallback(object state)
        {
            // Stop calling the callback
            if (IsGameFinished)
            {
                mainGameTimer.Change(Timeout.Infinite, Timeout.Infinite);
                lock(playersLock)
                {
                    this.winner = GetWinner();
                }

                return;
            }

            lock (playersLock)
            {
                // Checking if the round is finished
                var allAnsweredCurrectly = true;
                foreach (var pair in playersData)
                {
                    if (pair.Key == CurrentPainter.Id)
                        continue;

                    if (!pair.Value.GuessedCurrentWord)
                        allAnsweredCurrectly = false;
                }

                if (RemainingTime == RevealFirstLetterTime || RemainingTime == RevealSecondLetterTime)
                {
                    RevealLetter();
                }

                // if the guessing time is finished 
                else if (RemainingTime == 0 || allAnsweredCurrectly || isTurnFinished)
                {
                    isTurnFinished = false;
                    RevealedLetters = GetAllCurrentWordLetters();
                    RemainingTime = 0;

                    // Resetting the guessed current word stat
                    foreach (var pair in playersData)
                        pair.Value.GuessedCurrentWord = false;
                }
                else if (RemainingTime == ResetTime)
                {
                    ResetTurn();
                }

                RemainingTime--;
            }
        }


        /// <summary>
        /// Reveales the whole word in the RevealedLetters list
        /// </summary>
        private List<RevealedLetter> GetAllCurrentWordLetters()
        {
            if (currentWord == null)
                return null;

            var letters = new List<RevealedLetter>();

            for (var i = 0; i < currentWord.Text.Length; i++)
            {
                letters.Add(new RevealedLetter
                {
                    Letter = currentWord.Text[i],
                    LetterIndex = i
                });
            }

            return letters;
        }

        /// <summary>
        /// Reveales a single letter that hasnt been revealed yet
        /// </summary>
        private void RevealLetter()
        {
            var rand = new Random();
            var index = rand.Next(currentWord.Text.Length);
            var letter = currentWord.Text[index];

            while (RevealedLetters.Any(rl => rl.LetterIndex == index))
            {
                index = rand.Next(currentWord.Text.Length);
                letter = currentWord.Text[index];
            }

            RevealedLetters.Add(new RevealedLetter() { Letter = letter, LetterIndex = index });
        }

        /// <summary>
        /// Resets the turn and picks a new painter
        /// </summary>
        private void ResetTurn()
        {
            if (words.IsEmpty)
            {
                IsGameFinished = true;
                currentWord = null;
                return;
            }

            // Needs to reset round
            if (currentRoundPainters.Count == gameRoom.Users.Count)
            {
                currentRoundNumber++;
                currentRoundPainters.Clear();
            }

            var rand = new Random();
            int newPainterId;
            
            while (true)
            {
                var index = rand.Next(0, gameRoom.Users.Count);
                newPainterId = gameRoom.Users.ToList().ElementAt(index).Key;
                
                if (newPainterId != currentPainterId &&
                    !currentRoundPainters.Any(id => id == newPainterId))
                    break;
            }
            currentPainterId = newPainterId;
            currentRoundPainters.Add(currentPainterId);

            words.TryDequeue(out var word);
            currentWord = word;
            RevealedLetters.Clear();
            RemainingTime = GuessTime;
        }

        /// <summary>
        /// Gets the winner (doesnt lock the players data)
        /// </summary>
        /// <returns></returns>
        private User GetWinner()
        {
            var highestScore = -1;
            User winner = null;

            foreach (var pair in playersData)
            {
                if (pair.Value.score > highestScore)
                {
                    highestScore = pair.Value.score;
                    winner = gameRoom.Users[pair.Key];
                }
            }

            return winner;
        }

        #endregion
    }
}
