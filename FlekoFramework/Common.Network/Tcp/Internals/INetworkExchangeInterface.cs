using System;
using System.Net;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    public interface INetworkExchangeInterface: IDisposable
    {
        int Read(byte[] data, int timeout);
        int Write(byte[] buffer, int offset, int size, int timeout);
        bool IsConnected { get; }

        IPEndPoint LocalEndpoint { get; }
        IPEndPoint RemoteEndpoint { get; }

        event EventHandler DisconnectedEvent;
    }
}
