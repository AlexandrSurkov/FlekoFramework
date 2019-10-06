using System;
using System.Collections.Generic;
using Flekosoft.Common.Collection;

namespace Flekosoft.Common.Logging
{
    public class Logger : DisposableBase
    {
        #region Singleton part
        // ReSharper disable once InconsistentNaming
        public static Logger Instance { get; } = new Logger();

        #endregion

        public static LoggerOutput ConsoleOutput = new LoggerConsoleOutput();

        public ListCollection<LoggerOutput> LoggerOutputs { get; } = new ListCollection<LoggerOutput>("Logger outputs collection", true);


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void AppendLog(LogRecord logRecord)
        {
            foreach (LoggerOutput loggerOutput in LoggerOutputs)
            {

                loggerOutput.AppendLogRecord(new LogRecord(logRecord));
            }
            OnLogEvent(logRecord);
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void AppendException(Exception ex)
        {
            List<string> strings = new List<string>();
            FillExceptionStrings(strings, ex);
            var logRec = new LogRecord(DateTime.Now, strings, LogRecordLevel.Error, ConsoleColor.Red);
            AppendLog(logRec);
        }

        void FillExceptionStrings(List<string> strings, Exception ex)
        {
            strings.Add("Type:\t" + ex.GetType());
            strings.Add("Message:\t" + ex.Message);
            strings.Add("Source:\t" + ex.Source);
            strings.Add("StackTrace:\t" + ex.StackTrace);
            if (ex.InnerException != null)
            {
                strings.Add(String.Empty);
                strings.Add("Inner exception:");
                FillExceptionStrings(strings, ex.InnerException);
            }
        }
        public void AppendDebug(ICollection<string> strings)
        {
            var logRec = new LogRecord(DateTime.Now, strings, LogRecordLevel.Debug, ConsoleColor.Blue);
            AppendLog(logRec);
        }
        public void AppendDebug(string str)
        {
            var stringList = new List<string> {str};
            AppendDebug(stringList);
        }

        public event EventHandler<LogEventArgs> LogEvent;
        protected void OnLogEvent(LogRecord logRecord)
        {
            // ReSharper disable once UseNullPropagation
            LogEvent?.Invoke(this, new LogEventArgs(logRecord));
        }

        #region Disposable Members
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (LoggerOutput loggerOutput in LoggerOutputs)
                {
                    loggerOutput.Dispose();
                }

                ConsoleOutput.Dispose();

                LogEvent = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
