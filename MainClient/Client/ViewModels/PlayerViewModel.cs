using Client.MainServer;
using Client.Utility;

namespace Client.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        #region Fields

        private User player;

        private bool isPainter;

        private int score;

        private int position;

        #endregion

        #region Properties

        public User Player
        {
            get => player;
            set
            {
                player = value;
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

        public int Score
        {
            get => score;
            set
            {
                score = value;
                OnPropertyChanged();
            }
        }

        public int Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public PlayerViewModel(User player, bool isPainter, int score, int position)
        {
            Player = player;
            IsPainter = isPainter;
            Score = score;
            Position = position; 
        }

        #endregion

    }
}
