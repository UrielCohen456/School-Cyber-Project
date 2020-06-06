using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;
using static Client.ViewModels.GameScoreScreenViewModel;

namespace Client.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// Controller
        /// </summary>
        private readonly GameMainViewModel mainController;

        /// <summary>
        /// Timer that ticks and asks the server for updates on the state of the game
        /// </summary>
        private readonly DispatcherTimer timer;

        /// <summary>
        /// The current game's id
        /// </summary>
        private readonly int gameId;

        /// <summary>
        /// All the messages sent by the guessers
        /// </summary>
        private ObservableCollection<string> messages;

        /// <summary>
        /// The guess of the user
        /// </summary>
        private string messageText;

        /// <summary>
        /// The players in the room
        /// </summary>
        private ObservableCollection<PlayerViewModel> players;

        /// <summary>
        /// The current round's info
        /// </summary>
        private GameInformation gameInfo;

        /// <summary>
        /// The remaining time for the round
        /// </summary>
        private int remainingTime;

        /// <summary>
        /// The current word's letters with the revealed letters
        /// </summary>
        private string wordLetters;

        /// <summary>
        /// Indicates whether the user is the painter in this turn
        /// </summary>
        private bool isPainter;

        /// <summary>
        /// The strokes on the board from the painter
        /// </summary>
        private StrokeCollection strokes;

        /// <summary>
        /// Shows the current round out of the total amount of rounds
        /// </summary>
        private string roundText;

        #endregion

        #region Properties

        /// <summary>
        /// The messages sent by the players and such
        /// </summary>
        public ObservableCollection<string> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged();
            }
        }

        public string MessageText
        {
            get => messageText;
            set
            {
                messageText = value;
                OnPropertyChanged();
            }
        }

        public string WordLetters
        {
            get => wordLetters;
            set
            {
                wordLetters = value;
                OnPropertyChanged();
            }
        }

        public int RemainingTime
        {
            get => remainingTime;
            set
            {
                remainingTime = value;
                OnPropertyChanged();
            }
        }

        public int Countdown
        {
            get => remainingTime;
            set
            {
                remainingTime = value;
                OnPropertyChanged();
            }
        }

        public bool IsPainter
        {
            get => isPainter;
            set
            {
                isPainter = value;
                OnPropertyChanged();
            }
        }

        public string RoundText
        {
            get => roundText;
            set
            {
                roundText = value;
                OnPropertyChanged();
            }
        }

        public StrokeCollection Strokes
        {
            get => strokes;
            set
            {
                if (value == null)
                    return;

                strokes = value;
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

        public ICommand SendMessageCommand => new RelayCommand(SendMessage);

        public ICommand LeaveGameCommand => new RelayCommand(LeaveGame);

        #endregion

        #region Constructors
        public GameViewModel(GameMainViewModel mainController, int gameId)
        {
            this.mainController = mainController;
            this.gameId = gameId;
            Messages = new ObservableCollection<string>();
            Players = new ObservableCollection<PlayerViewModel>();
            Timer_Tick(null, null);
            UpdatePlayersList();

            Strokes = new StrokeCollection();
            if (IsPainter)
                Strokes.StrokesChanged += StrokesChanged;

            timer = new DispatcherTimer(DispatcherPriority.Render, Globals.UIDispatcher)
            {
                IsEnabled = true,
                Interval = TimeSpan.FromMilliseconds(900)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            timer.Start();

            Connection.Instance.PlayerAnsweredCorrectlyEvent += OnPlayerAnsweredCorrectly; 
            Connection.Instance.PlayerSubmitedGuessEvent += OnPlayerSubmitedGuess;
            Connection.Instance.BoardChangedEvent += OnBoardChanged;
            Connection.Instance.PlayerLeftTheGameEvent += OnPlayerLeft;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (timer.IsEnabled)
                timer.Stop();

            Connection.Instance.PlayerAnsweredCorrectlyEvent -= OnPlayerAnsweredCorrectly;
            Connection.Instance.PlayerSubmitedGuessEvent -= OnPlayerSubmitedGuess;
            Connection.Instance.BoardChangedEvent -= OnBoardChanged;
            Connection.Instance.PlayerLeftTheGameEvent -= OnPlayerLeft;
        }

        #endregion

        #region Methods

        private void OnPlayerAnsweredCorrectly(object sender, PlayerAnsweredCorrectlyEventArgs e)
        {
            Globals.UIDispatcher.Invoke(() =>
            {
                Messages.Add($"{e.Player.Name} guessed currectly");
            });
        }

        private void OnPlayerSubmitedGuess(object sender, PlayerSubmitedGuessEventArgs e)
        {
            Globals.UIDispatcher.Invoke(() =>
            {
                Messages.Add($"{e.Player.Name}: {e.Guess}");
            });
        }

        private void OnBoardChanged(object sender, BoardChangedEventArgs e)
        {
            Globals.UIDispatcher.Invoke(() =>
            {
                Strokes = new StrokeCollection(e.Strokes);
            });
        }

        private void OnPlayerLeft(object sender, PlayerLeftTheGameEventArgs e)
        {
            var playerVM = Players.FirstOrDefault(vm => vm.Player.Id == e.Player.Id);
            if (playerVM == null)
                return;

            Globals.UIDispatcher.Invoke(() =>
            {
                Players.Remove(playerVM);
            });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var newGameInfo = ExecuteFaultableMethod(() => Connection.Instance.Service.GetGameInformation(gameId) );
            
            if (newGameInfo == null)
                return;

            gameInfo = newGameInfo;

            if (gameInfo.IsGameFinished)
            {
                GameFinished();
                return;
            }

            RemainingTime = gameInfo.RemainingTime > 0 ? gameInfo.RemainingTime : 5 - (-1 * gameInfo.RemainingTime);
            ExtractWordLetters();

            RoundText = $"Round {gameInfo.CurrentRoundNumber} of {gameInfo.NumberOfRounds}";

            // To not allow to draw when round is finished
            if (RemainingTime <= 0)
                IsPainter = false;
            // When the round resets
            if (gameInfo.TurnReset)
            {
                IsPainter = gameInfo.Painter.Id == Globals.LoggedUser.Id;
                Strokes = new StrokeCollection();
                if (IsPainter)
                    Strokes.StrokesChanged += StrokesChanged;
                UpdatePlayersList();
            }
        }
       
        private void StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            Strokes = (StrokeCollection)sender;

            if (IsPainter)
            {
                using (var ms = new MemoryStream())
                {
                    Strokes.Save(ms);
                    ms.Position = 0L;
                    ExecuteFaultableMethod(() => Connection.Instance.Service.SubmitDraw(gameId, ms));
                }
            }
        }

        private void GameFinished()
        {
            timer.Stop();
            UpdatePlayersList();
            mainController.ChangeToGameScoreScreenCommand.Execute(new GameScoreScreenParams(gameId, new ObservableCollection<PlayerViewModel>(Players)));
        }

        private void UpdatePlayersList()
        {
            if (gameInfo == null)
                return;

            var users = ExecuteFaultableMethod(() => Connection.Instance.Service.GetAllPlayers(gameId));
            var scores = ExecuteFaultableMethod(() => Connection.Instance.Service.GetScores(gameId));

            var players = (from score in scores
                           from user in users
                           where user.Id == score.userId
                           orderby score.score
                           select new PlayerViewModel(
                               user,
                               gameInfo.Painter == null ? false : gameInfo.Painter.Id == user.Id,
                               score.score,
                               0)).ToList();
            for (var i = 0; i < users.Count; i++)
            {
                players[i].Position = i;
            }

            Globals.UIDispatcher.Invoke(() =>
            {
                Players.Clear();
                players.ForEach(player => Players.Add(player));
            });
        }

        private void ExtractWordLetters()
        {
            var builder = new StringBuilder();

            builder.Append('_', gameInfo.CurrentWordLength);
            foreach (var revealedLetter in gameInfo.RevealedLetters)
            {
                builder.Remove(revealedLetter.LetterIndex, 1)
                       .Insert(revealedLetter.LetterIndex, revealedLetter.Letter);
            }

            WordLetters = builder.ToString();
        }

        private void SendMessage()
        {

            if (string.IsNullOrEmpty(MessageText))
                return;

            var result = default(AnswerSubmitResult);
            var submitted = ExecuteFaultableMethod(() => result = Connection.Instance.Service.SubmitGuess(gameId, MessageText));
            if (!submitted)
                return;

            if (result == AnswerSubmitResult.Right)
                Messages.Add($"{MessageText} was the right answer!");
            else if (result == AnswerSubmitResult.Wrong)
                Messages.Add(MessageText);

            MessageText = null;
        }

        private void LeaveGame()
        {
            ExecuteFaultableMethod(() => Connection.Instance.Service.LeaveGame(gameId));
            mainController.ChangeToRoomFinderViewCommand.Execute(null);
        }

        #endregion
    }
}
