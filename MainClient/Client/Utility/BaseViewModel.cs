using System;

namespace Client.Utility
{
    public abstract class BaseViewModel : ObservableObject, IDisposable
    {
        public virtual void Dispose() { }
    }
}
