using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class SignupViewModel : BaseViewModel
    {
        #region Fields

        private string username;
        private string name;
        
        #endregion

        #region Properties

        /// <summary>
        /// Public binding for the username
        /// </summary>
        public string Username
        {
            get => username;
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Public binding for the name
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Command that switches the view model to a login view model
        /// </summary>
        public ICommand SwitchToLoginViewCommand => new RelayCommand(SwitchToLoginView);

        /// <summary>
        /// Command that needs to receive a password box and then sends an async singup request to the server based
        /// on the username and the password inside the password box
        /// </summary>
        public ICommand SignupCommand => new RelayCommand<PasswordBox>(Signup);

        #endregion

        #region Constructors
        #endregion

        #region Methods

        private void SwitchToLoginView()
        {
            ViewModelController.ChangeViewModel(new LoginViewModel());
        }

        private async void Signup(PasswordBox passwordBox)
        {
            try
            {
                Globals.LoggedUser = await Connection.Instance.Service?.SignupAsync(Name, Username, passwordBox?.Password);
                ViewModelController.ChangeViewModel(new MainViewModel());
            }
            catch (FaultException<OperationFault> fault)
            {
                MessageBox.Show(fault.Detail.ErrorMessage);
            }
        } 

        #endregion
    }
}
