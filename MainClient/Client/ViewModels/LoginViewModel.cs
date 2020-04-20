using Client.Utility;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Models.Networking;
using System.Windows;
using System.ServiceModel;
using Client.MainServer;

namespace Client.ViewModels
{
    //#region Fields
    //#endregion

    //#region Properties
    //#endregion

    //#region Constructors
    //#endregion

    //#region Methods
    //#endregion

    public class LoginViewModel : BaseViewModel
    {
        #region Fields

        private string username = "Uriel1";

        #endregion

        #region Properties

        /// <summary>
        /// Binding to the username for logging in
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
        /// Command that switches the view model to a signup view model
        /// </summary>
        public ICommand SwitchToSignupViewCommand => new RelayCommand(SwitchToSignupView);

        /// <summary>
        /// Command that needs to receive a password box and then sends a login request to the server based 
        /// on the username and the password inside the password box
        /// </summary>
        public ICommand LoginCommand => new RelayCommand<PasswordBox>(Login);

        public ICommand Idan1 => new RelayCommand<PasswordBox>((passBox) => {
            Username = "Idan1";
            passBox.Password = "Idan1";
        });
        public ICommand Uriel1 => new RelayCommand<PasswordBox>((passBox) => {
            Username = "Uriel1";
            passBox.Password = "Uriel1";
        });


        #endregion

        #region Constructors
        #endregion

        #region Methods

        private void SwitchToSignupView()
        {
            ViewModelController.ChangeViewModel(new SignupViewModel());
        }

        private async void Login(PasswordBox passwordBox)
        {
            try
            {
                Globals.LoggedUser = await Connection.Instance.Service.LoginAsync(Username, passwordBox?.Password);
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
