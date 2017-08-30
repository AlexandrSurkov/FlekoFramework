using System;
using System.Net;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Collections.Generic;
using Flekosoft.Common.Network;
using System.Threading;

namespace Flekosoft.UnitTests.Common.Network
{

    class TcpClientTestClass : TcpClient
    {
        public DateTime PollCalledDateTime { get; set; }
        public int PollColledCount { get; set; }
        public bool PollResult { get; set; } = true;
        protected override bool Poll()
        {
            PollCalledDateTime = DateTime.Now;
            PollColledCount++;
            return PollResult;
        }
    }

    [TestClass]
    public class TcpClientTests
    {
        [TestMethod]
        public void CreateStartStopDisposeTest()
        {
            var client = new TcpClient();
            client.ErrorEvent += Client_ErrorEvent;
            Assert.IsFalse(client.DataTrace);
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            ClientPropertyChangedEventArgs = null;
            StoppedEventColled = false;
            StartedEventColled = false;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            //Check just properties and events
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            Assert.IsFalse(client.DataTrace);
            Assert.IsTrue(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);
            client.Stop();

            Assert.IsFalse(client.DataTrace);
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            client.PropertyChanged += Client_PropertyChanged;
            client.StartedEvent += Client_StartedEvent;
            client.StoppedEvent += Client_StoppedEvent;

            //Start with events
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            Assert.IsFalse(client.DataTrace);
            Assert.IsTrue(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.AreEqual("IsStarted", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsTrue(StartedEventColled);

            ClientPropertyChangedEventArgs = null;
            StoppedEventColled = false;
            StartedEventColled = false;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            client.Stop();
            Assert.IsFalse(client.DataTrace);
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.AreEqual("IsStarted", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.IsTrue(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            client.Dispose();
            Assert.IsFalse(client.IsStarted);
            Assert.IsTrue(client.IsDisposed);

        }

        [TestMethod]
        public void PropertiesTest()
        {
            var client = new TcpClient();
            client.ErrorEvent += Client_ErrorEvent;
            client.PropertyChanged += Client_PropertyChanged;

            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            var intVal = client.ConnectInterval + 1;
            client.ConnectInterval = intVal;
            Assert.AreEqual("ConnectInterval", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.AreEqual(intVal, client.ConnectInterval);

            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            intVal = client.PollFailLimit + 1;
            client.PollFailLimit = intVal;
            Assert.AreEqual("PollFailLimit", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.AreEqual(intVal, client.PollFailLimit);

            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            intVal = client.PollInterval + 1;
            client.PollInterval = intVal;
            Assert.AreEqual("PollInterval", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.AreEqual(intVal, client.PollInterval);

            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            intVal = client.ReadBufferSize + 1;
            client.ReadBufferSize = intVal;
            Assert.AreEqual("ReadBufferSize", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.AreEqual(intVal, client.ReadBufferSize);

            client.DataTrace = false;
            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            client.DataTrace = !client.DataTrace;
            Assert.AreEqual("DataTrace", ClientPropertyChangedEventArgs?.PropertyName);

            Assert.IsTrue(client.DataTrace);

            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), client.DestinationIpEndPoint.Address);
            Assert.AreEqual(1234, client.DestinationIpEndPoint.Port);

            client.Dispose();
        }

        [TestMethod]
        public void ConnectDisconnectServerTests()
        {
            var server = new TcpServer();
            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9876);

            epList.Add(new TcpServerLocalEndpoint(ipEp1, 1));

            var client = new TcpClient();
            client.ErrorEvent += Client_ErrorEvent;
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);
            Assert.IsFalse(client.IsConnected);

            client.ConnectedEvent += Client_ConnectedEvent;
            client.DisconnectedEvent += Client_DisconnectedEvent;
            client.ReconnectingEvent += Client_ReconnectingEvent;
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9876));

            ClientReconnectingEventColled = false;
            Assert.IsFalse(ClientReconnectingEventColled);
            Thread.Sleep((int)(1.5 * client.ConnectInterval));
            Assert.IsTrue(ClientReconnectingEventColled);

            ClientConnectedEventArgs = null;
            Assert.IsNull(ClientConnectedEventArgs);
            server.Start(epList);
            Thread.Sleep((int)(1.5 * client.ConnectInterval));
            Assert.IsNotNull(ClientConnectedEventArgs);
            Assert.AreEqual(server.Endpoints[0].EndPoint, ClientConnectedEventArgs.RemoteEndPoint);

            ClientReconnectingEventColled = false;
            Assert.IsFalse(ClientDisconnectedEventCalled);
            client.Stop();
            Thread.Sleep((int)(1.5 * client.PollInterval));
            Assert.IsTrue(ClientDisconnectedEventCalled);

            ClientConnectedEventArgs = null;
            Assert.IsNull(ClientConnectedEventArgs);
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9876));
            Thread.Sleep((int)(1.5 * client.ConnectInterval));
            Assert.IsNotNull(ClientConnectedEventArgs);
            Assert.AreEqual(server.Endpoints[0].EndPoint, ClientConnectedEventArgs.RemoteEndPoint);

            ClientDisconnectedEventCalled = false;
            Assert.IsFalse(ClientDisconnectedEventCalled);
            server.Stop();
            Thread.Sleep(100);
            Assert.IsTrue(ClientDisconnectedEventCalled);

            server.Dispose();
            client.Dispose();
        }

