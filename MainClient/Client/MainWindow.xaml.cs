using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using Client.ViewModels;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread connectionThread;

        public MainWindow()
        {
            InitializeComponent();
            Globals.UIDispatcher = Dispatcher;
            DataContext = ViewModelController.Instance;
            ViewModelController.ChangeViewModel(new LoginViewModel());
            //Connection.Initalize();// TODO: Change to LoginViewModel or something
            connectionThread = new Thread(() => Connection.Initalize());
            connectionThread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ViewModelController.ChangeViewModel(null);

                if (Globals.LoggedUser != null && Connection.Instance.Service?.State == CommunicationState.Opened)
                    Connection.Instance.Service?.Logout();
                Connection.Instance.Service?.Abort();
            }
            catch (FaultException<OperationFault> faultException)
            {
                MessageBox.Show(faultException.Detail.ErrorMessage,
                                $"Error in: {faultException.Detail.Operation}",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                $"Error in:",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
