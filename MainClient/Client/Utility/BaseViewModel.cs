using Client.MainServer;
using System;
using System.ServiceModel;
using System.Windows;

namespace Client.Utility
{
    public abstract class BaseViewModel : ObservableObject, IDisposable
    {
        public virtual void Dispose() { }

        public void ShowFault(FaultException<OperationFault> faultException)
        {
            MessageBox.Show(faultException.Detail.ErrorMessage,
                            $"Error in: {faultException.Detail.Operation}",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }
}
