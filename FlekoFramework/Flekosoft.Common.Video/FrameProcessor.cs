using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Video
{
    public abstract class FrameProcessor : PropertyChangedErrorNotifyDisposableBase, IFrameProcessor
    {
        private readonly ConcurrentQueue<VideoFrame> _frameQueue = new ConcurrentQueue<VideoFrame>();
        readonly EventWaitHandle _newFrameWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly Thread _processThread;

        protected FrameProcessor()
        {
            _processThread = new Thread(ProcessFrameThreadFunc);
            _processThread.Start();
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Created" }, LogRecordLevel.Debug));
#endif
        }

        public void ProcessFrame(VideoFrame frame)
        {
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Frame enqueued: {frame}" }, LogRecordLevel.Debug));
#endif
            _frameQueue.Enqueue(frame);
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Queue length: {_frameQueue.Count}" }, LogRecordLevel.Debug));
#endif
            _newFrameWh.Set();
        }

        protected abstract void ProcessFrameInternal(VideoFrame frame);

        private void ProcessFrameThreadFunc()
        {
            while (true)
            {
                try
                {
                    if (_newFrameWh.WaitOne(Timeout.Infinite))
                    {
                        VideoFrame frame;
                        if (_frameQueue.TryDequeue(out frame))
                        {
#if DEBUG
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Processing: {frame}" }, LogRecordLevel.Debug));
#endif
                            ProcessFrameInternal(frame);
                        }
                        if (_frameQueue.IsEmpty) _newFrameWh.Reset();
                        else _newFrameWh.Set();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_processThread != null)
                {
                    if (_processThread.IsAlive)
                    {
                        _processThread.Abort();
                    }
                }
                _newFrameWh?.Dispose();
            }
            base.Dispose(disposing);
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Disposed" }, LogRecordLevel.Debug));
#endif
        }
    }
}