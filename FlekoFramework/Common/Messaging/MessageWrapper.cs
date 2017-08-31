using System;

namespace FlekoSoft.Common.Messaging
{
    /// <summary>
    /// Хранилище отложенных сообщений
    /// </summary>
    class DelayedMessage
    {
        private readonly Message _message;

        public DelayedMessage(Message message, DateTime dispatchTime)
        {
            _message = message;
            DispatchTime = dispatchTime;
        }

        public DateTime DispatchTime { get; }

        public Message Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            var str = Message + " DispatchTime: " + DispatchTime.ToString("hh:mm:ss.fffffff");
            return str;
        }
    }
}
