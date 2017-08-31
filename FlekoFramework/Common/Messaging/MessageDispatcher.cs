using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Flekosoft.Common;

namespace FlekoSoft.Common.Messaging
{
    /// <summary>
    /// Диспетчер сообщений.
    /// Singleton класс, глобально доступный всем.
    /// </summary>
    public class MessageDispatcher: LoggingBase
    {
        #region Singleton part
        public static MessageDispatcher Instance { get; } = new MessageDispatcher();

        #endregion

        private readonly ConcurrentQueue<DelayedMessage> _messages = new ConcurrentQueue<DelayedMessage>();
        private readonly List<IMessageHandler> _handlers = new List<IMessageHandler>();

        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        public void DispatchMessage(Message message)
        {
            DispatchMessage(message, 0);
        }

        /// <summary>
        /// Отправить сообщение с задержкой
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <param name="delayMs">Задержка, мс</param>
        public void DispatchMessage(Message message, int delayMs)
        {
            if (delayMs <= 0)
            {
                AppendDebugMessage("MessageDispatcher.\tDispatching message: ");
                ProcessMessage(message);
            }
            else
            {
                DateTime dispatchTime = DateTime.Now.AddMilliseconds(delayMs);
                var wrapper = new DelayedMessage(message, dispatchTime);
                _messages.Enqueue(wrapper);
                AppendDebugMessage("MessageDispatcher.\tMessage delayed: " + wrapper);
            }
        }

        /// <summary>
        /// Проверяет и отправляет отложенные сообщения
        /// </summary>
        public void DispatchDelayedMessages()
        {
            var now = DateTime.Now;
            DelayedMessage mw;

            var peeked = _messages.TryPeek(out mw);

            while (peeked && mw.DispatchTime <= now)
            {
                if (_messages.TryDequeue(out mw))
                {
                    AppendDebugMessage("MessageDispatcher.\tDispatching delayed message (Dispatch time = " + mw.DispatchTime.ToString("hh:mm:ss.fffffff") + "): ");
                    ProcessMessage(mw.Message);
                }
                peeked = _messages.TryPeek(out mw);
            }

        }


        /// <summary>
        /// Отправляет сообщение получателям
        /// </summary>
        /// <param name="message"></param>
        void ProcessMessage(Message message)
        {
            foreach (var handler in _handlers)
            {
                if (message.Receiver == null)
                {
                    handler.HandleMessage(message);
                    AppendDebugMessage("\t\t\tBroadcast message dispatched: " + message + " Receiver: " + handler);
                }
                else
                {
                    if (handler.Equals(message.Receiver))
                    {
                        if (!handler.HandleMessage(message))
                        {
                            AppendDebugMessage("\t\t\tMessage not handled: " + message);
                        }
                        else
                        {
                            AppendDebugMessage("\t\t\tMessage dispatched: " + message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Регистрирует получателя сообщений
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterHandler(IMessageHandler handler)
        {
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
                AppendDebugMessage("MessageDispatcher.\tHandler registred: " + handler);
            }
            else
            {
                AppendDebugMessage("MessageDispatcher.\tHandler has allready registred: " + handler);
            }
        }

        /// <summary>
        /// Удаляет получателя сообщений
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveHandler(IMessageHandler handler)
        {
            if (_handlers.Contains(handler))
            {
                _handlers.Remove(handler);
                AppendDebugMessage("MessageDispatcher.\tHandler removed: " + handler);
            }
            else
            {
                AppendDebugMessage("MessageDispatcher.\tHandler is not registred: " + handler);
            }
        }
    }
}
