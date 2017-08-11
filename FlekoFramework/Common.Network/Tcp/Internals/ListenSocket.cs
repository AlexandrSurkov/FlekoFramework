using System;
using System.Net.Sockets;
using System.Threading;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    class ListenSocket : IDisposable
    {
        private Socket _socket;
        private readonly ManualResetEvent _acceptedE;

        public ListenSocket(Socket socket)
        {
            _socket = socket;
            _acceptedE = new ManualResetEvent(false);
            AcceptBeginned = false;
        }

        public bool AcceptBeginned { get; set; }

        public Socket Socket
        {
            get { return _socket; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_acceptedE != null) _acceptedE.Close();

                    if (_socket != null)
                    {
                        if (_socket.Connected)
                        {
                            try
                            {
                                _socket.Shutdown(SocketShutdown.Both);
                            }
                            // ReSharper disable EmptyGeneralCatchClause
                            catch (Exception)
                            // ReSharper restore EmptyGeneralCatchClause
                            {
                            }
                        }
                        _socket.Close();
                        _socket = null;
                    }
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