        [TestMethod]
        public void SendReceiveTest()
        {
            var server = new TcpServer();

            server.DataReceivedEvent += Server_DataReceivedEvent;

            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), 1));

            epList.Add(ipEp1);
            server.Start(epList);

            var client = new TcpClient();
            client.DataReceivedEvent += Client_DataReceivedEvent;
            client.Start(ipEp1.EndPoint);
            Thread.Sleep(200);

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                ServerDataReceivedEvent.Clear();
                Assert.AreEqual(0, ServerDataReceivedEvent.Count);
                client.SendData(data);
                Thread.Sleep(100);
                Assert.AreEqual(data.Length, ServerDataReceivedEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], ServerDataReceivedEvent[j].Data[0]);
                    Assert.AreEqual(client.ExchangeInterface.LocalEndPoint, ServerDataReceivedEvent[j].RemoteEndPoint);
                    Assert.AreEqual(client.ExchangeInterface.RemoteEndPoint, ServerDataReceivedEvent[j].LocalEndPoint);
                }
            }

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                ClientDataReceivedEvent.Clear();
                Assert.AreEqual(0, ClientDataReceivedEvent.Count);
                server.Write(data, client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint);
                Thread.Sleep(100);
                Assert.AreEqual(data.Length, ClientDataReceivedEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], ClientDataReceivedEvent[j].Data[0]);
                    Assert.AreEqual(ipEp1.EndPoint, ClientDataReceivedEvent[j].RemoteEndPoint);
                }
            }

            server.Dispose();
        }

        [TestMethod]
        public void DataTraceTest()
        {
            var server = new TcpServer();

            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2345), 1));

            epList.Add(ipEp1);
            server.DataReceivedEvent += Server_DataReceivedEvent;
            server.Start(epList);

            var client = new TcpClient();
            client.DataReceivedEvent += Client_DataReceivedEvent;
            client.ReceiveDataTraceEvent += Client_ReceiveDataTraceEvent;
            client.SendDataTraceEvent += Client_SendDataTraceEvent;

            client.Start(ipEp1.EndPoint);
            Thread.Sleep(200);

            client.DataTrace = false;
            server.DataTrace = false;

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                ServerDataReceivedEvent.Clear();
                ClientSendDataTraceEvent.Clear();
                Assert.AreEqual(0, ServerDataReceivedEvent.Count);
                Assert.AreEqual(0, ClientSendDataTraceEvent.Count);
                client.SendData(data);
                Thread.Sleep(100);
                Assert.AreEqual(data.Length, ServerDataReceivedEvent.Count);
                Assert.AreEqual(0, ClientSendDataTraceEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], ServerDataReceivedEvent[j].Data[0]);
                    Assert.AreEqual(client.ExchangeInterface.LocalEndPoint, ServerDataReceivedEvent[j].RemoteEndPoint);
                    Assert.AreEqual(client.ExchangeInterface.RemoteEndPoint, ServerDataReceivedEvent[j].LocalEndPoint);
                }
            }

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                ClientDataReceivedEvent.Clear();
                ClientReceiveDataTraceEvent.Clear();
                Assert.AreEqual(0, ClientDataReceivedEvent.Count);
                Assert.AreEqual(0, ClientReceiveDataTraceEvent.Count);
                server.Write(data, client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint);
                Thread.Sleep(100);
                Assert.AreEqual(data.Length, ClientDataReceivedEvent.Count);
                Assert.AreEqual(0, ClientReceiveDataTraceEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], ClientDataReceivedEvent[j].Data[0]);
                    Assert.AreEqual(ipEp1.EndPoint, ClientDataReceivedEvent[j].RemoteEndPoint);
                }
            }


            client.DataTrace = true;
            server.DataTrace = true;

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                ServerDataReceivedEvent.Clear();
                ClientSendDataTraceEvent.Clear();
                Assert.AreEqual(0, ServerDataReceivedEvent.Count);
                Assert.AreEqual(0, ClientSendDataTraceEvent.Count);
                client.SendData(data);
                Thread.Sleep(100);
                Assert.AreEqual(data.Length, ServerDataReceivedEvent.Count);
                Assert.AreEqual(1, ClientSendDataTraceEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], ServerDataReceivedEvent[j].Data[0]);
                    Assert.AreEqual(client.ExchangeInterface.LocalEndPoint, ServerDataReceivedEvent[j].RemoteEndPoint);
                    Assert.AreEqual(client.ExchangeInterface.RemoteEndPoint, ServerDataReceivedEvent[j].LocalEndPoint);

                    Assert.AreEqual(data[j], ClientSendDataTraceEvent[0].Data[j]);
                }


                Assert.AreEqual(ipEp1.EndPoint, ClientSendDataTraceEvent[0].RemoteEndPoint);
            }

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                ClientDataReceivedEvent.Clear();
                ClientReceiveDataTraceEvent.Clear();
                Assert.AreEqual(0, ClientDataReceivedEvent.Count);
                Assert.AreEqual(0, ClientReceiveDataTraceEvent.Count);
                server.Write(data, client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint);
                Thread.Sleep(100);
                Assert.AreEqual(data.Length, ClientDataReceivedEvent.Count);
                Assert.AreEqual(1, ClientReceiveDataTraceEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], ClientDataReceivedEvent[j].Data[0]);
                    Assert.AreEqual(ipEp1.EndPoint, ClientDataReceivedEvent[j].RemoteEndPoint);

                    Assert.AreEqual(data[j], ClientReceiveDataTraceEvent[0].Data[j]);
                }

                Assert.AreEqual(ipEp1.EndPoint, ClientReceiveDataTraceEvent[0].RemoteEndPoint);
            }

            server.Dispose();
        }

        [TestMethod]
        public void ConnectIntervalTest()
        {
            var client = new TcpClient();
            client.ErrorEvent += Client_ErrorEvent;
            client.PropertyChanged += Client_PropertyChanged;
            client.ReconnectingEvent += Client_ReconnectingEvent;
            client.ConnectionFailEvent += Client_ConnectionFailEvent;
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));


            ConnectIntervalCheck(client, 100);
            ConnectIntervalCheck(client, 500);
            ConnectIntervalCheck(client, 1000);
            ConnectIntervalCheck(client, 2000);


            client.Dispose();
        }

        [TestMethod]
        public void PollTest()
        {
            var server = new TcpServer();
            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2345), 1));
            epList.Add(ipEp1);
            server.DataReceivedEvent += Server_DataReceivedEvent;
            server.Start(epList);

            var client = new TcpClientTestClass();
            client.ErrorEvent += Client_ErrorEvent;
            client.DisconnectedEvent += Client_DisconnectedEvent;
            client.ConnectedEvent += Client_ConnectedEvent;
            client.Start(ipEp1.EndPoint);

            CheckPollInterval(client, 100);
            CheckPollInterval(client, 500);
            CheckPollInterval(client, 1000);
            CheckPollInterval(client, 2000);

            CheckPollFailCount(client, 1);
            CheckPollFailCount(client, 3);
            CheckPollFailCount(client, 5);
            CheckPollFailCount(client, 10);

            client.Dispose();
            server.Dispose();
        }

        void CheckPollFailCount(TcpClientTestClass client, int count)
        {
            client.PollResult = true;
            client.PollInterval = 100;
            client.PollFailLimit = count;

            client.PollColledCount = 0;
            var waitStart = DateTime.Now;
            while (client.PollColledCount == 0)
            {
                var delta = DateTime.Now - waitStart;
                if(delta.TotalSeconds>5) Assert.Fail("Wait Timeout");
            }

            client.PollColledCount = 0;
            Assert.AreEqual(0, client.PollColledCount);
            client.PollResult = false;
            ClientDisconnectedEventCalled = false;

            waitStart = DateTime.Now;
            while (!ClientDisconnectedEventCalled)
            {
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }

            Assert.AreEqual(count, client.PollFailLimit);
            Assert.AreEqual(count, client.PollColledCount);
        }

        void CheckPollInterval(TcpClientTestClass client, int interval)
        {
            client.PollResult = true;
            client.PollInterval = interval;
            client.PollColledCount = 0;

            var waitStart = DateTime.Now;
            while (client.PollColledCount == 0)
            {
                var dta = DateTime.Now - waitStart;
                if (dta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }
            var now = DateTime.Now;

            client.PollColledCount = 0;
            client.PollCalledDateTime = DateTime.MinValue;
            Assert.AreEqual(interval, client.PollInterval);
            Assert.AreEqual(DateTime.MinValue, client.PollCalledDateTime);
            Assert.AreEqual(0, client.PollColledCount);

            Thread.Sleep((int)(1.5 * client.PollInterval));

            Assert.AreEqual(1, client.PollColledCount);
            Assert.AreNotEqual(DateTime.MinValue, client.PollCalledDateTime);
            var delta = client.PollCalledDateTime - now;
            var minDelta = interval - interval * 0.1;
            var maxDelta = interval + interval * 0.1;
            Assert.IsTrue(minDelta <= delta.TotalMilliseconds);
            Assert.IsTrue(maxDelta >= delta.TotalMilliseconds);
        }

        void ConnectIntervalCheck(TcpClient client, int interval)
        {

            client.ConnectInterval = interval;
            ClientConnectionFailEvent = null;
            Assert.IsNull(ClientConnectionFailEvent);

            var waitStart = DateTime.Now;
            while (ClientConnectionFailEvent == null)
            {
                var dta = DateTime.Now - waitStart;
                if (dta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }
            var now = DateTime.Now;

            ClientReconnectingEventColled = false;
            ClientReconnectingEventColledDateTime = DateTime.MinValue;
            ClientConnectionFailEvent = null;
            Assert.IsFalse(ClientReconnectingEventColled);
            Assert.AreEqual(DateTime.MinValue, ClientReconnectingEventColledDateTime);
            Assert.AreEqual(interval, client.ConnectInterval);
            Assert.IsNull(ClientConnectionFailEvent);

            Thread.Sleep((int)(1.5 * client.ConnectInterval));

            Assert.IsTrue(ClientReconnectingEventColled);
            Assert.AreNotEqual(DateTime.MinValue, ClientReconnectingEventColledDateTime);
            var delta = ClientReconnectingEventColledDateTime - now;
            var minDelta = interval - interval * 0.05;
            var maxDelta = interval + interval * 0.05;
            Assert.IsTrue(minDelta <= delta.TotalMilliseconds);
            Assert.IsTrue(maxDelta >= delta.TotalMilliseconds);
        }

        public ConnectionFailEventArgs ClientConnectionFailEvent { get; set; }
        private void Client_ConnectionFailEvent(object sender, ConnectionFailEventArgs e)
        {
            ClientConnectionFailEvent = e;
        }

        public List<NetworkDataEventArgs> ClientSendDataTraceEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client_SendDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ClientSendDataTraceEvent.Add(e);
        }

        public List<NetworkDataEventArgs> ClientReceiveDataTraceEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client_ReceiveDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ClientReceiveDataTraceEvent.Add(e);
        }

        public List<NetworkDataEventArgs> ClientDataReceivedEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            ClientDataReceivedEvent.Add(e);
        }

        public List<NetworkDataEventArgs> ServerDataReceivedEvent { get; } = new List<NetworkDataEventArgs>();
        private void Server_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            ServerDataReceivedEvent.Add(e);
        }

        public bool ClientReconnectingEventColled { get; set; }
        public DateTime ClientReconnectingEventColledDateTime { get; set; }
        private void Client_ReconnectingEvent(object sender, EventArgs e)
        {
            ClientReconnectingEventColled = true;
            ClientReconnectingEventColledDateTime = DateTime.Now;
        }
        public bool ClientDisconnectedEventCalled { get; set; }
        private void Client_DisconnectedEvent(object sender, EventArgs e)
        {
            ClientDisconnectedEventCalled = true;
        }

        public ConnectionEventArgs ClientConnectedEventArgs { get; set; }

        private void Client_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            ClientConnectedEventArgs = e;
        }

        public bool StoppedEventColled { get; set; }

        private void Client_StoppedEvent(object sender, System.EventArgs e)
        {
            StoppedEventColled = true;
        }

        public bool StartedEventColled { get; set; }

        private void Client_StartedEvent(object sender, System.EventArgs e)
        {
            StartedEventColled = true;
        }

        public PropertyChangedEventArgs ClientPropertyChangedEventArgs { get; set; }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ClientPropertyChangedEventArgs = e;
        }

        private void Client_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }
    }
}
