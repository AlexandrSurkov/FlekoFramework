using System;
using Flekosoft.Common.Serialization;

namespace Flekosoft.Common
{
    public abstract class DisposableBase : LoggingSerializableBase, IDisposable, ISerializable
    {
        #region IDisposable Members
        private bool _disposed;

        public bool IsDisposed => _disposed;

        protected bool IsDisposing
        {
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
                    Serializers.Clear();
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
