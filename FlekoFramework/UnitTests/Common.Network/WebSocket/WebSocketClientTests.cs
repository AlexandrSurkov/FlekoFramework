using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Tcp;
using Flekosoft.Common.Network.WebSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network.WebSocket
{
    [TestClass]
    public class WebSocketClientTests
    {
        private ConnectionEventArgs _serverConnectionEventArgs;
        private ConnectionEventArgs _serverDisconnectionEventArgs;
        private ConnectionEventArgs _serverHandshakeEventArgs;
        private ConnectionCloseEventArgs _serverConnectionCloseEventArgs;
        private NetworkDataEventArgs _tcpServerNetworkDataEventArgs;

        private ConnectionEventArgs _clientConnectionEventArgs;
        private bool _clientDisconnectionEventArgs;
        private ConnectionEventArgs _clientHandshakeEventArgs;
        private ConnectionCloseEventArgs _clientConnectionCloseEventArgs;
        private System.IO.ErrorEventArgs _clietErrorEventArgs;

        [TestMethod]
        public void WebSocketClient_HandShake_Test()
        {
            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4447);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new WebSocketServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.HandshakeEvent += S_HandshakeEvent;

            s.Start(epList);

            var c = new WebSocketClient(String.Empty);
            c.ConnectedEvent += C_ConnectedEvent;
            c.DisconnectedEvent += C_DisconnectedEvent;
            c.HandshakeEvent += C_HandshakeEvent;
            c.ErrorEvent += C_ErrorEvent;

            _serverConnectionEventArgs = null;
            _clientConnectionEventArgs = null;
            _serverHandshakeEventArgs = null;
            _clientHandshakeEventArgs = null;
            _clietErrorEventArgs = null;

            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_serverConnectionEventArgs == null || _clientConnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            startTime = DateTime.Now;
            while (_serverHandshakeEventArgs == null || _clientHandshakeEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            _serverDisconnectionEventArgs = null;
            _clientDisconnectionEventArgs = false;
            c.Stop();
            startTime = DateTime.Now;
            while (_serverDisconnectionEventArgs == null || !_clientDisconnectionEventArgs)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            Assert.IsNull(_clietErrorEventArgs);

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketClient_HandShake_Fail_Test()
        {
            var c = new WebSocketClient(String.Empty);
            c.ConnectedEvent += C_ConnectedEvent;
            c.DisconnectedEvent += C_DisconnectedEvent;
            c.HandshakeEvent += C_HandshakeEvent;
            c.ErrorEvent += C_ErrorEvent;

            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4447);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new TcpServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.DataReceivedEvent += tcpS_DataReceivedEvent;
            s.Start(epList);

            _serverConnectionEventArgs = null;
            _clientConnectionEventArgs = null;
            _serverHandshakeEventArgs = null;
            _clientHandshakeEventArgs = null;
            _tcpServerNetworkDataEventArgs = null;
            _clietErrorEventArgs = null;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_serverConnectionEventArgs == null || _clientConnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            var str = Encoding.UTF8.GetString(_tcpServerNetworkDataEventArgs.Data);
            var strarr = str.Split('\r', '\n');
            Assert.AreEqual("GET  HTTP/1.1", strarr[0]);
            Assert.AreEqual("Upgrade: websocket", strarr[2]);
            Assert.AreEqual("Connection: Upgrade", strarr[4]);
            Assert.IsTrue(strarr[6].Contains("Sec-WebSocket-Key:"));
            Assert.AreEqual("Sec-WebSocket-Version: 13", strarr[8]);

            Assert.IsNull(_clietErrorEventArgs);

            s.Write(Encoding.UTF8.GetBytes("123\r\n\r\n"), _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);
            startTime = DateTime.Now;
            while (_clietErrorEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            Assert.AreEqual("Respond format error", _clietErrorEventArgs.GetException().Message);

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketClient_HandShake_Fail_Accept_Test()
        {
            var c = new WebSocketClient(String.Empty);
            c.ConnectedEvent += C_ConnectedEvent;
            c.DisconnectedEvent += C_DisconnectedEvent;
            c.HandshakeEvent += C_HandshakeEvent;
            c.ErrorEvent += C_ErrorEvent;

            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4447);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new TcpServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.DataReceivedEvent += tcpS_DataReceivedEvent;
            s.Start(epList);

            _serverConnectionEventArgs = null;
            _clientConnectionEventArgs = null;
            _serverHandshakeEventArgs = null;
            _clientHandshakeEventArgs = null;
            _tcpServerNetworkDataEventArgs = null;
            _clietErrorEventArgs = null;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_serverConnectionEventArgs == null || _clientConnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            var str = Encoding.UTF8.GetString(_tcpServerNetworkDataEventArgs.Data);
            var strarr = str.Split('\r', '\n');
            Assert.AreEqual("GET  HTTP/1.1", strarr[0]);
            Assert.AreEqual("Upgrade: websocket", strarr[2]);
            Assert.AreEqual("Connection: Upgrade", strarr[4]);
            Assert.IsTrue(strarr[6].Contains("Sec-WebSocket-Key:"));
            Assert.AreEqual("Sec-WebSocket-Version: 13", strarr[8]);

            Assert.IsNull(_clietErrorEventArgs);

            var retStr = "HTTP/1.1 101 Switching Protocols\r\n";
            retStr += "Upgrade: websocket\r\n";
            retStr += "Connection: Upgrade\r\n";
            retStr += $"Sec-WebSocket-Accept: 123\r\n\r\n";

            s.Write(Encoding.UTF8.GetBytes(retStr), _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);
            startTime = DateTime.Now;
            while (_clietErrorEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual("Wrong Accept", _clietErrorEventArgs.GetException().Message);

            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketClient_Poll_Test()
        {
            var c = new WebSocketClient(String.Empty);
            c.ConnectedEvent += C_ConnectedEvent;
            c.DisconnectedEvent += C_DisconnectedEvent;
            c.HandshakeEvent += C_HandshakeEvent;
            c.ErrorEvent += C_ErrorEvent;

            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4447);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new TcpServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.DataReceivedEvent += tcpS_DataReceivedEvent;
            s.Start(epList);

            _serverConnectionEventArgs = null;
            _clientConnectionEventArgs = null;
            _serverHandshakeEventArgs = null;
            _clientHandshakeEventArgs = null;
            _serverDisconnectionEventArgs = null;
            _tcpServerNetworkDataEventArgs = null;
            _clietErrorEventArgs = null;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_serverConnectionEventArgs == null || _clientConnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            var str = Encoding.UTF8.GetString(_tcpServerNetworkDataEventArgs.Data);
            var strarr = str.Split('\r', '\n');
            Assert.IsTrue(strarr[6].Contains("Sec-WebSocket-Key:"));
            var key = strarr[6].Split(':')[1].Trim();

            var retStr = "HTTP/1.1 101 Switching Protocols\r\n";
            retStr += "Upgrade: websocket\r\n";
            retStr += "Connection: Upgrade\r\n";
            retStr += $"Sec-WebSocket-Accept: {AcceptKeyGenerator.AcceptKey(key)}\r\n\r\n";

            s.Write(Encoding.UTF8.GetBytes(retStr), _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);

            _clientHandshakeEventArgs = null;
            startTime = DateTime.Now;
            while (_clientHandshakeEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            _tcpServerNetworkDataEventArgs = null;
            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(0x89, _tcpServerNetworkDataEventArgs.Data[0]);
            Assert.AreEqual(0x00, _tcpServerNetworkDataEventArgs.Data[1]);
            s.Write(new byte[] { 0x8A, 0x00 }, _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);

            _tcpServerNetworkDataEventArgs = null;
            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(0x89, _tcpServerNetworkDataEventArgs.Data[0]);
            Assert.AreEqual(0x00, _tcpServerNetworkDataEventArgs.Data[1]);
            s.Write(new byte[] { 0x8A, 0x00 }, _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);

            _tcpServerNetworkDataEventArgs = null;
            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(0x89, _tcpServerNetworkDataEventArgs.Data[0]);
            Assert.AreEqual(0x00, _tcpServerNetworkDataEventArgs.Data[1]);
            s.Write(new byte[] { 0x8A, 0x00 }, _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);

            Assert.IsNull(_serverDisconnectionEventArgs);
            c.Dispose();
            s.Dispose();
        }

        [TestMethod]
        public void WebSocketClient_PollFailed_Test()
        {
            var c = new WebSocketClient(String.Empty);
            c.ConnectedEvent += C_ConnectedEvent;
            c.DisconnectedEvent += C_DisconnectedEvent;
            c.HandshakeEvent += C_HandshakeEvent;
            c.ErrorEvent += C_ErrorEvent;

            var clientsCount = 1;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4447);
            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(ipep, clientsCount)
            };

            var s = new TcpServer();
            s.ConnectedEvent += S_ConnectedEvent;
            s.DisconnectedEvent += S_DisconnectedEvent;
            s.DataReceivedEvent += tcpS_DataReceivedEvent;
            s.Start(epList);

            _serverConnectionEventArgs = null;
            _clientConnectionEventArgs = null;
            _serverHandshakeEventArgs = null;
            _clientHandshakeEventArgs = null;
            _tcpServerNetworkDataEventArgs = null;
            _serverDisconnectionEventArgs = null;
            _clietErrorEventArgs = null;
            c.Start(ipep);
            var startTime = DateTime.Now;
            while (_serverConnectionEventArgs == null || _clientConnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            var str = Encoding.UTF8.GetString(_tcpServerNetworkDataEventArgs.Data);
            var strarr = str.Split('\r', '\n');
            Assert.IsTrue(strarr[6].Contains("Sec-WebSocket-Key:"));
            var key = strarr[6].Split(':')[1].Trim();

            var retStr = "HTTP/1.1 101 Switching Protocols\r\n";
            retStr += "Upgrade: websocket\r\n";
            retStr += "Connection: Upgrade\r\n";
            retStr += $"Sec-WebSocket-Accept: {AcceptKeyGenerator.AcceptKey(key)}\r\n\r\n";

            s.Write(Encoding.UTF8.GetBytes(retStr), _tcpServerNetworkDataEventArgs.LocalEndPoint, _tcpServerNetworkDataEventArgs.RemoteEndPoint);

            _clientHandshakeEventArgs = null;
            startTime = DateTime.Now;
            while (_clientHandshakeEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }

            _tcpServerNetworkDataEventArgs = null;
            startTime = DateTime.Now;
            while (_tcpServerNetworkDataEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 5) Assert.Fail();
                Thread.Sleep(1);
            }
            Assert.AreEqual(0x89, _tcpServerNetworkDataEventArgs.Data[0]);
            Assert.AreEqual(0x00, _tcpServerNetworkDataEventArgs.Data[1]);

            _serverDisconnectionEventArgs = null;
            startTime = DateTime.Now;
            while (_serverDisconnectionEventArgs == null)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalSeconds > 10) Assert.Fail();
                Thread.Sleep(1);
            }

            c.Dispose();
            s.Dispose();
        }

        private void C_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            _clietErrorEventArgs = e;
        }

        private void tcpS_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            _tcpServerNetworkDataEventArgs = e;
        }

        private void S_DisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            _serverDisconnectionEventArgs = e;
        }

        private void S_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            _serverConnectionEventArgs = e;
        }

        private void S_ConnectionCloseEvent(object sender, ConnectionCloseEventArgs e)
        {
            _serverConnectionCloseEventArgs = e;
        }

        private void S_HandshakeEvent(object sender, ConnectionEventArgs e)
        {
            _serverHandshakeEventArgs = e;
        }

        private void C_DisconnectedEvent(object sender, EventArgs e)
        {
            _clientDisconnectionEventArgs = true;
        }

        private void C_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            _clientConnectionEventArgs = e;
        }

        private void C_ConnectionCloseEvent(object sender, ConnectionCloseEventArgs e)
        {
            _clientConnectionCloseEventArgs = e;
        }

        private void C_HandshakeEvent(object sender, ConnectionEventArgs e)
        {
            _clientHandshakeEventArgs = e;
        }
    }
}
