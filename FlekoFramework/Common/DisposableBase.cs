using System;

namespace Flekosoft.Common
{
    public abstract class DisposableBase : IDisposable
    {
        #region IDisposable Members
        private bool _disposed;

        public bool IsDisposed
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _disposed; }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
