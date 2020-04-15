using Client.Models.Networking;
using Client.Utility;
using Client.ViewModels;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModelController.Instance;
            ViewModelController.ChangeViewModel(new LoginViewModel()); // TODO: Change to LoginViewModel or something
            Connection.Initalize();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Connection.Instance.Service.State == System.ServiceModel.CommunicationState.Opened)
                Connection.Instance.Service?.Close();
        }
    }
}
