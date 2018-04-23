using System;
using System.Collections.Generic;

namespace Flekosoft.Common.Logging
{
    public class LogRecord
    {
        public List<string> LogStrings = new List<string>();

        public LogRecord(LogRecord logRecord)
        {
            DateTime = logRecord.DateTime;
            RecordType = logRecord.RecordType;
            Color = logRecord.Color;
            LogStrings.AddRange(logRecord.LogStrings);
        }

        public LogRecord(DateTime dateTime, ICollection<string> logStrings, LogRecordLevel recordType)
        {
            DateTime = dateTime;
            RecordType = recordType;
            Color = ConsoleColor.Black;
            LogStrings.AddRange(logStrings);
        }

        public LogRecord(DateTime dateTime, ICollection<string> logStrings, LogRecordLevel recordType, ConsoleColor color)
        {
            DateTime = dateTime;
            Color = color;
            RecordType = recordType;
            LogStrings.AddRange(logStrings);
        }

        public DateTime DateTime { get; }
        public ConsoleColor Color { get; }
        public LogRecordLevel RecordType { get; }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LogRecord)obj);
        }

        protected bool Equals(LogRecord other)
        {
            return Color.Equals(other.Color) && DateTime.Equals(other.DateTime) && RecordType.Equals(other.RecordType) && LogStrings.Equals(other.LogStrings);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Color.GetHashCode() * 397) ^ (DateTime.GetHashCode() * 398) ^ (RecordType.GetHashCode() * 399) ^ LogStrings.GetHashCode();
            }
        }

        public override string ToString()
        {
            var text = string.Empty;
            foreach (string logString in LogStrings)
            {
                text += logString + "\r\n";
            }

            return text;
        }

        #endregion
    }
}
