using System;
using System.Collections.Generic;

namespace Flekosoft.Common.Logging
{
    public class Logger : DisposableBase
    {
        #region Singleton part
        // ReSharper disable once InconsistentNaming
        public static Logger Instance { get; } = new Logger();

        #endregion

        public static LoggerOutput ConsoleOutput = new LoggerConsoleOutput();

        public List<LoggerOutput> LogerOutputs { get; } = new List<LoggerOutput>();


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void AppendLog(LogRecord logRecord)
        {
            foreach (LoggerOutput loggerOutput in LogerOutputs)
            {
                loggerOutput.AppendLogRecord(logRecord);
            }
            OnLogEvent(logRecord);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void AppendException(Exception ex)
        {
            List<string> strings = new List<string>();
            FillExceprionStrings(strings, ex);
            var logrec = new LogRecord(DateTime.Now, strings, LogRecordLevel.Error, ConsoleColor.Red);
            AppendLog(logrec);
        }

        void FillExceprionStrings(List<string> strings, Exception ex)
        {
            strings.Add("Type:\t" + ex.GetType());
            strings.Add("Message:\t" + ex.Message);
            strings.Add("Source:\t" + ex.Source);
            strings.Add("StackTrace:\t" + ex.StackTrace);
            if (ex.InnerException != null)
            {
                strings.Add(String.Empty);
                strings.Add("Inner exception:");
                FillExceprionStrings(strings, ex.InnerException);
            }
        }
        public void AppendDebug(ICollection<string> strings)
        {
            var logrec = new LogRecord(DateTime.Now, strings, LogRecordLevel.Debug, ConsoleColor.Blue);
            AppendLog(logrec);
        }
        public void AppendDebug(string str)
        {
            var strs = new List<string>();
            strs.Add(str);
            AppendDebug(strs);
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
                foreach (LoggerOutput loggerOutput in LogerOutputs)
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
