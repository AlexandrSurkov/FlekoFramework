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
        readonly EventWaitHandle _threadFinishedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private LogRecordLevel _logLevel;
        private DateTimeFormat _dateTimeFormat;
        private PrefixFormat _prefixFormat;

        // Define the cancellation token.
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected LoggerOutput(string instanceName) : base(instanceName)
        {
            _cultureInfo = CultureInfo.CurrentCulture;
            _uiCultureInfo = CultureInfo.CurrentUICulture;


            DateTimeFormat = DateTimeFormat.Long;
            PrefixFormat = PrefixFormat.DateTimeAndType;
            LogLevel = LogRecordLevel.All;


            _outputThread = new Thread(MainThread);
            _outputThread.Start(_cancellationTokenSource.Token);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void MainThread(object o)
        {
            _outputThread.CurrentCulture = _cultureInfo;
            _outputThread.CurrentUICulture = _uiCultureInfo;

            var cancellationToken = (CancellationToken)o;

            while (true)
            {
                try
                {
                    if (_queueWaitHandle.SafeWaitHandle.IsClosed) continue;
                    if (_queueWaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
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
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    break;
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
            _threadFinishedWaitHandle.Set();
        }

        public new LogRecordLevel LogLevel
        {
            get => _logLevel;
            set
            {
                if (_logLevel != value)
                {
                    _logLevel = value;
                    OnPropertyChanged(nameof(LogLevel));
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

        public PrefixFormat PrefixFormat
        {
            get => _prefixFormat;
            set
            {
                if (_prefixFormat != value)
                {
                    _prefixFormat = value;
                    OnPropertyChanged(nameof(PrefixFormat));
                }
            }
        }



        private string GetPrefixString(LogRecord logRecord)
        {
            var result = string.Empty;

            var datetime = string.Empty;

            // ReSharper disable UseFormatSpecifierInInterpolation
            switch (DateTimeFormat)
            {
                case DateTimeFormat.None:
                    break;
                case DateTimeFormat.Short:
                    datetime += $"{logRecord.DateTime.ToString("yy/MM/dd hh:mm:ss")}\t";
                    break;
                case DateTimeFormat.Long:
                    datetime += $"{logRecord.DateTime.ToString("yy/MM/dd hh:mm:ss.fffffff")}\t";
                    break;
            }

            var type = string.Empty;
            // ReSharper restore UseFormatSpecifierInInterpolation
            switch (logRecord.RecordType)
            {
                case LogRecordLevel.Debug:
                    type += "DEBUG\t";
                    break;
                case LogRecordLevel.Error:
                    type += "ERROR\t";
                    break;
                case LogRecordLevel.Fatal:
                    type += "FATAL\t";
                    break;
                default:
                    type += "\t";
                    break;
            }

            switch (PrefixFormat)
            {
                case PrefixFormat.None:
                    break;
                case PrefixFormat.DateTime:
                    result += datetime;
                    break;
                case PrefixFormat.Type:
                    result += type;
                    break;
                case PrefixFormat.DateTimeAndType:
                    result += datetime;
                    result += type;
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
                        //_outputThread.Abort();
                        _cancellationTokenSource.Cancel();
                        _threadFinishedWaitHandle.WaitOne(Timeout.Infinite);
                    }
                }
                _cancellationTokenSource.Dispose();
                _queueWaitHandle?.Dispose();
                _threadFinishedWaitHandle?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
