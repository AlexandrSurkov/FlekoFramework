using System.Net;

namespace Flekosoft.Common.Network.WebSocket
{
    public enum WebSocketOpcode : byte
    {
        ContinuationFrame = 0x00,
        TextFrame = 0x01,
        BinaryFrame = 0x02,
        ConnectionClose = 0x08,
        Ping = 0x09,
        Pong = 0x0A
    }

    public class DataReceivedEventArgs : ConnectionEventArgs
    {
        public DataReceivedEventArgs(EndPoint localEndPoint, EndPoint remoteEndPoint, WebSocketOpcode opcode, byte[] data) : base(localEndPoint, remoteEndPoint)
        {
            Opcode = opcode;
            Data = data;
        }

        public WebSocketOpcode Opcode { get; }
        public byte[] Data { get; }
    }
}
