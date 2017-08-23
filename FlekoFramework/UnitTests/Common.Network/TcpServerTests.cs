using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
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
            Assert.IsTrue(server.Start(new List<TcpServerLocalEndpoint>()));
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
            Assert.IsTrue(server.Start(new List<TcpServerLocalEndpoint>()));
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

            Assert.IsTrue(server.Start(new List<TcpServerLocalEndpoint>()));
            Assert.IsFalse(server.Start(new List<TcpServerLocalEndpoint>()));
            server.Dispose();
            Assert.IsTrue(server.DataTrace);
            Assert.IsFalse(server.IsStarted);
            Assert.IsTrue(server.IsDisposed);

        }

        [TestMethod]
        public void LocalhostServerTests()
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
