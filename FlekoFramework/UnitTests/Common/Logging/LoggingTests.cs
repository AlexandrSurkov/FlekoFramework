using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Flekosoft.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Logging
{
    class Writer : LoggerOutput
    {
        public LogRecord LogRecord { get; set; }

        protected override void AppendLogRecordInternal(LogRecord logRecord)
        {
            LogRecord = logRecord;
        }
    }

    [TestClass]
    public class LoggingTests
    {
        readonly Writer _writer = new Writer();

        private readonly object _syncObject = new object();
        [TestMethod]
        public void LoggingTest()
        {
            lock (_syncObject)
            {
                Logger.Instance.LogerOutputs.Clear();

                _writer.LogRecord = null;

                TestLogRecord(LogRecordLevel.All, LogRecordLevel.Debug, true);

                Logger.Instance.LogerOutputs.Add(_writer);

                TestLogRecord(LogRecordLevel.All, LogRecordLevel.Debug, false);
                TestLogRecord(LogRecordLevel.All, LogRecordLevel.Info, false);
                TestLogRecord(LogRecordLevel.All, LogRecordLevel.Error, false);
                TestLogRecord(LogRecordLevel.All, LogRecordLevel.Fatal, false);


                TestLogRecord(LogRecordLevel.Debug, LogRecordLevel.Debug, false);
                TestLogRecord(LogRecordLevel.Debug, LogRecordLevel.Info, false);
                TestLogRecord(LogRecordLevel.Debug, LogRecordLevel.Error, false);
                TestLogRecord(LogRecordLevel.Debug, LogRecordLevel.Fatal, false);

                TestLogRecord(LogRecordLevel.Info, LogRecordLevel.Debug, true);
                TestLogRecord(LogRecordLevel.Info, LogRecordLevel.Info, false);
                TestLogRecord(LogRecordLevel.Info, LogRecordLevel.Error, false);
                TestLogRecord(LogRecordLevel.Info, LogRecordLevel.Fatal, false);

                TestLogRecord(LogRecordLevel.Error, LogRecordLevel.Debug, true);
                TestLogRecord(LogRecordLevel.Error, LogRecordLevel.Info, true);
                TestLogRecord(LogRecordLevel.Error, LogRecordLevel.Error, false);
                TestLogRecord(LogRecordLevel.Error, LogRecordLevel.Fatal, false);

                TestLogRecord(LogRecordLevel.Fatal, LogRecordLevel.Debug, true);
                TestLogRecord(LogRecordLevel.Fatal, LogRecordLevel.Info, true);
                TestLogRecord(LogRecordLevel.Fatal, LogRecordLevel.Error, true);
                TestLogRecord(LogRecordLevel.Fatal, LogRecordLevel.Fatal, false);

                TestLogRecord(LogRecordLevel.Off, LogRecordLevel.Debug, true);
                TestLogRecord(LogRecordLevel.Off, LogRecordLevel.Info, true);
                TestLogRecord(LogRecordLevel.Off, LogRecordLevel.Error, true);
                TestLogRecord(LogRecordLevel.Off, LogRecordLevel.Fatal, true);

                Logger.Instance.LogerOutputs.Clear();
            }
        }

        void TestLogRecord(LogRecordLevel writerLevel, LogRecordLevel recordLevel, bool isShouldBeNull)
        {
            var dt = DateTime.Now;
            var s = new List<string> { writerLevel.ToString(), recordLevel.ToString() };
            var c = ConsoleColor.Red;

            _writer.LogLevel = writerLevel;
            _writer.LogRecord = null;
            Assert.IsNull(_writer.LogRecord);
            var logRecord = new LogRecord(dt, new List<string>(s), recordLevel, c);
            Logger.Instance.AppendLog(logRecord);
            Thread.Sleep(10);  //Wait until async threads did their work
            if (isShouldBeNull)
                Assert.IsNull(_writer.LogRecord);
            else
                Assert.AreEqual(logRecord, _writer.LogRecord);
        }

        [TestMethod]
        public void ConsoleLogTest()
        {
            lock (_syncObject)
            {
                Logger.Instance.LogerOutputs.Clear();

                var dt = DateTime.Now;
                var s = new List<string> { DateTime.Now.Ticks.ToString(), DateTime.Now.Ticks.ToString() };
                var c = ConsoleColor.Red;

                var sw = new StringWriter();
                Console.SetOut(sw);

                var writer = Logger.ConsoleOutput;
                writer.LogLevel = LogRecordLevel.All;
                Logger.Instance.LogerOutputs.Add(writer);

                var logRecord = new LogRecord(dt, new List<string>(s), LogRecordLevel.Fatal, c);
                Logger.Instance.AppendLog(logRecord);
                Thread.Sleep(5);//Wait until async threads did their work

                var str = sw.GetStringBuilder().ToString();
                var srcStr = $"{logRecord.LogStrings[0]}\r\n{logRecord.LogStrings[1]}\r\n";
                var res = String.CompareOrdinal(str, srcStr);
                Assert.AreEqual(0, res, str);

                Logger.Instance.LogerOutputs.Clear();

                Logger.Instance.AppendLog(logRecord);
                str = sw.GetStringBuilder().ToString();
                srcStr = $"{logRecord.LogStrings[0]}\r\n{logRecord.LogStrings[1]}\r\n";
                res = String.CompareOrdinal(str, srcStr);
                Assert.AreEqual(0, res, str);

                sw.Dispose();

                Logger.Instance.LogerOutputs.Clear();
            }
        }

        //[TestMethod]
        //public void ReachTextBoxLogTest()
        //{
        //    lock (_syncObject)
        //    {
        //        var dt = DateTime.Now;
        //        var s = new List<string> { DateTime.Now.Ticks.ToString(), DateTime.Now.Ticks.ToString() };
        //        var c = ConsoleColor.Red;

        //        Logger.Instance.LogerOutputs.Clear();

        //        var rtb = new RichTextBox();
        //        var rtbtw = new LoggerRichTextBoxOutput(rtb) { LogLevel = LogRecordLevel.All };

        //        Logger.Instance.LogerOutputs.Add(rtbtw);

        //        var logRecord = new LogRecord(dt, new List<string>(s), LogRecordLevel.Fatal, c);
        //        Logger.Instance.AppendLog(logRecord);
        //        Thread.Sleep(1000);//Wait until async threads did their work

        //        var str = rtb.Text;
        //        var srcStr = $"{logRecord.LogStrings[0]}\r\n{logRecord.LogStrings[1]}\r\n";
        //        var res = String.CompareOrdinal(str, srcStr);
        //        Assert.AreEqual(0, res, str);

        //        Logger.Instance.LogerOutputs.Clear();

        //        rtb.Dispose();
        //    }
        //}
    }
}
