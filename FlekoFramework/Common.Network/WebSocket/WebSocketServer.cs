using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Flekosoft.Common.Network.Tcp;

namespace Flekosoft.Common.Network.WebSocket
{
    public class WebSocketServer : TcpServerBase
    {
        private readonly Dictionary<EndPoint, EndpointDataParser> _endpointDataParsers = new Dictionary<EndPoint, EndpointDataParser>();

        public WebSocketServer()
        {
            ConnectedEvent += WebSocketServer_ConnectedEvent;
            DisconnectedEvent += WebSocketServer_DisconnectedEvent;
        }

        private void WebSocketServer_DisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            if (_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Remove(e.RemoteEndPoint);
        }

        private void WebSocketServer_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            lock (_endpointDataParsers)
            {
                if (!_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Add(e.RemoteEndPoint, new EndpointDataParser());
            }
        }

        private void ParseHandshake(NetworkDataEventArgs e)
        {
            var parser = _endpointDataParsers[e.RemoteEndPoint];
            parser.NetworkReceivedString += Encoding.UTF8.GetString(e.Data);

            if (parser.NetworkReceivedString.Contains("\r\n"))
            {
                parser.HttpRequest.Add(parser.NetworkReceivedString);
                parser.NetworkReceivedString = string.Empty;
            }

            if (parser.HttpRequest.Count > 0 && parser.HttpRequest[parser.HttpRequest.Count - 1] == "\r\n")
            {
                parser.NetworkReceivedString = string.Empty;

                try
                {


                    bool containsUpgrade = false;
                    foreach (string s in parser.HttpRequest)
                    {
                        if (s == "Upgrade: websocket\r\n")
                        {
                            containsUpgrade = true;
                            break;
                        }
                    }

                    if (containsUpgrade)
                    {
                        string key = string.Empty;
                        foreach (string s in parser.HttpRequest)
                        {
                            if (s.Contains("Sec-WebSocket-Key"))
                            {
                                var strings = s.Split(new[] { ": ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                key = strings[1];
                            }
                        }

                        if (key != string.Empty)
                        {
                            var acceptKey = AcceptKey(key);

                            var retStr = "HTTP/1.1 101 Switching Protocols\r\n";
                            retStr += "Upgrade: websocket\r\n";
                            retStr += "Connection: Upgrade\r\n";
                            retStr += $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";
                            Write(Encoding.UTF8.GetBytes(retStr), e.LocalEndPoint, e.RemoteEndPoint);
                            parser.FirstConnected = false;
                            parser.IsFirstDataByte = true;

                            HandshakeEvent?.Invoke(this, new ConnectionEventArgs(e.LocalEndPoint, e.RemoteEndPoint));
                            return;
                        }
                    }

                    var errorStr = "HTTP/1.1 400 Bad Request\r\n";
                    errorStr += "Content-Type: text/plain\r\n";
                    errorStr += "Content-Length: 0\r\n";
                    errorStr += "Connection: close\r\n\r\n";

                    Write(Encoding.UTF8.GetBytes(errorStr), e.LocalEndPoint, e.RemoteEndPoint);

                }
                catch
                {
                    var errorStr = "HTTP/1.1 400 Bad Request\r\n";
                    errorStr += "Content-Type: text/plain\r\n";
                    errorStr += "Content-Length: 0\r\n";
                    errorStr += "Connection: close\r\n\r\n";

                    Write(Encoding.UTF8.GetBytes(errorStr), e.LocalEndPoint, e.RemoteEndPoint);
                }
            }
        }

        private static string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private string AcceptKey(string key)
        {
            string longKey = key + guid;
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(longKey));
            return Convert.ToBase64String(hashBytes);
        }


        private void ParseData(NetworkDataEventArgs e)
        {
            var parser = _endpointDataParsers[e.RemoteEndPoint];

            foreach (byte b in e.Data)
            {
                if (parser.IsFirstDataByte)
                {
                    parser.DataBuffer.Clear();
                    parser.Fin = 0;
                    parser.Opcode = 0;
                    parser.Mask = 0;
                    parser.PayloadLen1 = 0;
                    parser.PayloadLen = 0;
                    parser.HeaderReceived = false;
                    parser.FrameReceived = false;
                    parser.PayloadLenReceived = false;
                    parser.MaskingKey[0] = 0;
                    parser.MaskingKey[1] = 0;
                    parser.MaskingKey[2] = 0;
                    parser.MaskingKey[3] = 0;
                    parser.PayloadLenLenght = 0;
                    parser.MaskingKeyLenght = 0;
                }

                parser.DataBuffer.AddRange(e.Data);
                parser.IsFirstDataByte = false;

                if (parser.DataBuffer.Count == 2)
                {
                    parser.Fin = (parser.DataBuffer[0] & 0x80) >> 7;
                    parser.Opcode = (parser.DataBuffer[0] & 0x0F);
                    parser.Mask = (b & 0x80) >> 7;
                    parser.PayloadLen1 = (b & 0x7F);
                    if (parser.PayloadLen1 < 126)
                    {
                        parser.PayloadLen = parser.PayloadLen1;
                        parser.PayloadLenReceived = true;
                        parser.PayloadLenLenght = 0;
                    }
                }

                if (parser.PayloadLenReceived == false)
                {
                    if (parser.PayloadLen1 == 126 && parser.DataBuffer.Count == 4)
                    {
                        parser.PayloadLen = (parser.DataBuffer[2] << 8) + parser.DataBuffer[3];
                        parser.PayloadLenReceived = true;
                        parser.PayloadLenLenght = 2;
                        continue;
                    }
                    if (parser.PayloadLen1 == 127 && parser.DataBuffer.Count == 6)
                    {
                        parser.PayloadLen = (parser.DataBuffer[2] << 24) + (parser.DataBuffer[3] << 16) + (parser.DataBuffer[4] << 8) + (parser.DataBuffer[5] << 0);
                        parser.PayloadLenReceived = true;
                        parser.PayloadLenLenght = 4;
                        continue;
                    }
                }
                if (parser.PayloadLenReceived && parser.HeaderReceived == false)
                {
                    if (parser.Mask == 1)
                    {
                        parser.MaskingKeyLenght = 4;
                        if (parser.DataBuffer.Count == 2 + parser.PayloadLenLenght + 4)
                        {
                            parser.MaskingKey[0] = parser.DataBuffer[parser.DataBuffer.Count - 4];
                            parser.MaskingKey[1] = parser.DataBuffer[parser.DataBuffer.Count - 3];
                            parser.MaskingKey[2] = parser.DataBuffer[parser.DataBuffer.Count - 2];
                            parser.MaskingKey[3] = parser.DataBuffer[parser.DataBuffer.Count - 1];
                            parser.HeaderReceived = true;
                        }
                    }
                    else
                    {
                        parser.MaskingKeyLenght = 0;
                        parser.HeaderReceived = true;
                    }
                }
                if (parser.HeaderReceived && parser.DataBuffer.Count == 2 + parser.PayloadLenLenght + parser.MaskingKeyLenght + parser.PayloadLen)
                {
                    if (parser.Mask == 1)
                    {
                        var j = 0;
                        for (int i = 2 + parser.PayloadLenLenght +
                                     parser.MaskingKeyLenght; i < parser.DataBuffer.Count; i++, j++)
                        {
                            parser.DataBuffer[i] = (byte)(parser.DataBuffer[i] ^ parser.MaskingKey[j % 4]);
                        }
                    }
                    ParseFrame(e);
                    parser.IsFirstDataByte = true;
                }
            }
        }

        private void ParseFrame(NetworkDataEventArgs e)
        {
            var data = new List<byte>();
            var parser = _endpointDataParsers[e.RemoteEndPoint];
            var payloadStartIndex = 2 + parser.PayloadLenLenght + parser.MaskingKeyLenght;
            switch ((WebSocketOpcode)parser.Opcode)
            {
                case WebSocketOpcode.Ping:
                    //Ping frame. Sent Pong Frame.
                    data.AddRange(parser.DataBuffer);
                    data[0] = (byte)(data[0] & 0xF0);
                    data[0] = (byte)(data[0] | (byte)WebSocketOpcode.Pong);
                    Write(data.ToArray(), e.LocalEndPoint, e.RemoteEndPoint);
                    break;
                case WebSocketOpcode.ConnectionClose:
                    //Close frame. Sent Close back.
                    var connectionCloseReason = (parser.DataBuffer[payloadStartIndex] << 8) + ((int)parser.DataBuffer[payloadStartIndex + 1] << 0);
                    ConnectionCloseEvent?.Invoke(this, new ConnectionCloseEventArgs((ConnectionCloseReason)connectionCloseReason));
                    SendClose((ConnectionCloseReason)connectionCloseReason, e.LocalEndPoint, e.RemoteEndPoint);
                    break;
                case WebSocketOpcode.BinaryFrame:
                case WebSocketOpcode.TextFrame:
                    DataReceivedEvent?.Invoke(this, new DataReceivedEventArgs(e.LocalEndPoint, e.RemoteEndPoint, (WebSocketOpcode)parser.Opcode, parser.DataBuffer.GetRange(payloadStartIndex, parser.PayloadLen).ToArray()));
                    break;
            }
        }

        private void SendClose(ConnectionCloseReason connectionCloseReason, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            List<byte> data = new List<byte>();
            data.Add(0x80 | (byte)WebSocketOpcode.ConnectionClose);
            data.Add(0x02);
            data.Add((byte)((int)connectionCloseReason >> 8));
            data.Add((byte)connectionCloseReason);
            Write(data.ToArray(), localEndPoint, remoteEndPoint);
        }

        protected override void ProcessDataInternal(NetworkDataEventArgs e)
        {
            lock (_endpointDataParsers)
            {
                if (!_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Add(e.RemoteEndPoint, new EndpointDataParser());
            }

            if (_endpointDataParsers[e.RemoteEndPoint].FirstConnected) ParseHandshake(e);
            else ParseData(e);
        }

        public void SendData(WebSocketOpcode opcode, byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, bool isFinalFrame)
        {
            var buffer = new List<byte>();
            long len = data.LongLength;
            var b1 = (byte)(0x7F & (byte)opcode);
            if (isFinalFrame) b1 = (byte)(b1 | 0x80);
            buffer.Add(b1);
            if (len < 126)
            {
                var b2 = (byte)(0x7F & data.Length);
                buffer.Add(b2);
            }
            else if (len < 0xFFFF)
            {
                buffer.Add(0x7E);
                buffer.Add((byte)((len & 0xFF00) >> 8));
                buffer.Add((byte)((len & 0x00FF) >> 0));
            }
            else if (len < 0x7FFFFFFFFFFFFFFF)
            {
                buffer.Add(0x7F);
                buffer.Add((byte)((len & 0x7F00000000000000) >> 7 * 8));
                buffer.Add((byte)((len & 0x00FF000000000000) >> 6 * 8));
                buffer.Add((byte)((len & 0x0000FF0000000000) >> 5 * 8));
                buffer.Add((byte)((len & 0x000000FF00000000) >> 4 * 8));
                buffer.Add((byte)((len & 0x00000000FF000000) >> 3 * 8));
                buffer.Add((byte)((len & 0x0000000000FF0000) >> 2 * 8));
                buffer.Add((byte)((len & 0x000000000000FF00) >> 1 * 8));
                buffer.Add((byte)((len & 0x00000000000000FF) >> 0 * 8));
            }
            buffer.AddRange(data);

            Write(buffer.ToArray(), localEndPoint, remoteEndPoint);
        }

        public override void Stop()
        {
            var connections = GetConnections();
            foreach (var connection in connections)
            {
                SendClose(ConnectionCloseReason.NormalClose, (IPEndPoint)connection.LocalEndPoint, (IPEndPoint)connection.RemoteEndPoint);
            }
            base.Stop();
        }

        public event EventHandler<ConnectionEventArgs> HandshakeEvent;
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
        public event EventHandler<ConnectionCloseEventArgs> ConnectionCloseEvent;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                HandshakeEvent = null;
                DataReceivedEvent = null;
                ConnectionCloseEvent = null;
            }
            base.Dispose(disposing);
        }
    }

    class EndpointDataParser
    {
        public string NetworkReceivedString = string.Empty;
        public readonly List<string> HttpRequest = new List<string>();
        public bool FirstConnected = true;
        public List<byte> DataBuffer = new List<byte>();
        public bool IsFirstDataByte = true;


        public int Fin;
        public int Opcode;
        public int Mask;
        public byte[] MaskingKey = new byte[4];
        public int MaskingKeyLenght;
        public int PayloadLen1;
        public int PayloadLen;
        public int PayloadLenLenght;
        public bool PayloadLenReceived;
        public bool HeaderReceived;
        public bool FrameReceived;
    }
}
