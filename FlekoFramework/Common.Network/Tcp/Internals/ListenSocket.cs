using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    class ListenSocket : IDisposable
    {
        private Socket _socket;
        private readonly ManualResetEvent _acceptedE;

        public ListenSocket(Socket socket, TcpServerLocalEndpoint tcpServerLocalEndpoint)
        {
            _socket = socket;
            TcpServerLocalEndpoint = tcpServerLocalEndpoint;
            _acceptedE = new ManualResetEvent(false);
            AcceptBeginned = false;
        }

        public bool AcceptBeginned { get; set; }

        public TcpServerLocalEndpoint TcpServerLocalEndpoint { get; }

        public ObservableCollection<SocketAsyncNetworkExchangeDriver> ConnectedSockets { get; } = new ObservableCollection<SocketAsyncNetworkExchangeDriver>();

        public Socket Socket
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
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
                    _acceptedE?.Close();

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

                        foreach (var connection in ConnectedSockets)
                        // ReSharper restore LoopCanBeConvertedToQuery
                        {
                            connection?.Dispose();
                        }
                        ConnectedSockets.Clear();
                    }
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
