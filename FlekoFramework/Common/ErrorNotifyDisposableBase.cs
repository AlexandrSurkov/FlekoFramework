using System;
using System.IO;

namespace Flekosoft.Common
{
    public abstract class ErrorNotifyDisposableBase : DisposableBase
    {
        protected ErrorNotifyDisposableBase(string instanceName)
        {
            InstanceName = instanceName;
        }

        protected ErrorNotifyDisposableBase()
        {
            InstanceName = string.Empty;
        }

        public string InstanceName { get; }
        public event EventHandler<ErrorEventArgs> ErrorEvent;
        protected void OnErrorEvent(Exception exception)
        {
            AppendExceptionLogMessage(new Exception(InstanceName, exception));
            // ReSharper disable once UseNullPropagation
            ErrorEvent?.Invoke(this, new ErrorEventArgs(exception));
        }

        public override string ToString()
        {
            return InstanceName;
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
