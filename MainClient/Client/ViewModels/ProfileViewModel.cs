using Client.MainServer;
using Client.Models.Networking;
using Client.Utility;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        #region Fields

        private UserProfileInfo profileInfo;

        #endregion

        #region Properties

        public UserProfileInfo ProfileInfo
        {
            get => profileInfo;
            set
            {
                profileInfo = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public ProfileViewModel()
        {
            ProfileInfo = ExecuteFaultableMethod( () =>  Connection.Instance.Service.GetProfileInfo());
        }

        #endregion

        #region Methods

        #endregion
    }
}
