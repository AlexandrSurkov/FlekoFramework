using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Flekosoft.Common.Logging.Windows
{
    public class LoggerListBoxOutput : LoggerOutput
    {
        private delegate void AppendTextAsync(LogRecord logRecord);
        private readonly AppendTextAsync _appendTextDelegate;
        private readonly List<LogRecord> _logRecordsList = new List<LogRecord>();
        private readonly List<string> _appendStringsList = new List<string>();

        private readonly ListBox _logBox;
        private string _filter = string.Empty;
        private uint _maximumLines = 5000;


        public LoggerListBoxOutput(ListBox listBox)
        {
            _logBox = listBox;
            _appendTextDelegate = AppendText;
            PropertyChanged += LoggerListBoxOutput_PropertyChanged;
        }

        private void LoggerListBoxOutput_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_logBox.InvokeRequired)
            {
                var args = new Object[2];
                args[0] = sender;
                args[1] = e;
                _logBox.Invoke(new EventHandler<PropertyChangedEventArgs>(LoggerListBoxOutput_PropertyChanged), args);
            }
            else
            {
                if (e.PropertyName == "LogLevel")
                    UpdateListBox(true);
            }
        }

        public uint MaximumLines
        {
            get { return _maximumLines; }
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
            get { return _filter; }
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    UpdateListBox(true);
                }
            }
        }

        //[DllImport("user32.dll")]
        //// ReSharper disable InconsistentNaming
        //private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        //// ReSharper restore InconsistentNaming
        //// ReSharper disable InconsistentNaming
        //private const int WM_USER = 0x400;
        //private const int EM_HIDESELECTION = WM_USER + 63;
        //// ReSharper restore InconsistentNaming

        //[DllImport("user32.dll")]
        //private static extern bool LockWindowUpdate(IntPtr hWndLock);

        public void AppendText(LogRecord logRecord)
        {
            _logRecordsList.Add(logRecord);
            UpdateListBox(false);
        }


        public void Clear()
        {
            _logRecordsList.Clear();
            UpdateListBox(true);
        }
        void UpdateListBox(bool clear)
        {
            _logBox.SuspendLayout();

            if (_logRecordsList.Count > MaximumLines)
            {
                _logRecordsList.RemoveAt(0);
            }
            _appendStringsList.Clear();
            foreach (LogRecord record in _logRecordsList)
            {
                if (LogLevel <= record.RecordType)
                {
                    if (Filter == String.Empty)
                        _appendStringsList.AddRange(record.LogStrings);
                    else
                    {
                        foreach (string logString in record.LogStrings)
                        {
                            if (logString.Contains(Filter))
                            {
                                _appendStringsList.AddRange(record.LogStrings);
                                break;
                            }
                        }
                    }
                }
            }

            if (clear)
            {
                _logBox.Items.Clear();
                _logBox.Items.AddRange(_appendStringsList.ToArray());
            }
            else
            {
                var oldItemsCount = _logBox.Items.Count;
                var newItemsCount = _appendStringsList.Count;

                _logBox.Items.AddRange(_appendStringsList.GetRange(oldItemsCount, newItemsCount - oldItemsCount).ToArray());
            }

            _logBox.TopIndex = _logBox.Items.Count - 1;

            _logBox.ResumeLayout();
        }

        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            if (_logBox.InvokeRequired)
            {
                _logBox.BeginInvoke(_appendTextDelegate, new object[] { logRecord });
            }
            else
            {
                _appendTextDelegate(logRecord);
            }
        }
    }
}
