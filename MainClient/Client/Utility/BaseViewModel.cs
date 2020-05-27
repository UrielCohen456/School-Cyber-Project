using Client.MainServer;
using Client.Models.Networking;
using System;
using System.ServiceModel;
using System.Windows;

namespace Client.Utility
{
    public abstract class BaseViewModel : ObservableObject, IDisposable
    {
        public virtual void Dispose() { }

        public bool ExecuteFaultableMethod(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (FaultException<OperationFault> faultException)
            {
                ShowFault(faultException);
                return false;
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return false;
            }
        }

        public T ExecuteFaultableMethod<T>(Func<T> func) where T : class
        {
            try
            {
                return func();
            }
            catch (FaultException<OperationFault> faultException)
            {
                ShowFault(faultException);
                return null;
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return null;
            }
        }

        public void ShowFault(FaultException<OperationFault> faultException)
        {
            MessageBox.Show(faultException.Detail.ErrorMessage,
                            $"Error in: {faultException.Detail.Operation}",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }

        public void ShowException(Exception e)
        {
            MessageBox.Show(e.Message,
                            "Error occured",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }
}
