using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Tcp;
using Flekosoft.Common.Network.WebSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network.WebSocket
{
    [TestClass]
    public class WebSocketServerTests
    {
        private ConnectionEventArgs _connectionEventArgs;
        private ConnectionEventArgs _disconnectionEventArgs;
        private ConnectionEventArgs _handshakeEventArgs;
        private ConnectionCloseEventArgs _connectionCloseEventArgs;
        private DataReceivedEventArgs _dataReceivedEventArgs;
        private string _networkReceivedString = string.Empty;
        private readonly List<string> _httpRequest = new List<string>();
        private readonly List<byte> _clientFrameData = new List<byte>();
        private bool _handshakeReceived;
        private bool _handshakeTest;


        [TestMethod]
        public void WebSocketServer_HandShake_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.HandshakeEvent += S_HandshakeEvent;

            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            _connectionEventArgs = null;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            var r = new Random((int)DateTime.Now.Ticks);
            List<byte> key = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                key.Add((byte)r.Next(32, 127));
            }
            var keyStr = Convert.ToBase64String(key.ToArray());

            var reqStr = $"GET /Test HTTP/1.1\r\n";
            reqStr += "Upgrade: websocket\r\n";
            reqStr += "Connection: Upgrade\r\n";
            reqStr += $"Sec-WebSocket-Key: {keyStr}\r\n";
            reqStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            _handshakeTest = true;
            _handshakeReceived = false;
            _handshakeEventArgs = null;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            startTime = DateTime.Now;
            while (_handshakeEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            string longKey = keyStr + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(longKey));
            var acceptKey = Convert.ToBase64String(hashBytes);

            Assert.AreEqual(5, _httpRequest.Count);
            Assert.AreEqual("HTTP/1.1 101 Switching Protocols\r\n", _httpRequest[0]);
            Assert.AreEqual("Upgrade: websocket\r\n", _httpRequest[1]);
            Assert.AreEqual("Connection: Upgrade\r\n", _httpRequest[2]);
            Assert.AreEqual($"Sec-WebSocket-Accept: {acceptKey}\r\n", _httpRequest[3]);
            Assert.AreEqual($"\r\n", _httpRequest[4]);


            _disconnectionEventArgs = null;
            c.Stop();
            startTime = DateTime.Now;
            while (_disconnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketServer_HandShake_Fail_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.HandshakeEvent += S_HandshakeEvent;

            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            _connectionEventArgs = null;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            var reqStr = $"123\r\n\r\n";

            _handshakeTest = true;
            _handshakeEventArgs = null;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            Assert.AreEqual(5, _httpRequest.Count);
            Assert.AreEqual("HTTP/1.1 400 Bad Request\r\n", _httpRequest[0]);
            Assert.AreEqual("Content-Type: text/plain\r\n", _httpRequest[1]);
            Assert.AreEqual("Content-Length: 0\r\n", _httpRequest[2]);
            Assert.AreEqual("Connection: close\r\n", _httpRequest[3]);
            Assert.AreEqual("\r\n", _httpRequest[4]);


            _disconnectionEventArgs = null;
            c.Stop();
            startTime = DateTime.Now;
            while (_disconnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketServer_ServerToClientData_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            _handshakeTest = false;
            byte mask;
            long payloadLen;
            byte payloadLen1;

            var data = new List<byte>();

            _clientFrameData.Clear();
            data.Clear();
            s.SendData(WebSocketOpcode.TextFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
            Thread.Sleep(10);
            Assert.AreEqual(data.Count + 2, _clientFrameData.Count);

            var fin = (_clientFrameData[0] & 0x80) >> 7;
            var opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual((byte)WebSocketOpcode.TextFrame, opcode);

            _clientFrameData.Clear();
            data.Clear();
            s.SendData(WebSocketOpcode.BinaryFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, false);
            Thread.Sleep(10);
            Assert.AreEqual(data.Count + 2, _clientFrameData.Count);

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x00, fin);
            Assert.AreEqual((byte)WebSocketOpcode.BinaryFrame, opcode);

            _clientFrameData.Clear();
            data.Clear();
            s.SendData(WebSocketOpcode.ConnectionClose, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
            Thread.Sleep(10);
            Assert.AreEqual(data.Count + 2, _clientFrameData.Count);

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual((byte)WebSocketOpcode.ConnectionClose, opcode);

            _clientFrameData.Clear();
            data.Clear();
            s.SendData(WebSocketOpcode.Pong, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, false);
            Thread.Sleep(10);
            Assert.AreEqual(data.Count + 2, _clientFrameData.Count);

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x00, fin);
            Assert.AreEqual((byte)WebSocketOpcode.Pong, opcode);

            _clientFrameData.Clear();
            data.Clear();
            s.SendData(WebSocketOpcode.ContinuationFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
            Thread.Sleep(10);
            Assert.AreEqual(data.Count + 2, _clientFrameData.Count);

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual((byte)WebSocketOpcode.ContinuationFrame, opcode);

            _clientFrameData.Clear();
            data.Clear();
            s.SendData(WebSocketOpcode.Ping, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, false);
            Thread.Sleep(10);
            Assert.AreEqual(data.Count + 2, _clientFrameData.Count);

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x00, fin);
            Assert.AreEqual((byte)WebSocketOpcode.Ping, opcode);


            for (int i = 0; i < 126; i += 25)
            {
                _clientFrameData.Clear();
                data.Clear();
                for (int j = 0; j < i; j++)
                {
                    data.Add((byte)j);
                }

                s.SendData(WebSocketOpcode.BinaryFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
                startTime = DateTime.Now;
                while (_clientFrameData.Count < data.Count + 2)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalSeconds > 10) Assert.Fail();
                    Thread.Sleep(1);
                }

                fin = (_clientFrameData[0] & 0x80) >> 7;
                opcode = (_clientFrameData[0] & 0x0F);
                mask = (byte)((_clientFrameData[1] & 0x80) >> 7);
                payloadLen = (_clientFrameData[1] & 0x7F);

                Assert.AreEqual(0x01, fin);
                Assert.AreEqual((byte)WebSocketOpcode.BinaryFrame, opcode);
                Assert.AreEqual(0x00, mask);
                Assert.AreEqual(i, payloadLen);
                for (int j = 0; j < data.Count; j++)
                {
                    Assert.AreEqual(data[j], _clientFrameData[j + 2]);
                }
            }

            for (int i = 126; i < 65535; i += 10000)
            {
                _clientFrameData.Clear();
                data.Clear();
                for (int j = 0; j < i; j++)
                {
                    data.Add((byte)j);
                }

                s.SendData(WebSocketOpcode.BinaryFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
                startTime = DateTime.Now;
                while (_clientFrameData.Count < data.Count + 4)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalSeconds > 10) Assert.Fail();
                    Thread.Sleep(1);
                }

                fin = (_clientFrameData[0] & 0x80) >> 7;
                opcode = (_clientFrameData[0] & 0x0F);
                mask = (byte)((_clientFrameData[1] & 0x80) >> 7);
                payloadLen1 = (byte)(_clientFrameData[1] & 0x7F);
                payloadLen = ((_clientFrameData[2] << 8) + _clientFrameData[3]);

                Assert.AreEqual(0x01, fin);
                Assert.AreEqual((byte)WebSocketOpcode.BinaryFrame, opcode);
                Assert.AreEqual(0x00, mask);
                Assert.AreEqual(126, payloadLen1);
                Assert.AreEqual(i, payloadLen);
                for (int j = 0; j < data.Count; j++)
                {
                    Assert.AreEqual(data[j], _clientFrameData[j + 4]);
                }
            }

            _clientFrameData.Clear();
            data.Clear();
            for (int j = 0; j < 65534; j++)
            {
                data.Add((byte)j);
            }

            s.SendData(WebSocketOpcode.BinaryFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
            startTime = DateTime.Now;
            while (_clientFrameData.Count < data.Count + 4)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            mask = (byte)((_clientFrameData[1] & 0x80) >> 7);
            payloadLen1 = (byte)(_clientFrameData[1] & 0x7F);
            payloadLen = ((_clientFrameData[2] << 8) + _clientFrameData[3]);

            Assert.AreEqual(0x01, fin);
            Assert.AreEqual((byte)WebSocketOpcode.BinaryFrame, opcode);
            Assert.AreEqual(0x00, mask);
            Assert.AreEqual(126, payloadLen1);
            Assert.AreEqual(65534, payloadLen);
            for (int j = 0; j < data.Count; j++)
            {
                Assert.AreEqual(data[j], _clientFrameData[j + 4]);
            }

            for (long i = 0xFFFF; i < 0x0000000000FFFFFF; i += 0x0000000000400000)
            {
                _clientFrameData.Clear();
                data.Clear();
                for (int j = 0; j < i; j++)
                {
                    data.Add((byte)j);
                }

                s.SendData(WebSocketOpcode.BinaryFrame, data.ToArray(), (IPEndPoint)s.GetConnections()[0].LocalEndPoint, (IPEndPoint)s.GetConnections()[0].RemoteEndPoint, true);
                startTime = DateTime.Now;
                while (_clientFrameData.Count < data.Count + 10)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalSeconds > 10) Assert.Fail();
                    Thread.Sleep(1);
                }

                fin = (_clientFrameData[0] & 0x80) >> 7;
                opcode = (_clientFrameData[0] & 0x0F);
                mask = (byte)((_clientFrameData[1] & 0x80) >> 7);
                payloadLen1 = (byte)(_clientFrameData[1] & 0x7F);
                payloadLen = ((_clientFrameData[2] << 7 * 8) +
                              (_clientFrameData[3] << 6 * 8) +
                              (_clientFrameData[4] << 5 * 8) +
                              (_clientFrameData[5] << 4 * 8) +
                              (_clientFrameData[6] << 3 * 8) +
                              (_clientFrameData[7] << 2 * 8) +
                              (_clientFrameData[8] << 1 * 8) +
                              (_clientFrameData[9] << 0 * 8));

                Assert.AreEqual(0x01, fin);
                Assert.AreEqual((byte)WebSocketOpcode.BinaryFrame, opcode);
                Assert.AreEqual(0x00, mask);
                Assert.AreEqual(127, payloadLen1);
                Assert.AreEqual(i, payloadLen);
                for (int j = 0; j < (int)data.Count; j++)
                {
                    Assert.AreEqual(data[j], _clientFrameData[j + 10]);
                }
            }


            _disconnectionEventArgs = null;
            c.Stop();
            startTime = DateTime.Now;
            while (_disconnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketServer_ClientToServerData_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.HandshakeEvent += S_HandshakeEvent;
            s.DataReceivedEvent += S_DataReceivedEvent;
            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            var r = new Random((int)DateTime.Now.Ticks);
            List<byte> key = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                key.Add((byte)r.Next(32, 127));
            }
            var keyStr = Convert.ToBase64String(key.ToArray());

            var reqStr = $"GET /Test HTTP/1.1\r\n";
            reqStr += "Upgrade: websocket\r\n";
            reqStr += "Connection: Upgrade\r\n";
            reqStr += $"Sec-WebSocket-Key: {keyStr}\r\n";
            reqStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            _handshakeTest = true;
            _handshakeReceived = false;
            _handshakeEventArgs = null;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            startTime = DateTime.Now;
            while (_handshakeEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }


            _handshakeTest = false;

            _clientFrameData.Clear();

            var data = new List<byte>();
            for (int i = 0; i < 126; i += 25)
            {
                _dataReceivedEventArgs = null;
                data.Clear();
                data.Add(0x80 | (byte)WebSocketOpcode.TextFrame);
                data.Add((byte)(0x80 | (byte)i));

                var maskArr = new byte[] { 0x01, 0x02, 0x03, 0x04 };
                data.AddRange(maskArr);

                var unMaskedData = new List<byte>();
                for (int j = 0; j < i; j++)
                {
                    unMaskedData.Add((byte)j);
                }
                var maskedData = new List<byte>();
                var k = 0;
                for (int j = 0; j < unMaskedData.Count; j++, k++)
                {
                    maskedData.Add((byte)(unMaskedData[j] ^ maskArr[k % 4]));
                }
                data.AddRange(maskedData);
                c.SendData(data.ToArray());
                startTime = DateTime.Now;
                while (_dataReceivedEventArgs == null)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalSeconds > 10) Assert.Fail();
                    Thread.Sleep(1);
                }

                Assert.AreEqual(c.DestinationIpEndPoint, _dataReceivedEventArgs.LocalEndPoint);
                for (int j = 0; j < unMaskedData.Count; j++)
                {
                    Assert.AreEqual(unMaskedData[j], _dataReceivedEventArgs.Data[j]);
                }
            }

            for (int i = 126; i < 0xFFFF; i += 20000)
            {
                _dataReceivedEventArgs = null;
                data.Clear();
                data.Add(0x80 | (byte)WebSocketOpcode.TextFrame);
                data.Add((byte)(0x80 | (byte)126));
                data.Add((byte)(i >> 8));
                data.Add((byte)(i));

                var maskArr = new byte[] { 0x01, 0x02, 0x03, 0x04 };
                data.AddRange(maskArr);

                var unMaskedData = new List<byte>();
                for (int j = 0; j < i; j++)
                {
                    unMaskedData.Add((byte)j);
                }
                var maskedData = new List<byte>();
                var k = 0;
                for (int j = 0; j < unMaskedData.Count; j++, k++)
                {
                    maskedData.Add((byte)(unMaskedData[j] ^ maskArr[k % 4]));
                }
                data.AddRange(maskedData);
                c.SendData(data.ToArray());
                startTime = DateTime.Now;
                while (_dataReceivedEventArgs == null)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalSeconds > 10) Assert.Fail();
                    Thread.Sleep(1);
                }

                Assert.AreEqual(c.DestinationIpEndPoint, _dataReceivedEventArgs.LocalEndPoint);
                for (int j = 0; j < unMaskedData.Count; j++)
                {
                    Assert.AreEqual(unMaskedData[j], _dataReceivedEventArgs.Data[j]);
                }
            }

            for (int i = 0x10000; i < 0x10002; i += 1)
            {
                _dataReceivedEventArgs = null;
                data.Clear();
                data.Add(0x80 | (byte)WebSocketOpcode.TextFrame);
                data.Add((byte)(0x80 | (byte)127));
                data.Add((byte)(i >> 24));
                data.Add((byte)(i >> 16));
                data.Add((byte)(i >> 8));
                data.Add((byte)(i));

                var maskArr = new byte[] { 0x01, 0x02, 0x03, 0x04 };
                data.AddRange(maskArr);

                var unMaskedData = new List<byte>();
                for (int j = 0; j < i; j++)
                {
                    unMaskedData.Add((byte)j);
                }
                var maskedData = new List<byte>();
                var k = 0;
                for (int j = 0; j < unMaskedData.Count; j++, k++)
                {
                    maskedData.Add((byte)(unMaskedData[j] ^ maskArr[k % 4]));
                }
                data.AddRange(maskedData);
                c.SendData(data.ToArray());
                startTime = DateTime.Now;
                while (_dataReceivedEventArgs == null)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalSeconds > 10) Assert.Fail();
                    Thread.Sleep(1);
                }

                Assert.AreEqual(c.DestinationIpEndPoint, _dataReceivedEventArgs.LocalEndPoint);
                for (int j = 0; j < unMaskedData.Count; j++)
                {
                    Assert.AreEqual(unMaskedData[j], _dataReceivedEventArgs.Data[j]);
                }
            }

            c.Dispose();
            s.Dispose();
        }

        private void S_DataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            _dataReceivedEventArgs = e;
        }

        [TestMethod]
        public void WebSocketServer_Ping_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.HandshakeEvent += S_HandshakeEvent;
            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            var r = new Random((int)DateTime.Now.Ticks);
            List<byte> key = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                key.Add((byte)r.Next(32, 127));
            }
            var keyStr = Convert.ToBase64String(key.ToArray());

            var reqStr = $"GET /Test HTTP/1.1\r\n";
            reqStr += "Upgrade: websocket\r\n";
            reqStr += "Connection: Upgrade\r\n";
            reqStr += $"Sec-WebSocket-Key: {keyStr}\r\n";
            reqStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            _handshakeTest = true;
            _handshakeReceived = false;
            _handshakeEventArgs = null;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            startTime = DateTime.Now;
            while (_handshakeEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }


            _handshakeTest = false;
            byte mask;
            long payloadLen;

            _clientFrameData.Clear();

            c.SendData(new byte[] { 0x80 | (byte)WebSocketOpcode.Ping, 0x00 });
            startTime = DateTime.Now;
            while (_clientFrameData.Count < 2)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(2, _clientFrameData.Count);

            var fin = (_clientFrameData[0] & 0x80) >> 7;
            var opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual(0x0A, opcode);

            _clientFrameData.Clear();
            byte len = 0x03;
            c.SendData(new byte[] { 0x80 | (byte)WebSocketOpcode.Ping, len, 0x01, 0x02, 0x03 });

            startTime = DateTime.Now;
            while (_clientFrameData.Count < len + 2)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            fin = (_clientFrameData[0] & 0x80) >> 7;
            opcode = (_clientFrameData[0] & 0x0F);
            mask = (byte)((_clientFrameData[1] & 0x80) >> 7);
            payloadLen = (_clientFrameData[1] & 0x7F);

            Assert.AreEqual(0x01, fin);
            Assert.AreEqual(0x0A, opcode);
            Assert.AreEqual(0x00, mask);
            Assert.AreEqual(3, payloadLen);

            Assert.AreEqual(0x01, _clientFrameData[2]);
            Assert.AreEqual(0x02, _clientFrameData[3]);
            Assert.AreEqual(0x03, _clientFrameData[4]);



            _disconnectionEventArgs = null;
            c.Stop();
            startTime = DateTime.Now;
            while (_disconnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketServer_CloseConnection_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.ConnectionCloseEvent += S_ConnectionCloseEvent;
            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            var r = new Random((int)DateTime.Now.Ticks);
            List<byte> key = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                key.Add((byte)r.Next(32, 127));
            }
            var keyStr = Convert.ToBase64String(key.ToArray());

            var reqStr = $"GET /Test HTTP/1.1\r\n";
            reqStr += "Upgrade: websocket\r\n";
            reqStr += "Connection: Upgrade\r\n";
            reqStr += $"Sec-WebSocket-Key: {keyStr}\r\n";
            reqStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            _handshakeTest = true;
            _handshakeReceived = false;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            _handshakeTest = false;

            _clientFrameData.Clear();
            ConnectionCloseReason closeReason = ConnectionCloseReason.NormalClose;
            List<byte> data = new List<byte>();
            data.Add(0x80 | (byte)WebSocketOpcode.ConnectionClose);
            data.Add(0x02);
            data.Add((byte)((int)closeReason >> 8));
            data.Add((byte)closeReason);
            c.SendData(data.ToArray());

            startTime = DateTime.Now;
            _connectionCloseEventArgs = null;
            while (_clientFrameData.Count < 4)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(closeReason, _connectionCloseEventArgs?.ConnectionCloseReason);
            Assert.AreEqual(4, _clientFrameData.Count);

            var fin = (_clientFrameData[0] & 0x80) >> 7;
            var opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual(0x08, opcode);

            Assert.AreEqual(0x02, _clientFrameData[1]);
            Assert.AreEqual(0x03, _clientFrameData[2]);
            Assert.AreEqual(0xE8, _clientFrameData[3]);

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketServer_CloseConnection_OnStopServerTest()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.ConnectionCloseEvent += S_ConnectionCloseEvent;
            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            var r = new Random((int)DateTime.Now.Ticks);
            List<byte> key = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                key.Add((byte)r.Next(32, 127));
            }
            var keyStr = Convert.ToBase64String(key.ToArray());

            var reqStr = $"GET /Test HTTP/1.1\r\n";
            reqStr += "Upgrade: websocket\r\n";
            reqStr += "Connection: Upgrade\r\n";
            reqStr += $"Sec-WebSocket-Key: {keyStr}\r\n";
            reqStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            _handshakeTest = true;
            _handshakeReceived = false;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            _handshakeTest = false;

            _clientFrameData.Clear();
            s.Stop();
            startTime = DateTime.Now;
            _connectionCloseEventArgs = null;
            while (_clientFrameData.Count < 4)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(4, _clientFrameData.Count);

            var fin = (_clientFrameData[0] & 0x80) >> 7;
            var opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual(0x08, opcode);

            Assert.AreEqual(0x02, _clientFrameData[1]);
            Assert.AreEqual(0x03, _clientFrameData[2]);
            Assert.AreEqual(0xE8, _clientFrameData[3]);

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketServer_CloseConnection_OnDisposeServerTest()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.ConnectionCloseEvent += S_ConnectionCloseEvent;
            s.Start(epList);

            var c = new TcpClient();
            c.DataReceivedEvent += C_DataReceivedEvent;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_connectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            var r = new Random((int)DateTime.Now.Ticks);
            List<byte> key = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                key.Add((byte)r.Next(32, 127));
            }
            var keyStr = Convert.ToBase64String(key.ToArray());

            var reqStr = $"GET /Test HTTP/1.1\r\n";
            reqStr += "Upgrade: websocket\r\n";
            reqStr += "Connection: Upgrade\r\n";
            reqStr += $"Sec-WebSocket-Key: {keyStr}\r\n";
            reqStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            _handshakeTest = true;
            _handshakeReceived = false;
            c.SendData(Encoding.UTF8.GetBytes(reqStr));
            startTime = DateTime.Now;
            while (_handshakeReceived == false)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            _handshakeTest = false;

            _clientFrameData.Clear();
            s.Dispose();
            startTime = DateTime.Now;
            _connectionCloseEventArgs = null;
            while (_clientFrameData.Count < 4)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(4, _clientFrameData.Count);

            var fin = (_clientFrameData[0] & 0x80) >> 7;
            var opcode = (_clientFrameData[0] & 0x0F);
            Assert.AreEqual(0x01, fin);
            Assert.AreEqual(0x08, opcode);

            Assert.AreEqual(0x02, _clientFrameData[1]);
            Assert.AreEqual(0x03, _clientFrameData[2]);
            Assert.AreEqual(0xE8, _clientFrameData[3]);

            c.Dispose();
            s.Dispose();
        }

        private void S_ConnectionCloseEvent(object sender, ConnectionCloseEventArgs e)
        {
            _connectionCloseEventArgs = e;
        }

        private void S_HandshakeEvent(object sender, ConnectionEventArgs e)
        {
            _handshakeEventArgs = e;
        }

        private void C_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            if (_handshakeTest)
            {
                foreach (byte b in e.Data)
                {
                    _networkReceivedString += Encoding.UTF8.GetString(new byte[] { b });

                    if (_networkReceivedString.Contains("\r\n"))
                    {
                        _httpRequest.Add(_networkReceivedString);
                        _networkReceivedString = string.Empty;
                    }

                    if (_httpRequest.Count > 0 && _httpRequest[_httpRequest.Count - 1] == "\r\n")
                    {
                        _networkReceivedString = string.Empty;
                        _handshakeReceived = true;

                    }
                }
            }
            else
            {
                _clientFrameData.AddRange(e.Data);
            }
        }

        private void S_DisconnectedEvent(object sender, Flekosoft.Common.Network.ConnectionEventArgs e)
        {
            _disconnectionEventArgs = e;
        }

        private void S_ConnectedEvent(object sender, Flekosoft.Common.Network.ConnectionEventArgs e)
        {
            _connectionEventArgs = e;
        }
    }
}
