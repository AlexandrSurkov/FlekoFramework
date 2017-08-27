using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network
{
    [TestClass]
    public class TcpServerTests
    {
        [TestMethod]
        public void CreateStartStopDispose()
        {
            var server = new TcpServer();
            server.ErrorEvent += Server_ErrorEvent;
            Assert.IsFalse(server.DataTrace);
            Assert.IsFalse(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);

            ServerPropertyChangedEventArgs = null;
            StoppedEventColled = false;
            StartedEventColled = false;
            Assert.IsNull(ServerPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            //Check just properties and events
            server.Start(new List<TcpServerLocalEndpoint>());
            Assert.IsFalse(server.DataTrace);
            Assert.IsTrue(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);

            Assert.IsNull(ServerPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);
            server.Stop();

            Assert.IsFalse(server.DataTrace);
            Assert.IsFalse(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);

            Assert.IsNull(ServerPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            server.PropertyChanged += Server_PropertyChanged;
            server.StartedEvent += Server_StartedEvent;
            server.StoppedEvent += Server_StoppedEvent;

            //Start with events
            server.Start(new List<TcpServerLocalEndpoint>());
            Assert.IsFalse(server.DataTrace);
            Assert.IsTrue(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);

            Assert.AreEqual("IsStarted", ServerPropertyChangedEventArgs?.PropertyName);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsTrue(StartedEventColled);

            ServerPropertyChangedEventArgs = null;
            StoppedEventColled = false;
            StartedEventColled = false;
            Assert.IsNull(ServerPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            server.Stop();
            Assert.IsFalse(server.DataTrace);
            Assert.IsFalse(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);

            Assert.AreEqual("IsStarted", ServerPropertyChangedEventArgs?.PropertyName);
            Assert.IsTrue(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            ServerPropertyChangedEventArgs = null;
            Assert.IsNull(ServerPropertyChangedEventArgs);
            server.DataTrace = !server.DataTrace;
            Assert.AreEqual("DataTrace", ServerPropertyChangedEventArgs?.PropertyName);

            server.Start(new List<TcpServerLocalEndpoint>());
            server.Start(new List<TcpServerLocalEndpoint>());
            server.Dispose();
            Assert.IsTrue(server.DataTrace);
            Assert.IsFalse(server.IsStarted);
            Assert.IsTrue(server.IsDisposed);

        }

        [TestMethod]
        public void LocalhostServerStartedTests()
        {
            var server = new TcpServer();
            server.ErrorEvent += Server_ErrorEvent;

            server.StartListeningEvent += Server_StartListeningEvent;

            var epList = new List<TcpServerLocalEndpoint>();
            epList.Add(new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1111), 1));
            epList.Add(new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222), 1));
            epList.Add(new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3333), 1));

            EndPointArgs.Clear();
            Assert.AreEqual(0, EndPointArgs.Count);
            server.Start(epList);
            Assert.AreEqual(3, EndPointArgs.Count);
            Assert.AreEqual(epList[0].EndPoint, EndPointArgs[0].EndPoint);
            Assert.AreEqual(epList[1].EndPoint, EndPointArgs[1].EndPoint);
            Assert.AreEqual(epList[2].EndPoint, EndPointArgs[2].EndPoint);


            Assert.IsTrue(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);
            server.Dispose();
            Assert.IsTrue(server.IsDisposed);
            Assert.IsFalse(server.IsStarted);
        }

        [TestMethod]
        public void ConnectDisconnectServerTests()
        {
            var server = new TcpServer();
            server.ErrorEvent += Server_ErrorEvent;

            server.ConnectedEvent += Server_ConnectedEvent;
            server.DisconnectedEvent += Server_DisconnectedEvent;

            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1111);
            var ipEp2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);
            var ipEp3 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3333);

            epList.Add(new TcpServerLocalEndpoint(ipEp1, 1));
            epList.Add(new TcpServerLocalEndpoint(ipEp2, 1));
            epList.Add(new TcpServerLocalEndpoint(ipEp3, 1));

            server.Start(epList);

            var client = new TcpClient();

            ConnectionTest(client, ipEp1);
            ConnectionTest(client, ipEp2);
            ConnectionTest(client, ipEp3);

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client.Start(ipEp1);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp1, ConnectedEventArgs.LocalEndPoint);

            var remoteIp = ConnectedEventArgs.RemoteEndPoint;

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client.Dispose();
            Thread.Sleep(1500);
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNotNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp1, DisconnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(remoteIp, DisconnectedEventArgs.RemoteEndPoint);

            var client1 = new TcpClient();
            var client2 = new TcpClient();
            var client3 = new TcpClient();

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client1.Start(ipEp1);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp1, ConnectedEventArgs.LocalEndPoint);

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client2.Start(ipEp2);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp2, ConnectedEventArgs.LocalEndPoint);

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client3.Start(ipEp3);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp3, ConnectedEventArgs.LocalEndPoint);


            client1.DisconnectedEvent += Client1_DisconnectedEvent;
            client2.DisconnectedEvent += Client2_DisconnectedEvent;
            client3.DisconnectedEvent += Client3_DisconnectedEvent;

            Client1DisconnectedEventColled = false;
            Client2DisconnectedEventColled = false;
            Client3DisconnectedEventColled = false;
            Assert.IsFalse(Client1DisconnectedEventColled);
            Assert.IsFalse(Client2DisconnectedEventColled);
            Assert.IsFalse(Client3DisconnectedEventColled);

            server.Stop();

            Thread.Sleep(1500);

            Assert.IsTrue(Client1DisconnectedEventColled);
            Assert.IsTrue(Client2DisconnectedEventColled);
            Assert.IsTrue(Client3DisconnectedEventColled);



            server.Start(epList);
            Thread.Sleep(1000);

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client1.Start(ipEp1);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp1, ConnectedEventArgs.LocalEndPoint);

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client2.Start(ipEp2);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp2, ConnectedEventArgs.LocalEndPoint);

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client3.Start(ipEp3);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(ipEp3, ConnectedEventArgs.LocalEndPoint);

            Client1DisconnectedEventColled = false;
            Client2DisconnectedEventColled = false;
            Client3DisconnectedEventColled = false;
            Assert.IsFalse(Client1DisconnectedEventColled);
            Assert.IsFalse(Client2DisconnectedEventColled);
            Assert.IsFalse(Client3DisconnectedEventColled);
            Assert.IsTrue(server.IsStarted);
            Assert.IsFalse(server.IsDisposed);
            server.Dispose();

            Thread.Sleep(1500);

            Assert.IsTrue(Client1DisconnectedEventColled);
            Assert.IsTrue(Client2DisconnectedEventColled);
            Assert.IsTrue(Client3DisconnectedEventColled);

            Assert.IsTrue(server.IsDisposed);
            Assert.IsFalse(server.IsStarted);
        }

        [TestMethod]
        public void ConnectionCountTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void SendReceiveTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DataTraceTest()
        {
            Assert.Fail();
        }

        public bool Client3DisconnectedEventColled { get; set; }
        private void Client3_DisconnectedEvent(object sender, System.EventArgs e)
        {
            Client3DisconnectedEventColled = true;
        }

        public bool Client2DisconnectedEventColled { get; set; }
        private void Client2_DisconnectedEvent(object sender, System.EventArgs e)
        {
            Client2DisconnectedEventColled = true;
        }

        public bool Client1DisconnectedEventColled { get; set; }
        private void Client1_DisconnectedEvent(object sender, System.EventArgs e)
        {
            Client1DisconnectedEventColled = true;
        }

        void ConnectionTest(TcpClient client, IPEndPoint endPoint)
        {
            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client.Start(endPoint);
            Thread.Sleep(200);
            Assert.IsNotNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            Assert.AreEqual(endPoint, ConnectedEventArgs.LocalEndPoint);

            var remoteIp = ConnectedEventArgs.RemoteEndPoint;

            ConnectedEventArgs = null;
            DisconnectedEventArgs = null;
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNull(DisconnectedEventArgs);
            client.Stop();
            Thread.Sleep(1500);
            Assert.IsNull(ConnectedEventArgs);
            Assert.IsNotNull(DisconnectedEventArgs);
            Assert.AreEqual(endPoint, DisconnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(remoteIp, DisconnectedEventArgs.RemoteEndPoint);
        }

        public ConnectionEventArgs DisconnectedEventArgs { get; set; }
        private void Server_DisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            DisconnectedEventArgs = e;
        }

        public ConnectionEventArgs ConnectedEventArgs { get; set; }

        private void Server_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            ConnectedEventArgs = e;
        }

        private void Server_StartListeningEvent(object sender, EndPointArgs e)
        {
            EndPointArgs.Add(e);
        }

        public List<EndPointArgs> EndPointArgs { get; } = new List<EndPointArgs>();

        public bool StoppedEventColled { get; set; }

        private void Server_StoppedEvent(object sender, System.EventArgs e)
        {
            StoppedEventColled = true;
        }

        public bool StartedEventColled { get; set; }

        private void Server_StartedEvent(object sender, System.EventArgs e)
        {
            StartedEventColled = true;
        }

        private void Server_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }

        public PropertyChangedEventArgs ServerPropertyChangedEventArgs { get; set; }

        private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ServerPropertyChangedEventArgs = e;
        }
    }
}