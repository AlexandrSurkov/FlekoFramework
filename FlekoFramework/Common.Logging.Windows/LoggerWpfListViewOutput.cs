using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private DateTime _lastUpdateTime = DateTime.MinValue;


        public LoggerWpfListViewOutput(ListView listView)
        {
            _listView = listView;
            _appendTextDelegate = AppendText;
            PropertyChanged += LoggerListBoxOutput_PropertyChanged;
        }

        private void LoggerListBoxOutput_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_listView.Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                if (e.PropertyName == "LogLevel")
                {
                    _listView.Dispatcher.Invoke(new Action(delegate { UpdateListBox(true); }));
                }
            }
            else
            {
                if (e.PropertyName == "LogLevel")
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
            var now = DateTime.Now;
            var diff = now - _lastUpdateTime;
            UpdateListBox(false);
            if (diff.TotalMilliseconds > 100)
            {
                UpdateListBox(false);
                _lastUpdateTime = DateTime.Now;
            }
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
                var oldItemsCount = _listView.Items.Count;
                var newItemsCount = linesCount;

                //if (newItemsCount < oldItemsCount) _logBox.Items.AddRange(_appendStringsList.ToArray());
                //else _logBox.Items.AddRange(_appendStringsList.GetRange(oldItemsCount, newItemsCount - oldItemsCount).ToArray());

                var range = _appendStringsList.GetRange(oldItemsCount, newItemsCount - oldItemsCount).ToArray();
                foreach (string s in range)
                {
                    _listView.Items.Add(s);
                }
            }

            if (_listView.Items.Count > 0)
            {
                _listView.ScrollIntoView(_listView.Items[_listView.Items.Count - 1]);
            }

        }

        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            if (!_listView.Dispatcher.CheckAccess())
            {
                _listView.Dispatcher.Invoke(_appendTextDelegate, logRecord);
            }
            else
            {
                _appendTextDelegate(logRecord);
            }
        }
    }
}
