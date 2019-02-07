using System;
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

            var path = dir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(_logFilePath) + "_" + DateTime.Now.ToString("dd_MM_yyyy") + Path.GetExtension(_logFilePath);
            // ReSharper restore AssignNullToNotNullAttribute

            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            using (var outfile = new StreamWriter(path, true))
            {
                foreach (string logString in logRecord.LogStrings)
                {
                    outfile.WriteLine(logString);
                }
            }
        }
    }
}
