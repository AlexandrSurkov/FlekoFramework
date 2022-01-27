using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network
{
    [TestClass]
    public class EncryptedTcpServerTests
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
        public void CreateStartStopDisposeTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var epList = new List<TcpServerLocalEndpoint>();
                epList.Add(new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4321), 1));

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
                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
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
                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
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

                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                server.Dispose();
                Assert.IsTrue(server.DataTrace);
                Assert.IsFalse(server.IsStarted);
                Assert.IsTrue(server.IsDisposed);
            }
        }


        [TestMethod]
        public void EncryptedServerVsUnencryptedClientConnectDisconnectTests()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();
                server.ErrorEvent += Server_NonFailErrorEvent;

                server.ConnectedEvent += Server_ConnectedEvent;
                server.DisconnectedEvent += Server_DisconnectedEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7483);

                epList.Add(new TcpServerLocalEndpoint(ipEp1, 1));

                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                var client = new TcpClient();
                client.ConnectedEvent += Client_ConnectedEvent;
                client.DisconnectedEvent += Client_DisconnectedEvent;
                client.ConnectionFailEvent += Client_ConnectionFailEvent;

                ServerConnectedEventArgs = null;
                ServerDisconnectedEventArgs = null;
                ClientConnectedEventEventArgs = null;
                ServerErrorEvent = null;
                ConnectionFailEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                Assert.IsNull(ClientConnectedEventEventArgs);
                Assert.IsNull(ServerErrorEvent);
                Assert.IsNull(ConnectionFailEventArgs);
                Assert.IsFalse(ClientDisconnectedEventColled);

                client.Start(ipEp1);

                var waitStart = DateTime.Now;
                while (ServerErrorEvent == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 20) Assert.Fail("Wait Timeout");
                }

                Assert.IsNotNull(ServerErrorEvent);
                Assert.IsNull(ConnectionFailEventArgs);
                Assert.IsNotNull(ClientConnectedEventEventArgs);
                Assert.IsTrue(ClientDisconnectedEventColled);
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);

                server.Dispose();
                client.Dispose();
            }
        }

        [TestMethod]
        public void ConnectDisconnectServerTests()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();
                server.ErrorEvent += Server_ErrorEvent;

                server.ConnectedEvent += Server_ConnectedEvent;
                server.DisconnectedEvent += Server_DisconnectedEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
                var ipEp2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
                var ipEp3 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);

                epList.Add(new TcpServerLocalEndpoint(ipEp1, 1));
                epList.Add(new TcpServerLocalEndpoint(ipEp2, 1));
                epList.Add(new TcpServerLocalEndpoint(ipEp3, 1));

                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                var client = new TcpClient();
                client.ConnectedEvent += Client_ConnectedEvent;
                client.DisconnectedEvent += Client_DisconnectedEvent;

                X509Certificate clientCert1 = MakeCert(serverName);
                X509Certificate clientCert2 = MakeCert(serverName);
                X509Certificate clientCert3 = MakeCert(serverName);

                ConnectionTest(client, ipEp1, serverName, clientCert1, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                ConnectionTest(client, ipEp2, serverName, clientCert2, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                ConnectionTest(client, ipEp3, serverName, clientCert3, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                while (server.GetConnections().Count != 0) { };

                ServerConnectedEventArgs = null;
                ServerDisconnectedEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                client.Start(ipEp1, serverName, clientCert1, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                var waitStart = DateTime.Now;
                while (ServerConnectedEventArgs == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNotNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                Assert.AreEqual(ipEp1, ServerConnectedEventArgs.LocalEndPoint);

                var remoteIp = ServerConnectedEventArgs.RemoteEndPoint;

                ServerConnectedEventArgs = null;
                ServerDisconnectedEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                client.Dispose();
                waitStart = DateTime.Now;
                while (ServerDisconnectedEventArgs == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNotNull(ServerDisconnectedEventArgs);
                Assert.AreEqual(ipEp1, ServerDisconnectedEventArgs.LocalEndPoint);
                Assert.AreEqual(remoteIp, ServerDisconnectedEventArgs.RemoteEndPoint);

                var client1 = new TcpClient();
                var client2 = new TcpClient();
                var client3 = new TcpClient();

                ServerConnectedEventArgs = null;
                ServerDisconnectedEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                client1.Start(ipEp1, serverName, clientCert1, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                waitStart = DateTime.Now;
                while (ServerConnectedEventArgs == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNull(ServerDisconnectedEventArgs);
                Assert.AreEqual(ipEp1, ServerConnectedEventArgs.LocalEndPoint);

                ServerConnectedEventArgs = null;
                ServerDisconnectedEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                client2.Start(ipEp2, serverName, clientCert2, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                waitStart = DateTime.Now;
                while (ServerConnectedEventArgs == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNull(ServerDisconnectedEventArgs);
                Assert.AreEqual(ipEp2, ServerConnectedEventArgs.LocalEndPoint);

                ServerConnectedEventArgs = null;
                ServerDisconnectedEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ServerDisconnectedEventArgs);
                client3.Start(ipEp3, serverName, clientCert3, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                waitStart = DateTime.Now;
                while (ServerConnectedEventArgs == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNull(ServerDisconnectedEventArgs);
                Assert.AreEqual(ipEp3, ServerConnectedEventArgs.LocalEndPoint);


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
                Assert.IsFalse(client1.IsConnected);
                Assert.IsFalse(client2.IsConnected);
                Assert.IsFalse(client3.IsConnected);

                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                Thread.Sleep(1000);

                Assert.IsTrue(client1.IsConnected);
                Assert.IsTrue(client2.IsConnected);
                Assert.IsTrue(client3.IsConnected);


                var conn = server.GetConnections();
                var contains = false;
                foreach (Connection connection in conn)
                {
                    if (Equals(connection.RemoteEndPoint, client1.ExchangeInterface.LocalEndPoint) &&
                        Equals(connection.LocalEndPoint, client1.ExchangeInterface.RemoteEndPoint))
                    {
                        contains = true;
                        break;

                    }
                }
                Assert.IsTrue(contains);

                contains = false;
                foreach (Connection connection in conn)
                {
                    if (Equals(connection.RemoteEndPoint, client2.ExchangeInterface.LocalEndPoint) &&
                        Equals(connection.LocalEndPoint, client2.ExchangeInterface.RemoteEndPoint))
                    {
                        contains = true;
                        break;

                    }
                }
                Assert.IsTrue(contains);

                contains = false;
                foreach (Connection connection in conn)
                {
                    if (Equals(connection.RemoteEndPoint, client3.ExchangeInterface.LocalEndPoint) &&
                        Equals(connection.LocalEndPoint, client3.ExchangeInterface.RemoteEndPoint))
                    {
                        contains = true;
                        break;

                    }
                }
                Assert.IsTrue(contains);



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

                Assert.IsFalse(client1.IsConnected);
                Assert.IsFalse(client2.IsConnected);
                Assert.IsFalse(client3.IsConnected);

                Assert.IsTrue(server.IsDisposed);
                Assert.IsFalse(server.IsStarted);


                client1.Dispose();
                client2.Dispose();
                client3.Dispose();

                clientCert1.Dispose();
                clientCert2.Dispose();
                clientCert3.Dispose();
            }
        }

        [TestMethod]
        public void ConnectionCountTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();
                server.ErrorEvent += Server_ErrorEvent;

                server.ConnectedEvent += Server_ConnectedEvent;
                server.DisconnectedEvent += Server_DisconnectedEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1111), 1));
                var ipEp2 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222), 2));
                var ipEp3 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3333), 3));

                epList.Add(ipEp1);
                epList.Add(ipEp2);
                epList.Add(ipEp3);

                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                ConnectionTest(ipEp1, serverName, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                ConnectionTest(ipEp2, serverName, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                ConnectionTest(ipEp3, serverName, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                server.Dispose();
            }
        }

        [TestMethod]
        public void ClientCertNotRequiredCheckTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new EncryptionTestTcpServer();
                server.ErrorEvent += Server_ErrorEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), 1));

                epList.Add(ipEp1);
                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                X509Certificate clientCert = MakeCert(serverName);
                var client = new TcpClient();

                server.ClientCertificate = null;
                client.Start(ipEp1.EndPoint, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                Thread.Sleep(5000);
                Assert.IsNull(server.ClientCertificate);

                client.Stop();
                server.Stop();

                server.Dispose();
                client.Dispose();
                clientCert.Dispose();
            }
        }

        [TestMethod]
        public void ClientCertRequiredCheckTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new EncryptionTestTcpServer();
                server.ErrorEvent += Server_ErrorEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), 1));

                epList.Add(ipEp1);
                server.Start(epList, serverCert, true, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                X509Certificate clientCert = MakeCert(serverName);
                var client = new TcpClient();

                server.ClientCertificate = null;
                client.Start(ipEp1.EndPoint, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                var waitStart = DateTime.Now;
                while (server.ClientCertificate == null)
                {
                    var delta = DateTime.Now - waitStart;
                    if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
                }
                Assert.IsNotNull(server.ClientCertificate);
                Assert.AreEqual(clientCert, server.ClientCertificate);

                client.Stop();
                server.Stop();

                server.Dispose();
                client.Dispose();
                clientCert.Dispose();
            }
        }

        [TestMethod]
        public void SendReceiveTest()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();
                server.ErrorEvent += Server_ErrorEvent;

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
                    
                    var waitStart = DateTime.Now;
                    while (ServerDataReceivedEvent.Count == 0)
                    {
                        var delta = DateTime.Now - waitStart;
                        if (delta.TotalMilliseconds > 100) Assert.Fail("Data not received");
                    }

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

                    var waitStart = DateTime.Now;
                    while (ClientDataReceivedEvent.Count == 0)
                    {
                        var delta = DateTime.Now - waitStart;
                        if (delta.TotalMilliseconds > 100) Assert.Fail("Data not received");
                    }

                    Assert.AreEqual(1, ClientDataReceivedEvent.Count);
                    Assert.AreEqual(data.Length, ClientDataReceivedEvent[0].Data.Length);
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
        public void DisconnectClient_Tests()
        {
            var serverName = "TestServer";
            using (X509Certificate serverCert = MakeCert(serverName))
            {
                var server = new TcpServer();
                server.ErrorEvent += Server_ErrorEvent;

                server.ConnectedEvent += Server_ConnectedEvent;
                server.DisconnectedEvent += Server_DisconnectedEvent;

                var epList = new List<TcpServerLocalEndpoint>();
                var ipEp1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
                var ipEp2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
                var ipEp3 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);

                epList.Add(new TcpServerLocalEndpoint(ipEp1, 1));
                epList.Add(new TcpServerLocalEndpoint(ipEp2, 1));
                epList.Add(new TcpServerLocalEndpoint(ipEp3, 1));

                server.Start(epList, serverCert, false, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                var client = new TcpClient();
                client.ConnectedEvent += Client_ConnectedEvent;
                client.DisconnectedEvent += Client_DisconnectedEvent;

                X509Certificate clientCert = MakeCert(serverName);

                DisconnectClientTest(client, server, ipEp1, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                DisconnectClientTest(client, server, ipEp2, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);
                DisconnectClientTest(client, server, ipEp3, serverName, clientCert, SslProtocols.None, false, EncryptionPolicy.RequireEncryption);

                server.Dispose();

                client.Dispose();

                clientCert.Dispose();
            }
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

        public List<NetworkDataEventArgs> ServerSendDataTraceEvent { get; } = new List<NetworkDataEventArgs>();
        private void Server_SendDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ServerSendDataTraceEvent.Add(e);
        }

        public List<NetworkDataEventArgs> ServerReceiveDataTraceEvent { get; } = new List<NetworkDataEventArgs>();
        private void Server_ReceiveDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ServerReceiveDataTraceEvent.Add(e);
        }

        public bool ClientDisconnectedEventColled { get; set; }
        private void Client_DisconnectedEvent(object sender, System.EventArgs e)
        {
            ClientDisconnectedEventColled = true;
        }

        public ConnectionEventArgs ClientConnectedEventEventArgs { get; set; }
        private void Client_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            ClientConnectedEventEventArgs = e;
        }

        public ConnectionFailEventArgs ConnectionFailEventArgs { get; set; }
        private void Client_ConnectionFailEvent(object sender, ConnectionFailEventArgs e)
        {
            ConnectionFailEventArgs = e;
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

        void ConnectionTest(TcpServerLocalEndpoint tslep, string serverName,
            SslProtocols enabledSslProtocols,
            bool checkCertificateRevocation,
            EncryptionPolicy encryptionPolicy)
        {
            List<TcpClient> clientsList = new List<TcpClient>();
            List<X509Certificate> clientsCerts = new List<X509Certificate>();
            X509Certificate clientCert;

            TcpClient client;

            for (int i = 0; i < tslep.MaxClients; i++)
            {
                clientCert = MakeCert(serverName);
                clientsCerts.Add(clientCert);
                client = new TcpClient();
                client.ConnectionFailEvent += Client_ConnectionFailEvent;
                client.DisconnectedEvent += Client_DisconnectedEvent;
                clientsList.Add(client);
                ServerConnectedEventArgs = null;
                ConnectionFailEventArgs = null;
                //ServerDisconnectedEventArgs = null;
                Assert.IsNull(ServerConnectedEventArgs);
                Assert.IsNull(ConnectionFailEventArgs);
                //Assert.IsNull(ServerDisconnectedEventArgs);
                client.Start(tslep.EndPoint, serverName, clientCert, enabledSslProtocols, checkCertificateRevocation, encryptionPolicy);
                Thread.Sleep(200);
                Assert.IsNull(ConnectionFailEventArgs);
                Assert.IsNotNull(ServerConnectedEventArgs);
                //Assert.IsNull(ServerDisconnectedEventArgs);
                Assert.AreEqual(tslep.EndPoint, ServerConnectedEventArgs.LocalEndPoint);

            }

            clientCert = MakeCert(serverName);
            clientsCerts.Add(clientCert);
            client = new TcpClient();
            client.ConnectionFailEvent += Client_ConnectionFailEvent;
            client.DisconnectedEvent += Client_DisconnectedEvent;
            clientsList.Add(client);
            ServerConnectedEventArgs = null;
            ConnectionFailEventArgs = null;
            ClientDisconnectedEventColled = false;
            Assert.IsNull(ServerConnectedEventArgs);
            Assert.IsNull(ConnectionFailEventArgs);
            Assert.IsFalse(ClientDisconnectedEventColled);
            client.Start(tslep.EndPoint, serverName, clientCert, enabledSslProtocols, checkCertificateRevocation, encryptionPolicy);
            Thread.Sleep(400);
            Assert.IsNull(ServerConnectedEventArgs);
            Assert.IsTrue(ConnectionFailEventArgs != null || ClientDisconnectedEventColled);

            foreach (TcpClient tcpClient in clientsList)
            {
                tcpClient.Dispose();
            }
            clientsList.Clear();

            foreach (X509Certificate cert in clientsCerts)
            {
                cert.Dispose();
            }
            clientsCerts.Clear();
        }

        void ConnectionTest(TcpClient client, IPEndPoint endPoint, string serverName, X509Certificate clientCertificate,
            SslProtocols enabledSslProtocols,
            bool checkCertificateRevocation,
            EncryptionPolicy encryptionPolicy)
        {
            ServerConnectedEventArgs = null;
            //ServerDisconnectedEventArgs = null;
            ClientConnectedEventEventArgs = null;
            Assert.IsNull(ServerConnectedEventArgs);
            //Assert.IsNull(ServerDisconnectedEventArgs);
            Assert.IsNull(ClientConnectedEventEventArgs);
            client.Start(endPoint, serverName, clientCertificate, enabledSslProtocols, checkCertificateRevocation, encryptionPolicy);


            var waitStart = DateTime.Now;
            while (ServerConnectedEventArgs == null || ClientConnectedEventEventArgs == null)
            {
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 60) Assert.Fail("Wait Timeout");
            }
            Assert.IsNotNull(ClientConnectedEventEventArgs);
            Assert.IsNotNull(ServerConnectedEventArgs);
            //Assert.IsNull(ServerDisconnectedEventArgs);
            Assert.AreEqual(endPoint, ServerConnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(endPoint, ClientConnectedEventEventArgs.RemoteEndPoint);

            var clientLocalEp = ClientConnectedEventEventArgs.LocalEndPoint;

            var remoteIp = ServerConnectedEventArgs.RemoteEndPoint;

            ServerConnectedEventArgs = null;
            ServerDisconnectedEventArgs = null;
            Assert.IsNull(ServerConnectedEventArgs);
            Assert.IsNull(ServerDisconnectedEventArgs);
            client.Stop();
            waitStart = DateTime.Now;
            while (!Equals(ServerDisconnectedEventArgs?.RemoteEndPoint, clientLocalEp))
            {
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }

            Assert.IsNull(ServerConnectedEventArgs);
            Assert.IsNotNull(ServerDisconnectedEventArgs);
            Assert.AreEqual(endPoint, ServerDisconnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(remoteIp, ServerDisconnectedEventArgs.RemoteEndPoint);
        }

        void DisconnectClientTest(TcpClient client, TcpServer server, IPEndPoint endPoint, string serverName, X509Certificate clientCertificate,
            SslProtocols enabledSslProtocols,
            bool checkCertificateRevocation,
            EncryptionPolicy encryptionPolicy)
        {
            ServerConnectedEventArgs = null;
            ServerDisconnectedEventArgs = null;
            ClientConnectedEventEventArgs = null;
            Assert.IsNull(ServerConnectedEventArgs);
            Assert.IsNull(ServerDisconnectedEventArgs);
            Assert.IsNull(ClientConnectedEventEventArgs);
            client.Start(endPoint, serverName, clientCertificate, enabledSslProtocols, checkCertificateRevocation, encryptionPolicy);
            var waitStart = DateTime.Now;
            while (ServerConnectedEventArgs == null || ClientConnectedEventEventArgs == null)
            {
                Thread.Sleep(1);
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }
            Assert.IsNotNull(ClientConnectedEventEventArgs);
            Assert.IsNotNull(ServerConnectedEventArgs);
            Assert.IsNull(ServerDisconnectedEventArgs);
            Assert.AreEqual(endPoint, ServerConnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(endPoint, ClientConnectedEventEventArgs.RemoteEndPoint);

            var clientLocalEp = ClientConnectedEventEventArgs.LocalEndPoint;

            var remoteIp = ServerConnectedEventArgs.RemoteEndPoint;
            var localIp = ServerConnectedEventArgs.LocalEndPoint;

            ServerConnectedEventArgs = null;
            ServerDisconnectedEventArgs = null;
            Assert.IsNull(ServerConnectedEventArgs);
            Assert.IsNull(ServerDisconnectedEventArgs);
            server.DisconnectClient((IPEndPoint)localIp, (IPEndPoint)remoteIp);
            waitStart = DateTime.Now;
            while (!Equals(ServerDisconnectedEventArgs?.RemoteEndPoint, clientLocalEp))
            {
                Thread.Sleep(1);
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }
            Assert.IsNotNull(ServerDisconnectedEventArgs);
            Assert.AreEqual(endPoint, ServerDisconnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(remoteIp, ServerDisconnectedEventArgs.RemoteEndPoint);

            //Wait before client will be connected once again.
            waitStart = DateTime.Now;
            ClientConnectedEventEventArgs = null;
            while (ServerConnectedEventArgs == null || ClientConnectedEventEventArgs == null)
            {
                Thread.Sleep(1);
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail("Wait Timeout");
            }

            Assert.IsNotNull(ClientConnectedEventEventArgs);
            Assert.AreEqual(endPoint, ClientConnectedEventEventArgs.RemoteEndPoint);
            clientLocalEp = ClientConnectedEventEventArgs.LocalEndPoint;
            remoteIp = ServerConnectedEventArgs.RemoteEndPoint;

            //Disconnect client.
            ServerDisconnectedEventArgs = null;
            client.Stop();

            waitStart = DateTime.Now;
            while (!Equals(ServerDisconnectedEventArgs?.RemoteEndPoint, clientLocalEp))
            {
                Thread.Sleep(1);
                var delta = DateTime.Now - waitStart;
                if (delta.TotalSeconds > 5) Assert.Fail($"Wait Timeout: expected{ServerDisconnectedEventArgs?.RemoteEndPoint} actual {clientLocalEp}");
            }

            Assert.IsNotNull(ServerDisconnectedEventArgs);
            Assert.AreEqual(endPoint, ServerDisconnectedEventArgs.LocalEndPoint);
            Assert.AreEqual(remoteIp, ServerDisconnectedEventArgs.RemoteEndPoint);
        }

        public ConnectionEventArgs ServerDisconnectedEventArgs { get; set; }
        private void Server_DisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            ServerDisconnectedEventArgs = e;
        }

        public ConnectionEventArgs ServerConnectedEventArgs { get; set; }

        private void Server_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            ServerConnectedEventArgs = e;
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

        System.IO.ErrorEventArgs ServerErrorEvent { get; set; }

        private void Server_NonFailErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            ServerErrorEvent = e;
        }

        public PropertyChangedEventArgs ServerPropertyChangedEventArgs { get; set; }

        private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ServerPropertyChangedEventArgs = e;
        }
    }


    class EncryptionTestTcpServer : TcpServer
    {
        public X509Certificate ClientCertificate { get; set; }
        protected override bool ValidateClientCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate != null)
                ClientCertificate = new X509Certificate(certificate);
            return true;
        }
    }
}