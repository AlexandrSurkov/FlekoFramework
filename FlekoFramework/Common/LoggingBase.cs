using System.Collections.Generic;
using Flekosoft.Common.Logging;
using Flekosoft.Common.Serialization;

namespace Flekosoft.Common
{
    public abstract class LoggingBase
    {
        /// <summary>
        /// SendDebugMessages
        /// </summary>
        public bool SendDebugMessages { get; set; }

        protected void AppendDebugMessage(string message)
        {
            if (SendDebugMessages) Logger.Instance.AppendDebug(message);
        }

        protected void AppendDebugMessage(ICollection<string> messages)
        {
            if (SendDebugMessages) Logger.Instance.AppendDebug(messages);
        }

        public List<ISerializer> Serializers { get; } = new List<ISerializer>();
    }
}
