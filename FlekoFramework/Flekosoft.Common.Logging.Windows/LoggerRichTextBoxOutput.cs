using System.Windows.Forms;

namespace Flekosoft.Common.Logging.Windows
{
    public class LoggerRichTextBoxOutput : LoggerOutput
    {
        private delegate void AppendTextAsync(LogRecord logRecord);
        private readonly AppendTextAsync _appendTextDelegate;

        private readonly RichTextBox _logBox;

        public LoggerRichTextBoxOutput(RichTextBox listBox) : base("Logger RichTextBox output")
        {
            _logBox = listBox;
            _appendTextDelegate = AppendText;
        }

        public void AppendText(LogRecord logRecord)
        {
            _logBox.AppendText(logRecord.ToString());
            _logBox.ScrollToCaret();
        }

        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            if (_logBox.InvokeRequired)
            {
                _logBox.BeginInvoke(_appendTextDelegate, logRecord);
            }
            else
            {
                _appendTextDelegate(logRecord);
            }
        }
    }
}
