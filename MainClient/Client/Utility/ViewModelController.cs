namespace Client.Utility
{
    public class ViewModelController : ObservableObject
    {
        public static ViewModelController Instance { get; } = new ViewModelController();
        private ViewModelController() { }

        private BaseViewModel currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => currentViewModel;
            private set
            {
                if (currentViewModel != value)
                {
                    currentViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public static void ChangeViewModel(BaseViewModel viewModel)
        {
            Instance.CurrentViewModel?.Dispose();
            Instance.CurrentViewModel = viewModel;
        }
    }
}
