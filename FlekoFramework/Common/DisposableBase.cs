using System;
using Flekosoft.Common.Serialization;

namespace Flekosoft.Common
{
    public abstract class DisposableBase : LoggingBase, IDisposable, ISerializabe
    {
        #region IDisposable Members
        private bool _disposed;

        public bool IsDisposed
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _disposed; }
        }

        protected bool IsDisposing
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get;
            set;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposing = true;
                Dispose(IsDisposing);
                GC.SuppressFinalize(this);
                IsDisposing = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var serializer in Serializers)
                    {
                        serializer.Dispose();
                    }
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
