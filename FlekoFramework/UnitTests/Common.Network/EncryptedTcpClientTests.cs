using System;
using System.Net;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Collections.Generic;
using Flekosoft.Common.Network;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;
using System.Security.Cryptography;

namespace Flekosoft.UnitTests.Common.Network
{

    class EncryptedTcpClientTestClass : TcpClient
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

        public X509Certificate ServerCertificate { get; set; }

        protected override bool ValidateServerCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate != null)
                ServerCertificate = new X509Certificate(certificate);
            return true;
        }
    }

    [TestClass]
    public class EncryptedTcpClientTests
    {

        static X509Certificate MakeCert(string commonName)
        {
            using (var rsa = RSA.Create()) // generate asymmetric key pair  
            {
                var req = new CertificateRequest($"cn={commonName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
                var key = cert.PrivateKey;

                var arr = cert.Export(X509ContentType.Pkcs12);
                cert.Dispose();
                X509Certificate2 certRes = new X509Certificate2();
                certRes.Import(arr);
                return certRes;
            }
        }


        [TestMethod]
        public void SendReceiveTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();

                server.DataReceivedEvent += Server_DataReceivedEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), 1));

                epList.Add(ipEp1);
                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                X509Certificate clientCert = MakeCert(serverName);
                var client = new TcpClient();
                client.DataReceivedEvent += Client_DataReceivedEvent;
                client.Start(ipEp1.EndPoint, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                Thread.Sleep(200);

                for (int i = 1; i < 10; i++)
                {
                    var data = BitConverter.GetBytes(i);
                    ServerDataReceivedEvent.Clear();
                    Assert.AreEqual(0, ServerDataReceivedEvent.Count);
                    client.SendData(data);
                    Thread.Sleep(100);
                    Assert.AreEqual(1, ServerDataReceivedEvent.Count);
                    Assert.AreEqual(data.Length, ServerDataReceivedEvent[0].Data.Length);
                    for (int j = 0; j < data.Length; j++)
                    {
                        Assert.AreEqual(data[j], ServerDataReceivedEvent[0].Data[j]);
                        Assert.AreEqual(client.ExchangeInterface.LocalEndPoint, ServerDataReceivedEvent[0].RemoteEndPoint);
                        Assert.AreEqual(client.ExchangeInterface.RemoteEndPoint, ServerDataReceivedEvent[0].LocalEndPoint);
                    }
                }

                for (int i = 1; i < 10; i++)
                {
                    var data = BitConverter.GetBytes(i);
                    ClientDataReceivedEvent.Clear();
                    Assert.AreEqual(0, ClientDataReceivedEvent.Count);
                    server.Write(data, client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint);
                    Thread.Sleep(100);
                    Assert.AreEqual(1, ServerDataReceivedEvent.Count);
                    Assert.AreEqual(data.Length, ServerDataReceivedEvent[0].Data.Length);
                    for (int j = 0; j < data.Length; j++)
                    {
                        Assert.AreEqual(data[j], ClientDataReceivedEvent[0].Data[j]);
                        Assert.AreEqual(ipEp1.EndPoint, ClientDataReceivedEvent[0].RemoteEndPoint);
                    }
                }

                server.Dispose();
                client.Dispose();
                clientCert.Dispose();
            }
        }  

        [TestMethod]
        public void PollTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();
                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0192), 1));
                epList.Add(ipEp1);
                server.DataReceivedEvent += Server_DataReceivedEvent;
                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                X509Certificate clientCert = MakeCert(serverName);
                var client = new TcpClientTestClass();
                client.ErrorEvent += Client_ErrorEvent;
                client.DisconnectedEvent += Client_DisconnectedEvent;
                client.ConnectedEvent += Client_ConnectedEvent;
                client.Start(ipEp1.EndPoint, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                Thread.Sleep(200);

                CheckPollInterval(client, server, 100);
                CheckPollInterval(client, server, 500);
                CheckPollInterval(client, server, 1000);
                CheckPollInterval(client, server, 2000);

                CheckPollFailCount(client, server, 1);
                CheckPollFailCount(client, server, 3);
                CheckPollFailCount(client, server, 5);
                CheckPollFailCount(client, server, 10);

                client.Dispose();
                server.Dispose();
                clientCert.Dispose();
            }
        }

        [TestMethod]
        public void ServertCertCheckTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), 1));

                epList.Add(ipEp1);
                server.Start(epList, serverCert, true, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                X509Certificate clientCert = MakeCert(serverName);
                var client = new EncryptedTcpClientTestClass();

                client.ServerCertificate = null;
                client.Start(ipEp1.EndPoint, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                var waitStart = DateTime.Now;
                while (client.ServerCertificate == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNotNull(client.ServerCertificate);
                Assert.AreEqual(serverCert, client.ServerCertificate);

                client.Stop();
                server.Stop();

                server.Dispose();
                client.Dispose();
                clientCert.Dispose();
            }
        }

        void CheckPollFailCount(TcpClientTestClass client, TcpServer server, int count)
        {
            client.PollResult = true;
            client.PollInterval = 100;
            client.PollFailLimit = count;

            client.PollColledCount = 0;
            var waitStart = DateTime.Now;
            while (client.PollColledCount == 0)
            {
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
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

        void CheckPollInterval(TcpClientTestClass client, TcpServer server, int interval)
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
            var minDelta = interval - interval * 100;
            var maxDelta = interval + interval * 100;
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
            var minDelta = interval - 100;
            var maxDelta = interval + 100;
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
