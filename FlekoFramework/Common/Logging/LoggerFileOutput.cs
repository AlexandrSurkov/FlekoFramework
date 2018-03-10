using System.IO;

namespace Flekosoft.Common.Logging
{
    public class LoggerFileOutput : LoggerOutput
    {
        private readonly string _logFilePath;
        public LoggerFileOutput(string logFilePath) : base("Logger file output")
        {
            _logFilePath = logFilePath;
        }
        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            var dir = Path.GetDirectoryName(_logFilePath);
            // ReSharper disable AssignNullToNotNullAttribute
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            // ReSharper restore AssignNullToNotNullAttribute

            if (!File.Exists(_logFilePath))
            {
                File.Create(_logFilePath).Close();
            }
            using (var outfile = new StreamWriter(_logFilePath, true))
            {
                foreach (string logString in logRecord.LogStrings)
                {
                    outfile.WriteLine(logString);
                }
            }
        }
    }
}
