using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using Client.ViewModels;
using System;
using System.ServiceModel;
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
            try
            {
                if (Globals.LoggedUser != null && Connection.Instance.Service.State == CommunicationState.Opened)
                    Connection.Instance.Service?.Logout();
                if (Connection.Instance.Service.State == CommunicationState.Opened)
                    Connection.Instance.Service?.Close();
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
