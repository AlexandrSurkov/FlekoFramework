using System;

namespace Flekosoft.Common.Logging
{
    public enum LogRecordLevel
    {
        None = 0,
        All = 0x01,
        Debug = 0x02,
        Info = 0x03,
        Error = 0x04,
        Fatal = 0x05,
        Off = 0x06,
    }

    public enum DateTimeFormat
    {
        None = 0,
        Short = 0x01,
        Long = 0x02,
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(LogRecord logRecord)
        {
            LogRecord = logRecord;
        }

        public LogRecord LogRecord { get; }
    }
}
