using System;
using System.Net;

namespace Flekosoft.Common.Network
{
    public class ConnectionFailEventArgs : EventArgs
    {
        public ConnectionFailEventArgs(string result)
        {
            ConnectionResult = result;
        }

        public string ConnectionResult { get; }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
        }
        public EndPoint LocalEndPoint { get; }
        public EndPoint RemoteEndPoint { get; }
    }
}
