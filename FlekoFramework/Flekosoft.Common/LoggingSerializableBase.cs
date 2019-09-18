using System;
using System.Collections.Generic;
using Flekosoft.Common.Logging;
using Flekosoft.Common.Serialization;

namespace Flekosoft.Common
{
    public abstract class LoggingSerializableBase
    {
        public LogRecordLevel LogLevel { get; set; } = LogRecordLevel.All;

        protected void AppendExceptionLogMessage(Exception ex)
        {
            if (LogLevel <= LogRecordLevel.Error)
            {
                Logger.Instance.AppendException(ex);
            }
        }

        protected void AppendDebugLogMessage(string message)
        {
            if (LogLevel <= LogRecordLevel.Debug)
            {
                Logger.Instance.AppendDebug(message);
            }
        }

        protected void AppendLogMessage(LogRecord logRecord)
        {
            if (LogLevel <= logRecord.RecordType)
            {
                Logger.Instance.AppendLog(logRecord);
            }
        }

        protected void AppendDebugLogMessage(ICollection<string> messages)
        {
            if (LogLevel <= LogRecordLevel.Debug)
            {
                Logger.Instance.AppendDebug(messages);
            }
        }

        public List<ISerializer> Serializers { get; } = new List<ISerializer>();
    }
}
