using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;

namespace Flekosoft.Common.Logging
{
    public abstract class LoggerOutput : PropertyChangedErrorNotifyDisposableBase
    {
        private readonly Thread _outputThread;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly CultureInfo _cultureInfo;
        private readonly CultureInfo _uiCultureInfo;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private readonly ConcurrentQueue<LogRecord> _logRecords = new ConcurrentQueue<LogRecord>();
        readonly EventWaitHandle _queueWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private LogRecordLevel _logLevel;
        private DateTimeFormat _dateTimeFormat;

        protected LoggerOutput(string instanceName) : base(instanceName)
        {
            _cultureInfo = CultureInfo.CurrentCulture;
            _uiCultureInfo = CultureInfo.CurrentUICulture;


            DateTimeFormat = DateTimeFormat.Long;
            LogLevel = LogRecordLevel.All;


            _outputThread = new Thread(MainThread)
            {
                CurrentCulture = _cultureInfo,
                CurrentUICulture = _uiCultureInfo
            };
            _outputThread.Start();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void MainThread(object o)
        {
            while (true)
            {
                try
                {
                    if (_queueWaitHandle.SafeWaitHandle.IsClosed) continue;
                    if (_queueWaitHandle.WaitOne(Timeout.Infinite))
                    {
                        if (_logRecords.TryDequeue(out var record))
                        {
                            if (LogLevel <= record.RecordType)
                            {
                                if (record.LogStrings.Count > 0)
                                {
                                    record.LogStrings[0] = GetPrefixString(record) + record.LogStrings[0];
                                }
                                AppendLogRecordInternal(record);
                            }
                        }
                        if (_logRecords.IsEmpty)
                            if (!_queueWaitHandle.SafeWaitHandle.IsClosed) _queueWaitHandle.Reset();
                            else
                            if (!_queueWaitHandle.SafeWaitHandle.IsClosed) _queueWaitHandle.Set();
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
            }
        }

        public new LogRecordLevel LogLevel
        {
            get => _logLevel;
            set
            {
                if (_logLevel != value)
                {
                    _logLevel = value;
                    OnPropertyChanged(nameof(LogRecordLevel));
                }
            }
        }

        public DateTimeFormat DateTimeFormat
        {
            get => _dateTimeFormat;
            set
            {
                if (_dateTimeFormat != value)
                {
                    _dateTimeFormat = value;
                    OnPropertyChanged(nameof(DateTimeFormat));
                }
            }
        }

        private string GetPrefixString(LogRecord logRecord)
        {
            var result = string.Empty;
            // ReSharper disable UseFormatSpecifierInInterpolation
            switch (DateTimeFormat)
            {
                case DateTimeFormat.None:
                    break;
                case DateTimeFormat.Short:
                    result += $"{logRecord.DateTime.ToString("yy/MM/dd hh:mm:ss")}";
                    break;
                case DateTimeFormat.Long:
                    result += $"{logRecord.DateTime.ToString("yy/MM/dd hh:mm:ss.fffffff")}";
                    break;
            }
            // ReSharper restore UseFormatSpecifierInInterpolation
            switch (logRecord.RecordType)
            {
                case LogRecordLevel.Debug:
                    result += "\tDEBUG\t";
                    break;
                case LogRecordLevel.Error:
                    result += "\tERROR\t";
                    break;
                case LogRecordLevel.Fatal:
                    result += "\tFATAL\t";
                    break;
                default:
                    result += "\t\t";
                    break;
            }
            return result;
        }
        public void AppendLogRecord(LogRecord logRecord)
        {
            _logRecords.Enqueue(logRecord);
            if (!_queueWaitHandle.SafeWaitHandle.IsClosed) _queueWaitHandle?.Set();
        }

        protected abstract void AppendLogRecordInternal(LogRecord logRecord);


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_outputThread != null)
                {
                    if (_outputThread.IsAlive)
                    {
                        _outputThread.Abort();
                    }
                }

                _queueWaitHandle?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
