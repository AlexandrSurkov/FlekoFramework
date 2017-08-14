using System;
using System.IO;

namespace Flekosoft.Common
{
    public abstract class ErrorNotifyDisposableBase : DisposableBase
    {
        public event EventHandler<ErrorEventArgs> ErrorEvent;
        protected void OnErrorEvent(Exception exception)
        {
            // ReSharper disable once UseNullPropagation
            ErrorEvent?.Invoke(this, new ErrorEventArgs(exception));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ErrorEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
