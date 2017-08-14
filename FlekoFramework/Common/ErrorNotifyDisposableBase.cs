﻿using System;
using System.IO;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common
{
    public abstract class ErrorNotifyDisposableBase : DisposableBase
    {
        protected ErrorNotifyDisposableBase(string name)
        {
            Name = name;
        }

        protected ErrorNotifyDisposableBase()
        {
            Name = string.Empty;
        }

        public string Name { get; }
        public event EventHandler<ErrorEventArgs> ErrorEvent;
        protected void OnErrorEvent(Exception exception)
        {
            Logger.Instance.AppendException(new Exception(Name, exception));
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
