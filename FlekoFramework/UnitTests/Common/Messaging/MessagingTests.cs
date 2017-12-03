using System.Threading;
using Flekosoft.Common.Logging;
using FlekoSoft.Common.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Messaging
{

    class Handler : MessageHandler
    {
        public bool Handled { get; set; }
        public override bool HandleMessage(Message message)
        {
            Handled = true;
            return base.HandleMessage(message);
        }
    }

    [TestClass]
    public class MessagingTests : MessageHandler
    {
        private bool _handleReturn = true;
        private bool _messageHandled = true;
        [TestMethod]
        public void MessageDispatcherTests()
        {
            Logger.Instance.LogerOutputs.Add(Logger.ConsoleOutput);

            var handler = new Handler { Handled = false };

            MessageDispatcher.Instance.RegisterHandler(this);
            MessageDispatcher.Instance.RegisterHandler(handler);

            var message = new TestMessage(this, this);
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsTrue(_messageHandled);
            Assert.IsFalse(handler.Handled);

            _messageHandled = false;
            _handleReturn = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsTrue(_messageHandled);
            Assert.IsFalse(handler.Handled);

            _messageHandled = false;
            handler.Handled = false;
            message = new TestMessage(this, handler);
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsFalse(_messageHandled);
            Assert.IsTrue(handler.Handled);

            _messageHandled = false;
            handler.Handled = false;
            _handleReturn = true;
            message = new TestMessage(handler);
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsTrue(_messageHandled);
            Assert.IsTrue(handler.Handled);

            //Delayed messaged

            _messageHandled = false;
            handler.Handled = false;
            message = new TestMessage(this, handler);
            MessageDispatcher.Instance.DispatchMessage(message, 10);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
            Thread.Sleep(1);
            MessageDispatcher.Instance.DispatchDelayedMessages();
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
            Thread.Sleep(10);
            MessageDispatcher.Instance.DispatchDelayedMessages();
            Assert.IsFalse(_messageHandled);
            Assert.IsTrue(handler.Handled);

            _messageHandled = false;
            handler.Handled = false;
            message = new TestMessage(handler, this);
            MessageDispatcher.Instance.DispatchMessage(message, 10);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
            Thread.Sleep(1);
            MessageDispatcher.Instance.DispatchDelayedMessages();
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
            Thread.Sleep(10);
            MessageDispatcher.Instance.DispatchDelayedMessages();
            Assert.IsTrue(_messageHandled);
            Assert.IsFalse(handler.Handled);

            _messageHandled = false;
            handler.Handled = false;
            message = new TestMessage(handler);
            MessageDispatcher.Instance.DispatchMessage(message, 10);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
            Thread.Sleep(1);
            MessageDispatcher.Instance.DispatchDelayedMessages();
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
            Thread.Sleep(10);
            MessageDispatcher.Instance.DispatchDelayedMessages();
            Assert.IsTrue(_messageHandled);
            Assert.IsTrue(handler.Handled);


            MessageDispatcher.Instance.RemoveHandler(this);

            message = new TestMessage(this, this);
            handler.Handled = false;
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);

            message = new TestMessage(this, handler);
            handler.Handled = false;
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsFalse(_messageHandled);
            Assert.IsTrue(handler.Handled);

            MessageDispatcher.Instance.RegisterHandler(this);
            MessageDispatcher.Instance.RemoveHandler(handler);

            message = new TestMessage(this, this);
            handler.Handled = false;
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsTrue(_messageHandled);
            Assert.IsFalse(handler.Handled);

            message = new TestMessage(this, handler);
            handler.Handled = false;
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);


            MessageDispatcher.Instance.RemoveHandler(this);
            MessageDispatcher.Instance.RemoveHandler(handler);

            message = new TestMessage(this, this);
            handler.Handled = false;
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);

            message = new TestMessage(this, handler);
            _messageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(message);
            Assert.IsFalse(_messageHandled);
            Assert.IsFalse(handler.Handled);
        }

        public override bool HandleMessage(Message message)
        {
            _messageHandled = true;
            return _handleReturn;
        }
    }

    class TestMessage : Message
    {
        public TestMessage(object sender) : base(sender)
        {
        }

        public TestMessage(object sender, object receiver) : base(sender, receiver)
        {
        }
    }
}
