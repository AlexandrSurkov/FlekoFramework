using System;

namespace Flekosoft.Common.Logging
{
    class LoggerConsoleOutput:LoggerOutput
    {
        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            foreach (string logString in logRecord.LogStrings)
            {
                Console.WriteLine(logString);
            }
        }
    }
}
