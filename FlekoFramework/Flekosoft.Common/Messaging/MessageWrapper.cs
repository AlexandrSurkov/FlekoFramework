using System;

namespace Flekosoft.Common.Messaging
{
    /// <summary>
    /// Delayed message storage
    /// </summary>
    class DelayedMessage
    {
        public DelayedMessage(Message message, DateTime dispatchTime)
        {
            Message = message;
            DispatchTime = dispatchTime;
        }

        public DateTime DispatchTime { get; }

        public Message Message { get; }

        public override string ToString()
        {
            var str = Message + " DispatchTime: " + DispatchTime.ToString("hh:mm:ss.fffffff");
            return str;
        }
    }
}
