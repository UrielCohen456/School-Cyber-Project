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

        private string username;

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
        public ICommand SwitchToSignupViewCommand => new RelayCommand(() =>
        {
            // TODO: Change to signup
            ViewModelController.ChangeViewModel(new SignupViewModel());
        });

        /// <summary>
        /// Command that needs to receive a password box and then sends a login request to the server based 
        /// on the username and the password inside the password box
        /// </summary>
        public ICommand LoginCommand => new RelayCommand<PasswordBox>(async (passBox) =>
        {
            try
            {
                var result = await Connection.Instance.Service.LoginAsync(Username, passBox?.Password);
                MessageBox.Show(result.Id.ToString());
                MessageBox.Show(result.Name);
            }
            catch (FaultException<OperationFault> fault)
            {
                MessageBox.Show(fault.Detail.ErrorMessage);
            }
        });

        #endregion

        #region Constructors
        #endregion

        #region Methods
        #endregion
    }
}
