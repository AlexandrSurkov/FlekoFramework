﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flekosoft.Common.Messaging
{
    /// <summary>
    /// Message dispatcher.
    /// Uses singleton pattern so has general visibility
    /// </summary>
    public class MessageDispatcher: LoggingSerializableBase
    {
        #region Singleton part
        public static MessageDispatcher Instance { get; } = new MessageDispatcher();

        #endregion

        private readonly ConcurrentQueue<DelayedMessage> _messages = new ConcurrentQueue<DelayedMessage>();
        private readonly List<IMessageHandler> _handlers = new List<IMessageHandler>();

        /// <summary>
        /// Dispatch message
        /// </summary>
        /// <param name="message">Message for dispatch</param>
        public void DispatchMessage(Message message)
        {
            DispatchMessage(message, 0);
        }

        /// <summary>
        /// Dispatch message with delay
        /// </summary>
        /// <param name="message">Message for dispatch</param>
        /// <param name="delayMs">Delay in ms</param>
        public void DispatchMessage(Message message, int delayMs)
        {
            if (delayMs <= 0)
            {
                AppendDebugLogMessage("MessageDispatcher.\tDispatching message: ");
                ProcessMessage(message);
            }
            else
            {
                DateTime dispatchTime = DateTime.Now.AddMilliseconds(delayMs);
                var wrapper = new DelayedMessage(message, dispatchTime);
                _messages.Enqueue(wrapper);
                AppendDebugLogMessage("MessageDispatcher.\tMessage delayed: " + wrapper);
            }
        }

        /// <summary>
        /// Checks and sends delayed messages
        /// </summary>
        public void DispatchDelayedMessages()
        {
            var now = DateTime.Now;

            var peeked = _messages.TryPeek(out var mw);

            while (peeked && mw.DispatchTime <= now)
            {
                if (_messages.TryDequeue(out mw))
                {
                    AppendDebugLogMessage("MessageDispatcher.\tDispatching delayed message (Dispatch time = " + mw.DispatchTime.ToString("hh:mm:ss.fffffff") + "): ");
                    ProcessMessage(mw.Message);
                }
                peeked = _messages.TryPeek(out mw);
            }

        }


        /// <summary>
        /// Sends a message to recipients
        /// </summary>
        /// <param name="message"></param>
        void ProcessMessage(Message message)
        {
            foreach (var handler in _handlers)
            {
                if (message.Receiver == null)
                {
                    handler.HandleMessage(message);
                    AppendDebugLogMessage("\t\t\tBroadcast message dispatched: " + message + " Receiver: " + handler);
                }
                else
                {
                    if (handler.Equals(message.Receiver))
                    {
                        if (!handler.HandleMessage(message))
                        {
                            AppendDebugLogMessage("\t\t\tMessage not handled: " + message);
                        }
                        else
                        {
                            AppendDebugLogMessage("\t\t\tMessage dispatched: " + message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers the message recipient
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterHandler(IMessageHandler handler)
        {
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
                AppendDebugLogMessage("MessageDispatcher.\tHandler registered: " + handler);
            }
            else
            {
                AppendDebugLogMessage("MessageDispatcher.\tHandler has already registered: " + handler);
            }
        }

        /// <summary>
        /// Deletes the message recipient
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveHandler(IMessageHandler handler)
        {
            if (_handlers.Contains(handler))
            {
                _handlers.Remove(handler);
                AppendDebugLogMessage("MessageDispatcher.\tHandler removed: " + handler);
            }
            else
            {
                AppendDebugLogMessage("MessageDispatcher.\tHandler is not registered: " + handler);
            }
        }
    }
}
