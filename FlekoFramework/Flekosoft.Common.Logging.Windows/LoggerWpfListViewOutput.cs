using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;

namespace Flekosoft.Common.Logging.Windows
{
    public class LoggerWpfListViewOutput : LoggerOutput
    {
        private delegate void AppendTextAsync(LogRecord logRecord);
        private readonly AppendTextAsync _appendTextDelegate;
        private readonly List<LogRecord> _logRecordsList = new List<LogRecord>();
        private readonly List<string> _appendStringsList = new List<string>();

        private readonly ListView _listView;
        private string _filter = string.Empty;
        private uint _maximumLines = 500;
        //private DateTime _lastUpdateTime = DateTime.MinValue;

        private readonly Thread _updateThread;

        // Define the cancellation token.
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly EventWaitHandle _threadFinishedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);


        public LoggerWpfListViewOutput(ListView listView) : base("Logger WpfListBox output")
        {
            _listView = listView;
            _appendTextDelegate = AppendText;
            _updateThread = new Thread(UpdateThread);
            _updateThread.Start(_cancellationTokenSource.Token);
            PropertyChanged += LoggerListBoxOutput_PropertyChanged;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void UpdateThread(object o)
        {
            var cancellationToken = (CancellationToken)o;

            while (true)
            {
                try
                {
                    Thread.Sleep(100);
                    if (!IsDisposed)
                    {
                        if (!IsDisposing)
                            _listView.Dispatcher?.Invoke(delegate { UpdateListBox(false); });
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

        private void LoggerListBoxOutput_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_listView.Dispatcher != null && !_listView.Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                if (e.PropertyName == nameof(LogLevel))
                {
                    _listView.Dispatcher.Invoke(delegate { UpdateListBox(true); });
                }
            }
            else
            {
                if (e.PropertyName == nameof(LogLevel))
                {
                    UpdateListBox(true);
                }
            }
        }

        public uint MaximumLines
        {
            get => _maximumLines;
            set
            {
                if (_maximumLines != value)
                {
                    _maximumLines = value;
                    OnPropertyChanged(nameof(MaximumLines));
                    UpdateListBox(true);
                }
            }
        }

        public string Filter
        {
            get => _filter;
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    UpdateListBox(true);
                }
            }
        }

        public void AppendText(LogRecord logRecord)
        {
            _logRecordsList.Add(logRecord);
            //var now = DateTime.Now;
            //var diff = now - _lastUpdateTime;
            ////UpdateListBox(false);
            //if (diff.TotalMilliseconds > 100)
            //{
            //    UpdateListBox(false);
            //    _lastUpdateTime = DateTime.Now;
            //}
        }


        public void Clear()
        {
            _logRecordsList.Clear();
            UpdateListBox(true);
        }

        void UpdateListBox(bool clear)
        {

            int linesCount = 0;

            while (_logRecordsList.Count > MaximumLines)
            {
                linesCount += _logRecordsList[0].LogStrings.Count;
                _logRecordsList.RemoveAt(0);
                //_logBox.TopIndex = _logBox.Items.Count - 1;
            }

            for (int i = 0; i < linesCount; i++)
            {
                if (_listView.Items.Count > 0)
                    _listView.Items.RemoveAt(0);
                if (_listView.Items.Count > 0)
                {
                    _listView.ScrollIntoView(_listView.Items[_listView.Items.Count - 1]);
                }
            }

            _appendStringsList.Clear();
            linesCount = 0;
            foreach (LogRecord record in _logRecordsList)
            {
                if (LogLevel <= record.RecordType)
                {
                    if (Filter == String.Empty)
                    {
                        _appendStringsList.AddRange(record.LogStrings);
                        linesCount += record.LogStrings.Count;
                    }
                    else
                    {
                        foreach (string logString in record.LogStrings)
                        {
                            if (logString.Contains(Filter))
                            {
                                _appendStringsList.AddRange(record.LogStrings);
                                linesCount += record.LogStrings.Count;
                                break;
                            }
                        }
                    }
                }
            }

            var oldItemsCount = _listView.Items.Count;
            var newItemsCount = linesCount;
            var delta = newItemsCount - oldItemsCount;

            if (clear)
            {
                _listView.Items.Clear();
                foreach (string s in _appendStringsList)
                {
                    _listView.Items.Add(s);
                }
            }
            else
            {


                var range = _appendStringsList.GetRange(oldItemsCount, newItemsCount - oldItemsCount).ToArray();

                foreach (string s in range)
                {
                    _listView.Items.Add(s);
                }
            }

            if (_listView.Items.Count > 0 && delta != 0)
            {
                _listView.ScrollIntoView(_listView.Items[_listView.Items.Count - 1]);
            }

        }

        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            if (_listView.Dispatcher != null && !_listView.Dispatcher.CheckAccess())
            {
                _listView.Dispatcher.BeginInvoke(_appendTextDelegate, logRecord);
            }
            else
            {
                _appendTextDelegate(logRecord);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_updateThread != null)
                {
                    if (_updateThread.IsAlive)
                    {
                        //_updateThread.Abort();
                        _cancellationTokenSource.Cancel();
                        _threadFinishedWaitHandle.WaitOne(Timeout.Infinite);
                    }
                }
                _cancellationTokenSource.Dispose();
                _threadFinishedWaitHandle?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
