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

    public class NetworkDataEventArgs : EventArgs
    {
        public NetworkDataEventArgs(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            Data = data;
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
        }

        public byte[] Data { get; }
        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint RemoteEndPoint { get; }
    }

    public class EndPointArgs : EventArgs
    {
        public EndPointArgs(EndPoint endPoint)
        {
            EndPoint = endPoint;
        }
        public EndPoint EndPoint { get; }
    }
}
