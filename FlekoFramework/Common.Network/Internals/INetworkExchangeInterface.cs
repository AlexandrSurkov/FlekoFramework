using System;
using System.Net;

namespace Flekosoft.Common.Network.Internals
{
    public interface INetworkExchangeInterface: IDisposable
    {
        int Read(byte[] data, int timeout);
        int Write(byte[] buffer, int offset, int size, int timeout);
        bool IsConnected { get; }

        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }

        event EventHandler DisconnectedEvent;
    }
}
